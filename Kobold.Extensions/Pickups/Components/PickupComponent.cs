namespace Kobold.Extensions.Pickups
{
    /// <summary>
    /// Component marking an entity as a collectible pickup.
    /// Contains the effect that will be applied when the pickup is collected.
    ///
    /// Usage:
    /// - For automatic collection: Use with BoxCollider on CollisionLayer.Pickup
    /// - For interactive collection: Use with TriggerComponent and Trigger tag
    /// </summary>
    public struct PickupComponent
    {
        /// <summary>
        /// The effect that will be applied when this pickup is collected
        /// </summary>
        public IPickupEffect Effect { get; set; }

        /// <summary>
        /// Whether this pickup requires a button press to collect (true) or is automatic on contact (false)
        /// </summary>
        public bool RequiresInteraction { get; set; }

        /// <summary>
        /// Optional tag to identify the pickup type (e.g., "coin", "health", "weapon")
        /// </summary>
        public string? PickupTag { get; set; }

        public PickupComponent(IPickupEffect effect, bool requiresInteraction = false, string? pickupTag = null)
        {
            Effect = effect;
            RequiresInteraction = requiresInteraction;
            PickupTag = pickupTag;
        }
    }
}
