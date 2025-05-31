using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.Input;
using TheGreen.Game.Items;
using TheGreen.Game.Tiles.TileData;
using TheGreen.Game.UI.Containers;

namespace TheGreen.Game.Inventory
{
    public class InventoryManager : UIContainer
    {
        private Hotbar _hotbar;
        private Inventory _inventoryMenu;
        private DragItem _dragItem;
        private InventoryTileData _inventoryTileData;
        private Point _inventoryTileDataCoordinates;
        private Inventory _tileInventory;
        private bool _inventoryOpen;
        private CraftingGrid _craftingMenu;
        private Item[] _inventoryItems;
        private ToolTip _toolTip;
        public InventoryManager(int rows, int cols) : base(anchor: UI.Anchor.TopLeft)
        {
            //Temporary inventory
            
            _inventoryItems = new Item[rows * cols];

            for (int i = 0; i <= 9; i++)
            {
                Item item = ItemDatabase.InstantiateItemByID(i);
                item.Quantity = item.MaxStack;
                _inventoryItems[i] = item;
            }

            _toolTip = new ToolTip();
            _dragItem = new DragItem(Vector2.Zero);
            _inventoryMenu = new Inventory(cols, _dragItem, _toolTip, _inventoryItems, margin: 2, position: new Vector2(20, 20), itemSlotColor: new Color(34, 139, 34, 250));
            _hotbar = new Hotbar(cols, _inventoryItems, margin: 2, itemSlotColor: new Color(34, 139, 34, 200), position: new Vector2(20, 20));
            _craftingMenu = new CraftingGrid(3, _dragItem, _toolTip, margin: 2, position: new Vector2(20, _inventoryMenu.Size.Y + 25), itemSlotColor: new Color(222, 184, 135, 230), anchor: UI.Anchor.TopLeft);
            _inventoryOpen = false;
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
                        Vector2 velocity = new Vector2(Main.EntityManager.GetPlayer().FlipSprite ? -50 : 50, 0);
                        Main.EntityManager.AddItemDrop(itemDrop, Main.EntityManager.GetPlayer().Position, velocity, false);
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
                int direction = Math.Sign(InputManager.GetMouseWorldPosition().X - Main.EntityManager.GetPlayer().Position.X);
                Vector2 itemVelocity = new Vector2(direction * 50, 0);
                Main.EntityManager.AddItemDrop(_dragItem.Item, Main.EntityManager.GetPlayer().Position, itemVelocity, false);
                _dragItem.Item = null;
                InputManager.MarkInputAsHandled(@event);
            }
        }

        public override void Update(double delta)
        {
            base.Update(delta);
            _dragItem.Update(delta);
            _toolTip.Update(delta);
            if (_toolTip.ItemSlotIndex == -1)
            {
                if (Main.EntityManager.MouseEntity != null)
                {
                    _toolTip.SetText(Main.EntityManager.MouseEntity.Name);
                }
                else
                {
                    _toolTip.SetText("");
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, RasterizerState rasterizerState = null)
        {
            base.Draw(spriteBatch);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: TheGreen.UIScaleMatrix);
            _dragItem.Draw(spriteBatch);
            _toolTip.Draw(spriteBatch);
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
            return _inventoryOpen;
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
            _tileInventory = new Inventory(inventoryTileData.Cols, _dragItem, _toolTip, items, margin: 2, position: new Vector2(_inventoryMenu.Size.X + 25, 20 ), itemSlotColor: Color.Crimson);
            _inventoryTileDataCoordinates = coordinates;
            _inventoryTileData = inventoryTileData;
            AddContainerChild(_tileInventory);
        }
        public void SetInventoryOpen(bool open)
        {
            if (open == InventoryVisible())
                return;
            _inventoryOpen = open;
            if (open)
            {
                AddContainerChild(_inventoryMenu);
                AddContainerChild(_craftingMenu);
                RemoveContainerChild(_hotbar);
            }
            else
            {
                if (_toolTip.ItemSlotIndex != -1)
                {
                    _toolTip.ItemSlotIndex = -1;
                    _toolTip.SetText("");
                }
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
            item.Quantity -= 1;
            if (item.Quantity <= 0)
            {
                if (InventoryVisible())
                {
                    _dragItem.Item = null;
                }
                else
                {
                    _inventoryItems[_hotbar.Selected] = null;
                }
            }
            return itemUsed;
        }
        public Item AddItemToPlayerInventory(Item item)
        {
            return _inventoryMenu.AddItem(item);
        }
    }
}
