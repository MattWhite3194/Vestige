using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
using Vestige.Game.WorldMap;

namespace Vestige.Game
{
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
        public static Map map;
        private WorldFile _worldFile;
        private RenderTarget2D _bgTarget;
        private RenderTarget2D _wallTarget;
        private RenderTarget2D _gameTarget;
        private RenderTarget2D _liquidRenderTarget;
        public static Texture2D DayTimeSkyGradient;
        private Vestige _gameHandle;
        private Player _localPlayer;
        private SunMoon _sunMoon;
        private bool _gamePaused;
        private Map _map;
        public bool SmoothLighting;
        public bool ShowMiniMap;

        public Main(Vestige gameHandle, WorldGen world, WorldFile worldFile, GraphicsDevice graphicsDevice)
        {
            _gameHandle = gameHandle;
            World = world;
            _graphicsDevice = graphicsDevice;
            _tileRenderer = new TileRenderer(graphicsDevice);
            DayTimeSkyGradient = Utilities.GenerateVerticalGradient(graphicsDevice, [Color.Blue, Color.LightBlue], Vestige.NativeResolution.Y);
            LightEngine = new LightEngine(_graphicsDevice);
            EntityManager = new EntityManager();
            _parallaxManager = new ParallaxManager();
            GameClock = new GameClock();
            _worldFile = worldFile;
            _localPlayer = new Player();
            InventoryManager inventory = new InventoryManager(_localPlayer, _worldFile.GetPlayerItems(), 8);
            _localPlayer.Inventory = inventory;
            SmoothLighting = (bool)Vestige.Settings.Get("smooth-lighting");
            ShowMiniMap = (bool)Vestige.Settings.Get("show-minimap");

            _sunMoon = new SunMoon(ContentLoader.SunMoonTexture, Vector2.Zero);
            gameHandle.OnRenderDestinationUpdated += UpdateRenderTargets;
            UpdateRenderTargets();

            _map = new Map(World, graphicsDevice);
            //TEMPORARY: This reveals all tiles in the world at the start.
            _map.RevealAllMapTiles();
            InGameMenu inGameUIHandler = new InGameMenu(gameHandle, this, _localPlayer, _map, inventory, graphicsDevice);
            inGameUIHandler.AssignSaveAndQuitAction(() =>
            {
                inGameUIHandler.Dereference();
                InputManager.UnregisterHandler(_localPlayer);
                SaveAndQuit();
            });
            InputManager.RegisterHandler(_localPlayer);
            InputManager.RegisterHandler(inGameUIHandler);
            UIManager.RegisterContainer(inGameUIHandler);

            GameClock.SetGameClock(1000, 2000);
            _localPlayer.InitializeGameUpdates(worldFile.GetSpawnTile());

            //For the water shader
            _graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

            _parallaxManager.AddParallaxBackground(new Clouds(ContentLoader.Clouds, new Vector2(0.02f, 0.002f), _localPlayer.Position, (World.SurfaceDepth + 20) * Vestige.TILESIZE, (World.SurfaceDepth - 100) * Vestige.TILESIZE));
            _parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.MountainsBackground, new Vector2(0.01f, 0.001f), _localPlayer.Position, (World.SurfaceDepth + 20) * Vestige.TILESIZE, (World.SurfaceDepth - 80) * Vestige.TILESIZE));
            _parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFarthestBackground, new Vector2(0.1f, 0.06f), _localPlayer.Position + (new Vector2(Random.Next(-50, 50), 0) * Vestige.TILESIZE), (World.SurfaceDepth + 5) * Vestige.TILESIZE, (World.SurfaceDepth - 50) * Vestige.TILESIZE));
            _parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFartherBackground, new Vector2(0.2f, 0.08f), _localPlayer.Position + (new Vector2(Random.Next(-50, 50), 0) * Vestige.TILESIZE), (World.SurfaceDepth + 5) * Vestige.TILESIZE, (World.SurfaceDepth - 50) * Vestige.TILESIZE));
            _parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesBackground, new Vector2(0.3f, 0.1f), _localPlayer.Position + (new Vector2(Random.Next(-50, 50), 0) * Vestige.TILESIZE), (World.SurfaceDepth + 5) * Vestige.TILESIZE, (World.SurfaceDepth - 50) * Vestige.TILESIZE));
        }
        public void Update(double delta)
        {
            if (_gamePaused)
                return;
            GameClock.Update(delta);
            _sunMoon.UpdatePosition(new Vector2(-50, 200), (float)GameClock.GetCycleTime(), GameClock.TotalDayCycleTime / 2, Vestige.NativeResolution.X + 50, -100);
            World.Update(delta);
            _parallaxManager.Update(delta, GetCameraPosition() + Vestige.ScreenCenter.ToVector2());
            EntityManager.Update(delta);
            CalculateTranslation();
        }
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Point drawBoxMin = (GetCameraPosition() / Vestige.TILESIZE).ToPoint();
            Point drawBoxMax = (GetCameraPosition() / Vestige.TILESIZE).ToPoint() + Vestige.DrawDistance;
            Matrix viewMatrix = _translation * Matrix.CreateScale(Vestige.RenderDestination.Width / (float)Vestige.NativeResolution.X);
            _tileRenderer.SetDrawBox(drawBoxMin, drawBoxMax);
            _tileRenderer.SetTranslation(viewMatrix);
            LightEngine.SetDrawBox(drawBoxMin, drawBoxMax);
            LightEngine.CalculateLightMap();
            float normalizedGlobalLight = (GameClock.GlobalLight - 50) / 205.0f;

            //Draw background elements
            _graphicsDevice.SetRenderTarget(_bgTarget);
            _graphicsDevice.Clear(new Color((int)(50 * normalizedGlobalLight), (int)(109 * normalizedGlobalLight), (int)(255 * normalizedGlobalLight)));
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.LinearClamp, transformMatrix: Matrix.CreateScale(Vestige.RenderDestination.Width / (float)Vestige.NativeResolution.X));
            spriteBatch.Draw(DayTimeSkyGradient, new Rectangle(Point.Zero, Vestige.NativeResolution), new Color(GameClock.GlobalLight, GameClock.GlobalLight, GameClock.GlobalLight));
            _sunMoon.Draw(spriteBatch);
            _parallaxManager.Draw(spriteBatch, new Color(GameClock.GlobalLight, GameClock.GlobalLight, GameClock.GlobalLight));
            spriteBatch.End();

            _graphicsDevice.SetRenderTarget(_wallTarget);
            _graphicsDevice.Clear(Color.Transparent);
            if (SmoothLighting)
            {
                _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                _tileRenderer.DrawWalls_SmoothLighting(spriteBatch, _graphicsDevice);
                _tileRenderer.DrawTiles_SmoothLighting(spriteBatch, _graphicsDevice, true);
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix);
                _tileRenderer.DrawWalls_DefaultLighting(spriteBatch);
                _tileRenderer.DrawTiles_DefaultLighting(spriteBatch, true);
                spriteBatch.End();
            }

            _graphicsDevice.SetRenderTarget(_liquidRenderTarget);
            _graphicsDevice.Clear(Color.Transparent);
            ContentLoader.WaterShader.Parameters["BackgroundTexture"].SetValue(_wallTarget);
            ContentLoader.WaterShader.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            ContentLoader.WaterShader.Parameters["ModelMatrix"].SetValue(Matrix.Invert(viewMatrix));
            ContentLoader.WaterShader.CurrentTechnique = ContentLoader.WaterShader.Techniques["SpriteDrawing"];
            spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix, blendState: BlendState.AlphaBlend, effect: ContentLoader.WaterShader);
            _tileRenderer.DrawLiquidInTiles(spriteBatch);
            spriteBatch.End();

            _graphicsDevice.SetRenderTarget(_gameTarget);
            _graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_wallTarget, Vector2.Zero, Color.White);
            spriteBatch.Draw(_liquidRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
            if (SmoothLighting)
            {
                _tileRenderer.DrawTiles_SmoothLighting(spriteBatch, _graphicsDevice);
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix);
                _tileRenderer.DrawTiles_DefaultLighting(spriteBatch);
                spriteBatch.End();
            }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix);
            EntityManager.Draw(spriteBatch);
            spriteBatch.End();

            //Draw Liquids
            _graphicsDevice.SetRenderTarget(_liquidRenderTarget);
            _graphicsDevice.Clear(Color.Transparent);
            ContentLoader.WaterShader.Parameters["BackgroundTexture"].SetValue(_gameTarget);
            ContentLoader.WaterShader.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            ContentLoader.WaterShader.Parameters["ModelMatrix"].SetValue(Matrix.Invert(viewMatrix));
            if (SmoothLighting)
            {
                ContentLoader.WaterShader.CurrentTechnique = ContentLoader.WaterShader.Techniques["PrimitiveDrawing"];
                ContentLoader.WaterShader.Parameters["View"].SetValue(viewMatrix);
                _tileRenderer.DrawLiquids_SmoothLighting(_graphicsDevice);
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix, blendState: BlendState.AlphaBlend, effect: ContentLoader.WaterShader);
                _tileRenderer.DrawLiquids(spriteBatch);
                spriteBatch.End();
            }

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
            int dx = (int)Math.Round((Vestige.NativeResolution.X / 2) - _localPlayer.Position.X - _localPlayer.Origin.X);
            dx = MathHelper.Clamp(dx, (-World.WorldSize.X * Vestige.TILESIZE) + Vestige.NativeResolution.X, 0);
            int dy = (int)Math.Round((Vestige.NativeResolution.Y / 2) - _localPlayer.Position.Y - _localPlayer.Origin.Y);
            dy = MathHelper.Clamp(dy, (-World.WorldSize.Y * Vestige.TILESIZE) + Vestige.NativeResolution.Y, 0);
            _translation = Matrix.CreateTranslation(dx, dy, 0f);
        }
        private void UpdateRenderTargets()
        {
            _wallTarget = new RenderTarget2D(_graphicsDevice, Vestige.RenderDestination.Width, Vestige.RenderDestination.Height);
            _gameTarget = new RenderTarget2D(_graphicsDevice, Vestige.RenderDestination.Width, Vestige.RenderDestination.Height);
            _liquidRenderTarget = new RenderTarget2D(_graphicsDevice, Vestige.RenderDestination.Width, Vestige.RenderDestination.Height);
            _bgTarget = new RenderTarget2D(_graphicsDevice, Vestige.RenderDestination.Width, Vestige.RenderDestination.Height);

            //Update Water shader bounds
            ContentLoader.WaterShader.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, Vestige.RenderDestination.Width, Vestige.RenderDestination.Height, 0, 0, 1));
            ContentLoader.WaterShader.Parameters["screen_size"].SetValue(new Vector2(Vestige.RenderDestination.Width, Vestige.RenderDestination.Height));
            _tileRenderer.ResetProjectionBounds();
        }
        private void SaveAndQuit()
        {
            _worldFile.Save(World, _localPlayer);
            GameClock = null;
            LightEngine = null;
            EntityManager = null;
            World = null;
            _bgTarget.Dispose();
            _wallTarget.Dispose();
            _gameTarget.Dispose();
            _liquidRenderTarget.Dispose();
            _map.MapRenderTarget.Dispose();
            _gameHandle.OnRenderDestinationUpdated -= UpdateRenderTargets;
            _gameHandle.LoadMainMenu();
        }
        public void SetGameState(bool paused)
        {
            _gamePaused = paused;
        }
    }
}
