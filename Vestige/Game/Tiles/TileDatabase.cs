using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Vestige.Game.Tiles.TileData;
using Vestige.Game.Tiles.WallData;

namespace Vestige.Game.Tiles
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
        private static readonly DefaultTileData[] _tileData = [
            new DefaultTileData(0, "", TileProperty.None, Color.CornflowerBlue),
            new DefaultTileData(1, "Dirt", TileProperty.Solid | TileProperty.PickaxeMineable, new Color(142, 96, 59), itemID: 0, wallID: 1, health: 40, tileMerges: [2]),
            new OverlayTileData(2, "Grass", TileProperty.Solid | TileProperty.PickaxeMineable, Color.Green, baseTileID: 1),
            new DefaultTileData(3, "Cobblestone", TileProperty.Solid | TileProperty.PickaxeMineable, Color.Gray, itemID: 1, wallID: 4, health : 100, tileMerges: [1, 4]),
            new DefaultTileData(4, "Stone", TileProperty.Solid | TileProperty.PickaxeMineable, Color.Gray, itemID: 1, wallID: 2, health: 100, tileMerges: [1]),
            new TreeData(5, "Spruce Log", TileProperty.AxeMineable, Color.Brown, itemID: 8, health: 80),
            new TreeTopData(6, "Spruce Treetop", TileProperty.AxeMineable, Color.Green, offset: new Vector2(-48, -152)),
            new TorchData(7, "Torch", TileProperty.PickaxeMineable, Color.Yellow, itemID: 3),
            new InventoryTileData(8, "Wood Chest", TileProperty.PickaxeMineable, Color.Brown, itemID: 5, cols: 5, rows: 3),
            new ClosedDoorData(9, "Wood Door", TileProperty.PickaxeMineable | TileProperty.Solid, Color.Brown, 10, itemID: 7),
            new OpenDoorData(10, "Wood Door", TileProperty.PickaxeMineable, Color.Brown, 9, itemID: 7),
            new DefaultTileData(11, "Wood", TileProperty.Solid | TileProperty.PickaxeMineable, Color.BurlyWood, itemID: 8, wallID: 3, health: 40, tileMerges: [1]),
            new DefaultTileData(12, "Coal Ore", TileProperty.Solid | TileProperty.PickaxeMineable, Color.Black, itemID: 13, health: 110, tileMerges: [1, 4]),
            new PlatformData(13, "Wood Platform", TileProperty.PickaxeMineable, Color.BurlyWood, itemID: 12, health: 0),
            new DefaultTileData(14, "Stone Bricks", TileProperty.Solid | TileProperty.PickaxeMineable, Color.Gray, itemID: 14, wallID: 5, health: 100, tileMerges: [1]),
        ];

        //Only add walls here that require special functions, all walls should more or less work the exact same way.
        private static readonly DefaultWallData[] _wallData = [
            new DefaultWallData(0, ""),
            new DefaultWallData(1, "Dirt Wall", 0),
            new DefaultWallData(2, "Stone Wall", 1),
            new DefaultWallData(3, "Wood Wall", 8),
            new DefaultWallData(4, "Cobblestone Wall", 1),
            new DefaultWallData(5, "Stone Brick Wall", 14)
        ];

        private static Rectangle CreateAtlasRect(int x, int y)
        {
            return new Rectangle(x * Vestige.TILESIZE, y * Vestige.TILESIZE, Vestige.TILESIZE, Vestige.TILESIZE);
        }
        private static Rectangle CreateWallAtlasRect(int x, int y)
        {
            return new Rectangle(x * (Vestige.TILESIZE + 4), y * (Vestige.TILESIZE + 4), Vestige.TILESIZE + 4, Vestige.TILESIZE + 4);
        }
        /// <summary>
        /// Stores the texture atlas coords of a standard tile with the give tile state.
        /// </summary>
        private static Dictionary<byte, Rectangle> _tileTextureAtlasRects = new Dictionary<byte, Rectangle>()
        {
            {0, CreateAtlasRect(0,0)}, {2, CreateAtlasRect(1,0)}, {8, CreateAtlasRect(2,0)}, {10, CreateAtlasRect(3,0)}, {14, CreateAtlasRect(4,0)}, {32, CreateAtlasRect(5,0)},
            {34, CreateAtlasRect(0,1)}, {40, CreateAtlasRect(1,1)}, {42, CreateAtlasRect(2,1)}, {46, CreateAtlasRect(3,1)}, {56, CreateAtlasRect(4,1)}, {58, CreateAtlasRect(5,1)},
            {62, CreateAtlasRect(0,2)}, {128, CreateAtlasRect(1,2)}, {130, CreateAtlasRect(2,2)}, {131, CreateAtlasRect(3,2)}, {136, CreateAtlasRect(4,2)}, {138, CreateAtlasRect(5,2)},
            {139, CreateAtlasRect(0,3)}, {142, CreateAtlasRect(1,3)}, {143, CreateAtlasRect(2,3)}, {160, CreateAtlasRect(3,3)}, {162, CreateAtlasRect(4,3)}, {163, CreateAtlasRect(5,3)},
            {168, CreateAtlasRect(0,4)}, {170, CreateAtlasRect(1,4)}, {171, CreateAtlasRect(2,4)}, {174, CreateAtlasRect(3,4)}, {175, CreateAtlasRect(4,4)}, {184, CreateAtlasRect(5,4)},
            {186, CreateAtlasRect(0,5)}, {187, CreateAtlasRect(1,5)}, {190, CreateAtlasRect(2,5)}, {191, CreateAtlasRect(3,5)}, {224, CreateAtlasRect(4,5)}, {226, CreateAtlasRect(5,5)},
            {227, CreateAtlasRect(0,6)}, {232, CreateAtlasRect(1,6)}, {234, CreateAtlasRect(2,6)}, {235, CreateAtlasRect(3,6)}, {238, CreateAtlasRect(4,6)}, {239, CreateAtlasRect(5,6)},
            {248, CreateAtlasRect(0,7)}, {250, CreateAtlasRect(1,7)}, {251, CreateAtlasRect(2,7)}, {254, CreateAtlasRect(3,7)}, {255, CreateAtlasRect(4,7)}
        };

        private static Dictionary<byte, Rectangle> _wallTextureAtlasRects = new Dictionary<byte, Rectangle>()
        {
            {0, CreateWallAtlasRect(0,0)}, {2, CreateWallAtlasRect(1,0)}, {8, CreateWallAtlasRect(2,0)}, {10, CreateWallAtlasRect(3,0)}, {32, CreateWallAtlasRect(4,0)}, {34, CreateWallAtlasRect(5,0)},
            {40, CreateWallAtlasRect(0,1)}, {42, CreateWallAtlasRect(1,1)}, {128, CreateWallAtlasRect(2,1)}, {130, CreateWallAtlasRect(3,1)}, {136, CreateWallAtlasRect(4,1)}, {138, CreateWallAtlasRect(5,1)},
            {160, CreateWallAtlasRect(0,2)}, {162, CreateWallAtlasRect(1,2)}, {168, CreateWallAtlasRect(2,2)}, {170, CreateWallAtlasRect(3,2)}
        };
        /// <summary>
        /// Check if the tile type has a property or properties.
        /// </summary>
        /// <param name="tileID"></param>
        /// <param name="properties"></param>
        /// <returns>True if the tile type contains any of the properties specified.</returns>
        public static bool TileHasProperties(ushort tileID, TileProperty properties)
        {
            if (properties == TileProperty.None) return _tileData[tileID].Properties == TileProperty.None;
            return (_tileData[tileID].Properties & properties) != 0;
        }

        public static DefaultTileData GetTileData(ushort tileID)
        {
            return _tileData[tileID];
        }

        public static DefaultWallData GetWallData(ushort wallID)
        {
            return _wallData[wallID];
        }
        public static Rectangle GetTileTextureAtlas(byte state)
        {
            return _tileTextureAtlasRects[state];
        }
        public static Rectangle GetWallTextureAtlas(byte state)
        {
            return _wallTextureAtlasRects[state];
        }
    }
}
