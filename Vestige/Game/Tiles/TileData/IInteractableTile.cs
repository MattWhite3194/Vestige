using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vestige.Game.WorldGeneration;

namespace Vestige.Game.Tiles.TileData
{
    internal interface IInteractableTile
    {
        void OnRightClick(WorldGen world, int x, int y);
    }
}
