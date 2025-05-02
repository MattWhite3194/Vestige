using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using TheGreen.Game.Entities;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Tiles.TileData
{
    public class ClosedDoorData : LargeTileData, ICollideableTile, IInteractableTile
    {
        private ushort _openDoorID;
        public ClosedDoorData(int tileID, TileProperty properties, Color color, ushort openDoorID, int itemID = -1) : base(tileID, properties | TileProperty.Solid, color, new Point(1, 3), itemID: itemID)
        {
            _openDoorID = openDoorID;
        }

        public override int VerifyTile(int x, int y)
        {
            Point topLeft = GetTopLeft(x, y);
            if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(topLeft.X, topLeft.Y - 1), TileProperty.Solid))
                return -1;
            return base.VerifyTile(x, y);
        }

        public void OnCollision(int x, int y, Entity entity)
        {
            if (entity.Layer != CollisionLayer.Player) return;
            Vector2 topLeft = GetTopLeft(x, y).ToVector2() * TheGreen.TILESIZE;

            int forceDirection = Math.Sign(topLeft.X - entity.Position.X);
            OpenDoor(x, y, forceDirection, forceDirection);
        }

        public void OnRightClick(int x, int y)
        {
            Point topLeft = GetTopLeft(x, y);
            Point playerPosition = (Main.EntityManager.GetPlayer().Position / TheGreen.TILESIZE).ToPoint();

            //  -1 - left     1 - right
            int playerDirection = Math.Sign(topLeft.X - playerPosition.X);
            OpenDoor(x, y, playerDirection);
        }
        private void OpenDoor(int x, int y, int openDirection, int forceDirection = 0)
        {
            Point topLeft = GetTopLeft(x, y);


            int left = -1;
            int right = 1;

            //check if the door can open
            for (int i = 0; i < this.TileSize.Y; i++)
            {
                if (WorldGen.World.GetTileID(topLeft.X + left, topLeft.Y + i) != 0)
                {
                    left = 0;
                }
                if (WorldGen.World.GetTileID(topLeft.X + right, topLeft.Y + i) != 0)
                {
                    right = 0;
                }
            }
            if (left == 0 && right == 0)
                return;

            int direction = left != 0 ? -1 : 0;
            if (openDirection == 1 && right != 0)
                direction = 1;

            if (direction != forceDirection && forceDirection != 0)
                return;

            if (direction == 1)
                direction = 0;

            for (int i = 0; i < this.TileSize.Y; i++)
            {
                WorldGen.World.SetTile(topLeft.X, topLeft.Y + i, 0);
            }

            WorldGen.World.SetTile(topLeft.X + direction, topLeft.Y + TileSize.Y - 1, _openDoorID);
            if (direction != -1)
                return;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    WorldGen.World.SetTileState(topLeft.X - 1 + i, topLeft.Y + j, (byte)(j * 10 + i + 2));
                }
            }
        }
    }
}
