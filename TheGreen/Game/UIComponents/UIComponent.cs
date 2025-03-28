using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using TheGreen.Game.Input;

namespace TheGreen.Game.UIComponents
{
    public abstract class UIComponent
    {
        public delegate void GuiInput(InputEvent @event);
        public GuiInput OnGuiInput;
        public delegate void PositionChanged();
        public PositionChanged OnPositionChanged;
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
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdateDrawPosition();
                OnPositionChanged.Invoke();
            }
        }
        private Vector2 _size;
        public Vector2 Size {
            get { return _size; }
            set
            {
                _size = value;
                _origin = _size / 2.0f;
                UpdateDrawPosition();
            }
        }
        protected Vector2 _origin;
        public bool MouseInside = false;
        protected GraphicsDevice _graphicsDevice;
        protected bool _drawCentered;
        protected float _rotation;
        protected float _scale;

        public UIComponent(Vector2 position, Texture2D image = null, Color color = default(Color), GraphicsDevice graphicsDevice = null, bool drawCentered = false, float rotation = 0.0f, float scale = 0.0f)
        {
            this._position = position;
            this.image = image;
            this.color = color;
            this._graphicsDevice = graphicsDevice;
            this._drawCentered = drawCentered;
            this._rotation = rotation;
            this._scale = scale;
            _drawPosition = _drawCentered ? _position - _origin : _position;
            if (image != null)
                Size = new Vector2(image.Width, image.Height);
            else
                Size = Vector2.Zero;
            OnGuiInput += (@event) => HandleGuiInput(@event);
            OnMouseEntered += () => HandleMouseEntered();
            OnMouseExited += () => HandleMouseExited();
            OnPositionChanged += () => HandlePositionUpdate();
        }

        public virtual void Update(double delta) { }

        public virtual void Draw(SpriteBatch spriteBatch) 
        {
            spriteBatch.Draw(image, _drawPosition, color);
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

        private void UpdateDrawPosition()
        {
            _drawPosition = _drawCentered ? _position - _origin : _position;
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

        protected virtual void HandleMouseEntered()
        {

        }

        protected virtual void HandleMouseExited()
        {

        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)_drawPosition.X, (int)_drawPosition.Y, (int)_size.X, (int)_size.Y);
        }

        //TODO: implement something like this and add it to a global window resize delegate or something similar
        public void OnWindowResize(Point oldResolution, Point newResolution)
        {
            Vector2 newPosition;
            newPosition.X = (int)(Position.X * (newResolution.X / (float)oldResolution.X));
            newPosition.Y = (int)(Position.Y * (newResolution.Y / (float)oldResolution.Y));
            Position = newPosition;
        }
    }
}
