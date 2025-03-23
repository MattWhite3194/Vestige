using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGreen.Game.Input;

namespace TheGreen.Game.UIComponents
{
    internal class Textbox : UIComponent
    {
        private string text = "";
        private string placeholder = "";
        public Textbox(Vector2 position, Texture2D image, Color color) : base(position, image, color)
        {

        }

        protected override void HandleGuiInput(InputEvent @event)
        {
            if (@event is MouseInputEvent mouseInputEvent)
            {
                if (mouseInputEvent.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.LeftMouse)
                {
                    if (IsFocused())
                    {
                        if (InputManager.GetMouseWindowBounds().Intersects(GetBounds()))
                        {
                            //Do something if you click somewhere on the textbox while its focused
                        }
                        else
                        {
                            SetFocused(false);
                        }
                    }
                    else
                    {
                        SetFocused(true);
                    }
                }
            }
        }

        public void AcceptTextInput(char input)
        {
            text += input;
        }
    }
}
