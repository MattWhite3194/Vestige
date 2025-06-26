using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.WallData
{
    public class DefaultWallData
    {
        public readonly ushort WallID;
        public readonly int Health;
        public readonly string Name;
        public readonly int ItemID;
        public readonly Color MapColor;
        public DefaultWallData(ushort wallID, string name, Color mapColor, int itemID = -1, int health = 100)
        {
            WallID = wallID;
            Name = name;
            MapColor = mapColor;
            Health = health;
            ItemID = itemID;
        }
        public virtual int VerifyWall(WorldGen world, int x, int y)
        {
            ushort top = world.GetWallID(x, y - 1);
            ushort right = world.GetWallID(x + 1, y);
            ushort bottom = world.GetWallID(x, y + 1);
            ushort left = world.GetWallID(x - 1, y);
            ushort tile = world.GetTileID(x, y);

            return Math.Sign(top + right + bottom + left + tile);
        }
        public virtual byte GetUpdatedWallState(WorldGen world, int x, int y)
        {
            if (world.GetWallID(x, y) == 0)
            {
                return 0;
            }
            //Important: if a corner doesn't have both sides touching it, it won't be counted
            ushort top = world.GetWallID(x, y - 1);
            ushort right = world.GetWallID(x + 1, y);
            ushort bottom = world.GetWallID(x, y + 1);
            ushort left = world.GetWallID(x - 1, y);

            return (byte)((Math.Sign(top) * 2) + (Math.Sign(right) * 8) + (Math.Sign(bottom) * 32) + (Math.Sign(left) * 128));
        }

        public virtual void Draw(SpriteBatch spriteBatch, int x, int y, byte state, Color light)
        {
            spriteBatch.Draw(ContentLoader.WallTextures[WallID], new Vector2((x * Vestige.TILESIZE) - 2, (y * Vestige.TILESIZE) - 2), TileDatabase.GetWallTextureAtlas(state), light);
        }

        public virtual void DrawPrimitive(GraphicsDevice graphicsDevice, BasicEffect tileDrawEffect, int x, int y, byte state, Color tl, Color tr, Color bl, Color br)
        {
            Vector2 position = new Vector2(x * Vestige.TILESIZE, y * Vestige.TILESIZE);
            Rectangle sourceRect = TileDatabase.GetWallTextureAtlas(state);
            Texture2D wallTexture = ContentLoader.WallTextures[WallID];
            Vector2 uvTopLeft = new Vector2(
                sourceRect.X / (float)wallTexture.Width,
                sourceRect.Y / (float)wallTexture.Height
            );

            Vector2 uvBottomRight = new Vector2(
                (sourceRect.X + sourceRect.Width) / (float)wallTexture.Width,
                (sourceRect.Y + sourceRect.Height) / (float)wallTexture.Height
            );
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[6]
            {
                        new VertexPositionColorTexture(new Vector3(position.X - 2, position.Y - 2, 0f), tl, uvTopLeft),
                        new VertexPositionColorTexture(new Vector3(position.X + Vestige.TILESIZE + 2, position.Y + Vestige.TILESIZE + 2, 0f), br, uvBottomRight),
                        new VertexPositionColorTexture(new Vector3(position.X + Vestige.TILESIZE + 2, position.Y - 2, 0f),    tr,    new Vector2(uvBottomRight.X, uvTopLeft.Y)),

                        new VertexPositionColorTexture(new Vector3(position.X - 2, position.Y - 2, 0f), tl, uvTopLeft),
                        new VertexPositionColorTexture(new Vector3(position.X - 2, position.Y + Vestige.TILESIZE + 2, 0f), bl, new Vector2(uvTopLeft.X, uvBottomRight.Y)),
                        new VertexPositionColorTexture(new Vector3(position.X + Vestige.TILESIZE + 2, position.Y + Vestige.TILESIZE + 2, 0f), br, uvBottomRight),
            };
            if (tileDrawEffect.Texture != wallTexture)
                tileDrawEffect.Texture = wallTexture;
            foreach (EffectPass pass in tileDrawEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
            }
        }
    }
}
