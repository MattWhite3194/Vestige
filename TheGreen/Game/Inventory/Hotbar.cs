using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.UI.Containers;

namespace TheGreen.Game.Inventory
{
    public class Hotbar : GridContainer
    {
        private Item[] _inventoryItems;
        private ItemSlot[] _hotbarItemSlots;
        private int selected;
        public Hotbar(int cols, Item[] inventoryItems, int margin = 5, Vector2 position = default) : base(cols, margin, position)
        {
            _inventoryItems = inventoryItems;
            _hotbarItemSlots = new ItemSlot[cols];
            for (int i = 0; i < cols; i++)
            {
                int index = i;
                _hotbarItemSlots[i] = new ItemSlot(Vector2.Zero, ContentLoader.ItemSlotTexture, new Color(34, 139, 34, 150));
                AddUIComponent(_hotbarItemSlots[i]);
                _hotbarItemSlots[i].OnMouseInput += (@mouseEvent, mouseCoordinates) => OnItemSlotGuiInput(index, @mouseEvent);
            }
            SetSelected(0);
        }
        private void OnItemSlotGuiInput(int index, InputEvent @mouseEvent)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                SetSelected(index);
                _hotbarItemSlots[selected].SetColor(Color.Yellow);
                InputManager.MarkInputAsHandled(@mouseEvent);
            }
        }
        public Item GetSelected()
        {
            return _inventoryItems[selected];
        }
        public void SetSelected(int index)
        {
            _hotbarItemSlots[selected].SetColor(new Color(34, 139, 34, 150));
            selected = index;
            _hotbarItemSlots[selected].SetColor(Color.Yellow);
        }
        public void SetSelectedQuantity(int quantity)
        {
            if (quantity <= 0)  
                _inventoryItems[selected] = null;
            else
                _inventoryItems[selected].Quantity = quantity;
        }
        public override void HandleInput(InputEvent @event)
        {
            base.HandleInput(@event);
            if (@event.InputButton == InputButton.MiddleMouse)
            {
                if (@event.EventType == InputEventType.MouseButtonUp)
                {
                    SetSelected((selected + 1) % _hotbarItemSlots.Length);
                }
                else
                {
                    SetSelected((selected + _hotbarItemSlots.Length - 1) % _hotbarItemSlots.Length);
                }
                InputManager.MarkInputAsHandled(@event);
            }
        }
        public override void Draw(SpriteBatch spritebatch)
        {
            for (int i = 0; i < _hotbarItemSlots.Length; i++)
            {
                _hotbarItemSlots[i].Draw(spritebatch);
                _hotbarItemSlots[i].DrawItem(spritebatch, _inventoryItems[i]);
            }
        }
    }
}
