using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TheGreen.Game.Drawables;
using TheGreen.Game.Entities;
using TheGreen.Game.Lighting;
using TheGreen.Game.Renderer;
using TheGreen.Game.Time;
using TheGreen.Game.WorldGeneration;

namespace TheGreen.Game
{
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
        public static EntityManager EntityManager = null;
        public static ParallaxManager ParallaxManager = null;
        public static GameClock GameClock;

        public Main(Player player, GraphicsDevice graphicsDevice)
        {
            _tileRenderer = new TileRenderer();
            LightEngine = new LightEngine(_graphicsDevice);
            EntityManager = new EntityManager();
            ParallaxManager = new ParallaxManager();
            GameClock = new GameClock();
            GameClock.StartGameClock(1000, 2000);
            WorldGen.World.InitializeGameUpdates();
            player.InitializeGameUpdates();
            _graphicsDevice = graphicsDevice;
            
            EntityManager.SetPlayer(player);
            //EntityManager.CreateEnemy(0, player.Position + new Vector2(500, -100));
            //EntityManager.CreateEnemy(0, player.Position + new Vector2(-500, -100));
            
            ParallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.MountainsBackground, new Vector2(0.01f, 0.001f), EntityManager.GetPlayer().Position, (WorldGen.World.SurfaceDepth + 20) * TheGreen.TILESIZE, (WorldGen.World.SurfaceDepth - 80) * TheGreen.TILESIZE));
            ParallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFarthestBackground, new Vector2(0.1f, 0.06f), EntityManager.GetPlayer().Position, (WorldGen.World.SurfaceDepth + 5) * TheGreen.TILESIZE, (WorldGen.World.SurfaceDepth - 30) * TheGreen.TILESIZE));
            ParallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFartherBackground, new Vector2(0.2f, 0.08f), EntityManager.GetPlayer().Position, (WorldGen.World.SurfaceDepth + 5) * TheGreen.TILESIZE, (WorldGen.World.SurfaceDepth - 30) * TheGreen.TILESIZE));
            ParallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesBackground, new Vector2(0.3f, 0.1f), EntityManager.GetPlayer().Position, (WorldGen.World.SurfaceDepth + 5) * TheGreen.TILESIZE, (WorldGen.World.SurfaceDepth - 30) * TheGreen.TILESIZE));
        }
        public void Update(double delta)
        {
            GameClock.Update(delta);
            WorldGen.World.Update(delta);
            ParallaxManager.Update(delta, GetCameraPosition() + TheGreen.ScreenCenter.ToVector2());
            EntityManager.Update(delta);
            CalculateTranslation();
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            Point drawBoxMin = new Point(((int)-_translation.Translation.X / TheGreen.TILESIZE), ((int)-_translation.Translation.Y / TheGreen.TILESIZE));
            Point drawBoxMax = new Point(((int)-_translation.Translation.X / TheGreen.TILESIZE) + TheGreen.DrawDistance.X, ((int)-_translation.Translation.Y / TheGreen.TILESIZE) + TheGreen.DrawDistance.Y);
            _tileRenderer.SetDrawBox(drawBoxMin, drawBoxMax);
            LightEngine.SetDrawBox(drawBoxMin, drawBoxMax);
            LightEngine.CalculateLightMap();

            float normalizedGlobalLight = (GameClock.GlobalLight - 50) / 205.0f;
            _graphicsDevice.Clear(new Color((int)(100 * normalizedGlobalLight), (int)(149 * normalizedGlobalLight), (int)(237 * normalizedGlobalLight)));
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(2.0f));
            ParallaxManager.Draw(spriteBatch, new Color(GameClock.GlobalLight, GameClock.GlobalLight, GameClock.GlobalLight));
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, transformMatrix: _translation * Matrix.CreateScale(2.0f), blendState: BlendState.NonPremultiplied);
            
            _tileRenderer.DrawWalls(spriteBatch);
            _tileRenderer.DrawBackgroundTiles(spriteBatch);
            _tileRenderer.DrawTiles(spriteBatch);
            //_tileRenderer.DrawDebug(spriteBatch);
            EntityManager.Draw(spriteBatch);
            _tileRenderer.DrawLiquids(spriteBatch);
            spriteBatch.End();
        }
        public static Vector2 GetCameraPosition()
        {
            return new Vector2(Math.Abs(_translation.Translation.X), Math.Abs(_translation.Translation.Y));
        }
        private void CalculateTranslation()
        {
            Player player = EntityManager.GetPlayer();
            int dx = (int)(TheGreen.NativeResolution.X / 2 - player.Position.X);
            dx = MathHelper.Clamp(dx, -WorldGen.World.WorldSize.X * TheGreen.TILESIZE + TheGreen.NativeResolution.X, 0);
            int dy = (int)(TheGreen.NativeResolution.Y / 2 - player.Position.Y);
            dy = MathHelper.Clamp(dy, -WorldGen.World.WorldSize.Y * TheGreen.TILESIZE + TheGreen.NativeResolution.Y, 0);
            _translation = Matrix.CreateTranslation(dx, dy, 0f);
        }
    }
}
