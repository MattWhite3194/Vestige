using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.Tiles;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Items
{
    public class TileItem : Item
    {
        public readonly ushort TileID;
        public TileItem(int id, string name, string description, Texture2D image, ushort tileID, int maxStack = 999) : base(id, name, description, image, true, true, 0.15, true, maxStack, UseStyle.Swing)
        { 
            this.TileID = tileID;
        }
        public override bool UseItem()
        {
            Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(TheGreen.TILESIZE, TheGreen.TILESIZE);
            //TODO: check if the mouse is colliding with an entity or colliding with a tile that the entity is colliding with (will happen in entity manager)
            if (Vector2.Distance(mouseTilePosition.ToVector2() * TheGreen.TILESIZE, Main.EntityManager.GetPlayer().Position) > Main.EntityManager.GetPlayer().MaxPlaceDistance)
                return false;
            if (Main.EntityManager.MouseCollidingWithEntityTile && TileDatabase.TileHasProperty(TileID, TileProperty.Solid))
                return false;
            if (WorldGen.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y) == 0)
            {
                if (WorldGen.World.PlaceTile(mouseTilePosition.X, mouseTilePosition.Y, TileID))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
