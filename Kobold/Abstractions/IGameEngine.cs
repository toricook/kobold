using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Abstractions
{
    public interface IGameEngine
    {
        void Initialize();
        void Update(float deltaTime);
        void Render();
        void Shutdown();
    }
}
