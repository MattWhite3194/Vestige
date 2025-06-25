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
            List<(int, int)> animationFrames = null)
            : base(image, default, size: size, animationFrames: animationFrames, name: name)
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
            if (entity is ItemCollider itemCollider)
            {
                ApplyDamage(((WeaponItem)itemCollider.Item).Damage);
                ApplyKnockback(((WeaponItem)itemCollider.Item).Knockback, entity.Position + entity.Origin);
            }
            else if (entity is NPC npc)
            {
                ApplyDamage(((NPC)entity).Damage);
                ApplyKnockback(((NPC)entity).Knockback, entity.Position + entity.Origin);
            }
            else if (entity is Projectile projectile)
            {
                ApplyDamage(((Projectile)entity).Damage);
                ApplyKnockback(((Projectile)entity).Knockback, entity.Position + entity.Origin);
            }
        }
        private void ApplyDamage(int damage)
        {
            _health -= damage;
            _invincible = true;
            if (_health <= 0)
                Active = false;
            if (Active)
            {
                _invincibilityTimeLeft = _maxInvincibilityTime;
            }
        }
        private void ApplyKnockback(int knockback, Vector2 knockbackSource)
        {
            Velocity = new Vector2(1.0f, 0.5f) * new Vector2(Math.Sign(Position.X + Origin.X - knockbackSource.X) * knockback, 1) * 100.0f;
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
            return new NPC(npc.ID, npc.Name, npc.Image, npc.Size, npc._health, npc.Damage, npc.CollidesWithTiles, npc._behavior.Clone(), npc.DrawBehindTiles, npc.Friendly, npc._animationFrames);
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
