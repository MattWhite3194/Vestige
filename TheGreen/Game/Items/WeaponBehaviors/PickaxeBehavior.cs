using Microsoft.Xna.Framework;
using TheGreen.Game.Input;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Items.WeaponBehaviors
{
    internal class PickaxeBehavior : IWeaponBehavior
    {
        private int _minePower;
        public PickaxeBehavior(int minePower)
        {
            this._minePower = minePower;
        }
        public bool UseItem()
        {
            Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(Globals.TILESIZE, Globals.TILESIZE);
            if (WorldGen.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y) == 0)
                return false;
            WorldGen.World.DamageTile(mouseTilePosition, _minePower);
            return true;
        }
    }
}
