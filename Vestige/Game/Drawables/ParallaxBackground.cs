using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Vestige.Game.Drawables
{
    public class ParallaxBackground
    {
        protected Texture2D backgroundImage;
        protected Vector2 offset;
        public Vector2 Speed;
        /// <summary>
        /// The height in the world that this parallax background should be draw in
        /// </summary>
        private int _maxDrawDepth;
        private int _minDrawDepth;
        public bool Active = true;
        private Vector2 _currentPosition;
        private float _alpha = 1.0f;
        protected Vector2 size;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="backgroundImage"></param>
        /// <param name="speed"></param>
        /// <param name="initialPlayerPosition"></param>
        /// <param name="maxDrawDepth">The lowest height this parallax background draws at, also the height at which the offset is calculated from. If this position is at the bottom of the screen, the parallax background will draw here with no offset</param>
        /// <param name="minDrawDepth">The point at which the parallax background becomes invisible</param>
        public ParallaxBackground(Texture2D backgroundImage, Vector2 speed, Vector2 initialPlayerPosition, int maxDrawDepth, int minDrawDepth, Vector2 size = default)
        {
            this.backgroundImage = backgroundImage;
            Speed = speed;
            _currentPosition = initialPlayerPosition;
            _maxDrawDepth = maxDrawDepth;
            _minDrawDepth = minDrawDepth;
            offset = new Vector2(0, (maxDrawDepth - initialPlayerPosition.Y) * Speed.Y);
            if (size == default)
            {
                this.size = new Vector2(backgroundImage.Width, backgroundImage.Height);
            }
        }

        public virtual void Update(double delta, Vector2 position)
        {
            //Only Activate this parallax if the player is above the maxdrawdepth
            Active = position.Y <= _maxDrawDepth && position.Y > _minDrawDepth;
            //offset increases as the player is moving left, and decreases as the player is moving right
            offset.X -= (position.X - _currentPosition.X) * Speed.X;
            offset.Y = (_maxDrawDepth - position.Y) * Speed.Y;
            _currentPosition = position;
            if (offset.X < 0)
            {
                offset.X = size.X + offset.X;
            }
            if (offset.X > size.X)
            {
                offset.X = offset.X - size.X;
            }
            if (Active && _alpha < 1.0f)
            {
                _alpha = (float)Math.Min(1.0f, _alpha + (4 * delta));
            }
            else if (!Active && _alpha > 0.0f)
            {
                _alpha = (float)Math.Max(0.0f, _alpha - (4 * delta));
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (_alpha == 0.0f)
                return;
            color *= _alpha;
            for (int i = 0; i <= (int)Math.Ceiling(Vestige.NativeResolution.X / (float)size.X); i++)
            {
                spriteBatch.Draw(
                    backgroundImage,
                    new Vector2(offset.X + (i * size.X) - size.X, Vestige.NativeResolution.Y - size.Y + offset.Y),
                    color
                    );
            }
        }
    }
}
