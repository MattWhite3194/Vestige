namespace TheGreen.Game.Tiles
{
    public interface IInventoryTile
    {
        int GetCols();
        int GetRows();
        /// <summary>
        /// Use this to apply any tile modifications when the inventory closes
        /// </summary>
        void CloseInventory();
    }
}
