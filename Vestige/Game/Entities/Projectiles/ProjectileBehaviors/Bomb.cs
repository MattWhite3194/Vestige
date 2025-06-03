using Microsoft.Xna.Framework;
using System;

namespace Vestige.Game.Entities.Projectiles.ProjectileBehaviors
{
    public class Bomb : IProjectileBehavior
    {
        private int _radius;
        private float _maxFallSpeed = 400f;
        public Bomb(int radius) 
        {
            _radius = radius;
        }
        public void AI(double delta, Projectile projectile)
        {
            Vector2 newVelocity = projectile.Velocity;
            newVelocity.Y = newVelocity.Y + (float)delta * (Vestige.GRAVITY / 3);
            if (newVelocity.Y > _maxFallSpeed)
            {
                newVelocity.Y = _maxFallSpeed;
            }
            projectile.Rotation += (float)(projectile.Velocity.X * delta) / projectile.Size.X;
            if (projectile.IsOnFloor)
            {
                newVelocity.X *= 0.99f;
                if (Math.Abs(newVelocity.X) < 2f)
                {
                    newVelocity.X = 0.0f;
                }
            }
            projectile.Velocity = newVelocity;
        }
        public IProjectileBehavior Clone()
        {
            return new Bomb(_radius);
        }
        public void OnCollision(Projectile projectile, Entity entity) { }
        public void OnDeath(Projectile projectile) 
        {
            Vector2 bombTilePosition = Vector2.Round(projectile.Position / Vestige.TILESIZE);
            for (int i = -_radius; i < _radius; i++)
            {
                for (int j = -_radius; j < _radius; j++)
                {
                    if (Vector2.Distance(bombTilePosition, bombTilePosition + new Vector2(i, j)) < _radius)
                    {
                        Main.World.DamageTile(new Point((int)bombTilePosition.X + i, (int)bombTilePosition.Y + j), 500);
                        Main.World.DamageWall(new Point((int)bombTilePosition.X + i, (int)bombTilePosition.Y + j), 500);
                    }
                }
            }
        }
        public void OnTileCollision(Projectile projectile) 
        { 
            //TODO: bounce
        }
    }
}
