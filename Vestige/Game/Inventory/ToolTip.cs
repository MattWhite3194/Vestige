using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Vestige.Game.Input;
using Vestige.Game.Items;
using Vestige.Game.UI.Components;

namespace Vestige.Game.Inventory
{
    public class ToolTip : UIComponent
    {
        private bool _drawBackground;
        private string _text;
        public int ItemSlotIndex = -1;
        public ToolTip() : base(Vector2.Zero)
        {
        }
        public override void Update(double delta)
        {
            Position = Vector2.Transform(InputManager.GetMouseWindowPosition() + new Vector2(10), Matrix.Invert(Vestige.UIScaleMatrix));
        }
        public void SetText(string text, bool drawBackground = false)
        {
            _text = text;
            Size = ContentLoader.GameFont.MeasureString(text);
            _drawBackground = drawBackground;
        }
        public void ShowItemStats(Item item)
        {
            if (item == null)
            {
                SetText("");
                return;
            }
            SetText(item.Name + (item.Description != "" ? "\n" +  item.Description : "" ), true);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_drawBackground)
            {
                Utilities.DrawRoundedRectangle(spriteBatch, (int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y, Vestige.UIPanelColorOpaque);
            }
            spriteBatch.DrawString(ContentLoader.GameFont, _text, Vector2.Floor(Position), Color);
        }
    }
}
