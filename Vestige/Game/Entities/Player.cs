using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vestige.Game.Entities.NPCs;
using Vestige.Game.Entities.Projectiles;
using Vestige.Game.Input;
using Vestige.Game.Inventory;
using Vestige.Game.Tiles;
using Vestige.Game.Tiles.TileData;

namespace Vestige.Game.Entities
{
    public class Player : Entity, IInputHandler, IRespawnable
    {
        private float _acceleration = 350;
        public Vector2 Direction = Vector2.Zero;
        private int _maxSpeed = 150;
        private int _maxFallSpeed = 900;
        private int _jumpVelocity = -450;
        public InventoryManager Inventory;
        private int _health;
        private float _fallDistance = 0;
        private bool _queueJump = false;
        public ItemCollider ItemCollider;
        private float _invincibilityTimeLeft = -1f;
        private float _maxInvincibilityTime = 0.5f;
        private float _respawnTimeLeft;
        private float _maxRespawnTime = 8f;
        float IRespawnable.RespawnTime
        {
            get
            {
                return _respawnTimeLeft;
            }
            set
            {
                _respawnTimeLeft = value;
            }
        }
        private bool _invincible = false;
        private bool _dead = false;
        private HashSet<InputButton> _activeInputs = new HashSet<InputButton>();
        private Texture2D _headTexture;
        private Texture2D _torsoTexture;
        private Texture2D _legsTexture;
        private Texture2D _armTexture;
        public int MaxPlaceDistance = Vestige.TILESIZE * 7;
        public int MaxBreakDistance = Vestige.TILESIZE * 7;
        public bool Dead { get { return _dead; } }

        public Player(InventoryManager inventory) : base(null, default, size: new Vector2(20, 42), animationFrames: new List<(int, int)> { (0, 0), (1, 8), (9, 9), (10, 10) }, drawLayer: 2, name: "Player")
        {
            _headTexture = ContentLoader.PlayerHead;
            _torsoTexture = ContentLoader.PlayerTorso;
            _legsTexture = ContentLoader.PlayerLegs;
            _armTexture = ContentLoader.PlayerArm;
            this.Inventory = inventory;
            CollidesWithTiles = true;
            CollidesWithPlatforms = true;
            _health = 100;
            this.Layer = CollisionLayer.Player;
            this.CollidesWith = CollisionLayer.Enemy | CollisionLayer.ItemDrop | CollisionLayer.HostileProjectile;
        }

        /// <summary>
        /// Called once when the player is initially added to the world
        /// </summary>
        public void InitializeGameUpdates(Point tilePosition)
        {
            ItemCollider = new ItemCollider(Inventory);
            Position = tilePosition.ToVector2() * Vestige.TILESIZE - new Vector2(0, Size.Y);
            Main.EntityManager.AddEntity(this);
            Main.EntityManager.AddEntity(ItemCollider);
        }

