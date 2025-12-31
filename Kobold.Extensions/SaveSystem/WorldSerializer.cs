using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using Kobold.Extensions.SaveSystem.Data;
using Kobold.Extensions.SaveSystem.Serializers;

namespace Kobold.Extensions.SaveSystem
{
    /// <summary>
    /// Handles serialization and deserialization of the entire ECS world state.
    /// Uses the component serializer registry to convert entities and their components
    /// to/from a save-friendly format.
    /// </summary>
    public class WorldSerializer
    {
        private readonly World _world;
        private readonly ComponentSerializerRegistry _registry;
        private readonly EntityReferenceResolver _referenceResolver;

        public WorldSerializer(World world, ComponentSerializerRegistry registry)
        {
            _world = world;
            _registry = registry;
            _referenceResolver = new EntityReferenceResolver();
        }

        /// <summary>
        /// Serializes all entities in the world to save data.
        /// Only components with registered serializers are saved.
        /// </summary>
        /// <param name="metadata">Metadata for this save</param>
        /// <returns>Complete save file data</returns>
        public SaveFileData SerializeWorld(SaveMetadata metadata)
        {
            var saveData = new SaveFileData
            {
                Metadata = metadata,
                Entities = new List<Data.EntityData>()
            };

            // Query ALL entities (no filter - automatic save of everything)
            var query = new QueryDescription();

            _world.Query(in query, (Entity entity) =>
            {
                var entityData = SerializeEntity(entity);
                if (entityData != null && entityData.Components.Count > 0)
                {
                    saveData.Entities.Add(entityData);
                }
            });

            return saveData;
        }

        /// <summary>
        /// Deserializes save data and reconstructs the world state.
        /// Uses two-phase loading: first creates entities, then resolves references.
        /// </summary>
        /// <param name="saveData">Save data to load</param>
        public void DeserializeWorld(SaveFileData saveData)
        {
            // Clear reference resolver for new load operation
            _referenceResolver.Clear();

            // Phase 1: Create all entities and restore components
            foreach (var entityData in saveData.Entities)
            {
                var newEntity = DeserializeEntity(entityData);
                _referenceResolver.MapEntityId(entityData.Id, newEntity.Id);
            }

            // Phase 2: Resolve entity references (if any components need it)
            // Note: This is a hook for future entity reference support
            // For now, most components don't reference other entities
        }

        /// <summary>
        /// Serializes a single entity and all its registered components.
        /// </summary>
        private Data.EntityData? SerializeEntity(Entity entity)
        {
            var entityData = new Data.EntityData
            {
                Id = entity.Id,
                Components = new List<ComponentData>()
            };

            // Iterate through all registered component types
            foreach (var componentType in _registry.GetRegisteredTypes())
            {
                // Check if entity has this component using Arch's Has<T> method
                if (HasComponent(entity, componentType))
                {
                    var componentData = SerializeComponent(entity, componentType);
                    if (componentData != null)
                    {
                        entityData.Components.Add(componentData);
                    }
                }
            }

            return entityData.Components.Count > 0 ? entityData : null;
        }

        /// <summary>
        /// Serializes a specific component on an entity.
        /// </summary>
        private ComponentData? SerializeComponent(Entity entity, Type componentType)
        {
            try
            {
                // Get the serializer for this component type
                var serializer = _registry.GetSerializer(componentType);
                if (serializer == null) return null;

                // Get the component from the entity using reflection
                var getMethod = typeof(Arch.Core.Extensions.EntityExtensions)
                    .GetMethod("Get", BindingFlags.Public | BindingFlags.Static)
                    ?.MakeGenericMethod(componentType);

                if (getMethod == null) return null;

                // Get returns ref, so we need to handle it carefully
                object?[] parameters = new object?[] { entity };
                var component = getMethod.Invoke(null, parameters);

                // Get the Serialize method from the serializer
                var serializeMethod = serializer.GetType().GetMethod("Serialize");
                if (serializeMethod == null) return null;

                // Serialize the component
                var data = serializeMethod.Invoke(serializer, new[] { component });

                return new ComponentData
                {
                    TypeName = componentType.AssemblyQualifiedName ?? componentType.FullName ?? componentType.Name,
                    Data = data
                };
            }
            catch (Exception)
            {
                // If serialization fails, skip this component
                return null;
            }
        }

        /// <summary>
        /// Deserializes entity data and creates a new entity with components.
        /// </summary>
        private Entity DeserializeEntity(Data.EntityData entityData)
        {
            var entity = _world.Create();

            foreach (var componentData in entityData.Components)
            {
                DeserializeComponent(entity, componentData);
            }

            return entity;
        }

        /// <summary>
        /// Deserializes a component and adds it to an entity.
        /// </summary>
        private void DeserializeComponent(Entity entity, ComponentData componentData)
        {
            try
            {
                var componentType = Type.GetType(componentData.TypeName);
                if (componentType == null) return;

                // Get the serializer
                var serializer = _registry.GetSerializer(componentType);
                if (serializer == null) return;

                // Deserialize the component
                var deserializeMethod = serializer.GetType().GetMethod("Deserialize");
                if (deserializeMethod == null) return;

                var component = deserializeMethod.Invoke(serializer, new[] { componentData.Data });
                if (component == null) return;

                // Add component to entity using reflection
                var addMethod = typeof(Arch.Core.Extensions.EntityExtensions)
                    .GetMethod("Add", BindingFlags.Public | BindingFlags.Static)
                    ?.MakeGenericMethod(componentType);

                if (addMethod == null) return;

                object?[] addParams = new object?[] { entity, component };
                addMethod.Invoke(null, addParams);
            }
            catch (Exception)
            {
                // If deserialization fails, skip this component
            }
        }

        /// <summary>
        /// Checks if an entity has a specific component type.
        /// </summary>
        private bool HasComponent(Entity entity, Type componentType)
        {
            try
            {
                var hasMethod = typeof(Arch.Core.Extensions.EntityExtensions)
                    .GetMethod("Has", BindingFlags.Public | BindingFlags.Static)
                    ?.MakeGenericMethod(componentType);

                if (hasMethod == null) return false;

                object?[] hasParams = new object?[] { entity };
                var result = hasMethod.Invoke(null, hasParams);
                return result is bool hasIt && hasIt;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the entity reference resolver for manual entity reference handling.
        /// </summary>
        public EntityReferenceResolver GetReferenceResolver() => _referenceResolver;
    }
}
