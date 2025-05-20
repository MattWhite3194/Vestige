using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.UI.Components;

namespace TheGreen.Game.Inventory
{
    internal class ToolTip : UIComponent
    {
        public ToolTip() : base(Vector2.Zero)
        {
        }
        public override void Update(double delta)
        {
            Position = Vector2.Transform(InputManager.GetMouseWindowPosition(), Matrix.Invert(TheGreen.UIScaleMatrix));
        }
        public void DisplayItemInfo(Item item)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch); 
        }
    }
}
