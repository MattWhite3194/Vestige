using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vestige.Game.Tiles
{
    //change to int if need be. minimal data usage because tiledata properties are one per tile type, not per tile
    public enum TileProperty : ushort
    {
        None = 1 << 0,
        /// <summary>
        /// The player can collide with this tile, and it absorbs light.
        /// </summary>
        Solid = 1 << 1,
        /// <summary>
        /// The tile emits light.
        /// </summary>
        LightEmitting = 1 << 2,
        PickaxeMineable = 1 << 5,
        AxeMineable = 1 << 6,
        HammerMineable = 1 << 7,
        LargeTile = 1 << 8,
        /// <summary>
        /// The tile performs an action when an entity collides with it
        /// </summary>
        TileCollider = 1 << 9,
        Platform = 1 << 10,
    }
}
