using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using TheGreen.Game.Input;

namespace TheGreen.Game.UI.Containers
{
    public class ScrollContainer : GridContainer
    {
        private int _viewHeight;
        private int _initialPositionY;
        private int _scrollSpeed;

        public ScrollContainer(Vector2 position, int viewHeight, int margin = 5, int scrollSpeed = 2, Vector2 size = default) : base(1, margin, position, size)
        {
            _viewHeight = viewHeight;
            _initialPositionY = (int)position.Y;
            _scrollSpeed = scrollSpeed;
        }
        public override void HandleInput(InputEvent @event)
        {
            if (@event is MouseInputEvent mouseInputEvent && mouseInputEvent.InputButton == InputButton.MiddleMouse && new Rectangle((int)Position.X, _initialPositionY, (int)Size.X, _viewHeight).Contains(Vector2.Transform(InputManager.GetMouseWindowPosition(), invertedAnchorMatrix)))
            {
                if (mouseInputEvent.EventType == InputEventType.MouseButtonDown)
                {
                    OnScroll(_scrollSpeed);
                }
                else
                {
                    OnScroll(-_scrollSpeed);
                }
                InputManager.MarkInputAsHandled(mouseInputEvent);
                return;
            }
            base.HandleInput(@event);
        }
        private void OnScroll(int scrollAmount)
        {
            if (scrollAmount < 0)
            {
                if (Position.Y + Size.Y <= _initialPositionY + _viewHeight)
                {
                    return;
                }
            }
            else
            {
                if (Position.Y >= _initialPositionY)
                {
                    return;
                }
            }
            Position = new Vector2(Position.X, Position.Y + scrollAmount);
        }
        public override void Draw(SpriteBatch spritebatch)
        {
            base.Draw(spritebatch);
            DebugHelper.DrawDebugRectangle(spritebatch, new Rectangle((int)Position.X, _initialPositionY, (int)Size.X, _viewHeight), Color.Red);
        }
        public override Vector2 GetSize()
        {
            return new Vector2(Size.X, _viewHeight);
        }
    }
}
