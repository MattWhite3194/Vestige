using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Vestige.Game.Entities;
using Vestige.Game.Input;
using Vestige.Game.Inventory;
using Vestige.Game.UI.Components;
using Vestige.Game.UI.Containers;
using Vestige.Game.WorldMap;

namespace Vestige.Game.Menus
{
    public class InGameMenu : UIContainer
    {
        private InventoryManager _inventoryManager;
        private UIContainer _activeMenu;
        private Main _gameManager;
        private CommandTerminal _commandTerminal;
        private Player _owner;
        private ChatDisplay _chatDisplay;
        private Button _saveAndQuitButton;
        private Button _backButton;
        private Stack<UIContainer> _subMenus;
        private UIContainer _optionsPanel;
        private MapMenu _mapMenu;
        public InGameMenu(Vestige gameHandle, Main gameManager, Player owner, Map map, InventoryManager inventoryManager, GraphicsDevice graphicsDevice) : base(anchor: UI.Anchor.None)
        {
            _gameManager = gameManager;
            _owner = owner;
            _inventoryManager = inventoryManager;
            _commandTerminal = new CommandTerminal(AddMessageToChat);
            _chatDisplay = new ChatDisplay(position: new Vector2(0, -_commandTerminal.Size.Y), anchor: UI.Anchor.BottomLeft);
            _activeMenu = inventoryManager;
            _mapMenu = new MapMenu(map);
            _commandTerminal.OnExitTerminal += () =>
            {
                RemoveContainerChild(_activeMenu);
                _commandTerminal.SetFocused(false);
                AddContainerChild(_inventoryManager);
                _activeMenu = _inventoryManager;
            };
            _subMenus = new Stack<UIContainer>();
            _optionsPanel = new PanelContainer(Vector2.Zero, new Vector2(288, 150), Vestige.UIPanelColor, new Color(0, 0, 0, 255), 20, 1, 10, graphicsDevice);
            GridContainer optionsGrid = new GridContainer(1, size: new Vector2(288, 150));

            PanelContainer settingsPanel = new PanelContainer(Vector2.Zero, new Vector2(288, 150), Vestige.UIPanelColor, new Color(0, 0, 0, 255), 20, 1, 10, graphicsDevice);
            GridContainer settingsGrid = new GridContainer(1, size: new Vector2(288, 150));

            _backButton = new Button(Vector2.Zero, "Back", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            _backButton.OnButtonPress += RemoveSubMenu;
            //options menu
            _saveAndQuitButton = new Button(Vector2.Zero, "Save and Quit", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            Button settingsButton = new Button(Vector2.Zero, "Settings", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            settingsButton.OnButtonPress += () =>
            {
                AddSubMenu(settingsPanel);
            };
            optionsGrid.AddComponentChild(_saveAndQuitButton);
            optionsGrid.AddComponentChild(settingsButton);
            _optionsPanel.AddContainerChild(optionsGrid);

            //settings menu
            Slider uiScaleSlider = new Slider(Vector2.Zero, new Vector2(288, 10), "UI Scale:", 50, 200, 100, "%");
            uiScaleSlider.OnValueChanged += (value) =>
            {
                value = (int)value;
                gameHandle.SetUIScale(value / 100);
            };
            settingsGrid.AddComponentChild(uiScaleSlider);

            //TODO: add an apply button
            Button resolutionSelector = new Button(Vector2.Zero, $"{gameHandle.ScreenResolution.X} x {gameHandle.ScreenResolution.Y}", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            resolutionSelector.OnButtonPress += () =>
            {
                (int width, int height) = gameHandle.GetNextSupportedResolution();
                gameHandle.SetResolution(width, height);
                resolutionSelector.SetText($"{width} x {height}");
            };
            settingsGrid.AddComponentChild(resolutionSelector);

            string fullScreenSelectorText = gameHandle.IsFullScreen ? "Toggle Windowed" : "Toggle Fullscreen";
            Button fullScreenSelector = new Button(Vector2.Zero, fullScreenSelectorText, Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            fullScreenSelector.OnButtonPress += () =>
            {
                bool fullscreen = !gameHandle.IsFullScreen;
                gameHandle.SetFullScreen(fullscreen);
                fullScreenSelector.SetText(fullscreen ? "Toggle Windowed" : "Toggle Fullscreen");
            };
            settingsGrid.AddComponentChild(fullScreenSelector);
            settingsPanel.AddContainerChild(settingsGrid);

            AddContainerChild(inventoryManager);
            AddContainerChild(_chatDisplay);
        }
        public void AddMessageToChat(string message)
        {
            _chatDisplay.AddMessage(message);
            _chatDisplay.UpdateAnchorMatrix((int)Size.X, (int)Size.Y, default);
        }
        public override void HandleInput(InputEvent @event)
        {
            //There has to be a cleaner way to do this
            base.HandleInput(@event);
            if (InputManager.IsEventHandled(@event))
                return;
            if (@event.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.Options)
            {
                if (_activeMenu == _inventoryManager)
                {
                    _inventoryManager.SetInventoryOpen(false);
                    RemoveContainerChild(_inventoryManager);
                    RemoveContainerChild(_chatDisplay);
                    ClearSubMenus();
                    AddSubMenu(_optionsPanel);
                    _activeMenu = _optionsPanel;
                    _gameManager.SetGameState(true);
                }
                else if (_activeMenu == _optionsPanel)
                {
                    RemoveContainerChild(_subMenus.Peek());
                    AddContainerChild(_chatDisplay);
                    AddContainerChild(_inventoryManager);
                    _activeMenu = _inventoryManager;
                    _gameManager.SetGameState(false);
                }
                else if (_activeMenu == _mapMenu)
                {
                    RemoveContainerChild(_mapMenu);
                    AddContainerChild(_chatDisplay);
                    AddContainerChild(_inventoryManager);
                    _activeMenu = _inventoryManager;
                }
                _owner.ClearInputs();
                InputManager.MarkInputAsHandled(@event);
                return;
            }
            else if (@event.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.Map && _activeMenu == _mapMenu)
            {
                RemoveContainerChild(_mapMenu);
                AddContainerChild(_chatDisplay);
                AddContainerChild(_inventoryManager);
                _activeMenu = _inventoryManager;
                InputManager.MarkInputAsHandled(@event);
                return;
            }
            else if (_activeMenu != _inventoryManager)
            {
                InputManager.MarkInputAsHandled(@event);
                return;
            }

            //open command prompt and map only when the inventory is visible
            if (@event.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.Terminal)
            {
                RemoveContainerChild(_activeMenu);
                AddContainerChild(_commandTerminal);
                _commandTerminal.SetFocused(true);
                _activeMenu = _commandTerminal;
                _owner.ClearInputs();
                InputManager.MarkInputAsHandled(@event);
                return;
            }
            else if (@event.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.Map)
            {
                RemoveContainerChild(_activeMenu);
                RemoveContainerChild(_chatDisplay);
                AddContainerChild(_mapMenu);
                _activeMenu = _mapMenu;
                _owner.ClearInputs();
                InputManager.MarkInputAsHandled(@event);
                return;
            }
        }
        public void AssignSaveAndQuitAction(Action saveAndQuitAction)
        {
            _saveAndQuitButton.OnButtonPress += () => saveAndQuitAction();
        }
        private void ClearSubMenus()
        {
            for (int i = 0; i < _subMenus.Count; i++)
            {
                _subMenus.Pop();
            }
        }
        private void AddSubMenu(UIContainer menu)
        {
            if (_subMenus.Count > 0)
            {
                RemoveContainerChild(_subMenus.Peek());
                _backButton.Size = new Vector2(menu.Size.X, _backButton.Size.Y);
                _backButton.Position = new Vector2(0, menu.Size.Y);
                menu.AddComponentChild(_backButton);
            }
            AddContainerChild(menu);
            _subMenus.Push(menu);
        }
        private void RemoveSubMenu()
        {
            UIContainer menu = _subMenus.Pop();
            menu.RemoveComponentChild(_backButton);
            RemoveContainerChild(menu);
            AddContainerChild(_subMenus.Peek());
        }
    }
}
