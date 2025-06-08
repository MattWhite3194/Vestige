using Microsoft.Xna.Framework;
using Vestige.Game.Input;
using Vestige.Game.Tiles;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Items.Weapons
{
    internal class Pickaxe : IWeapon
    {
        private int _minePower;
        public Pickaxe(int minePower)
        {
            this._minePower = minePower;
        }
        public bool UseItem(bool altUse)
        {
            Point mouseTilePosition = Main.GetMouseWorldPosition() / new Point(Vestige.TILESIZE, Vestige.TILESIZE);
            if (Vector2.Distance(mouseTilePosition.ToVector2() * Vestige.TILESIZE, Main.EntityManager.GetPlayer().Position) > Main.EntityManager.GetPlayer().MaxBreakDistance)
                return false;
            if (altUse)
            {
                if (Main.World.GetWallID(mouseTilePosition.X, mouseTilePosition.Y) != 0)
                {
                    Main.World.DamageWall(mouseTilePosition, _minePower);
                    return true;
                }
            }
            if (TileDatabase.TileHasProperties(Main.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y), TileProperty.PickaxeMineable))
            {
                Main.World.DamageTile(mouseTilePosition, _minePower);
                return true;
            }
            return false;
        }
    }
}
