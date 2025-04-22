using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TheGreen.Game.Items;
using TheGreen.Game.Tiles;
using TheGreen.Game.WorldGeneration.WorldUpdaters;

namespace TheGreen.Game.WorldGeneration
{
    public class WorldGen
    {
        private static WorldGen _world = null;
        private WorldGen()
        {

        }
        public static WorldGen World
        {
            get
            {
                if (_world == null)
                {
                    _world = new WorldGen();
                }
                return _world;
            }
        }
        private Tile[] _tiles;
        private Random _random = new Random();
        private Point _spawnTile;
        public Point SpawnTile
        {
            get { return _spawnTile; }
        }
        private int _dirtDepth = 20;
        private int _grassDepth = 8;
        /// <summary>
        /// The lowest point of the surface in the world. Y-Down
        /// </summary>
        private int _surfaceHeight;
        public int SurfaceDepth;
        public Point WorldSize;
        public static readonly byte MaxLiquid = 255;
        private int[,,] gradients = new int[256, 256, 2];

        /// <summary>
        /// Stores the location and damage information of any tiles that are actively being mined by the player
        /// </summary>
        private Dictionary<Point, DamagedTile> _minedTiles = new Dictionary<Point, DamagedTile>();
        
        private List<WorldUpdater> _worldUpdaters;
        private LiquidUpdater _liquidUpdater;
        private OverlayTileUpdater _overlayTileUpdater;
        private Dictionary<Point, Item[]> _tileInventories;
        
        public void GenerateWorld(int sizeX, int sizeY, int seed = 0)
        {
            _random = seed != 0 ? new Random(seed) : new Random();
            WorldSize = new Point(sizeX, sizeY);
            _tiles = new Tile[sizeX * sizeY];
            _tileInventories = new Dictionary<Point, Item[]>();
            int[] surfaceNoise = Generate1DNoise(sizeX, 150, 300, 4, 0.4f);
            surfaceNoise = Smooth(surfaceNoise, 2);
            int[] surfaceTerrain = new int[sizeX];

            _surfaceHeight = sizeY;
            //place stone and get surface height
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = sizeY / 2 - sizeY / 4 + surfaceNoise[i]; j < sizeY; j++)
                {
                    SetInitialTile(i, j, 4);
                    SetInitialWall(i, j, 4);
                    surfaceTerrain[i] = sizeY / 2 - sizeY / 4 + surfaceNoise[i];

                    if (sizeY - surfaceTerrain[i] < _surfaceHeight)
                        _surfaceHeight = sizeY - surfaceTerrain[i];
                }
            }
            SurfaceDepth = sizeY - _surfaceHeight;
            //place dirt
            for (int i = 0; i < sizeX; i++)
            {
                
                for (int j = 0; j < _dirtDepth + _random.Next(0, 3); j++)
                {
                    if (surfaceTerrain[i] < SurfaceDepth - 100 - _random.Next(0, 30))
                        continue;
                    if (j > 3)
                    {
                        SetInitialWall(i, surfaceTerrain[i] + j, 1);
                    }
                    else
                    {
                        SetInitialWall(i, surfaceTerrain[i] + j, 0);
                    }
                    SetInitialTile(i, surfaceTerrain[i] + j, 1);
                }
            }

