using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Vestige.Game.Input;

namespace Vestige.Game.UI.Components
{
    internal class TextBox : Label
    {
        private string _placeHolder;
        private bool _usingPlaceHolder = false;
        private string _textCursor = "|";
        private bool _drawTextCursor = false;
        private int _cursorIndex;
        private int _maxTextLength;
        private double _elapsedTime = 0.0;
        private double _cursorHideTime = 0.3;
        public event Action OnEnterPressed;
        public event Action OnEscapePressed;
        public TextBox(Vector2 position, string defaultText, Vector2 padding, int maxTextLength = -1, string placeHolder = null, int maxWidth = 0, TextAlign textAlign = TextAlign.Center) : base(position, defaultText, padding, maxWidth: maxWidth, textAlign: textAlign)
        {
            _maxTextLength = maxTextLength;
            _placeHolder = placeHolder != null ? placeHolder : "Text";
            if (defaultText == "")
            {
                SetText(_placeHolder);
                Color = Color.LightGray;
                _usingPlaceHolder = true;
            }
            OnMouseEntered += () => Mouse.SetCursor(MouseCursor.IBeam);
            OnMouseExited += () => Mouse.SetCursor(MouseCursor.Arrow);
        }

        public override void HandleMouseInput(MouseInputEvent @mouseEvent, Vector2 mouseCoordinates)
        {
            if (@mouseEvent.EventType == InputEventType.MouseButtonDown && @mouseEvent.InputButton == InputButton.LeftMouse)
            {
                if (!MouseInside)
                {
                    SetFocused(false);
                }
                else
                {
                    if (!IsFocused())
                    {
                        SetFocused(true);
                    }
                    else
                    {
                        //cursor positioning is rounded so the cursor will position to whatever character edge the mouse is closest to.
                        //Specifically so clicking between two characters results in that position.
                        _cursorIndex = (int)Math.Round((mouseCoordinates.X - _stringPosition.X) / ContentLoader.GameFont.MeasureString("A").X);
                        if (_cursorIndex < 0) _cursorIndex = 0;
                        if (_cursorIndex > _text.Length) _cursorIndex = _text.Length;
                    }
                    InputManager.MarkInputAsHandled(@mouseEvent);
                }
            }
        }
        public override void SetFocused(bool isFocused)
        {
            if (isFocused)
            {
                if (_usingPlaceHolder)
                {
                    _usingPlaceHolder = false;
                    SetText("");
                    Color = Color.White;
                }
                Vestige.GameWindow.TextInput += OnTextInput;
                Vestige.GameWindow.KeyDown += OnKeyDown;
                _cursorIndex = _text.Length;
            }
            else
            {
                _drawTextCursor = false;
                _elapsedTime = 0.0;
                Vestige.GameWindow.TextInput -= OnTextInput;
                Vestige.GameWindow.KeyDown -= OnKeyDown;
                if (_text == "")
                {
                    SetText(_placeHolder);
                    Color = Color.LightGray;
                    _usingPlaceHolder = true;
                }
            }
            base.SetFocused(isFocused);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_drawTextCursor)
            {
                Vector2 cursorPosition = _stringPosition + new Vector2(ContentLoader.GameFont.MeasureString(_text.Substring(0, _cursorIndex)).X, 0);
                spriteBatch.DrawString(ContentLoader.GameFont, _textCursor, cursorPosition + Origin, Color, _rotation, Origin, Scale, SpriteEffects.None, 0.0f);
            }
        }
        public override void Update(double delta)
        {
            base.Update(delta);
            if (IsFocused())
            {
                _elapsedTime += delta;
                if (_elapsedTime > _cursorHideTime)
                {
                    _drawTextCursor = !_drawTextCursor;
                    _elapsedTime = 0.0;
                }
            }
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Back:
                    if (_cursorIndex > 0)
                    {
                        SetText(_text.Substring(0, _cursorIndex - 1) + _text.Substring(_cursorIndex));
                        _cursorIndex -= 1;
                    }
                    return;
                case Keys.Enter:
                    OnEnterPressed?.Invoke();
                    SetFocused(false);
                    return;
                case Keys.Escape:
                    OnEscapePressed?.Invoke();
                    SetFocused(false);
                    return;
                case Keys.Tab:
                    return;
                default:
                    if (_maxTextLength == -1 || _text.Length < _maxTextLength)
                    {
                        SetText(_text.Substring(0, _cursorIndex) + e.Character + (_cursorIndex < _text.Length ? _text.Substring(_cursorIndex) : ""));
                        _cursorIndex += 1;
                    }
                    return;
            }
        }
        private void OnKeyDown(object sender, InputKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Right:
                    _drawTextCursor = true;
                    _cursorIndex = Math.Min(_text.Length, _cursorIndex + 1);
                    return;
                case Keys.Left:
                    _drawTextCursor = true;
                    _cursorIndex = Math.Max(0, _cursorIndex - 1);
                    return;
            }
        }
        public string GetText()
        {
            return _usingPlaceHolder ? "" : _text;
        }
    }
}
