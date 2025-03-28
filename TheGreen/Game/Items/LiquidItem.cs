using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Items
{
    public class LiquidItem : Item
    {
        private ushort _liquidID;
        public LiquidItem(int id, string name, string description, Texture2D Image, ushort liquidID)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Image = Image;
            this.Stackable = true;
            //TODO: liquid IDS like lava or water or whatever
            this._liquidID = liquidID;
            this.UseSpeed = 0.15;
            this.AutoUse = true;
        }
        public override bool UseItem()
        {
            Point mouseTilePosition = InputManager.GetMouseWorldPosition() / new Point(Globals.TILESIZE, Globals.TILESIZE);
            if (WorldGen.World.GetLiquid(mouseTilePosition.X, mouseTilePosition.Y) != 255)
            {
                WorldGen.World.SetLiquid(mouseTilePosition.X, mouseTilePosition.Y, 255);
                return true;
            }
            return false;
        }
    }
}
