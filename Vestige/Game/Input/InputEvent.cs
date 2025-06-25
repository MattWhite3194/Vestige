namespace Vestige.Game.Input
{
    public class InputEvent
    {
        public InputEventType EventType;
        public InputButton InputButton;
        public bool handled;
        public InputEvent(InputEventType eventType, InputButton input)
        {
            EventType = eventType;
            InputButton = input;
            handled = false;
        }
    }
    public class MouseInputEvent : InputEvent
    {
        public MouseInputEvent(InputEventType eventType, InputButton input) : base(eventType, input)
        {
        }
    }

    public enum InputEventType
    {
        KeyDown,
        KeyUp,
        MouseButtonDown,
        MouseButtonUp
    }

    public enum InputButton
    {
        Up,
        Down,
        Left,
        Right,
        Jump,
        Inventory,
        LeftArrow,
        RightArrow,
        LeftMouse,
        RightMouse,
        MiddleMouse,
        Options,
        Terminal,
        AltUse,
        Map
    }
}
