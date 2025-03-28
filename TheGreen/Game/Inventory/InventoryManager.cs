using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.Entities;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.UIComponents;

namespace TheGreen.Game.Inventory
{
    public class InventoryManager : UIComponentContainer
    {
        //TODO: change item slots to have a reference to an item
        //set the pointers for the hotbar and menu to the inventoryItems array. will nullify the use for the ItemsChanged method
        private Item[] _inventoryItems;
        private ItemSlot[] _inventoryItemSlots;
        private ItemSlot[] _hotbarItemSlots;
        private Grid _hotbar;
        private Grid _menu;
        private DragItem _dragItem;
        private int selected;
        private UIComponentContainer _activeMenu;
        public InventoryManager(int rows, int cols)
        {
            _inventoryItems = new Item[rows * cols];

            //Temporary inventory
            _inventoryItems[0] = ItemDatabase.GetItemByID(0, quantity: 500);
            _inventoryItems[1] = ItemDatabase.GetItemByID(1, quantity: 500);
            _inventoryItems[2] = ItemDatabase.GetItemByID(2);
            _inventoryItems[3] = ItemDatabase.GetItemByID(3, quantity: 500);
            _inventoryItems[4] = ItemDatabase.GetItemByID(4, quantity: 100);

            

            _inventoryItemSlots = new ItemSlot[rows * cols];
            _hotbarItemSlots = new ItemSlot[cols];

            _menu = new Grid(cols, margin: 2, position: new Vector2(20, 20));
            _hotbar = new Grid(cols, margin: 2, position: new Vector2(20, 20));


            for (int i = 0; i < _inventoryItemSlots.Length; i++)
            {
                int index = i;
                _inventoryItemSlots[i] = new ItemSlot(Vector2.Zero, ContentLoader.ItemSlotTexture, Color.ForestGreen);
                _menu.AddGridItem(_inventoryItemSlots[i]);
                _inventoryItemSlots[i].OnGuiInput += (@event) => OnItemSlotGuiInput(index, @event);
                _inventoryItemSlots[index].Item = _inventoryItems[index];
            }
            for (int i = 0; i < cols; i++)
            {
                int index = i;
                _hotbarItemSlots[i] = new ItemSlot(Vector2.Zero, ContentLoader.ItemSlotTexture, new Color(34, 139, 34, 200));
                _hotbar.AddGridItem(_hotbarItemSlots[i]);
                _hotbarItemSlots[i].OnGuiInput += (@event) => OnItemSlotGuiInput(index, @event);
                _hotbarItemSlots[index].Item = _inventoryItems[index];
            }
            _dragItem = new DragItem(Vector2.Zero);


            _activeMenu = _hotbar;

            SetSelected(0);
        }

        public override void Update(double delta)
        {
            if (GetSelected()?.Update(delta) ?? false)
            {
                ItemsChanged([selected]);
            }
            _activeMenu.Update(delta);
            _dragItem.Update(delta);
            
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            _activeMenu.Draw(spritebatch);
            _dragItem.Draw(spritebatch);
        }
        public override void HandleInput(InputEvent @event)
        {
            //accept input for middle mouse events
            if (@event.InputButton == InputButton.MiddleMouse && (!GetSelected()?.Active ?? true))
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
            else if (@event.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.Inventory)
            {
                _activeMenu = InventoryVisible() ? _hotbar : _menu;
                InputManager.MarkInputAsHandled(@event);
            }
            else
            {
                _activeMenu.HandleInput(@event);
            }
            if (InputManager.IsEventHandled(@event))
                return;
            //accept input for right mouse down if the inventory is visible
            else if (@event.InputButton == InputButton.RightMouse && @event.EventType == InputEventType.MouseButtonDown && InventoryVisible())
            {
                if (_dragItem.Item == null)
                    return;
                Main.EntityManager.AddItemDrop(_dragItem.Item, InputManager.GetMouseWorldPosition().ToVector2());
                _dragItem.Item = null;
                InputManager.IsEventHandled(@event);
            }
        }

        //update inventory gui when the items in the array change
        private void ItemsChanged(int[] indices)
        {
            _dragItem.Refresh();
            foreach (int index in indices)
            {
                if ((_inventoryItems[index]?.Stackable ?? false) && _inventoryItems[index].Quantity <= 0)
                    _inventoryItems[index] = null;
                if (index < _hotbarItemSlots.Length)
                {
                    _hotbarItemSlots[index].Item = _inventoryItems[index];
                    _hotbarItemSlots[index].Refresh();
                }
                _inventoryItemSlots[index].Item = _inventoryItems[index];
                _inventoryItemSlots[index].Refresh();
            }
        }

