using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGreen.Game.Items.WeaponBehaviors
{
    /// <summary>
    /// use for special weapon types that have unique behaviors only
    /// </summary>
    public interface IWeaponBehavior
    {
        public bool UseItem();
    }
}
