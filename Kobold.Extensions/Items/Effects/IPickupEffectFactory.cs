using System;
using System.Collections.Generic;
using Kobold.Extensions.Pickups;

namespace Kobold.Extensions.Items.Effects
{
    /// <summary>
    /// Factory interface for creating IPickupEffect instances from JSON data.
    /// Game projects implement this to map effect type names to concrete implementations.
    /// </summary>
    public interface IPickupEffectFactory
    {
        /// <summary>
        /// Create a pickup effect by type name with parameters from JSON
        /// </summary>
        /// <param name="effectType">Effect type name (e.g., "AddCoins", "HealPlayer")</param>
        /// <param name="parameters">Parameters from JSON effectParams dictionary</param>
        /// <returns>Instantiated IPickupEffect</returns>
        /// <exception cref="InvalidOperationException">If effect type is not registered</exception>
        IPickupEffect CreateEffect(string effectType, Dictionary<string, object> parameters);

        /// <summary>
        /// Register a custom effect type with a factory function
        /// </summary>
        /// <param name="effectType">Effect type name to register</param>
        /// <param name="factory">Function that creates the effect from parameters</param>
        void RegisterEffectType(string effectType, Func<Dictionary<string, object>, IPickupEffect> factory);

        /// <summary>
        /// Check if an effect type is registered
        /// </summary>
        /// <param name="effectType">Effect type name to check</param>
        /// <returns>True if the effect type is registered</returns>
        bool HasEffectType(string effectType);
    }
}
