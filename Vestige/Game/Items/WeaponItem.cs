using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vestige.Game.Entities;
using Vestige.Game.Items.Weapons;

namespace Vestige.Game.Items
{
    public class WeaponItem : Item
    {
        private int _baseDamage;
        private int _baseKnockback;
        public int Damage { get { return _baseDamage; } }
        public int Knockback { get { return _baseKnockback; } }
        public bool SpriteDoesDamage;
        private IWeapon _weaponBehavior;
        public IWeapon WeaponBehavior { get { return _weaponBehavior; } }
        private readonly int _projectileID;
        private readonly float _projectileSpeed;
        public WeaponItem(int id, string name, string description, Texture2D image, Vector2 origin, bool stackable, double useSpeed, bool autoUse, bool spriteDoesDamage, int baseDamage, int baseKnockback, UseStyle useStyle = UseStyle.Swing, IWeapon weaponBehavior = null, int maxStack = 1, int projectileID = -1, float projectileSpeed = 100f) 
            : base(id, name, description, image, origin, stackable, true, useSpeed, autoUse, maxStack, useStyle)
        {
            SpriteDoesDamage = spriteDoesDamage;
            _baseDamage = baseDamage;
            _baseKnockback = baseKnockback;
            _weaponBehavior = weaponBehavior;
            _projectileID = projectileID;
            _projectileSpeed = projectileSpeed;
        }
        public override bool UseItem(Player player)
        {
            if (_projectileID != -1)
            {
                Vector2 direction = Vector2.Normalize(Main.GetMouseWorldPosition().ToVector2() - player.Position);
                Main.EntityManager.CreateProjectile(_projectileID, player.Position, _projectileSpeed, direction);
            }
            return _weaponBehavior?.UseItem() ?? true;
        }
        protected override Item CloneItem()
        {
            return new WeaponItem(ID, Name, Description, Image, Origin, Stackable, UseSpeed, AutoUse, SpriteDoesDamage, Damage, Knockback, UseStyle, WeaponBehavior, MaxStack, _projectileID, _projectileSpeed);
        }
    }
}
