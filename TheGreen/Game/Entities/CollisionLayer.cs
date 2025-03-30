using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGreen.Game.Entities
{
    [Flags]
    public enum CollisionLayer
    {
        Player = 1 << 0,
        Enemy = 1 << 1,
        FriendlyProjectile = 1 << 2,
        HostileProjectile = 1 << 3,
        /// <summary>
        /// Item drops in the world
        /// </summary>
        ItemDrop = 1 << 4,
        /// <summary>
        /// The players item hitbox
        /// </summary>
        ItemCollider = 1 << 5,
    }
}
