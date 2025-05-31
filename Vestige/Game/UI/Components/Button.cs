using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vestige.Game.Input;

namespace Vestige.Game.UI.Components
{

    //TODO: implement rounded corners and border color
    internal class Button : Label
    {
        public delegate void ButtonPress();
        public ButtonPress OnButtonPress;
        private Color _clickedColor;
        private Color _hoveredColor;
        private Color _defaultColor;
        private bool _clicked = false;
        public Button(Vector2 position, string text, Vector2 padding,
            Color color = default, Color clickedColor = default, Color hoveredColor = default,
            int maxWidth = 0, float scale = 1.0f, TextAlign textAlign = TextAlign.Center) : base(position, text, padding, color, maxWidth, scale: scale, textAlign: textAlign)
        {
            _clickedColor = clickedColor;
            _hoveredColor = hoveredColor;
            _defaultColor = color;
            OnMouseEntered += HoverButton;
            OnMouseExited += ResetButton;
        }

        protected override void HandleMouseInput(MouseInputEvent @mouseEvent, Vector2 mouseCoordinates)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                Color = _clickedColor;
                _clicked = true;
                InputManager.MarkInputAsHandled(@mouseEvent);
            }
            else if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonUp)
            {
                if (_clicked)
                {
                    OnButtonPress();
                }
                Color = _hoveredColor;
                _clicked = false;
                InputManager.MarkInputAsHandled(@mouseEvent);
            }
        }

        private void ResetButton()
        {
            _clicked = false;
            Scale = Scale - 0.2f;
            Color = _defaultColor;
        }

        private void HoverButton()
        {
            Color = _hoveredColor;
            Scale = Scale + 0.2f;
        }
        public override Rectangle GetBounds()
        {
            return new Rectangle(_stringPosition.ToPoint(), _stringSize.ToPoint());
        }
    }
}
