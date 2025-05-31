using System.Collections.Generic;
using Vestige.Game.Items.Weapons;
using Vestige.Game.Tiles;

namespace Vestige.Game.Items
{
    /// <summary>
    /// Stores template classes for items.
    /// </summary>
    public static class ItemDatabase
    {
        private static Dictionary<int, Item> _items = new Dictionary<int, Item>
        {
            {0, new TileItem(0, "Dirt", "Keep your hands off my dirt!", ContentLoader.ItemTextures[0], 1) },
            {1, new TileItem(1, "Stone", "Hard as a rock.", ContentLoader.ItemTextures[1], 3) },
            {2, new WeaponItem(2, "Stone Pickaxe", "Go and break something.", ContentLoader.ItemTextures[2], false, 0.3f, true, true, 4, 2, UseStyle.Swing, new Pickaxe(35)) }, //0.2f
            {3, new TileItem(3, "Torch", "Light it up!", ContentLoader.ItemTextures[3], 7) },
            {4, new LiquidItem(4, "Water Bucket", "It's a little wet.", ContentLoader.ItemTextures[4], 1) },
            {5, new TileItem(5, "Chest", "For storing shiny things!", ContentLoader.ItemTextures[5], 8) },
            {6, new WeaponItem(6, "Steel Axe", "Don't take from my pile.", ContentLoader.ItemTextures[6], false, 0.2f, true, true, 12, 1, UseStyle.Swing, new Axe(10)) },
            {7, new TileItem(7, "Door", "When one door closes, you can't get in anymore.", ContentLoader.ItemTextures[7], 9) },
            {8, new WeaponItem(8, "Steel Hammer", "Time for smashing things", ContentLoader.ItemTextures[8], false, 0.2f, true, true, 8, 1, UseStyle.Swing, new Hammer(20)) },
            {9, new TileItem(9, "Wood Planks", "", ContentLoader.ItemTextures[9], 11) },
            {10, new Item(10, "Stick", "", ContentLoader.ItemTextures[10], true) }
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Item object with the specified ID</returns>
        public static Item InstantiateItemByID(int id, int quantity = 1)
        {
            Item item = CloneItem(_items[id]);
            item.Quantity = quantity;
            return item;
        }

        private static Item CloneItem(Item item)
        {
            return item switch
            {
                TileItem tileItem => new TileItem(tileItem.ID, tileItem.Name, tileItem.Description, tileItem.Image, tileItem.TileID),
                WeaponItem weapon => new WeaponItem(weapon.ID, weapon.Name, weapon.Description, weapon.Image, weapon.Stackable, weapon.UseSpeed, weapon.AutoUse, weapon.SpriteDoesDamage, weapon.Damage, weapon.Knockback, useStyle: weapon.UseStyle, weaponBehavior: weapon.WeaponBehavior),
                LiquidItem liquid => new LiquidItem(liquid.ID, liquid.Name, liquid.Description, liquid.Image, liquid.LiquidID),
                Item defaultItem => new Item(defaultItem.ID, defaultItem.Name, defaultItem.Description, defaultItem.Image, defaultItem.Stackable, defaultItem.CanUse, defaultItem.UseSpeed, defaultItem.AutoUse, defaultItem.MaxStack, defaultItem.UseStyle),
                _ => null
            };
        }

        public static Item InstantiateItemByTileID(ushort tileID, int quantity = 1)
        {
            int itemID = TileDatabase.GetTileData(tileID).ItemID;
            return itemID == -1 ? null : InstantiateItemByID(itemID);
        }
    }
}
