using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TheGreen.Game.Drawables;

namespace TheGreen.Game.Entities
{
    public abstract class Entity : Sprite
    {
        public Vector2 Velocity;
        public bool IsOnFloor = false, IsOnCeiling = false;
        public bool CollidesWithTiles;

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
