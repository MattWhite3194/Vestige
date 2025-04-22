using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheGreen.Game.Input;
using TheGreen.Game.UI;
using TheGreen.Game.UI.Components;
using TheGreen.Game.UI.Containers;
using TheGreen.Game.UIComponents;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Menus
{
    public class MainMenu
    {
        private UIComponentContainer _startMenu;
        private GridContainer _createWorldMenu;
        private GridContainer _settingsMenu;
        private ScrollContainer _loadGameMenu;
        private Button _backButton;
        private Stack<UIComponentContainer> _menus;
        private TheGreen _game;
        private MainMenuBackground _mainMenuBackground;
        private TextBox _worldNameTextBox;
        private GraphicsDevice _graphicsDevice;

        //new selector class that has a list of options and will instantiate button components for each selection and store a variable that keeps track of the selected.

        //TODO: export each menu to its own class so this doesn't become a nightmare file

        //TODO: base menu class, extend for other menus
        public MainMenu(TheGreen game, GraphicsDevice graphicsDevice)
        {
            _game = game;
            _graphicsDevice = graphicsDevice;
            _menus = new Stack<UIComponentContainer>();


            _startMenu = new UIComponentContainer(position: new Vector2(0, 40), anchor: Anchor.TopMiddle);
            _createWorldMenu = new GridContainer(1);
            _createWorldMenu.Anchor = Anchor.MiddleMiddle;
            _settingsMenu = new GridContainer(1);
            _settingsMenu.Anchor = Anchor.MiddleMiddle;

            Label _titleLabel = new Label(new Vector2(0, 0), "The Green", Vector2.Zero, textColor: Color.ForestGreen, scale: 5.0f, maxWidth: 360);
            _startMenu.AddUIComponent(_titleLabel);

            Button newGameButton = new Button(new Vector2(0, 140), "New Game", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 360);
            newGameButton.OnButtonPress += () => AddSubMenu(_createWorldMenu);
            _startMenu.AddUIComponent(newGameButton);

            Button loadGameButton = new Button(new Vector2(0, 160), "Load Game", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 360);
            loadGameButton.OnButtonPress += ListWorlds;
            _startMenu.AddUIComponent(loadGameButton);

            Button settingsMenuButton = new Button(new Vector2(0, 180), "Settings", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 360);
            settingsMenuButton.OnButtonPress += () => AddSubMenu(_settingsMenu);
            _startMenu.AddUIComponent(settingsMenuButton);

            Button worldGenTestButton = new Button(new Vector2(0, 240), "Test World Gen", Vector2.Zero, borderRadius: 0, textColor: Color.Red, textClickedColor: Color.Salmon, textHoveredColor: Color.LightSalmon, maxWidth: 360);
            worldGenTestButton.OnButtonPress += () => DebugHelper.RunWorldGenTest(4360, 1360, _graphicsDevice, 69);
            _startMenu.AddUIComponent(worldGenTestButton);

            Button reduceUIScaleButton = new Button(new Vector2(0, 0), "Reduce UI Scale", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 360);
            reduceUIScaleButton.OnButtonPress += () => 
            {
                _game.SetUIScaleMatrix(Math.Max(0.1f, TheGreen.UIScaleMatrix.M11 - 0.1f));
            };
            _settingsMenu.AddUIComponent(reduceUIScaleButton);

            Button increaseUIScaleButton = new Button(new Vector2(0, 0), "Increase UI Scale", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 360);
            increaseUIScaleButton.OnButtonPress += () =>
            { 
                _game.SetUIScaleMatrix(Math.Min(5f, TheGreen.UIScaleMatrix.M11 + 0.1f));
            };
            _settingsMenu.AddUIComponent(increaseUIScaleButton);

            _worldNameTextBox = new TextBox(new Vector2(0, 180), "", Vector2.Zero, maxTextLength: 24, placeHolder: "Enter World Name:", maxWidth: 360);
            _createWorldMenu.AddUIComponent(_worldNameTextBox);

            Button createWorldButton = new Button(new Vector2(0, 0), "Create World", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 360);
            createWorldButton.OnButtonPress += CreateWorld;
            _createWorldMenu.AddUIComponent( createWorldButton );

            _backButton = new Button(new Vector2(0, 0), "Back", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 360);
            _backButton.OnButtonPress += RemoveSubMenu;

            _mainMenuBackground = new MainMenuBackground();
            UIManager.RegisterContainer( _mainMenuBackground );
            UIManager.RegisterContainer(_startMenu);
            InputManager.RegisterHandler(_startMenu);
            _menus.Push(_startMenu);
        }
        private async void CreateWorld()
        {
            bool worldGenSuccessful = true;
            int numMenus = _menus.Count;
            UIManager.UnregisterContainer(_createWorldMenu);
            await Task.Run(() =>
            {
                WorldGen.World.GenerateWorld(4360, 1360);
                worldGenSuccessful = WorldGen.World.SaveWorld(_worldNameTextBox.GetText());
            });
            if (!worldGenSuccessful)
            {
                UIManager.RegisterContainer(_createWorldMenu);
                return;
            }
            for (int i = 0; i < numMenus; i++)
            {
                _menus.Pop().Dereference();
            }
            UIManager.UnregisterContainer(_mainMenuBackground);
            _game.StartGame();
        }
        private void LoadWorld(string worldName)
        {
            bool worldLoadingSuccessful = WorldGen.World.LoadWorld(worldName);
            if (!worldLoadingSuccessful)
            {
                return;
            }
            int numMenus = _menus.Count;
            for (int i = 0; i < numMenus; i++)
            {
                _menus.Pop().Dereference();
            }
            UIManager.UnregisterContainer(_mainMenuBackground);
            _game.StartGame();
        }
        private void ListWorlds()
        {
            _loadGameMenu = new ScrollContainer(Vector2.Zero, 100, size: new Vector2(432, 0));
            string worldPath = Path.Combine(TheGreen.SavePath, "Worlds");
            if (!Path.Exists(worldPath))
                return;
            string[] worldDirectories = Directory.EnumerateDirectories(worldPath).ToArray();
            foreach (string worldDirectory in worldDirectories)
            {
                string worldName = worldDirectory.Split('\\').Last();
                Button worldButton = new Button(Vector2.Zero, worldName, Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 360);
                worldButton.OnButtonPress += () => LoadWorld(worldName);
                _loadGameMenu.AddUIComponent(worldButton);
            }
            AddSubMenu(_loadGameMenu);
        }
        private void AddSubMenu(UIComponentContainer menu)
        {
            if (_menus.Count != 0)
            {
                UIManager.UnregisterContainer(_menus.Peek());
                InputManager.UnregisterHandler(_menus.Peek());
            }
            UIManager.RegisterContainer(menu);
            InputManager.RegisterHandler(menu);
            _menus.Push(menu);
            menu.AddUIComponent(_backButton);
        }
        private void RemoveSubMenu()
        {
            UIManager.UnregisterContainer(_menus.Peek());
            InputManager.UnregisterHandler(_menus.Peek());
            _menus.Peek().RemoveUIComponent(_backButton);
            _menus.Pop();
            if (_menus.Count != 0)
            {
                UIManager.RegisterContainer(_menus.Peek());
                InputManager.RegisterHandler(_menus.Peek());
            }
        }
    }
}
