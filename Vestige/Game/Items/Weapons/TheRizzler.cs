using Microsoft.Xna.Framework;
using Vestige.Game.Tiles;

namespace Vestige.Game.Items.Weapons
{
    public class TheRizzler : IWeapon
    {
        public bool UseItem(bool altUse)
        {
            Point mouseTilePosition = Main.GetMouseWorldPosition() / new Point(Vestige.TILESIZE, Vestige.TILESIZE);
            Point playerTileSize = Vector2.Ceiling(Main.EntityManager.GetPlayer().Size / Vestige.TILESIZE).ToPoint();
            for (int i = 0; i < playerTileSize.X; i++)
            {
                for (int j = 0; j < playerTileSize.Y; j++)
                {
                    if (TileDatabase.TileHasProperties(Main.World.GetTileID(mouseTilePosition.X + i, mouseTilePosition.Y + j), TileProperty.Solid))
                        return false;
                }
            }
            Main.EntityManager.GetPlayer().Position = mouseTilePosition.ToVector2() * Vestige.TILESIZE;
            Main.EntityManager.GetPlayer().Velocity = new Vector2(Main.EntityManager.GetPlayer().Velocity.X, 0);
            return true;
        }
    }
}
