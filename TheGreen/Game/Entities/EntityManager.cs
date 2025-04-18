using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TheGreen.Game.Entities.NPCs;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.Tiles;
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
        public Entity MouseEntity;

        public void Update(double delta)
        {
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
                if (entity.Velocity.X * (float)delta >= TheGreen.TILESIZE || entity.Velocity.Y * (float)delta >= TheGreen.TILESIZE)
                {
                    entity.Velocity = Vector2.Zero;
                    continue;
                }
                
                //update enemies positions that don't collide with tiles
                if (!entity.CollidesWithTiles)
                {
                    entity.Position += entity.Velocity * (float)delta;
                    continue;
                }

                entity.Position.X += entity.Velocity.X * (float)delta;

                Rectangle entityBounds = entity.GetBounds();
                int startX = entityBounds.Left / TheGreen.TILESIZE;
                int endX = entityBounds.Right / TheGreen.TILESIZE;
                int startY = entityBounds.Top / TheGreen.TILESIZE;
                int endY = entityBounds.Bottom / TheGreen.TILESIZE;
                int horizontalCollisionDirection = 0;
                
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x, y), TileProperty.Solid))
                        {
                            continue;
                        }
                        Rectangle collision = new Rectangle(x * TheGreen.TILESIZE, y * TheGreen.TILESIZE, TheGreen.TILESIZE, TheGreen.TILESIZE);
                        if (entity.GetBounds().Intersects(collision))
                        {
                            Vector2 penetrationDistance = GetPenetrationDepth(entity.GetBounds(), collision);
                            entity.Position.X += penetrationDistance.X;
                            horizontalCollisionDirection = -Math.Sign(penetrationDistance.X);
                        }
                    }
                }

                bool floorCollision = false;
                bool ceilingCollision = false;
                if ((int)(entity.Position.Y + entity.Velocity.Y * (float)delta) == (int)entity.Position.Y)
                {
                    floorCollision = entity.IsOnFloor;
                    ceilingCollision = entity.IsOnCeiling;
                }

                entity.Position.Y += entity.Velocity.Y * (float)delta;
                
                entityBounds = entity.GetBounds();
                startX = entityBounds.Left / TheGreen.TILESIZE;
                endX = entityBounds.Right / TheGreen.TILESIZE;
                startY = entityBounds.Top / TheGreen.TILESIZE;
                endY = entityBounds.Bottom / TheGreen.TILESIZE;
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        if (!TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x, y), TileProperty.Solid))
                        {
                            continue;
                        }
                        Rectangle collision = new Rectangle(x * TheGreen.TILESIZE, y * TheGreen.TILESIZE, TheGreen.TILESIZE, TheGreen.TILESIZE);
                        if (entity.GetBounds().Intersects(collision))
                        {
                            if (collision.Y > entity.Position.Y)
                            {
                                floorCollision = true;
                            }
                            else if (collision.Y < entity.Position.Y && entity.Velocity.Y < 0.0f)
                            {
                                ceilingCollision = true;
                            }
                            Vector2 penetrationDistance = GetPenetrationDepth(entity.GetBounds(), collision);
                            entity.Position.Y += penetrationDistance.Y;
                            
                            if (penetrationDistance.Y != 0)
                            {
                                entity.Velocity.Y = 0;
                            }
                            
                        }
                    }
                }
                entity.IsOnFloor = floorCollision;
                entity.IsOnCeiling = ceilingCollision;

                if (horizontalCollisionDirection != 0)
                {
                    if (Math.Sign(entity.Velocity.X) == horizontalCollisionDirection && CanEntityHop(entity, (entity.Position / TheGreen.TILESIZE).ToPoint(), entityBounds.Width / TheGreen.TILESIZE, entityBounds.Height / TheGreen.TILESIZE, horizontalCollisionDirection))
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
            //TODO: check here if the mouse is over an entity, and if so, save the entity
            //MouseEntity = null
            for (int i = 0; i < _entities.Count; i++)
            {
                for (int j = i + 1; j < _entities.Count; j++)
                {
                    //if (_entities[i].Contains(InputManager.GetMouseWorldCoordinates()))
                    //  MouseEntity = _entities[i]
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

        public void Draw(SpriteBatch spriteBatch)
        {
            //TODO: add drawing order using SpriteSortMode based on collision layer
            foreach (Entity entity in _entities)
            {
                entity.Draw(spriteBatch);
            }
        }

        Vector2 GetPenetrationDepth(Rectangle a, Rectangle b)
        {
            float dx = (a.Center.X - b.Center.X);
            float dy = (a.Center.Y - b.Center.Y);

            float px = (a.Width / 2f + b.Width / 2f) - Math.Abs(dx);
            float py = (a.Height / 2f + b.Height / 2f) - Math.Abs(dy);

            if (px <= 0 || py <= 0)
                return Vector2.Zero; // No collision

            // Determine the smallest axis and push in that direction
            if (px < py)
                return new Vector2(dx < 0 ? -px : px, 0); // Push left or right
            else
                return new Vector2(0, dy < 0 ? -py : py); // Push up or down
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
