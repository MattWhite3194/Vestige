using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vestige.Game.Entities;
using Vestige.Game.Input;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Items
{
    public class LiquidItem : Item
    {
        public readonly ushort LiquidID;
        public LiquidItem(int id, string name, string description, Texture2D image, ushort liquidID) : base(id, name, description, image, default, true, true, 0.15, true, 50, UseStyle.Swing)
        {
            //TODO: liquid IDS like lava or water or whatever
            this.LiquidID = liquidID;
        }
        public override bool UseItem(Player player)
        {
            Point mouseTilePosition = Main.GetMouseWorldPosition() / new Point(Vestige.TILESIZE);
            if (Main.World.GetLiquid(mouseTilePosition.X, mouseTilePosition.Y) != WorldGen.MaxLiquid)
            {
                Main.World.SetLiquid(mouseTilePosition.X, mouseTilePosition.Y, WorldGen.MaxLiquid, true);
                return true;
            }
            return false;
        }
        protected override Item CloneItem()
        {
            return new LiquidItem(ID, Name, Description, Image, LiquidID);
        }
    }
}
