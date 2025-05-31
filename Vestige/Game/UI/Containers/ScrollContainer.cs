using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.Input;
using Vestige.Game.UI.Components;

namespace Vestige.Game.UI.Containers
{
    public class ScrollContainer : GridContainer
    {
        private float _viewHeight;
        private int _initialPositionY;
        private int _scrollSpeed;
        private float _scrollerSize;
        private float _scrollOffset;
        private RasterizerState _rasterizerState = new RasterizerState()
        {
            ScissorTestEnable = true
        };
        public ScrollContainer(Vector2 position, int margin = 5, int scrollSpeed = 4, Vector2 size = default, Anchor anchor = Anchor.MiddleMiddle) : base(1, margin, position, size, anchor)
        {
            _viewHeight = size.Y;
            _initialPositionY = (int)position.Y;
            _scrollSpeed = scrollSpeed;
        }
        public override void HandleInput(InputEvent @event)
        {
            if (!new Rectangle((int)Position.X, _initialPositionY, (int)Size.X, (int)_viewHeight).Contains(Vector2.Transform(InputManager.GetMouseWindowPosition(), invertedAnchorMatrix)))
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
                if (Position.Y + Size.Y + _scrollOffset <= _initialPositionY + _viewHeight)
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
            for (int i = 0; i < this.ContainerCount; i++)
            {
                GetContainerChild(i).Position = GetContainerChild(i).Position + new Vector2(0, scrollAmount);
                GetContainerChild(i).UpdateAnchorMatrix((int)Size.X, (int)Size.Y, AnchorMatrix);
            }
        }
        public override void Draw(SpriteBatch spriteBatch, RasterizerState rasterizerState = null)
        {
            Rectangle clippingRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
            spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(Vector2.Transform(new Vector2((int)Position.X, _initialPositionY), AnchorMatrix).ToPoint(), Vector2.Transform(new Vector2((int)Size.X, _viewHeight), Vestige.UIScaleMatrix).ToPoint());
            base.Draw(spriteBatch, _rasterizerState);
            spriteBatch.GraphicsDevice.ScissorRectangle = clippingRectangle;
        }
        public override void AddComponentChild(UIComponent component)
        {
            base.AddComponentChild(component);
            _scrollerSize = Math.Max(Math.Min(1f, _viewHeight / Size.Y), 0.1f) * _viewHeight;
        }
        public override void AddContainerChild(UIContainer container)
        {
            base.AddContainerChild(container);
            _scrollerSize = Math.Max(Math.Min(1f, _viewHeight / Size.Y), 0.1f) * _viewHeight;
        }
        protected override void DrawComponents(SpriteBatch spriteBatch)
        {
            base.DrawComponents(spriteBatch);
            if (Size.Y > _viewHeight)
            {
                DebugHelper.DrawFilledRectangle(spriteBatch, new Rectangle((int)Size.X - 5, 0, 4, (int)_viewHeight - 1), Color.DimGray);
                DebugHelper.DrawFilledRectangle(spriteBatch, new Rectangle((int)Size.X - 5, (int)((_initialPositionY - (Position.Y + _scrollOffset)) / (base.Size.Y - _viewHeight) * (_viewHeight - _scrollerSize)), 4, (int)_scrollerSize), Color.LightGray);
            }
        }
    }
}
