using System;
using System.Collections.Generic;
using System.Text.Json;
using Kobold.Extensions.Pickups;

namespace Kobold.Extensions.Items.Effects
{
    /// <summary>
    /// Default implementation of IPickupEffectFactory.
    /// Game projects extend this class to register their custom pickup effects.
    /// </summary>
    public class PickupEffectFactory : IPickupEffectFactory
    {
        private readonly Dictionary<string, Func<Dictionary<string, object>, IPickupEffect>> _effectFactories =
            new Dictionary<string, Func<Dictionary<string, object>, IPickupEffect>>();

        /// <summary>
        /// Register a custom effect type with a factory function
        /// </summary>
        /// <param name="effectType">Effect type name (case-sensitive)</param>
        /// <param name="factory">Function that creates the effect from parameters</param>
        public void RegisterEffectType(string effectType, Func<Dictionary<string, object>, IPickupEffect> factory)
        {
            if (string.IsNullOrEmpty(effectType))
                throw new ArgumentException("Effect type cannot be null or empty", nameof(effectType));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _effectFactories[effectType] = factory;
        }

        /// <summary>
        /// Create a pickup effect by type name with parameters
        /// </summary>
        /// <param name="effectType">Effect type name</param>
        /// <param name="parameters">Parameters from JSON</param>
        /// <returns>Instantiated IPickupEffect</returns>
        /// <exception cref="InvalidOperationException">If effect type is not registered</exception>
        public IPickupEffect CreateEffect(string effectType, Dictionary<string, object> parameters)
        {
            if (!_effectFactories.TryGetValue(effectType, out var factory))
            {
                throw new InvalidOperationException(
                    $"Unknown effect type: '{effectType}'. Did you register it in your PickupEffectFactory subclass? " +
                    $"Available types: {string.Join(", ", _effectFactories.Keys)}");
            }

            return factory(parameters ?? new Dictionary<string, object>());
        }

        /// <summary>
        /// Check if an effect type is registered
        /// </summary>
        /// <param name="effectType">Effect type name to check</param>
        /// <returns>True if the effect type is registered</returns>
        public bool HasEffectType(string effectType)
        {
            return _effectFactories.ContainsKey(effectType);
        }

        /// <summary>
        /// Helper to get parameter value from JSON dictionary with type conversion.
        /// Handles JsonElement conversion from System.Text.Json deserialization.
        /// </summary>
        /// <typeparam name="T">Type to convert parameter to</typeparam>
        /// <param name="parameters">Parameter dictionary from JSON</param>
        /// <param name="key">Parameter key to look up</param>
        /// <param name="defaultValue">Default value if key not found</param>
        /// <returns>Parameter value as type T</returns>
        protected T GetParam<T>(Dictionary<string, object> parameters, string key, T defaultValue = default)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            if (value == null)
                return defaultValue;

            // Handle JsonElement conversion (from System.Text.Json)
            if (value is JsonElement jsonElement)
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                }
                catch (JsonException)
                {
                    // Fallback to default if deserialization fails
                    return defaultValue;
                }
            }

            // Direct type conversion for simple types
            try
            {
                if (value is T typedValue)
                    return typedValue;

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (InvalidCastException)
            {
                Console.WriteLine($"Warning: Failed to convert parameter '{key}' to type {typeof(T).Name}. Using default value.");
                return defaultValue;
            }
        }

        /// <summary>
        /// Get count of registered effect types
        /// </summary>
        public int RegisteredEffectCount => _effectFactories.Count;
    }
}
