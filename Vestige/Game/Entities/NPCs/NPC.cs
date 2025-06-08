using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Vestige.Game.Entities.NPCs.Behaviors;
using Vestige.Game.Entities.Projectiles;
using Vestige.Game.Items;

namespace Vestige.Game.Entities.NPCs
{
    public class NPC : Entity
    {
        public readonly int ID;
        private int _maxHealth;
        private int _health;
        public readonly int Damage;
        public readonly int Knockback;
        public readonly bool Friendly;
        private INPCBehavior _behavior;
        private float _invincibilityTimeLeft = -1f;
        private float _maxInvincibilityTime = 0.5f;
        private bool _invincible = false;
        private List<(int, int)> _animationFrames;
        public NPC(int id, 
            string name, 
            Texture2D image, 
            Vector2 size, 
            int health, 
            int damage, 
            bool collidesWithTiles, 
            INPCBehavior behavior,
            bool drawBehindTiles = false,
            bool friendly = false,
            List<(int, int)> animationFrames = null, 
            CollisionLayer layer = default, 
            CollisionLayer collidedWith = default) 
            : base(image, default, size: size, animationFrames: animationFrames, drawLayer: 3, name: name)
        {
            ID = id;
            Damage = damage;
            _maxHealth = health;
            _health = health;
            CollidesWithTiles = collidesWithTiles;
            CollidesWithPlatforms = collidesWithTiles;
            Friendly = friendly;
            _behavior = behavior;
            _animationFrames = animationFrames;
            Layer = layer;
            CollidesWith = collidedWith;
            if (Layer == default)
            {
                Layer = friendly ? CollisionLayer.Player : CollisionLayer.Enemy;
            }
            if (CollidesWith == default)
            {
                CollidesWith = friendly ? CollisionLayer.Enemy | CollisionLayer.HostileProjectile : CollisionLayer.Player | CollisionLayer.ItemCollider | CollisionLayer.FriendlyProjectile;
            }
        }
        public override void Update(double delta)
        {
            if (_invincible)
            {
                _invincibilityTimeLeft -= (float)delta;
                if (_invincibilityTimeLeft <= 0.0f)
                    _invincible = false;
            }
            //TODO: get target based on friendly or not friendly, pass it to AI
            _behavior?.AI(delta, this);
            base.Update(delta);
        }
        public override void OnCollision(Entity entity)
        {
            if (_invincible)
                return;
            switch (entity.Layer)
            {
                case CollisionLayer.ItemCollider:
                    ItemCollider itemCollider = (ItemCollider)entity;
                    ApplyDamage(((WeaponItem)itemCollider.Item).Damage);
                    ApplyKnockback(((WeaponItem)itemCollider.Item).Knockback, entity.Position + entity.Origin);
                    break;
                case CollisionLayer.Enemy:
                    ApplyDamage(((NPC)entity).Damage);
                    ApplyKnockback(((NPC)entity).Knockback, entity.Position + entity.Origin);
                    break;
                case CollisionLayer.FriendlyProjectile:
                    if (Friendly) return;
                    ApplyDamage(((Projectile)entity).Damage);
                    ApplyKnockback(((Projectile)entity).Knockback, entity.Position + entity.Origin);
                    break;
                case CollisionLayer.HostileProjectile:
                    if (Friendly) return;
                    ApplyDamage(((Projectile)entity).Damage);
                    ApplyKnockback(((Projectile)entity).Knockback, entity.Position + entity.Origin);
                    break;
            }
        }
        private void ApplyDamage(int damage)
        {
            this._health -= damage;
            _invincible = true;
            if (this._health <= 0)
                Active = false;
            if (Active)
            {
                _invincibilityTimeLeft = _maxInvincibilityTime;
            } 
        }
        private void ApplyKnockback(int knockback, Vector2 knockbackSource)
        {
            this.Velocity = new Vector2(1.0f, 0.5f) * new Vector2(Math.Sign(Position.X + Origin.X - knockbackSource.X) * knockback);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="npcID"></param>
        /// <returns>A new npc instance with the specified id</returns>
        public static NPC InstantiateNPCByID(int npcID)
        {
            return CloneNPC(_npcs[npcID]);
        }
        private static NPC CloneNPC(NPC npc)
        {
            return new NPC(npc.ID, npc.Name, npc.Image, npc.Size, npc._health, npc.Damage, npc.CollidesWithTiles, npc._behavior.Clone(), npc.DrawBehindTiles, npc.Friendly, npc._animationFrames, npc.Layer, npc.CollidesWith);
        }
        private static Dictionary<int, NPC> _npcs = new Dictionary<int, NPC>
        {
            {0, new NPC(0, "Mutant Cricket", ContentLoader.EnemyTextures[0], new Vector2(69, 34), 100, 10, true, new MutantCricketBehavior(), animationFrames: new List<(int, int)> { (0, 3), (4, 4)})}
        };
        public override string GetTooltipDisplay()
        {
            return $"{Name} {_health} / {_maxHealth}";
        }
    }
}
