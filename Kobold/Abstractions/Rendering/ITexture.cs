using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Abstractions.Rendering
{
    /// <summary>
    /// An abstraction for an image that can be rendered by the graphics backend
    /// </summary>
    public interface ITexture
    {
        int Width { get; }
        int Height { get; }
    }
}
