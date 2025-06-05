using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Vestige.Game.Drawables;

namespace Vestige.Game.Entities
{
    /// <summary>
    /// Defines a basic entity, something that moves and collides in the world
    /// </summary>
    public abstract class Entity : Sprite
    {
        public Vector2 Velocity;
        public bool IsOnFloor = false, IsOnCeiling = false;
        public bool DrawBehindTiles = false;
        /// <summary>
        /// The entity will stop when it collides with a tile
        /// </summary>
        public bool CollidesWithTiles;
        public bool CollidesWithPlatforms;
        /// <summary>
        /// If true, the entity will not stop when colliding with a single-tile tall barrier. Instead it will move over it.
        /// </summary>
        public bool HopTiles;
        /// <summary>
        /// When set to false, the entity will be removed in the next update
        /// </summary>
        public bool Active = true;
        /// <summary>
        /// The layer other entities receive when they collide with this entity
        /// </summary>
        public CollisionLayer Layer;
        /// <summary>
        /// The collision layers this entity receives collision events from
        /// </summary>
        public CollisionLayer CollidesWith;
        /// <summary>
        /// The layer this entity draws on
        /// </summary>
        public readonly int DrawLayer;
        public readonly string Name;
        protected Entity(Texture2D image, Vector2 position, Vector2 size = default, Vector2 origin = default, List<(int, int)> animationFrames = null, int drawLayer = 0, string name = null) : base(image, position, size, origin: origin, animationFrames: animationFrames)
        {
            this.DrawLayer = drawLayer;
            Name = name != null ? name : "";
         }

        public virtual void OnCollision(Entity entity)
        {

        }
        public virtual void OnTileCollision(int x, int y, ushort tileID)
        {
            //TODO: call this method
        }
        public virtual CollisionRectangle GetBounds()
        {
            return new CollisionRectangle(Position + Origin - Size / 2.0f, Size.ToPoint());
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Point centerTilePosition = ((Position + Size / 2) / Vestige.TILESIZE).ToPoint();
            spriteBatch.Draw(Image,
                Vector2.Round(Position + Origin),
                Animation?.AnimationRectangle ?? null,
                Main.LightEngine.GetLight(centerTilePosition.X, centerTilePosition.Y),
                Rotation,
                Origin,
                Scale,
                FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0.0f
            );
        }
        /// <summary>
        /// Fired after all collisions are resolved
        /// </summary>
        /// <param name="delta"></param>
        public virtual void PostCollisionUpdate(double delta) { }
    }
}
