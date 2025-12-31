using Kobold.Core.Abstractions.Input;

namespace Kobold.Extensions.Combat.Components
{
    /// <summary>
    /// Component defining a melee weapon with area-of-effect damage.
    /// Entities with this component can perform melee attacks in a radius around them.
    /// </summary>
    public struct MeleeWeaponComponent
    {
        /// <summary>Amount of damage dealt per attack</summary>
        public int Damage { get; set; }

        /// <summary>Attack radius in pixels (damage area around the entity)</summary>
        public float AttackRadius { get; set; }

        /// <summary>Cooldown duration between attacks in seconds</summary>
        public float CooldownDuration { get; set; }

        /// <summary>Which input key triggers this weapon (default: Space)</summary>
        public KeyCode AttackKey { get; set; }

        /// <summary>
        /// Which entities can be damaged by this weapon.
        /// Examples: "Enemy", "Destructible", "All"
        /// Leave null to damage anything with HealthComponent.
        /// </summary>
        public string? TargetTag { get; set; }

        /// <summary>Can this weapon damage the entity that owns it?</summary>
        public bool CanDamageSelf { get; set; }

        public MeleeWeaponComponent(
            int damage,
            float attackRadius,
            float cooldownDuration = 0.5f,
            KeyCode attackKey = KeyCode.Space,
            string? targetTag = null,
            bool canDamageSelf = false)
        {
            Damage = damage;
            AttackRadius = attackRadius;
            CooldownDuration = cooldownDuration;
            AttackKey = attackKey;
            TargetTag = targetTag;
            CanDamageSelf = canDamageSelf;
        }
    }
}
