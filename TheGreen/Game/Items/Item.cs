using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        public float Scale = 1f;
        
        public readonly UseStyle UseStyle;

        protected Item(int id, string name, string description, Texture2D image, bool stackable, double useSpeed, bool autoUse, UseStyle useStyle = UseStyle.Swing)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Image = image;
            this.Stackable = stackable;
            this.UseSpeed = useSpeed;
            this.AutoUse = autoUse;
            this.UseStyle = useStyle;
        }

        public virtual bool UseItem()
        {
            return true;
        }

        /// <summary>
        /// Override this for any custom drawing effects
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="drawPosition"></param>
        public virtual void Draw(SpriteBatch spriteBatch, Vector2 drawPosition) { }
    }
    public enum UseStyle
    {
        None,
        Swing,
        Hold,
        Point
    }
}
