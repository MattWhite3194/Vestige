using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Vestige.Game.Entities;
using Vestige.Game.Items.Weapons;

namespace Vestige.Game.Items
{
    /// <summary>
    /// Used by the inventory. Describes an Items functionality.
    /// </summary>
    public class Item
    {
        public readonly int ID;
        public readonly string Name;
        public readonly string Description;
        public readonly Texture2D Image;
        public int Quantity;
        public readonly bool Stackable;
        public readonly double UseSpeed;
        public readonly bool AutoUse;
        public readonly int MaxStack;
        public readonly bool CanUse;
        public float Scale = 1f;
        public readonly UseStyle UseStyle;
        public Vector2 Origin;

        public Item(int id, string name, string description, Texture2D image, Vector2 origin = default, bool stackable = false, bool canUse = false, double useSpeed = 0.5f, bool autoUse = false, int maxStack = 999, UseStyle useStyle = UseStyle.None)
        {
            ID = id;
            Name = name;
            Description = description;
            Image = image;
            Origin = origin;
            Stackable = stackable;
            UseSpeed = useSpeed;
            AutoUse = autoUse;
            UseStyle = useStyle;
            MaxStack = maxStack;
            CanUse = canUse;
        }
        public virtual bool UseItem(Player player, bool altUse)
        {
            return true;
        }
        protected virtual Item CloneItem()
        {
            return new Item(ID, Name, Description, Image, Origin, Stackable, CanUse, UseSpeed, AutoUse, MaxStack, UseStyle);
        }

        private static Item[] _items = 
        {
            new TileItem(0, "Dirt", "Keep your hands off my dirt!", ContentLoader.ItemTextures[0], 1),
            new TileItem(1, "Cobblestone", "Hard as a rock.", ContentLoader.ItemTextures[1], 3),
            new WeaponItem(2, "Stone Pickaxe", "Go and break something.", ContentLoader.ItemTextures[2], default, false, 0.4f, true, true, 4, 2, UseStyle.Swing, new Pickaxe(35)), //0.2f
            new TileItem(3, "Torch", "Light it up!", ContentLoader.ItemTextures[3], 7),
            new LiquidItem(4, "Water Bucket", "It's a little wet.", ContentLoader.ItemTextures[4], 1),
            new TileItem(5, "Chest", "For storing shiny things!", ContentLoader.ItemTextures[5], 8),
            new WeaponItem(6, "Steel Axe", "Don't take from my pile.", ContentLoader.ItemTextures[6], default, false, 0.35f, true, true, 6, 1, UseStyle.Swing, new Axe(10)),
            new TileItem(7, "Wood Door", "When one door closes, you can't get in anymore.", ContentLoader.ItemTextures[7], 9),
            new TileItem(8, "Wood Planks", "", ContentLoader.ItemTextures[8], 11),
            new Item(9, "Stick", "", ContentLoader.ItemTextures[9], stackable: true),
            new WeaponItem(10, "Wood Bow", "", ContentLoader.ItemTextures[10], new Vector2(4, 15), false, 0.5f, true, false, 10, 2, UseStyle.Point, projectileID: 0, projectileSpeed: 500f), //500f
            new WeaponItem(11, "Bomb", "", ContentLoader.ItemTextures[11], default, true, 0.5f, true, false, 10, 2, UseStyle.Throw, projectileID: 1, projectileSpeed: 200f, maxStack: 50),
            new TileItem(12, "Wood Platform", "", ContentLoader.ItemTextures[12], 13),
            new Item(13, "Coal", "", ContentLoader.ItemTextures[13], stackable: true),
            new TileItem(14, "Stone Bricks", "", ContentLoader.ItemTextures[14], 14),
            new TileItem(15, "Stone", "", ContentLoader.ItemTextures[15], 4),
            new WeaponItem(16, "Steel Pickaxe", "Upgraded breaking thinks.", ContentLoader.ItemTextures[16], default, false, 0.35f, true, true, 5, 2, UseStyle.Swing, new Pickaxe(55)), //0.2f
            new WeaponItem(17, "Stone Axe", "Go and break something.", ContentLoader.ItemTextures[17], default, false, 0.4f, true, true, 4, 2, UseStyle.Swing, new Axe(6)), //0.2f
            new WeaponItem(18, "Steel Sword", "Time for dicing.", ContentLoader.ItemTextures[18], default, false, 0.35f, true, true, 11, 3, UseStyle.Swing), //0.2f
            new WeaponItem(19, "Stone Sword", "Time for slicing.", ContentLoader.ItemTextures[19], default, false, 0.4f, true, true, 7, 2, UseStyle.Swing), //0.2f

        };
        public static Item InstantiateItemByID(int id, int quantity = 1)
        {
            Item item = _items[id].CloneItem();
            item.Quantity = quantity;
            return item;
        }
    }
    public enum UseStyle
    {
        None,
        Swing,
        Hold,
        Point,
        Throw,
    }
}
