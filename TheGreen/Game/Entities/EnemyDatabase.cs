using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace TheGreen.Game.Entities
{
    public static class EnemyDatabase
    {

        private static Dictionary<int, object[]> _enemies = new Dictionary<int, object[]> 
        {
            {0, [0, "Mutant Cricket", ContentLoader.EnemyTextures[0], new Vector2(69, 34), true, new MutantCricketBehavior(), new List<(int, int)> { (0, 3), (4, 4)}]}
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enemyID"></param>
        /// <returns>A new enemy instance with the specified id</returns>
        public static Enemy InstantiateEnemyByID(int enemyID)
        {
            //little bit of code smell (¬_¬)
            Enemy enemy = (Enemy)Activator.CreateInstance(typeof(Enemy), _enemies[enemyID]);
            return enemy;
        }
    }
}
