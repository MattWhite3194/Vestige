using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Vestige.Game.UI;
using Vestige.Game.UI.Components;
using Vestige.Game.UI.Containers;

namespace Vestige.Game.Menus
{
    internal class ChatDisplay : GridContainer
    {
        private class ChatMessage
        {
            public Label Message;
            public double Time;
            public ChatMessage(Label message, double time)
            {
                Message = message;
                Time = time;
            }
        }
        private List<ChatMessage> _messages;
        public ChatDisplay(int margin = 5, Vector2 position = default, Vector2 size = default, Anchor anchor = Anchor.MiddleMiddle) : base(1, margin, position, size, anchor)
        {
            _messages = new List<ChatMessage>();
        }
        public void AddMessage(string message)
        {
            Label messageLabel = new Label(Vector2.Zero, message, Vector2.Zero);
            _messages.Add(new ChatMessage(messageLabel, 20.0f));
            AddComponentChild(messageLabel);
        }
        public override void Update(double delta)
        {
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                _messages[i].Time -= delta;
                if (_messages[i].Time < 0)
                {
                    RemoveComponentChild(_messages[i].Message);
                    _messages.RemoveAt(i);
                }
            }
        }
    }
}
