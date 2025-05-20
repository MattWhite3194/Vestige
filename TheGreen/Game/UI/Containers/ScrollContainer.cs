using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.Input;
using TheGreen.Game.UI.Components;

namespace TheGreen.Game.UI.Containers
{
    public class ScrollContainer : GridContainer
    {
        private int _viewHeight;
        private int _initialPositionY;
        private int _scrollSpeed;
        private float _scrollerSize;
        private float _scrollOffset;
        public new Vector2 Size
        {
            get { return new Vector2(base.Size.X, _viewHeight);}
        }

        //TODO: implement button as the scroller, when clicked, it will follow the y position of the mouse, and the scrollContainers position will be updated based on it
        private Button _scroller;
        private RasterizerState rasterizerState = new RasterizerState()
        {
            ScissorTestEnable = true
        };
        public ScrollContainer(Vector2 position, int viewHeight, int margin = 5, int scrollSpeed = 4, Vector2 size = default, Anchor anchor = Anchor.MiddleMiddle) : base(1, margin, position, size, anchor)
        {
            _viewHeight = viewHeight;
            _initialPositionY = (int)position.Y;
            _scrollSpeed = scrollSpeed;
        }
        public override void HandleInput(InputEvent @event)
        {
            if (!new Rectangle((int)Position.X, _initialPositionY, (int)Size.X, _viewHeight).Contains(Vector2.Transform(InputManager.GetMouseWindowPosition(), invertedAnchorMatrix)))
                return;
            if (@event is MouseInputEvent mouseInputEvent && mouseInputEvent.InputButton == InputButton.MiddleMouse)
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
                if (Position.Y + base.Size.Y + _scrollOffset <= _initialPositionY + _viewHeight)
                {
                    return;
                }
            }
            else
            {
                if (Position.Y + _scrollOffset >= _initialPositionY)
                {
                    return;
                }
            }
            _scrollOffset += scrollAmount;
            for (int i = 0; i < this.ComponentCount; i++)
            {
                GetComponentChild(i).Position = GetComponentChild(i).Position + new Vector2(0, scrollAmount);
            }
        }
        public override void AddComponentChild(UIComponent component)
        {
            base.AddComponentChild(component);
            _scrollerSize = Math.Max(Math.Min(1f, _viewHeight / base.Size.Y), 0.1f) * _viewHeight;
        }
        protected override void DrawComponents(SpriteBatch spriteBatch)
        {
            spriteBatch.GraphicsDevice.RasterizerState = rasterizerState;
            Rectangle clippingRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
            spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(Vector2.Transform(new Vector2((int)Position.X, _initialPositionY), AnchorMatrix).ToPoint(), Vector2.Transform(new Vector2((int)Size.X, _viewHeight), TheGreen.UIScaleMatrix).ToPoint());
            base.DrawComponents(spriteBatch);
            DebugHelper.DrawFilledRectangle(spriteBatch, new Rectangle((int)Size.X - 5, 0, 4, _viewHeight - 1), Color.DimGray);
            DebugHelper.DrawFilledRectangle(spriteBatch, new Rectangle((int)Size.X - 5, (int)((_initialPositionY - (Position.Y + _scrollOffset)) / (base.Size.Y - _viewHeight) * (_viewHeight - _scrollerSize)), 4, (int)_scrollerSize), Color.LightGray);
            spriteBatch.GraphicsDevice.ScissorRectangle = clippingRectangle;
        }
    }
}
