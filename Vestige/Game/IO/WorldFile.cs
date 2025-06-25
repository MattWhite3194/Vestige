using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Vestige.Game.Entities;
using Vestige.Game.Items;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.IO
{
    public class WorldFile
    {
        private string _name;
        private string _date;
        public string Name { get { return _name; } }
        public string Date { get { return _date; } }
        private string _path;
        private Item[] _playerItems;
        private Point _spawnTile;

        public WorldFile(string path)
        {
            _path = path;
        }
        public WorldFile()
        {

        }

        public void SetPath(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "New World";
            _name = name;
            string validWorldName = GetValidWorldName(name);
            string directory = Path.Combine(Vestige.SavePath, "Worlds", validWorldName);
            if (!Path.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            _path = Path.Combine(directory, validWorldName + ".wld");
        }
        private string GetValidWorldName(string worldName)
        {
            //Replace forbidden chars
            foreach (char forbiddenChar in Path.GetInvalidFileNameChars())
            {
                worldName = worldName.Replace(forbiddenChar, '-');
            }
            foreach (char forbiddenChar in Path.GetInvalidPathChars())
            {
                worldName = worldName.Replace(forbiddenChar, '-');
            }
            worldName = worldName.Replace('.', '_');

            //if the world name already exists, iterate it until a new filename is found
            int nameIteration = 1;
            string worldPath = Path.Combine(Vestige.SavePath, "Worlds", worldName);
            string iteratedWorldName = worldName;
            while (Path.Exists(worldPath))
            {
                iteratedWorldName = worldName + nameIteration;
                nameIteration++;
                worldPath = Path.Combine(Vestige.SavePath, "Worlds", iteratedWorldName);
            }
            worldName = iteratedWorldName;
            return worldName;
        }
        public Dictionary<string, string> GetMetaData()
        {
            if (!Path.Exists(_path))
            {
                throw new FileNotFoundException("Could not find the saved game data.");
            }
            using (FileStream worldData = File.OpenRead(_path))
            using (BinaryReader binaryReader = new BinaryReader(worldData))
            {
                LoadMetaData(binaryReader);
            }
            return new Dictionary<string, string>() { { "Name", _name }, { "Date", _date } };
        }
        public void Save(WorldGen world, Player player = null)
        {

            using (FileStream worldPath = File.Create(_path))
            using (BinaryWriter binaryWriter = new BinaryWriter(worldPath))
            {
                SaveMetaData(binaryWriter);
                SaveTiles(world, binaryWriter);
                SaveTileInventories(world, binaryWriter);
                SaveTileUpdates(binaryWriter);
                SavePlayer(player, binaryWriter);
            }
        }
        private void SaveMetaData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(_name);
            binaryWriter.Write(DateTime.Now.ToString("MMM dd, yyyy - h:mm tt"));
        }
        private void SaveTiles(WorldGen world, BinaryWriter binaryWriter)
        {
            _spawnTile = world.SpawnTile;
            binaryWriter.Write(world.SpawnTile.X);
            binaryWriter.Write(world.SpawnTile.Y);
            binaryWriter.Write(world.WorldSize.X);
            binaryWriter.Write(world.WorldSize.Y);
            binaryWriter.Write(world.SurfaceDepth);
            for (int i = 0; i < world.WorldSize.X; i++)
            {
                for (int j = 0; j < world.WorldSize.Y; j++)
                {
                    binaryWriter.Write(world.GetTileID(i, j));
                    binaryWriter.Write(world.GetTileState(i, j));
                    binaryWriter.Write(world.GetWallID(i, j));
                    binaryWriter.Write(world.GetWallState(i, j));
                    binaryWriter.Write(world.GetLiquid(i, j));
                }
            }
        }
        private void SaveTileInventories(WorldGen world, BinaryWriter binaryWriter)
        {
            Dictionary<Point, Item[]> tileInventories = world.GetAllTileInventories();
            binaryWriter.Write(tileInventories.Count);
            foreach (KeyValuePair<Point, Item[]> inventory in tileInventories)
            {
                binaryWriter.Write(inventory.Key.X);
                binaryWriter.Write(inventory.Key.Y);
                binaryWriter.Write(inventory.Value.Length);
                foreach (Item item in inventory.Value)
                {
                    binaryWriter.Write(item?.ID ?? -1);
                    binaryWriter.Write(item?.Quantity ?? -1);
                }
            }
        }
        private void SaveTileUpdates(BinaryWriter binaryWriter)
        {

        }
        private void SavePlayer(Player player, BinaryWriter binaryWriter)
        {

            if (player == null)
            {
                binaryWriter.Write(-1);
                binaryWriter.Write(-1);
                binaryWriter.Write(-1);
                return;
            }
            if (!player.Active)
            {
                binaryWriter.Write(-1);
                binaryWriter.Write(-1);
            }
            else
            {
                binaryWriter.Write((int)(player.Position.X / Vestige.TILESIZE));
                binaryWriter.Write((int)(player.Position.Y + player.Size.Y) / Vestige.TILESIZE);
                _spawnTile = (player.Position / Vestige.TILESIZE).ToPoint();
            }
            Item[] playerItems = player.Inventory.GetItems();
            binaryWriter.Write(playerItems.Length);
            foreach (Item item in playerItems)
            {
                binaryWriter.Write(item?.ID ?? -1);
                binaryWriter.Write(item?.Quantity ?? -1);
            }
        }
        public WorldGen Load()
        {
            WorldGen world;
            if (!Path.Exists(_path))
            {
                throw new FileNotFoundException("Could not find the saved game data.");
            }
            using (FileStream worldData = File.OpenRead(_path))
            using (BinaryReader binaryReader = new BinaryReader(worldData))
            {
                LoadMetaData(binaryReader);
                world = LoadTiles(binaryReader);
                LoadTileInventories(world, binaryReader);
                _spawnTile = LoadPlayerPosition(binaryReader);
                if (_spawnTile.X == -1 || _spawnTile.Y == -1)
                    _spawnTile = world.SpawnTile;
                _playerItems = LoadPlayerItems(binaryReader);
            }
            return world;
        }
        private void LoadMetaData(BinaryReader binaryReader)
        {
            _name = binaryReader.ReadString();
            _date = binaryReader.ReadString();
        }
        private WorldGen LoadTiles(BinaryReader binaryReader)
        {
            Point spawnTile = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32());
            Point worldSize = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32());
            int surfaceDepth = binaryReader.ReadInt32();
            WorldGen world = new WorldGen(worldSize.X, worldSize.Y);
            world.SpawnTile = spawnTile;
            world.WorldSize = worldSize;
            world.SurfaceDepth = surfaceDepth;
            for (int i = 0; i < worldSize.X; i++)
            {
                for (int j = 0; j < worldSize.Y; j++)
                {
                    world.SetTile(i, j, binaryReader.ReadUInt16());
                    world.SetTileState(i, j, binaryReader.ReadByte());
                    world.SetWall(i, j, binaryReader.ReadUInt16());
                    world.SetWallState(i, j, binaryReader.ReadByte());
                    world.SetLiquid(i, j, binaryReader.ReadByte());
                }
            }
            return world;
        }
        private void LoadTileInventories(WorldGen world, BinaryReader binaryReader)
        {
            int numTileInventories = binaryReader.ReadInt32();
            for (int i = 0; i < numTileInventories; i++)
            {
                Point tile = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32());
                int numItems = binaryReader.ReadInt32();
                Item[] items = new Item[numItems];
                for (int j = 0; j < numItems; j++)
                {
                    int itemID = binaryReader.ReadInt32();
                    int itemQuantity = binaryReader.ReadInt32();
                    items[j] = itemID == -1 ? null : Item.InstantiateItemByID(itemID, itemQuantity);
                }
                world.AddTileInventory(tile, items);
            }
        }
        private void LoadTileUpdates(BinaryReader binaryReader)
        {

        }
        private Point LoadPlayerPosition(BinaryReader binaryReader)
        {
            Point position = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32());
            return position;
        }
        private Item[] LoadPlayerItems(BinaryReader binaryReader)
        {
            int numPlayerItems = binaryReader.ReadInt32();
            if (numPlayerItems == -1)
            {
                return null;
            }
            Item[] playerItems = new Item[numPlayerItems];
            for (int i = 0; i < numPlayerItems; i++)
            {
                int itemID = binaryReader.ReadInt32();
                int itemQuantity = binaryReader.ReadInt32();
                playerItems[i] = itemID == -1 ? null : Item.InstantiateItemByID(itemID, itemQuantity);
            }
            return playerItems;
        }
        public Item[] GetPlayerItems()
        {
            return _playerItems;
        }
        public Point GetSpawnTile()
        {
            return _spawnTile;
        }
    }
}
