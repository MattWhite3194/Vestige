using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGreen.Game.Tiles
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
        /// <summary>
        /// The tile does damage to entities.
        /// </summary>
        Overlay = 1 << 4,
        /// <summary>
        /// The tile's state does not change when tiles around it are updated
        /// </summary>
        StaticTileState = 1 << 5,
        PickaxeMineable = 1 << 6,
        AxeMineable = 1 << 7,
        HammerMineable = 1 << 8,
    }
}
