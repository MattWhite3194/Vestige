using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.UI.Components;

namespace TheGreen.Game.Inventory
{
    public class DragItem : UIComponent
    {
        private Item _item;
        public Item Item
        {
            get { return _item; }
            set
            {
                _item = value;
            }
        }

        public DragItem(Vector2 position) : base(position)
        {
            this.Position = position;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_item == null) return;
            spriteBatch.Draw(_item.Image, Position, null, Color.White, _rotation, Origin, 1.0f, SpriteEffects.None, 0.0f);
            if (_item.Stackable)
            {
                string quantity = _item.Quantity.ToString();
                Vector2 stringOrigin = ContentLoader.GameFont.MeasureString(quantity) / 2;
                Vector2 stringPosition = Position + new Vector2(_item.Image.Width / 2, _item.Image.Height + 2);
                spriteBatch.DrawString(ContentLoader.GameFont, quantity, stringPosition, Color.White, _rotation, stringOrigin, 1.0f, SpriteEffects.None, 0.0f);
            }
        }

        public override void Update(double delta)
        {
            Position = Vector2.Transform(InputManager.GetMouseWindowPosition(), Matrix.Invert(TheGreen.UIScaleMatrix));
        }
    }
}
