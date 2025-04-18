using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheGreen.Game.Drawables
{
    public class ParallaxManager
    {
        private List<ParallaxBackground> _parallaxBackgrounds = new List<ParallaxBackground>();

        public void AddParallaxBackground(ParallaxBackground parallaxBackground)
        {
            _parallaxBackgrounds.Add(parallaxBackground);
        }
        public void RemoveParallaxBackground()
        {

        }

        public void Update(double delta, Vector2 position)
        {
            for (int i = 0; i < _parallaxBackgrounds.Count; i++)
            {
                _parallaxBackgrounds[i].Update(delta, position);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            
            for (int i = 0; i < _parallaxBackgrounds.Count; i++)
            {
                if (!_parallaxBackgrounds[i].Active)
                    continue;
                _parallaxBackgrounds[i].Draw(spriteBatch, color);
            }
        }
    }
}
