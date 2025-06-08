using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Vestige.Game.Tiles;
using Vestige.Game.Tiles.TileData;

namespace Vestige.Game.WorldGeneration.WorldUpdaters
{
    /// <summary>
    /// Spreads overlay tiles marked as spreadable. Examples: grass
    /// </summary>
    internal class OverlayTileUpdater : WorldUpdater
    {
        private Queue<(int, int, ushort)> _overlayUpdateQueue = new Queue<(int, int, ushort)>();
        private HashSet<(int, int, ushort)> _baseTiles = new HashSet<(int, int, ushort)>();
        private Point[] _surroundingTiles = { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };

        public OverlayTileUpdater(WorldGen world, double updateRate) : base(world, updateRate)
        {
        }

        public void EnqueueOverlayTile(int x, int y, ushort overlayTileID)
        {
            foreach (Point point in _surroundingTiles)
            {
                if (!world.IsTileInBounds(x + point.X, y + point.Y))
                {
                    continue;
                }

                if (_baseTiles.Contains((x + point.X, y + point.Y, overlayTileID)))
                {
                    continue;
                }
                if (world.GetTileID(x + point.X, y + point.Y) != ((OverlayTileData)TileDatabase.GetTileData(overlayTileID)).BaseTileID || world.GetTileState(x + point.X, y + point.Y) == 255)
                    continue;
                _overlayUpdateQueue.Enqueue((x + point.X, y + point.Y, overlayTileID));
                _baseTiles.Add((x + point.X, y + point.Y, overlayTileID));
            }
        }

        protected override void OnUpdate()
        {
            if (_overlayUpdateQueue.Count == 0)
                return;
            (int x, int y, ushort overlayTileID) = _overlayUpdateQueue.Dequeue();
            _baseTiles.Remove((x, y, overlayTileID));
            bool foundOverlayTile = false;
            foreach (Point point in _surroundingTiles)
            {
                if (world.GetTileID(x + point.X, y + point.Y) == overlayTileID)
                {
                    foundOverlayTile = true;
                    break;
                }
            }
            if (!foundOverlayTile)
                return;
            if (world.GetTileState(x, y) != 255 && world.GetTileID(x, y) == ((OverlayTileData)TileDatabase.GetTileData(overlayTileID)).BaseTileID)
            {
                world.PlaceTile(x, y, overlayTileID);
            }
        }
    }
}
