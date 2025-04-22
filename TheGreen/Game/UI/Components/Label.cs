using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Linq;

namespace TheGreen.Game.UI.Components
{
    public class Label : UIComponent
    {
        protected string _text;
        private int _borderRadius;
        private int _borderThickness;
        private Vector2 _padding;
        private Vector2 _stringOrigin;
        private Color _borderColor;
        protected Color _textColor;
        protected Vector2 _stringPosition;
        protected Vector2 _stringSize;
        private int _maxWidth;
        private TextAlign _textAlign;
        public Label(Vector2 position, string text, Vector2 padding, int borderRadius = 0, Color color = default, Color textColor = default,
            GraphicsDevice graphicsDevice = null, bool drawCentered = false, int maxWidth = 0, float rotation = 0.0f, float scale = 1.0f, TextAlign textAlign = TextAlign.Center) : base(position, null, color, graphicsDevice, drawCentered, rotation, scale)
        {
            this.color = color;
            _textColor = textColor == default ? Color.White : textColor;
            _padding = padding;
            _borderRadius = borderRadius;
            _maxWidth = maxWidth;
            _textAlign = textAlign;

            SetText(text);
        }

        protected override void UpdateDrawPosition()
        {
            base.UpdateDrawPosition();
            if (_text == null) return;
            _stringPosition.Y = _drawPosition.Y + Size.Y / 2 - _stringSize.Y / 2;
            switch (_textAlign)
            {
                case TextAlign.Left:
                    _stringPosition.X = _drawPosition.X + _padding.X;
                    Origin = new Vector2(0, Size.Y / 2);
                    _stringOrigin = new Vector2(0, _stringSize.Y / 2);
                    break;
                case TextAlign.Center:
                    _stringPosition.X = _drawPosition.X + Size.X / 2 - _stringSize.X / 2;
                    Origin = Size / 2;
                    _stringOrigin = _stringSize / 2.0f;
                    break;
                case TextAlign.Right:
                    _stringPosition.X = _drawPosition.X + Size.X - _padding.X - _stringSize.X;
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
            if (_stringSize.X < maxWidth)
                return new Vector2(maxWidth, _stringSize.Y);
            float characterWidth = _stringSize.X / _text.Length;
            int charsPerLine = (int)(maxWidth / characterWidth);
            string newText = "";
            int textIndex = 0;
            while (textIndex < _text.Length)
            {
                newText += _text[textIndex];
                if ((textIndex + 1) % charsPerLine == 0)
                    newText += "\n";
                textIndex++;
            }
            _text = newText;
            return new Vector2(maxWidth, ContentLoader.GameFont.MeasureString(newText).Y);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //TODO: possibly move the custom logic to a new spritefont class with it's own draw function
            if (image != null)
            {
                spriteBatch.Draw(image, _drawPosition, null, color, _rotation, Origin, _scale, SpriteEffects.None, 0.0f);
            }

            spriteBatch.DrawString(ContentLoader.GameFont, _text, _stringPosition + _stringOrigin, _textColor, _rotation, _stringOrigin, _scale, SpriteEffects.None, 0.0f);

        }

        public void SetText(string text)
        {
            _text = text;
            if (_graphicsDevice != null)
            {
                Vector2 imageSize = Vector2.Add(WrapText(_maxWidth), 2 * _padding);
                image = new Texture2D(_graphicsDevice, (int)imageSize.X, (int)imageSize.Y);
                var data = Enumerable.Repeat(Color.White, (int)(imageSize.X * imageSize.Y)).ToArray();
                image.SetData(data);
                Size = imageSize;
            }
            else
            {
                Size = WrapText(_maxWidth);
            }
        }
    }
}
