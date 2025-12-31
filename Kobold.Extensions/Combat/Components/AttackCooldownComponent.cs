namespace Kobold.Extensions.Combat.Components
{
    /// <summary>
    /// Component tracking the cooldown state of an entity's attack.
    /// Automatically managed by AttackCooldownSystem.
    /// </summary>
    public struct AttackCooldownComponent
    {
        /// <summary>Remaining time until the entity can attack again (in seconds)</summary>
        public float RemainingCooldown { get; set; }

        /// <summary>Is the attack currently on cooldown?</summary>
        public bool IsOnCooldown => RemainingCooldown > 0f;

        public AttackCooldownComponent(float remainingCooldown = 0f)
        {
            RemainingCooldown = remainingCooldown;
        }
    }
}
