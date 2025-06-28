using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Vestige.Game.Input;
using Vestige.Game.UI.Components;

namespace Vestige.Game.UI.Containers
{
    /// <summary>
    /// A collection of UIComponents.
    /// Should be used to create menu sections or UIComponent groups.
    /// </summary>
    public class UIContainer : IInputHandler
    {

        private static UIComponent _focusedUIComponent;
        public int ComponentCount;
        public int ContainerCount;
        public Vector2 Position;
        private Vector2 _size;
        protected Vector2 parentSize;
        private Matrix _parentMatrix;
        public Vector2 Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                UpdateAnchorMatrix((int)parentSize.X, (int)parentSize.Y, _parentMatrix);
            }
        }
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

        public UIContainer(Vector2 position = default, Vector2 size = default, Anchor anchor = Anchor.MiddleMiddle)
        {
            Position = position;
            _size = size;
            ComponentCount = 0;
            ContainerCount = 0;
            _anchor = anchor;
            parentSize = Vector2.Zero;
            _parentMatrix = default;
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
                        component.HandleMouseInput(mouseEvent, GetLocalMouseCoordinates());
                    else
                        component.HandleGuiInput(@event);
                }
                else if (@event is MouseInputEvent @mouseEvent && component.MouseInside)
                {
                    component.HandleMouseInput(mouseEvent, GetLocalMouseCoordinates());
                }
            }
            for (int i = _containerChildren.Count - 1; i >= 0; i--)
            {
                _containerChildren[i].HandleInput(@event);
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
                        component.HandleMouseEntered();
                        component.MouseInside = true;
                    }
                }
                else if (component.MouseInside)
                {
                    component.HandleMouseExited();
                    component.MouseInside = false;
                }
                component.Update(delta);
            }
            for (int i = _containerChildren.Count - 1; i >= 0; i--)
            {
                _containerChildren[i].Update(delta);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>The mouse coordinates scaled to this UIContainers anchor matrix</returns>
        protected Vector2 GetLocalMouseCoordinates()
        {
            return Vector2.Transform(InputManager.GetMouseWindowPosition(), invertedAnchorMatrix);
        }
        public virtual void Draw(SpriteBatch spriteBatch, RasterizerState rasterizerState = null)
        {
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp, null, transformMatrix: AnchorMatrix, rasterizerState: rasterizerState);
            DrawComponents(spriteBatch);
            //Utilities.DrawOutlineRectangle(spriteBatch, new Rectangle(Point.Zero, Size.ToPoint()), Color.Red);
            spriteBatch.End();
            foreach (UIContainer uiComponentContainer in _containerChildren)
            {
                uiComponentContainer.Draw(spriteBatch, rasterizerState);
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
        public virtual void AddComponentChild(UIComponent component)
        {
            if (_componentChildren.Contains(component))
                return;
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
        public virtual void AddContainerChild(UIContainer container)
        {
            if (_containerChildren.Contains(container))
                return;
            _containerChildren.Add(container);
            container.UpdateAnchorMatrix((int)Size.X, (int)Size.Y, AnchorMatrix);
            ContainerCount++;
        }
        public virtual void RemoveContainerChild(UIContainer container)
        {
            _containerChildren.Remove(container);
            ContainerCount--;
        }
        public UIContainer GetContainerChild(int index)
        {
            return _containerChildren[index];
        }
        public void SetFocusedComponent(UIComponent component)
        {
            _focusedUIComponent = component;
        }

        public UIComponent GetFocusedComponent(UIComponent component)
        {
            return _focusedUIComponent;
        }
        public void Dereference()
        {
            InputManager.UnregisterHandler(this);
            UIManager.UnregisterContainer(this);
        }
        public void UpdateAnchorMatrix(int parentWidth, int parentHeight, Matrix parentMatrix = default)
        {
            parentSize = new Vector2(parentWidth, parentHeight);
            _parentMatrix = parentMatrix;
            _anchorMatrix = GetAnchorMatrix(parentWidth, parentHeight, parentMatrix);
            invertedAnchorMatrix = Matrix.Invert(_anchorMatrix);
            foreach (UIContainer uiComponentContainer in _containerChildren)
            {
                uiComponentContainer.UpdateAnchorMatrix((int)Size.X, (int)Size.Y, AnchorMatrix);
            }
        }
        private Matrix GetAnchorMatrix(int parentWidth, int parentHeight, Matrix parentMatrix = default)
        {
            if (_anchor == Anchor.None)
            {
                _size = new Vector2(parentWidth, parentHeight);
                return default;
            }
            Vector2 transformedPosition = Vector2.Transform(Position, Vestige.UIScaleMatrix);
            if (parentMatrix == default)
            {
                parentMatrix = Vestige.UIScaleMatrix;
            }
            else
            {
                Vector2 transformedSize = Vector2.Transform(new Vector2(parentWidth, parentHeight), Vestige.UIScaleMatrix);
                parentWidth = (int)transformedSize.X;
                parentHeight = (int)transformedSize.Y;
            }

            Vector2 containerSize = Vector2.Transform(Size, Vestige.UIScaleMatrix);
            Vector2 anchorPos = _anchor switch
            {
                Anchor.TopLeft => transformedPosition,
                Anchor.TopMiddle => new Vector2((parentWidth / 2) - (containerSize.X / 2), 0) + transformedPosition,
                Anchor.TopRight => new Vector2(parentWidth - containerSize.X, 0) + transformedPosition,

                Anchor.MiddleLeft => new Vector2(0, (parentHeight / 2) - (containerSize.Y / 2)) + transformedPosition,
                Anchor.MiddleMiddle => new Vector2((parentWidth / 2) - (containerSize.X / 2), (parentHeight / 2) - (containerSize.Y / 2)) + transformedPosition,
                Anchor.MiddleRight => new Vector2(parentWidth - containerSize.X, (parentHeight / 2) - (containerSize.Y / 2)) + transformedPosition,

                Anchor.BottomLeft => new Vector2(0, parentHeight - containerSize.Y) + transformedPosition,
                Anchor.BottomMiddle => new Vector2((parentWidth / 2) - (containerSize.X / 2), parentHeight - containerSize.Y) + transformedPosition,
                Anchor.BottomRight => new Vector2(parentWidth - containerSize.X, parentHeight - containerSize.Y) + transformedPosition,

                _ => Vector2.Zero
            };

            Matrix translation = Matrix.CreateTranslation(anchorPos.X, anchorPos.Y, 0);

            return parentMatrix * translation;
        }
    }
}
