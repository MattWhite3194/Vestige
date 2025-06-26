using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.Tiles;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Renderers
{
    public class TileRenderer
    {
        private Point _drawBoxMin;
        private Point _drawBoxMax;
        private BasicEffect _tileDrawEffect;
        private Matrix _translation;
        private int _tileWaterOverlap = 4;

        public TileRenderer(GraphicsDevice graphicsDevice)
        {
            _tileDrawEffect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(0, 1920, 1280, 0, 0, 1),
                View = Matrix.Identity,
                World = Matrix.Identity
            };
        }
        public void DrawWalls_DefaultLighting(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort wallID = Main.World.GetWallID(i, j);
                    if (wallID == 0)
                        continue;
                    TileDatabase.GetWallData(wallID).Draw(spriteBatch, i, j, Main.World.GetWallState(i, j), Main.LightEngine.GetLight(i, j));
                }
            }
            foreach (DamagedTile damagedWall in Main.World.GetDamagedWalls().Values)
            {
                Color color = Color.Black;
                color.A = 125;
                int cracksTextureAtlasY = (5 - (int)(damagedWall.Health / (float)damagedWall.TotalTileHealth * 5)) * Vestige.TILESIZE;
                spriteBatch.Draw(ContentLoader.Cracks, new Vector2(damagedWall.X, damagedWall.Y) * Vestige.TILESIZE, new Rectangle(0, cracksTextureAtlasY, Vestige.TILESIZE, Vestige.TILESIZE), color);
            }
        }
        public void DrawWalls_SmoothLighting(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort wallID = Main.World.GetWallID(i, j);
                    if (wallID == 0)
                        continue;
                    Color tl = Main.LightEngine.GetCornerLight(i, j);
                    Color tr = Main.LightEngine.GetCornerLight(i + 1, j);
                    Color bl = Main.LightEngine.GetCornerLight(i, j + 1);
                    Color br = Main.LightEngine.GetCornerLight(i + 1, j + 1);
                    TileDatabase.GetWallData(wallID).DrawPrimitive(graphicsDevice, _tileDrawEffect, i, j, Main.World.GetWallState(i, j), tl, tr, bl, br);
                }
            }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: _translation);
            foreach (DamagedTile damagedWall in Main.World.GetDamagedWalls().Values)
            {
                Color color = Color.Black;
                color.A = 125;
                int cracksTextureAtlasY = (5 - (int)(damagedWall.Health / (float)damagedWall.TotalTileHealth * 5)) * Vestige.TILESIZE;
                spriteBatch.Draw(ContentLoader.Cracks, new Vector2(damagedWall.X, damagedWall.Y) * Vestige.TILESIZE, new Rectangle(0, cracksTextureAtlasY, Vestige.TILESIZE, Vestige.TILESIZE), color);
            }
            spriteBatch.End();
        }
        public void DrawLiquidInTiles(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort tileID = Main.World.GetTileID(i, j);
                    if (!TileDatabase.TileHasProperties(tileID, TileProperty.Solid))
                        continue;
                    //Draw water behind tile
                    bool top = Main.World.GetLiquid(i, j - 1) != 0;
                    bool bottom = Main.World.GetLiquid(i, j + 1) == WorldGen.MaxLiquid;
                    int left = (int)(Main.World.GetLiquid(i - 1, j) / (float)WorldGen.MaxLiquid * (Vestige.TILESIZE - 2));
                    int right = (int)(Main.World.GetLiquid(i + 1, j) / (float)WorldGen.MaxLiquid * (Vestige.TILESIZE - 2));

                    if (top)
                    {
                        spriteBatch.Draw(ContentLoader.LiquidTexture, new Vector2(i * Vestige.TILESIZE, j * Vestige.TILESIZE), new Rectangle(Vestige.TILESIZE, 0, Vestige.TILESIZE, (bottom || left != 0 || right != 0) ? Vestige.TILESIZE : _tileWaterOverlap), Main.LightEngine.GetLight(i, j - 1));
                    }
                    else if (left != 0 || right != 0)
                    {
                        int height = Math.Max(left + 2, right + 2);
                        Color light = Main.LightEngine.GetLight(i - 1, j);
                        if (height == right + 2)
                        {
                            light = Main.LightEngine.GetLight(i + 1, j);
                        }
                        int width = (left != 0 && right != 0) || bottom ? Vestige.TILESIZE : Vestige.TILESIZE / 2;
                        spriteBatch.Draw(ContentLoader.LiquidTexture, new Vector2((i * Vestige.TILESIZE) + (left != 0 || bottom ? 0 : Vestige.TILESIZE / 2), (j * Vestige.TILESIZE) + Vestige.TILESIZE - height), new Rectangle(Vestige.TILESIZE, 0, width, height), light);
                    }
                    else if (bottom)
                    {
                        spriteBatch.Draw(ContentLoader.LiquidTexture, new Vector2(i * Vestige.TILESIZE, (j * Vestige.TILESIZE) + Vestige.TILESIZE - _tileWaterOverlap), new Rectangle(Vestige.TILESIZE, 0, Vestige.TILESIZE, _tileWaterOverlap), Main.LightEngine.GetLight(i, j + 1));
                    }
                }
            }
        }
        public void DrawTiles_SmoothLighting(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, bool background = false)
        {
            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort tileID = Main.World.GetTileID(i, j);
                    if (background && (TileDatabase.TileHasProperties(tileID, TileProperty.Solid) || tileID == 0))
                        continue;
                    else if (!background && !TileDatabase.TileHasProperties(tileID, TileProperty.Solid))
                        continue;
                    Color tl = Main.LightEngine.GetCornerLight(i, j);
                    Color tr = Main.LightEngine.GetCornerLight(i + 1, j);
                    Color bl = Main.LightEngine.GetCornerLight(i, j + 1);
                    Color br = Main.LightEngine.GetCornerLight(i + 1, j + 1);
                    TileDatabase.GetTileData(tileID).DrawPrimitive(graphicsDevice, _tileDrawEffect, i, j, Main.World.GetTileState(i, j), tl, tr, bl, br);
                }
            }
            if (background)
                return;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: _translation);
            foreach (DamagedTile damagedTile in Main.World.GetDamagedTiles().Values)
            {
                Color color = Color.Black;
                color.A = 125;
                int cracksTextureAtlasY = (5 - (int)(damagedTile.Health / (float)damagedTile.TotalTileHealth * 5)) * Vestige.TILESIZE;
                spriteBatch.Draw(ContentLoader.Cracks, new Vector2(damagedTile.X, damagedTile.Y) * Vestige.TILESIZE, new Rectangle(0, cracksTextureAtlasY, Vestige.TILESIZE, Vestige.TILESIZE), color);
            }
            spriteBatch.End();
        }
        public void DrawTiles_DefaultLighting(SpriteBatch spriteBatch, bool background = false)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    ushort tileID = Main.World.GetTileID(i, j);
                    if (background && (TileDatabase.TileHasProperties(tileID, TileProperty.Solid) || tileID == 0))
                        continue;
                    else if (!background && !TileDatabase.TileHasProperties(tileID, TileProperty.Solid))
                        continue;
                    //Draw Tile
                    TileDatabase.GetTileData(tileID).Draw(spriteBatch, i, j, Main.World.GetTileState(i, j), Main.LightEngine.GetLight(i, j));
                }
            }
            if (background)
                return;
            foreach (DamagedTile damagedTile in Main.World.GetDamagedTiles().Values)
            {
                Color color = Color.Black;
                color.A = 125;
                int cracksTextureAtlasY = (5 - (int)(damagedTile.Health / (float)damagedTile.TotalTileHealth * 5)) * Vestige.TILESIZE;
                spriteBatch.Draw(ContentLoader.Cracks, new Vector2(damagedTile.X, damagedTile.Y) * Vestige.TILESIZE, new Rectangle(0, cracksTextureAtlasY, Vestige.TILESIZE, Vestige.TILESIZE), color);
            }
        }
        public void DrawLiquids(SpriteBatch spriteBatch)
        {
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    if (Main.World.GetLiquid(i, j) != 0)
                    {
                        if (Main.World.GetLiquid(i, j - 1) != 0 || (Main.World.GetLiquid(i, j) == WorldGen.MaxLiquid && TileDatabase.TileHasProperties(Main.World.GetTileID(i, j - 1), TileProperty.Solid)))
                        {
                            spriteBatch.Draw(ContentLoader.LiquidTexture, new Vector2(i, j) * Vestige.TILESIZE, new Rectangle(Vestige.TILESIZE, 0, Vestige.TILESIZE, Vestige.TILESIZE), Main.LightEngine.GetLight(i, j));
                        }
                        else
                        {
                            int textureOffset = 2 + (int)(Main.World.GetLiquid(i, j) / (float)WorldGen.MaxLiquid * (Vestige.TILESIZE - 2));
                            spriteBatch.Draw(ContentLoader.LiquidTexture, new Vector2(i * Vestige.TILESIZE, (j * Vestige.TILESIZE) + Vestige.TILESIZE - textureOffset), new Rectangle(0, 0, Vestige.TILESIZE, textureOffset), Main.LightEngine.GetLight(i, j));
                        }
                    }
                }
            }
        }
        public void DrawLiquids_SmoothLighting(GraphicsDevice graphicsDevice)
        {
            ContentLoader.WaterShader.Parameters["SpriteTexture"].SetValue(ContentLoader.LiquidTexture);
            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            for (int i = _drawBoxMin.X; i < _drawBoxMax.X; i++)
            {
                for (int j = _drawBoxMin.Y; j < _drawBoxMax.Y; j++)
                {
                    if (Main.World.GetLiquid(i, j) != 0)
                    {
                        Rectangle sourceRect = default;
                        Vector2 position = default;
                        if (Main.World.GetLiquid(i, j - 1) != 0 || (Main.World.GetLiquid(i, j) == WorldGen.MaxLiquid && TileDatabase.TileHasProperties(Main.World.GetTileID(i, j - 1), TileProperty.Solid)))
                        {
                            sourceRect = new Rectangle(Vestige.TILESIZE, 0, Vestige.TILESIZE, Vestige.TILESIZE);
                            position = new Vector2(i * Vestige.TILESIZE, j * Vestige.TILESIZE);
                        }
                        else
                        {
                            int textureOffset = 2 + (int)(Main.World.GetLiquid(i, j) / (float)WorldGen.MaxLiquid * (Vestige.TILESIZE - 2));
                            sourceRect = new Rectangle(0, 0, Vestige.TILESIZE, textureOffset);
                            position = new Vector2(i * Vestige.TILESIZE, (j * Vestige.TILESIZE) + Vestige.TILESIZE - textureOffset);
                        }
                        Color tl = Main.LightEngine.GetCornerLight(i, j);
                        Color tr = Main.LightEngine.GetCornerLight(i + 1, j);
                        Color bl = Main.LightEngine.GetCornerLight(i, j + 1);
                        Color br = Main.LightEngine.GetCornerLight(i + 1, j + 1);
                        Texture2D liquidTexture = ContentLoader.LiquidTexture;
                        Vector2 uvTopLeft = new Vector2(
                            sourceRect.X / (float)liquidTexture.Width,
                            sourceRect.Y / (float)liquidTexture.Height
                        );

                        Vector2 uvBottomRight = new Vector2(
                            (sourceRect.X + sourceRect.Width) / (float)liquidTexture.Width,
                            (sourceRect.Y + sourceRect.Height) / (float)liquidTexture.Height
                        );
                        VertexPositionColorTexture[] vertices =
                        {
                            new VertexPositionColorTexture(new Vector3(position.X, position.Y, 0f), tl, uvTopLeft),
                            new VertexPositionColorTexture(new Vector3(position.X + Vestige.TILESIZE, position.Y + sourceRect.Height, 0f), br, uvBottomRight),
                            new VertexPositionColorTexture(new Vector3(position.X + Vestige.TILESIZE, position.Y, 0f), tr, new Vector2(uvBottomRight.X, uvTopLeft.Y)),

                            new VertexPositionColorTexture(new Vector3(position.X, position.Y, 0f), tl, uvTopLeft),
                            new VertexPositionColorTexture(new Vector3(position.X, position.Y + sourceRect.Height, 0f), bl, new Vector2(uvTopLeft.X, uvBottomRight.Y)),
                            new VertexPositionColorTexture(new Vector3(position.X + Vestige.TILESIZE, position.Y + sourceRect.Height, 0f), br, uvBottomRight),
                        };
                        foreach (EffectPass pass in ContentLoader.WaterShader.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
                        }
                    }
                }
            }
        }
        public void SetTranslation(Matrix translation)
        {
            _translation = translation;
            _tileDrawEffect.View = translation;
        }
        public void SetDrawBox(Point drawBoxMin, Point drawBoxMax)
        {
            _drawBoxMin = drawBoxMin;
            _drawBoxMax = drawBoxMax;
        }
    }
}