namespace Vestige.Game.Entities
{
    internal interface IRespawnable
    {
        internal float RespawnTime { get; set; }
        internal void Respawn();
    }
}
