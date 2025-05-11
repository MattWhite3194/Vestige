using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TheGreen.Game.UI.Containers;

namespace TheGreen.Game.UI
{
    /// <summary>
    /// updating and drawing for all UIComponent Containers.
    /// </summary>
    public static class UIManager
    {
        private static List<UIContainer> _uiComponentContainers = new List<UIContainer>();

        public static void Update(double delta)
        {
            for (int i = _uiComponentContainers.Count - 1; i >= 0 ; i--)
            {
                _uiComponentContainers[i].Update(delta);
            }
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _uiComponentContainers.Count; i++) 
            {
                _uiComponentContainers[i].Draw(spriteBatch);
            }
        }

        public static void RegisterContainer(UIContainer container)
        {
            _uiComponentContainers.Add(container);
            container.UpdateAnchorMatrix(TheGreen.ScreenResolution.X, TheGreen.ScreenResolution.Y);
        }

        public static void UnregisterContainer(UIContainer container)
        {
            _uiComponentContainers.Remove(container);
        }
        public static void OnUIScaleChanged(int screenWidth, int screenHeight)
        {
            for (int i = 0; i < _uiComponentContainers.Count; i++)
            {
                _uiComponentContainers[i].UpdateAnchorMatrix(TheGreen.ScreenResolution.X, TheGreen.ScreenResolution.Y);
            }
        }
    }
}
