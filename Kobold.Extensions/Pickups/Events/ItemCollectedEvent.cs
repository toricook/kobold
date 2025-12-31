using Arch.Core;
using Kobold.Core.Events;

namespace Kobold.Extensions.Pickups
{
    /// <summary>
    /// Published when a pickup item is collected by an entity.
    /// Subscribe to this event to add visual effects, sounds, or other responses to item collection.
    /// </summary>
    public class ItemCollectedEvent : BaseEvent
    {
        /// <summary>The pickup entity that was collected</summary>
        public Entity PickupEntity { get; }

        /// <summary>The entity that collected the pickup</summary>
        public Entity CollectorEntity { get; }

        /// <summary>Optional tag identifying the pickup type (from PickupComponent.PickupTag)</summary>
        public string? PickupTag { get; }

        /// <summary>The effect that was applied</summary>
        public IPickupEffect Effect { get; }

        public ItemCollectedEvent(
            Entity pickupEntity,
            Entity collectorEntity,
            IPickupEffect effect,
            string? pickupTag = null)
        {
            PickupEntity = pickupEntity;
            CollectorEntity = collectorEntity;
            Effect = effect;
            PickupTag = pickupTag;
        }
    }
}
