using System;
using System.Collections.Generic;
using TheGreen.Game.Tiles;

namespace TheGreen.Game.WorldGeneration.WorldUpdaters
{
    //TODO: move liquid settling to here
    internal class LiquidUpdater : WorldUpdater
    {
        private Queue<(int, int)> _liquidUpdateQueue = new Queue<(int, int)>();
        private HashSet<(int, int)> _liquidTiles = new HashSet<(int, int)>();
        public LiquidUpdater(double updateRate) : base(updateRate)
        {
        }

        protected override void OnUpdate()
        {
            int numLiquidTiles = _liquidUpdateQueue.Count;
            for (int i = 0; i < numLiquidTiles; i++)
            {
                (int, int) queuedLiquidPoint = _liquidUpdateQueue.Dequeue();
                _liquidTiles.Remove(queuedLiquidPoint);
                SettleLiquid(queuedLiquidPoint.Item1, queuedLiquidPoint.Item2);
            }
        }
        public void QueueLiquidUpdate(int x, int y)
        {
            if (!_liquidTiles.Contains((x, y)))
            {
                _liquidUpdateQueue.Enqueue((x, y));
                _liquidTiles.Add((x, y));
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
                int flow = Math.Min(255 - WorldGen.World.GetLiquid(x, y + 1), remainingMass);
                WorldGen.World.SetLiquid(x, y, (byte)(WorldGen.World.GetLiquid(x, y) - flow));
                WorldGen.World.SetLiquid(x, y + 1, (byte)(WorldGen.World.GetLiquid(x, y + 1) + flow));
                remainingMass -= flow;
            }
            if (remainingMass <= 0)
                return;

            if (WorldGen.World.IsTileInBounds(x - 1, y) && !TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x - 1, y), TileProperty.Solid))
            {
                int flow = (int)Math.Ceiling((WorldGen.World.GetLiquid(x, y) - WorldGen.World.GetLiquid(x - 1, y)) / 4.0f);
                flow = int.Clamp(flow, 0, remainingMass);
                WorldGen.World.SetLiquid(x, y, (byte)(WorldGen.World.GetLiquid(x, y) - flow));
                WorldGen.World.SetLiquid(x - 1, y, (byte)(WorldGen.World.GetLiquid(x - 1, y) + flow));
                remainingMass -= flow;
            }
            if (remainingMass <= 0)
                return;
            if (WorldGen.World.IsTileInBounds(x + 1, y) && !TileDatabase.TileHasProperty(WorldGen.World.GetTileID(x + 1, y), TileProperty.Solid))
            {
                int flow = (int)Math.Ceiling((WorldGen.World.GetLiquid(x, y) - WorldGen.World.GetLiquid(x + 1, y)) / 4.0f);
                flow = int.Clamp(flow, 0, remainingMass);
                WorldGen.World.SetLiquid(x, y, (byte)(WorldGen.World.GetLiquid(x, y) - flow));
                WorldGen.World.SetLiquid(x + 1, y, (byte)(WorldGen.World.GetLiquid(x + 1, y) + flow));
                remainingMass -= flow;
            }
        }
    }
}
