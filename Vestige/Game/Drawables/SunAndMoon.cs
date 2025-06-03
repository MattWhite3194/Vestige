using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Vestige.Game.Drawables
{
    public class SunAndMoon : Sprite
    {
        public SunAndMoon(Texture2D image, Vector2 position, Vector2 size = default, Vector2 origin = default, Color color = default, List<(int, int)> animationFrames = null) : base(image, position, size, origin, color, animationFrames)
        {
        }
        public override void Update(double delta)
        {
            base.Update(delta);
        }
        public void UpdatePosition(Vector2 startPosition, float time, float maxTime, float maxPosition, float maxYOffset)
        {
            float normalizedTime = time / maxTime;
            float xTravel = maxPosition - startPosition.X;
            Position = startPosition + new Vector2(normalizedTime * xTravel, (float)Math.Sin(MathHelper.Pi * normalizedTime) * maxYOffset);
        }
    }
}
