using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TheGreen.Game.Entities.Enemies.EnemyBehaviors;

namespace TheGreen.Game.Entities.Enemies
{
    public class Enemy : Entity
    {
        public int ID;
        public string Name;
        private IEnemyBehavior _behavior;
        private int health;
        public Enemy(int id, string name, Texture2D image, Vector2 size, bool collidesWithTiles, IEnemyBehavior behavior, List<(int, int)> animationFrames = null) : base(image, default, size: size, animationFrames: animationFrames)
        {
            ID = id;
            Name = name;
            CollidesWithTiles = collidesWithTiles;
            _behavior = behavior;

        }
        public override void Update(double delta)
        {
            _behavior?.AI(delta, this);
            base.Update(delta);
        }
    }
}
