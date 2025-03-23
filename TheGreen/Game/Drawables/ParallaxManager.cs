using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TheGreen.Game.Drawables
{
    public class ParallaxManager
    {
        private static List<ParallaxBackground> _parallaxBackgrounds = new List<ParallaxBackground>();

        private static ParallaxManager _instance;
        private ParallaxManager()
        {

        }

        public static ParallaxManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ParallaxManager();
                }
                return _instance;
            }
        }

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
