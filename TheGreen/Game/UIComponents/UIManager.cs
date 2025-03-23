using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TheGreen.Game.UIComponents
{
    /// <summary>
    /// Handles input and drawing for all UIComponent Containers.
    /// </summary>
    public static class UIManager
    {
        private static List<UIComponentContainer> _uiComponentContainers = new List<UIComponentContainer>();

        public static void Update(double delta)
        {
            foreach (UIComponentContainer container in _uiComponentContainers)
            {
                container.Update(delta);
            }
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (UIComponentContainer container in _uiComponentContainers)
            {
                container.Draw(spriteBatch);
            }
        }

        public static void RegisterContainer(UIComponentContainer container)
        {
            _uiComponentContainers.Add(container);
        }

        public static void UnregisterContainer(UIComponentContainer container)
        {
            _uiComponentContainers.Remove(container);
        }
    }
}
