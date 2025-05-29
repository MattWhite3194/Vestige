using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TheGreen.Game.Entities.Projectiles.ProjectileBehaviors;

namespace TheGreen.Game.Entities.Projectiles
{
    public class Projectile : Entity
    {
        private float _timeLeft;
        private IProjectileBehavior _behavior;
        public readonly int Damage;
        public readonly int Knockback;
        public readonly bool Friendly;
        private List<(int, int)> _animationFrames;
        public Projectile(Texture2D image, Vector2 size, int damage, int knockback, float timeLeft, bool friendly, bool collidesWithTiles, IProjectileBehavior behavior, List<(int, int)> animationFrames = null) : base(image, default, size, animationFrames: animationFrames, drawLayer: 1)
        {
            Layer = 0;
            CollidesWith = 0;
            CollidesWithTiles = collidesWithTiles;
            Damage = damage;
            Knockback = knockback;
            _timeLeft = timeLeft;
            Friendly = friendly;
            _behavior = behavior;
            _animationFrames = animationFrames;
            Layer |= friendly ? CollisionLayer.FriendlyProjectile : CollisionLayer.HostileProjectile;
            CollidesWith |= friendly ? CollisionLayer.Enemy : CollisionLayer.Player;
        }
        public override void OnCollision(Entity entity)
        {
            base.OnCollision(entity);
        }
        public override void Update(double delta)
        {
            _behavior.AI(delta, this);
            _timeLeft -= (float)delta;
            if (_timeLeft <= 0.0f)
                Active = false;
            base.Update(delta);
        }
        public static Projectile CloneProjectile(Projectile projectile)
        {
            return new Projectile(projectile.Image, projectile.Size, projectile.Damage, projectile.Knockback, projectile._timeLeft, projectile.Friendly, projectile.CollidesWithTiles, projectile._behavior.Clone(), projectile._animationFrames);
        }
    }
}
