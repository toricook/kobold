using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Abstractions.Engine
{
    /// <summary>
    /// At its core, a system is just a thing that is reponsible for applying updates to entities.
    /// </summary>
    public interface ISystem
    {
        void Update(float deltaTime);
    }
}
