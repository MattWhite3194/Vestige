using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Vestige.Game.Entities.Projectiles.ProjectileBehaviors
{
    public class Arrow : IProjectileBehavior
    {
        private float _maxFallSpeed = 300f;
        public void AI(double delta, Projectile projectile)
        {
            Vector2 newVelocity = projectile.Velocity;
            newVelocity.Y = newVelocity.Y + (float)delta * (Vestige.GRAVITY / 6);
            if (newVelocity.Y > _maxFallSpeed)
            {
                newVelocity.Y = _maxFallSpeed;
            }
            projectile.Velocity = newVelocity;
            projectile.Rotation = (float)Math.Atan2(newVelocity.Y, newVelocity.X) + MathHelper.PiOver2;
        }

        public IProjectileBehavior Clone()
        {
            return new Arrow();
        }
    }
}
