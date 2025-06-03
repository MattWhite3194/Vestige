using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.Input;
using Vestige.Game.Items;
using Vestige.Game.Tiles.TileData;
using Vestige.Game.UI;
using Vestige.Game.UI.Components;
using Vestige.Game.UI.Containers;

namespace Vestige.Game.Inventory
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
        public InventoryManager(Item[] playerItems, int cols) : base(anchor: Anchor.TopLeft)
        {
            _inventoryItems = playerItems;
            if (_inventoryItems == null)
            {
                _inventoryItems = new Item[5 * cols];
                _inventoryItems[0] = Item.InstantiateItemByID(2);
                _inventoryItems[1] = Item.InstantiateItemByID(6);
            }

            _toolTip = new ToolTip();
            _dragItem = new DragItem(Vector2.Zero);
            _inventoryMenu = new Inventory(cols, _dragItem, _toolTip, _inventoryItems, margin: 2, position: new Vector2(20, 20), itemSlotColor: new Color(70, 85, 100, 230));
            _hotbar = new Hotbar(cols, _inventoryItems, margin: 2, itemSlotColor: new Color(70, 85, 100, 220), highLightedColor: Vestige.HighlightedTextColor, position: new Vector2(20, 20));
            _craftingMenu = new CraftingGrid(3, _dragItem, _toolTip, margin: 2, position: new Vector2(20, _inventoryMenu.Size.Y + 25), itemSlotColor: new Color(222, 184, 135, 230), anchor: Anchor.TopLeft);
            _inventoryOpen = false;
            AddContainerChild(_hotbar);
        }
        public override void HandleInput(InputEvent @event)
        {
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
                        Vector2 position = Main.EntityManager.GetPlayer().Position + Main.EntityManager.GetPlayer().Size / 2;
                        Main.EntityManager.CreateItemDrop(itemDrop, position, velocity, false);
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
                Vector2 position = Main.EntityManager.GetPlayer().Position + Main.EntityManager.GetPlayer().Size / 2;
                int direction = Math.Sign(Main.GetMouseWorldPosition().X - position.X);
                Vector2 itemVelocity = new Vector2(direction * 50, 0);
                Main.EntityManager.CreateItemDrop(_dragItem.Item, position, itemVelocity, false);
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
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Vestige.UIScaleMatrix);
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
                _inventoryTileData.CloseInventory(Main.World, _inventoryTileDataCoordinates.X, _inventoryTileDataCoordinates.Y);
            }
            _tileInventory = new Inventory(inventoryTileData.Cols, _dragItem, _toolTip, items, margin: 2, position: new Vector2(_inventoryMenu.Size.X + 25, 20 ), itemSlotColor: new Color(75, 70, 60, 220));
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
                        _inventoryTileData.CloseInventory(Main.World, _inventoryTileDataCoordinates.X, _inventoryTileDataCoordinates.Y);
                        _inventoryTileData = null;
                    }
                }
            }
        }
        public void UseSelected()
        {
            Item item = GetSelected();
            if (item == null)
                return;
            bool itemUsed = item.UseItem(Main.EntityManager.GetPlayer());
            if (!item.Stackable || !itemUsed)
                return;
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
        }
        public Item AddItemToPlayerInventory(Item item)
        {
            return _inventoryMenu.AddItem(item);
        }
        public Item[] GetItems()
        {
            return _inventoryItems;
        }
        public bool ConsumeAmmo(int ammoType)
        {
            //TODO: Implementation
            return false;
        }
    }
}
