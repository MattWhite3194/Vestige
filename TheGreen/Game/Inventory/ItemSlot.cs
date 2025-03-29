using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Items;
using TheGreen.Game.UIComponents;

namespace TheGreen.Game.Inventory
{
    public class ItemSlot : UIComponent
    {
        private Item _item;
        public Item Item 
        { 
            get { return _item; } 
            set 
            { 
                _item = value;
                _quantityLabel.SetText(_item?.Quantity.ToString() ?? "");
                if (_item == null) return;
                _itemImagePosition = Position + new Vector2((int)(Size.X - _item.Image.Width) / 2, (int)(Size.Y - _item.Image.Height) / 2);
            }
        }
        private Vector2 _itemImagePosition;
        private Label _quantityLabel;

        public ItemSlot(Vector2 position, Texture2D image, Color color) : base(position, image, color)
        {
            _quantityLabel = new Label(Vector2.Zero, "", Vector2.Zero, drawCentered: true, scale: 0.6f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_item == null) return;

            spriteBatch.Draw(_item.Image, _itemImagePosition, Color.White);

            if (_item.Stackable)
            {
                _quantityLabel.Draw(spriteBatch);
            }
        }

        //Children of this component need to be updated when the grid parent changes this components position
        protected override void HandlePositionUpdate()
        {
            _quantityLabel.Position = Position + new Vector2(Size.X/2, Size.Y - 10);
        }
    }
}
