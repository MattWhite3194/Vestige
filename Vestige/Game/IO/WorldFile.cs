using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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

        public WorldFile(string path)
        {
            _path = path;
        }
        public WorldFile()
        {

        }

        public void SetPath(string name)
        {
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
        public void Save()
        {
            
            using (FileStream world = File.Create(_path))
            using (BinaryWriter binaryWriter = new BinaryWriter(world))
            {
                SaveMetaData(binaryWriter);
                SaveTiles(binaryWriter);
                SaveTileInventories(binaryWriter);
                SaveTileUpdates(binaryWriter);
            }
        }
        private void SaveMetaData(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(_name);
            binaryWriter.Write(DateTime.Now.ToString("MMM dd, yyyy - h:mm tt"));
        }
        private void SaveTiles(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(WorldGen.World.SpawnTile.X);
            binaryWriter.Write(WorldGen.World.SpawnTile.Y);
            binaryWriter.Write(WorldGen.World.WorldSize.X);
            binaryWriter.Write(WorldGen.World.WorldSize.Y);
            binaryWriter.Write(WorldGen.World.SurfaceDepth);
            for (int i = 0; i < WorldGen.World.WorldSize.X; i++)
            {
                for (int j = 0; j < WorldGen.World.WorldSize.Y; j++)
                {
                    binaryWriter.Write(WorldGen.World.GetTileID(i, j));
                    binaryWriter.Write(WorldGen.World.GetTileState(i, j));
                    binaryWriter.Write(WorldGen.World.GetWallID(i, j));
                    binaryWriter.Write(WorldGen.World.GetWallState(i, j));
                    binaryWriter.Write(WorldGen.World.GetLiquid(i, j));
                }
            }
        }
        private void SaveTileInventories(BinaryWriter binaryWriter)
        {
            
        }
        private void SaveTileUpdates(BinaryWriter binaryWriter)
        {

        }
        public void Load()
        {
            if (!Path.Exists(_path))
            {
                throw new FileNotFoundException("Could not find the saved game data.");
            }
            using (FileStream worldData = File.OpenRead(_path))
            using (BinaryReader binaryReader = new BinaryReader(worldData))
            {
                LoadMetaData(binaryReader);
                LoadTiles(binaryReader);
            }
        }
        private void LoadMetaData(BinaryReader binaryReader)
        {
            _name = binaryReader.ReadString();
            _date = binaryReader.ReadString();
        }
        private void LoadTiles(BinaryReader binaryReader)
        {
            WorldGen.World.SpawnTile = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32());
            WorldGen.World.SetWorldSize(binaryReader.ReadInt32(), binaryReader.ReadInt32());
            WorldGen.World.SurfaceDepth = binaryReader.ReadInt32();
            for (int i = 0; i < WorldGen.World.WorldSize.X; i++)
            {
                for (int j = 0; j < WorldGen.World.WorldSize.Y; j++)
                {
                    WorldGen.World.ForceTile(i, j, binaryReader.ReadUInt16());
                    WorldGen.World.SetTileState(i, j, binaryReader.ReadByte());
                    WorldGen.World.ForceWall(i, j, binaryReader.ReadUInt16());
                    WorldGen.World.SetWallState(i, j, binaryReader.ReadByte());
                    WorldGen.World.SetLiquid(i, j, binaryReader.ReadByte());
                }
            }
        }
        private void LoadTileInventories(BinaryReader binaryReader)
        {

        }
        private void LoadTileUpdates(BinaryReader binaryReader)
        {

        }
        private Item[] LoadPlayerInventory(BinaryReader binaryReader)
        {
            return null;
        }
    }
}
