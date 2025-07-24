using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    /// <summary>
    /// Component to mark entities for destruction at the end of the frame
    /// This prevents mid-frame destruction issues with collision detection
    /// </summary>
    public struct PendingDestruction
    {
        public DestructionReason Reason;
        public float TimeRemaining; // Optional delay before destruction

        public PendingDestruction(DestructionReason reason, float delay = 0f)
        {
            Reason = reason;
            TimeRemaining = delay;
        }
    }

    public enum DestructionReason
    {
        BoundaryExit,
        Collision,
        Lifetime,
        Manual,
        GameRestart
    }
}
