using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Tiles;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Renderer
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
                    if (i >= 0 && i < WorldGen.World.WorldSize.X && j >= 0 && j < WorldGen.World.WorldSize.Y)
                    {
                        //if (WorldGen.World.GetTileLight(i, j) == 0)
                        //    continue;
                        //TEMPORARY
                        if (WorldGen.World.GetWallID(i, j) != 0)
                            TileDatabase.DrawWall(spriteBatch, WorldGen.World.GetWallID(i, j), WorldGen.World.GetWallState(i, j), i, j);
                    }
                }
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
                    TileDatabase.DrawTile(spriteBatch, tileID, WorldGen.World.GetTileState(i, j), i, j);
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
                    //if (WorldGen.World.GetTileLight(i, j) == 0)
                    //    continue;
                    TileDatabase.DrawTile(spriteBatch, tileID, WorldGen.World.GetTileState(i, j), i, j);
                }
            }
            foreach (Point crackPoint in WorldGen.World.GetMinedTiles().Keys)
            {
                spriteBatch.Draw(ContentLoader.Cracks, crackPoint.ToVector2() * Globals.TILESIZE, Color.White);
            }
        }

        public void DrawLiquids(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    if (WorldGen.World.GetLiquid(i, j) != 0)
                        spriteBatch.Draw(ContentLoader.LiquidTexture, new Vector2(i * Globals.TILESIZE, j * Globals.TILESIZE),  Color.White);
                }
            }
        }

        /// <summary>
        /// Debugging purposes only, use for showing tile values displayed over tiles
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawDebug(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _drawBoxMax.X - _drawBoxMin.X; i++)
            {
                for (int j = 0; j < _drawBoxMax.Y - _drawBoxMin.Y; j++)
                {
                    if (WorldGen.World.GetLiquid(i + _drawBoxMin.X, j + _drawBoxMin.Y) != 0)
                        spriteBatch.DrawString(ContentLoader.GameFont, WorldGen.World.GetLiquid(i + _drawBoxMin.X, j + _drawBoxMin.Y) + "", new Vector2(i, j) * Globals.TILESIZE, Color.White, 0.0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0.0f);
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