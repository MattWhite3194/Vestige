using Microsoft.Xna.Framework;
using Vestige.Game.Entities;
using Vestige.Game.Tiles;

namespace Vestige.Game.Items.Weapons
{
    internal class Axe : IWeapon
    {
        private int _axePower;
        public Axe(int axePower)
        {
            _axePower = axePower;
        }
        public bool UseItem(Player player, bool altUse)
        {
            Point mouseTilePosition = Main.GetMouseWorldPosition() / new Point(Vestige.TILESIZE, Vestige.TILESIZE);
            if (Vector2.Distance(mouseTilePosition.ToVector2() * Vestige.TILESIZE, player.Position) > player.MaxBreakDistance)
                return false;
            if (TileDatabase.TileHasProperties(Main.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y), TileProperty.AxeMineable))
            {
                Main.World.DamageTile(mouseTilePosition, _axePower);
                return true;
            }
            return false;
        }
    }
}
