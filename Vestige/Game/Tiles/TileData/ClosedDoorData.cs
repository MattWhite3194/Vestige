using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using Vestige.Game.Entities;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    public class ClosedDoorData : LargeTileData, ICollideableTile, IInteractableTile
    {
        private ushort _openDoorID;
        public ClosedDoorData(int tileID, TileProperty properties, Color color, ushort openDoorID, int itemID = -1) : base(tileID, properties | TileProperty.Solid, color, new Point(1, 3), itemID: itemID)
        {
            _openDoorID = openDoorID;
        }

        public override int VerifyTile(WorldGen world, int x, int y)
        {
            Point topLeft = GetTopLeft(world, x, y);
            if (!TileDatabase.TileHasProperty(world.GetTileID(topLeft.X, topLeft.Y - 1), TileProperty.Solid))
                return -1;
            return base.VerifyTile(world, x, y);
        }

        public void OnCollision(WorldGen world, int x, int y, Entity entity)
        {
            if (entity.Layer != CollisionLayer.Player) return;
            Vector2 topLeft = GetTopLeft(world, x, y).ToVector2() * Vestige.TILESIZE;

            int forceDirection = Math.Sign(topLeft.X - entity.Position.X);
            OpenDoor(world, x, y, forceDirection, true);
        }

        public void OnRightClick(WorldGen world, int x, int y)
        {
            Point topLeft = GetTopLeft(world, x, y);
            Point playerPosition = (Main.EntityManager.GetPlayer().Position / Vestige.TILESIZE).ToPoint();

            int playerDirection = Math.Sign(topLeft.X - playerPosition.X);
            OpenDoor(world, x, y, playerDirection);
        }
        private void OpenDoor(WorldGen world, int x, int y, int openDirection, bool openedByCollision = false)
        {
            Point topLeft = GetTopLeft(world, x, y);


            int left = -1;
            int right = 1;

            //check if the door can open
            for (int i = 0; i < this.TileSize.Y; i++)
            {
                if (world.GetTileID(topLeft.X + left, topLeft.Y + i) != 0)
                {
                    left = 0;
                }
                if (world.GetTileID(topLeft.X + right, topLeft.Y + i) != 0)
                {
                    right = 0;
                }
            }
            if (left == 0 && right == 0)
                return;

            int direction = left != 0 ? -1 : 0;
            if (openDirection == 1 && right != 0)
                direction = 1;

            if (direction == 1)
                direction = 0;

            for (int i = 0; i < this.TileSize.Y; i++)
            {
                world.PlaceTile(topLeft.X, topLeft.Y + i, 0);
            }

            world.PlaceTile(topLeft.X + direction, topLeft.Y + TileSize.Y - 1, _openDoorID);
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    world.SetTileState(topLeft.X - (direction == -1 ? 1 : 0) + i, topLeft.Y + j, (byte)(j * 10 + i + (direction == -1 ? 2 : 0) + (openedByCollision ? 100 : 0)));
                }
            }
        }
    }
}
