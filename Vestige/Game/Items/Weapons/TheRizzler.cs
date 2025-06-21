using Microsoft.Xna.Framework;
using Vestige.Game.Entities;
using Vestige.Game.Tiles;

namespace Vestige.Game.Items.Weapons
{
    public class TheRizzler : IWeapon
    {
        public bool UseItem(Player player, bool altUse)
        {
            Point mouseTilePosition = Main.GetMouseWorldPosition() / new Point(Vestige.TILESIZE, Vestige.TILESIZE);
            Point playerTileSize = Vector2.Ceiling(player.Size / Vestige.TILESIZE).ToPoint();
            for (int i = 0; i < playerTileSize.X; i++)
            {
                for (int j = 0; j < playerTileSize.Y; j++)
                {
                    if (TileDatabase.TileHasProperties(Main.World.GetTileID(mouseTilePosition.X + i, mouseTilePosition.Y + j), TileProperty.Solid))
                        return false;
                }
            }
            player.Position = mouseTilePosition.ToVector2() * Vestige.TILESIZE;
            player.Velocity = new Vector2(player.Velocity.X, 0);
            return true;
        }
    }
}
