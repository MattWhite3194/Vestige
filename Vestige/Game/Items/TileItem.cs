using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Vestige.Game.Entities;
using Vestige.Game.Tiles;

namespace Vestige.Game.Items
{
    public class TileItem : Item
    {
        public readonly ushort TileID;
        public TileItem(int id, string name, string description, Texture2D image, ushort tileID, int maxStack = 999) : base(id, name, description, image, default, true, true, 0.15f, true, maxStack, UseStyle.Swing)
        { 
            this.TileID = tileID;
        }
        public override bool UseItem(Player player, bool altUse)
        {
            Point mouseTilePosition = Main.GetMouseWorldPosition() / new Point(Vestige.TILESIZE, Vestige.TILESIZE);
            //TODO: check if the mouse is colliding with an entity or colliding with a tile that the entity is colliding with (will happen in entity manager)
            if (Vector2.Distance(mouseTilePosition.ToVector2() * Vestige.TILESIZE, Main.EntityManager.GetPlayer().Position) > Main.EntityManager.GetPlayer().MaxPlaceDistance)
                return false;
            if (altUse)
            {
                int wallID = TileDatabase.GetTileData(TileID).WallID;
                if (wallID == -1)
                    return false;
                if (Main.World.GetWallID(mouseTilePosition.X, mouseTilePosition.Y) == 0 && Main.World.PlaceWall(mouseTilePosition.X, mouseTilePosition.Y, (byte)wallID))
                {
                    return true;
                }
                return false;
            }
            if (Main.EntityManager.TileOccupied(mouseTilePosition.X, mouseTilePosition.Y) && TileDatabase.TileHasProperties(TileID, TileProperty.Solid))
                return false;
            if (Main.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y) == 0 && Main.World.PlaceTile(mouseTilePosition.X, mouseTilePosition.Y, TileID))
            {
                return true;
            }
            return false;
        }
        protected override Item CloneItem()
        {
            return new TileItem(ID, Name, Description, Image, TileID, MaxStack);
        }
    }
}
