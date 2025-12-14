using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    /// <summary>
    /// Standard rendering layer values
    /// Lower values render first (behind), higher values render last (on top)
    /// </summary>
    public static class RenderLayers
    {
        public const int Background = -100;
        public const int GameObjects = 0;
        public const int Effects = 50;
        public const int UI = 100;
        public const int Debug = 1000;
    }

}
