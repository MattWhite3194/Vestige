using Microsoft.Xna.Framework;
using Vestige.Game.Input;
using Vestige.Game.Inventory;
using Vestige.Game.UI.Containers;

namespace Vestige.Game.Menus
{
    public class InGameUIHandler : UIContainer
    {
        private InventoryManager _inventoryManager;
        private UIContainer _optionsMenu;
        private UIContainer _activeMenu;
        public InGameUIHandler(InventoryManager inventoryManager, UIContainer optionsMenu, Vector2 size) : base(size: size, anchor: UI.Anchor.TopLeft)
        {
            _optionsMenu = optionsMenu;
            _inventoryManager = inventoryManager;
            AddContainerChild(inventoryManager);
            _activeMenu = inventoryManager;
        }
        public override void HandleInput(InputEvent @event)
        {
            if (@event.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.Options)
            {
                InputManager.MarkInputAsHandled(@event);
                if (_activeMenu == _inventoryManager)
                {
                    if (_inventoryManager.InventoryVisible())
                    {
                        _inventoryManager.SetInventoryOpen(false);
                    }
                    RemoveContainerChild(_inventoryManager);
                    AddContainerChild(_optionsMenu);
                    _activeMenu = _optionsMenu;
                }
                else
                {
                    RemoveContainerChild(_optionsMenu);
                    AddContainerChild(_inventoryManager);
                    _activeMenu = _inventoryManager;
                }
                return;
            }
            base.HandleInput(@event);
        }
    }
}
