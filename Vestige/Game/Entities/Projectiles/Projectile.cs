using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Vestige.Game.Entities.Projectiles.ProjectileBehaviors;
using Vestige.Game.Tiles;

namespace Vestige.Game.Entities.Projectiles
{
    public class Projectile : Entity
    {
        public readonly int ID;
        private float _timeLeft;
        private IProjectileBehavior _behavior;
        public readonly int Damage;
        public readonly int Knockback;
        public readonly bool Friendly;
        private List<(int, int)> _animationFrames;
        private int _tilePenetration;
        private int _entityPenetration;
        private HashSet<(int, int)> _tileCollisions;
        public Projectile(int id, Texture2D image, Vector2 size, Vector2 origin, int damage, int knockback, float timeLeft, bool friendly, bool collidesWithTiles, int tilePenetration = -1, int entityPenetration = -1, IProjectileBehavior behavior = null, List<(int, int)> animationFrames = null) : base(image, default, size, origin, animationFrames: animationFrames)
        {
            ID = id;
            CollidesWithTiles = collidesWithTiles;
            if (tilePenetration != -1)
                _tileCollisions = new HashSet<(int, int)>();
            Damage = damage;
            Knockback = knockback;
            _timeLeft = timeLeft;
            Friendly = friendly;
            _behavior = behavior;
            _entityPenetration = entityPenetration;
            _tilePenetration = tilePenetration;
            _animationFrames = animationFrames;
        }
        public override void OnCollision(Entity entity)
        {
            if (_entityPenetration == -1)
                return;
            _entityPenetration--;
            _behavior.OnCollision(this, entity);
            if (_entityPenetration <= 0)
            {
                OnDeath();
            }
        }
        public override void OnTileCollision(int x, int y, ushort tileID)
        {
            if (_tilePenetration == -1)
                return;
            _tilePenetration--;
            _behavior.OnTileCollision(this);
            if (_tilePenetration <= 0)
            {
                OnDeath();
            }
        }
        public override void Update(double delta)
        {
            //TODO: get collisions here for tile penetration if collidesWithTiles is false
            if (!CollidesWithTiles && _tilePenetration != -1)
            {
                CollisionRectangle bounds = GetBounds();
                int startX = (int)bounds.Left / Vestige.TILESIZE;
                int endX = (int)Math.Ceiling(bounds.Right / Vestige.TILESIZE);
                int startY = (int)bounds.Top / Vestige.TILESIZE;
                int endY = (int)Math.Ceiling(bounds.Bottom / Vestige.TILESIZE);
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        if (!GetBounds().Intersects(new CollisionRectangle(x * Vestige.TILESIZE, y * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE)))
                            continue;
                        if (TileDatabase.TileHasProperties(Main.World.GetTileID(x, y), TileProperty.Solid) && !_tileCollisions.Contains((x, y)))
                        {
                            _tileCollisions.Add((x, y));
                            OnTileCollision(x, y, Main.World.GetTileID(x, y));
                        }
                    }
                }
            }
            _behavior.AI(delta, this);
            _timeLeft -= (float)delta;
            if (_timeLeft <= 0.0f)
                OnDeath();
            base.Update(delta);
        }
        private void OnDeath()
        {
            if (!Active)
                return;
            Active = false;
            _behavior.OnDeath(this);
        }
        private Projectile CloneProjectile()
        {
            return new Projectile(ID, Image, Size, Origin, Damage, Knockback, _timeLeft, Friendly, CollidesWithTiles, _tilePenetration, _entityPenetration, _behavior.Clone(), _animationFrames);
        }
        public static Projectile InstantiateProjectileByID(int id)
        {
            return _projectiles[id].CloneProjectile();
        }
        private static Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>
        {
            {0, new Projectile(0, ContentLoader.ProjectileTextures[0], new Vector2(4, 4), new Vector2(2.5f, 9), 10, 2, 3.0f, true, false, 1, 1, new Arrow()) },
            {1, new Projectile(0, ContentLoader.ProjectileTextures[1], new Vector2(13, 13), new Vector2(6.5f, 11), 0, 0, 5, true, true, behavior: new Bomb(5)) }
        };
    }
}
