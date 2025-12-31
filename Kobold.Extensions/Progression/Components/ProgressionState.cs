using System;
using System.Collections.Generic;

namespace Kobold.Extensions.Progression
{
    /// <summary>
    /// Component tracking level progression state for roguelike/dungeon crawler games.
    /// Manages depth tracking, level history, and level snapshots for seamless navigation.
    /// Attach to a game state entity.
    /// </summary>
    public struct ProgressionState
    {
        /// <summary>
        /// Current depth level (0-based, where 0 is the surface/first level)
        /// </summary>
        public int CurrentDepth { get; set; }

        /// <summary>
        /// Maximum allowed depth (default: 10 levels, 0-9)
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// History stack of visited depths for navigation.
        /// Push on descend, pop on ascend.
        /// </summary>
        public Stack<int> DepthHistory { get; set; }

        /// <summary>
        /// Snapshots of visited levels, keyed by depth.
        /// Allows returning to previous levels with exact same layout.
        /// </summary>
        public Dictionary<int, LevelStateSnapshot> LevelSnapshots { get; set; }

        /// <summary>
        /// Creates a new ProgressionState with default values
        /// </summary>
        public ProgressionState()
        {
            CurrentDepth = 0;
            MaxDepth = 10;
            DepthHistory = new Stack<int>();
            LevelSnapshots = new Dictionary<int, LevelStateSnapshot>();
        }
    }
}
