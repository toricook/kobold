using System;
using System.Collections.Generic;
using Kobold.Extensions.SaveSystem.Serializers;

namespace Kobold.Extensions.SaveSystem
{
    /// <summary>
    /// Registry for component serializers.
    /// Manages the mapping between component types and their serializers.
    /// Provides type-safe registration and lookup of serializers.
    /// </summary>
    public class ComponentSerializerRegistry
    {
        private readonly Dictionary<Type, object> _serializers = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a serializer for a specific component type.
        /// </summary>
        /// <typeparam name="TComponent">Component type to register</typeparam>
        /// <param name="serializer">Serializer implementation</param>
        /// <exception cref="InvalidOperationException">Thrown if a serializer is already registered for this type</exception>
        public void Register<TComponent>(IComponentSerializer<TComponent> serializer)
            where TComponent : struct
        {
            var componentType = typeof(TComponent);

            if (_serializers.ContainsKey(componentType))
            {
                throw new InvalidOperationException(
                    $"Serializer for {componentType.Name} is already registered. " +
                    $"Use Unregister() first if you need to replace it.");
            }

            _serializers[componentType] = serializer;
        }

        /// <summary>
        /// Registers a serializer using delegate functions.
        /// Convenience method for lambda-based registration.
        /// </summary>
        /// <typeparam name="TComponent">Component type</typeparam>
        /// <param name="serialize">Serialization function</param>
        /// <param name="deserialize">Deserialization function</param>
        public void Register<TComponent>(
            Func<TComponent, object> serialize,
            Func<object, TComponent> deserialize)
            where TComponent : struct
        {
            Register(new DelegateComponentSerializer<TComponent>(serialize, deserialize));
        }

        /// <summary>
        /// Gets the serializer for a specific component type.
        /// </summary>
        /// <typeparam name="TComponent">Component type</typeparam>
        /// <returns>Serializer instance, or null if not registered</returns>
        public IComponentSerializer<TComponent>? GetSerializer<TComponent>()
            where TComponent : struct
        {
            var componentType = typeof(TComponent);

            if (_serializers.TryGetValue(componentType, out var serializer))
            {
                return (IComponentSerializer<TComponent>)serializer;
            }

            return null;
        }

        /// <summary>
        /// Gets the serializer for a component type (non-generic version).
        /// Used by reflection-based serialization in WorldSerializer.
        /// </summary>
        /// <param name="componentType">Component type</param>
        /// <returns>Serializer object, or null if not registered</returns>
        public object? GetSerializer(Type componentType)
        {
            return _serializers.TryGetValue(componentType, out var serializer) ? serializer : null;
        }

        /// <summary>
        /// Checks if a serializer is registered for the given component type.
        /// </summary>
        /// <param name="componentType">Component type to check</param>
        /// <returns>True if a serializer is registered</returns>
        public bool HasSerializer(Type componentType)
        {
            return _serializers.ContainsKey(componentType);
        }

        /// <summary>
        /// Unregisters a serializer for a component type.
        /// Useful for replacing serializers or cleaning up.
        /// </summary>
        /// <typeparam name="TComponent">Component type</typeparam>
        /// <returns>True if a serializer was removed</returns>
        public bool Unregister<TComponent>() where TComponent : struct
        {
            return _serializers.Remove(typeof(TComponent));
        }

        /// <summary>
        /// Gets all registered component types.
        /// Useful for debugging or displaying serializer information.
        /// </summary>
        /// <returns>Collection of registered component types</returns>
        public IEnumerable<Type> GetRegisteredTypes()
        {
            return _serializers.Keys;
        }

        /// <summary>
        /// Gets the count of registered serializers.
        /// </summary>
        public int Count => _serializers.Count;

        /// <summary>
        /// Clears all registered serializers.
        /// </summary>
        public void Clear()
        {
            _serializers.Clear();
        }
    }
}
