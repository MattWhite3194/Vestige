using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
