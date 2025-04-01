using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TheGreen.Game.Drawables;

namespace TheGreen.Game.Entities
{
    /// <summary>
    /// Defines a basic entity, something that moves and collides in the world
    /// </summary>
    public abstract class Entity : Sprite
    {
        public Vector2 Velocity;
        public bool IsOnFloor = false, IsOnCeiling = false;
        public bool CollidesWithTiles;
        public bool Active = true;
        public CollisionLayer Layer;
        public CollisionLayer CollidesWith;

        protected Entity(Texture2D image, Vector2 position, Vector2 size = default, List<(int, int)> animationFrames = null) : base(image, position, size, animationFrames)
        {
        }

        public virtual void OnCollision(Entity entity)
        {

        }

        public virtual void OnTileCollision()
        {

        }

        public Rectangle GetBounds()
        {
            return new Rectangle(Position.ToPoint(), Size.ToPoint());
        }
    }
}
