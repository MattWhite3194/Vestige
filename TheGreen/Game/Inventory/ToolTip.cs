using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.UI.Components;

namespace TheGreen.Game.Inventory
{
    public class ToolTip : UIComponent
    {
        public bool DrawBackground = false;
        private string _text;
        public int ItemSlotIndex = -1;
        public ToolTip() : base(Vector2.Zero)
        {
        }
        public override void Update(double delta)
        {
            Position = Vector2.Transform(InputManager.GetMouseWindowPosition() + new Vector2(10), Matrix.Invert(TheGreen.UIScaleMatrix));
        }
        public void SetText(string text)
        {
            _text = text;
            Size = ContentLoader.GameFont.MeasureString(text);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DrawBackground)
            {
                DebugHelper.DrawFilledRectangle(spriteBatch, new Rectangle(Position.ToPoint(), Size.ToPoint()), Color.Green);
            }
            spriteBatch.DrawString(ContentLoader.GameFont, _text, Position, Color);
        }
    }
}
