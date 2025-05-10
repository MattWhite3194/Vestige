using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.UI.Containers;
using TheGreen.Game.UI;
using System.Diagnostics;

namespace TheGreen.Game.Inventory
{
    public class CraftingGrid : UIComponentContainer
    {
        private Item[] _craftingInputItems;
        private ItemSlot[] _craftingInputSlots;
        private DragItem _dragItem;
        private ItemSlot _craftingOutputSlot;
        private Item _craftingOutputItem;
        private GridContainer _grid;

        public CraftingGrid(int size, DragItem dragItem, int margin = 5, Vector2 position = default, Color itemSlotColor = default, Anchor anchor = Anchor.BottomLeft) : base(anchor: anchor)
        {
            _craftingInputItems = new Item[size * size];
            _craftingInputSlots = new ItemSlot[size * size];
            _grid = new GridContainer(size, margin, position, anchor: anchor);
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
                _grid.AddComponentChild(_craftingInputSlots[i]);
                _craftingInputSlots[i].OnMouseInput += (@mouseEvent, mouseCoordinates) => OnCraftingInputSlotGUIInput(index, @mouseEvent);
            }
            AddContainerChild(_grid);
            itemSlotColor *= 0.8f;
            _craftingOutputSlot = new ItemSlot(_grid.Position + new Vector2(_grid.Size.X + margin * 3, _grid.Size.Y / 2 - ContentLoader.ItemSlotTexture.Height / 2), ContentLoader.ItemSlotTexture, itemSlotColor);
            _craftingOutputSlot.OnMouseInput += (@mouseEvent, mouseCoordinates) => OnCraftingOutputSlotGUIInput(@mouseEvent);
            AddComponentChild(_craftingOutputSlot);
        }
        private void OnCraftingInputSlotGUIInput(int index, MouseInputEvent @mouseEvent)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                PlaceItem(index);
                FindRecipe();
            }
            else if (@mouseEvent.InputButton == InputButton.RightMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                SplitItem(index);
                FindRecipe();
            }
            InputManager.MarkInputAsHandled(@mouseEvent);
        }
        private void OnCraftingOutputSlotGUIInput(MouseInputEvent @mouseEvent)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                if (_dragItem.Item != null || _craftingOutputItem == null)
                    return;
                _dragItem.Item = _craftingOutputItem;
                _craftingOutputItem = null;
                //change this to subtract from recipe
                //store recipe as class variable
                
                for (int i = 0; i < _craftingInputItems.Length; i++)
                {
                    _craftingInputItems[i] = null;
                }
                //delete this^^^
            }
            InputManager.MarkInputAsHandled(@mouseEvent);
        }
        private void FindRecipe()
        {
            _craftingOutputItem = null;
            if (_craftingInputItems[0] != null && _craftingInputItems[0].ID == 1)
            {
                _craftingOutputItem = ItemDatabase.InstantiateItemByID(2);
                Debug.WriteLine("Ouput recipe");
            }
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
            if (_craftingInputItems[index] == null && (_dragItem.Item == null))
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
            else if (_craftingInputItems[index] == null || (_dragItem.Item.ID == _craftingInputItems[index].ID && _dragItem.Item.Stackable))
            {
                if (_craftingInputItems[index] == null)
                {
                    SetItem(ItemDatabase.InstantiateItemByID(_dragItem.Item.ID), index);
                }
                else
                {
                    int newQuantity = _craftingInputItems[index].Quantity + 1;
                    if (newQuantity > _craftingInputItems[index].MaxStack)
                        return;
                    SetItemQuantity(index, newQuantity);
                }
                _dragItem.Item.Quantity -= 1;
                if (_dragItem.Item.Quantity <= 0)
                    _dragItem.Item = null;
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
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: AnchorMatrix);
            for (int i = 0; i < _craftingInputSlots.Length; i++)
            {
                _craftingInputSlots[i].DrawItem(spriteBatch, _craftingInputItems[i]);
            }
            _craftingOutputSlot.DrawItem(spriteBatch, _craftingOutputItem);
            spriteBatch.End();
        }
    }
}
