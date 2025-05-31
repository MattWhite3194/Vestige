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
                    ushort wallID = WorldGen.World.GetWallID(i, j);
                    if (wallID == 0)
                        continue;
                    TileDatabase.GetWallData(wallID).Draw(spriteBatch, i, j);
                }
            }
            foreach (DamagedTile damagedWall in WorldGen.World.GetDamagedWalls().Values)
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
                    ushort tileID = WorldGen.World.GetTileID(i, j);
                    //Draw all tiles that are not solid in the wall layer
                    if (TileDatabase.TileHasProperty(tileID, TileProperty.Solid) || tileID == 0)
                        continue;
                    TileDatabase.GetTileData(tileID).Draw(spriteBatch, WorldGen.World.GetTileState(i, j), i, j);
                }
            }
        }

        public void DrawTiles(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort tileID = WorldGen.World.GetTileID(i, j);
                    if (!TileDatabase.TileHasProperty(tileID, TileProperty.Solid))
                        continue;
                    TileDatabase.GetTileData(tileID).Draw(spriteBatch, WorldGen.World.GetTileState(i, j), i, j);
                }
            }
            foreach (DamagedTile damagedTile in WorldGen.World.GetDamagedTiles().Values)
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
                    if (WorldGen.World.GetLiquid(i, j) != 0)
                    {
                        int rectX = WorldGen.World.GetLiquid(i, j - 1) != 0 ? 15 * 18 : (int)(WorldGen.World.GetLiquid(i, j) / (float)WorldGen.MaxLiquid * 14) * 18;
                        spriteBatch.Draw(ContentLoader.LiquidTexture, new Vector2(i * Vestige.TILESIZE - 1, j * Vestige.TILESIZE), new Rectangle(rectX, 0, 18, 17), Main.LightEngine.GetLight(i, j));
                    }
                }
            }
        }

        /// <summary>
        /// Debugging purposes only, use for showing tile values displayed over tiles
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawDebug(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                { 
                    spriteBatch.DrawString(ContentLoader.GameFont, WorldGen.World.GetLiquid(i, j) + "", new Vector2(i, j) * Vestige.TILESIZE, Color.White, 0.0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0.0f);
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