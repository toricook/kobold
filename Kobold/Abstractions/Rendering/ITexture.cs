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
        /// <summary>
        /// Width of the texture in pixels
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Height of the texture in pixels
        /// </summary>
        int Height { get; }
    }
}
