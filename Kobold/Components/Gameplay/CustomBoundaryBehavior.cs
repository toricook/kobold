using Kobold.Core.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components.Gameplay
{
    /// <summary>
    /// Component that allows an entity to have custom boundary behavior that overrides the default
    /// </summary>
    public struct CustomBoundaryBehavior
    {
        public BoundaryBehavior Behavior;

        public CustomBoundaryBehavior(BoundaryBehavior behavior)
        {
            Behavior = behavior;
        }
    }
}
