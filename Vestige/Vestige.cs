using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using Vestige.Game;
using Vestige.Game.Input;
using Vestige.Game.IO;
using Vestige.Game.Menus;
using Vestige.Game.UI;
using Vestige.Game.WorldGeneration;

namespace Vestige
{
    public class Vestige : Microsoft.Xna.Framework.Game
    {
        private MainMenu _mainMenu;
        public static string SavePath;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Main _gameManager;
        public static Matrix UIScaleMatrix;
        /// <summary>
        /// The default scale of the UI in relation to the current screen resolution
        /// </summary>
        private float _defaultUIScale;
        /// <summary>
        /// The users selected UI scale, this will multiply with _defaultUIScale for the final scale
        /// </summary>
        private float _userUIScale;
        public float UserUIScale
        {
            get
            {
                return _userUIScale;
            }
        }
        public static Rectangle RenderDestination;
        public static GameWindow GameWindow;
        public static readonly Point NativeResolution = new Point(960, 640);
        public static readonly int TILESIZE = 16;
        public static readonly Point DrawDistance = new Point((960 / TILESIZE) + 1, (640 / TILESIZE) + 2);
        public static readonly float GRAVITY = 1300.0f;
        public static readonly Point ScreenCenter = new Point(960 / 2, 640 / 2);
        public static Settings Settings;
        public static readonly Color UIPanelColor = new Color(42, 45, 48, 196);
        public static readonly Color UIPanelColorOpaque = new Color(42, 45, 48, 255);
        public static readonly Color HighlightedTextColor = new Color(163, 213, 255, 255);
        public static readonly Color SelectedTextColor = new Color(120, 180, 230, 255);
        public Point ScreenResolution;
        //TODO: add resolution index to retrieve next resolution, so it doesn't get stuck on a resolution in fullscreen
        private List<Point> _supportedResolutions;
        private Point _maxScreenResolution;
        public bool IsFullScreen = false;
        /*
         Charcoal Gray - Color(42, 45, 48, 196)
         Steel Blue - Color(58, 74, 89, 196)
         Mist Gray - Color(94, 108, 116, 196)
         Icy Blue - Color(163, 213, 255, 255)
         Pine Green - Color(58, 90, 64, 255)
         Desaturated Gold - Color(191, 167, 111, 255)
         */

        public Vestige()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            GameWindow = Window;
            SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Vestige");
            //create save directory if it doesn't exist
            if (!Path.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
            Settings = new Settings(Path.Combine(SavePath, "config.json"));
        }

