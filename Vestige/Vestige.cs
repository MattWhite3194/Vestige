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

namespace Vestige
{
    public class Vestige : Microsoft.Xna.Framework.Game
    {
        public static string SavePath;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Main _gameManager;
        public static Matrix UIScaleMatrix;
        public static Rectangle RenderDestination;
        public static GameWindow GameWindow;
        public static Point NativeResolution = new Point(960, 640);
        public static readonly int TILESIZE = 16;
        public static Point DrawDistance = new Point(960 / TILESIZE + 1, 640 / TILESIZE + 2);
        public static readonly float GRAVITY = 1300.0f;
        public static Point ScreenCenter = new Point(960 / 2, 640 / 2);
        public static Point ScreenResolution;
        public static Settings Settings;

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
            Settings.load();
            SetWindowProperties((int)Settings.Get("screen-width"), (int)Settings.Get("screen-height"), (bool)Settings.Get("fullscreen"));
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnClientSizeChanged;
            //For unlimited fps:
            //IsFixedTimeStep = false;
            DebugHelper.Initialize(GraphicsDevice);
            base.Initialize();
        }
        private void OnClientSizeChanged(object sender, EventArgs e)
        {
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
            MainMenu mainMenu = new MainMenu(this, GraphicsDevice);
        }
        protected override void Update(GameTime gameTime)
        {
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
            GraphicsDevice.Clear(Color.BlanchedAlmond);
            _gameManager?.Draw(_spriteBatch, gameTime);
            
            UIManager.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
        public void SetWindowProperties(int width, int height, bool fullScreen)
        {
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.IsFullScreen = fullScreen;
            //_graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.ApplyChanges();
            UpdateRenderDestination(width, height);
        }
        public void StartGame(WorldFile worldFile)
        {
            InventoryManager inventory = new InventoryManager(5, 8);
            Player player = new Player(inventory);
            _gameManager = new Main(player, worldFile, GraphicsDevice);
        }
        private void UpdateRenderDestination(int width, int height)
        {
            ScreenResolution = new Point(width, height);
            int xScale = (int)Math.Ceiling(width / (float)NativeResolution.X);
            int yScale = (int)Math.Ceiling(height / (float)NativeResolution.Y);
            SetUIScaleMatrix(width / (float)NativeResolution.X);
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
    }
}
