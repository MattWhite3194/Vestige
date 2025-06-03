namespace Vestige.Game.Entities.Projectiles.ProjectileBehaviors
{
    public interface IProjectileBehavior
    {
        void AI(double delta, Projectile projectile);

        void OnDeath(Projectile projectile);

        void OnTileCollision(Projectile projectile);

        void OnCollision(Projectile projectile, Entity entity);

        IProjectileBehavior Clone();
    }
}
