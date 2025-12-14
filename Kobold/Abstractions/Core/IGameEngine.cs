using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Abstractions.Core
{
    /// <summary>
    /// The basic definition of what makes a game. A game is initialized once at the beginning and shut down once at the end,
    /// and in between, the game runs. While it runs, there is a loop. In every iteration of the loop, the state of everything that
    /// exists in the game (the "entities") is updated and then the current state of the game is rendered. The update loop
    /// requires knowledge of how much time has passed since the last update was called (the "delta time") since update loops
    /// may run at different rates on different hardware.
    /// </summary>
    public interface IGameEngine
    {
        void Initialize();
        void Update(float deltaTime);
        void Render();
        void Shutdown();
    }
}
