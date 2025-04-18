using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using TheGreen.Game.UI.Containers;

namespace TheGreen.Game.UIComponents
{
    /// <summary>
    /// updating and drawing for all UIComponent Containers.
    /// </summary>
    public static class UIManager
    {
        private static List<UIComponentContainer> _uiComponentContainers = new List<UIComponentContainer>();
        private static Point _screenSize;
        private static RasterizerState rasterizerState = new RasterizerState()
        {
            ScissorTestEnable = true
        };

        public static void Update(double delta)
        {
            for (int i = _uiComponentContainers.Count - 1; i >= 0 ; i--)
            {
                _uiComponentContainers[i].Update(delta);
            }
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _uiComponentContainers.Count; i++) {
                UIComponentContainer componentContainer = _uiComponentContainers[i];
                spriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, DepthStencilState.None, rasterizerState, transformMatrix: componentContainer.AnchorMatrix);
                _uiComponentContainers[i].Draw(spriteBatch);
                spriteBatch.End();
            }
        }

        public static void RegisterContainer(UIComponentContainer container)
        {
            container.SetAnchorMatrix(GetAnchorMatrix(container, _screenSize.X, _screenSize.Y));
            _uiComponentContainers.Add(container);
        }

        public static void UnregisterContainer(UIComponentContainer container)
        {
            _uiComponentContainers.Remove(container);
        }
        private static Matrix GetAnchorMatrix(UIComponentContainer componentContainer, int screenWidth, int screenHeight)
        {
            if (componentContainer.Anchor == Anchor.None)
            {
                return Matrix.Identity;
            }
            else if (componentContainer.Anchor == Anchor.ScreenScale)
            {
                return Matrix.CreateScale(Math.Max(screenWidth / (float)TheGreen.NativeResolution.X, screenHeight / (float)TheGreen.NativeResolution.Y));
            }
            Vector2 componentSize = Vector2.Transform(componentContainer.GetSize(), TheGreen.UIScaleMatrix);
            Vector2 anchorPos = componentContainer.Anchor switch
            {
                Anchor.TopLeft => new Vector2(0, 0),
                Anchor.TopMiddle => new Vector2(screenWidth / 2 - componentSize.X / 2, 0),
                Anchor.TopRight => new Vector2(screenWidth - componentSize.X, 0),

                Anchor.MiddleLeft => new Vector2(0, screenHeight / 2 - componentSize.Y / 2),
                Anchor.MiddleMiddle => new Vector2(screenWidth / 2 - componentSize.X / 2, screenHeight / 2 - componentSize.Y / 2),
                Anchor.MiddleRight => new Vector2(screenWidth - componentSize.X, screenHeight / 2 - componentSize.Y / 2),

                Anchor.BottomLeft => new Vector2(0, screenHeight - componentSize.Y),
                Anchor.BottomMiddle => new Vector2(screenWidth / 2 - componentSize.X / 2, screenHeight - componentSize.Y),
                Anchor.BottomRight => new Vector2(screenWidth - componentSize.X, screenHeight - componentSize.Y),

                _ => Vector2.Zero
            };

            Matrix translation = Matrix.CreateTranslation(anchorPos.X, anchorPos.Y, 0);
            
            return TheGreen.UIScaleMatrix * translation;
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
