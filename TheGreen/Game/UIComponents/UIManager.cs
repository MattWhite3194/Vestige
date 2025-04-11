using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace TheGreen.Game.UIComponents
{
    /// <summary>
    /// updating and drawing for all UIComponent Containers.
    /// </summary>
    public static class UIManager
    {
        private static List<UIComponentContainer> _uiComponentContainers = new List<UIComponentContainer>();
        private static Point _screenSize;

        public static void Update(double delta)
        {
            for (int i = 0; i < _uiComponentContainers.Count; i++)
            {
                _uiComponentContainers[i].Update(delta);
            }
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _uiComponentContainers.Count; i++) {
                UIComponentContainer componentContainer = _uiComponentContainers[i];
                spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: componentContainer.AnchorMatrix);
                _uiComponentContainers.First()?.Draw(spriteBatch);
                spriteBatch.End();
            }
        }

        public static void RegisterContainer(UIComponentContainer container)
        {
            container.SetAnchorMatrix(GetAnchorMatrix(container, _screenSize.X, _screenSize.Y));
            _uiComponentContainers.Insert(0, container);
        }

        public static void UnregisterContainer(UIComponentContainer container)
        {
            _uiComponentContainers.Remove(container);
        }
        private static Matrix GetAnchorMatrix(UIComponentContainer componentContainer, int screenWidth, int screenHeight)
        {
            switch (componentContainer.Anchor)
            {
                case Anchor.Center:
                    return Matrix.CreateTranslation(new Vector3(-componentContainer.Position, 0)) * TheGreen.UIScaleMatrix * Matrix.CreateTranslation(new Vector3(componentContainer.Position * (new Vector2(screenWidth, screenHeight) / Globals.NativeResolution.ToVector2()), 0));
                case Anchor.TopLeft:
                    return TheGreen.UIScaleMatrix;
                case Anchor.TopRight:
                    return Matrix.CreateTranslation(-Globals.NativeResolution.X, 0, 0) * TheGreen.UIScaleMatrix * Matrix.CreateTranslation(screenWidth - componentContainer.Size.X * TheGreen.UIScaleMatrix.M11, 0, 0);
                default:
                    return Matrix.Identity;
            }
        }
        public static void OnUIScaleChanged(int screenWidth, int screenHeight)
        {
            _screenSize = new Point(screenWidth, screenHeight);
            for (int i = 0; i < _uiComponentContainers.Count; i++)
            {
                _uiComponentContainers[i].SetAnchorMatrix(GetAnchorMatrix(_uiComponentContainers[i], screenWidth, screenHeight));
            }
        }
    }
}
