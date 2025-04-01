using Microsoft.Xna.Framework;
using TheGreen.Game.Items;

namespace TheGreen.Game.Entities
{
    /// <summary>
    /// An entity that contains an item.
    /// </summary>
    public class ItemDrop : Entity
    {
        private Item _item;
        private int _maxFallSpeed = 700;
        public ItemDrop(Item item, Vector2 position) : base(item.Image, position)
        {
            _item = item;
            CollidesWithTiles = true;
            this.Layer = CollisionLayer.ItemDrop;
        }
        public override void Update(double delta)
        {
            base.Update(delta);
            Vector2 newVelocity = Velocity;
            newVelocity.Y += Globals.GRAVITY / 2 * (float)delta;
            if (newVelocity.Y > _maxFallSpeed)
                newVelocity.Y = _maxFallSpeed;
            Velocity = newVelocity;
        }
        public Item GetItem() { return _item; }
    }
}
