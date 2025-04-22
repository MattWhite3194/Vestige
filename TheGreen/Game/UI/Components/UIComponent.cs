using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;

namespace TheGreen.Game.UI.Components
{
    public abstract class UIComponent
    {
        public delegate void GuiInput(InputEvent @event);
        public GuiInput OnGuiInput;
        public delegate void MouseInput(MouseInputEvent @mouseEvent, Vector2 mouseCoordinates);
        public MouseInput OnMouseInput;
        public delegate void MouseEntered();
        public delegate void MouseExited();
        public MouseEntered OnMouseEntered;
        public MouseExited OnMouseExited;
        protected bool hidden = false;
        protected Texture2D image;
        protected Color color = Color.White;
        //for textbox implementation
        private bool focused = false;
        protected Vector2 _drawPosition;
        private Vector2 _position;
        private Vector2 _anchorOrigin;
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdateDrawPosition();
            }
        }
        private Vector2 _size;
        public Vector2 Size
        {
            get { return _size; }
            set
            {
                _size = value;
                Origin = _size / 2.0f;
                UpdateDrawPosition();
            }
        }
        public Vector2 Origin;
        public bool MouseInside = false;
        protected GraphicsDevice _graphicsDevice;
        protected bool _drawCentered;
        protected float _rotation;
        protected float _scale;
        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                UpdateDrawPosition();
            }
        }

        public UIComponent(Vector2 position, Texture2D image = null, Color color = default, GraphicsDevice graphicsDevice = null, bool drawCentered = false, float rotation = 0.0f, float scale = 1.0f)
        {
            _position = position;
            this.image = image;
            this.color = color;
            _graphicsDevice = graphicsDevice;
            _drawCentered = drawCentered;
            _rotation = rotation;
            _scale = scale;
            _drawPosition = _drawCentered ? _position - Origin * scale : _position;
            if (image != null)
                Size = new Vector2(image.Width, image.Height);
            else
                Size = Vector2.Zero;
            OnGuiInput += HandleGuiInput;
            OnMouseInput += HandleMouseInput;
            OnMouseEntered += () => HandleMouseEntered();
            OnMouseExited += () => HandleMouseExited();
        }

        public virtual void Update(double delta) { }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(image, _drawPosition, null, color, 0.0f, _anchorOrigin, _scale, SpriteEffects.None, 0.0f);
        }

        public bool IsFocused()
        {
            return focused;
        }

        public void SetFocused(bool isFocused)
        {
            focused = isFocused;
        }

        public void SetColor(Color color)
        {
            this.color = color;
        }

        protected virtual void UpdateDrawPosition()
        {
            _drawPosition = _drawCentered ? _position - Origin * _scale : _position;
        }

        /// <summary>
        /// Called when the UIComponents position is changed.
        /// </summary>
        protected virtual void HandlePositionUpdate()
        {

        }

        public virtual void Hide()
        {
            hidden = true;
        }

        public virtual void Show()
        {
            hidden = false;
        }

        public virtual bool IsVisible()
        {
            //to be honest this doesn't make a lot of sense since it's a private variable
            return !hidden;
        }

        protected virtual void HandleGuiInput(InputEvent @event)
        {

        }

        protected virtual void HandleMouseInput(MouseInputEvent @mouseEvent, Vector2 mouseCoordinates)
        {

        }

        protected virtual void HandleMouseEntered()
        {

        }

        protected virtual void HandleMouseExited()
        {

        }

        public virtual Rectangle GetBounds()
        {
            return new Rectangle((int)_drawPosition.X, (int)_drawPosition.Y, (int)(_size.X * _scale), (int)(_size.Y * _scale));
        }
    }
}
