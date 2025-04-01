using System;
using System.Collections.Generic;
using System.Reflection;
using TheGreen.Game.Items.WeaponBehaviors;
using TheGreen.Game.Tiles;

namespace TheGreen.Game.Items
{
    /// <summary>
    /// Holds information about items.
    /// </summary>
    public static class ItemDatabase
    {
        private static string _itemNamespace = "TheGreen.Game.Items.";
        //TODO: Move to JSON file preferably
        //  <ID, (typeName, [type constructors], [attributes]>
        private static Dictionary<int, (string, object[], string[])> _items = new Dictionary<int, (string, object[], string[])>
        {
            {0, ("TileItem", [0, "Dirt", "Keep your hands off my dirt!", ContentLoader.ItemTextures[0], (ushort)1], []) },
            {1, ("TileItem", [1, "Stone", "Hard as a rock.", ContentLoader.ItemTextures[1], (ushort)3], []) },
            {2, ("WeaponItem", [2, "Basic Pickaxe", "Go and break something.", ContentLoader.ItemTextures[2], false, 0.2f, true, true, 20, 2, UseStyle.Swing, new Pickaxe(30)], []) }, //0.2f
            {3, ("TileItem", [3, "Torch", "Light it up!", ContentLoader.ItemTextures[3], (ushort)7], []) },
            {4, ("LiquidItem", [4, "Water Bucket", "It's a little wet.", ContentLoader.ItemTextures[4], (ushort)1], []) }
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Item object with the specified ID</returns>
        public static Item GetItemByID(int id, int quantity = 1)
        {
            
            Type type = Type.GetType(_itemNamespace + _items[id].Item1);
            Type[] types = GetTypes(_items[id].Item2);
            ConstructorInfo constructor = type.GetConstructor(types);
            Item item = (Item)constructor.Invoke(_items[id].Item2);
            item.Quantity = quantity;
            return item;
        }
        
        private static Type[] GetTypes(object[] values)
        {
            Type[] types = new Type[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                types[i] = values[i].GetType();
            }
            return types;
        }

        public static Item GetItemByTileID(ushort tileID, int quantity = 1)
        {
            int itemID = TileDatabase.GetTileItemID(tileID);
            return itemID == -1 ? null : GetItemByID(itemID);
        }
    }
}
