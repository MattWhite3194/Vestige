using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheGreen.Game.Entities;
using TheGreen.Game.Items;
using TheGreen.Game.Tiles;

namespace TheGreen.Game.WorldGeneration
{
    public class WorldGen
    {
        private static WorldGen _instance;
        private WorldGen()
        {

        }

        public static WorldGen Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WorldGen();
                }
                return _instance;
            }
        }
        private Tile[] _tiles;
        private readonly Random _random = new Random();
        private Point _spawnTile;
        public Point SpawnTile
        {
            get { return _spawnTile; }
        }
        private int _dirtDepth = 20;
        private int _grassDepth = 8;
        private int _surfaceHeight;
        public Point WorldSize;
        private int[,,] gradients = new int[256, 256, 2];
        public Texture2D Map;
        public readonly byte TileLightAbsorption = 32;
        public readonly byte WallLightAbsorption = 8;
        /// <summary>
        /// The maximum distance light can travel
        /// </summary>
        private int _lightRange;
        /// <summary>
        /// Quick access to surrounding tile points
        /// </summary>
        private Point[] _surroundingTiles = { new Point(0, -1), new Point(0, 1), new Point(-1, 0), new Point(1, 0) };

        /// <summary>
        /// Stores the location and damage information of any tiles that are actively being mined by the player
        /// </summary>
        private Dictionary<Point, DamagedTile> _minedTiles = new Dictionary<Point, DamagedTile>();
        private Queue<Point> _liquidUpdateQueue = new Queue<Point>();
        public class DamagedTile
        {
            /// <summary>
            /// The health left on the tile
            /// </summary>
            public int Health;
            /// <summary>
            /// The time left before the tile is removed from the dictionary and any damage done is reset
            /// </summary>
            public double Time;
            public DamagedTile(int health, int time)
            {
                this.Health = health;
                this.Time = time;
            }
        }
        public void GenerateWorld(int size_x, int size_y)
        {
            WorldSize = new Point(size_x, size_y);
            _tiles = new Tile[size_x * size_y];
            int[] surfaceNoise = Generate1DNoise(size_x, 50, 10, 6, 0.5f);
            int[] surfaceTerrain = new int[size_x];

            _surfaceHeight = size_y;
            //place stone and get surface height
            for (int i = 0; i < size_x; i++)
            {
                for (int j = size_y / 2 - size_y / 4 + surfaceNoise[i]; j < size_y; j++)
                {
                    SetInitialTile(i, j, 4);
                    SetInitialWall(i, j, 4);
                    surfaceTerrain[i] = size_y / 2 - size_y / 4 + surfaceNoise[i];

                    if (size_y - surfaceTerrain[i] < _surfaceHeight)
                        _surfaceHeight = size_y - surfaceTerrain[i];
                }
            }
            //place dirt
            for (int i = 0; i < size_x; i++)
            {
                for (int j = 0; j < _dirtDepth; j++)
                {
                    if (j > 4)
                    {
                        SetInitialWall(i, surfaceTerrain[i], 0);
                    }
                    SetInitialTile(i, surfaceTerrain[i] + j, 1);
                }
            }

            _spawnTile = new Point(size_x / 2, surfaceTerrain[size_x / 2]);
            //generate caves
            InitializeGradients();
            double[,] perlinNoise = GeneratePerlinNoiseWithOctaves(size_x, _surfaceHeight - _dirtDepth - 1, scale: 40, octaves: 5, persistence: 0.5);
            //threshhold cave noise
            for (int x = 0; x < size_x; x++)
            {
                for (int y = 0; y < _surfaceHeight - _dirtDepth - 1; y++)
                {
                    if (perlinNoise[y, x] < -0.1)
                    {
                        RemoveInitialTile(x, size_y - _surfaceHeight + _dirtDepth + y);
                    }
                }
            }

            perlinNoise = null;

            //calculate tile states
            for (int i = 1; i < size_x - 1; i++)
            {
                for (int j = 1; j < size_y - 1; j++)
                {
                    SetTileState(i, j, TileDatabase.GetUpdatedTileState(GetTileID(i, j), i, j));
                    UpdateWallState(i, j);
                }
            }

            //spread grass
            for (int i = 0; i < size_x; i++)
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
            for (int i = 10; i < size_x - 10; i++)
            {
                if (_random.NextDouble() < 0.2 && i - lastTreeX > minTreeDistance)
                {
                    GenerateTree(i, surfaceTerrain[i] - 1);
                    lastTreeX = i;
                }
            }

            //generate map file
            /*Color[] colorData = new Color[size_x * size_y];
            for (int x = 0; x < size_x; x++)
            {
                for (int y = 0; y < size_y; y++)
                {
                    colorData[x + y * size_x] = TileDatabase.GetTileMapColor(GetTileID(x, y));
                }
            }

            Map.SetData(colorData);
            string gamePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TheGreen");
            if (!Directory.Exists(gamePath))
            {
                Directory.CreateDirectory(gamePath);
            }
            Stream stream = File.Create(gamePath + "/map.jpg");
            Map.SaveAsJpeg(stream, size_x, size_y);
            stream.Close();*/

            _lightRange = 256 / WallLightAbsorption;
            CalculateInitialLighting();
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

        public void LoadWorld()
        {
            //TODO: implementation
            //get size from file
            _tiles = new Tile[0];
        }

        public void Update(double delta)
        {
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

            int numLiquidsUpdated = 0;
            while (_liquidUpdateQueue.Count != 0 && numLiquidsUpdated < 3)
            {
                numLiquidsUpdated++;
                Point queuedLiquidPoint = _liquidUpdateQueue.Dequeue();
                SettleLiquid(queuedLiquidPoint.X, queuedLiquidPoint.Y);
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
            if (GetTileID(coordinates.X, coordinates.Y) == 0)
                return;
            DamagedTile damagedTileData = _minedTiles.ContainsKey(coordinates)? _minedTiles[coordinates] : new DamagedTile(TileDatabase.GetTileHealth(GetTileID(coordinates.X, coordinates.Y)), 0);
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
        }

        public void QueueLiquidUpdate(int x, int y)
        {
            _liquidUpdateQueue.Enqueue(new Point(x, y));
        }
        private void SettleLiquid(int x, int y)
        {
            int minMass = 5;
            int remainingMass = GetLiquid(x, y);
            if (remainingMass < minMass) {
                SetLiquid(x, y, 0);
                return;
            }

            if (IsTileInBounds(x, y + 1) && !TileDatabase.TileHasProperty(GetTileID(x, y + 1), TileProperty.Solid))
            {
                int flow = Math.Min(255 - GetLiquid(x, y + 1), remainingMass);
                SetLiquid(x, y, (byte)(GetLiquid(x, y) - flow));
                SetLiquid(x, y + 1, (byte)(GetLiquid(x, y + 1) + flow));
                remainingMass -= flow;
            }
            if (remainingMass <= 0)
                return;

            if (IsTileInBounds(x - 1, y) && !TileDatabase.TileHasProperty(GetTileID(x - 1, y), TileProperty.Solid))
            {
                int flow = (GetLiquid(x, y) - GetLiquid(x - 1, y)) / 4;
                flow = int.Clamp(flow, 0, remainingMass);

                SetLiquid(x, y, (byte)(GetLiquid(x, y) - flow));
                SetLiquid(x - 1, y, (byte)(GetLiquid(x - 1, y) + flow));
                remainingMass -= flow;
            }
            if (remainingMass <= 0)
                return;
            if (IsTileInBounds(x + 1, y) && !TileDatabase.TileHasProperty(GetTileID(x + 1, y), TileProperty.Solid))
            {
                int flow = (GetLiquid(x, y) - GetLiquid(x + 1, y)) / 4;
                flow = int.Clamp(flow, 0, remainingMass);

                SetLiquid(x, y, (byte)(GetLiquid(x, y) - flow));
                SetLiquid(x + 1, y, (byte)(GetLiquid(x + 1, y) + flow));
                remainingMass -= flow;
            }
        }
        private void CalculateInitialLighting()
        {
            //TODO: possibly change this nightmare to the recursive algorithm, same as dynamic light calculations
            for (int i = 0; i < WorldSize.X; i++)
            {
                for (int j = 0; j < WorldSize.Y; j++)
                {
                    if (TileDatabase.TileHasProperty(GetTileID(i, j), TileProperty.Solid) || GetWallID(i, j) != 0)
                    {
                        SetTileLight(i, j, 0);
                    }
                    else
                    {
                        SetTileLight(i, j, 255);
                    }
                }
            }
            for (int i = 0; i < WorldSize.X; i++)
            {
                for (int j = 0; j < WorldSize.Y; j++)
                {
                    CalculateTileLight(i, j);
                }
            }
            for (int i = WorldSize.X - 1; i >= 0; i--)
            {
                for (int j = WorldSize.Y - 1; j >= 0; j--)
                {
                    CalculateTileLight(i, j);
                }
            }
            for (int i = WorldSize.X - 1; i >= 0; i--)
            {
                for (int j = 0; j < WorldSize.Y; j++)
                {
                    CalculateTileLight(i, j);
                }
            }
            for (int i = WorldSize.X - 1; i >= 0; i--)
            {
                for (int j = 0; j < WorldSize.Y; j++)
                {
                    CalculateTileLight(i, j);
                }
            }
        }
        public bool IsTileInBounds(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < WorldSize.X && y < WorldSize.Y);
        }
        private void RecalculateTileLighting(int x, int y)
        {

            //4 O(n^2) loops fucking horrible, but it works so optimize later
            //spread light from left to right and top to bottom
            for (int i = -_lightRange; i <= _lightRange; i++)
            {
                for (int j = -_lightRange; j <= _lightRange; j++)
                {
                    if (!IsTileInBounds(x + i, y + j))
                    {
                        continue;
                    }
                    if (!TileDatabase.TileHasProperty(GetTileID(x + i, y + j), TileProperty.Solid) && GetWallID(x + i, y + j) == 0)
                    {
                        SetTileLight(x + i, y + j, 255);
                    }
                    else
                    {
                        SetTileLight(x + i, y + j, 0);
                    }
                    CalculateTileLight(x + i, y + j);
                }
            }
            //spread light from right to left and bottom to top
            for (int i = _lightRange; i >= -_lightRange; i--)
            {
                for (int j = _lightRange; j >= -_lightRange; j--)
                {
                    if (!IsTileInBounds(x + i, y + j))
                    {
                        continue;
                    }
                    CalculateTileLight(x + i, y + j);
                }
            }
            //spread light from right to left 
            for (int i = _lightRange; i >= -_lightRange; i--)
            {
                for (int j = -_lightRange; j <= _lightRange; j++)
                {
                    if (!IsTileInBounds(x + i, y + j))
                    {
                        continue;
                    }
                    CalculateTileLight(x + i, y + j);
                }
            }
            //spread light from left to right and bottom to top
            for (int i = -_lightRange; i <= _lightRange; i++)
            {
                for (int j = _lightRange; j >= -_lightRange; j--)
                {
                    if (!IsTileInBounds(x + i, y + j))
                    {
                        continue;
                    }
                    CalculateTileLight(x + i, y + j);
                }
            }
        }

        private void CalculateTileLight(int x, int y)
        {
            if (GetTileLight(x, y) == 255)
                return;
            int light = GetTileLight(x, y);
            foreach (Point point in _surroundingTiles)
            {
                if (!IsTileInBounds(x + point.X, y + point.Y))
                {
                    continue;
                }
                byte absorption = 0;
                if (GetWallID(x + point.X, y + point.Y) != 0)
                    absorption = WallLightAbsorption;
                if (TileDatabase.TileHasProperty(GetTileID(x + point.X, y + point.Y), TileProperty.Solid))
                    absorption = TileLightAbsorption;
                if (GetTileLight(x + point.X, y + point.Y) - absorption > light)
                    light = GetTileLight(x + point.X, y + point.Y) - absorption;
            }
            if (light < 0)
                light = 0;
            SetTileLight(x, y, (byte)light);
            return;
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
        public int[] Generate1DNoise(int size, float height, float frequency, int octaves, float persistance)
        {
            float[] noise = new float[size];
            int[] intNoise = new int[size];
            for (int octave = 0; octave < octaves; octave++)
            {
                float[] values = new float[(int)frequency];
                int xOffset = (int)(4000 / frequency);
                int currentValue = 0;
                int nextValue = 1;

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (float)_random.NextDouble() * height + noise[(i * xOffset) % size];
                }

                int step = 0;
                for (int i = 0; i < size; i++)
                {
                    if (currentValue >= values.Length)
                    {
                        currentValue = 0;
                    }
                    if (nextValue >= values.Length)
                    {
                        nextValue = 0;
                    }
                    noise[i] = CubicInterpolation(currentValue * xOffset, values[currentValue], nextValue * xOffset, values[nextValue], i);
                    step++;
                    if (step == xOffset)
                    {
                        currentValue++;
                        nextValue++;
                        step = 0;
                    }
                }
                frequency *= 2;
                height *= persistance;
            }

            for (int i = 0; i < intNoise.Length; i++)
            {
                intNoise[i] = (int)(noise[i]);
            }

            return intNoise;
        }

        private float CubicInterpolation(float x0, float y0, float x1, float y1, float t)
        {
            float normalized_t = (t - x0) / (x1 - x0);

            float mu2 = (1.0f - (float)Math.Cos(normalized_t * Math.PI)) / 2.0f;

            return y0 * (1.0f - mu2) + y1 * mu2;
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

        public byte GetTileLight(int x, int y)
        {
            return _tiles[y * WorldSize.X + x].Light;
        }

        private void SetTileLight(int x, int y, byte light)
        {
            _tiles[y * WorldSize.X + x].Light = light;
        }

        public bool SetTile(int x, int y, ushort ID)
        {
            if (ID != 0 && TileDatabase.VerifyTile(ID, x, y) != 1)
                return false;
            _tiles[y * WorldSize.X + x].ID = ID;
            RecalculateTileLighting(x, y);
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    ushort tileID = GetTileID(x + i, y + j);
                    if (TileDatabase.VerifyTile(tileID, x + i, y + j) == -1)
                    {
                        RemoveTile(x + i, y + j);
                    }
                    if (GetLiquid(x + i, y + j) != 0)
                    {
                        QueueLiquidUpdate(x + i, y + j);
                    }
                    byte state = TileDatabase.GetUpdatedTileState(tileID, x + i, y + j);
                    if (state == 255 && TileDatabase.TileHasProperty(tileID, TileProperty.Overlay))
                        _tiles[(y + j) * WorldSize.X + (x + i)].ID = TileDatabase.GetTileBaseID(tileID);
                    SetTileState(x + i, y + j, state);
                }
            }
            return true;
        }

        public void SetWall(int x, int y, byte WallID)
        {
            _tiles[y * WorldSize.X + x].WallID = WallID;
            RecalculateTileLighting(x, y);
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    UpdateWallState(x + i, y + j);
                }
            }
        }

        public void RemoveTile(int x, int y)
        {
            Item item = ItemDatabase.GetItemByTileID(GetTileID(x, y));
            if (item != null)
            {
                EntityManager.Instance.AddItemDrop(item, new Vector2(x, y) * Globals.TILESIZE);
            }
            SetTile(x, y, 0);
        }

        private void SetInitialTile(int x, int y, ushort ID)
        {
            _tiles[y * WorldSize.X + x].ID = ID;
        }

        private void SetInitialWall(int x, int y, byte WallID)
        {
            _tiles[y * WorldSize.X + x].WallID = WallID;
        }

        private void RemoveInitialTile(int x, int y)
        {
            _tiles[y * WorldSize.X + x].ID = 0;
        }

        private void SetTileState(int x, int y, byte state)
        {
            _tiles[y * WorldSize.X + x].State = state;
        }

        public Dictionary<Point, DamagedTile> GetMinedTiles()
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

            _tiles[y * WorldSize.X + x].WallState = (byte)((Math.Sign(top) * 2) + (Math.Sign(right) * 8) + (Math.Sign(bottom) * 32) + (Math.Sign(left) * 128));
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
        public void SetLiquid(int x, int y, byte amount)
        {
            if (amount > 0 && GetLiquid(x, y) != amount)
                QueueLiquidUpdate(x, y);
            _tiles[y * WorldSize.X + x].Liquid = amount;
        }
    }
}
