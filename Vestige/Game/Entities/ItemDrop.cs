using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.Items;

namespace Vestige.Game.Entities
{
    /// <summary>
    /// An entity that contains an item.
    /// </summary>
    public class ItemDrop : Entity
    {
        private Item _item;
        private int _maxFallSpeed = 700;
        public static Vector2 ColliderSize = new Vector2(10, 10);
        private float _acceleration = 50f;
        public bool CanBePickedUp;
        private float _pickupTimer;
        public ItemDrop(Item item, Vector2 position, bool canBePickedUp = true) : base(item.Image, position, drawLayer: 0, name: item.Name)
        {
            _item = item;
            CollidesWithTiles = true;
            this.Size = ColliderSize;
            this.Layer = CollisionLayer.ItemDrop;
            this.CollidesWith = CollisionLayer.ItemDrop;
            CanBePickedUp = canBePickedUp;
            _pickupTimer = canBePickedUp ? 0.0f : 1.0f;
        }
        public Item GetItem() { return _item; }
        public override void Update(double delta)
        {
            base.Update(delta);
            if (_pickupTimer > 0.0f)
            {
                _pickupTimer -= (float)delta;
                if (_pickupTimer <= 0.0f)
                {
                    CanBePickedUp = true;
                }
            }
            Vector2 newVelocity = Velocity;
            newVelocity.Y += Vestige.GRAVITY / 2 * (float)delta;
            if (newVelocity.Y > _maxFallSpeed)
                newVelocity.Y = _maxFallSpeed;
            if (newVelocity.X != 0.0f)
            {
                int direction = Math.Sign(newVelocity.X);
                newVelocity.X -= _acceleration * (float)delta * direction;
                if (Math.Sign(newVelocity.X) != direction)
                    newVelocity.X = 0.0f;
            }
            Velocity = newVelocity;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Point centerTilePosition = ((Position + Size / 2) / Vestige.TILESIZE).ToPoint();
            spriteBatch.Draw(Image,
                new Vector2((int)Position.X, (int)Position.Y) + Origin + new Vector2(-_item.Image.Width / 2 + 5, -_item.Image.Height + 10),
                Animation?.AnimationRectangle ?? null,
                Main.LightEngine.GetLight(centerTilePosition.X, centerTilePosition.Y),
                Rotation,
                Origin,
                Scale,
                FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0.0f
            );
        }
    }
}
