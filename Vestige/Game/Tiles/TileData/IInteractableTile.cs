using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vestige.Game.Tiles.TileData
{
    internal interface IInteractableTile
    {
        void OnRightClick(int x, int y);
    }
}
