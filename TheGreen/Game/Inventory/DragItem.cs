using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.UIComponents;

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
                _quantityLabel.SetText(_item?.Quantity.ToString() ?? "");
            }
        }
        private Label _quantityLabel;

        public DragItem(Vector2 position) : base(position)
        {
            this.Position = position;
            _quantityLabel = new Label(Vector2.Zero, "", Vector2.Zero, scale: 0.6f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_item == null) return;
            spriteBatch.Draw(_item.Image, Position, Color.White);
            if (_item.Stackable)
            {
                _quantityLabel.Draw(spriteBatch);
            }
        }

        public override void Update(double delta)
        {
            Position = InputManager.GetMouseWindowPosition();
            _quantityLabel.Position = this.Position + new Vector2(Size.X / 2, Size.Y - 10);
        }
    }
}
