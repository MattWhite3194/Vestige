using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vestige.Game.Tiles;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Renderers
{
    public class TileRenderer
    {
        private Point _drawBoxMin;
        private Point _drawBoxMax;
        public void DrawWalls(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort wallID = Main.World.GetWallID(i, j);
                    if (wallID == 0)
                        continue;
                    TileDatabase.GetWallData(wallID).Draw(spriteBatch, i, j, Main.World.GetWallState(i, j), Main.LightEngine.GetLight(i, j));
                }
            }
            foreach (DamagedTile damagedWall in Main.World.GetDamagedWalls().Values)
            {
                Color color = Color.Black;
                color.A = 125;
                int cracksTextureAtlasY = (5 - (int)(damagedWall.Health / (float)damagedWall.TotalTileHealth * 5)) * Vestige.TILESIZE;
                spriteBatch.Draw(ContentLoader.Cracks, new Vector2(damagedWall.X, damagedWall.Y) * Vestige.TILESIZE, new Rectangle(0, cracksTextureAtlasY, Vestige.TILESIZE, Vestige.TILESIZE), color);
            }
        }

        public void DrawBackgroundTiles(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort tileID = Main.World.GetTileID(i, j);
                    //Draw all tiles that are not solid in the wall layer
                    if (TileDatabase.TileHasProperties(tileID, TileProperty.Solid) || tileID == 0)
                        continue;
                    TileDatabase.GetTileData(tileID).Draw(spriteBatch, i, j, Main.World.GetTileState(i, j), Main.LightEngine.GetLight(i, j));
                }
            }
        }
        private int _tileLiquidTextureAmount = 4;
        public void DrawTiles(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort tileID = Main.World.GetTileID(i, j);
                    if (!TileDatabase.TileHasProperties(tileID, TileProperty.Solid))
                        continue;
                    //Draw water behind tile
                    bool top = Main.World.GetLiquid(i, j - 1) != 0;
                    bool bottom = Main.World.GetLiquid(i, j + 1) == WorldGen.MaxLiquid;
                    int left = (int)(Main.World.GetLiquid(i - 1, j) / (float)WorldGen.MaxLiquid * (Vestige.TILESIZE - 2));
                    int right = (int)(Main.World.GetLiquid(i + 1, j) / (float)WorldGen.MaxLiquid * (Vestige.TILESIZE - 2));

                    if (top)
                    {
                        spriteBatch.Draw(ContentLoader.LiquidTexture, new Rectangle(i * Vestige.TILESIZE, j * Vestige.TILESIZE, Vestige.TILESIZE, _tileLiquidTextureAmount), new Rectangle(0, 2, Vestige.TILESIZE, _tileLiquidTextureAmount + 2), Main.LightEngine.GetLight(i, j));
                    }
                    if (bottom)
                    {
                        spriteBatch.Draw(ContentLoader.LiquidTexture, new Rectangle(i * Vestige.TILESIZE, j * Vestige.TILESIZE + _tileLiquidTextureAmount, Vestige.TILESIZE, _tileLiquidTextureAmount), new Rectangle(0, 2, Vestige.TILESIZE, _tileLiquidTextureAmount + 2), Main.LightEngine.GetLight(i, j));
                    }
                    if (left != 0)
                    {
                        left = left + 2;
                        spriteBatch.Draw(ContentLoader.LiquidTexture, new Rectangle(i * Vestige.TILESIZE, j * Vestige.TILESIZE + Vestige.TILESIZE - left, _tileLiquidTextureAmount, left), new Rectangle(0, 0, _tileLiquidTextureAmount, left), Main.LightEngine.GetLight(i, j));
                    }
                    if (right != 0)
                    {
                        right = right + 2;
                        spriteBatch.Draw(ContentLoader.LiquidTexture, new Rectangle(i * Vestige.TILESIZE + Vestige.TILESIZE - _tileLiquidTextureAmount, j * Vestige.TILESIZE + Vestige.TILESIZE - right, _tileLiquidTextureAmount, right), new Rectangle(0, 0, _tileLiquidTextureAmount, right), Main.LightEngine.GetLight(i, j));
                    }
                    //Draw Tile
                    TileDatabase.GetTileData(tileID).Draw(spriteBatch, i, j, Main.World.GetTileState(i, j), Main.LightEngine.GetLight(i, j));
                }
            }
            foreach (DamagedTile damagedTile in Main.World.GetDamagedTiles().Values)
            {
                Color color = Color.Black;
                color.A = 125;
                int cracksTextureAtlasY = (5 - (int)(damagedTile.Health / (float)damagedTile.TotalTileHealth * 5)) * Vestige.TILESIZE;
                spriteBatch.Draw(ContentLoader.Cracks, new Vector2(damagedTile.X, damagedTile.Y) * Vestige.TILESIZE, new Rectangle(0, cracksTextureAtlasY, Vestige.TILESIZE, Vestige.TILESIZE), color);
            }
        }

        public void DrawLiquids(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    if (Main.World.GetLiquid(i, j) != 0)
                    {
                        if (Main.World.GetLiquid(i, j) == WorldGen.MaxLiquid && (Main.World.GetLiquid(i, j - 1) != 0 || TileDatabase.TileHasProperties(Main.World.GetTileID(i, j-1), TileProperty.Solid)))
                        {
                            spriteBatch.Draw(ContentLoader.LiquidTexture, new Vector2(i, j) * Vestige.TILESIZE, new Rectangle(Vestige.TILESIZE, 0, Vestige.TILESIZE, Vestige.TILESIZE), Main.LightEngine.GetLight(i, j));
                        }
                        else
                        {
                            int textureOffset = 2 + (int)(Main.World.GetLiquid(i, j) / (float)WorldGen.MaxLiquid * (Vestige.TILESIZE - 2));
                            spriteBatch.Draw(ContentLoader.LiquidTexture, new Vector2(i * Vestige.TILESIZE, j * Vestige.TILESIZE + Vestige.TILESIZE - textureOffset), new Rectangle(0, 0, Vestige.TILESIZE, textureOffset), Main.LightEngine.GetLight(i, j));
                        }
                    }
                }
            }
        }

        public void SetDrawBox(Point drawBoxMin, Point drawBoxMax)
        {
            this._drawBoxMin = drawBoxMin;
            this._drawBoxMax = drawBoxMax;
        }
    }
}