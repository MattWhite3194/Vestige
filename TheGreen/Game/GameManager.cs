using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.Drawables;
using TheGreen.Game.Entities;
using TheGreen.Game.Input;
using TheGreen.Game.Renderer;
using TheGreen.Game.Renderers;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game
{
    /// <summary>
    /// The instance of the game itself.
    /// </summary>
    public class GameManager
    {
        public GameManager() { }
        private GraphicsDevice _graphicsDevice;
        private static Matrix _translation;
        private TileRenderer _tileRenderer;
        private LightRenderer _lightRenderer;
        private RenderTarget2D _backgroundTarget;
        private RenderTarget2D _foregroundTarget;
        private RenderTarget2D _entityRenderTarget;
        public static readonly Random Random = new Random();


        public GameManager(Player player, GraphicsDevice graphicsDevice)
        {
            Globals.StartGameClock(10, 30);
            _graphicsDevice = graphicsDevice;
            EntityManager.Instance.SetPlayer(player);
            InputManager.RegisterHandler(player);
            _tileRenderer = new TileRenderer();
            _lightRenderer = new LightRenderer(_graphicsDevice);
            _backgroundTarget = new RenderTarget2D(_graphicsDevice, Globals.NativeResolution.X, Globals.NativeResolution.Y);
            _foregroundTarget = new RenderTarget2D(_graphicsDevice, Globals.NativeResolution.X, Globals.NativeResolution.Y, false, SurfaceFormat.Color, DepthFormat.None);
            _entityRenderTarget = new RenderTarget2D(_graphicsDevice, Globals.NativeResolution.X * 2, Globals.NativeResolution.Y * 2, false, SurfaceFormat.Color, DepthFormat.None);
            ParallaxManager.Instance.AddParallaxBackground(new ParallaxBackground(ContentLoader.MountainsBackground, new Vector2(0.01f, 0.001f), EntityManager.Instance.GetPlayer().Position, 300 * Globals.TILESIZE, 50 * Globals.TILESIZE));
            ParallaxManager.Instance.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFarthestBackground, new Vector2(0.1f, 0.06f), EntityManager.Instance.GetPlayer().Position, 250 * Globals.TILESIZE, 140 * Globals.TILESIZE));
            ParallaxManager.Instance.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFartherBackground, new Vector2(0.2f, 0.08f), EntityManager.Instance.GetPlayer().Position, 250 * Globals.TILESIZE, 140 * Globals.TILESIZE));
            ParallaxManager.Instance.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesBackground, new Vector2(0.3f, 0.1f), EntityManager.Instance.GetPlayer().Position, 250 * Globals.TILESIZE, 140 * Globals.TILESIZE));
        }
        public void Update(double delta)
        {
            Globals.UpdateGameTime(delta);
            ParallaxManager.Instance.Update(delta, GetCameraPosition());
            EntityManager.Instance.Update(delta);
            WorldGen.Instance.Update(delta);
            
            CalculateTranslation();
        }
        public void Draw(SpriteBatch spriteBatch)
        {

            Point drawBoxMin = new Point(((int)-_translation.Translation.X / Globals.TILESIZE), ((int)-_translation.Translation.Y / Globals.TILESIZE));
            Point drawBoxMax = new Point(((int)-_translation.Translation.X / Globals.TILESIZE) + Globals.DrawDistance.X, ((int)-_translation.Translation.Y / Globals.TILESIZE) + Globals.DrawDistance.Y);
            _tileRenderer.SetDrawBox(drawBoxMin, drawBoxMax);
            //Render entities scaled twice or thrice to a larger render target, then scale the target down



            //draw background elements to background render target
            _graphicsDevice.SetRenderTarget(_backgroundTarget);
            float normalizedGlobalLight = (Globals.GlobalLight - 50) / 205.0f;
            _graphicsDevice.Clear(new Color((int)(100 * normalizedGlobalLight), (int)(149 * normalizedGlobalLight), (int)(237 * normalizedGlobalLight)));
            spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
            ParallaxManager.Instance.Draw(spriteBatch, new Color(Globals.GlobalLight, Globals.GlobalLight, Globals.GlobalLight));
            spriteBatch.End();


            //draw entities to entity target
            //Entities drawn seperately at twice the scale to produce clean rotation
            _graphicsDevice.SetRenderTarget(_entityRenderTarget);
            _graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, transformMatrix: _translation * Matrix.CreateScale(2.0f));
            EntityManager.Instance.Draw(spriteBatch);
            spriteBatch.End();

            //draw foreground elements to foreground render target
            _graphicsDevice.SetRenderTarget(_foregroundTarget);
            _graphicsDevice.Clear(Color.Transparent);
            //draw walls and background tiles
            spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, transformMatrix: _translation);
            _tileRenderer.DrawWalls(spriteBatch);
            _tileRenderer.DrawBackgroundTiles(spriteBatch);
            spriteBatch.End();
            //draw entity render target to foreground
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_entityRenderTarget, new Rectangle(Point.Zero, Globals.NativeResolution), Color.White);
            spriteBatch.End();
            //draw tiles and lighting above entities and walls
            spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.LinearClamp, transformMatrix: _translation);
            _tileRenderer.DrawTiles(spriteBatch);
            _tileRenderer.DrawLiquids(spriteBatch);
            _lightRenderer.Draw(spriteBatch, drawBoxMin, drawBoxMax);
            spriteBatch.End();


            //draw render targets to screen
            _graphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: TheGreen.ScreenScaleMatrix);
            spriteBatch.Draw(_backgroundTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: TheGreen.ScreenScaleMatrix, effect: ContentLoader.LightShader);
            spriteBatch.Draw(_foregroundTarget, Vector2.Zero, Color.White);
            _tileRenderer.DrawDebug(spriteBatch);
            spriteBatch.End();
        }
        private Vector2 GetCameraPosition()
        {
            return new Vector2(Math.Abs(_translation.Translation.X), Math.Abs(_translation.Translation.Y));
        }
        public static Point GetTranslation()
        {
            return new Point((int)_translation.Translation.X, (int)_translation.Translation.Y);
        }
        private void CalculateTranslation()
        {
            Player player = EntityManager.Instance.GetPlayer();
            int dx = (int)(Globals.NativeResolution.X / 2 - player.Position.X);
            dx = MathHelper.Clamp(dx, -WorldGen.Instance.WorldSize.X * Globals.TILESIZE + Globals.NativeResolution.X, 0);
            int dy = (int)(Globals.NativeResolution.Y / 2 - player.Position.Y);
            dy = MathHelper.Clamp(dy, -WorldGen.Instance.WorldSize.Y * Globals.TILESIZE + Globals.NativeResolution.Y, 0);
            _translation = Matrix.CreateTranslation(dx, dy, 0f);
        }
    }
}
