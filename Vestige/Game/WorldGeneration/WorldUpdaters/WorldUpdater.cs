namespace Vestige.Game.WorldGeneration.WorldUpdaters
{
    internal abstract class WorldUpdater
    {
        private double _updateRate;
        private double _elapsedTime;
        protected WorldGen world;
        protected WorldUpdater(WorldGen world, double updateRate)
        {
            _updateRate = updateRate;
            this.world = world;
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
