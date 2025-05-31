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
        protected override void OnUpdate()
        {
            int numLiquidTiles = _liquidUpdateQueue.Count;
            for (int i = 0; i < numLiquidTiles; i++)
            {
                Point queuedLiquidPoint = _liquidUpdateQueue.Dequeue();
                _liquidTiles.Remove(queuedLiquidPoint);
                SettleLiquid(queuedLiquidPoint.X, queuedLiquidPoint.Y);
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
        //This is the most disgusting code I've ever written
        private void SettleLiquid(int x, int y)
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
                if (WorldGen.World.IsTileInBounds(x + 1, y) && !TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x + 1, y), TileProperty.Solid))
                {
                    totalLiquid += WorldGen.World.GetLiquid(x + 1, y);
                    right++;
                }
                if (WorldGen.World.IsTileInBounds(x - 1, y) && !TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x - 1, y), TileProperty.Solid))
                {
                    totalLiquid += WorldGen.World.GetLiquid(x - 1, y);
                    left++;
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
