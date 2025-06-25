using Microsoft.Xna.Framework;

namespace Vestige.Game.Tiles.TileData
{
    public class OverlayTileData : DefaultTileData
    {
        public readonly ushort BaseTileID;
        public OverlayTileData(int tileID, string name, TileProperty properties, Color color, int itemID = -1, int health = 0, ushort baseTileID = 0) : base(tileID, name, properties, color, itemID, -1, health)
        {
            BaseTileID = baseTileID;
        }
        internal override bool IsTileInMergeList(ushort tileID)
        {
            return base.IsTileInMergeList(tileID) || TileDatabase.GetTileData(BaseTileID).CanMerge(tileID);
        }
    }
}