        protected override void Initialize()
        {
            GetSupportedDisplayModes();
            Settings.Load();
            //Necessary Conversion due to JSON saving 1.0 as an integer. So casting as a float when UIScale is saved at 100% will fail
            _userUIScale = Convert.ToSingle(Settings.Get("ui-scale"));
            SetWindowProperties((int)Settings.Get("screen-width"), (int)Settings.Get("screen-height"), false);
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (sender, e) => UpdateRenderDestination(Window.ClientBounds.Width, Window.ClientBounds.Height);
            //For unlimited fps:
            //IsFixedTimeStep = false;
            Utilities.Initialize(GraphicsDevice);
            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            ContentLoader.Load(Content);
            Mouse.SetCursor(MouseCursor.FromTexture2D(ContentLoader.Squircle, 0, 0));
            base.LoadContent();
        }
        protected override void BeginRun()
        {
            LoadMainMenu();
            //need to call this here because starting the application in fullscreen mode will disable window resizing when set back to windowed. Why, I have no clue.
            if ((bool)Settings.Get("fullscreen"))
            {
                SetFullScreen(true);
            }
        }
        protected override void Update(GameTime gameTime)
        {
            if (!IsActive)
            {
                base.Update(gameTime);
                return;
            }
            //get input
            InputManager.Update();

            //update ui
            UIManager.Update(gameTime.ElapsedGameTime.TotalSeconds);
            //update game
            _gameManager?.Update(gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);

        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _gameManager?.Draw(_spriteBatch, gameTime);

            UIManager.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
        public void SetResolution(int width, int height)
        {
            SetWindowProperties(width, height, _graphics.IsFullScreen);
        }
        public void SetFullScreen(bool fullscreen)
        {
            SetWindowProperties(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, fullscreen);
        }
        private void SetWindowProperties(int width, int height, bool fullscreen)
        {
            if (fullscreen != IsFullScreen)
            {
                if (!IsFullScreen)
                {
                    //going into fullscreen will clamp width and height to the supposed max monitor resolution. There are no other APIs to get cross platform resolutions unless I use SDL2
                    width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    _maxScreenResolution = new Point(width, height);
                    _graphics.PreferredBackBufferWidth = width;
                    _graphics.PreferredBackBufferHeight = height;
                    _graphics.ApplyChanges();
                }
                else
                {
                    //if toggling out of fullscreen, clamp width to bottom bar bounds
                    _graphics.IsFullScreen = fullscreen;
                    _graphics.ApplyChanges();
                    if (height >= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
                    {
                        height = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 0.97f);
                        width = Math.Min(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, width);
                    }
                }
            }
            //not allowed to set resolution to larger than the screen if in fullscreen. - Will break UI transform matrices
            if (fullscreen)
            {
                width = Math.Min(_maxScreenResolution.X, width);
                height = Math.Min(_maxScreenResolution.Y, height);
            }
            IsFullScreen = fullscreen;
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.IsFullScreen = fullscreen;
            _graphics.ApplyChanges();
            UpdateRenderDestination(width, height);
        }
        private void UpdateRenderDestination(int width, int height)
        {
            ScreenResolution = new Point(width, height);
            int xScale = (int)Math.Ceiling(width / (float)NativeResolution.X);
            int yScale = (int)Math.Ceiling(height / (float)NativeResolution.Y);
            _defaultUIScale = width / (float)NativeResolution.X;
            SetUIScale(_userUIScale);
            float scale = Math.Max(xScale, yScale);
            RenderDestination = new Rectangle(
                (width / 2) - ((int)(NativeResolution.X * scale) / 2),
                (height / 2) - ((int)(NativeResolution.Y * scale) / 2),
                (int)(NativeResolution.X * scale),
                (int)(NativeResolution.Y * scale)
                );
        }
        private void GetSupportedDisplayModes()
        {
            _supportedResolutions = new List<Point>();
            foreach (DisplayMode displayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (displayMode.Width < NativeResolution.X || displayMode.Height < NativeResolution.Y)
                    continue;
                _supportedResolutions.Add(new Point(displayMode.Width, displayMode.Height));
            }
        }
        public Point GetNextSupportedResolution()
        {
            int currentResolutionIndex = _supportedResolutions.IndexOf(ScreenResolution);
            return currentResolutionIndex != -1
                ? _supportedResolutions[(currentResolutionIndex + 1) % _supportedResolutions.Count]
                : _supportedResolutions[0];
        }
        public void SetUIScale(float scale)
        {
            UIScaleMatrix = Matrix.CreateScale(_defaultUIScale * scale);
            _userUIScale = scale;
            UIManager.OnUIScaleChanged(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        }
        public void LoadMainMenu()
        {
            _mainMenu = new MainMenu(this, GraphicsDevice);
            _gameManager = null;
            UIManager.RegisterContainer(_mainMenu);
            InputManager.RegisterHandler(_mainMenu);
        }
        public void StartGame(WorldGen world, WorldFile worldFile)
        {
            _mainMenu.Dereference();
            _gameManager = new Main(this, world, worldFile, GraphicsDevice);
        }
        public void QuitGame()
        {
            Settings.Set("screen-width", GraphicsDevice.PresentationParameters.BackBufferWidth);
            Settings.Set("screen-height", GraphicsDevice.PresentationParameters.BackBufferHeight);
            Settings.Set("fullscreen", _graphics.IsFullScreen);
            Settings.Set("ui-scale", _userUIScale);
            Settings.Save();
            Exit();
        }
    }
}
