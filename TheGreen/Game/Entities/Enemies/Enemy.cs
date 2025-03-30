using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Timers;
using TheGreen.Game.Entities.Enemies.EnemyBehaviors;

namespace TheGreen.Game.Entities.Enemies
{
    public class Enemy : Entity
    {
        public int ID;
        public string Name;
        private int _health = 100;
        private int _damage;
        private IEnemyBehavior _behavior;
        private Timer _invincibilityTimer;
        private bool invincible = false;
        public Enemy(int id, string name, Texture2D image, Vector2 size, bool collidesWithTiles, Type behaviorType, List<(int, int)> animationFrames = null) : base(image, default, size: size, animationFrames: animationFrames)
        {
            ID = id;
            Name = name;
            CollidesWithTiles = collidesWithTiles;
            if (typeof(IEnemyBehavior).IsAssignableFrom(behaviorType))
            {
                _behavior = (IEnemyBehavior)Activator.CreateInstance(behaviorType);
            }
            _invincibilityTimer = new Timer(100);
            _invincibilityTimer.Elapsed += OnInvincibleTimeout;
            this.Layer = CollisionLayer.Enemy;
            this.CollidesWith = CollisionLayer.Player | CollisionLayer.ItemCollider;
        }
        public override void Update(double delta)
        {
            _behavior?.AI(delta, this);
            base.Update(delta);
        }
        public override void OnCollision(Entity entity)
        {
            
        }
        private void OnInvincibleTimeout(object sender, ElapsedEventArgs e)
        {
            invincible = false;
        }
    }
}
