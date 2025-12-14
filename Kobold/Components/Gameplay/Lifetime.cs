using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components.Gameplay
{
    public struct Lifetime
    {
        public float RemainingTime;

        public Lifetime(float lifetime)
        {
            RemainingTime = lifetime;
        }
    }
}
