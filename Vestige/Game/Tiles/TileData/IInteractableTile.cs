using Vestige.Game.Entities;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    internal interface IInteractableTile
    {
        void OnRightClick(WorldGen world, Player player, int x, int y);
    }
}
