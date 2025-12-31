using System.Collections.Generic;

namespace Kobold.Extensions.SaveSystem.Data
{
    /// <summary>
    /// Root data structure for save files.
    /// Contains metadata and all serialized entities.
    /// This entire structure is serialized to JSON and compressed with GZip.
    /// </summary>
    public class SaveFileData
    {
        /// <summary>
        /// Save file metadata (timestamp, playtime, version, custom data, etc.)
        /// </summary>
        public SaveMetadata Metadata { get; set; } = new SaveMetadata();

        /// <summary>
        /// List of all entities in the saved world state.
        /// Each EntityData contains the entity ID and all its components.
        /// </summary>
        public List<EntityData> Entities { get; set; } = new List<EntityData>();
    }
}
