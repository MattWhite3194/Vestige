using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles
{
    internal class TileData
    {
        public TileProperty Properties;
        public Color MapColor;
        public int ItemID;
        public int Health;
        public ushort BaseTileID;

        public TileData(TileProperty properties, Color color, int itemID = -1, int health = 0, ushort baseTileID = 0)
        {
            Properties = properties;
            MapColor = color;
            ItemID = itemID;
            Health = health;
            BaseTileID = baseTileID;
        }
        /// <summary>
        /// Runs whenever the tile state is updated, used to determine if the tile is valid in the world relative to its surrounding tile
        /// </summary>
        /// <param name="tileID"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>An integer representing the tiles verifcation state. -1: tile should be removed, 0: tile is not verified for placing, 1: tile is verified</returns>
        public virtual int VerifyTile(ushort tileID, int x, int y)
        {
            ushort top = WorldGen.Instance.GetTileID(x, y - 1);
            ushort right = WorldGen.Instance.GetTileID(x + 1, y);
            ushort bottom = WorldGen.Instance.GetTileID(x, y + 1);
            ushort left = WorldGen.Instance.GetTileID(x - 1, y);
            ushort wall = WorldGen.Instance.GetWallID(x, y);


            return Math.Sign(top + right + bottom + left + wall);
        }
        public virtual byte GetUpdatedTileState(ushort tileID, int x, int y)
        {
            if (tileID == 0) return 0;
            if (TileDatabase.TileHasProperty(tileID, TileProperty.StaticTileState)) return WorldGen.Instance.GetTileState(x, y);
            ushort top = WorldGen.Instance.GetTileID(x, y - 1);
            ushort right = WorldGen.Instance.GetTileID(x + 1, y);
            ushort bottom = WorldGen.Instance.GetTileID(x, y + 1);
            ushort left = WorldGen.Instance.GetTileID(x - 1, y);
            ushort tl = WorldGen.Instance.GetTileID(x - 1, y - 1);
            ushort tr = WorldGen.Instance.GetTileID(x + 1, y - 1);
            ushort bl = WorldGen.Instance.GetTileID(x - 1, y + 1);
            ushort br = WorldGen.Instance.GetTileID(x + 1, y + 1);
            if (!TileDatabase.TileHasProperty(top, TileProperty.Solid))
            {
                tl = 0;
                tr = 0;
                top = 0;
            }
            if (!TileDatabase.TileHasProperty(right, TileProperty.Solid))
            {
                tr = 0;
                br = 0;
                right = 0;
            }
            if (!TileDatabase.TileHasProperty(left, TileProperty.Solid))
            {
                tl = 0;
                bl = 0;
                left = 0;
            }
            if (!TileDatabase.TileHasProperty(bottom, TileProperty.Solid))
            {
                bl = 0;
                br = 0;
                bottom = 0;
            }
            //check corners
            if (!TileDatabase.TileHasProperty(tr, TileProperty.Solid))
                tr = 0;
            if (!TileDatabase.TileHasProperty(tl, TileProperty.Solid))
                tl = 0;
            if (!TileDatabase.TileHasProperty(br, TileProperty.Solid))
                br = 0;
            if (!TileDatabase.TileHasProperty(bl, TileProperty.Solid))
                bl = 0;

            return (byte)(Math.Sign(tl) + Math.Sign(top) * 2 + Math.Sign(tr) * 4 + Math.Sign(right) * 8 + Math.Sign(br) * 16 + Math.Sign(bottom) * 32 + Math.Sign(bl) * 64 + Math.Sign(left) * 128);
        }
        public virtual void Draw(SpriteBatch spriteBatch, int tileID, byte tileState, int x, int y)
        {
            spriteBatch.Draw(ContentLoader.TileTextures[tileID], new Vector2(x * Globals.TILESIZE, y * Globals.TILESIZE), TileDatabase.GetTileTextureAtlas(tileState), Color.White);
        }
    }
}
