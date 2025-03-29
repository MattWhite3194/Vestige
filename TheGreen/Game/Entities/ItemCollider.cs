using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using TheGreen.Game.Input;
using TheGreen.Game.Inventory;
using TheGreen.Game.Items;

namespace TheGreen.Game.Entities
{
    public class ItemCollider : Entity
    {
        private InventoryManager _inventory;
        public Item Item;
        public bool Active;
        private double _holdTime;
        private bool _leftReleased = false;
        public ItemCollider(InventoryManager inventory) : base(null, default, default)
        {
            this._inventory = inventory;
        }
        public void HandleInput(InputEvent @event)
        {
            if (@event is MouseInputEvent mouseInputEvent)
            {
                if (mouseInputEvent.InputButton == InputButton.LeftMouse)
                {
                    if (mouseInputEvent.EventType == InputEventType.MouseButtonDown)
                    {
                        Debug.WriteLine(_inventory);
                        if (Active || _inventory.GetSelected() == null) return;
                        _holdTime = 0.0f;
                        _leftReleased = false;
                        Active = true;
                        this.Item = _inventory.GetSelected();
                    }
                    else if (mouseInputEvent.EventType == InputEventType.MouseButtonUp)
                    {
                        _leftReleased = true;
                    }
                    InputManager.MarkInputAsHandled(@event);
                }
            }
        }

        public override void Update(double delta)
        {
            if (!Active)
                return;
            if (_holdTime == 0)
            {
                if (Item.UseItem() && Item.Stackable)
                    _inventory.SetSelectedQuantity(Item.Quantity - 1);
            }
            switch (Item.UseStyle)
            {
                case UseStyle.Point:
                    if (_holdTime == 0.0f)
                    {
                        Vector2 playerPosition = Main.EntityManager.GetPlayer().Position;
                        Point mousePosition = InputManager.GetMouseWorldPosition();
                        Rotation = (float)Math.Atan((playerPosition.Y - mousePosition.Y) / (playerPosition.X - mousePosition.X));
                    }
                    break;
                case UseStyle.Swing:
                    Rotation = (float)(_holdTime / Item.UseSpeed * MathHelper.PiOver2);
                    break;
                default:
                    break;
            }
            _holdTime += delta;
            if (_holdTime >= Item.UseSpeed)
            {
                _holdTime = 0.0f;
                if (!Item.AutoUse || _leftReleased)
                {
                    Active = false;
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 bottomPosition = new Vector2(Position.X, Position.Y + Item.Image.Height);
            switch (Item.UseStyle)
            {
                case UseStyle.Hold:
                    
                    spriteBatch.Draw(Item.Image,
                        new Vector2((int)Position.X, (int)Position.Y) + (FlipSprite ? new Vector2(12, 0) : new Vector2(8, 0)),
                        null,
                        Color.White,
                        0.0f,
                        FlipSprite ? new Vector2(Item.Image.Width + 8, Item.Image.Height) : new Vector2(-8, Item.Image.Height),
                        Item.Scale,
                        FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                        0f
                    );
                    break;
                case UseStyle.Point:
                    spriteBatch.Draw(Item.Image,
                        new Vector2((int)Position.X, (int)Position.Y) + (FlipSprite ? new Vector2(12, 0) : new Vector2(8, 0)),
                        null,
                        Color.White,
                        Rotation,
                        FlipSprite ? new Vector2(Item.Image.Width + 8, Item.Image.Height) : new Vector2(-8, Item.Image.Height),
                        Item.Scale,
                        FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                        0f
                    );
                    break;
                case UseStyle.Swing:
                    spriteBatch.Draw(Item.Image,
                        new Vector2((int)Position.X, (int)Position.Y) + (FlipSprite ? new Vector2(12, 0) : new Vector2(8, 0)),
                        null,
                        Color.White,
                        FlipSprite ? -Rotation : Rotation,
                        FlipSprite ? new Vector2(Item.Image.Width + 8, Item.Image.Height) : new Vector2(-8, Item.Image.Height),
                        Item.Scale,
                        FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                        0f
                    );
                    break;
                default:
                    break;
            }
        }

        public Rectangle GetItemBounds()
        {
            //TODO: account for item rotation
            if (Active && Item is WeaponItem weaponItem && weaponItem.SpriteDoesDamage)
            {
                return new Rectangle(Position.ToPoint(), new Point(Item.Image.Width, Item.Image.Height));
            }
            return default;
        }
    }
}
