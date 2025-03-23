using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace TheGreen.Game.Items
{
    /// <summary>
    /// Used by the inventory. Describes an Items functionality.
    /// </summary>
    public abstract class Item
    {
        /*
        Saving to file:
        id, name, description, image (stored in dictionary), quantity (if applicable), type, usespeed
        store other related attributes based on type
        instantiate the appropriate subclass based on type, add the attributes
        */
        public int ID;
        public string Name;
        public string Description;
        public Texture2D Image;
        public int Quantity;
        public bool Stackable;
        public double UseSpeed;
        public bool AutoUse;
        public bool Active;
        private double _holdTime;
        private float _rotation = 0.0f;
        private float _scale = 1f;
        private bool _leftReleased = false;

        public virtual void OnLeftPressed() 
        {
            _holdTime = UseSpeed;
            _leftReleased = false;
            Active = true;
        }
        public virtual void OnLeftReleased() 
        {
            _leftReleased = true;
        }

        public virtual bool Update(double delta)
        {
            if (!Active)
                return false;
            bool itemUsed = false;
            _holdTime += delta;
            if (_holdTime >= UseSpeed)
            {
                
                if (UseItem())
                {
                    _holdTime = 0.0f;
                    if (Stackable)
                        Quantity -= 1;
                    itemUsed = true;
                }
                else
                {
                    _holdTime = UseSpeed;
                }
            }
            _rotation += (float)(MathHelper.PiOver2 * (delta / UseSpeed));
            if (_rotation > MathHelper.PiOver2)
            {
                if (!AutoUse || _leftReleased)
                {
                    Active = false;
                }
                _rotation = 0.0f;
            }
            return itemUsed;
        }

        public virtual bool UseItem()
        {
            return true;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, bool flipped)
        {
            Vector2 bottomPosition = new Vector2(position.X, position.Y + Image.Height);
            spriteBatch.Draw(Image, 
                new Vector2((int)position.X, (int)position.Y) + (flipped? new Vector2(12, 0) : new Vector2(8, 0)), 
                null, 
                Color.White, 
                flipped? -_rotation : _rotation, 
                flipped? new Vector2(Image.Width + 8, Image.Height) : new Vector2(-8, Image.Height), 
                _scale, 
                flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 
                0f
            );
        }
    }
}
