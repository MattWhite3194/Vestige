using Microsoft.Xna.Framework;

namespace Vestige.Game.WorldGeneration
{
    internal struct Structure
    {
        private Rectangle _bounds;
        public Rectangle Bounds { get { return _bounds; } }
        private int _priority;
        public int Priority { get { return _priority; } }
        public Structure(int x, int y, int width, int height, int priority = 0)
        {
            _bounds = new Rectangle(x, y, width, height);
            _priority = priority;
        }
        public Structure(Rectangle bounds, int priority = 0)
        {
            _bounds = bounds;
            _priority = priority;
        }
    }
}
