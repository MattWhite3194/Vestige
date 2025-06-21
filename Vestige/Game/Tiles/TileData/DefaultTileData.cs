using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class DefaultTileData
    {
        public readonly TileProperty Properties;
        public readonly Color MapColor;
        public readonly int ItemID;
        public readonly int Health;
        public readonly int TileID;
        public readonly string Name;
        /// <summary>
        /// The wall that this tile places when shift clicking.
        /// </summary>
        public readonly int WallID;
        private readonly ushort[] tileMerges;

        //TODO: add WorldGen world to constructor
        public DefaultTileData(int tileID, string name, TileProperty properties, Color color, int itemID = -1, int wallID = -1, int health = 0, ushort[] tileMerges = null)
        {
            TileID = tileID;
            Properties = properties;
            MapColor = color;
            ItemID = itemID;
            Health = health;
            WallID = wallID;
            Name = name;
            this.tileMerges = tileMerges ?? [];
        }
        /// <summary>
        /// Runs whenever the tile state is updated, used to determine if the tile is valid in the world relative to its surrounding tiles
        /// </summary>
        /// <param name="tileID"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>An integer representing the tiles verification state. -1: tile should be removed, 0: tile is not verified for placing, 1: tile is verified</returns>
        public virtual int VerifyTile(WorldGen world, int x, int y)
        {
            bool top = TileDatabase.TileHasProperties(world.GetTileID(x, y - 1), TileProperty.Solid | TileProperty.Platform);
            bool right = TileDatabase.TileHasProperties(world.GetTileID(x + 1, y), TileProperty.Solid | TileProperty.Platform);
            bool bottom = TileDatabase.TileHasProperties(world.GetTileID(x, y + 1), TileProperty.Solid | TileProperty.Platform);
            bool left = TileDatabase.TileHasProperties(world.GetTileID(x - 1, y), TileProperty.Solid | TileProperty.Platform);
            bool wall = world.GetWallID(x, y) != 0;


            return top || right || bottom || left || wall ? 1 : 0;
        }
        /// <summary>
        /// Used by any damage inflictors like pickaxes or bombs.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Whether or not the tile is able to be damaged based on its situation in the world</returns>
        public virtual bool CanTileBeDamaged(WorldGen world, int x, int y)
        {
            ushort top = world.GetTileID(x, y - 1);
            if (TileDatabase.TileHasProperties(top, TileProperty.LargeTile))
                return (TileDatabase.GetTileData(top) as LargeTileData).CanTileBeDamaged(world, x, y - 1);
            if (TileDatabase.TileHasProperties(top, TileProperty.AxeMineable))
                return false;
            return true;
        }
        public virtual byte GetUpdatedTileState(WorldGen world, int x, int y)
        {
            if (TileID == 0) return 0;
            ushort top = world.GetTileID(x, y - 1);
            ushort right = world.GetTileID(x + 1, y);
            ushort bottom = world.GetTileID(x, y + 1);
            ushort left = world.GetTileID(x - 1, y);
            ushort tl = world.GetTileID(x - 1, y - 1);
            ushort tr = world.GetTileID(x + 1, y - 1);
            ushort bl = world.GetTileID(x - 1, y + 1);
            ushort br = world.GetTileID(x + 1, y + 1);
            if (!CanMerge(top))
            {
                tl = 0;
                tr = 0;
                top = 0;
            }
            if (!CanMerge(right))
            {
                tr = 0;
                br = 0;
                right = 0;
            }
            if (!CanMerge(left))
            {
                tl = 0;
                bl = 0;
                left = 0;
            }
            if (!CanMerge(bottom))
            {
                bl = 0;
                br = 0;
                bottom = 0;
            }
            //check corners
            if (!CanMerge(tr))
                tr = 0;
            if (!CanMerge(tl))
                tl = 0;
            if (!CanMerge(br))
                br = 0;
            if (!CanMerge(bl))
                bl = 0;

            return (byte)(Math.Sign(tl) + Math.Sign(top) * 2 + Math.Sign(tr) * 4 + Math.Sign(right) * 8 + Math.Sign(br) * 16 + Math.Sign(bottom) * 32 + Math.Sign(bl) * 64 + Math.Sign(left) * 128);
        }
        /// <summary>
        /// Used by GetUpdatesTileState to determine if the tile merges with the specified tileID
        /// </summary>
        /// <param name="tileID"></param>
        /// <returns></returns>
        internal virtual bool CanMerge(ushort tileID)
        {
            //merge if they're equal
            if (tileID == TileID)
                return true;
            
            //Merge if the other tile contains this tile in its list of mergeable tiles, reduces redundancy
            if (TileDatabase.GetTileData(tileID).IsTileInMergeList((ushort)TileID))
                return true;
            //merge if the tile is in the list of mergable tiles
            return IsTileInMergeList(tileID);
        }
        /// <summary>
        /// Checks if the specified tileID is contained in the list of mergeable tiles
        /// </summary>
        /// <param name="tileID"></param>
        /// <returns></returns>
        internal virtual bool IsTileInMergeList(ushort tileID)
        {
            foreach (ushort tile in tileMerges)
            {
                if (tileID == tile)
                {
                    return true;
                }
            }
            return false;
        }
        public virtual void Draw(SpriteBatch spriteBatch, int x, int y, byte state, Color light)
        {
            spriteBatch.Draw(ContentLoader.TileTextures[TileID], new Vector2(x, y) * Vestige.TILESIZE, TileDatabase.GetTileTextureAtlas(state), light);
        }
        public virtual void Place(WorldGen world, int x, int y)
        {
            //TODO: implement these in tiledata classes to remove redundant if else blocks
        }
        public virtual void Break(WorldGen world, int x, int y)
        {
            //TODO: same thing here
        }
    }
}
