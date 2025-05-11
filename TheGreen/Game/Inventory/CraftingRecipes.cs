using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TheGreen.Game.Items;

namespace TheGreen.Game.Inventory
{
    //TODO: convert this to JSON: For real this time
    //TODO: shapeless recipes
    /*
     Add a ShapelessCraftingKey struct and another dictionary, compare lists by sorting by item id, and check if the item ids in the list are equal. Check Shaped recipes first, if there are none found, then check ShapelessRecipes
     */
    public static class CraftingRecipes
    {
        //crafting recipe size variable. etc: 1x2 3x3 2x2, so the recipes with smaller grids can be placed anywhere on the table
        //Point for grid size
        //Array of tuples or dictionary for items and their positions
        //items should go left->right & top->bottom

        private struct CraftingKey
        {
            private Point _size;
            private List<(byte x, byte y, int itemID)> _inputs;

            public CraftingKey(Point size, List<(byte x, byte y, int itemID)> inputs)
            {
                _size = size;
                _inputs = inputs;
            }
            public override bool Equals(object obj)
            {
                if (obj is CraftingKey other)
                {
                    return _size.Equals(other._size) && _inputs.SequenceEqual(other._inputs);
                }
                return false;
            }
            public override int GetHashCode()
            {
                int hash = _size.GetHashCode();
                foreach (var input in _inputs)
                {
                    hash = HashCode.Combine(hash, input.GetHashCode());
                }
                return hash;
            }
        }
        private struct ShapelessCraftingKey
        {
            //TODO: implementation
        }
        private static Dictionary<CraftingKey, int> _recipes = new Dictionary<CraftingKey, int>()
        {
            {new CraftingKey(
                new Point(1, 3),
                    [
                        (0, 0, 0),
                        (0, 1, 0),
                        (0, 2, 0)
                    ]
            ), 7},
        };
        private static Dictionary<ShapelessCraftingKey, int> _shapelessRecipes = new Dictionary<ShapelessCraftingKey, int>()
        {

        };
        public static Item GetItemFromRecipe(Point size, List<(byte, byte, int)> inputs)
        {
            if (_recipes.TryGetValue(new CraftingKey(size, inputs), out int itemID)) 
            {
                return ItemDatabase.InstantiateItemByID(itemID);
            }
            return null;
        }
    }
}
