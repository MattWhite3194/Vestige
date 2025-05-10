using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.UI.Containers;
using TheGreen.Game.UI;

namespace TheGreen.Game.Inventory
{
    public class CraftingGrid : GridContainer
    {
        private Item[] _craftingInputItems;
        private ItemSlot[] _craftingInputSlots;
        private DragItem _dragItem;
        private ItemSlot _craftingOutputSlot;
        private Item _craftingOutputItem;

        public CraftingGrid(int size, DragItem dragItem, int margin = 5, Vector2 position = default, Color itemSlotColor = default, Anchor anchor = Anchor.BottomLeft) : base(size, margin, position, anchor: anchor)
        {
            _craftingInputItems = new Item[size * size];
            _craftingInputSlots = new ItemSlot[size * size];
            this._dragItem = dragItem;
            if (itemSlotColor == default)
            {
                itemSlotColor = Color.ForestGreen;
            }
            itemSlotColor.A = 200;
            for (int i = 0; i < _craftingInputSlots.Length; i++)
            {
                int index = i;
                _craftingInputSlots[i] = new ItemSlot(Vector2.Zero, ContentLoader.ItemSlotTexture, itemSlotColor);
                AddComponentChild(_craftingInputSlots[i]);
                _craftingInputSlots[i].OnMouseInput += (@mouseEvent, mouseCoordinates) => OnCraftingInputSlotGUIInput(index, @mouseEvent);
            }
            itemSlotColor *= 0.8f;
            _craftingOutputSlot = new ItemSlot(Position + new Vector2(Size.X + margin * 3, Size.Y / size), ContentLoader.ItemSlotTexture, itemSlotColor);
            _craftingOutputSlot.OnMouseInput += (@mouseEvent, mouseCoordinates) => OnCraftingOutputSlotGUIInput(@mouseEvent);
        }
        private void OnCraftingInputSlotGUIInput(int index, MouseInputEvent @mouseEvent)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                PlaceItem(index);
                InputManager.MarkInputAsHandled(@mouseEvent);
            }
            else if (@mouseEvent.InputButton == InputButton.RightMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                SplitItem(index);
                InputManager.MarkInputAsHandled(@mouseEvent);
            }
            FindRecipe();
        }
        private void OnCraftingOutputSlotGUIInput(MouseInputEvent @mouseEvent)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                if (_dragItem.Item != null || _craftingOutputItem == null)
                    return;
                _dragItem.Item = _craftingOutputItem;
                _craftingOutputItem = null;
                for (int i = 0; i < _craftingInputItems.Length; i++)
                {
                    _craftingInputItems[i] = null;
                }
            }
        }
        private void FindRecipe()
        {
            //TODO: implementation
        }
        private void PlaceItem(int index)
        {
            if (_dragItem.Item == null && _craftingInputItems[index] == null)
            {
                return;
            }
            if (_dragItem.Item == null)
            {
                _dragItem.Item = _craftingInputItems[index];
                SetItem(null, index);
            }
            else if (_craftingInputItems[index] == null)
            {
                SetItem(_dragItem.Item, index);
                _dragItem.Item = null;
            }
            else if (_craftingInputItems[index].ID == _dragItem.Item.ID && _craftingInputItems[index].Stackable && _craftingInputItems[index].Quantity < _craftingInputItems[index].MaxStack)
            {
                int newQuantity = _craftingInputItems[index].Quantity + _dragItem.Item.Quantity;
                if (newQuantity > _craftingInputItems[index].MaxStack)
                {
                    SetItemQuantity(index, _craftingInputItems[index].MaxStack);
                    _dragItem.Item.Quantity = newQuantity - _dragItem.Item.MaxStack;
                    return;
                }
                SetItemQuantity(index, newQuantity);
                _dragItem.Item = null;
            }
            else
            {
                Item temp = _craftingInputItems[index];
                SetItem(_dragItem.Item, index);
                _dragItem.Item = temp;
            }
        }

        private void SplitItem(int index)
        {
            if (_craftingInputItems[index] == null)
                return;
            if (_dragItem.Item == null)
            {
                if (_craftingInputItems[index].Stackable)
                {
                    Item splitItem = ItemDatabase.InstantiateItemByID(_craftingInputItems[index].ID);
                    splitItem.Quantity = (int)Math.Ceiling(_craftingInputItems[index].Quantity / 2.0f);
                    _dragItem.Item = splitItem;
                    SetItemQuantity(index, _craftingInputItems[index].Quantity - splitItem.Quantity);
                }
                else
                {
                    _dragItem.Item = _craftingInputItems[index];
                    SetItem(null, index);

                }
            }
        }
        private void SetItem(Item item, int index)
        {
            _craftingInputItems[index] = item;
        }
        private void SetItemQuantity(int index, int quantity)
        {
            _craftingInputItems[index].Quantity = quantity;
            SetItem(_craftingInputItems[index], index);
            if (quantity <= 0)
            {
                SetItem(null, index);
            }
        }
        protected override void DrawComponents(SpriteBatch spritebatch)
        {
            for (int i = 0; i < _craftingInputSlots.Length; i++)
            {
                _craftingInputSlots[i].Draw(spritebatch);
                _craftingInputSlots[i].DrawItem(spritebatch, _craftingInputItems[i]);
            }
            _craftingOutputSlot.Draw(spritebatch);
            _craftingOutputSlot.DrawItem(spritebatch, _craftingOutputItem);
        }
    }
}
