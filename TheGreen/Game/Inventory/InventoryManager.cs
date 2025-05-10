using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.Tiles.TileData;
using TheGreen.Game.UI.Containers;

namespace TheGreen.Game.Inventory
{
    public class InventoryManager : UIComponentContainer
    {
        private Hotbar _hotbar;
        private Inventory _inventoryMenu;
        private DragItem _dragItem;
        private InventoryTileData _inventoryTileData;
        private Point _inventoryTileDataCoordinates;
        private Inventory _tileInventory;
        private GridContainer _activeMenu;
        private CraftingGrid _craftingMenu;
        public InventoryManager(int rows, int cols)
        {
            //Temporary inventory
            
            Item[] inventoryItems = new Item[rows * cols];

            for (int i = 0; i <= 7; i++)
            {
                Item item = ItemDatabase.InstantiateItemByID(i);
                item.Quantity = item.MaxStack;
                inventoryItems[i] = item;
            }


            _dragItem = new DragItem(Vector2.Zero);
            _inventoryMenu = new Inventory(cols, _dragItem, inventoryItems, margin: 2, position: new Vector2(20, 20));
            _hotbar = new Hotbar(cols, inventoryItems, margin: 2, position: new Vector2(20, 20));
            _craftingMenu = new CraftingGrid(3, _dragItem, margin: 2, position: new Vector2(20, _inventoryMenu.Size.Y + 25), itemSlotColor: Color.BurlyWood, anchor: UI.Anchor.TopLeft);
            _activeMenu = _hotbar;
            AddContainerChild(_hotbar);
        }
        public override void HandleInput(InputEvent @event)
        {
            //Don't accept any input if the player is using an item to prevent any weird bugs if the player decides to press random buttons while using the item
            //TODO: maybe want to make this a little less dependent and spaghetti-ish
            if (Main.EntityManager.GetPlayer().ItemCollider.ItemActive) return;
            else if (@event.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.Inventory)
            {
                SetInventoryOpen(!InventoryVisible());
                if (_dragItem.Item != null)
                {
                    Item itemDrop = _inventoryMenu.AddItem(_dragItem.Item);
                    _dragItem.Item = null;
                    if (itemDrop != null)
                    {
                        Main.EntityManager.AddItemDrop(itemDrop, Main.EntityManager.GetPlayer().Position);
                    }
                }
                InputManager.MarkInputAsHandled(@event);
            }
            else
            {
                base.HandleInput(@event);
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
                InputManager.MarkInputAsHandled(@event);
            }
        }

        public override void Update(double delta)
        {
            base.Update(delta);
            _dragItem.Update(delta);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: TheGreen.UIScaleMatrix);
            _dragItem.Draw(spriteBatch);
            spriteBatch.End();
        }

        public Item GetSelected()
        {
            if (InventoryVisible())
            {
                return _dragItem.Item;
            }
            return _hotbar.GetSelected();
        }

        public bool InventoryVisible()
        {
            return _activeMenu == _inventoryMenu;
        }

        public void DisplayTileInventory(InventoryTileData inventoryTileData, Point coordinates, Item[] items)
        {
            SetInventoryOpen(true);
            if (_tileInventory != null)
            {
                RemoveContainerChild(_tileInventory);
            }
            if (_inventoryTileData != null && coordinates != _inventoryTileDataCoordinates)
            {
                _inventoryTileData.CloseInventory(_inventoryTileDataCoordinates.X, _inventoryTileDataCoordinates.Y);
            }
            _tileInventory = new Inventory(inventoryTileData.Cols, _dragItem, items, margin: 2, position: new Vector2(_inventoryMenu.Size.X + 25, 20 ), itemSlotColor: Color.Crimson);
            _inventoryTileDataCoordinates = coordinates;
            _inventoryTileData = inventoryTileData;
            AddContainerChild(_tileInventory);
        }
        public void SetInventoryOpen(bool open)
        {
            if (open == InventoryVisible())
                return;
            _activeMenu = open ? _inventoryMenu : _hotbar;
            if (open)
            {
                AddContainerChild(_inventoryMenu);
                AddContainerChild(_craftingMenu);
                RemoveContainerChild(_hotbar);
            }
            else
            {
                RemoveContainerChild(_inventoryMenu);
                RemoveContainerChild(_craftingMenu);
                AddContainerChild(_hotbar);
                if (_tileInventory != null)
                {
                    RemoveContainerChild(_tileInventory);
                    _tileInventory = null;

                    if (_inventoryTileData != null)
                    {
                        _inventoryTileData.CloseInventory(_inventoryTileDataCoordinates.X, _inventoryTileDataCoordinates.Y);
                        _inventoryTileData = null;
                    }
                }
            }
        }
        public bool UseSelected()
        {
            Item item = GetSelected();
            if (item == null)
                return false;
            bool itemUsed = item.UseItem();
            if (!item.Stackable || !itemUsed)
                return itemUsed;
            if (InventoryVisible())
            {
                _dragItem.Item.Quantity -= 1;
                if (_dragItem.Item.Quantity <= 0)
                    _dragItem.Item = null;
            }
            else
                _hotbar.SetSelectedQuantity(item.Quantity - 1);
            return itemUsed;
        }
        public Item AddItemToPlayerInventory(Item item)
        {
            return _inventoryMenu.AddItem(item);
        }
        public override void UpdateAnchorMatrix(int parentSizeX, int parentSizeY)
        {
            this.Size = new Vector2(parentSizeX, parentSizeY);
            base.UpdateAnchorMatrix(parentSizeX, parentSizeY);
        }
    }
}
