using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vestige.Game.Input;
using Vestige.Game.Items;
using Vestige.Game.UI.Components;

namespace Vestige.Game.Inventory
{
    public class DragItem : UIComponent
    {
        public Item Item;

        public DragItem(Vector2 position) : base(position)
        {
            Position = position;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Item == null) return;
            spriteBatch.Draw(Item.Image, Position, null, Color.White, _rotation, Origin, 1.0f, SpriteEffects.None, 0.0f);
            if (Item.Stackable)
            {
                string quantity = Item.Quantity.ToString();
                Vector2 stringOrigin = ContentLoader.GameFont.MeasureString(quantity) / 2;
                Vector2 stringPosition = Position + new Vector2(Item.Image.Width / 2, Item.Image.Height + 2);
                spriteBatch.DrawString(ContentLoader.GameFont, quantity, stringPosition, Color.White, _rotation, stringOrigin, 1.0f, SpriteEffects.None, 0.0f);
            }
        }

        public override void Update(double delta)
        {
            Position = Vector2.Transform(InputManager.GetMouseWindowPosition(), Matrix.Invert(Vestige.UIScaleMatrix));
        }
    }
}
