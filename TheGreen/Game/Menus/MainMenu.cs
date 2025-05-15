using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheGreen.Game.Input;
using TheGreen.Game.UI.Components;
using TheGreen.Game.UI.Containers;
using TheGreen.Game.UI;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game.Menus
{
    public class MainMenu
    {
        private UIContainer _startMenu;
        private GridContainer _createWorldMenu;
        private GridContainer _settingsMenu;
        private PanelContainer _loadGameMenu;
        private Button _backButton;
        private Stack<UIContainer> _menus;
        private TheGreen _game;
        private MainMenuBackground _mainMenuBackground;
        private TextBox _worldNameTextBox;
        private GraphicsDevice _graphicsDevice;

        //new selector class that has a list of options and will instantiate button components for each selection and store a variable that keeps track of the selected.

        //TODO: Make each menu a separate UIComponentContainer, use this class to add and remove them from the UIManager and InputHandler
        //make the back button return menu a paramater in the menu declaration so the back button can be placed anywhere in the menu
        public MainMenu(TheGreen game, GraphicsDevice graphicsDevice)
        {
            _game = game;
            _graphicsDevice = graphicsDevice;
            _menus = new Stack<UIContainer>();


            PanelContainer testPanelContainer = new PanelContainer(Vector2.Zero, new Vector2(288, 400), new Color(0, 179, 146, 196), new Color(0, 0, 0, 255), 10, 2, 20, graphicsDevice);


            _startMenu = new UIContainer(position: new Vector2(0, 40), size: new Vector2(288, 800), anchor: Anchor.TopMiddle);
            _createWorldMenu = new GridContainer(1);
            _settingsMenu = new GridContainer(1);

            Label _titleLabel = new Label(new Vector2(0, 0), "Yet Another Block Game", Vector2.Zero, textColor: Color.ForestGreen, scale: 4.0f, maxWidth: 288);
            _startMenu.AddComponentChild(_titleLabel);

            Button newGameButton = new Button(new Vector2(0, 140), "New Game", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 288);
            newGameButton.OnButtonPress += () => AddSubMenu(_createWorldMenu);
            _startMenu.AddComponentChild(newGameButton);

            Button loadGameButton = new Button(new Vector2(0, 160), "Load Game", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 288);
            loadGameButton.OnButtonPress += ListWorlds;
            _startMenu.AddComponentChild(loadGameButton);

            Button settingsMenuButton = new Button(new Vector2(0, 180), "Settings", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 288);
            settingsMenuButton.OnButtonPress += () => AddSubMenu(_settingsMenu);
            _startMenu.AddComponentChild(settingsMenuButton);

            Button worldGenTestButton = new Button(new Vector2(0, 288), "Test World Gen", Vector2.Zero, borderRadius: 0, textColor: Color.Red, textClickedColor: Color.Salmon, textHoveredColor: Color.LightSalmon, maxWidth: 288);
            worldGenTestButton.OnButtonPress += () => DebugHelper.RunWorldGenTest(4288, 1288, _graphicsDevice, 69);
            _startMenu.AddComponentChild(worldGenTestButton);

            Button reduceUIScaleButton = new Button(new Vector2(0, 0), "Reduce UI Scale", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 288);
            reduceUIScaleButton.OnButtonPress += () => 
            {
                _game.SetUIScaleMatrix(Math.Max(0.1f, TheGreen.UIScaleMatrix.M11 - 0.1f));
            };
            _settingsMenu.AddComponentChild(reduceUIScaleButton);

            Button increaseUIScaleButton = new Button(new Vector2(0, 0), "Increase UI Scale", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 288);
            increaseUIScaleButton.OnButtonPress += () =>
            { 
                _game.SetUIScaleMatrix(Math.Min(5f, TheGreen.UIScaleMatrix.M11 + 0.1f));
            };
            _settingsMenu.AddComponentChild(increaseUIScaleButton);

            _worldNameTextBox = new TextBox(new Vector2(0, 180), "", Vector2.Zero, maxTextLength: 24, placeHolder: "Enter World Name:", maxWidth: 288);
            _createWorldMenu.AddComponentChild(_worldNameTextBox);

            Button createWorldButton = new Button(new Vector2(0, 0), "Create World", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 288);
            createWorldButton.OnButtonPress += CreateWorld;
            _createWorldMenu.AddComponentChild( createWorldButton );

            _backButton = new Button(new Vector2(0, 0), "Back", Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 288);
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
                WorldGen.World.GenerateWorld(4200, 1200);
                worldGenSuccessful = WorldGen.World.SaveWorld(_worldNameTextBox.GetText());
            });
            UIManager.RegisterContainer(_createWorldMenu);

            if (!worldGenSuccessful)
            {
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
            _loadGameMenu = new PanelContainer(Vector2.Zero, new Vector2(288, 350), new Color(0, 179, 146, 196), new Color(0, 0, 0, 255), 20, 1, 20, _graphicsDevice);
            ScrollContainer worldList = new ScrollContainer(Vector2.Zero, 300, size: new Vector2(288, 0), anchor: Anchor.None);
            string worldPath = Path.Combine(TheGreen.SavePath, "Worlds");
            if (!Path.Exists(worldPath))
                return;
            string[] worldDirectories = Directory.EnumerateDirectories(worldPath).ToArray();
            foreach (string worldDirectory in worldDirectories)
            {
                string worldName = worldDirectory.Split('\\').Last();
                Button worldButton = new Button(Vector2.Zero, worldName, Vector2.Zero, borderRadius: 0, textColor: Color.White, textClickedColor: Color.Orange, textHoveredColor: Color.Yellow, maxWidth: 288);
                worldButton.OnButtonPress += () => LoadWorld(worldName);
                worldList.AddComponentChild(worldButton);
            }
            _loadGameMenu.AddContainerChild(worldList);
            AddSubMenu(_loadGameMenu);
        }
        private void AddSubMenu(UIContainer menu)
        {
            if (_menus.Count != 0)
            {
                UIManager.UnregisterContainer(_menus.Peek());
                InputManager.UnregisterHandler(_menus.Peek());
            }
            UIManager.RegisterContainer(menu);
            InputManager.RegisterHandler(menu);
            _menus.Push(menu);
            _backButton.Position = new Vector2(0, menu.Size.Y);
            menu.AddComponentChild(_backButton);
        }
        private void RemoveSubMenu()
        {
            UIManager.UnregisterContainer(_menus.Peek());
            InputManager.UnregisterHandler(_menus.Peek());
            _menus.Peek().RemoveComponentChild(_backButton);
            _menus.Pop();
            if (_menus.Count != 0)
            {
                UIManager.RegisterContainer(_menus.Peek());
                InputManager.RegisterHandler(_menus.Peek());
            }
        }
    }
}
