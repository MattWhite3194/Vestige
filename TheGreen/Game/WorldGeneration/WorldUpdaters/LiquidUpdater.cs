using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheGreen.Game.Tiles;

namespace TheGreen.Game.WorldGeneration.WorldUpdaters
{
    //TODO: move liquid settling to here
    internal class LiquidUpdater : WorldUpdater
    {
        private Queue<Point> _liquidUpdateQueue = new Queue<Point>();
        private HashSet<Point> _liquidTiles = new HashSet<Point>();
        public LiquidUpdater(double updateRate) : base(updateRate)
        {
        }

        //if ypu're unhappy with the current system:
        //Terrarias Method: -3 -2 -1 0 1 2 3 getting average of all of these tiles liquids, setting all of them to the average, if a tile is not the average before it's set, Add it to _liquidUpdateQueue
        //A valid tile for this average is a tile with the same liquid type and it contains liquid. if it doesn;t contain liquid, don't count it. also do the lower transfer first, which is just adding the minimum of the tile above's current liquid level and the tile below required liquid level.
        protected override void OnUpdate()
        {
            int numLiquidTiles = _liquidUpdateQueue.Count;
            //(int, int)[] updateArray = _liquidTiles.ToArray();
            //for (int passes = 0; passes < 3; passes++)
            //{
            //    for (int i = 0; i < numLiquidTiles; i++) {
            //        SettleLiquid(updateArray[i].Item1, updateArray[i].Item2);
            //    }
            //}
            for (int i = 0; i < numLiquidTiles; i++)
            {
                Point queuedLiquidPoint = _liquidUpdateQueue.Dequeue();
                _liquidTiles.Remove(queuedLiquidPoint);
                SettleLiquid_Method2(queuedLiquidPoint.X, queuedLiquidPoint.Y);
            }
        }
        public void QueueLiquidUpdate(int x, int y)
        {
            Point point = new Point(x, y);
            if (!_liquidTiles.Contains(point))
            {
                
                _liquidUpdateQueue.Enqueue(point);
                _liquidTiles.Add(point);
            }
        }
        private void SettleLiquid(int x, int y)
        {
            int remainingMass = WorldGen.World.GetLiquid(x, y);
            if (remainingMass <= 2)
            {
                WorldGen.World.SetLiquid(x, y, 0);
                return;
            }

            if (WorldGen.World.IsTileInBounds(x, y + 1) && !TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x, y + 1), TileProperty.Solid))
            {
                int flow = Math.Min(WorldGen.MaxLiquid - WorldGen.World.GetLiquid(x, y + 1), remainingMass);
                WorldGen.World.SetLiquid(x, y, (byte)(WorldGen.World.GetLiquid(x, y) - flow));
                WorldGen.World.SetLiquid(x, y + 1, (byte)(WorldGen.World.GetLiquid(x, y + 1) + flow));
                remainingMass -= flow;
            }
            if (remainingMass <= 0)
                return;

            if (WorldGen.World.IsTileInBounds(x - 1, y) && !TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x - 1, y), TileProperty.Solid))
            {
                int flow = (int)Math.Ceiling((WorldGen.World.GetLiquid(x, y) - WorldGen.World.GetLiquid(x - 1, y)) * 0.5f);
                flow = int.Clamp(flow, 0, remainingMass);
                WorldGen.World.SetLiquid(x, y, (byte)(WorldGen.World.GetLiquid(x, y) - flow));
                WorldGen.World.SetLiquid(x - 1, y, (byte)(WorldGen.World.GetLiquid(x - 1, y) + flow));
                remainingMass -= flow;
            }
            if (remainingMass <= 0)
                return;
            if (WorldGen.World.IsTileInBounds(x + 1, y) && !TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x + 1, y), TileProperty.Solid))
            {
                int flow = (int)Math.Ceiling((WorldGen.World.GetLiquid(x, y) - WorldGen.World.GetLiquid(x + 1, y)) * 0.5f);
                flow = int.Clamp(flow, 0, remainingMass);
                WorldGen.World.SetLiquid(x, y, (byte)(WorldGen.World.GetLiquid(x, y) - flow));
                WorldGen.World.SetLiquid(x + 1, y, (byte)(WorldGen.World.GetLiquid(x + 1, y) + flow));
                remainingMass -= flow;
            }
        }
        //This is the most disgusting code I've ever written
        private void SettleLiquid_Method2(int x, int y)
        {
            int remainingMass = WorldGen.World.GetLiquid(x, y);
            if (WorldGen.World.IsTileInBounds(x, y + 1) && !TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x, y + 1), TileProperty.Solid))
            {
                int flow = Math.Min(WorldGen.MaxLiquid - WorldGen.World.GetLiquid(x, y + 1), remainingMass);
                if (flow != 0)
                {
                    WorldGen.World.SetLiquid(x, y, (byte)(WorldGen.World.GetLiquid(x, y) - flow));
                    WorldGen.World.SetLiquid(x, y + 1, (byte)(WorldGen.World.GetLiquid(x, y + 1) + flow), true);
                    remainingMass -= flow;
                }
            }
            if (remainingMass > 0)
            {
                if (remainingMass < 3)
                {
                    remainingMass -= 1;
                }
                int left = 0;
                int right = 0;
                int totalLiquid = remainingMass;
                bool continueRight = true;
                bool continueLeft = true;
                for (int i = 1; i <= 1; i++)
                {
                    if (WorldGen.World.IsTileInBounds(x + i, y) && !TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x + i, y), TileProperty.Solid) && continueRight)
                    {
                        totalLiquid += WorldGen.World.GetLiquid(x + i, y);
                        right++;
                    }
                    else
                        continueRight = false;
                    if (WorldGen.World.IsTileInBounds(x - i, y) && !TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x - i, y), TileProperty.Solid) && continueLeft)
                    {
                        totalLiquid += WorldGen.World.GetLiquid(x - i, y);
                        left++;
                    }
                    else
                        continueLeft = false;
                    if (!continueLeft || !continueRight)
                        break;
                }
                int averageLiquid = (int)Math.Round((float)totalLiquid / (left + right + 1));
                int numNotChanged = 0;
                for (int i = -left; i <= right; i++)
                {
                    if (i == 0)
                        continue;
                    if (WorldGen.World.GetLiquid(x + i, y) != averageLiquid)
                        WorldGen.World.SetLiquid(x + i, y, (byte)averageLiquid, true);
                    else
                        numNotChanged++;
                }
                if (averageLiquid == WorldGen.MaxLiquid - 1 && WorldGen.World.GetLiquid(x, y) == WorldGen.MaxLiquid)
                {
                    averageLiquid = WorldGen.MaxLiquid;
                }
                WorldGen.World.SetLiquid(x, y, (byte)averageLiquid);
            }
            else
            {
                if (WorldGen.World.GetLiquid(x - 1, y) != 0)
                {
                    QueueLiquidUpdate(x - 1, y);
                }
                if (WorldGen.World.GetLiquid(x + 1, y) != 0)
                {
                    QueueLiquidUpdate(x + 1, y);
                }
            }
            if (WorldGen.World.GetLiquid(x, y - 1) != 0 && WorldGen.World.GetLiquid(x, y) != WorldGen.MaxLiquid)
                QueueLiquidUpdate(x, y - 1);
        }
    }
}
