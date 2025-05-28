using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGreen.Game.Entities.Projectiles.ProjectileBehaviors
{
    public interface IProjectileBehavior
    {
        void AI(double delta, Projectile projectile);

        IProjectileBehavior Clone();
    }
}
