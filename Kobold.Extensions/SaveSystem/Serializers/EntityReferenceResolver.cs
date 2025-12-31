using System.Collections.Generic;

namespace Kobold.Extensions.SaveSystem.Serializers
{
    /// <summary>
    /// Handles entity ID mapping and reference resolution during save/load.
    /// When loading a save, entity IDs may change. This class maintains the mapping
    /// from old IDs (in the save file) to new IDs (newly created entities).
    /// Components that reference other entities can use this mapping to restore references.
    /// </summary>
    public class EntityReferenceResolver
    {
        private readonly Dictionary<int, int> _idMapping = new Dictionary<int, int>();

        /// <summary>
        /// Maps an old entity ID (from save file) to a new entity ID (newly created).
        /// </summary>
        /// <param name="oldId">Entity ID from the save file</param>
        /// <param name="newId">New entity ID after creation</param>
        public void MapEntityId(int oldId, int newId)
        {
            _idMapping[oldId] = newId;
        }

        /// <summary>
        /// Resolves an old entity ID to the corresponding new entity ID.
        /// </summary>
        /// <param name="oldId">Entity ID from the save file</param>
        /// <param name="newId">Resolved new entity ID (output parameter)</param>
        /// <returns>True if the mapping exists, false otherwise</returns>
        public bool TryResolve(int oldId, out int newId)
        {
            return _idMapping.TryGetValue(oldId, out newId);
        }

        /// <summary>
        /// Gets the new entity ID for an old ID.
        /// </summary>
        /// <param name="oldId">Old entity ID</param>
        /// <returns>New entity ID, or -1 if not found</returns>
        public int Resolve(int oldId)
        {
            return _idMapping.TryGetValue(oldId, out var newId) ? newId : -1;
        }

        /// <summary>
        /// Checks if a mapping exists for the given old ID.
        /// </summary>
        public bool HasMapping(int oldId)
        {
            return _idMapping.ContainsKey(oldId);
        }

        /// <summary>
        /// Clears all ID mappings.
        /// Should be called before starting a new load operation.
        /// </summary>
        public void Clear()
        {
            _idMapping.Clear();
        }

        /// <summary>
        /// Gets the count of ID mappings.
        /// </summary>
        public int Count => _idMapping.Count;
    }
}
