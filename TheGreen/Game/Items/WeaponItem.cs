using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Items.WeaponBehaviors;

namespace TheGreen.Game.Items
{
    public class WeaponItem : Item
    {
        private int _baseDamage;
        public bool SpriteDoesDamage;
        private IWeaponBehavior _weaponBehavior;
        public WeaponItem(int id, string name, string description, Texture2D image, bool stackable, double useSpeed, bool autoUse, bool spriteDoesDamage, UseStyle useStyle = UseStyle.Swing, IWeaponBehavior weaponBehavior = null) : base(id, name, description, image, stackable, useSpeed, autoUse, useStyle)
        {
            this.SpriteDoesDamage = true;
            this._weaponBehavior = weaponBehavior;
        }
        public override bool UseItem()
        {
            return _weaponBehavior?.UseItem() ?? true;
        }
    }
}
