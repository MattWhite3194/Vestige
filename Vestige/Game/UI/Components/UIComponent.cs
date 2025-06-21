using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.Input;

namespace Vestige.Game.UI.Components
{
    public abstract class UIComponent
    {
        public event Action<InputEvent> OnGuiInput;
        public event Action<MouseInputEvent, Vector2> OnMouseInput;
        public event Action OnMouseEntered;
        public event Action OnMouseExited;
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

        public virtual void SetFocused(bool isFocused)
        {
            focused = isFocused;
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

        public virtual void HandleGuiInput(InputEvent @event)
        {
            OnGuiInput?.Invoke(@event);
        }

        public virtual void HandleMouseInput(MouseInputEvent @mouseEvent, Vector2 mouseCoordinates)
        {
            OnMouseInput?.Invoke(@mouseEvent, mouseCoordinates);
        }

        public virtual void HandleMouseEntered()
        {
            OnMouseEntered?.Invoke();
        }

        public virtual void HandleMouseExited()
        {
            OnMouseExited?.Invoke();
        }

        public virtual Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)(Size.X * Scale), (int)(Size.Y * Scale));
        }
    }
}
