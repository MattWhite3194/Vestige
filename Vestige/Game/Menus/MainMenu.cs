using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vestige.Game.Drawables;
using Vestige.Game.IO;
using Vestige.Game.UI;
using Vestige.Game.UI.Components;
using Vestige.Game.UI.Containers;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Menus
{
    //TODO: clear the delegate on the resolution selector when this Menu is removed, or keep a single main menu open
    public class MainMenu : UIContainer
    {
        private ParallaxManager parallaxManager;
        private Vector2 _parallaxOffset;
        private UIContainer _startMenu;
        private PanelContainer _createWorldMenu;
        private GridContainer _settingsMenu;
        private PanelContainer _loadGameMenu;
        private Button _backButton;
        private Stack<UIContainer> _subMenus;
        private Vestige _game;
        private TextBox _worldNameTextBox;
        private GraphicsDevice _graphicsDevice;
        private SelectionContainer _worldSizeSelector;
        private Func<List<(UIContainer, Dictionary<string, string>)>, List<UIContainer>> _worldSortMethod;
        private EventHandler _updateResolutionText;

        public MainMenu(Vestige gameHandle, GraphicsDevice graphicsDevice) : base(anchor: Anchor.None)
        {
            _parallaxOffset = new Vector2(0, Vestige.NativeResolution.Y);
            parallaxManager = new ParallaxManager();
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.MountainsBackground, new Vector2(2f, 0), _parallaxOffset, Vestige.NativeResolution.Y + 50, -1));
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFarthestBackground, new Vector2(30f, 1), _parallaxOffset, Vestige.NativeResolution.Y + 50, -1));
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFartherBackground, new Vector2(35f, 1), _parallaxOffset, Vestige.NativeResolution.Y + 50, -1));
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesBackground, new Vector2(40f, 1), _parallaxOffset, Vestige.NativeResolution.Y + 50, -1));
            _worldSortMethod = SortWorldsByDateDescending;
            _game = gameHandle;
            _graphicsDevice = graphicsDevice;
            _subMenus = new Stack<UIContainer>();
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
            Slider uiScaleSlider = new Slider(Vector2.Zero, new Vector2(288, 10), "UI Scale:", 50, 200, gameHandle.UserUIScale * 100, "%");
            uiScaleSlider.OnValueChanged += (value) =>
            {
                value = (int)value;
                _game.SetUIScale(value / 100);
            };
            _settingsMenu.AddComponentChild(uiScaleSlider);

            //TODO: add an apply button
            Button resolutionSelector = new Button(Vector2.Zero, $"{gameHandle.ScreenResolution.X} x {gameHandle.ScreenResolution.Y}", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            resolutionSelector.OnButtonPress += () =>
            {
                (int width, int height) = _game.GetNextSupportedResolution();
                gameHandle.SetResolution(width, height);
                resolutionSelector.SetText($"{width} x {height}");
            };
            _updateResolutionText = (sender, e) =>
            {
                resolutionSelector.SetText($"{gameHandle.ScreenResolution.X} x {gameHandle.ScreenResolution.Y}");
            };
            Vestige.GameWindow.ClientSizeChanged += (sender, e) => _updateResolutionText(sender, e);
            _settingsMenu.AddComponentChild(resolutionSelector);

            string fullScreenSelectorText = _game.IsFullScreen ? "Toggle Windowed" : "Toggle Fullscreen";
            Button fullScreenSelector = new Button(Vector2.Zero, fullScreenSelectorText, Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            fullScreenSelector.OnButtonPress += () =>
            {
                bool fullscreen = !_game.IsFullScreen;
                gameHandle.SetFullScreen(fullscreen);
                fullScreenSelector.SetText(fullscreen ? "Toggle Windowed" : "Toggle Fullscreen");
            };

            _settingsMenu.AddComponentChild(fullScreenSelector);

            //create world menu
            _worldNameTextBox = new TextBox(new Vector2(0, 0), "", Vector2.Zero, maxTextLength: 24, placeHolder: "Enter World Name:", maxWidth: 288, textAlign: TextAlign.Center);
            _createWorldMenu.AddComponentChild(_worldNameTextBox);

            _worldSizeSelector = new SelectionContainer(4, [(new Point(1050, 300), "Tiny"), (new Point(4200, 1200), "Small"), (new Point(6400, 1800), "Medium"), (new Point(8400, 2400), "Large")], Color.Gray, Color.White, Vestige.HighlightedTextColor, buttonWidth: 60, margin: 2, anchor: Anchor.MiddleMiddle);
            PanelContainer selectorPanel = new PanelContainer(new Vector2(0, 30), new Vector2(288, 30), Vestige.UIPanelColor, new Color(0, 0, 0, 255), 0, 1, 5, _graphicsDevice, anchor: Anchor.TopMiddle);
            selectorPanel.AddContainerChild(_worldSizeSelector);
            _createWorldMenu.AddContainerChild(selectorPanel);

            Button createWorldButton = new Button(new Vector2(0, 70), "Create World", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            createWorldButton.OnButtonPress += CreateWorld;
            _createWorldMenu.AddComponentChild(createWorldButton);

            Button worldGenTestButton = new Button(new Vector2(0, 100), "Test World Gen", Vector2.Zero, color: Color.Yellow, clickedColor: Color.Salmon, hoveredColor: Color.LightSalmon, maxWidth: 288);
            worldGenTestButton.OnButtonPress += () =>
            {
                Point worldSize = (Point)_worldSizeSelector.GetSelected();
                Utilities.RunWorldGenTest(worldSize.X, worldSize.Y, _graphicsDevice, 69);
            };
            _createWorldMenu.AddComponentChild(worldGenTestButton);

            _backButton = new Button(new Vector2(0, 0), "Back", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);
            _backButton.OnButtonPress += RemoveSubMenu;

            AddSubMenu(_startMenu);
        }
        public override void Update(double delta)
        {
            _parallaxOffset.X += (float)delta;
            parallaxManager.Update(delta, _parallaxOffset);
            base.Update(delta);
        }
        public override void Draw(SpriteBatch spriteBatch, RasterizerState rasterizerState = null)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState: SamplerState.LinearClamp, transformMatrix: Matrix.CreateScale(Size.X / (float)Vestige.NativeResolution.X));
            parallaxManager.Draw(spriteBatch, Color.White);
            spriteBatch.End();
            base.Draw(spriteBatch, rasterizerState);
        }
        private async void CreateWorld()
        {
            string worldName = _worldNameTextBox.GetText();
            WorldFile newWorldFile = new WorldFile();
            newWorldFile.SetPath(worldName);
            RemoveContainerChild(_createWorldMenu);
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
                AddContainerChild(_createWorldMenu);
                return;
            }
            Vestige.GameWindow.ClientSizeChanged -= (sender, e) => _updateResolutionText(sender, e);
            _game.StartGame(world, newWorldFile);
        }
        private void ListWorlds()
        {
            _loadGameMenu = new PanelContainer(Vector2.Zero, new Vector2(320, 330), Vestige.UIPanelColor, new Color(0, 0, 0, 255), 20, 1, 10, _graphicsDevice);
            //TODO: Attach sort method function as object to dropdown container, resort the world list on OnSelectionChanged.
            DropdownContainer sortMethodContainer = new DropdownContainer([(1, "Date Ascending"), (2, "Date Descending"), (3, "Name")], Color.White, Vestige.SelectedTextColor, Vestige.HighlightedTextColor, position: new Vector2(-20, 0), buttonWidth: 150, anchor: Anchor.BottomLeft);
            _loadGameMenu.AddContainerChild(sortMethodContainer);
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
                Vestige.GameWindow.ClientSizeChanged -= (sender, e) => _updateResolutionText(sender, e);
                _game.StartGame(world, worldFile);
            };
            deleteButton.OnButtonPress += () =>
            {
                UIContainer confirmDeletionMenu = CreateDeletionMenu(path);
                AddSubMenu(confirmDeletionMenu, false);
            };
            worldPanel.AddComponentChild(worldName);
            worldPanel.AddComponentChild(worldDate);
            worldPanel.AddComponentChild(worldButton);
            worldPanel.AddComponentChild(deleteButton);
            DateTime parsedDate = DateTime.ParseExact(worldMetaData["Date"], "MMM dd, yyyy - h:mm tt", CultureInfo.InvariantCulture);
            return (worldPanel, worldMetaData);
        }
        private UIContainer CreateDeletionMenu(string path)
        {
            PanelContainer confirmDeletionMenu = new PanelContainer(Vector2.Zero, new Vector2(288, 60), Vestige.UIPanelColor, Color.Black, 20, 1, 5, graphicsDevice: _graphicsDevice);

            Label confirmationTextLabel = new Label(Vector2.Zero, "Are you sure you want to deletethis? This cannot be undone.", Vector2.Zero, Color.White, 288);
            confirmDeletionMenu.AddComponentChild(confirmationTextLabel);

            Button cancelButton = new Button(new Vector2(40, 40), "Cancel", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 60);
            cancelButton.OnButtonPress += RemoveSubMenu;
            confirmDeletionMenu.AddComponentChild(cancelButton);

            Button deleteButton = new Button(new Vector2(188, 40), "Delete", Vector2.Zero, color: Color.Red, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 60);
            deleteButton.OnButtonPress += () =>
            {
                File.Delete(path);
                Directory.Delete(Path.GetDirectoryName(path));
                RemoveSubMenu();
                RemoveSubMenu();
                ListWorlds();
            };
            confirmDeletionMenu.AddComponentChild(deleteButton);

            return confirmDeletionMenu;
        }
        private void AddSubMenu(UIContainer menu, bool addBackButton = true)
        {
            if (_subMenus.Count > 0)
            {
                RemoveContainerChild(_subMenus.Peek());
                if (addBackButton)
                {
                    _backButton.Size = new Vector2(menu.Size.X, _backButton.Size.Y);
                    _backButton.Position = new Vector2(0, menu.Size.Y);
                    menu.AddComponentChild(_backButton);
                }
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
