using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    public struct ScreenBounds
    {
        public float Width;
        public float Height;

        public ScreenBounds(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }
}
