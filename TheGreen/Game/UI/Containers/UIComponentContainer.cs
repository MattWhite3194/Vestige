using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using TheGreen.Game.Input;
using TheGreen.Game.UI.Components;
using TheGreen.Game.UIComponents;

namespace TheGreen.Game.UI.Containers
{
    /// <summary>
    /// A collection of UIComponents.
    /// Should be used to create menu sections or UIComponent groups.
    /// </summary>
    public class UIComponentContainer : IInputHandler
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
        private Vector2 _defaultSize;
        public Vector2 Size;
        private List<UIComponent> _uiComponents = new List<UIComponent>();
        public Anchor Anchor;
        private Matrix _anchorMatrix;
        public Matrix AnchorMatrix
        {
            get
            {
                return _anchorMatrix;
            }
            set
            {
                _anchorMatrix = value;
                invertedAnchorMatrix = Matrix.Invert(_anchorMatrix);
            }
        }
        protected Matrix invertedAnchorMatrix;

        public UIComponentContainer(Vector2 position = default, Vector2 size = default, GraphicsDevice graphicsDevice = null, Anchor anchor = Anchor.MiddleMiddle)
        {
            Position = position;
            _defaultSize = size;
            Size = size;
            this.graphicsDevice = graphicsDevice;
            ComponentCount = 0;
            Anchor = anchor;
        }
        public virtual void HandleInput(InputEvent @event)
        {
            for (int i = 0; i < _uiComponents.Count; i++)
            {
                UIComponent component = _uiComponents[i];
                if (InputManager.IsEventHandled(@event)) break;

                if (!component.IsVisible()) continue;

                if (component.IsFocused())
                {
                    component.OnGuiInput(@event);
                }
                else if (@event is MouseInputEvent @mouseEvent && component.MouseInside)
                {
                    component.OnMouseInput(@mouseEvent, GetLocalMouseCoordinates());
                }
            }
        }
        public virtual void Update(double delta)
        {

            foreach (UIComponent component in _uiComponents)
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
        }

        protected Vector2 GetLocalMouseCoordinates()
        {
            return Vector2.Transform(InputManager.GetMouseWindowPosition(), invertedAnchorMatrix);
        }
        public virtual void Draw(SpriteBatch spritebatch)
        {
            foreach (UIComponent component in _uiComponents)
            {
                if (component.IsVisible())
                {
                    component.Draw(spritebatch);
                }
            }
        }
        private void UpdateChildPositions(Vector2 oldPosition, Vector2 newPosition)
        {
            foreach (UIComponent component in _uiComponents)
            {
                component.Position -= oldPosition;
                component.Position += newPosition;
            }
        }
        public virtual void AddUIComponent(UIComponent component)
        {
            component.Position = component.Position + Position;
            _uiComponents.Add(component);
            ComponentCount++;
            RecalculateSize();
        }
        public virtual void RemoveUIComponent(UIComponent component)
        {
            _uiComponents.Remove(component);
            ComponentCount--;
            RecalculateSize();
        }
        public UIComponent GetUIComponent(int index)
        {
            return _uiComponents[index];
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
        public virtual void SetAnchorMatrix(Matrix anchorMatrix)
        {
            AnchorMatrix = anchorMatrix;
        }
        private void RecalculateSize()
        {
            Vector2 newSize = Vector2.Zero;
            foreach (UIComponent component in _uiComponents)
            {
               newSize = Vector2.Max(newSize, component.Position - Position + component.Size);
            }
            Size.X = _defaultSize.X != 0 ? _defaultSize.X : newSize.X;
            Size.Y = _defaultSize.Y != 0 ? _defaultSize.Y : newSize.Y;
        }
        public virtual Vector2 GetSize()
        {
            return Size;
        }
    }
}
