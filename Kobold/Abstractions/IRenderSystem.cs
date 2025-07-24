using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Abstractions
{
    /// <summary>
    /// Interface for systems that only need to render
    /// </summary>
    public interface IRenderSystem
    {
        void Render();
    }
}
