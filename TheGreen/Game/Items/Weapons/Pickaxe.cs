using Microsoft.Xna.Framework;
using TheGreen.Game.Input;
using TheGreen.Game.Tiles;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Items.WeaponBehaviors
{
    internal class Pickaxe : IWeapon
    {
        private int _minePower;
        public Pickaxe(int minePower)
        {
            this._minePower = minePower;
        }
        public bool UseItem()
        {
            Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(Globals.TILESIZE, Globals.TILESIZE);
            if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y), TileProperty.PickaxeMineable))
            {
                WorldGen.World.DamageTile(mouseTilePosition, _minePower);
                return true;
            }
            return false;
        }
    }
}
