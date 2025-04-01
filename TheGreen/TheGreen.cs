using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Main _gameManager;
        private MainMenu _mainMenu;
        public static Matrix ScreenScaleMatrix;

        public TheGreen()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            
            //Screen settings
            SetWindowProperties(1920, 1080, false);
            //For unlimited fps:
            IsFixedTimeStep = false;
            
            UpdateScreenScaleMatrix();
            base.Initialize();
            
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            ContentLoader.Load(Content);

            base.LoadContent();
        }
        protected override void BeginRun()
        {
            _mainMenu = new MainMenu(this, GraphicsDevice);
            UIManager.RegisterContainer(_mainMenu);
            InputManager.RegisterHandler(_mainMenu);
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //draw game
            _gameManager?.Draw(_spriteBatch);

            //draw UI
            _spriteBatch.Begin(transformMatrix: ScreenScaleMatrix, samplerState: SamplerState.PointClamp);
            UIManager.Draw(_spriteBatch);
            _spriteBatch.End();
            

            base.Draw(gameTime);
        }
        

        private void SetWindowProperties(int width, int height, bool fullScreen)
        {
            Globals.SetNativeResolution(new Point(width, height));
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.IsFullScreen = fullScreen;
            //_graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.ApplyChanges();
        }
        public void StartNewWorld(Point size)
        {
            _mainMenu = null;
            InventoryManager inventory = new InventoryManager(5, 8);
            WorldGen.World.GenerateWorld(size.X, size.Y);
            Player player = new Player(ContentLoader.PlayerTexture, (WorldGen.World.SpawnTile.ToVector2() - new Vector2(0, 5)) * Globals.TILESIZE, inventory, 100);
            _gameManager = new Main(player, GraphicsDevice);
            InputManager.RegisterHandler(inventory);
            UIManager.RegisterContainer(inventory);
        }
        private void UpdateScreenScaleMatrix()
        {
            ScreenScaleMatrix = Matrix.CreateScale(GraphicsDevice.PresentationParameters.BackBufferWidth / (float)Globals.NativeResolution.X);
        }
    }
}
