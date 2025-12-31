using System;

namespace Kobold.Extensions.Combat.Components
{
    /// <summary>
    /// Component for tracking entity health points.
    /// Can be used for players, enemies, destructible objects, etc.
    /// </summary>
    public struct HealthComponent
    {
        /// <summary>Current health points</summary>
        public int CurrentHealth { get; set; }

        /// <summary>Maximum health points</summary>
        public int MaxHealth { get; set; }

        /// <summary>Whether this entity is currently alive</summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// Invulnerability duration after taking damage (in seconds).
        /// Set to 0 for no i-frames.
        /// </summary>
        public float InvulnerabilityDuration { get; set; }

        /// <summary>Remaining invulnerability time</summary>
        public float InvulnerabilityTimer { get; set; }

        public HealthComponent(int maxHealth, float invulnerabilityDuration = 0f)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            InvulnerabilityDuration = invulnerabilityDuration;
            InvulnerabilityTimer = 0f;
        }

        /// <summary>
        /// Heal the entity by the specified amount, capped at MaxHealth
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        /// <returns>Actual amount healed</returns>
        public int Heal(int amount)
        {
            int oldHealth = CurrentHealth;
            CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
            return CurrentHealth - oldHealth;
        }

        /// <summary>
        /// Damage the entity by the specified amount.
        /// Returns actual damage dealt (0 if invulnerable).
        /// </summary>
        /// <param name="amount">Amount of damage</param>
        /// <returns>Actual damage dealt</returns>
        public int TakeDamage(int amount)
        {
            if (InvulnerabilityTimer > 0f)
                return 0;

            int oldHealth = CurrentHealth;
            CurrentHealth = Math.Max(CurrentHealth - amount, 0);

            // Start invulnerability timer
            if (InvulnerabilityDuration > 0f)
            {
                InvulnerabilityTimer = InvulnerabilityDuration;
            }

            return oldHealth - CurrentHealth;
        }
    }
}
