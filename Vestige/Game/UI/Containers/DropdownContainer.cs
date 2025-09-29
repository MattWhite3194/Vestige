using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vestige.Game.Input;
using Vestige.Game.UI.Components;

namespace Vestige.Game.UI.Containers
{
    public class DropdownContainer : UIContainer
    {
        private object _selected;
        private int _selectedIndex = 0;
        private Color _buttonColor;
        private Color _buttonSelectedColor;
        private List<(object selection, string label)> _selections;
        private GridContainer _dropdownMenu;
        private Button _dropdownToggleButton;
        private PanelContainer _dropdownBackground;
        public Action<object> OnSelectionChanged;
        public DropdownContainer(List<(object selection, string label)> selections, Color buttonColor, Color buttonSelectedColor, Color buttonHoveredColor, int buttonWidth = 50, int margin = 5, Vector2 position = default, Vector2 size = default, object defaultSelected = null, bool drawPanel = false, GraphicsDevice graphicsDevice = null, Anchor anchor = Anchor.MiddleMiddle) : base(position, size, anchor)
        {
            _buttonColor = buttonColor;
            _buttonSelectedColor = buttonSelectedColor;
            _selections = selections;
            if (selections.Count > 0)
            {
                if (defaultSelected == null)
                {
                    defaultSelected = selections[0].selection;
                    _selectedIndex = 0;
                }
                else
                {
                    for (int i = 0; i < _selections.Count; i++)
                    {
                        if (selections[i].selection == defaultSelected)
                        {
                            _selected = selections[i].selection;
                            _selectedIndex = i;
                            break;
                        }
                    }
                }
            }
            _dropdownToggleButton = new Button(new Vector2(0, 0), _selections[_selectedIndex].label, Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: buttonWidth);
            _dropdownMenu = new GridContainer(1, margin: margin, position: new Vector2(0, _dropdownToggleButton.Size.Y + margin), anchor: Anchor.TopLeft);
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
                _dropdownMenu.AddComponentChild(label);
            }
            if (drawPanel)
            {
                _dropdownBackground = new PanelContainer(new Vector2(0, _dropdownToggleButton.Size.Y + margin), _dropdownMenu.Size, Vestige.UIPanelColor, new Color(0, 0, 0, 255), 5, 1, 10, graphicsDevice, anchor: Anchor.TopLeft);
            }
            _dropdownToggleButton.OnButtonPress += () =>
            {
                if (ContainerCount > 0)
                {
                    RemoveContainerChild(_dropdownBackground);
                    RemoveContainerChild(_dropdownMenu);
                }
                else
                {
                    AddContainerChild(_dropdownBackground);
                    AddContainerChild(_dropdownMenu);
                }
            };
            _dropdownMenu.GetComponentChild(_selectedIndex).Color = buttonSelectedColor;
            AddComponentChild(_dropdownToggleButton);
        }
        private void OnSelectionLabelInput(MouseInputEvent @mouseEvent, int index)
        {
            if (mouseEvent.InputButton == InputButton.LeftMouse && mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                _dropdownMenu.GetComponentChild(_selectedIndex).Color = _buttonColor;
                _selectedIndex = index;
                _dropdownMenu.GetComponentChild(index).Color = _buttonSelectedColor;
                _selected = _selections[index].selection;
                _dropdownToggleButton.SetText(_selections[index].label);
                RemoveContainerChild(_dropdownMenu);
                OnSelectionChanged?.Invoke(_selected);
            }
        }
        public object GetSelected()
        {
            return _selected;
        }
    }
}
