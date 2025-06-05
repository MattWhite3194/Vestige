using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vestige.Game.UI.Components;

namespace Vestige.Game.UI.Containers
{
    public class GridContainer : UIContainer
    {
        private int _cols, _margin;
        private float[] _columnPositions;
        private float _currentContainerHeight;
        private List<object> _gridElements;

        public GridContainer(int cols, int margin = 5, Vector2 position = default, Vector2 size = default, Anchor anchor = Anchor.MiddleMiddle) : base(position, size, anchor: anchor)
        {
            _cols = cols;
            _margin = margin;
            _columnPositions = new float[cols];
            _currentContainerHeight = 0;
            _gridElements = new List<object>();
        }
        //TODO: fix issue where removing components does not update the size of the container
        public override void AddComponentChild(UIComponent component)
        {
            int i = (ComponentCount + ContainerCount) % _cols;
            component.Position = new Vector2(_columnPositions[i], _currentContainerHeight);
            if (i != _cols - 1)
            {
                _columnPositions[i + 1] = Math.Max(_columnPositions[i + 1], _columnPositions[i] + component.Size.X + _margin); 
            }
            else
            {
                _currentContainerHeight += component.Size.Y + _margin;
            }
            Size = Vector2.Max(Size, component.Position + component.Size);
            _gridElements.Add(component);
            base.AddComponentChild(component);
        }
        public override void AddContainerChild(UIContainer container)
        {
            int i = (ComponentCount + ContainerCount) % _cols;
            container.Position = new Vector2(_columnPositions[i], _currentContainerHeight);
            if (i != _cols - 1)
            {
                _columnPositions[i + 1] = Math.Max(_columnPositions[i + 1], _columnPositions[i] + container.Size.X + _margin);
            }
            else
            {
                _currentContainerHeight += container.Size.Y + _margin;
            }
            Size = Vector2.Max(Size, container.Position + container.Size);
            _gridElements.Add(container);
            base.AddContainerChild(container);
        }
        //Naive implentation: Just going to reset the container and re-add the grid items in order.
        //Its's not like I'll ever have a grid container with thousands of items that needs to be adjusted on the fly. - If so I'll come up with a better solution
        public override void RemoveComponentChild(UIComponent component)
        {
            _gridElements.Remove(component);
            base.RemoveComponentChild(component);
            RepositionAllElements();
        }
        public override void RemoveContainerChild(UIContainer container)
        {
            _gridElements.Remove(container);
            base.RemoveContainerChild(container);
            RepositionAllElements();
        }
        private void RepositionAllElements()
        {
            Size = Vector2.Zero;
            _currentContainerHeight = 0;
            for (int i = 0; i < _columnPositions.Length; i++)
            {
                _columnPositions[i] = 0;
            }
            for (int i = 0; i < _gridElements.Count; i++)
            {
                object child = _gridElements[i];
                int columnIndex = i % _cols;
                Vector2 childSize = Vector2.Zero;
                Vector2 childPosition = Vector2.Zero;
                if (child is UIContainer container)
                {
                    container.Position = new Vector2(_columnPositions[columnIndex], _currentContainerHeight);
                    childSize = container.Size;
                    childPosition = container.Position;
                }
                else if (child is UIComponent component)
                {
                    component.Position = new Vector2(_columnPositions[columnIndex], _currentContainerHeight);
                    childSize = component.Size;
                    childPosition = component.Position;
                }

                if (columnIndex != _cols - 1)
                {
                    _columnPositions[i + 1] = Math.Max(_columnPositions[i + 1], _columnPositions[i] + childSize.X + _margin);
                }
                else
                {
                    _currentContainerHeight += childSize.Y + _margin;
                }
                Size = Vector2.Max(Size, childPosition + childSize);
            }
        }
    }
}
