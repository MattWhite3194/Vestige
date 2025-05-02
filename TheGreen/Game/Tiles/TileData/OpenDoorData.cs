using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using TheGreen.Game.Entities;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles.TileData
{
    public class OpenDoorData : LargeTileData, ICollideableTile, IInteractableTile
    {
        private ushort _closedDoorID;
        public OpenDoorData(int tileID, TileProperty properties, Color color, ushort closedDoorID, int itemID = -1) : base(tileID, properties, color, new Point(2, 3), itemID: itemID)
        {
            _closedDoorID = closedDoorID;
        }

        public override int VerifyTile(int x, int y)
        {
            Point topLeft = GetTopLeft(x, y);
            if (WorldGen.World.GetTileID(topLeft.X, topLeft.Y) != TileID)
                return 1;
            int closeDirection = WorldGen.World.GetTileState(topLeft.X, topLeft.Y) % 10 >= TileSize.X ? 1 : 0;
            if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(topLeft.X + closeDirection, topLeft.Y - 1), TileProperty.Solid))
                return -1;
            else if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(topLeft.X + closeDirection, topLeft.Y + TileSize.Y), TileProperty.Solid))
                return -1;
            return 1;
        }

        public void OnCollision(int x, int y, Entity entity)
        {
            //Add a dictionary of door positions with an open indicator, on collision, set its open indicator to 1, a world updater will check these doors and subtract one from them each pass
            //if the indicator reaches 0 and the updater checks it at 0, the door will close
        }

        public void OnRightClick(int x, int y)
        {
            Point topLeft = GetTopLeft(x, y);
            int closeDirection = WorldGen.World.GetTileState(topLeft.X, topLeft.Y) % 10 >= TileSize.X ? 1 : 0;
            //check if the player is colliding with any of the tiles
            for (int i = 0; i < TileSize.Y; i++)
            {
                Rectangle tileCollider = new Rectangle((topLeft.X + closeDirection) * TheGreen.TILESIZE, (topLeft.Y + i) * TheGreen.TILESIZE, TheGreen.TILESIZE, TheGreen.TILESIZE);
                //possibly change this to all entities
                if (Main.EntityManager.GetPlayer().GetBounds().Intersects(tileCollider))
                    return;
            }
            for (int i = 0; i < TileSize.X; i++)
            {
                for (int j = 0; j < TileSize.Y; j++)
                {
                    WorldGen.World.SetTile(topLeft.X + i, topLeft.Y + j, 0);
                }
            }
            WorldGen.World.SetTile(topLeft.X + closeDirection, topLeft.Y + TileSize.Y - 1, _closedDoorID);
        }
    }
}
