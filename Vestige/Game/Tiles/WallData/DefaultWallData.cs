using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.WallData
{
    public class DefaultWallData
    {
        public readonly ushort WallID;
        public readonly int Health;
        public DefaultWallData(ushort wallID, int health = 100) 
        { 
            this.WallID = wallID;
            this.Health = health;
        }

        public virtual byte GetUpdatedWallState(WorldGen world, int x, int y)
        {
            if (world.GetWallID(x, y) == 0)
            {
                return 0;
            }
            //Important: if a corner doesn't have both sides touching it, it won't be counted
            ushort top = world.GetWallID(x, y - 1);
            ushort right = world.GetWallID(x + 1, y);
            ushort bottom = world.GetWallID(x, y + 1);
            ushort left = world.GetWallID(x - 1, y);

            return (byte)((Math.Sign(top) * 2) + (Math.Sign(right) * 8) + (Math.Sign(bottom) * 32) + (Math.Sign(left) * 128));
        }

        public virtual void Draw(SpriteBatch spriteBatch, int x, int y, byte state, Color light)
        {
            spriteBatch.Draw(ContentLoader.WallTextures[WallID], new Vector2(x * Vestige.TILESIZE - 2, y * Vestige.TILESIZE - 2), TileDatabase.GetWallTextureAtlas(state), light);
        }
    }
}
