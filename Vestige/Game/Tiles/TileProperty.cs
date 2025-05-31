using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vestige.Game.Tiles
{
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
        TileCollider = 1 << 9,
    }
}
