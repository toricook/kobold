using Arch.Core;

namespace Kobold.Extensions.Pickups
{
    /// <summary>
    /// Defines the effect that occurs when a pickup item is collected.
    /// Implement this interface to create custom pickup behaviors (coins, health, powerups, etc.)
    /// </summary>
    public interface IPickupEffect
    {
        /// <summary>
        /// Executes the pickup effect when an entity collects the item.
        /// </summary>
        /// <param name="world">The ECS world</param>
        /// <param name="pickupEntity">The entity being picked up</param>
        /// <param name="collectorEntity">The entity collecting the pickup</param>
        void Apply(World world, Entity pickupEntity, Entity collectorEntity);

        /// <summary>
        /// Optional: Returns a description of this pickup for UI/logging purposes.
        /// </summary>
        string GetDescription() => "Pickup";

        /// <summary>
        /// Indicates whether this pickup type should stack in inventory.
        /// Stackable items are counted by quantity (e.g., coins, potions).
        /// Non-stackable items are tracked individually (e.g., unique weapons, equipment).
        /// </summary>
        bool IsStackable { get; }
    }
}
