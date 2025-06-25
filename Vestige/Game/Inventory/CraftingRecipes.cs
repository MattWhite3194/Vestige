using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Vestige.Game.Items;

namespace Vestige.Game.Inventory
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
                return obj is CraftingKey other && _size.Equals(other._size) && _inputs.SequenceEqual(other._inputs);
            }
            public override int GetHashCode()
            {
                int hash = _size.GetHashCode();
                foreach ((byte x, byte y, int itemID) input in _inputs)
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
        //TODO: Add quantities to recipes
        private static Dictionary<CraftingKey, int> _recipes = new Dictionary<CraftingKey, int>()
        {
            //Wood Door
            {new CraftingKey(
                new Point(2, 3),
                    [
                        (0, 0, 8), (1, 0, 8),
                        (0, 1, 8), (1, 1, 8),
                        (0, 2, 8), (1, 2, 8)
                    ]
            ), 7},
            //Wood Chest
            {new CraftingKey(
                new Point(3, 3),
                    [
                        (0, 0, 8), (1, 0, 8), (2, 0, 8),
                        (0, 1, 8),            (2, 1, 8),
                        (0, 2, 8), (1, 2, 8), (2, 2, 8)
                    ]
            ), 5},
            //Stick
            {new CraftingKey(
                new Point(1, 2),
                    [
                        (0, 0, 8),
                        (0, 1, 8),
                    ]
            ), 9},
            //Torch
            {new CraftingKey(
                new Point(1, 2),
                    [
                        (0, 0, 13),
                        (0, 1, 9),
                    ]
            ), 3},
            //Bow
            {new CraftingKey(
                new Point(2, 3),
                    [
                        (0, 0, 9),
                                    (1, 1, 9),
                        (0, 2, 9),
                    ]
            ), 10},
            //Bomb
            {new CraftingKey(
                new Point(2, 2),
                    [
                        (0, 0, 1), (1, 0, 1),
                        (0, 1, 1), (1, 1, 1),

                    ]
            ), 11},
            //Wood Platform
            {new CraftingKey(
                new Point(2, 1),
                    [
                        (0, 0, 8), (1, 0, 8),
                    ]
            ), 12},
            //Stone bricks
            {new CraftingKey(
                new Point(2, 2),
                    [
                        (0, 0, 15), (1, 0, 15),
                        (0, 1, 15), (1, 1, 15),
                    ]
            ), 14},
        };
        private static Dictionary<ShapelessCraftingKey, int> _shapelessRecipes = new Dictionary<ShapelessCraftingKey, int>()
        {

        };
        public static Item GetItemFromRecipe(Point size, List<(byte, byte, int)> inputs)
        {
            return _recipes.TryGetValue(new CraftingKey(size, inputs), out int itemID) ? Item.InstantiateItemByID(itemID) : null;
        }
    }
}
