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

        public virtual byte GetUpdatedWallState(int x, int y)
        {
            if (WorldGen.World.GetWallID(x, y) == 0)
            {
                return 0;
            }
            //Important: if a corner doesn't have both sides touching it, it won't be counted
            ushort top = WorldGen.World.GetWallID(x, y - 1);
            ushort right = WorldGen.World.GetWallID(x + 1, y);
            ushort bottom = WorldGen.World.GetWallID(x, y + 1);
            ushort left = WorldGen.World.GetWallID(x - 1, y);

            return (byte)((Math.Sign(top) * 2) + (Math.Sign(right) * 8) + (Math.Sign(bottom) * 32) + (Math.Sign(left) * 128));
        }

        public virtual void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            spriteBatch.Draw(ContentLoader.WallTextures[WallID], new Vector2(x * Vestige.TILESIZE - 2, y * Vestige.TILESIZE - 2), TileDatabase.GetWallTextureAtlas(WorldGen.World.GetWallState(x, y)), Main.LightEngine.GetLight(x, y));
        }
    }
}