            _spawnTile = new Point(sizeX / 2, surfaceTerrain[sizeX / 2]);
            //generate caves
            InitializeGradients();
            double[,] perlinNoise = GeneratePerlinNoiseWithOctaves(sizeX, _surfaceHeight - _dirtDepth - 1, scale: 25, octaves: 4, persistence: 0.5);
            //threshhold cave noise
            int[] cornersX = [0, 0, 1, -1];
            int[] cornersY = [1, -1, 0, 0];
            //flood fill top edge so there are no sharp cutoffs on caves.
            Queue<Point> fillPoints = new Queue<Point>();
            for (int x = 0; x < sizeX; x++)
            {
                if (perlinNoise[0, x] < -0.1)
                {
                    perlinNoise[0, x] = 1;
                    fillPoints.Enqueue(new Point(x, 0));

                }
            }
            while (fillPoints.Count > 0)
            {
                Point fillPoint = fillPoints.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int x = fillPoint.X + cornersX[i];
                    int y = fillPoint.Y + cornersY[i];
                    if (x < 0 || y < 0 || x >= perlinNoise.GetLength(1) || y >= perlinNoise.GetLength(0))
                        continue;
                    if (perlinNoise[y, x] < -0.1)
                    {
                        perlinNoise[y, x] = 1;
                        fillPoints.Enqueue(new Point(x, y));
                    }
                }
            }
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < _surfaceHeight - _dirtDepth - 1; y++)
                {
                    if (perlinNoise[y, x] < -0.1)
                        RemoveInitialTile(x, sizeY - _surfaceHeight + _dirtDepth + y);
                }
            }

            perlinNoise = null;

            //calculate tile states
            for (int i = 1; i < sizeX - 1; i++)
            {
                for (int j = 1; j < sizeY - 1; j++)
                {
                    SetTileState(i, j, TileDatabase.GetTileData(GetTileID(i, j)).GetUpdatedTileState(i, j));
                    UpdateWallState(i, j);
                }
            }

            //spread grass
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < _grassDepth; j++)
                {
                    if (GetTileID(i, surfaceTerrain[i] + j) == 1 && GetTileState(i, surfaceTerrain[i] + j) != 255)
                    {
                        SetInitialTile(i, surfaceTerrain[i] + j, 2);
                    }
                }
            }

            int minTreeDistance = 5;
            int lastTreeX = 10;
            //Plant Trees
            for (int i = 10; i < sizeX - 10; i++)
            {
                if (_random.NextDouble() < 0.2 && i - lastTreeX > minTreeDistance)
                {
                    if (GetTileID(i, surfaceTerrain[i]) != 2)
                        continue;
                    GenerateTree(i, surfaceTerrain[i] - 1);
                    lastTreeX = i;
                }
            }
        }
        public bool LoadWorld(string worldName)
        {
            try
            {
                string worldPath = Path.Combine(TheGreen.SavePath, "Worlds", worldName);
                if (!Path.Exists(worldPath))
                {
                    return false;
                }
                _tileInventories = new Dictionary<Point, Item[]>();
                using (FileStream worldData = File.OpenRead(Path.Combine(worldPath, worldName + ".bin")))
                using (BinaryReader binaryReader = new BinaryReader(worldData))
                {
                    //TODO: load grass
                    //TODO: load water
                    //TODO: load tile inventories
                    _spawnTile = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32());
                    WorldSize = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32());
                    SurfaceDepth = binaryReader.ReadInt32();
                }
                _tiles = new Tile[WorldSize.X * WorldSize.Y];
                using (FileStream world = File.OpenRead(Path.Combine(worldPath, worldName + ".wld")))
                using (BinaryReader binaryReader = new BinaryReader(world) )
                {
                    for (int i = 0; i < WorldSize.X; i++)
                    {
                        for (int j = 0; j < WorldSize.Y; j++)
                        {
                            SetInitialTile(i, j, binaryReader.ReadUInt16());
                            SetTileState(i, j, binaryReader.ReadByte());
                            SetInitialWall(i, j, binaryReader.ReadUInt16());
                            SetWallState(i, j, binaryReader.ReadByte());
                            SetLiquid(i, j, binaryReader.ReadByte());
                        }
                    }
                }
                Debug.WriteLine("World loading successful");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving world", ex.Message, ["Ok"]);
                return false;
            }
        }
        public bool SaveWorld(string worldName)
        {
            try
            {
                string worldPath = Path.Combine(TheGreen.SavePath, "Worlds", worldName);
                if (!Path.Exists(worldPath))
                {
                    Directory.CreateDirectory(worldPath);
                }
                using (FileStream worldData = File.Create(Path.Combine(worldPath, worldName + ".bin")))
                using (BinaryWriter binaryWriter = new BinaryWriter(worldData))
                {
                    //TODO: write grass
                    //TODO: write water
                    //TODO: write tile inventories
                    binaryWriter.Write(_spawnTile.X);
                    binaryWriter.Write(_spawnTile.Y);
                    binaryWriter.Write(WorldSize.X);
                    binaryWriter.Write(WorldSize.Y);
                    binaryWriter.Write(SurfaceDepth);
                }
                using (FileStream world = File.Create(Path.Combine(worldPath, worldName + ".wld")))
                using (BinaryWriter binaryWriter = new BinaryWriter(world))
                {
                    for (int i = 0; i < WorldSize.X; i++)
                    {
                        for (int j = 0; j < WorldSize.Y; j++)
                        {
                            binaryWriter.Write(GetTileID(i, j));
                            binaryWriter.Write(GetTileState(i, j));
                            binaryWriter.Write(GetWallID(i, j));
                            binaryWriter.Write(GetWallState(i, j));
                            binaryWriter.Write(GetLiquid(i, j));
                        }
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving world", ex.Message, ["Ok"]);
                return false;
            }
        }

        private byte[] _randTreeTileStates = [0, 2, 8, 10];
        private void GenerateTree(int x, int y)
        {
            
            //generate base
            SetInitialTile(x, y, 5);
            SetTileState(x, y, 128);
            if (GetTileID(x-1, y) == 0)
            {
                SetInitialTile(x - 1, y, 5);
                SetTileState(x - 1, y, 62);
            }
            if (GetTileID(x + 1, y) == 0)
            {
                SetInitialTile(x + 1, y, 5);
                SetTileState(x + 1, y, 130);
            }
            //Generate trunk
            int height = _random.Next(5, 20);
            for (int h = 1; h < height; h++)
            {
                SetInitialTile(x, y - h, 5);
                SetTileState(x, y - h, _randTreeTileStates[_random.Next(0, _randTreeTileStates.Length)]);
            }

            //Add tree top
            SetInitialTile(x, y - height, 6);
            SetTileState(x, y - height, 0);
        }
        /// <summary>
        /// Called when a player starts a world. Use this to start any frame or tick updates.
        /// </summary>
        public void InitializeGameUpdates()
        {
            _worldUpdaters = new List<WorldUpdater>();
            _liquidUpdater = new LiquidUpdater(0.04);
            _overlayTileUpdater = new OverlayTileUpdater(5);
            _worldUpdaters.AddRange([ _liquidUpdater, _overlayTileUpdater]);
        }
        public void Update(double delta)
        {
            foreach (WorldUpdater worldUpdater in _worldUpdaters)
            {
                worldUpdater.Update(delta);
            }
            foreach (Point point in _minedTiles.Keys)
            {
                DamagedTile damagedTileData = _minedTiles[point];
                damagedTileData.Time += delta;
                if (damagedTileData.Time > 5 || GetTileID(point.X, point.Y) == 0)
                {
                    _minedTiles.Remove(point);
                }
                else
                    _minedTiles[point] = damagedTileData;
            }
            
        }

        /// <summary>
        /// Damages a tile at the specified point. If the tile health is depleted to 0, it will be removed.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="damage"></param>
        public void DamageTile(Point coordinates, int damage)
        {
            if (!IsTileInBounds(coordinates.X, coordinates.Y))
                return;
            ushort tileID = GetTileID(coordinates.X, coordinates.Y);
            if (tileID == 0)
                return;
            TileData tileData = TileDatabase.GetTileData(tileID);
            if (!tileData.CanTileBeDamaged(coordinates.X, coordinates.Y))
                return;
            DamagedTile damagedTileData = _minedTiles.ContainsKey(coordinates)? _minedTiles[coordinates] : new DamagedTile(coordinates.X, coordinates.Y, tileID, tileData.Health, tileData.Health, 0);
            damagedTileData.Health = damagedTileData.Health - damage;
            damagedTileData.Time = 0;
            if (damagedTileData.Health <= 0)
            {
                RemoveTile(coordinates.X, coordinates.Y);
                _minedTiles.Remove(coordinates);
            }
            else
            {
                _minedTiles[coordinates] = damagedTileData;
            }
            //TODO: play tile specific damage sound here, so it only plays if the tile was actually damaged. any mining sounds or item use sounds will be playes by the item collider or the inventory useItem
        }
        public bool IsTileInBounds(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < WorldSize.X && y < WorldSize.Y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="height"></param>
        /// <param name="frequency"></param>
        /// <param name="octaves">Number of passes. Will make the noise more detailed</param>
        /// <param name="persistance">Value less than 1. Reduces height of next octave.</param>
        /// <returns></returns>
        public int[] Generate1DNoise(int size, float height, int scale, int octaves, float persistance)
        {
            float[] values = new float[256];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (float)_random.NextDouble();
            }
            int[] noise = new int[size];
            for (int octave = 0; octave < octaves; octave++)
            {
                for (int i = 0; i < size; i++)
                {
                    int x0 = (i / scale) % 256;
                    int x1 = (x0 + 1) % 256;
                    int xMinus = (x0 - 1 + 256) % 256;
                    int xPlus = (x1 + 1) % 256;

                    float t = (float)Fade((i % scale) / (float)scale);

                    float y = CatmullRom(
                        values[xMinus] * height,
                        values[x0] * height,
                        values[x1] * height,
                        values[xPlus] * height,
                        t
                    );

                    noise[i] += (int)y;
                }
                scale /= 2;
                height *= persistance;
            }

            return noise;
        }
        private float CatmullRom(float y0, float y1, float y2, float y3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * y1) +
                (-y0 + y2) * t +
                (2f * y0 - 5f * y1 + 4f * y2 - y3) * t2 +
                (-y0 + 3f * y1 - 3f * y2 + y3) * t3
            );
        }

        public int[] Smooth(int[] noise, int passes)
        {
            int[] smoothed = new int[noise.Length];
            for (int pass = 0; pass < passes; pass++)
            {
                for (int i = 0; i < noise.Length; i++)
                {
                    smoothed[i] = (noise[Math.Max(0, i - 1)] + noise[i] + noise[Math.Min(i + 1, noise.Length - 1)]) / 3;
                }
                noise = smoothed;
            }
            return smoothed;
        }

        void InitializeGradients()
        {
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    gradients[x, y, 0] = _random.Next(-1, 2);
                    gradients[x, y, 1] = _random.Next(-1, 2);
                }
            }
        }

        double GetInfluenceValue(double x, double y, int Xgrad, int Ygrad)
        {
            return (gradients[Xgrad % 256, Ygrad % 256, 0] * (x - Xgrad)) +
                   (gradients[Xgrad % 256, Ygrad % 256, 1] * (y - Ygrad));
        }

        double Lerp(double v0, double v1, double t)
        {
            return (1 - t) * v0 + t * v1;
        }

        double Fade(double t)
        {
            return 3 * Math.Pow(t, 2) - 2 * Math.Pow(t, 3);
        }

        double Perlin(double x, double y)
        {
            int X0 = (int)x;
            int Y0 = (int)y;
            int X1 = X0 + 1;
            int Y1 = Y0 + 1;

            double sx = Fade(x - X0);
            double sy = Fade(y - Y0);

            double topLeftDot = GetInfluenceValue(x, y, X0, Y1);
            double topRightDot = GetInfluenceValue(x, y, X1, Y1);
            double bottomLeftDot = GetInfluenceValue(x, y, X0, Y0);
            double bottomRightDot = GetInfluenceValue(x, y, X1, Y0);

            return Lerp(Lerp(bottomLeftDot, bottomRightDot, sx), Lerp(topLeftDot, topRightDot, sx), sy);
        }

        double[,] GeneratePerlinNoiseWithOctaves(int width, int height, double scale = 100.0, int octaves = 4, double persistence = 0.5)
        {
            double[,] noise = new double[height, width];
            double amplitude = 1.0;
            double frequency = 1.0;
            double maxValue = 0;  // To normalize the result

            for (int octave = 0; octave < octaves; octave++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        // Apply frequency and scale to the coordinates for each octave
                        noise[y, x] += Perlin(x / scale * frequency, y / scale * frequency) * amplitude;
                    }
                }

                maxValue += amplitude;
                amplitude *= persistence;  // Amplitude decreases with each octave
                frequency *= 2;  // Frequency doubles for each octave
            }

            // Normalize the noise to be between -1 and 1
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    noise[y, x] /= maxValue;
                }
            }

            return noise;
        }
        public ushort GetTileID(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].ID;
        }
        public ushort GetWallID(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].WallID;
        }
        public bool SetTile(int x, int y, ushort ID)
        {
            if (ID != 0 && TileDatabase.GetTileData(ID).VerifyTile(x, y) != 1)
                return false;
            if (TileDatabase.TileHasProperty(ID, TileProperty.LargeTile))
            {
                SetLargeTile(x, y, ID);
                //TEMPORARY
                //TODO: add tiles updated to a list, and then update the tiles in the list and around the tiles in the list
                return true;
            }
            else 
                _tiles[y * WorldSize.X + x].ID = ID;
            //tile states need to be updated first before calling any other checks
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (TileDatabase.GetTileData(GetTileID(x + i, y + j)).VerifyTile(x + i, y + j) == -1)
                    {
                        RemoveTile(x + i, y + j);
                        continue;
                    }
                    byte state = TileDatabase.GetTileData(GetTileID(x + i, y + j)).GetUpdatedTileState(x + i, y + j);
                    SetTileState(x + i, y + j, state);
                }
            }
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (GetLiquid(x + i, y + j) != 0)
                    {
                        _liquidUpdater.QueueLiquidUpdate(x + i, y + j);
                    }
                    if (TileDatabase.TileHasProperty(GetTileID(x + i, y + j), TileProperty.Overlay))
                    {
                        if (GetTileState(x + i, y + j) == 255)
                            _tiles[(y + j) * WorldSize.X + (x + i)].ID = TileDatabase.GetTileData(GetTileID(x + i, y + j)).BaseTileID;
                        else
                            _overlayTileUpdater.EnqueueOverlayTile(x + i, y + j, GetTileID(x + i, y + j));
                    }
                }
            }
            return true;
        }
        public void RemoveTile(int x, int y)
        {
            ushort tileID = GetTileID(x, y);
            if (TileDatabase.TileHasProperty(tileID, TileProperty.LargeTile))
            {
                RemoveLargeTile(x, y, tileID);
            }
            else
            {
                SetTile(x, y, 0);
            }
            Item item = ItemDatabase.InstantiateItemByTileID(tileID);
            if (item != null)
            {
                Main.EntityManager.AddItemDrop(item, new Vector2(x, y) * TheGreen.TILESIZE);
            }
        }
        private void SetLargeTile(int x, int y, ushort ID)
        {
            if (!(TileDatabase.GetTileData(ID) is LargeTileData largeTileData))
                return;
            Point topLeft = largeTileData.GetTopLeft(x, y) ;
            for (int i = 0; i < largeTileData.TileSize.X; i++)
            {
                for (int j = 0; j < largeTileData.TileSize.Y; j++)
                {
                    _tiles[(topLeft.Y + j) * WorldSize.X + (topLeft.X + i)].ID = ID;
                    SetTileState(topLeft.X + i, topLeft.Y + j, (byte)(j * 10 + i));
                }
            }
        }
        private void RemoveLargeTile(int x, int y, ushort ID)
        {
            if (!(TileDatabase.GetTileData(ID) is LargeTileData largeTileData))
                return;
            Point topLeft = largeTileData.GetTopLeft(x, y);
            for (int i = 0; i < largeTileData.TileSize.X; i++)
            {
                for (int j = 0; j < largeTileData.TileSize.Y; j++)
                {
                    _tiles[(topLeft.Y + j) * WorldSize.X + (topLeft.X + i)].ID = 0;
                    SetTileState(topLeft.X + i, topLeft.Y + j, 0);
                }
            }
        }
        public void SetWall(int x, int y, byte WallID)
        {
            _tiles[y * WorldSize.X + x].WallID = WallID;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    UpdateWallState(x + i, y + j);
                }
            }
        }
        public void SetTileState(int x, int y, byte state)
        {
            _tiles[y * WorldSize.X + x].State = state;
        }

        private void SetWallState(int x, int y, byte state)
        {
            _tiles[y * WorldSize.X + x].WallState = state;
        }

        private void SetInitialTile(int x, int y, ushort ID)
        {
            _tiles[y * WorldSize.X + x].ID = ID;
        }

        private void SetInitialWall(int x, int y, ushort WallID)
        {
            _tiles[y * WorldSize.X + x].WallID = WallID;
        }

        private void RemoveInitialTile(int x, int y)
        {
            _tiles[y * WorldSize.X + x].ID = 0;
        }

        public Dictionary<Point, DamagedTile> GetDamagedTiles()
        {
            return _minedTiles;
        }
        private void UpdateWallState(int x, int y)
        {
            if (GetWallID(x, y) == 0)
            {
                _tiles[y * WorldSize.X + x].WallState = 0;
                return;
            }
            //Important: if a corner doesn't have both sides touching it, it won't be counted
            ushort top = GetWallID(x, y - 1);
            ushort right = GetWallID(x + 1, y);
            ushort bottom = GetWallID(x, y + 1);
            ushort left = GetWallID(x - 1, y);

            SetWallState(x, y, (byte)((Math.Sign(top) * 2) + (Math.Sign(right) * 8) + (Math.Sign(bottom) * 32) + (Math.Sign(left) * 128)));
        }
        public byte GetTileState(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].State;
        }
        public byte GetWallState(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].WallState;
        }
        public byte GetLiquid(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].Liquid;
        }
        public void SetLiquid(int x, int y, byte amount, bool forceUpdate = false)
        {
            if (forceUpdate)
                _liquidUpdater.QueueLiquidUpdate(x, y);
            _tiles[y * WorldSize.X + x].Liquid = amount;
        }
        public void AddTileInventory(Point coordinates, Item[] items)
        {
            _tileInventories[coordinates] = items;
        }
        public void RemoveTileInventory(Point coordinates)
        {
            _tileInventories.Remove(coordinates);
        }
        public Item[] GetTileInventory(Point coordinates)
        {
            if (_tileInventories.ContainsKey(coordinates))
                return _tileInventories[coordinates];
            return null;
        }
    }
}
