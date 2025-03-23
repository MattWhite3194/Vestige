using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheGreen.Game.Drawables
{
    public class Sprite
    {
        public Texture2D Image;
        public Color Color = Color.White;
        public Vector2 Position, Origin;
        private Vector2 _size;
        public AnimationComponent Animation;
        public bool FlipSprite = false;
        public float Rotation = 0.0f;
        public float Scale = 1.0f;
        public Vector2 Size
        {
            get
            {
                if (_size == default)
                {
                    return Image == null ? Vector2.Zero : new Vector2(Image.Width, Image.Height);
                }
                return _size;
            }
            set
            {
                _size = value;
                Origin = new Vector2(value.X / 2, value.Y / 2);
            }
        }

        public Sprite(Texture2D image, Vector2 position, Vector2 size = default, List<(int, int)> animationFrames = null)
        {
            this.Image = image;
            this.Position = position;
            if (size != default)
            {
                this.Size = size;
            }
            if (animationFrames != null)
            {
                this.Animation = new AnimationComponent(size, animationFrames);
            }
        }

        public virtual void Update(double delta)
        {
            Animation?.Update(delta);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Image,
                new Vector2((int)Position.X, (int)Position.Y) + Origin,
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
