using Microsoft.Xna.Framework;
using Vestige.Game.Input;
using Vestige.Game.Inventory;
using Vestige.Game.UI.Containers;

namespace Vestige.Game.Menus.InGame
{
    public class InGameUIHandler : UIContainer
    {
        private InventoryManager _inventoryManager;
        private UIContainer _optionsMenu;
        private UIContainer _activeMenu;
        private Main _gameManager;
        private InGameTerminal _commandTerminal;
        private bool _justExitedTerminal = false;
        public InGameUIHandler(Main gameManager, InventoryManager inventoryManager, UIContainer optionsMenu, Vector2 size) : base(size: size, anchor: UI.Anchor.None)
        {
            _optionsMenu = optionsMenu;
            _inventoryManager = inventoryManager;
            AddContainerChild(inventoryManager);
            _activeMenu = inventoryManager;
            _gameManager = gameManager;
            _commandTerminal = new InGameTerminal();
            _commandTerminal.OnExitTerminal += (sender, e) =>
            {
                RemoveContainerChild(_activeMenu);
                _commandTerminal.SetFocused(false);
                AddContainerChild(_inventoryManager);
                _activeMenu = _inventoryManager;
                _justExitedTerminal = true;
            };
        }
        public override void HandleInput(InputEvent @event)
        {
            if (@event.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.Options)
            {
                if (_justExitedTerminal)
                {
                    _justExitedTerminal = false;
                    return;
                }
                else if (_activeMenu == _inventoryManager)
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
                else if (_activeMenu == _optionsMenu)
                {
                    RemoveContainerChild(_optionsMenu);
                    AddContainerChild(_inventoryManager);
                    _activeMenu = _inventoryManager;
                    _gameManager.SetGameState(false);
                }
                Main.EntityManager.GetPlayer().ClearInputs();
                InputManager.MarkInputAsHandled(@event);
                return;
            }
            else if (_activeMenu == _commandTerminal)
            {
                InputManager.MarkInputAsHandled(@event);
                return;
            }
            else if (@event.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.Terminal && _activeMenu == _inventoryManager)
            {
                RemoveContainerChild(_activeMenu);
                AddContainerChild(_commandTerminal);
                _commandTerminal.SetFocused(true);
                _activeMenu = _commandTerminal;
                Main.EntityManager.GetPlayer().ClearInputs();
                InputManager.MarkInputAsHandled(@event);
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
