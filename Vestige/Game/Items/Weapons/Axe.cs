using Microsoft.Xna.Framework;
using Vestige.Game.Input;
using Vestige.Game.Tiles;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Items.Weapons
{
    internal class Axe : IWeapon
    {
        private int _axePower;
        public Axe(int axePower)
        {
            this._axePower = axePower;
        }
        public bool UseItem()
        {
            Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(Vestige.TILESIZE, Vestige.TILESIZE);
            if (Vector2.Distance(mouseTilePosition.ToVector2() * Vestige.TILESIZE, Main.EntityManager.GetPlayer().Position) > Main.EntityManager.GetPlayer().MaxBreakDistance)
                return false;
            if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y), TileProperty.AxeMineable))
            {
                WorldGen.World.DamageTile(mouseTilePosition, _axePower);
                return true;
            }
            return false;
        }
    }
}
