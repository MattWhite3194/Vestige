using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TheGreen.Game.Tiles
{
    /// <summary>
    /// Holds tile information that would be considered redundant and memory inefficient in the Tile struct. 
    /// Reference this to retrieve properties of a tile type, and methods for drawing and updating a specific tile type
    /// </summary>
    public static class TileDatabase { 
        
        
        /// <summary>
        /// List of static tile type information
        /// Use & on TileProperties to check if it has a property.
        /// e.x. Dirt: _tileProperties[1].TileProperties & Solid; returns true
        /// </summary>
        private static readonly TileData[] _tileProperties = [
            new TileData(TileProperty.None, Color.CornflowerBlue),                     //Air
            new TileData(TileProperty.Solid, Color.Brown, itemID: 0, health: 40),           //Dirt
            new TileData(TileProperty.Solid | TileProperty.Overlay, Color.Green, itemID: 0, health: 60, baseTileID: 1),           //Grass
            new TileData(TileProperty.Solid, Color.Gray, itemID: 1, health : 100), //CobbleStone
            new TileData(TileProperty.Solid, Color.Gray, itemID: 1, health: 100),  //Stone
            new TreeData(TileProperty.StaticTileState, Color.Brown, health: 80),    //Tree
            new TreeTopData(TileProperty.StaticTileState, Color.Green, offset: new Vector2(-48, -152)), //TreeTop
            new TorchData(TileProperty.StaticTileState | TileProperty.LightEmitting, Color.Orange, itemID: 3)
            ];
        private static Rectangle CreateAtlasRect(int x, int y)
        {
            return new Rectangle(x * Globals.TILESIZE, y * Globals.TILESIZE, Globals.TILESIZE, Globals.TILESIZE);
        }
        /// <summary>
        /// Stores the texture atlas coords of a standard tile with the give tile state.
        /// </summary>
        private static Dictionary<byte, Rectangle> _textureAtlasRects = new Dictionary<byte, Rectangle>()
        {
            {0, CreateAtlasRect(0, 0)}, {2, CreateAtlasRect(1,0)}, {8, CreateAtlasRect(2,0)}, {10, CreateAtlasRect(3,0)}, {14, CreateAtlasRect(4,0)}, {32, CreateAtlasRect(5,0)},
            {34, CreateAtlasRect(0,1)}, {40, CreateAtlasRect(1,1)}, {42, CreateAtlasRect(2,1)}, {46, CreateAtlasRect(3,1)}, {56, CreateAtlasRect(4,1)}, {58, CreateAtlasRect(5,1)},
            {62, CreateAtlasRect(0,2)}, {128, CreateAtlasRect(1,2)}, {130, CreateAtlasRect(2,2)}, {131, CreateAtlasRect(3,2)}, {136, CreateAtlasRect(4,2)}, {138, CreateAtlasRect(5,2)},
            {139, CreateAtlasRect(0,3)}, {142, CreateAtlasRect(1,3)}, {143, CreateAtlasRect(2,3)}, {160, CreateAtlasRect(3,3)}, {162, CreateAtlasRect(4,3)}, {163, CreateAtlasRect(5,3)},
            {168, CreateAtlasRect(0,4)}, {170, CreateAtlasRect(1,4)}, {171, CreateAtlasRect(2,4)}, {174, CreateAtlasRect(3,4)}, {175, CreateAtlasRect(4,4)}, {184, CreateAtlasRect(5,4)},
            {186, CreateAtlasRect(0,5)}, {187, CreateAtlasRect(1,5)}, {190, CreateAtlasRect(2,5)}, {191, CreateAtlasRect(3,5)}, {224, CreateAtlasRect(4,5)}, {226, CreateAtlasRect(5,5)},
            {227, CreateAtlasRect(0,6)}, {232, CreateAtlasRect(1,6)}, {234, CreateAtlasRect(2,6)}, {235, CreateAtlasRect(3,6)}, {238, CreateAtlasRect(4,6)}, {239, CreateAtlasRect(5,6)},
            {248, CreateAtlasRect(0,7)}, {250, CreateAtlasRect(1,7)}, {251, CreateAtlasRect(2,7)}, {254, CreateAtlasRect(3,7)}, {255, CreateAtlasRect(4,7)}
        };

        public static bool TileHasProperty(ushort id, TileProperty property)
        {
            if (property == TileProperty.None) return _tileProperties[id].Properties == TileProperty.None;
            return (_tileProperties[id].Properties & property) == property;
        }
        public static Color GetTileMapColor(ushort id)
        {
            return _tileProperties[id].MapColor;
        }
        public static int GetTileHealth(ushort id)
        {
            return _tileProperties[id].Health;
        }
        public static int GetTileItemID(ushort id)
        {
            return _tileProperties[id].ItemID;
        }
        public static Type GetTileType(ushort id)
        {
            return _tileProperties[id].GetType();
        }
        public static Rectangle GetTileTextureAtlas(byte state)
        {
            return _textureAtlasRects[state];
        }
        /// <summary>
        /// Only overlay tiles like grass can have a set base tile.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An overlay tiles base tile. Ex: base tile of grass is dirt</returns>
        public static ushort GetTileBaseID(ushort id)
        {
            return _tileProperties[id].BaseTileID;
        }
        public static void DrawTile(SpriteBatch spriteBatch, int tileID, byte tileState, int x, int y)
        {
            _tileProperties[tileID].Draw(spriteBatch, tileID, tileState, x, y);
        }

        public static void DrawWall(SpriteBatch spriteBatch, int wallID, byte wallState, int x, int y)
        {
            //Temporary
            spriteBatch.Draw(ContentLoader.TileTextures[wallID], new Vector2(x * Globals.TILESIZE, y * Globals.TILESIZE), _textureAtlasRects[wallState], Color.White);
        }

        public static int VerifyTile(ushort tileID, int x, int y)
        {
            return _tileProperties[tileID].VerifyTile(tileID, x, y);
        }

        public static byte GetUpdatedTileState(ushort tileID, int x, int y)
        {
            return _tileProperties[tileID].GetUpdatedTileState(tileID, x, y);
        }
    }
}
