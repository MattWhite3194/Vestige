using Microsoft.Xna.Framework;
using System;

namespace Vestige.Game.Entities.NPCs.Behaviors
{
    /// <summary>
    /// Defined movement pattern for a mutant cricket
    /// </summary>
    public class MutantCricketBehavior : INPCBehavior
    {
        private int _directionX = 0;
        private double _elapsedTime = 0.0f;
        private double _nextJumpTime = 2.0;
        private float _maxSpeed = 50;
        private float _acceleration = 500;
        private bool _jumping = false;
        private int _lockedDirection;
        public void AI(double delta, NPC enemy)
        {
            Vector2 newVelocity = enemy.Velocity;
            newVelocity.Y += Vestige.GRAVITY * (float)delta;
            _elapsedTime += delta;

            Player target = Main.EntityManager.GetPlayerTarget();
            if (target != null)
            {
                if (Vector2.Distance(target.Position, enemy.Position) > 1600)
                {
                    enemy.Active = false;
                    return;
                }
                else
                {
                    _directionX = -MathF.Sign(enemy.Position.X + enemy.Origin.X - (target.Position.X + target.Origin.X));
                    if (target.Position.Y + target.Origin.Y > enemy.Position.Y + enemy.Origin.Y)
                    {
                        enemy.CollidesWithPlatforms = false;
                    }
                }
            }

            enemy.Animation.SetCurrentAnimation(0);
            enemy.Animation.SetAnimationSpeed(Math.Abs(newVelocity.X) / 10);

            if (enemy.IsOnFloor)
            {
                _jumping = false;
                _maxSpeed = 100;
            }
            else
            {
                enemy.Animation.SetCurrentAnimation(1);
            }

            if (_directionX != MathF.Sign(enemy.Velocity.X) && _jumping)
                newVelocity.X += _lockedDirection * (_acceleration * 2.0f) * (float)delta;
            else
                newVelocity.X += _acceleration * _directionX * (float)delta;
            if (MathF.Abs(enemy.Velocity.X) > _maxSpeed)
                newVelocity.X = _maxSpeed * Math.Sign(newVelocity.X);
            if (_elapsedTime >= _nextJumpTime)
            {
                _nextJumpTime = (Main.Random.NextDouble() * 5.0) + 2.0;
                _elapsedTime = 0.0;
                newVelocity.Y = _maxSpeed * Main.Random.Next(-4, -3);
                newVelocity.X = _maxSpeed * _directionX * Main.Random.Next(2, 5);
                _jumping = true;
                _lockedDirection = _directionX;
                _maxSpeed = Math.Abs(newVelocity.X);
            }

            if (newVelocity.X > 0)
                enemy.FlipSprite = true;
            if (newVelocity.X < 0)
                enemy.FlipSprite = false;

            enemy.Velocity = newVelocity;
        }

        public INPCBehavior Clone()
        {
            return new MutantCricketBehavior();
        }
    }
}
