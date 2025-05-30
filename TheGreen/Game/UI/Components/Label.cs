using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace TheGreen.Game.UI.Components
{
    public class Label : UIComponent
    {
        protected string _text;
        private Vector2 _padding;
        private Vector2 _stringOrigin;
        protected Vector2 _stringPosition;
        protected Vector2 _stringSize;
        private int _maxWidth;
        private TextAlign _textAlign;
        public override Vector2 Position
        {
            get 
            { 
                return base.Position;
            }
            set
            {
                base.Position = value;
                UpdateStringPosition();
            }
        }
        public Label(Vector2 position, string text, Vector2 padding, Color color = default,
            int maxWidth = 0, float rotation = 0.0f, float scale = 1.0f, TextAlign textAlign = TextAlign.Center) : base(position, null, color, rotation, scale)
        {
            this.Color = color == default ? Color.White : color;
            _padding = padding;
            _maxWidth = maxWidth;
            _textAlign = textAlign;
            SetText(text);
        }

        private void UpdateStringPosition()
        {
            if (_text == null) return;
            _stringPosition.Y = Position.Y + Size.Y / 2 - _stringSize.Y / 2;
            switch (_textAlign)
            {
                case TextAlign.Left:
                    _stringPosition.X = Position.X + _padding.X;
                    Origin = new Vector2(0, Size.Y / 2);
                    _stringOrigin = new Vector2(0, _stringSize.Y / 2);
                    break;
                case TextAlign.Center:
                    _stringPosition.X = Position.X + Size.X / 2 - _stringSize.X / 2;
                    Origin = Size / 2;
                    _stringOrigin = _stringSize / 2.0f;
                    break;
                case TextAlign.Right:
                    _stringPosition.X = Position.X + Size.X - _padding.X - _stringSize.X;
                    Origin = new Vector2(Size.X, Size.Y / 2);
                    _stringOrigin = new Vector2(_stringSize.X, _stringSize.Y / 2);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <returns>The dimensions of the wrappend string</returns>
        private Vector2 WrapText(int maxWidth)
        {
            _stringSize = ContentLoader.GameFont.MeasureString(_text);
            if (_stringSize.Y == 0)
            {
                _stringSize.Y = ContentLoader.GameFont.MeasureString("A").Y;
            }
            if (maxWidth == 0)
                return _stringSize;
            if (_stringSize.X <= maxWidth)
                return new Vector2(maxWidth, _stringSize.Y);
            float characterWidth = _stringSize.X / _text.Length;
            int charsPerLine = (int)(maxWidth / characterWidth);
            string newText = "";
            int textIndex = 0;
            int numLines = 1;
            while (textIndex < _text.Length)
            {
                newText += _text[textIndex];
                if ((textIndex + 1) % charsPerLine == 0)
                {
                    numLines++;
                    newText += "\n";
                }
                textIndex++;
            }
            _text = newText;
            _stringSize = new Vector2(maxWidth, numLines * _stringSize.Y);
            return _stringSize;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //TODO: possibly move the custom logic to a new spritefont class with it's own draw function
            if (image != null)
            {
                spriteBatch.Draw(image, Position + Origin, null, Color, _rotation, Origin, Scale, SpriteEffects.None, 0.0f);
            }
            spriteBatch.DrawString(ContentLoader.GameFont, _text, _stringPosition + _stringOrigin, Color, _rotation, _stringOrigin, Scale, SpriteEffects.None, 0.0f);

        }

        public void SetText(string text)
        {
            _text = text;
            Size = WrapText(_maxWidth);
            UpdateStringPosition();
        }
    }
}
