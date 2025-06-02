using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vestige.Game.Input;
using Vestige.Game.UI.Components;
using Vestige.Game.UI.Containers;
using Vestige.Game.UI;
using Vestige.Game.WorldGeneration;
using System.Diagnostics;
using Vestige.Game.IO;
using System.Globalization;

namespace Vestige.Game.Menus
{
    public class MainMenu
    {
        private UIContainer _startMenu;
        private PanelContainer _createWorldMenu;
        private GridContainer _settingsMenu;
        private PanelContainer _loadGameMenu;
        private Button _backButton;
        private Stack<UIContainer> _menus;
        private Vestige _game;
        private MainMenuBackground _mainMenuBackground;
        private TextBox _worldNameTextBox;
        private GraphicsDevice _graphicsDevice;
        private SelectionContainer _worldSizeSelector;
        private Func<List<(UIContainer, Dictionary<string, string>)>, List<UIContainer>> _worldSortMethod;

        //TODO: Make each menu a separate UIComponentContainer, use this class to add and remove them from the UIManager and InputHandler
        //make the back button return menu a paramater in the menu declaration so the back button can be placed anywhere in the menu
        public MainMenu(Vestige gameHandle, GraphicsDevice graphicsDevice)
        {
            _worldSortMethod = SortWorldsByDateDescending;
            _game = gameHandle;
            _graphicsDevice = graphicsDevice;
            _menus = new Stack<UIContainer>();
            _startMenu = new UIContainer(position: new Vector2(0, 40), size: new Vector2(288, 800), anchor: Anchor.TopMiddle);
            _createWorldMenu = new PanelContainer(Vector2.Zero, new Vector2(288, 150), Vestige.UIPanelColor, new Color(0, 0, 0, 255), 20, 1, 10, _graphicsDevice);
            _settingsMenu = new GridContainer(1);
            
            //start menu
            Label _titleLabel = new Label(new Vector2(0, 0), "Vestige", Vector2.Zero, color: Vestige.HighlightedTextColor, scale: 4.0f, maxWidth: 288);
            _startMenu.AddComponentChild(_titleLabel);

            Button newGameButton = new Button(new Vector2(0, 140), "New Game", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            newGameButton.OnButtonPress += () => AddSubMenu(_createWorldMenu);
            _startMenu.AddComponentChild(newGameButton);

            Button loadGameButton = new Button(new Vector2(0, 160), "Load Game", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            loadGameButton.OnButtonPress += ListWorlds;
            _startMenu.AddComponentChild(loadGameButton);

            Button settingsMenuButton = new Button(new Vector2(0, 180), "Settings", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            settingsMenuButton.OnButtonPress += () => AddSubMenu(_settingsMenu);
            _startMenu.AddComponentChild(settingsMenuButton);

            Button exitButton = new Button(new Vector2(0, 200), "Exit", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            exitButton.OnButtonPress += gameHandle.QuitGame;
            _startMenu.AddComponentChild(exitButton);

            //settings menu
            Button reduceUIScaleButton = new Button(new Vector2(0, 0), "Reduce UI Scale", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            reduceUIScaleButton.OnButtonPress += () => 
            {
                _game.SetUIScaleMatrix(Math.Max(0.1f, Vestige.UIScaleMatrix.M11 - 0.1f));
            };
            _settingsMenu.AddComponentChild(reduceUIScaleButton);

            Button increaseUIScaleButton = new Button(new Vector2(0, 0), "Increase UI Scale", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            increaseUIScaleButton.OnButtonPress += () =>
            { 
                _game.SetUIScaleMatrix(Math.Min(5f, Vestige.UIScaleMatrix.M11 + 0.1f));
            };
            _settingsMenu.AddComponentChild(increaseUIScaleButton);

            Button resolutionSelector = new Button(new Vector2(0, 0), $"{Vestige.ScreenResolution.X} x {Vestige.ScreenResolution.Y}", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            resolutionSelector.OnButtonPress += () =>
            {
                (int x, int y) = Vestige.Settings.GetNextResolution();
                gameHandle.SetWindowProperties(x, y, false);
                resolutionSelector.SetText($"{x} x {y}");
            };
            _settingsMenu.AddComponentChild(resolutionSelector);
            
            //create world menu
            _worldNameTextBox = new TextBox(new Vector2(0, 0), "", Vector2.Zero, maxTextLength: 24, placeHolder: "Enter World Name:", maxWidth: 288);
            _createWorldMenu.AddComponentChild(_worldNameTextBox);

            _worldSizeSelector = new SelectionContainer(4, [(new Point(500, 500), "Tiny"), (new Point(4200, 1200), "Small"), (new Point(6400, 1800), "Medium"), (new Point(8400, 2400), "Large")], Color.Gray, Color.White, Vestige.HighlightedTextColor, buttonWidth: 60, margin: 2, anchor: Anchor.MiddleMiddle);
            PanelContainer selectorPanel = new PanelContainer(new Vector2(0, 30), new Vector2(288, 30), Vestige.UIPanelColor, new Color(0, 0, 0, 255), 0, 1, 5, _graphicsDevice, anchor: Anchor.TopMiddle);
            selectorPanel.AddContainerChild(_worldSizeSelector);
            _createWorldMenu.AddContainerChild(selectorPanel);

            Button createWorldButton = new Button(new Vector2(0, 70), "Create World", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            createWorldButton.OnButtonPress += CreateWorld;
            _createWorldMenu.AddComponentChild( createWorldButton );

            Button worldGenTestButton = new Button(new Vector2(0, 100), "Test World Gen", Vector2.Zero, color: Color.Yellow, clickedColor: Color.Salmon, hoveredColor: Color.LightSalmon, maxWidth: 288);
            worldGenTestButton.OnButtonPress += () =>
            {
                Point worldSize = (Point)_worldSizeSelector.GetSelected();
                DebugHelper.RunWorldGenTest(worldSize.X, worldSize.Y, _graphicsDevice, 69);
            };
            _createWorldMenu.AddComponentChild(worldGenTestButton);



            _backButton = new Button(new Vector2(0, 0), "Back", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
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
            WorldFile newWorldFile = new WorldFile();
            newWorldFile.SetPath( worldName );
            int numMenus = _menus.Count;
            UIManager.UnregisterContainer(_createWorldMenu);
            bool worldGenSuccessful = true;
            Point worldSize = (Point)_worldSizeSelector.GetSelected();
            WorldGen world = new WorldGen(worldSize.X, worldSize.Y);
            await Task.Run(() =>
            {
                world.GenerateWorld();
                try
                {
                    newWorldFile.Save(world);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
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
            _game.StartGame(world, newWorldFile);
        }
        private void ListWorlds()
        {
            _loadGameMenu = new PanelContainer(Vector2.Zero, new Vector2(320, 330), Vestige.UIPanelColor, new Color(0, 0, 0, 255), 20, 1, 10, _graphicsDevice);
            ScrollContainer worldList = new ScrollContainer(Vector2.Zero, size: new Vector2(320, 320), anchor: Anchor.TopMiddle);
            string savePath = Path.Combine(Vestige.SavePath, "Worlds");
            if (!Path.Exists(savePath))
                Directory.CreateDirectory(savePath);
            string[] worldDirectories = Directory.EnumerateDirectories(savePath).ToArray();
            List<(UIContainer worldContainer, Dictionary<string, string>)> worldContainersAndMetaData = new List<(UIContainer worldContainer, Dictionary<string, string>)>();
            for (int i = 0; i < worldDirectories.Length; i++)
            {
                string[] worldPaths = Directory.GetFiles(worldDirectories[i], "*.wld");
                if (worldPaths.Length == 0)
                    continue;
                worldContainersAndMetaData.Add(GetWorldContainer(worldPaths[0]));
            }
            foreach (UIContainer container in _worldSortMethod(worldContainersAndMetaData))
            {
                worldList.AddContainerChild(container);
            }
            _loadGameMenu.AddContainerChild(worldList);
            AddSubMenu(_loadGameMenu);
        }

        private List<UIContainer> SortWorldsByDateDescending(List<(UIContainer worldContainer, Dictionary<string, string> metaData)> worldContainers)
        {
            return worldContainers.OrderByDescending(s => DateTime.ParseExact(s.metaData["Date"], "MMM dd, yyyy - h:mm tt", CultureInfo.InvariantCulture)).Select(s => s.worldContainer).ToList();
        }
        private List<UIContainer> SortWorldsByDateAscending(List<(UIContainer worldContainer, Dictionary<string, string> metaData)> worldContainers)
        {
            return worldContainers.OrderBy(s => DateTime.ParseExact(s.metaData["Date"], "MMM dd, yyyy - h:mm tt", CultureInfo.InvariantCulture)).Select(s => s.worldContainer).ToList();
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
            _backButton.Size = new Vector2(menu.Size.X, _backButton.Size.Y);
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
        private (UIContainer, Dictionary<string, string>) GetWorldContainer(string path)
        {
            WorldFile worldFile = new WorldFile(path);
            Dictionary<string, string> worldMetaData = worldFile.GetMetaData();
            PanelContainer worldPanel = new PanelContainer(Vector2.Zero, new Vector2(300, 50), Vestige.UIPanelColor, new Color(0, 0, 0, 255), 0, 1, 5, _graphicsDevice, anchor: Anchor.TopMiddle);
            Label worldName = new Label(new Vector2(5, 5), worldMetaData["Name"], Vector2.Zero, color: Color.White, maxWidth: 288, textAlign: TextAlign.Left);
            Label worldDate = new Label(new Vector2(5, 25), worldMetaData["Date"], Vector2.Zero, color: Color.LightGray, maxWidth: 288, textAlign: TextAlign.Left);
            Button worldButton = new Button(new Vector2(235, 25), "Play", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 60);
            Button deleteButton = new Button(new Vector2(235, 05), "Delete", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Color.Red, maxWidth: 60);
            worldButton.OnButtonPress += () =>
            {
                WorldGen world = null;
                try
                {
                    world = worldFile.Load();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return;
                }
                int numMenus = _menus.Count;
                for (int i = 0; i < numMenus; i++)
                {
                    _menus.Pop().Dereference();
                }
                UIManager.UnregisterContainer(_mainMenuBackground);
                _game.StartGame(world, worldFile);
            };
            deleteButton.OnButtonPress += () =>
            {
                File.Delete(path);
                Directory.Delete(Path.GetDirectoryName(path));
                RemoveSubMenu();
                ListWorlds();
            };
            worldPanel.AddComponentChild(worldName);
            worldPanel.AddComponentChild(worldDate);
            worldPanel.AddComponentChild(worldButton);
            worldPanel.AddComponentChild(deleteButton);
            DateTime parsedDate = DateTime.ParseExact(worldMetaData["Date"], "MMM dd, yyyy - h:mm tt", CultureInfo.InvariantCulture);
            return (worldPanel, worldMetaData);
        }
    }
}
