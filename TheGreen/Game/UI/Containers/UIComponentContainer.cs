using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TheGreen.Game.Input;
using TheGreen.Game.UI.Components;

namespace TheGreen.Game.UI.Containers
{
    /// <summary>
    /// A collection of UIComponents.
    /// Should be used to create menu sections or UIComponent groups.
    /// </summary>
    public class UIContainer : IInputHandler
    {

        private static UIComponent _focusedUIComponent;
        protected GraphicsDevice graphicsDevice;
        public int ComponentCount;
        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                UpdateChildPositions(_position, value);
                _position = value;
            }
        }
        public Vector2 Size;
        private List<UIComponent> _componentChildren = new List<UIComponent>();
        private List<UIContainer> _containerChildren = new List<UIContainer>();
        private Anchor _anchor;
        private Matrix _anchorMatrix;
        public Matrix AnchorMatrix
        {
            get
            {
                return _anchorMatrix;
            }
        }
        protected Matrix invertedAnchorMatrix;

        public UIContainer(Vector2 position = default, Vector2 size = default, GraphicsDevice graphicsDevice = null, Anchor anchor = Anchor.MiddleMiddle)
        {
            Position = position;
            Size = size;
            this.graphicsDevice = graphicsDevice;
            ComponentCount = 0;
            _anchor = anchor;
        }
        public virtual void HandleInput(InputEvent @event)
        {
            for (int i = 0; i < _componentChildren.Count; i++)
            {
                UIComponent component = _componentChildren[i];
                if (InputManager.IsEventHandled(@event)) break;

                if (!component.IsVisible()) continue;

                if (component.IsFocused())
                {
                    if (@event is MouseInputEvent @mouseEvent)
                        component.OnMouseInput(@mouseEvent, GetLocalMouseCoordinates());
                    else
                        component.OnGuiInput(@event);
                }
                else if (@event is MouseInputEvent @mouseEvent && component.MouseInside)
                {
                    component.OnMouseInput(@mouseEvent, GetLocalMouseCoordinates());
                }
            }
            for (int i = _containerChildren.Count - 1; i >= 0; i--)
            {
                _containerChildren[i].HandleInput( @event );
            }
        }
        public virtual void Update(double delta)
        {

            foreach (UIComponent component in _componentChildren)
            {
                if (!component.IsVisible()) continue;

                if (component.GetBounds().Contains(GetLocalMouseCoordinates()))
                {
                    if (!component.MouseInside)
                    {
                        component.OnMouseEntered.Invoke();
                        component.MouseInside = true;
                    }
                }
                else if (component.MouseInside)
                {
                    component.OnMouseExited.Invoke();
                    component.MouseInside = false;
                }
                component.Update(delta);
            }
            for (int i = _containerChildren.Count - 1; i >= 0; i--)
            {
                _containerChildren[i].Update(delta);
            }
        }

        protected Vector2 GetLocalMouseCoordinates()
        {
            return Vector2.Transform(InputManager.GetMouseWindowPosition(), invertedAnchorMatrix);
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp, DepthStencilState.None, transformMatrix: AnchorMatrix);
            DrawComponents(spriteBatch);
            DebugHelper.DrawDebugRectangle(spriteBatch, new Rectangle(Position.ToPoint(), GetSize().ToPoint()), Color.Red);
            spriteBatch.End();
            foreach (UIContainer uiComponentContainer in _containerChildren)
            {
                uiComponentContainer.Draw(spriteBatch);
            }
        }
        protected virtual void DrawComponents(SpriteBatch spriteBatch)
        {
            foreach (UIComponent component in _componentChildren)
            {
                if (component.IsVisible())
                {
                    component.Draw(spriteBatch);
                }
            }
        }
        private void UpdateChildPositions(Vector2 oldPosition, Vector2 newPosition)
        {
            foreach (UIComponent component in _componentChildren)
            {
                component.Position -= oldPosition;
                component.Position += newPosition;
            }
        }
        public virtual void AddComponentChild(UIComponent component)
        {
            component.Position = component.Position + Position;
            _componentChildren.Add(component);
            ComponentCount++;
        }
        public virtual void RemoveComponentChild(UIComponent component)
        {
            _componentChildren.Remove(component);
            ComponentCount--;
        }
        public UIComponent GetComponentChild(int index)
        {
            return _componentChildren[index];
        }
        public void AddContainerChild(UIContainer uiComponentContainer)
        {
            _containerChildren.Add(uiComponentContainer);
            uiComponentContainer.UpdateAnchorMatrix((int)Size.X, (int)Size.Y);
        }
        public void RemoveContainerChild(UIContainer uiComponentContainer)
        {
            _containerChildren.Remove(uiComponentContainer);
        }
        public void SetFocusedComponent(UIComponent component)
        {
            _focusedUIComponent = component;
        }

        public UIComponent GetFocusedComponent(UIComponent component)
        {
            return _focusedUIComponent;
        }
        public virtual void Dereference()
        {
            InputManager.UnregisterHandler(this);
            UIManager.UnregisterContainer(this);
        }
        public virtual Vector2 GetSize()
        {
            return Size;
        }
        public virtual void UpdateAnchorMatrix(int parentWidth, int parentHeight, Matrix parentMatrix = default)
        {
            _anchorMatrix = GetAnchorMatrix(parentWidth, parentHeight, parentMatrix);
            invertedAnchorMatrix = Matrix.Invert(_anchorMatrix);
            foreach (UIContainer uiComponentContainer in _containerChildren)
            {
                uiComponentContainer.UpdateAnchorMatrix((int)Size.X, (int)Size.Y, AnchorMatrix);
            } 
        }
        private Matrix GetAnchorMatrix(int parentWidth, int parentHeight, Matrix parentMatrix = default)
        {
            if (parentMatrix == default)
            {
                parentMatrix = TheGreen.UIScaleMatrix;
            }
            else
            {
                Vector2 transformedSize = Vector2.Transform(new Vector2(parentWidth, parentHeight), TheGreen.UIScaleMatrix);
                parentWidth = (int)transformedSize.X;
                parentHeight = (int)transformedSize.Y;
            }
            if (_anchor == Anchor.None)
            {
                return parentMatrix;
            }
            else if (_anchor == Anchor.ScreenScale)
            {
                return Matrix.CreateScale(Math.Max(parentWidth / (float)TheGreen.NativeResolution.X, parentHeight / (float)TheGreen.NativeResolution.Y));
            }
            Vector2 containerSize = Vector2.Transform(GetSize(), TheGreen.UIScaleMatrix);
            Vector2 anchorPos = _anchor switch
            {
                Anchor.TopLeft => new Vector2(0, 0),
                Anchor.TopMiddle => new Vector2(parentWidth / 2 - containerSize.X / 2, 0),
                Anchor.TopRight => new Vector2(parentWidth - containerSize.X, 0),

                Anchor.MiddleLeft => new Vector2(0, parentHeight / 2 - containerSize.Y / 2),
                Anchor.MiddleMiddle => new Vector2(parentWidth / 2 - containerSize.X / 2, parentHeight / 2 - containerSize.Y / 2),
                Anchor.MiddleRight => new Vector2(parentWidth - containerSize.X, parentHeight / 2 - containerSize.Y / 2),

                Anchor.BottomLeft => new Vector2(0, parentHeight - containerSize.Y),
                Anchor.BottomMiddle => new Vector2(parentWidth / 2 - containerSize.X / 2, parentHeight - containerSize.Y),
                Anchor.BottomRight => new Vector2(parentWidth - containerSize.X, parentHeight - containerSize.Y),

                _ => Vector2.Zero
            };

            Matrix translation = Matrix.CreateTranslation(anchorPos.X, anchorPos.Y, 0);

            return parentMatrix * translation;
        }
    }
}
