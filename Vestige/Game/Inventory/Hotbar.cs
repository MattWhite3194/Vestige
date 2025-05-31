using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vestige.Game.Input;
using Vestige.Game.Items;
using Vestige.Game.UI.Containers;
using Vestige.Game.UI;

namespace Vestige.Game.Inventory
{
    public class Hotbar : GridContainer
    {
        private Item[] _inventoryItems;
        private ItemSlot[] _hotbarItemSlots;
        public int Selected;
        public Hotbar(int cols, Item[] inventoryItems, int margin = 5, Vector2 position = default, Color itemSlotColor = default, Anchor anchor = Anchor.TopLeft) : base(cols, margin, position, anchor: anchor)
        {
            _inventoryItems = inventoryItems;
            _hotbarItemSlots = new ItemSlot[cols];
            if (itemSlotColor == default)
            {
                itemSlotColor = Color.White;
            }
            for (int i = 0; i < cols; i++)
            {
                int index = i;
                _hotbarItemSlots[i] = new ItemSlot(Vector2.Zero, ContentLoader.ItemSlotTexture, itemSlotColor);
                AddComponentChild(_hotbarItemSlots[i]);
                _hotbarItemSlots[i].OnMouseInput += (@mouseEvent, mouseCoordinates) => OnItemSlotGuiInput(index, @mouseEvent);
            }
            SetSelected(0);
        }
        private void OnItemSlotGuiInput(int index, InputEvent @mouseEvent)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                SetSelected(index);
                _hotbarItemSlots[Selected].Color = Color.Yellow;
                InputManager.MarkInputAsHandled(@mouseEvent);
            }
        }
        public Item GetSelected()
        {
            return _inventoryItems[Selected];
        }
        public void SetSelected(int index)
        {
            _hotbarItemSlots[Selected].Color = new Color(34, 139, 34, 150);
            Selected = index;
            _hotbarItemSlots[Selected].Color = Color.Yellow;
        }
        public override void HandleInput(InputEvent @event)
        {
            base.HandleInput(@event);
            if (@event.InputButton == InputButton.MiddleMouse)
            {
                if (@event.EventType == InputEventType.MouseButtonUp)
                {
                    SetSelected((Selected + 1) % _hotbarItemSlots.Length);
                }
                else
                {
                    SetSelected((Selected + _hotbarItemSlots.Length - 1) % _hotbarItemSlots.Length);
                }
                InputManager.MarkInputAsHandled(@event);
            }
        }
        protected override void DrawComponents(SpriteBatch spritebatch)
        {
            for (int i = 0; i < _hotbarItemSlots.Length; i++)
            {
                _hotbarItemSlots[i].Draw(spritebatch);
                _hotbarItemSlots[i].DrawItem(spritebatch, _inventoryItems[i]);
            }
        }
    }
}
