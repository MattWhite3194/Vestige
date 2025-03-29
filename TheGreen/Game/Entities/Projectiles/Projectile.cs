using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TheGreen.Game.Entities.Projectiles
{
    public class Projectile : Entity
    {
        public Projectile(Texture2D image, Vector2 position, Vector2 size = default, List<(int, int)> animationFrames = null) : base(image, position, size, animationFrames)
        {
        }
    }
}
