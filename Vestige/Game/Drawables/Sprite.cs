using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Vestige.Game.Drawables
{
    public class Sprite
    {
        public Texture2D Image;
        public Color Color;
        public Vector2 Position;
        public AnimationComponent Animation;
        public bool FlipSprite = false;
        public float Rotation = 0.0f;
        public float Scale = 1.0f;
        private Vector2 _size;
        public Vector2 Size
        {
            get
            {
                return _size == default ? Image == null ? Vector2.Zero : new Vector2(Image.Width, Image.Height) : _size;
            }
            set
            {
                _size = value;
            }
        }
        private Vector2 _origin;
        /// <summary>
        /// The origin at which the sprite is scaled from
        /// </summary>
        public Vector2 Origin
        {
            get
            {
                return _origin == default ? new Vector2(Size.X / 2, Size.Y / 2) : _origin;
            }
            set
            {
                _origin = value;
            }
        }

        public Sprite(Texture2D image, Vector2 position, Vector2 size = default, Vector2 origin = default, Color color = default, List<(int, int)> animationFrames = null)
        {
            Image = image;
            Position = position;
            if (size != default)
            {
                Size = size;
            }
            if (origin != default)
            {
                Origin = origin;
            }
            if (animationFrames != null)
            {
                Animation = new AnimationComponent(size, animationFrames);
            }
            Color = color == default ? Color.White : color;
        }

        public virtual void Update(double delta)
        {
            Animation?.Update(delta);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Image,
                Position + Origin,
                Animation?.AnimationRectangle ?? new Rectangle(Point.Zero, Size.ToPoint()),
                Color,
                Rotation,
                Origin,
                Scale,
                FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0.0f
            );
        }
    }
}
