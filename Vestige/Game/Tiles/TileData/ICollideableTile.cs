using Vestige.Game.Entities;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    internal interface ICollideableTile
    {
        void OnCollision(WorldGen world, int x, int y, Entity entity);
    }
}
