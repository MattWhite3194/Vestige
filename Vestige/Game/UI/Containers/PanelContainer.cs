using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Vestige.Game.UI.Containers
{
    public class PanelContainer : UIContainer
    {
        private Texture2D _panel;
        private Vector2 _panelPosition;
        public PanelContainer(Vector2 position, Vector2 size, Color color, Color borderColor, int padding, int borderWidth, int borderRadius, GraphicsDevice graphicsDevice, Anchor anchor = Anchor.MiddleMiddle) : base(position, size, anchor)
        {
            _panel = new Texture2D(graphicsDevice, (int)size.X + padding * 2, (int)size.Y + padding * 2);
            //TODO: need to add border radius somehow
            Color[] colorData = Enumerable.Repeat(color, _panel.Width * _panel.Height).ToArray();

            //Set borders
            for (int i = 0; i < _panel.Width; i++)
            {
                for (int j = 0; j < borderWidth; j++)
                {
                    colorData[_panel.Width * j + i] = borderColor;
                    colorData[colorData.Length - 1 - (_panel.Width * j + i)] = borderColor;
                }
            }
            for (int i = 0; i < _panel.Height; i++)
            {
                for (int j = 0; j < borderWidth; j++)
                {
                    colorData[i * _panel.Width + j] = borderColor;
                    colorData[colorData.Length - 1 - (i * _panel.Width) - j] = borderColor;
                }
            }

            //Attempting border radius
            //check borderRadius * borderRadius box for distance around point at (borderRadius, borderRadius)
            //only do it once for top left corner, and set pixels on all four corners
            for (int i = 0; i < borderRadius; i++)
            {
                for (int j = 0; j < borderRadius; j++)
                {
                    Color replacementColor = color;
                    if (Vector2.Distance(new Vector2(borderRadius), new Vector2(i, j)) >= borderRadius)
                    {
                        replacementColor = Color.Transparent;
                    }
                    else if (Vector2.Distance(new Vector2(borderRadius), new Vector2(i, j)) > borderRadius - borderWidth)
                    {
                        replacementColor = borderColor;
                    }
                    //Top left corner
                    colorData[j * _panel.Width + i] = replacementColor;
                    //Bottom right corner
                    colorData[colorData.Length - 1 - (j * _panel.Width + i)] = replacementColor;
                    //Top right
                    colorData[j * _panel.Width + (_panel.Width - 1 - i)] = replacementColor;
                    //bottom left
                    colorData[colorData.Length - 1 - (j * _panel.Width + (_panel.Width - 1 - i))] = replacementColor;
                }
            }
            _panel.SetData(colorData);
            _panelPosition = new Vector2(-padding);
        }
        protected override void DrawComponents(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_panel, _panelPosition, Color.White);
            base.DrawComponents(spriteBatch);
        }
    }
}
