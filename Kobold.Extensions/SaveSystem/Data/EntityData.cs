using System.Collections.Generic;

namespace Kobold.Extensions.SaveSystem.Data
{
    /// <summary>
    /// Represents serialized entity data for saving.
    /// Contains the original entity ID and all of its serialized components.
    /// </summary>
    public class EntityData
    {
        /// <summary>
        /// Original entity ID from the ECS World.
        /// Used to maintain entity references and create ID mappings during load.
        /// Note: Entity IDs may change when loading, so references need remapping.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// List of all serialized components attached to this entity.
        /// Each ComponentData contains the type name and serialized state.
        /// </summary>
        public List<ComponentData> Components { get; set; } = new List<ComponentData>();
    }
}