        public void HandleInput(InputEvent @event)
        {
            if (!Active) return;

            if (@event.InputButton == InputButton.Left)
            {
                if (@event.EventType == InputEventType.KeyDown)
                    _activeInputs.Add(InputButton.Left);
                else
                    _activeInputs.Remove(InputButton.Left);
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.Right)
            {
                if (@event.EventType == InputEventType.KeyDown)
                    _activeInputs.Add(InputButton.Right);
                else
                    _activeInputs.Remove(InputButton.Right);
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.Down)
            {
                if (@event.EventType == InputEventType.KeyDown)
                    CollidesWithPlatforms = false;
            }
            else if (@event.InputButton == InputButton.Jump)
            {
                _queueJump = @event.EventType == InputEventType.KeyDown;
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.RightMouse && @event.EventType == InputEventType.MouseButtonDown)
            {
                Point mouseTilePosition = Main.GetMouseWorldPosition() / new Point(Vestige.TILESIZE);
                if (TileDatabase.GetTileData(Main.World.GetTileID(mouseTilePosition.X, mouseTilePosition.Y)) is IInteractableTile interactableTile)
                {
                    interactableTile.OnRightClick(Main.World, mouseTilePosition.X, mouseTilePosition.Y);
                }
            }
            else
            {
                ItemCollider.HandleInput(@event);
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
            Vector2 newVelocity = Velocity;
            Direction.X = 0;
            if (_activeInputs.Contains(InputButton.Left)) Direction.X -= 1;
            if (_activeInputs.Contains(InputButton.Right)) Direction.X += 1;
            Animation.SetCurrentAnimation(1);
            Animation.SetAnimationSpeed(Math.Abs(Velocity.X / 10));
            if (MathF.Abs(newVelocity.X) == 0.0f)
            {
                Animation.SetCurrentAnimation(0);
            }
            if (Direction.X == 0.0f)
            {
                newVelocity.X -= MathF.Sign(newVelocity.X) * (_acceleration * 2.0f) * (float)delta;
                if (MathF.Sign(newVelocity.X) != 0 && MathF.Sign(newVelocity.X) != MathF.Sign(Velocity.X))
                    newVelocity.X = 0;
            }
            else
            {
                if ((int)Direction.X != MathF.Sign(Velocity.X))
                    newVelocity.X += Direction.X * (_acceleration * 2.0f) * (float)delta;
                else
                    newVelocity.X += Direction.X * _acceleration * (float)delta;
                if (MathF.Abs(newVelocity.X) > _maxSpeed)
                    newVelocity.X = Math.Sign(newVelocity.X) * _maxSpeed;
            }

            //add gravity
            newVelocity.Y += Vestige.GRAVITY * (float)delta;
            if (newVelocity.Y > _maxFallSpeed)
                newVelocity.Y = _maxFallSpeed;

            //calculate fall damage and jump
            if (IsOnFloor)
            {
                Direction.Y = 0;
                //if the player falls for 10 tiles or more, take damage
                if (_fallDistance / Vestige.TILESIZE > 10)
                {
                    ApplyDamage((((int)(_fallDistance / Vestige.TILESIZE) - 10) * 2));
                    
                }
                _fallDistance = 0;
                if (_queueJump)
                {
                    newVelocity.Y = _jumpVelocity;
                    _queueJump = false;
                }
            }
            else
            {
                if (Velocity.Y > 0)
                {
                    Direction.Y = 1;
                    Animation.SetCurrentAnimation(3);
                    _fallDistance += Velocity.Y * (float)delta;
                }
                else
                {
                    Direction.Y = -1;
                    Animation.SetCurrentAnimation(2);
                }
            }

            if (Direction.X < 0)
                FlipSprite = true;
            else if (Direction.X > 0)
                FlipSprite = false;
            if (ItemCollider.ItemActive && ItemCollider.Item.UseStyle == Items.UseStyle.Point)
            {
                FlipSprite = ItemCollider.ForcePlayerFlip;
            }
            
            Velocity = newVelocity;
            base.Update(delta);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawBodyPart(spriteBatch, _torsoTexture, Animation.AnimationRectangle);
            DrawBodyPart(spriteBatch, _headTexture, Animation.AnimationRectangle);
            DrawBodyPart(spriteBatch, _legsTexture, Animation.AnimationRectangle);
            ItemCollider.DrawItem(spriteBatch);
            float armRotation = Math.Min(ItemCollider.MaxRotation, Math.Max(0, Math.Abs(ItemCollider.Rotation + (FlipSprite ? -MathHelper.PiOver2 : MathHelper.PiOver2)) - MathHelper.PiOver4 / 2));
            int textureOffset = (int)Math.Floor(armRotation / MathHelper.PiOver4) * (int)Size.Y;
            DrawBodyPart(spriteBatch, _armTexture, ItemCollider.ItemActive ? new Rectangle(0, 11 * (int)Size.Y + textureOffset, (int)Size.X, (int)Size.Y) : Animation.AnimationRectangle);
        }
        private void DrawBodyPart(SpriteBatch spriteBatch, Texture2D bodyPart, Rectangle animationRect)
        {
            Point centerTilePosition = ((Position + Size / 2) / Vestige.TILESIZE).ToPoint();
            spriteBatch.Draw(bodyPart,
                Vector2.Round(Position + Origin),
                animationRect,
                Main.LightEngine.GetLight(centerTilePosition.X, centerTilePosition.Y),
                Rotation,
                Origin,
                Scale,
                FlipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0.0f
            );
        }

        public override void OnCollision(Entity entity)
        {
            switch (entity.Layer)
            {
                case CollisionLayer.ItemDrop:
                    ItemDrop itemDrop = (ItemDrop)entity;
                    if (itemDrop.CanBePickedUp && Inventory.AddItemToPlayerInventory(itemDrop.GetItem()) == null)
                    {
                        itemDrop.Active = false;
                    }
                    break;
                case CollisionLayer.Enemy:
                    if (_invincible) return;
                    NPC enemy = (NPC)entity;
                    ApplyDamage(enemy.Damage);
                    ApplyKnockback(enemy.Position + enemy.Origin);
                    break;
                case CollisionLayer.HostileProjectile:
                    if (_invincible) return;
                    Projectile projectile = (Projectile)entity;
                    ApplyDamage(projectile.Damage);
                    ApplyKnockback(projectile.Position + projectile.Origin);
                    break;
            }
        }
        public void ApplyDamage(int damage)
        {
            this._health -= damage;
            if (this._health <= 0)
            {
                Active = false;
                ItemCollider.Active = false;
                _dead = true;
                _respawnTimeLeft = _maxRespawnTime;
                Main.EntityManager.QueueRespawn(this);
                return;
            }
            _invincible = true;
            if (Active)
            {
                _invincibilityTimeLeft = _maxInvincibilityTime;
            }
        }
        private void ApplyKnockback(Vector2 knockbackSource)
        {
            this.Velocity.Y = Math.Min(-300, Velocity.Y);
            this.Velocity.X = Math.Sign((Position.X + Origin.X) - knockbackSource.X) * _maxSpeed;
        }
        void IRespawnable.Respawn()
        {
            Active = true;
            ItemCollider.Active = true;
            _dead = false;
            Position = Main.World.SpawnTile.ToVector2() * Vestige.TILESIZE - new Vector2(0, Size.Y - 1);
            Velocity = Vector2.Zero;
            ClearInputs();
            ItemCollider.ItemActive = false;
            _queueJump = false;
            Main.EntityManager.AddEntity(this);
            Main.EntityManager.AddEntity(ItemCollider);
            _health = 100;
        }
        public void ClearInputs()
        {
            _activeInputs.Clear();
        }
    }
}
