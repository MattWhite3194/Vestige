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
                    _lightColorMap[y * Globals.DrawDistance.X + x] = new Color((byte)0, (byte)0, (byte)0, (byte)(255 - (int)(WorldGen.World.GetTileLight(x + drawBoxMin.X, y + drawBoxMin.Y) / 255.0f * Globals.GlobalLight)));
                    if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x + drawBoxMin.X, y + drawBoxMin.Y), TileProperty.LightEmitting))
                        _dynamicLights.Enqueue((x, y, 0, TileDatabase.GetTileMapColor(WorldGen.World.GetTileID(x + drawBoxMin.X, y + drawBoxMin.Y)))); //TODO: change this depending on the tiles light value
                }
            }

            //Dynamic Lights
            while (_dynamicLights.Count > 0)
            {
                (int x, int y, int light, Color color) = _dynamicLights.Dequeue();
                
                if (_lightColorMap[y * Globals.DrawDistance.X + x].A <= light)
                    continue;
                if (light >= 255)
                    continue;
                int absorption = WorldGen.World.WallLightAbsorption;
                if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x + drawBoxMin.X, y + drawBoxMin.Y), TileProperty.Solid))
                    absorption = WorldGen.World.TileLightAbsorption;
                Color currentColor = _lightColorMap[y * Globals.DrawDistance.X + x];
                _lightColorMap[y * Globals.DrawDistance.X + x].R = (byte)((255 - light) / 255.0f * (color.R / 8));
                _lightColorMap[y * Globals.DrawDistance.X + x].G = (byte)((255 - light) / 255.0f * (color.G / 8));
                _lightColorMap[y * Globals.DrawDistance.X + x].B = (byte)((255 - light) / 255.0f * (color.B / 8));
                _lightColorMap[y * Globals.DrawDistance.X + x].A = (byte)light;
                for (int  i = 0; i < 4; i++)
                {
                    int nextX = x + _surroundingCoordsX[i];
                    int nextY = y + _surroundingCoordsY[i];
                    if (nextX < 0 || nextX >= Globals.DrawDistance.X || nextY < 0 || nextY >= Globals.DrawDistance.Y)
                        continue;
                    _dynamicLights.Enqueue((nextX, nextY, light + absorption, color));
                }
            }
        }
    }
}
