using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using TheGreen.Game;
using TheGreen.Game.Entities;
using TheGreen.Game.Input;
using TheGreen.Game.Inventory;
using TheGreen.Game.Menus;
using TheGreen.Game.UIComponents;
using TheGreen.Game.WorldGeneration;

namespace TheGreen
{

    public class TheGreen : Microsoft.Xna.Framework.Game
    {
        public static string SavePath;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Main _gameManager;
        private RenderTarget2D _gameTarget;
        public static Matrix UIScaleMatrix;
        public static Rectangle RenderDestination;
        public static GameWindow GameWindow;
        public static Point NativeResolution = new Point(960, 640);
        public static readonly int TILESIZE = 16;
        public static Point DrawDistance = new Point(960 / TILESIZE + 1, 640 / TILESIZE + 2);
        public static readonly float GRAVITY = 1400.0f;
        public static Point ScreenCenter = new Point(960 / 2, 640 / 2);

        public TheGreen()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            GameWindow = Window;
            SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TheGreen");
        }

        protected override void Initialize()
        {
            
            //Screen settings
            SetWindowProperties(1920, 1080, false);
            _gameTarget = new RenderTarget2D(GraphicsDevice, NativeResolution.X * 2, NativeResolution.Y * 2, false, SurfaceFormat.Color, DepthFormat.None);
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnClientSizeChanged;
            //For unlimited fps:
            IsFixedTimeStep = false;
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
            if (_gameManager != null)
            {
                GraphicsDevice.SetRenderTarget(_gameTarget);
                _gameManager.Draw(_spriteBatch);
                GraphicsDevice.SetRenderTarget(null);

                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _spriteBatch.Draw(_gameTarget, RenderDestination, Color.White);
                _spriteBatch.End();
            }
            
            UIManager.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
        

        private void SetWindowProperties(int width, int height, bool fullScreen)
        {
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.IsFullScreen = fullScreen;
            //_graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.ApplyChanges();
            UpdateRenderDestination(width, height);
        }
        public void StartGame()
        {
            InventoryManager inventory = new InventoryManager(5, 8);
            Player player = new Player(ContentLoader.PlayerTexture, inventory, 100);
            _gameManager = new Main(player, GraphicsDevice);
        }
        private void UpdateRenderDestination(int width, int height)
        {
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
