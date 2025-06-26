using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vestige.Game.Tiles;

namespace Vestige.Game.Lighting
{
    public class LightEngine
    {
        private Vector3[] _lightMap;
        private Vector3[] _maskMap;
        private Point _paddedDrawBoxMin;
        private Point _paddedDrawBoxMax;
        private int _lightRange;
        private Vector3 _wallAbsorption = new Vector3(0.9f, 0.9f, 0.9f);
        private Vector3 _tileAbsorption = new Vector3(0.7f, 0.7f, 0.7f);
        private Vector3 _liquidLightAbsorption = new Vector3(0.7f, 0.8f, 0.9f);
        private Queue<(int, int, Vector3)> _dynamicLights;

        public LightEngine(GraphicsDevice graphicsDevice)
        {
            _lightRange = 38;
            _lightMap = new Vector3[(Vestige.DrawDistance.X + (2 * _lightRange)) * (Vestige.DrawDistance.Y + (2 * _lightRange))];
            _maskMap = new Vector3[(Vestige.DrawDistance.X + (2 * _lightRange)) * (Vestige.DrawDistance.Y + (2 * _lightRange))];
            _dynamicLights = new Queue<(int, int, Vector3)>();
        }
        /// <summary>
        /// Apply absorption values and default colors to light map before performing blur
        /// </summary>
        private void ClearLightMap()
        {
            Parallel.For(_paddedDrawBoxMin.X, _paddedDrawBoxMax.X, x =>
            {
                Vector3 skyLight = new Vector3(Main.GameClock.GlobalLight / 255.0f);
                for (int y = _paddedDrawBoxMin.Y; y < _paddedDrawBoxMax.Y; y++)
                {
                    int mapIndex = ((y - _paddedDrawBoxMin.Y) * (Vestige.DrawDistance.X + (2 * _lightRange))) + (x - _paddedDrawBoxMin.X);
                    _lightMap[mapIndex] = Vector3.Zero;
                    _maskMap[mapIndex] = _wallAbsorption;
                    if (TileDatabase.TileHasProperties(Main.World.GetTileID(x, y), TileProperty.Solid))
                    {
                        _maskMap[mapIndex] = _tileAbsorption;
                    }
                    else if (Main.World.GetWallID(x, y) == 0)
                    {
                        _lightMap[mapIndex] = skyLight;
                    }
                    if (Main.World.GetLiquid(x, y) != 0)
                    {
                        _maskMap[mapIndex] = _liquidLightAbsorption;
                    }
                    if (TileDatabase.TileHasProperties(Main.World.GetTileID(x, y), TileProperty.LightEmitting))
                    {
                        _lightMap[mapIndex] = Vector3.Max(TileDatabase.GetTileData(Main.World.GetTileID(x, y)).MapColor.ToVector3(), _lightMap[mapIndex]);
                        _maskMap[mapIndex] = new Vector3(1f, 1f, 1f);
                    }
                }
            });
        }
        private void ApplyDynamicLights()
        {
            while (_dynamicLights.Count > 0)
            {
                (int x, int y, Vector3 light) = _dynamicLights.Dequeue();
                if (_paddedDrawBoxMin.X <= x && x < _paddedDrawBoxMax.X && _paddedDrawBoxMin.Y <= y && y < _paddedDrawBoxMax.Y)
                {
                    int mapIndex = ((y - _paddedDrawBoxMin.Y) * (Vestige.DrawDistance.X + (2 * _lightRange))) + (x - _paddedDrawBoxMin.X);
                    _lightMap[mapIndex] = Vector3.Max(light, _lightMap[mapIndex]);
                }
            }
        }
        public void CalculateLightMap()
        {
            //Perform two passes of light bluring, (possibly change this to spread left and down, then right and up for more readability
            ClearLightMap();
            ApplyDynamicLights();
            SpreadLight();
            SpreadLight();
        }
        private void SpreadLight()
        {
            int width = Vestige.DrawDistance.X + (2 * _lightRange);
            int height = Vestige.DrawDistance.Y + (2 * _lightRange);
            Parallel.For(0, width, x =>
            {
                SpreadLightInLine(x, x + ((height - 1) * width), width);
                SpreadLightInLine(x + ((height - 1) * width), x, -width);
            });

            Parallel.For(0, height, y =>
            {
                SpreadLightInLine(y * width, (y * width) + width - 1, 1);
                SpreadLightInLine((y * width) + width - 1, y * width, -1);
            });
        }
        private void SpreadLightInLine(int startIndex, int endIndex, int stride)
        {
            Vector3 light = Vector3.Zero;
            for (int i = startIndex; i != endIndex + stride; i += stride)
            {
                light = Vector3.Max(light, _lightMap[i]);
                if (light.X >= 0.0185f)
                {
                    _lightMap[i].X = light.X;
                    light.X *= _maskMap[i].X;
                }
                if (light.Y >= 0.0185f)
                {
                    _lightMap[i].Y = light.Y;
                    light.Y *= _maskMap[i].Y;
                }
                if (light.Z >= 0.0185f)
                {
                    _lightMap[i].Z = light.Z;
                    light.Z *= _maskMap[i].Z;
                }
            }
        }
        public void SetDrawBox(Point drawBoxMin, Point drawBoxMax)
        {
            _paddedDrawBoxMin = new Point(Math.Max(0, drawBoxMin.X - _lightRange), Math.Max(0, drawBoxMin.Y - _lightRange));
            _paddedDrawBoxMax = new Point(Math.Min(Main.World.WorldSize.X, drawBoxMax.X + _lightRange), Math.Min(Main.World.WorldSize.Y, drawBoxMax.Y + _lightRange));
        }
        public Color GetLight(int x, int y)
        {
            if (_paddedDrawBoxMin.X <= x && x < _paddedDrawBoxMax.X && _paddedDrawBoxMin.Y <= y && y < _paddedDrawBoxMax.Y)
            {
                int mapIndex = ((y - _paddedDrawBoxMin.Y) * (Vestige.DrawDistance.X + (2 * _lightRange))) + (x - _paddedDrawBoxMin.X);
                return new Color(_lightMap[mapIndex]);
            }
            return default;
        }
        //Lighting is stored centered on a tile, smooth lighting is calculated by corners. In order to get the proper light value for a corner, The max of the four surrounding center points is chosen for the specified corner
        /// <summary>
        /// Gets the corresponding light value at the specified corned. Value 0, 0 is the top left corner of the top left tile. Value (1, 1) is the bottom right corner of the top left tile, the top left corner of tile (1, 1) etc...
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetCornerLight(int x, int y)
        {
            Vector3 totalLight = Vector3.Max(Vector3.Max(GetLightAsVector(x, y), GetLightAsVector(x - 1, y)), Vector3.Max(GetLightAsVector(x, y - 1), GetLightAsVector(x - 1, y - 1)));
            return new Color(totalLight);
        }
        private Vector3 GetLightAsVector(int x, int y)
        {
            if (_paddedDrawBoxMin.X <= x && x < _paddedDrawBoxMax.X && _paddedDrawBoxMin.Y <= y && y < _paddedDrawBoxMax.Y)
            {
                int mapIndex = ((y - _paddedDrawBoxMin.Y) * (Vestige.DrawDistance.X + (2 * _lightRange))) + (x - _paddedDrawBoxMin.X);
                return _lightMap[mapIndex];
            }
            return default;
        }
        /// <summary>
        /// Use to add frame-by-frame dynamic lights to the map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void AddLight(int x, int y, Color color)
        {
            _dynamicLights.Enqueue((x, y, color.ToVector3()));
        }
    }
}
