using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.Drawables;
using Vestige.Game.Entities;
using Vestige.Game.Input;
using Vestige.Game.Inventory;
using Vestige.Game.IO;
using Vestige.Game.Lighting;
using Vestige.Game.Menus;
using Vestige.Game.Renderers;
using Vestige.Game.Time;
using Vestige.Game.UI;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game
{
    /*
     Some important formulas for later reference

    Rotation over a distance:
    Rotation += (Velocity.X * delta) / Radius
     */

    /*
     Things to remember

    Modularity is for unique functionality
    There is no need to create a new class for every npc type,
    but unique npcs and items will need their own classes.

    Remove static references to classes before it becomes unmanageable. Code is officially stinky.
     */

    /// <summary>
    /// The instance of the game itself.
    /// </summary>
    public class Main
    {
        private GraphicsDevice _graphicsDevice;
        private static Matrix _translation;
        private TileRenderer _tileRenderer;
        public static LightEngine LightEngine;
        public static readonly Random Random = new Random();
        public static EntityManager EntityManager;
        private ParallaxManager _parallaxManager;
        public static GameClock GameClock;
        public static WorldGen World;
        private WorldFile _worldFile;
        private RenderTarget2D _bgTarget;
        private RenderTarget2D _gameTarget;
        private RenderTarget2D _liquidRenderTarget;
        private Texture2D _daytimeSkyGradient;
        private Vestige _gameHandle;
        private Player _localPlayer;

        public Main(Vestige gameHandle, WorldGen world, WorldFile worldFile, GraphicsDevice graphicsDevice)
        {
            _gameHandle = gameHandle;
            _tileRenderer = new TileRenderer();
            _daytimeSkyGradient = DebugHelper.GenerateVerticalGradient(graphicsDevice, [Color.Blue, Color.LightBlue], Vestige.NativeResolution.Y);
            LightEngine = new LightEngine(_graphicsDevice);
            EntityManager = new EntityManager();
            _parallaxManager = new ParallaxManager();
            GameClock = new GameClock();
            _worldFile = worldFile;
            World = world;
            _gameTarget = new RenderTarget2D(graphicsDevice, Vestige.NativeResolution.X * 2, Vestige.NativeResolution.Y * 2);
            _liquidRenderTarget = new RenderTarget2D(graphicsDevice, Vestige.NativeResolution.X * 2, Vestige.NativeResolution.Y * 2);
            _bgTarget = new RenderTarget2D(graphicsDevice, Vestige.NativeResolution.X, Vestige.NativeResolution.Y);
            InventoryManager inventory = new InventoryManager(_worldFile.GetPlayerItems(), 8);
            _localPlayer = new Player(inventory);
            InGameOptionsMenu inGameOptionsMenu = new InGameOptionsMenu(graphicsDevice);
            InGameUIHandler inGameUIHandler = new InGameUIHandler(inventory, inGameOptionsMenu, Vestige.NativeResolution.ToVector2());
            inGameOptionsMenu.AssignSaveAndQuitAction(() =>
            {
                inGameUIHandler.Dereference();
                InputManager.UnregisterHandler(_localPlayer);
                SaveAndQuit();
            });
            InputManager.RegisterHandler(_localPlayer);
            InputManager.RegisterHandler(inGameUIHandler);
            UIManager.RegisterContainer(inGameUIHandler);
            GameClock.StartGameClock(1000, 2000);
            World.InitializeGameUpdates();
            _localPlayer.InitializeGameUpdates(worldFile.GetSpawnTile());
            _graphicsDevice = graphicsDevice;

            //For the water shader
            _graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

            EntityManager.SetPlayer(_localPlayer);
            //EntityManager.CreateEnemy(0, _localPlayer.Position + new Vector2(500, -100));
            //EntityManager.CreateEnemy(0, _localPlayer.Position + new Vector2(-500, -100));
            
            _parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.MountainsBackground, new Vector2(0.01f, 0.001f), EntityManager.GetPlayer().Position, (World.SurfaceDepth + 20) * Vestige.TILESIZE, (World.SurfaceDepth - 80) * Vestige.TILESIZE));
            _parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFarthestBackground, new Vector2(0.1f, 0.06f), EntityManager.GetPlayer().Position, (World.SurfaceDepth + 5) * Vestige.TILESIZE, (World.SurfaceDepth - 30) * Vestige.TILESIZE));
            _parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFartherBackground, new Vector2(0.2f, 0.08f), EntityManager.GetPlayer().Position, (World.SurfaceDepth + 5) * Vestige.TILESIZE, (World.SurfaceDepth - 30) * Vestige.TILESIZE));
            _parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesBackground, new Vector2(0.3f, 0.1f), EntityManager.GetPlayer().Position, (World.SurfaceDepth + 5) * Vestige.TILESIZE, (World.SurfaceDepth - 30) * Vestige.TILESIZE));
        }
        public void Update(double delta)
        {
            GameClock.Update(delta);
            World.Update(delta);
            _parallaxManager.Update(delta, GetCameraPosition() + Vestige.ScreenCenter.ToVector2());
            EntityManager.Update(delta);
            CalculateTranslation();
        }
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Point drawBoxMin = (GetCameraPosition() / Vestige.TILESIZE).ToPoint();
            Point drawBoxMax = (GetCameraPosition() / Vestige.TILESIZE).ToPoint() + Vestige.DrawDistance;
            _tileRenderer.SetDrawBox(drawBoxMin, drawBoxMax);
            LightEngine.SetDrawBox(drawBoxMin, drawBoxMax);
            LightEngine.CalculateLightMap();
            float normalizedGlobalLight = (GameClock.GlobalLight - 50) / 205.0f;

            _graphicsDevice.SetRenderTarget(_bgTarget);
            _graphicsDevice.Clear(new Color((int)(50 * normalizedGlobalLight), (int)(109 * normalizedGlobalLight), (int)(255 * normalizedGlobalLight)));
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_daytimeSkyGradient, new Rectangle(Point.Zero, Vestige.NativeResolution), new Color(GameClock.GlobalLight, GameClock.GlobalLight, GameClock.GlobalLight));
            _parallaxManager.Draw(spriteBatch, new Color(GameClock.GlobalLight, GameClock.GlobalLight, GameClock.GlobalLight));
            spriteBatch.End();

            _graphicsDevice.SetRenderTarget(_gameTarget);
            _graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, transformMatrix: _translation * Matrix.CreateScale(2.0f), blendState: BlendState.AlphaBlend);
            _tileRenderer.DrawWalls(spriteBatch);
            _tileRenderer.DrawBackgroundTiles(spriteBatch);
            _tileRenderer.DrawTiles(spriteBatch);
            EntityManager.Draw(spriteBatch);
            spriteBatch.End();

            _graphicsDevice.SetRenderTarget(_liquidRenderTarget);
            _graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, transformMatrix: _translation * Matrix.CreateScale(2.0f), blendState: BlendState.AlphaBlend, effect: ContentLoader.WaterShader);
            ContentLoader.WaterShader.Parameters["BackgroundTexture"].SetValue(_gameTarget);
            ContentLoader.WaterShader.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            ContentLoader.WaterShader.Parameters["ModelMatrix"].SetValue(Matrix.Invert(_translation * Matrix.CreateScale(2.0f)));
            _tileRenderer.DrawLiquids(spriteBatch);
            spriteBatch.End();

            _graphicsDevice.SetRenderTarget(null);
            
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(_bgTarget, Vestige.RenderDestination, Color.White);
            spriteBatch.Draw(_gameTarget, Vestige.RenderDestination, Color.White);
            spriteBatch.Draw(_liquidRenderTarget, Vestige.RenderDestination, Color.White);
            spriteBatch.End();
        }
        public static Vector2 GetCameraPosition()
        {
            return new Vector2(Math.Abs(_translation.Translation.X), Math.Abs(_translation.Translation.Y));
        }
        public static Point GetMouseWorldPosition()
        {
            Vector2 mousePosition = (InputManager.GetMouseWindowPosition() - Vestige.RenderDestination.Location.ToVector2()) * new Vector2(Vestige.NativeResolution.X / (float)Vestige.RenderDestination.Width);
            Point translation = GetCameraPosition().ToPoint();
            return mousePosition.ToPoint() + translation;
        }
        private void CalculateTranslation()
        {
            Player player = EntityManager.GetPlayer();
            int dx = (int)Math.Round(Vestige.NativeResolution.X / 2 - player.Position.X);
            dx = MathHelper.Clamp(dx, -World.WorldSize.X * Vestige.TILESIZE + Vestige.NativeResolution.X, 0);
            int dy = (int)Math.Round(Vestige.NativeResolution.Y / 2 - player.Position.Y);
            dy = MathHelper.Clamp(dy, -World.WorldSize.Y * Vestige.TILESIZE + Vestige.NativeResolution.Y, 0);
            _translation = Matrix.CreateTranslation(dx, dy, 0f);
        }
        private void SaveAndQuit()
        {
            _worldFile.Save(World, _localPlayer);
            GameClock = null;
            LightEngine = null;
            EntityManager = null;
            World = null;
            _gameHandle.LoadMainMenu();
        }
    }
}
