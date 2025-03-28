using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Items
{
    public class Pickaxe : Item
    {
        private int _minePower;
        public Pickaxe(int id, string name, string decription, Texture2D image, double useSpeed, int minePower)
        {
            this.ID = id;
            this.Name = name;
            this.Description = decription;
            this.Image = image;
            this.UseSpeed = useSpeed;
            this._minePower = minePower;
            this.Quantity = 1;
            this.Stackable = false;
            this.AutoUse = true;
        }

        public override bool UseItem()
        {
            
            Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(Globals.TILESIZE, Globals.TILESIZE);
            if (WorldGen.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y) == 0)
                return false;
            WorldGen.World.DamageTile(mouseTilePosition, _minePower);
            return true;
        }
    }
}
