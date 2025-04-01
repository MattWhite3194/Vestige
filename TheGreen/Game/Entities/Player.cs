using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheGreen.Game.Input;
using TheGreen.Game.Inventory;

namespace TheGreen.Game.Entities
{
    public class Player : Entity, IInputHandler
    {
        private float _acceleration = 500;
        public Vector2 Direction = Vector2.Zero;
        private int _maxSpeed = 200;
        private int _maxFallSpeed = 900;
        private int _jumpVelocity = -600;
        private InventoryManager _inventory;
        private int _health;
        private float _fallDistance = 0;
        private bool _queueJump = false;
        public ItemCollider ItemCollider;
        public Player(Texture2D image, Vector2 position, InventoryManager inventory, int health) : base(image, position, size: new Vector2(20, 42), animationFrames: new List<(int, int)> { (0, 0), (1, 8), (9, 9), (10, 10) })
        {
            this._inventory = inventory;
            CollidesWithTiles = true;
            _health = health;
            this.Layer = CollisionLayer.Player;
            this.CollidesWith = CollisionLayer.Enemy | CollisionLayer.HostileProjectile | CollisionLayer.ItemDrop;
        }

        public void InitializeGameUpdates()
        {
            ItemCollider = new ItemCollider(_inventory);
            Main.EntityManager.AddEntity(ItemCollider);
        }

        public void HandleInput(InputEvent @event)
        {
            if (@event.InputButton == InputButton.Left)
            {
                if (@event.EventType == InputEventType.KeyDown)
                    Direction.X -= 1;
                else
                    Direction.X += 1;
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.Right)
            {
                if (@event.EventType == InputEventType.KeyDown)
                    Direction.X += 1;
                else
                    Direction.X -= 1;
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.Jump)
            {
                _queueJump = @event.EventType == InputEventType.KeyDown;
                InputManager.MarkInputAsHandled(@event);
            }
            else
            {
                ItemCollider.HandleInput(@event);
            }
        }

        public override void Update(double delta)
        {
            base.Update(delta);
            Vector2 newVelocity = Velocity;

            //Slow down player if they stopped moving
            if (Direction.X == 0.0f)
            {
                if (MathF.Abs(newVelocity.X) < 20)
                {
                    newVelocity.X = 0;
                    Animation.SetCurrentAnimation(0);
                }
                else
                    newVelocity.X -= MathF.Sign(newVelocity.X) * (_acceleration * 2.0f) * (float)delta;             
            }
            else
            {
                if ((int)Direction.X != MathF.Sign(Velocity.X))
                    newVelocity.X += Direction.X * (_acceleration * 2.0f) * (float)delta;
                else
                    newVelocity.X += Direction.X * _acceleration * (float)delta;
                if (MathF.Abs(newVelocity.X) > _maxSpeed)
                    newVelocity.X = Direction.X * _maxSpeed;
                Animation.SetCurrentAnimation(1);
                Animation.SetAnimationSpeed(Math.Abs(Velocity.X / 10));
            }

            //add gravity
            newVelocity.Y += Globals.GRAVITY * (float)delta;
            if (newVelocity.Y > _maxFallSpeed)
                newVelocity.Y = _maxFallSpeed;

            //calculate fall damage and jump
            if (IsOnFloor)
            {
                Direction.Y = 0;
                //if the player falls for 10 tiles or more, take damage
                if (_fallDistance / Globals.TILESIZE > 10)
                {
                    TakeDamage((((int)(_fallDistance / Globals.TILESIZE) - 10) * 2));
                    
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
            
            Velocity = newVelocity;
        }

        public override void OnCollision(Entity entity)
        {
            switch (entity.Layer)
            {
                case CollisionLayer.ItemDrop:
                    ItemDrop itemDrop = (ItemDrop)entity;
                    if (_inventory.AddItem(itemDrop.GetItem()) <= 0)
                    {
                        Main.EntityManager.RemoveEntity(itemDrop);
                    }
                    break;
            }
        }
        public void TakeDamage(int damage)
        {
            _health -= damage;
            Debug.WriteLine("Player Damaged!\nDamage: " + damage);
        }
    }
}
