using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.Input;
using Vestige.Game.Items;
using Vestige.Game.UI.Containers;
using Vestige.Game.UI;
using System.Collections.Generic;

namespace Vestige.Game.Inventory
{
    //TODO: add ability to hold down on crafting output slot and automatically pick up items.

    //TODO: craft all available
    public class CraftingGrid : UIContainer
    {
        private Item[] _craftingInputItems;
        private ItemSlot[] _craftingInputSlots;
        private DragItem _dragItem;
        private ItemSlot _craftingOutputSlot;
        private Item _craftingOutputItem;
        private GridContainer _grid;
        private int _gridSize;
        private Point _recipeLocation;
        private List<(byte, byte, int)> _currentRecipe;
        private ToolTip _toolTip;

        public CraftingGrid(int size, DragItem dragItem, ToolTip toolTip, int margin = 5, Vector2 position = default, Color itemSlotColor = default, Anchor anchor = Anchor.TopLeft) : base(anchor: anchor)
        {
            _craftingInputItems = new Item[size * size];
            _craftingInputSlots = new ItemSlot[size * size];
            _grid = new GridContainer(size, margin, position, anchor: anchor);
            _gridSize = size;
            _dragItem = dragItem;
            _toolTip = toolTip;
            if (itemSlotColor == default)
            {
                itemSlotColor = Color.White;
            }
            for (int i = 0; i < _craftingInputSlots.Length; i++)
            {
                int index = i;
                _craftingInputSlots[i] = new ItemSlot(Vector2.Zero, ContentLoader.ItemSlotTexture, itemSlotColor);
                _grid.AddComponentChild(_craftingInputSlots[i]);
                _craftingInputSlots[i].OnMouseInput += (@mouseEvent, mouseCoordinates) => OnCraftingInputSlotGUIInput(index, @mouseEvent);
                _craftingInputSlots[i].OnMouseEntered += () =>
                {
                    if (_dragItem.Item == null)
                    {
                        toolTip.ShowItemStats(_craftingInputItems[index]);
                    }
                    toolTip.ItemSlotIndex = index;
                };
                _craftingInputSlots[i].OnMouseExited += () =>
                {
                    if (toolTip.ItemSlotIndex == index)
                    {
                        toolTip.SetText("");
                        toolTip.ItemSlotIndex = -1;
                    }
                };
            }
            AddContainerChild(_grid);
            itemSlotColor.A = 100;
            _craftingOutputSlot = new ItemSlot(_grid.Position + new Vector2(_grid.Size.X + margin * 10, _grid.Size.Y / 2 - ContentLoader.ItemSlotTexture.Height / 2), ContentLoader.ItemSlotTexture, itemSlotColor);
            _craftingOutputSlot.OnMouseInput += (@mouseEvent, mouseCoordinates) => OnCraftingOutputSlotGUIInput(@mouseEvent);
            _craftingOutputSlot.OnMouseEntered += () =>
            { 
                if (_dragItem.Item == null)
                {
                    toolTip.ShowItemStats(_craftingOutputItem);
                    toolTip.ItemSlotIndex = _craftingInputSlots.Length;
                }
            };
            _craftingOutputSlot.OnMouseExited += () =>
            {
                if (toolTip.ItemSlotIndex == _craftingInputSlots.Length)
                {
                    toolTip.SetText("");
                    toolTip.ItemSlotIndex = -1;
                }
            };
            AddComponentChild(_craftingOutputSlot);
        }
        private void OnCraftingInputSlotGUIInput(int index, MouseInputEvent @mouseEvent)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                _toolTip.SetText("");
                PlaceItem(index);
                FindRecipe();
            }
            else if (@mouseEvent.InputButton == InputButton.RightMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                _toolTip.SetText("");
                SplitItem(index);
                FindRecipe();
            }
            InputManager.MarkInputAsHandled(@mouseEvent);
        }
        private void OnCraftingOutputSlotGUIInput(MouseInputEvent @mouseEvent)
        {
            if (@mouseEvent.InputButton == InputButton.LeftMouse && @mouseEvent.EventType == InputEventType.MouseButtonDown)
            {
                InputManager.MarkInputAsHandled(@mouseEvent);
                if (_craftingOutputItem == null)
                    return;
                _toolTip.SetText("");
                if (_dragItem.Item != null)
                {
                    if (_dragItem.Item.ID == _craftingOutputItem.ID && _dragItem.Item.Stackable)
                    {
                        _dragItem.Item.Quantity += _craftingOutputItem.Quantity;
                        if (_dragItem.Item.Quantity > _dragItem.Item.MaxStack)
                        {
                            //drag item is full, don't consume recipe
                            _dragItem.Item.Quantity -= _craftingOutputItem.Quantity;
                            return;
                        }
                        else
                        {
                            _craftingOutputItem = null;
                        }
                    }
                    else return;
                }
                else
                {
                    _dragItem.Item = _craftingOutputItem;
                    _craftingOutputItem = null;
                }
                for (int i = 0; i < _currentRecipe.Count; i++)
                {
                    int index = (_recipeLocation.Y + _currentRecipe[i].Item2) * _gridSize + _recipeLocation.X + _currentRecipe[i].Item1;
                    _craftingInputItems[index].Quantity -= 1;
                    if (_craftingInputItems[index].Quantity <= 0)
                    {
                        _craftingInputItems[index] = null;
                    }
                }
                FindRecipe();
            }
        }
        private void FindRecipe()
        {
            _craftingOutputItem = null;
            
            int startX = -1, startY = -1;
            int endX = -1, endY = -1;
            List<(byte, byte, int)> recipeInputs = new List<(byte, byte, int)>();
            for (int i = 0; i < _craftingInputItems.Length; i++)
            {
                if (_craftingInputItems[i] != null)
                {
                    startX = startX == -1 ? i % _gridSize : Math.Min(startX, i % _gridSize);
                    startY = startY == -1 ? i / _gridSize : Math.Min(startY, i / _gridSize);
                    endX = endX == -1 ? i % _gridSize : Math.Max(endX, i % _gridSize);
                    endY = endY == -1 ? i / _gridSize : Math.Max(endY, i / _gridSize);
                    recipeInputs.Add(((byte)(i % _gridSize), (byte)(i/_gridSize), _craftingInputItems[i].ID));
                }
            }
            Point recipeSize = new Point(endX -  startX + 1, endY - startY + 1);
            for (int i = 0; i < recipeInputs.Count; i++)
            {
                recipeInputs[i] = ((byte)(recipeInputs[i].Item1 - startX), (byte)(recipeInputs[i].Item2 - startY), recipeInputs[i].Item3);
            }
            _currentRecipe = recipeInputs;
            _recipeLocation = new Point(startX, startY);
            _craftingOutputItem = CraftingRecipes.GetItemFromRecipe(recipeSize, recipeInputs);
            if (_craftingOutputItem != null)
            {
                _craftingOutputSlot.Color.A = 200;
            }
            else
            {
                _craftingOutputSlot.Color.A = 100;
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
                _toolTip.ShowItemStats(_craftingInputItems[index]);
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
                _toolTip.ShowItemStats(_craftingInputItems[index]);
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
                    Item splitItem = Item.InstantiateItemByID(_craftingInputItems[index].ID);
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
                    SetItem(Item.InstantiateItemByID(_dragItem.Item.ID), index);
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
                {
                    _dragItem.Item = null;
                    _toolTip.ShowItemStats(_craftingInputItems[index]);
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
        }
        public override void Draw(SpriteBatch spriteBatch, RasterizerState rasterizerState = null)
        {
            base.Draw(spriteBatch);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _grid.AnchorMatrix);
            for (int i = 0; i < _craftingInputSlots.Length; i++)
            {
                _craftingInputSlots[i].DrawItem(spriteBatch, _craftingInputItems[i]);
            }
            spriteBatch.End();
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: AnchorMatrix);
            _craftingOutputSlot.DrawItem(spriteBatch, _craftingOutputItem);
            spriteBatch.End();
        }
    }
}
