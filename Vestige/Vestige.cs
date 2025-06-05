using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Vestige.Game;
using Vestige.Game.Entities;
using Vestige.Game.Input;
using Vestige.Game.Inventory;
using Vestige.Game.IO;
using Vestige.Game.Menus;
using Vestige.Game.UI;
using Vestige.Game.WorldGeneration;

namespace Vestige
{
    public class Vestige : Microsoft.Xna.Framework.Game
    {
        public static string SavePath;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Main _gameManager;
        public static Matrix UIScaleMatrix;
        /// <summary>
        /// The default scale of the UI in relation to the current screen resolution
        /// </summary>
        public static float DefaultUIScale;
        public static Rectangle RenderDestination;
        public static GameWindow GameWindow;
        public static readonly Point NativeResolution = new Point(960, 640);
        public static readonly int TILESIZE = 16;
        public static readonly Point DrawDistance = new Point(960 / TILESIZE + 1, 640 / TILESIZE + 2);
        public static readonly float GRAVITY = 1300.0f;
        public static readonly Point ScreenCenter = new Point(960 / 2, 640 / 2);
        public static Point ScreenResolution;
        public static Settings Settings;
        public static readonly Color UIPanelColor = new Color(42, 45, 48, 196);
        public static readonly Color UIPanelColorOpaque = new Color(42, 45, 48, 255);
        public static readonly Color HighlightedTextColor = new Color(163, 213, 255, 255);
        public static readonly Color SelectedTextColor = new Color(120, 180, 230, 255);
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
            //Screen settings
            Settings.Load();
            SetWindowProperties((int)Settings.Get("screen-width"), (int)Settings.Get("screen-height"), (bool)Settings.Get("fullscreen"));
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnClientSizeChanged;
            //For unlimited fps:
            //IsFixedTimeStep = false;
            Utilities.Initialize(GraphicsDevice);
            base.Initialize();
        }
        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            Settings.Set("screen-width", GraphicsDevice.PresentationParameters.BackBufferWidth);
            Settings.Set("screen-height", GraphicsDevice.PresentationParameters.BackBufferHeight);
            UpdateRenderDestination(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            ContentLoader.Load(Content);

            base.LoadContent();
        }
        protected override void BeginRun()
        {
            LoadMainMenu();
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
            GraphicsDevice.Clear(Color.Gray);
            _gameManager?.Draw(_spriteBatch, gameTime);
            
            UIManager.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
        public void SetWindowProperties(int width, int height, bool fullScreen)
        {
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.IsFullScreen = fullScreen;
            Settings.Set("screen-width", width);
            Settings.Set("screen-height", height);
            //_graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.ApplyChanges();
            UpdateRenderDestination(width, height);
        }
        public void StartGame(WorldGen world, WorldFile worldFile)
        {
            _gameManager = new Main(this, world, worldFile, GraphicsDevice);
        }
        private void UpdateRenderDestination(int width, int height)
        {
            ScreenResolution = new Point(width, height);
            int xScale = (int)Math.Ceiling(width / (float)NativeResolution.X);
            int yScale = (int)Math.Ceiling(height / (float)NativeResolution.Y);
            DefaultUIScale = width / (float)NativeResolution.X;
            SetUIScaleMatrix(DefaultUIScale);
            float scale = Math.Max(xScale, yScale);
            RenderDestination = new Rectangle(
                width / 2 - (int)(NativeResolution.X * scale) / 2,
                height / 2 - (int)(NativeResolution.Y * scale) / 2,
                (int)(NativeResolution.X * scale),
                (int)(NativeResolution.Y * scale)
                );
        }
        public void SetUIScaleMatrix(float scale)
        {
            UIScaleMatrix = Matrix.CreateScale(scale);
            UIManager.OnUIScaleChanged(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        }
        public void LoadMainMenu()
        {
            _gameManager = null;
            MainMenu mainMenu = new MainMenu(this, GraphicsDevice);
        }
        public void QuitGame()
        {
            Settings.Save();
            this.Exit();
        }
    }
}
