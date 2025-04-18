using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace TheGreen.Game.Drawables
{
    public class ParallaxBackground
    {
        private Texture2D _backgroundImage;
        private Vector2 Offset;
        public Vector2 Speed;
        /// <summary>
        /// The height in the world that this parallax background should be draw in
        /// </summary>
        private int _maxDrawDepth;
        private int _minDrawDepth;
        public bool Active = true;
        private Vector2 _currentPosition;
        private float _alpha = 1.0f;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="backgroundImage"></param>
        /// <param name="speed"></param>
        /// <param name="initialPlayerPosition"></param>
        /// <param name="maxDrawDepth">The lowest height this parallax background draws at, also the height at which the offset is calculated from. If this position is at the bottom of the screen, the parallax background will draw here with no offset</param>
        /// <param name="minDrawDepth">The point at which the parallax backgorund becomes invisible</param>
        public ParallaxBackground(Texture2D backgroundImage, Vector2 speed, Vector2 initialPlayerPosition, int maxDrawDepth, int minDrawDepth) 
        { 
            this._backgroundImage = backgroundImage;
            this.Speed = speed;
            this._currentPosition = initialPlayerPosition;
            this._maxDrawDepth = maxDrawDepth;
            this._minDrawDepth = minDrawDepth;
            this.Offset = new Vector2(0, (maxDrawDepth - initialPlayerPosition.Y) * Speed.Y);
        }

        public void Update(double delta, Vector2 position)
        {
            //Only Activate this parallax if the player is above the maxdrawdepth
            Active = position.Y <= _maxDrawDepth && position.Y > _minDrawDepth;
            //offset increases as the player is moving left, and decreases as the player is moving right
            Offset.X -= (position.X - _currentPosition.X) * Speed.X;
            Offset.Y = (_maxDrawDepth - position.Y) * Speed.Y;
            _currentPosition = position;
            if (Offset.X < 0)
            {
                Offset.X = _backgroundImage.Width + Offset.X;
            }
            if (Offset.X > _backgroundImage.Width)
            {
                Offset.X = Offset.X - _backgroundImage.Width;
            }
            _alpha = Math.Min(50, Math.Max(0, Math.Min(_maxDrawDepth - position.Y, position.Y - _minDrawDepth))) / 50f;
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            color *= _alpha;
            for (int i = 0; i <= (int)Math.Ceiling(TheGreen.NativeResolution.X / (float)_backgroundImage.Width); i++)
            {
                spriteBatch.Draw(
                    _backgroundImage,
                    new Vector2((int)(Offset.X + (i * _backgroundImage.Width) - _backgroundImage.Width), (int)(TheGreen.NativeResolution.Y - _backgroundImage.Height + Offset.Y)),
                    color
                    );
            }
        }
    }
}
