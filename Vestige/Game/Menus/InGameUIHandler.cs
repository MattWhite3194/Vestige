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
        private Main _gameManager;
        public InGameUIHandler(Main gameManager, InventoryManager inventoryManager, UIContainer optionsMenu, Vector2 size) : base(size: size, anchor: UI.Anchor.None)
        {
            _optionsMenu = optionsMenu;
            _inventoryManager = inventoryManager;
            AddContainerChild(inventoryManager);
            _activeMenu = inventoryManager;
            _gameManager = gameManager;
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
                    _gameManager.SetGameState(true);
                }
                else
                {
                    RemoveContainerChild(_optionsMenu);
                    AddContainerChild(_inventoryManager);
                    _activeMenu = _inventoryManager;
                    _gameManager.SetGameState(false);
                }
                return;
            }
            base.HandleInput(@event);
        }
        //This is kind of hacky and goes against my system but whatever
        public override void UpdateAnchorMatrix(int parentWidth, int parentHeight, Matrix parentMatrix = default)
        {
            Size = Vestige.ScreenResolution.ToVector2();
            base.UpdateAnchorMatrix(parentWidth, parentHeight, parentMatrix);
        }
    }
}
