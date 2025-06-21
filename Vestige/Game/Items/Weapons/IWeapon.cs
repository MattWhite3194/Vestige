using Vestige.Game.Entities;

namespace Vestige.Game.Items.Weapons
{
    /// <summary>
    /// use for special weapon types that have unique behaviors only
    /// </summary>
    public interface IWeapon
    {
        public bool UseItem(Player player, bool altUse);
    }
}
