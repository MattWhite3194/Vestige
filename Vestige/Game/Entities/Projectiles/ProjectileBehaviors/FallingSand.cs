using Microsoft.Xna.Framework;
using System;

namespace Vestige.Game.Entities.Projectiles.ProjectileBehaviors
{
    public class FallingSand : IProjectileBehavior
    {
        private float _randomRotation = Main.Random.NextSingle() + 1.0f;
        public void AI(double delta, Projectile projectile)
        {
            projectile.Rotation += _randomRotation;
        }

        public IProjectileBehavior Clone()
        {
            return new FallingSand();
        }

        public void OnCollision(Projectile projectile, Entity entity) { }

        public void OnDeath(Projectile projectile) { }

        public void OnTileCollision(Projectile projectile)
        {
            Point tilePosition = (projectile.Position / 16.0f).ToPoint();
            Main.World.PlaceTile(tilePosition.X, tilePosition.Y, 16);
        }
    }
}
