using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Vestige.Game.Input;
using Vestige.Game.UI.Components;

namespace Vestige.Game.UI.Containers
{
    public class SelectionContainer : GridContainer
    {
        private object _selected;
        private int _selectedIndex = 0;
        private Color _buttonColor;
        private Color _buttonSelectedColor;
        private List<(object selection, string label)> _selections;
        public SelectionContainer(int cols, List<(object selection, string label)> selections, Color buttonColor, Color buttonSelectedColor, Color buttonHoveredColor, int buttonWidth = 50, int margin = 5, Vector2 position = default, Vector2 size = default, int defaultSelected = 0, Anchor anchor = Anchor.MiddleMiddle) : base(cols, margin, position, size, anchor)
        {
            _buttonColor = buttonColor;
            _buttonSelectedColor = buttonSelectedColor;
            _selections = selections;
            for (int i = 0; i < selections.Count; i++)
            {
                int index = i;
                Label label = new Label(Vector2.Zero, selections[i].label, Vector2.Zero, color: buttonColor, maxWidth: buttonWidth);
                label.OnMouseInput += (@mouseEvent, mouseCoordinates) => OnSelectionLabelInput(@mouseEvent, index);
                label.OnMouseEntered += () =>
                {
                    label.Color = _selectedIndex == index ? label.Color : buttonHoveredColor;
                };
                label.OnMouseExited += () =>
                {
                    label.Color = _selectedIndex == index ? label.Color : buttonColor;
                };
                AddComponentChild(label);
            }
            if (defaultSelected >= selections.Count)
                defaultSelected = 0;
            if (selections.Count > 0)
            {
                _selected = selections[defaultSelected].selection;
                _selectedIndex = defaultSelected;
                GetComponentChild(0).Color = buttonSelectedColor;
            }
        }
        private void OnSelectionLabelInput(MouseInputEvent @mouseEvent, int index)
        {
            if (mouseEvent.InputButton == InputButton.LeftMouse && mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                GetComponentChild(_selectedIndex).Color = _buttonColor;
                _selectedIndex = index;
                GetComponentChild(index).Color = _buttonSelectedColor;
                _selected = _selections[index].selection;
            }
        }
        public object GetSelected()
        {
            return _selected;
        }
    }
}
