using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGreen.Game.Input;

namespace TheGreen.Game.UIComponents
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
            Color color = default(Color), Color clickedColor = default(Color), Color hoveredColor = default(Color), 
            Color textColor = default(Color), Color textClickedColor = default(Color), Color textHoveredColor = default(Color), 
            GraphicsDevice graphicsDevice = null, Texture2D image = null, bool drawCentered = false, int maxWidth = 200) : base(position, text, padding, borderRadius, color, textColor, graphicsDevice, image, drawCentered, maxWidth)
        {
            this._clickedColor = clickedColor;
            this._hoveredColor = hoveredColor;
            this._textClickedColor = textClickedColor;
            this._textHoveredColor = textHoveredColor;
            this._defaultColor = color;
            this._defaultTextColor = textColor;
            this.OnMouseEntered += () => HoverButton();
            this.OnMouseExited += () => ResetButton();
        }

        protected override void HandleGuiInput(InputEvent @event)
        {
            if (@event.InputButton == InputButton.LeftMouse && @event.EventType == InputEventType.MouseButtonDown)
            {
                _textColor = _textClickedColor;
                color = _clickedColor;
                _clicked = true;
            }
            else if (@event.InputButton == InputButton.LeftMouse && @event.EventType == InputEventType.MouseButtonUp)
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
            _textColor = _defaultTextColor;
            color = _defaultColor;
        }

        private void HoverButton()
        {
            color = _hoveredColor;
            _textColor = _textHoveredColor;
        }
    }
}
