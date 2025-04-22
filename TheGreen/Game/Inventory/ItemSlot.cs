using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Items;
using TheGreen.Game.UI.Components;

namespace TheGreen.Game.Inventory
{
    public class ItemSlot : UIComponent
    {
        public ItemSlot(Vector2 position, Texture2D image, Color color) : base(position, image, color) { }
        public void DrawItem(SpriteBatch spriteBatch, Item item)
        {
            if (item == null) return;
            Vector2 itemPosition = Position + new Vector2((int)(Size.X - item.Image.Width) / 2, (int)(Size.Y - item.Image.Height) / 2);
            spriteBatch.Draw(
                item.Image,
                itemPosition,
                Color.White
                );
            
            if (item.Stackable)
            {
                string quantity = item.Quantity.ToString();
                Vector2 stringOrigin = ContentLoader.GameFont.MeasureString(quantity) / 2;
                Vector2 stringPosition = itemPosition + new Vector2(item.Image.Width / 2, item.Image.Height + 2);
                spriteBatch.DrawString(ContentLoader.GameFont, quantity, stringPosition, Color.White, _rotation, stringOrigin, 1.0f, SpriteEffects.None, 0.0f);
            }
        }
    }
}
