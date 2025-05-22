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
using System.Diagnostics;

namespace TheGreen.Game.Menus
{
    public class MainMenu
    {
        private UIContainer _startMenu;
        private PanelContainer _createWorldMenu;
        private GridContainer _settingsMenu;
        private PanelContainer _loadGameMenu;
        private Button _backButton;
        private Stack<UIContainer> _menus;
        private TheGreen _game;
        private MainMenuBackground _mainMenuBackground;
        private TextBox _worldNameTextBox;
        private GraphicsDevice _graphicsDevice;
        private SelectionContainer _worldSizeSelector;

        //new selector class that has a list of options and will instantiate button components for each selection and store a variable that keeps track of the selected.

        //TODO: Make each menu a separate UIComponentContainer, use this class to add and remove them from the UIManager and InputHandler
        //make the back button return menu a paramater in the menu declaration so the back button can be placed anywhere in the menu
        public MainMenu(TheGreen game, GraphicsDevice graphicsDevice)
        {
            _game = game;
            _graphicsDevice = graphicsDevice;
            _menus = new Stack<UIContainer>();
            _startMenu = new UIContainer(position: new Vector2(0, 40), size: new Vector2(288, 800), anchor: Anchor.TopMiddle);
            _createWorldMenu = new PanelContainer(Vector2.Zero, new Vector2(288, 150), new Color(0, 179, 146, 196), new Color(0, 0, 0, 255), 20, 1, 10, _graphicsDevice);
            _settingsMenu = new GridContainer(1);


            //start menu
            Label _titleLabel = new Label(new Vector2(0, 0), "Yet Another Block Game", Vector2.Zero, color: Color.ForestGreen, scale: 4.0f, maxWidth: 288);
            _startMenu.AddComponentChild(_titleLabel);

            Button newGameButton = new Button(new Vector2(0, 140), "New Game", Vector2.Zero, color: Color.White, clickedColor: Color.Orange, hoveredColor: Color.Yellow, maxWidth: 288);
            newGameButton.OnButtonPress += () => AddSubMenu(_createWorldMenu);
            _startMenu.AddComponentChild(newGameButton);

            Button loadGameButton = new Button(new Vector2(0, 160), "Load Game", Vector2.Zero, color: Color.White, clickedColor: Color.Orange, hoveredColor: Color.Yellow, maxWidth: 288);
            loadGameButton.OnButtonPress += ListWorlds;
            _startMenu.AddComponentChild(loadGameButton);

            Button settingsMenuButton = new Button(new Vector2(0, 180), "Settings", Vector2.Zero, color: Color.White, clickedColor: Color.Orange, hoveredColor: Color.Yellow, maxWidth: 288);
            settingsMenuButton.OnButtonPress += () => AddSubMenu(_settingsMenu);
            _startMenu.AddComponentChild(settingsMenuButton);

            Button worldGenTestButton = new Button(new Vector2(0, 288), "Test World Gen", Vector2.Zero, color: Color.Red, clickedColor: Color.Salmon, hoveredColor: Color.LightSalmon, maxWidth: 288);
            worldGenTestButton.OnButtonPress += () => DebugHelper.RunWorldGenTest(4288, 1288, _graphicsDevice, 69);
            _startMenu.AddComponentChild(worldGenTestButton);


            //settings menu
            Button reduceUIScaleButton = new Button(new Vector2(0, 0), "Reduce UI Scale", Vector2.Zero, color: Color.White, clickedColor: Color.Orange, hoveredColor: Color.Yellow, maxWidth: 288);
            reduceUIScaleButton.OnButtonPress += () => 
            {
                _game.SetUIScaleMatrix(Math.Max(0.1f, TheGreen.UIScaleMatrix.M11 - 0.1f));
            };
            _settingsMenu.AddComponentChild(reduceUIScaleButton);

            Button increaseUIScaleButton = new Button(new Vector2(0, 0), "Increase UI Scale", Vector2.Zero, color: Color.White, clickedColor: Color.Orange, hoveredColor: Color.Yellow, maxWidth: 288);
            increaseUIScaleButton.OnButtonPress += () =>
            { 
                _game.SetUIScaleMatrix(Math.Min(5f, TheGreen.UIScaleMatrix.M11 + 0.1f));
            };
            _settingsMenu.AddComponentChild(increaseUIScaleButton);

            
            //create world menu
            _worldNameTextBox = new TextBox(new Vector2(0, 0), "", Vector2.Zero, maxTextLength: 24, placeHolder: "Enter World Name:", maxWidth: 288);
            _createWorldMenu.AddComponentChild(_worldNameTextBox);

            _worldSizeSelector = new SelectionContainer(3, [(new Point(4200, 1200), "Small"), (new Point(6400, 1800), "Medium"), (new Point(8400, 2400), "Large")], Color.LightGray, Color.Yellow, buttonWidth: 80, anchor: Anchor.MiddleMiddle);
            PanelContainer selectorPanel = new PanelContainer(new Vector2(0, 30), new Vector2(288, 30), new Color(0, 179, 146, 196), new Color(0, 0, 0, 255), 0, 1, 5, _graphicsDevice, anchor: Anchor.TopMiddle);
            selectorPanel.AddContainerChild(_worldSizeSelector);
            _createWorldMenu.AddContainerChild(selectorPanel);

            Button createWorldButton = new Button(new Vector2(0, 70), "Create World", Vector2.Zero, color: Color.White, clickedColor: Color.Orange, hoveredColor: Color.Yellow, maxWidth: 288);
            createWorldButton.OnButtonPress += CreateWorld;
            _createWorldMenu.AddComponentChild( createWorldButton );

            

            _backButton = new Button(new Vector2(0, 0), "Back", Vector2.Zero, color: Color.White, clickedColor: Color.Orange, hoveredColor: Color.Yellow, maxWidth: 288);
            _backButton.OnButtonPress += RemoveSubMenu;

            _mainMenuBackground = new MainMenuBackground();
            UIManager.RegisterContainer( _mainMenuBackground );
            UIManager.RegisterContainer(_startMenu);
            InputManager.RegisterHandler(_startMenu);
            _menus.Push(_startMenu);
        }
        private async void CreateWorld()
        {
            string worldName = _worldNameTextBox.GetText();
            if (string.IsNullOrEmpty(worldName))
            {
                worldName = "New World";
            }
            string fileName = GetValidWorldName(worldName);
            int numMenus = _menus.Count;
            UIManager.UnregisterContainer(_createWorldMenu);
            bool worldGenSuccessful = true;
            await Task.Run(() =>
            {
                Point worldSize = (Point)_worldSizeSelector.GetSelected();
                WorldGen.World.GenerateWorld(worldSize.X, worldSize.Y);
                try
                {
                    WorldGen.World.SaveWorld(fileName, worldName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}");
                    worldGenSuccessful = false;
                }
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
        private string GetValidWorldName(string worldName)
        {
            //Replace forbidden chars
            foreach (char forbiddenChar in Path.GetInvalidFileNameChars())
            {
                worldName = worldName.Replace(forbiddenChar, '-');
            }
            foreach (char forbiddenChar in Path.GetInvalidPathChars())
            {
                worldName = worldName.Replace(forbiddenChar, '-');
            }
            worldName = worldName.Replace('.', '_');

            //if the world name already exists, iterate it until a new filename is found
            int nameIteration = 1;
            string worldPath = Path.Combine(TheGreen.SavePath, "Worlds", worldName);
            string iteratedWorldName = worldName;
            while (Path.Exists(worldPath))
            {
                iteratedWorldName = worldName + nameIteration;
                nameIteration++;
                worldPath = Path.Combine(TheGreen.SavePath, "Worlds", iteratedWorldName);
            }
            worldName = iteratedWorldName;
            return worldName;
        }
        private void LoadWorld(string worldName)
        {
            try
            {
                WorldGen.World.LoadWorld(worldName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
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
            _loadGameMenu = new PanelContainer(Vector2.Zero, new Vector2(288, 350), new Color(0, 179, 146, 196), new Color(0, 0, 0, 255), 20, 1, 10, _graphicsDevice);
            ScrollContainer worldList = new ScrollContainer(Vector2.Zero, size: new Vector2(288, 300), anchor: Anchor.TopLeft);
            string worldPath = Path.Combine(TheGreen.SavePath, "Worlds");
            if (!Path.Exists(worldPath))
                return;
            string[] worldDirectories = Directory.EnumerateDirectories(worldPath).ToArray();
            foreach (string worldDirectory in worldDirectories)
            {
                string worldName = worldDirectory.Split('\\').Last();
                Button worldButton = new Button(Vector2.Zero, worldName, Vector2.Zero, color: Color.White, clickedColor: Color.Orange, hoveredColor: Color.Yellow, maxWidth: 288);
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
