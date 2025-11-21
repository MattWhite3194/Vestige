
namespace Vestige.Game.Entities
{
    internal class EntitySpawner
    {
        private float _entitySpawnRate = 0.006f;

        public void Update(double delta)
        {
            if (Main.Random.NextDouble() < _entitySpawnRate)
            {
                //TrySpawnEnemy();
            }
        }

        private void TrySpawnEnemy(Player player)
        {
            //Select a random entity based on spawnchance
            //Try to spawn the entity off screen, if enemy collides with tiles and no open spot, spawn failed.
            //Spawn 5 tiles off screen
            float x = player.Position.X - (Vestige.DrawDistance.X + 5) * Vestige.TILESIZE;
            //check side to find nearest ground to player, and if the space is large enough to spawn the enemy
        }
    }
}
