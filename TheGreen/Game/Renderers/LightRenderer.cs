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
        private Color[] _lightColorMap;
        private byte[] _dynamicLightMap;
        private Texture2D LightTexture;
        private Rectangle _destinationRectangle;
        private Queue<(int, int, int, Color)> _dynamicLights = new Queue<(int, int, int, Color)>();
        private int[] _surroundingCoordsX = [1, -1, 0, 0];
        private int[] _surroundingCoordsY = [0, 0, 1, -1];
        private int _lightRange;

        public LightRenderer(GraphicsDevice graphicsDevice)
        {
            LightTexture = new Texture2D(graphicsDevice, Globals.DrawDistance.X, Globals.DrawDistance.Y);
            _lightRange = 256 / WorldGen.World.WallLightAbsorption;
            _lightColorMap = new Color[Globals.DrawDistance.X * Globals.DrawDistance.Y];
            _destinationRectangle = new Rectangle(
                0,
                0,
                Globals.DrawDistance.X * Globals.TILESIZE,
                Globals.DrawDistance.Y * Globals.TILESIZE
                );
            _dynamicLightMap = new byte[(Globals.DrawDistance.X + 2 * _lightRange) * (Globals.DrawDistance.Y + 2 * _lightRange)];
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
            Point dynamicLightDrawBoxMin = new Point(Math.Max(0, drawBoxMin.X - _lightRange), Math.Max(0, drawBoxMin.Y - _lightRange));
            Point dynamicLightDrawBoxMax = new Point(Math.Min(WorldGen.World.WorldSize.X, drawBoxMax.X + _lightRange), Math.Min(WorldGen.World.WorldSize.Y, drawBoxMax.Y + _lightRange));
            //Initial tile lighting
            for (int x = dynamicLightDrawBoxMin.X; x < dynamicLightDrawBoxMax.X; x++)
            {
                for (int y = dynamicLightDrawBoxMin.Y; y < dynamicLightDrawBoxMax.Y; y++)
                {
                    _dynamicLightMap[(y - dynamicLightDrawBoxMin.Y) * (Globals.DrawDistance.X + 2 * _lightRange) + (x - dynamicLightDrawBoxMin.X)] = (byte)(255 - (int)(WorldGen.World.GetTileLight(x, y) / 255.0f * Globals.GlobalLight));
                    if ((drawBoxMin.X <= x && x < drawBoxMax.X) && (drawBoxMin.Y <= y && y < drawBoxMax.Y))
                    {
                        _lightColorMap[(y - drawBoxMin.Y) * Globals.DrawDistance.X + (x - drawBoxMin.X)] = new Color((byte)0, (byte)0, (byte)0, (byte)(255 - (int)(WorldGen.World.GetTileLight(x, y) / 255.0f * Globals.GlobalLight)));
                    }
                    if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x, y), TileProperty.LightEmitting))
                        _dynamicLights.Enqueue((x, y, 0, TileDatabase.GetTileMapColor(WorldGen.World.GetTileID(x, y)))); //TODO: change this depending on the tiles light value
                }
            }

            //Dynamic Lights
            while (_dynamicLights.Count > 0)
            {
                (int x, int y, int light, Color color) = _dynamicLights.Dequeue();
                int dynamicMapIndex = (y - dynamicLightDrawBoxMin.Y) * (Globals.DrawDistance.X + 2 * _lightRange) + (x - dynamicLightDrawBoxMin.X);
                if (light >= 255)
                    continue;
                if (_dynamicLightMap[dynamicMapIndex] <= light)
                    continue;
                int absorption = WorldGen.World.WallLightAbsorption;
                if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x, y), TileProperty.Solid))
                    absorption = WorldGen.World.TileLightAbsorption;
                if ((drawBoxMin.X <= x && x < drawBoxMax.X) && (drawBoxMin.Y <= y && y < drawBoxMax.Y))
                {
                    int colorMapIndex = (y - drawBoxMin.Y) * Globals.DrawDistance.X + (x - drawBoxMin.X);
                    _lightColorMap[colorMapIndex].R = (byte)((255 - light) / 255.0f * (color.R / 8));
                    _lightColorMap[colorMapIndex].G = (byte)((255 - light) / 255.0f * (color.G / 8));
                    _lightColorMap[colorMapIndex].B = (byte)((255 - light) / 255.0f * (color.B / 8));
                    _lightColorMap[colorMapIndex].A = (byte)light;
                }
                _dynamicLightMap[dynamicMapIndex] = (byte)light;
                for (int  i = 0; i < 4; i++)
                {
                    int nextX = x + _surroundingCoordsX[i];
                    int nextY = y + _surroundingCoordsY[i];
                    if (nextX < dynamicLightDrawBoxMin.X || nextX >= dynamicLightDrawBoxMax.X || nextY < dynamicLightDrawBoxMin.Y || nextY >= dynamicLightDrawBoxMax.Y)
                        continue;
                    _dynamicLights.Enqueue((nextX, nextY, light + absorption, color));
                }
            }
        }
    }
}
