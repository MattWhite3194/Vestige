using Microsoft.Xna.Framework;
using TheGreen.Game.Input;
using TheGreen.Game.Tiles;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Items.Weapons
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
            Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(TheGreen.TILESIZE, TheGreen.TILESIZE);
            if (Vector2.Distance(mouseTilePosition.ToVector2() * TheGreen.TILESIZE, Main.EntityManager.GetPlayer().Position) > Main.EntityManager.GetPlayer().MaxBreakDistance)
                return false;
            if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y), TileProperty.HammerMineable))
            {
                WorldGen.World.DamageTile(mouseTilePosition, _hammerPower);
                return true;
            }
            else if (WorldGen.World.GetWallID(mouseTilePosition.X, mouseTilePosition.Y) != 0)
            {
                WorldGen.World.DamageWall(mouseTilePosition, _hammerPower);
                return true;
            }
            return false;
        }
    }
}
