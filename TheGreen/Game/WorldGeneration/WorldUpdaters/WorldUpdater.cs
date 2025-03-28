using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGreen.Game.WorldGeneration.WorldUpdaters
{
    internal abstract class WorldUpdater
    {
        private double _updateRate;
        private double _elapsedTime;
        protected WorldUpdater(double updateRate)
        {
            _updateRate = updateRate;
        }
        internal void Update(double delta)
        {
            _elapsedTime += delta;
            if (_elapsedTime > _updateRate)
            {
                _elapsedTime = 0;
                OnUpdate();
            }
        }
        protected abstract void OnUpdate();
    }
}
