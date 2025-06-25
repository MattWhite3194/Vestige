using Microsoft.Xna.Framework;

namespace Vestige.Game.Entities
{
    public struct CollisionRectangle
    {
        public float X;
        public float Y;
        public int Width;
        public int Height;
        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;
        public Vector2 Center => new Vector2(X + (Width / 2.0f), Y + (Height / 2.0f));
        public Vector2 Position => new Vector2(X, Y);
        public CollisionRectangle(float x, float y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public CollisionRectangle(Vector2 location, Point size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.X;
            Height = size.Y;
        }
        public bool Intersects(CollisionRectangle value)
        {
            return value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;
        }
        public bool Intersects(Rectangle value)
        {
            return value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;
        }
        public bool Contains(Vector2 value)
        {
            return X <= value.X && value.X < Right && Y <= value.Y && value.Y < (Y + Height);
        }
        public bool Contains(Point value)
        {
            return X <= value.X && value.X < (X + Width) && Y <= value.Y && value.Y < (Y + Height);
        }
        public override string ToString()
        {
            return $"X: {X} Y: {Y} Width: {Width} Height: {Height}";
        }
    }
}
