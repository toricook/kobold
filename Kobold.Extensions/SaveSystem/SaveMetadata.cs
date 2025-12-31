using System;
using System.Collections.Generic;

namespace Kobold.Extensions.SaveSystem
{
    /// <summary>
    /// Metadata about a save file.
    /// Contains timestamp, playtime, version information, and custom game-specific data.
    /// This can be serialized separately for quick save slot displays without loading the full save.
    /// </summary>
    public class SaveMetadata
    {
        /// <summary>
        /// When the save was created (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Total playtime in seconds when this save was created.
        /// Tracked by AutoSaveSystem or manually provided.
        /// </summary>
        public float Playtime { get; set; }

        /// <summary>
        /// Game version that created this save (e.g., "1.0.0").
        /// Used for compatibility checks and migration logic.
        /// </summary>
        public string GameVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Save system version (e.g., "1.0.0").
        /// Used to handle save format changes in future updates.
        /// </summary>
        public string SaveSystemVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Name of the save slot (e.g., "save_1", "autosave").
        /// </summary>
        public string SlotName { get; set; } = string.Empty;

        /// <summary>
        /// Custom game-specific metadata.
        /// Examples: player name, current level, difficulty, completion percentage, etc.
        /// Useful for displaying rich save slot information in the UI.
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Creates default metadata for a new save.
        /// </summary>
        public SaveMetadata()
        {
            Timestamp = DateTime.UtcNow;
            Playtime = 0f;
        }

        /// <summary>
        /// Creates metadata for a save with specific values.
        /// </summary>
        /// <param name="slotName">Save slot name</param>
        /// <param name="playtime">Total playtime in seconds</param>
        /// <param name="customData">Optional custom metadata</param>
        /// <returns>SaveMetadata instance</returns>
        public static SaveMetadata Create(string slotName, float playtime, Dictionary<string, string>? customData = null)
        {
            return new SaveMetadata
            {
                Timestamp = DateTime.UtcNow,
                Playtime = playtime,
                SlotName = slotName,
                CustomData = customData ?? new Dictionary<string, string>()
            };
        }
    }
}
