using Microsoft.Xna.Framework;
using Vestige.Game.Input;
using Vestige.Game.Tiles;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Items.Weapons
{ 
    internal class Hammer : IWeapon
    {
        private int _hammerPower;
        public Hammer(int hammerPower)
        {
            this._hammerPower = hammerPower;
        }
        public bool UseItem()
        {
            Point mouseTilePosition = Main.GetMouseWorldPosition() / new Point(Vestige.TILESIZE, Vestige.TILESIZE);
            if (Vector2.Distance(mouseTilePosition.ToVector2() * Vestige.TILESIZE, Main.EntityManager.GetPlayer().Position) > Main.EntityManager.GetPlayer().MaxBreakDistance)
                return false;
            if (TileDatabase.TileHasProperties(Main.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y), TileProperty.HammerMineable))
            {
                Main.World.DamageTile(mouseTilePosition, _hammerPower);
                return true;
            }
            else if (Main.World.GetWallID(mouseTilePosition.X, mouseTilePosition.Y) != 0)
            {
                Main.World.DamageWall(mouseTilePosition, _hammerPower);
                return true;
            }
            return false;
        }
    }
}
