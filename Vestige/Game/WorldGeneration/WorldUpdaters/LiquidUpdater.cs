using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vestige.Game.Tiles;

namespace Vestige.Game.WorldGeneration.WorldUpdaters
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
            int remainingMass = Main.World.GetLiquid(x, y);
            if (Main.World.IsTileInBounds(x, y + 1) && !TileDatabase.TileHasProperty(Main.World.GetTileID(x, y + 1), TileProperty.Solid))
            {
                int flow = Math.Min(WorldGen.MaxLiquid - Main.World.GetLiquid(x, y + 1), remainingMass);
                if (flow != 0)
                {
                    Main.World.SetLiquid(x, y, (byte)(Main.World.GetLiquid(x, y) - flow));
                    Main.World.SetLiquid(x, y + 1, (byte)(Main.World.GetLiquid(x, y + 1) + flow), true);
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
                if (Main.World.IsTileInBounds(x + 1, y) && !TileDatabase.TileHasProperty(Main.World.GetTileID(x + 1, y), TileProperty.Solid))
                {
                    totalLiquid += Main.World.GetLiquid(x + 1, y);
                    right++;
                }
                if (Main.World.IsTileInBounds(x - 1, y) && !TileDatabase.TileHasProperty(Main.World.GetTileID(x - 1, y), TileProperty.Solid))
                {
                    totalLiquid += Main.World.GetLiquid(x - 1, y);
                    left++;
                }
                int averageLiquid = (int)Math.Round((float)totalLiquid / (left + right + 1));
                int numNotChanged = 0;
                for (int i = -left; i <= right; i++)
                {
                    if (i == 0)
                        continue;
                    if (Main.World.GetLiquid(x + i, y) != averageLiquid)
                        Main.World.SetLiquid(x + i, y, (byte)averageLiquid, true);
                    else
                        numNotChanged++;
                }
                if (averageLiquid == WorldGen.MaxLiquid - 1 && Main.World.GetLiquid(x, y) == WorldGen.MaxLiquid)
                {
                    averageLiquid = WorldGen.MaxLiquid;
                }
                Main.World.SetLiquid(x, y, (byte)averageLiquid);
            }
            else
            {
                if (Main.World.GetLiquid(x - 1, y) != 0)
                {
                    QueueLiquidUpdate(x - 1, y);
                }
                if (Main.World.GetLiquid(x + 1, y) != 0)
                {
                    QueueLiquidUpdate(x + 1, y);
                }
            }
            if (Main.World.GetLiquid(x, y - 1) != 0 && Main.World.GetLiquid(x, y) != WorldGen.MaxLiquid)
                QueueLiquidUpdate(x, y - 1);
        }
    }
}
