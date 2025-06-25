using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.Input;

namespace Vestige.Game.UI.Components
{
    public class Slider : UIComponent
    {
        private string _label;
        private Vector2 _labelSize;
        private float _minValue;
        private float _maxValue;
        private float _value;
        private bool _mouseDown;
        private Vector2 _sliderSize;
        private Vector2 _sliderPosition;
        private Vector2 _sliderPositionOnGrab;
        private Vector2 _minPosition;
        private Vector2 _maxPosition;
        public event Action<float> OnValueChanged;
        private Vector2 _grabPosition;
        private string _valueModifiers;
        private string _valueString;
        public Slider(Vector2 position, Vector2 size, string label, float minValue, float maxValue, float defaultValue, string valueModifiers = "") : base(position)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _value = defaultValue;
            _minPosition = new Vector2(position.X, 0);
            _maxPosition = new Vector2(position.X + size.X, 0);
            _mouseDown = false;
            _sliderPosition = new Vector2((defaultValue - minValue) / (maxValue - minValue) * size.X, 0);
            Size = size;
            _sliderSize = size;
            _label = label;
            _labelSize = ContentLoader.GameFont.MeasureString(_label);
            _valueModifiers = valueModifiers;
            _valueString = (int)_value + _valueModifiers;
            Size.Y += _labelSize.Y + 2;
        }
        public override void HandleMouseInput(MouseInputEvent mouseEvent, Vector2 mouseCoordinates)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                SetFocused(true);
                _mouseDown = true;
                _sliderPosition = Vector2.Clamp(mouseCoordinates, _minPosition, _maxPosition);
                _sliderPositionOnGrab = _sliderPosition;
                InputManager.MarkInputAsHandled(@mouseEvent);
                _grabPosition = Vector2.Transform(InputManager.GetMouseWindowPosition(), Matrix.Invert(Vestige.UIScaleMatrix));
            }
            else if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonUp)
            {
                if (!_mouseDown)
                    return;
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
                _sliderPosition = Vector2.Clamp(_sliderPositionOnGrab + offset, _minPosition, _maxPosition);
                float range = _maxValue - _minValue;
                _value = ((_sliderPosition.X - Position.X) / Size.X * range) + _minValue;
                _valueString = (int)_value + _valueModifiers;
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Utilities.DrawFilledRectangle(spriteBatch, new Rectangle(Position.ToPoint(), _sliderSize.ToPoint()), Vestige.UIPanelColor);
            Utilities.DrawFilledRectangle(spriteBatch, new Rectangle(Position.ToPoint(), new Point((int)_sliderPosition.X, (int)_sliderSize.Y)), Vestige.HighlightedTextColor);
            Utilities.DrawOutlineRectangle(spriteBatch, new Rectangle(Position.ToPoint(), _sliderSize.ToPoint()), Color.Black, lineWidth: 2);
            Utilities.DrawFilledRectangle(spriteBatch, new Rectangle((Position + new Vector2(_sliderPosition.X - 2, -2)).ToPoint(), new Point(4, (int)_sliderSize.Y + 4)), Color.White);
            //Utilities.DrawOutlineRectangle(spriteBatch, new Rectangle((Position + new Vector2(_sliderPosition.X - 2, -2)).ToPoint(), new Point(4, (int)_sliderSize.Y + 4)), Color.Black, lineWidth: 1);
            spriteBatch.DrawString(ContentLoader.GameFont, _label, Position + new Vector2(0, _sliderSize.Y + 2), Color.White);
            spriteBatch.DrawString(ContentLoader.GameFont, _valueString, Position + new Vector2(_labelSize.X + 5, _sliderSize.Y + 2), Color.White);
        }
    }
}
