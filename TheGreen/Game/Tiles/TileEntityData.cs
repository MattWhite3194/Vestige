using Microsoft.Xna.Framework;

namespace TheGreen.Game.Tiles
{
    internal class TileEntityData : TileData
    {
        public TileEntityData(TileProperty properties, Color color, int itemID = -1) : base(properties, color, itemID, 0)
        {
        }
        public void OnRightClick()
        {

        }
    }
}
