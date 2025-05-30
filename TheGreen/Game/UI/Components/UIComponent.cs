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
        public Color Color;
        //for textbox implementation
        private bool focused = false;
        private Vector2 _position;
        public virtual Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public Vector2 Size;
        public Vector2 Origin;
        public bool MouseInside = false;
        protected float _rotation;
        public float Scale;

        public UIComponent(Vector2 position, Texture2D image = null, Color color = default, float rotation = 0.0f, float scale = 1.0f, Vector2 origin = default)
        {
            Position = position;
            this.image = image;
            this.Color = color == default ? Color.White : color;
            Origin = origin;
            _rotation = rotation;
            Scale = scale;
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
            spriteBatch.Draw(image, Position + Origin, null, Color, _rotation, Origin, Scale, SpriteEffects.None, 0.0f);
        }

        public bool IsFocused()
        {
            return focused;
        }

        public void SetFocused(bool isFocused)
        {
            focused = isFocused;
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
            return new Rectangle((int)Position.X, (int)Position.Y, (int)(Size.X * Scale), (int)(Size.Y * Scale));
        }
    }
}
