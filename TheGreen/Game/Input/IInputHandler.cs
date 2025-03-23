using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGreen.Game.Input
{
    public interface IInputHandler
    {
        void HandleInput(InputEvent @event);
    }
}
