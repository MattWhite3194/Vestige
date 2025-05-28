using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TheGreen.Game.Entities.NPCs;
using TheGreen.Game.Entities.Projectiles;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.Tiles;
using TheGreen.Game.Tiles.TileData;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Entities
{
    /// <summary>
    /// Handles collision detection between entities
    /// </summary>
    public class EntityManager
    {
        private Player _player;
        private List<Entity> _entities = new List<Entity>();
        private List<NPC> _npcs = new List<NPC>();
        private List<Projectile> _projectiles = new List<Projectile>();
        private List<ItemDrop> _itemDrops = new List<ItemDrop>();
        public Entity MouseEntity;
        public bool MouseCollidingWithEntityTile;

        public void Update(double delta)
        {
            MouseEntity = null;
            MouseCollidingWithEntityTile = false;
            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i].GetBounds().Contains(InputManager.GetMouseWorldPosition()))
                    MouseEntity = _entities[i];
                //check if mouse tile is colliding with an entity
                if (_entities[i].Layer != CollisionLayer.Enemy && _entities[i].Layer != CollisionLayer.Player)
                    continue;
                //add 1 pixel padding on every side since .Contains does not include edges
                Point topLeft = (Vector2.Floor(_entities[i].Position / TheGreen.TILESIZE) * TheGreen.TILESIZE).ToPoint();
                Point bottomRight = (Vector2.Ceiling((_entities[i].Position - new Vector2(1, 1) + _entities[i].Size) / TheGreen.TILESIZE) * TheGreen.TILESIZE).ToPoint() + new Point(1, 1);
                Rectangle entityTileBounds = new Rectangle(
                    topLeft, bottomRight - topLeft
                    );
                if (entityTileBounds.Contains(InputManager.GetMouseWorldPosition()))
                {
                    MouseCollidingWithEntityTile = true;
                }
            }
            //Handle tile collisions
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                Entity entity = _entities[i];
                if (!entity.Active)
                {
                    _entities.Remove(entity);
                    continue;
                }
                //Update Entities
                entity.Update(delta);

                Vector2 minPos = Vector2.Zero;
                Vector2 maxPos = new(WorldGen.World.WorldSize.X * TheGreen.TILESIZE - entity.Size.X, WorldGen.World.WorldSize.Y * TheGreen.TILESIZE - entity.Size.Y);
                //update enemies positions that don't collide with tiles
                if (!entity.CollidesWithTiles)
                {
                    entity.Position += entity.Velocity * (float)delta;
                    entity.Position = Vector2.Clamp(entity.Position, minPos, maxPos);
                    continue;
                }

                //horizontal tile collisions
                int distanceFactor = (int)Math.Min(entity.Size.X, TheGreen.TILESIZE);
                int tileWidth = (int)entity.Size.X / TheGreen.TILESIZE;
                int tileHeight = (int)entity.Size.Y / TheGreen.TILESIZE;
                float distanceX = entity.Velocity.X * (float)delta;
                int horizontalCollisionDirection = 0;
                List<float> horizontalDistances = new List<float>();
                for (int _ = 0; _ < Math.Floor(distanceX / (distanceFactor - 1)); _++)
                {
                    horizontalDistances.Add(distanceFactor - 1);
                }
                horizontalDistances.Add(distanceX % (distanceFactor - 1));
                foreach (float distance in horizontalDistances)
                {
                    horizontalCollisionDirection = HorizontalCollisionPass(entity, distance, minPos, maxPos);
                    if (horizontalCollisionDirection != 0)
                    {
                        break;
                    }
                }

                //vertical tile collisions
                distanceFactor = (int)Math.Min(entity.Size.Y, TheGreen.TILESIZE);
                float distanceY = entity.Velocity.Y * (float)delta;
                List<float> verticalDistances = new List<float>();
                for (int _ = 0; _ < Math.Floor(distanceY / distanceFactor); _++)
                {
                    verticalDistances.Add(distanceFactor);
                }
                verticalDistances.Add(distanceY % distanceFactor);
                foreach (float distance in verticalDistances)
                {
                    if (VerticalCollisionPass(entity, distance, minPos, maxPos))
                    {
                        break;
                    }
                }

                //Determine if an entity who has horizontally collided with a tile can hop up
                if (horizontalCollisionDirection != 0)
                {
                    if (Math.Sign(entity.Velocity.X) == horizontalCollisionDirection && CanEntityHop(entity, (entity.Position / TheGreen.TILESIZE).ToPoint(), entity.GetBounds().Width / TheGreen.TILESIZE, entity.GetBounds().Height / TheGreen.TILESIZE, horizontalCollisionDirection))
                    {
                        entity.Position.Y -= TheGreen.TILESIZE;
                        entity.Position.X += 2 * Math.Sign(entity.Velocity.X);
                    }
                    else
                    {
                        entity.Velocity.X = 0.0f;
                    }
                }
            }
            //check collisions between entities
            for (int i = 0; i < _entities.Count; i++)
            {
                for (int j = i + 1; j < _entities.Count; j++)
                {
                    if ((_entities[i].CollidesWith & _entities[j].Layer) == 0 && (_entities[j].CollidesWith & _entities[i].Layer) == 0)
                        continue;
                    if (!_entities[i].GetBounds().Intersects(_entities[j].GetBounds()))
                        continue;
                    if ((_entities[j].CollidesWith & _entities[i].Layer) != 0)
                        _entities[j].OnCollision(_entities[i]);
                    if ((_entities[i].CollidesWith & _entities[j].Layer) != 0)
                        _entities[i].OnCollision(_entities[j]);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name=""></param>
        /// <returns>The direction of the horizontal collision, 0 if there is none</returns>
        private int HorizontalCollisionPass(Entity entity, float distance, Vector2 minPos, Vector2 maxPos)
        {
            entity.Position.X += distance;
            entity.Position.X = float.Clamp(entity.Position.X, minPos.X, maxPos.X);

            CollisionRectangle entityBounds = entity.GetBounds();
            int startX = (int)entityBounds.Left / TheGreen.TILESIZE;
            int endX = (int)Math.Ceiling(entityBounds.Right / TheGreen.TILESIZE);
            int startY = (int)entityBounds.Top / TheGreen.TILESIZE;
            int endY = (int)Math.Ceiling(entityBounds.Bottom / TheGreen.TILESIZE);
            int horizontalCollisionDirection = 0;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    CollisionRectangle tileCollider = new CollisionRectangle(x * TheGreen.TILESIZE, y * TheGreen.TILESIZE, TheGreen.TILESIZE, TheGreen.TILESIZE);
                    //IMPORTANT: entity bounds will not intersect a tile or other collision if the position update is less than a pixels width, since bounds are calculated using integers. Players Velocity will get up to 30 before the player actually moves enough to detect a collision.
                    if (entity.GetBounds().Intersects(tileCollider))
                    {
                        if (TileDatabase.GetTileData(WorldGen.World.GetTileID(x, y)) is ICollideableTile collideableTile)
                            collideableTile.OnCollision(x, y, entity);
                        if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x, y), TileProperty.Solid))
                        {
                            continue;
                        }
                        float penetrationDistance = GetPenetrationX(entity.GetBounds(), tileCollider);
                        if (penetrationDistance < 0)
                        {
                            entity.Position.X = tileCollider.Left - entity.Size.X;
                            horizontalCollisionDirection = 1;
                        }
                        else if (penetrationDistance > 0)
                        {
                            entity.Position.X = tileCollider.Right;
                            horizontalCollisionDirection = -1;
                        }
                    }
                }
            }
            return horizontalCollisionDirection;
        }

        private bool VerticalCollisionPass(Entity entity, float distance, Vector2 minPos, Vector2 maxPos)
        {
            bool floorCollision = false;
            bool ceilingCollision = false;
            bool collisionDetected = false;
            if ((int)(entity.Position.Y + distance) == (int)entity.Position.Y)
            {
                floorCollision = entity.IsOnFloor;
                ceilingCollision = entity.IsOnCeiling;
            }

            entity.Position.Y += distance;

            CollisionRectangle entityBounds = entity.GetBounds();
            int startX = (int)entityBounds.Left / TheGreen.TILESIZE;
            int endX = (int)Math.Ceiling(entityBounds.Right / TheGreen.TILESIZE);
            int startY = (int)entityBounds.Top / TheGreen.TILESIZE;
            int endY = (int)Math.Ceiling(entityBounds.Bottom / TheGreen.TILESIZE);
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    CollisionRectangle tileCollider = new CollisionRectangle(x * TheGreen.TILESIZE, y * TheGreen.TILESIZE, TheGreen.TILESIZE, TheGreen.TILESIZE);
                    if (entity.GetBounds().Intersects(tileCollider))
                    {
                        if (TileDatabase.GetTileData(WorldGen.World.GetTileID(x, y)) is ICollideableTile collideableTile)
                            collideableTile.OnCollision(x, y, entity);
                        if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x, y), TileProperty.Solid))
                        {
                            continue;
                        }
                        collisionDetected = true;
                        if (tileCollider.Y > entity.Position.Y)
                        {
                            floorCollision = true;
                        }
                        else if (tileCollider.Y < entity.Position.Y && entity.Velocity.Y < 0.0f)
                        {
                            ceilingCollision = true;
                        }
                        float penetrationDistance = GetPenetrationY(entity.GetBounds(), tileCollider);
                        entity.Position.Y += penetrationDistance;
                        entity.Position.Y = (int)entity.Position.Y;

                        if (penetrationDistance != 0)
                        {
                            entity.Velocity.Y = 0;
                        }

                    }
                }
            }
            entity.IsOnFloor = floorCollision;
            entity.IsOnCeiling = ceilingCollision;
            return collisionDetected;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //TODO: add drawing order using SpriteSortMode based on collision layer
            foreach (Entity entity in _entities)
            {
                entity.Draw(spriteBatch);
            }

        }

        float GetPenetrationX(CollisionRectangle a, CollisionRectangle b)
        {
            float distanceX = (a.Center.X - b.Center.X);
            float penetrationX = (a.Width / 2f + b.Width / 2f) - Math.Abs(distanceX);
            if (penetrationX <= 0)
                return 0.0f;
            return distanceX < 0 ? -penetrationX : penetrationX;
        }
        float GetPenetrationY(CollisionRectangle a, CollisionRectangle b)
        {
            float distanceY = (a.Center.Y - b.Center.Y);
            float penetrationY = (a.Height / 2f + b.Height / 2f) - Math.Abs(distanceY);
            if (penetrationY <= 0)
                return 0.0f;
            return distanceY < 0 ? -penetrationY : penetrationY;
        }

        /// <summary>
        /// Sets the player for the game world, called only once
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer(Player player)
        {
            _player = player;
        }

        public Player GetPlayer()
        {
            return _player;
        }

        //Change this to spawn by enemy ID
        public void CreateEnemy(int enemyID, Vector2 Position)
        {
            NPC enemy = NPCDatabase.InstantiateNPCByID(enemyID);
            enemy.Position = Position;
            _entities.Add(enemy);
        }

        public void AddItemDrop(Item item, Vector2 position, Vector2 velocity = default)
        {

            ItemDrop itemDrop = new ItemDrop(item, position + new Vector2(TheGreen.TILESIZE / 2 - ItemDrop.ColliderSize.X / 2, 0));
            itemDrop.Velocity = velocity == default ? Vector2.Zero : velocity;
            _entities.Add(itemDrop);
        }
        public void AddEntity(Entity entity)
        {
            _entities.Add(entity);
        }
        public void RemoveEntity(Entity entity)
        {
            _entities.Remove(entity);
        }
        public List<Entity> GetEntities()
        {
            return _entities;
        }
        private bool CanEntityHop(Entity entity, Point tilePoint, int tileWidth, int tileHeight, int direction)
        {
            if (!entity.IsOnFloor || entity.Velocity.X == 0)
                return false;
            for (int x = 0; x <= tileWidth; x++)
            {
                if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(tilePoint.X + x, tilePoint.Y - 1), TileProperty.Solid))
                    return false;
            }
            int tilesInFrontOffset = direction == -1 ? -1 : tileWidth + 1;
            for (int y = -1; y < tileHeight; y++)
            {
                if (TileDatabase.TileHasProperty(WorldGen.World.GetTileID(tilePoint.X + tilesInFrontOffset, tilePoint.Y + y), TileProperty.Solid))
                    return false;
            }
            return true;
        }
    }
}
