using Vestige.Game.Entities;

namespace Vestige.Game.Tiles.TileData
{
    internal interface ICollideableTile
    {
        void OnCollision(int x, int y, Entity entity);
    }
}
