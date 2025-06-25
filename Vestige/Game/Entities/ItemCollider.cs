using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Vestige.Game.Input;
using Vestige.Game.Inventory;
using Vestige.Game.Items;

namespace Vestige.Game.Entities
{
    public class ItemCollider : Entity
    {
        private InventoryManager _inventory;
        public Item Item;
        public bool ItemActive = false;
        private double _holdTime;
        private bool _leftReleased = false;
        public const float MaxRotation = MathHelper.PiOver4 * 2.5f;
        public bool ForcePlayerFlip = false;
        private bool _canUseItem = false;
        private bool _altUse;
        private Player _player;
        public ItemCollider(Player player, InventoryManager inventory) : base(null, default, default)
        {
            _player = player;
            _inventory = inventory;
        }
        public void HandleInput(InputEvent @event)
        {
            if (@event is MouseInputEvent mouseInputEvent)
            {
                if (mouseInputEvent.InputButton == InputButton.LeftMouse)
                {
                    if (mouseInputEvent.EventType == InputEventType.MouseButtonDown)
                    {
                        _leftReleased = false;
                        if (ItemActive) return;
                        Item = _inventory.GetSelected();
                        if (Item == null || !Item.CanUse) return;
                        _holdTime = 0.0f;
                        _canUseItem = !_inventory.UseSelected(_altUse);
                        ItemActive = true;
                    }
                    else if (mouseInputEvent.EventType == InputEventType.MouseButtonUp)
                    {
                        _leftReleased = true;
                        _canUseItem = false;
                    }
                    InputManager.MarkInputAsHandled(@event);
                }
            }
            else if (@event.InputButton == InputButton.AltUse)
            {
                _altUse = @event.EventType == InputEventType.KeyDown;
            }
        }

        public override void Update(double delta)
        {
            if (!ItemActive)
                return;

            FlipSprite = _player.FlipSprite;
            Position = _player.Position + (_player.Size / 2) - new Vector2(0, Item.Image.Height - Item.Origin.Y + 4) + (FlipSprite ? new Vector2(-Item.Image.Width - 3, 0) : new Vector2(3, 0));
            Origin = new Vector2(0, Item.Image.Height) + (FlipSprite ? new Vector2(Item.Image.Width - Item.Origin.X + 9, -Item.Origin.Y) : new Vector2(Item.Origin.X - 9, -Item.Origin.Y));
            switch (Item.UseStyle)
            {
                case UseStyle.Point:
                    if (_holdTime == 0.0f)
                    {
                        Vector2 playerPosition = _player.Position;
                        Point mousePosition = Main.GetMouseWorldPosition();
                        if (mousePosition.X < playerPosition.X)
                        {
                            ForcePlayerFlip = true;
                            FlipSprite = true;
                        }
                        else
                        {
                            ForcePlayerFlip = false;
                            FlipSprite = false;
                        }
                        Rotation = FlipSprite ? (float)Math.Atan2(playerPosition.Y - mousePosition.Y, playerPosition.X - mousePosition.X) : (float)Math.Atan2(mousePosition.Y - playerPosition.Y, mousePosition.X - playerPosition.X);
                    }
                    break;
                case UseStyle.Swing:
                    Rotation = -MathHelper.PiOver4 + (float)(_holdTime / Item.UseSpeed * MaxRotation);
                    Rotation = FlipSprite ? -Rotation : Rotation;
                    break;
                case UseStyle.Throw:
                    Rotation = -MathHelper.PiOver4 + (float)(_holdTime / Item.UseSpeed * MaxRotation);
                    Rotation = FlipSprite ? -Rotation : Rotation;
                    break;
                default:
                    Rotation = 0.0f;
                    break;
            }
            _holdTime += delta;
            if (_holdTime >= Item.UseSpeed)
            {
                _holdTime = 0.0f;
                if (!Item.AutoUse || _leftReleased || _inventory.GetSelected() == null)
                {
                    ItemActive = false;
                }
            }
            if (ItemActive)
            {
                if (_holdTime == 0.0f || (_canUseItem && !_leftReleased))
                    _canUseItem = !_inventory.UseSelected(_altUse);
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!ItemActive || Item.UseStyle == UseStyle.Throw)
                return;
            Point centerTilePosition = ((Position + Origin) / Vestige.TILESIZE).ToPoint();
            spriteBatch.Draw(Item.Image,
                        Vector2.Round(Position + Origin),
                        null,
                        Main.LightEngine.GetLight(centerTilePosition.X, centerTilePosition.Y),
                        Rotation,
                        Origin,
                        Item.Scale,
                        FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                        0.0f
                    );
        }
        public override CollisionRectangle GetBounds()
        {
            if (ItemActive && Item is WeaponItem weaponItem && weaponItem.SpriteDoesDamage)
            {
                Vector2[] corners = {
                //top left
                Position + RotateVector2(new Vector2(FlipSprite ? Item.Image.Width : 0, 0) - Origin),
                //top right
                Position + RotateVector2(new Vector2(FlipSprite ? 0 : Item.Image.Width, 0) - Origin),
                //bottom left
                Position + RotateVector2(new Vector2(FlipSprite ? Item.Image.Width : 0, Item.Image.Height) - Origin),
                //bottom right
                Position + RotateVector2(new Vector2(FlipSprite ? 0 : Item.Image.Width, Item.Image.Height) - Origin)
                };
                float minX = corners.Min(c => c.X);
                float maxX = corners.Max(c => c.X);
                float minY = corners.Min(c => c.Y);
                float maxY = corners.Max(c => c.Y);
                return new CollisionRectangle(minX, minY, (int)(maxX - minX), (int)(maxY - minY));
            }

            return default;
        }
        private Vector2 RotateVector2(Vector2 vector)
        {
            return new Vector2(
                (float)((vector.X * Math.Cos(Rotation)) - (vector.Y * Math.Sin(Rotation))),
                (float)((vector.X * Math.Sin(Rotation)) + (vector.Y * Math.Cos(Rotation))));
        }
    }
}
