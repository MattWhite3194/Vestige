using Microsoft.Xna.Framework;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class PlatformData : DefaultTileData
    {
        public PlatformData(int tileID, TileProperty properties, Color color, int itemID = -1, int health = 0) : base(tileID, properties | TileProperty.Platform, color, itemID, health)
        {
        }
        public override byte GetUpdatedTileState(WorldGen world, int x, int y)
        {
            //This is not gonna be pretty
            ushort left = world.GetTileID(x - 1, y);
            ushort right = world.GetTileID(x + 1, y);

            bool leftPlatform = TileDatabase.TileHasProperties(left, TileProperty.Platform);
            bool rightPlatform = TileDatabase.TileHasProperties(right, TileProperty.Platform);
            bool leftSolid = TileDatabase.TileHasProperties(left, TileProperty.Solid) && !leftPlatform;
            bool rightSolid = TileDatabase.TileHasProperties(right, TileProperty.Solid) && !rightPlatform;

            if (!leftSolid && !rightSolid)
            {
                return (byte)((leftPlatform ? 8 : 0) + (rightPlatform ? 2 : 0));
            } 
            if (leftSolid && rightSolid)
            {
                return 42;
            }
            if (leftPlatform && rightSolid)
            {
                return 40;
            }
            if (rightPlatform && leftSolid)
            {
                return 32;
            }
            if (rightSolid)
            {
                return 34;
            }
            if (leftSolid)
            {
                return 14;
            }
            return 0;
        }
    }
}