        //handle item slot input for individual item slots
        private void OnItemSlotGuiInput(int index, InputEvent @event)
        {
            if (@event.InputButton == InputButton.LeftMouse && @event.EventType == InputEventType.MouseButtonDown)
            {
                if (!InventoryVisible())
                {
                    SetSelected(index);
                    _hotbarItemSlots[selected].SetColor(Color.Yellow);
                }
                else
                {
                    PlaceItem(index);
                }
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.RightMouse && @event.EventType == InputEventType.MouseButtonDown)
            {
                if (InventoryVisible())
                {
                    SplitItem(index);
                    InputManager.MarkInputAsHandled(@event);
                }
            }
        }

        public Item GetSelected()
        {
            if (InventoryVisible())
            {
                return _dragItem.Item;
            }
            return _inventoryItems[selected];
        }

        public void SetSelected(int index)
        {
            _hotbarItemSlots[selected].SetColor(new Color(34, 139, 34, 200));
            if (_inventoryItems[selected] != null)
                _inventoryItems[selected].Active = false;
            selected = index;
            _hotbarItemSlots[selected].SetColor(Color.Yellow);
        }

        private void SetItem(Item item, int index) {
            _inventoryItems[index] = item;
        }

        /// <summary>
        /// Attempts to add the item to the inventory.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The quantity remaining of the item added</returns>
        public int AddItem(Item item)
        {
            int emptyIndex = -1;
            int remainingQuantity = item.Quantity;
            for (int i = 0; i < _inventoryItems.Length; i++)
            {
                //get first empty slot starting from the back of the array
                if (emptyIndex == -1 && _inventoryItems[_inventoryItems.Length - 1 - i] == null)
                    emptyIndex = _inventoryItems.Length - 1 - i;
                else if (emptyIndex != -1 && !item.Stackable)
                    break;

                if (item.Stackable && item.ID == (_inventoryItems[i]?.ID ?? -1))
                {
                    if (_inventoryItems[i].Quantity >= 30)
                        continue;
                    int newQuantity = _inventoryItems[i].Quantity + remainingQuantity;
                    remainingQuantity = int.Clamp(newQuantity - 30, 0, 30);
                    newQuantity -= remainingQuantity;
                    
                    _inventoryItems[i].Quantity = newQuantity;
                    ItemsChanged([i]);
                }
            }
            item.Quantity = remainingQuantity;
            if (emptyIndex != -1 && remainingQuantity > 0)
            {
                SetItem(item, emptyIndex);
                remainingQuantity = 0;
                ItemsChanged([emptyIndex]);
            }
            return remainingQuantity;
        }

        private void PlaceItem(int index)
        {
            if (_dragItem.Item == null && _inventoryItems[index] == null)
            {
                return;
            }
            if (_dragItem.Item == null)
            {
                _dragItem.Item = _inventoryItems[index];
                SetItem(null, index);
            }
            else if (_inventoryItems[index] == null)
            {
                SetItem(_dragItem.Item, index);
                _dragItem.Item = null;
            }
            else if (_inventoryItems[index].ID == _dragItem.Item.ID && _inventoryItems[index].Stackable)
            {
                _inventoryItems[index].Quantity += _dragItem.Item.Quantity;
                _dragItem.Item = null;
            }
            else
            {
                Item temp = _inventoryItems[index];
                SetItem(_dragItem.Item, index);
                _dragItem.Item = temp;
            }
            ItemsChanged([index]);
        }

        private void SplitItem(int index)
        {
            if (_inventoryItems[index] == null)
                return;
            if (_dragItem.Item == null)
            {
                if (_inventoryItems[index].Stackable)
                {
                    Item splitItem = ItemDatabase.GetItemByID(_inventoryItems[index].ID);
                    splitItem.Quantity = (int)Math.Ceiling(_inventoryItems[index].Quantity / 2.0f);
                    _dragItem.Item = splitItem;
                    _inventoryItems[index].Quantity = _inventoryItems[index].Quantity - splitItem.Quantity;
                }
                else
                {
                    _dragItem.Item = _inventoryItems[index];
                    SetItem(null, index);
                    
                }
                ItemsChanged([index]);
            }
        }

        public bool InventoryVisible()
        {
            return _activeMenu == _menu;
        }
    }
}
