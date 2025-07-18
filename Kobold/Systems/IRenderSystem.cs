using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Systems
{
    /// <summary>
    /// Interface for systems that only need to render
    /// </summary>
    public interface IRenderSystem
    {
        void Render();
    }
}
