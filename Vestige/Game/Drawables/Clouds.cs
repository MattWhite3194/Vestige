using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Vestige.Game.Drawables
{
    public class Clouds : ParallaxBackground
    {
        private (Vector2 position, int atlas)[] _clouds;
        private int _numAtlases = 3;
        public Clouds(Texture2D image, Vector2 speed, Vector2 initialPlayerPosition, int maxDrawDepth, int minDrawDepth) : base(image, speed, initialPlayerPosition, minDrawDepth, maxDrawDepth)
        {
            size = new Vector2((image.Width * 20) + 200, (image.Height * 2) + 60);
            _clouds = new (Vector2 position, int atlas)[20 * 6];
            Vector2 position = Vector2.Zero;
            int index = 0;
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (Main.Random.Next(0, 5) == 1)
                    {
                        _clouds[index] = (new Vector2((i * (10 + image.Width)) + (((Main.Random.NextSingle() * 2.0f) - 1.0f) * 50), (j * (10 + (image.Height / _numAtlases))) + (((Main.Random.NextSingle() * 2.0f) - 1.0f) * 50)), Main.Random.Next(0, 3));
                        index++;
                    }
                }
            }
        }
        public override void Update(double delta, Vector2 position)
        {
            offset.X += 5 * (float)delta;
            base.Update(delta, position);
        }
        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            for (int i = 0; i <= (int)Math.Ceiling(Vestige.NativeResolution.X / size.X); i++)
            {
                for (int j = 0; j < _clouds.Length; j++)
                {
                    if (_clouds[j] == default)
                        break;
                    spriteBatch.Draw(backgroundImage,
                        new Vector2(offset.X + (i * size.X) - size.X, offset.Y + 100) + _clouds[j].position,
                        new Rectangle(0, _clouds[j].atlas * (backgroundImage.Height / _numAtlases), backgroundImage.Width, backgroundImage.Height / _numAtlases), color);
                }
            }
        }
    }
}
