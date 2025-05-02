using TheGreen.Game.Entities;

namespace TheGreen.Game.Tiles.TileData
{
    internal interface ICollideableTile
    {
        void OnCollision(int x, int y, Entity entity);
    }
}
