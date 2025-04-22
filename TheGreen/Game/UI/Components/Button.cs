using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;

namespace TheGreen.Game.UI.Components
{

    //TODO: implement rounded corners and border color
    internal class Button : Label
    {
        public delegate void ButtonPress();
        public ButtonPress OnButtonPress;
        private Color _clickedColor;
        private Color _hoveredColor;
        private Color _textClickedColor;
        private Color _textHoveredColor;
        private Color _defaultColor;
        private Color _defaultTextColor;
        private bool _clicked = false;
        public Button(Vector2 position, string text, Vector2 padding, int borderRadius = 0,
            Color color = default, Color clickedColor = default, Color hoveredColor = default,
            Color textColor = default, Color textClickedColor = default, Color textHoveredColor = default,
            GraphicsDevice graphicsDevice = null, bool drawCentered = false, int maxWidth = 0, float scale = 1.0f, TextAlign textAlign = TextAlign.Center) : base(position, text, padding, borderRadius, color, textColor, graphicsDevice, drawCentered, maxWidth, scale: scale, textAlign: textAlign)
        {
            _clickedColor = clickedColor;
            _hoveredColor = hoveredColor;
            _textClickedColor = textClickedColor;
            _textHoveredColor = textHoveredColor;
            _defaultColor = color;
            _defaultTextColor = textColor;
            OnMouseEntered += HoverButton;
            OnMouseExited += ResetButton;
        }

        protected override void HandleMouseInput(MouseInputEvent @mouseEvent, Vector2 mouseCoordinates)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                _textColor = _textClickedColor;
                color = _clickedColor;
                _clicked = true;
            }
            else if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonUp)
            {
                if (_clicked)
                {
                    OnButtonPress();
                }
                _textColor = _textHoveredColor;
                _clicked = false;
            }
        }

        private void ResetButton()
        {
            _clicked = false;
            Scale = Scale - 0.2f;
            _textColor = _defaultTextColor;
            color = _defaultColor;
        }

        private void HoverButton()
        {
            color = _hoveredColor;
            Scale = Scale + 0.2f;
            _textColor = _textHoveredColor;
        }
        public override Rectangle GetBounds()
        {
            return new Rectangle(_stringPosition.ToPoint(), _stringSize.ToPoint());
        }
    }
}
