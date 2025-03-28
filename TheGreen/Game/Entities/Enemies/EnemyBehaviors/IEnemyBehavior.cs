
namespace TheGreen.Game.Entities.Enemies.EnemyBehaviors
{
    /// <summary>
    /// Interface defining the methods required for an enemy movement pattern
    /// </summary>
    public interface IEnemyBehavior
    {
        void AI(double delta, Enemy enemy);
    }
}
