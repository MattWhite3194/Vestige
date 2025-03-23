using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheGreen.Game.Tiles;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Renderers
{
    public class LightRenderer
    {
        private Color[] _lightColorMap = new Color[Globals.DrawDistance.X * Globals.DrawDistance.Y];
        private Texture2D LightTexture;
        private Rectangle _destinationRectangle;
        private Queue<(int, int, int, Color)> _dynamicLights = new Queue<(int, int, int, Color)>();
        private int[] _surroundingCoordsX = [1, -1, 0, 0];
        private int[] _surroundingCoordsY = [0, 0, 1, -1];

        public LightRenderer(GraphicsDevice graphicsDevice)
        {
            LightTexture = new Texture2D(graphicsDevice, Globals.DrawDistance.X, Globals.DrawDistance.Y);
            _destinationRectangle = new Rectangle(
                0,
                0,
                Globals.DrawDistance.X * Globals.TILESIZE,
                Globals.DrawDistance.Y * Globals.TILESIZE
                );
        }
        public void Draw(SpriteBatch spriteBatch, Point drawBoxMin, Point drawBoxMax)
        {
            CalculateLighting(drawBoxMin, drawBoxMax);
            LightTexture.SetData<Color>(_lightColorMap);
            _destinationRectangle.Location = drawBoxMin * new Point(Globals.TILESIZE, Globals.TILESIZE);
            spriteBatch.Draw(LightTexture, _destinationRectangle, Color.White);
        }
        private void CalculateLighting(Point drawBoxMin, Point drawBoxMax)
        {
            //Initial tile lighting
            for (int x = 0; x < Globals.DrawDistance.X; x++)
            {
                for (int y = 0; y < Globals.DrawDistance.Y; y++)
                {
                    _lightColorMap[y * Globals.DrawDistance.X + x] = new Color((byte)0, (byte)0, (byte)0, (byte)(255 - (int)(WorldGen.Instance.GetTileLight(x + drawBoxMin.X, y + drawBoxMin.Y) / 255.0f * Globals.GlobalLight)));
                    if (TileDatabase.TileHasProperty(WorldGen.Instance.GetTileID(x + drawBoxMin.X, y + drawBoxMin.Y), TileProperty.LightEmitting))
                        _dynamicLights.Enqueue((x, y, 0, TileDatabase.GetTileMapColor(WorldGen.Instance.GetTileID(x + drawBoxMin.X, y + drawBoxMin.Y)))); //TODO: change this depending on the tiles light value
                }
            }

            //Dynamic Lights
            while (_dynamicLights.Count > 0)
            {
                (int, int, int, Color) dynamicLight = _dynamicLights.Dequeue();
                
                if (_lightColorMap[dynamicLight.Item2 * Globals.DrawDistance.X + dynamicLight.Item1].A <= dynamicLight.Item3)
                    continue;
                if (dynamicLight.Item3 >= 255)
                    continue;
                int absorption = WorldGen.Instance.WallLightAbsorption;
                if (TileDatabase.TileHasProperty(WorldGen.Instance.GetTileID(dynamicLight.Item1 + drawBoxMin.X, dynamicLight.Item2 + drawBoxMin.Y), TileProperty.Solid))
                    absorption = WorldGen.Instance.TileLightAbsorption;
                Color currentColor = _lightColorMap[dynamicLight.Item2 * Globals.DrawDistance.X + dynamicLight.Item1];
                _lightColorMap[dynamicLight.Item2 * Globals.DrawDistance.X + dynamicLight.Item1].R = (byte)((255 - dynamicLight.Item3) / 255.0f * (dynamicLight.Item4.R / 8));
                _lightColorMap[dynamicLight.Item2 * Globals.DrawDistance.X + dynamicLight.Item1].G = (byte)((255 - dynamicLight.Item3) / 255.0f * (dynamicLight.Item4.G / 8));
                _lightColorMap[dynamicLight.Item2 * Globals.DrawDistance.X + dynamicLight.Item1].B = (byte)((255 - dynamicLight.Item3) / 255.0f * (dynamicLight.Item4.B / 8));
                _lightColorMap[dynamicLight.Item2 * Globals.DrawDistance.X + dynamicLight.Item1].A = (byte)dynamicLight.Item3;
                for (int  i = 0; i < 4; i++)
                {
                    int x = dynamicLight.Item1 + _surroundingCoordsX[i];
                    int y = dynamicLight.Item2 + _surroundingCoordsY[i];
                    if (x < 0 || x >= Globals.DrawDistance.X || y < 0 || y >= Globals.DrawDistance.Y)
                        continue;
                    _dynamicLights.Enqueue((x, y, dynamicLight.Item3 + absorption, dynamicLight.Item4));
                }
            }
        }
    }
}
