using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vestige.Game.Input;

namespace Vestige.Game.UI.Components
{
    public class Slider : UIComponent
    {
        private float _minValue;
        private float _maxValue;
        private float _value;
        private bool _mouseDown;
        private Vector2 _sliderPosition;
        private Vector2 _minPosition;
        private Vector2 _maxPosition;
        public delegate void ValueChanged(float value);
        public ValueChanged OnValueChanged;
        private Vector2 _grabPosition;
        public Slider(Vector2 position, Vector2 size, float minValue, float maxValue, float defaultValue) : base(position)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _value = defaultValue;
            _minPosition = new Vector2(position.X, 0);
            _maxPosition = new Vector2(position.X + size.X, 0);
            _mouseDown = false;
            _sliderPosition = new Vector2(defaultValue / (maxValue - minValue) * size.X, 0);
            Size = size;
        }
        protected override void HandleMouseInput(MouseInputEvent mouseEvent, Vector2 mouseCoordinates)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                SetFocused(true);
                _mouseDown = true;
                _sliderPosition = Vector2.Clamp(mouseCoordinates, _minPosition, _maxPosition);
                InputManager.MarkInputAsHandled(@mouseEvent);
                _grabPosition = Vector2.Transform(InputManager.GetMouseWindowPosition(), Matrix.Invert(Vestige.UIScaleMatrix));
            }
            else if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonUp)
            {
                _mouseDown = false;
                SetFocused(false);
                OnValueChanged?.Invoke(_value);
                InputManager.MarkInputAsHandled(@mouseEvent);
            }
        }
        public override void Update(double delta)
        {
            if (_mouseDown)
            {
                Vector2 currentMousePos = Vector2.Transform(InputManager.GetMouseWindowPosition(), Matrix.Invert(Vestige.UIScaleMatrix));
                Vector2 offset = currentMousePos - _grabPosition;
                _sliderPosition = Vector2.Clamp(_sliderPosition + offset, _minPosition, _maxPosition);
                _grabPosition = currentMousePos;
                float range = _maxValue - _minValue;
                _value = (_sliderPosition.X - Position.X) / Size.X * range + _minValue;
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            DebugHelper.DrawFilledRectangle(spriteBatch, new Rectangle(Position.ToPoint(), Size.ToPoint()), Vestige.UIPanelColor);
            DebugHelper.DrawFilledRectangle(spriteBatch, new Rectangle((Position + _sliderPosition).ToPoint(), new Point(5, (int)Size.Y)), Vestige.HighlightedTextColor);
            spriteBatch.DrawString(ContentLoader.GameFont, (int)_value + "", Position + new Vector2(0, Size.Y + 2), Color.White);
        }
    }
}
