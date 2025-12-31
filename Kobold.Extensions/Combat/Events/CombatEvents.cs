using System.Numerics;
using Arch.Core;
using Kobold.Core.Events;

namespace Kobold.Extensions.Combat.Events
{
    /// <summary>
    /// Published when an entity requests to perform an attack.
    /// Subscribed by MeleeAttackSystem to process the attack.
    /// </summary>
    public class AttackRequestedEvent : BaseEvent
    {
        public Entity AttackerEntity { get; }
        public Vector2 AttackPosition { get; }
        public float AttackRadius { get; }
        public int Damage { get; }
        public string? TargetTag { get; }
        public bool CanDamageSelf { get; }

        public AttackRequestedEvent(
            Entity attackerEntity,
            Vector2 attackPosition,
            float attackRadius,
            int damage,
            string? targetTag,
            bool canDamageSelf)
        {
            AttackerEntity = attackerEntity;
            AttackPosition = attackPosition;
            AttackRadius = attackRadius;
            Damage = damage;
            TargetTag = targetTag;
            CanDamageSelf = canDamageSelf;
        }
    }

    /// <summary>
    /// Published after an attack has been processed (regardless of hits).
    /// Subscribe to trigger visual/audio effects.
    /// </summary>
    public class AttackPerformedEvent : BaseEvent
    {
        public Entity AttackerEntity { get; }
        public Vector2 AttackPosition { get; }
        public float AttackRadius { get; }
        public int TargetsHit { get; }

        public AttackPerformedEvent(
            Entity attackerEntity,
            Vector2 attackPosition,
            float attackRadius,
            int targetsHit)
        {
            AttackerEntity = attackerEntity;
            AttackPosition = attackPosition;
            AttackRadius = attackRadius;
            TargetsHit = targetsHit;
        }
    }

    /// <summary>
    /// Published when damage is dealt to an entity.
    /// Subscribe for damage numbers, hit effects, screen shake, etc.
    /// </summary>
    public class DamageDealtEvent : BaseEvent
    {
        public Entity AttackerEntity { get; }
        public Entity TargetEntity { get; }
        public int DamageAmount { get; }
        public Vector2 HitPosition { get; }

        public DamageDealtEvent(
            Entity attackerEntity,
            Entity targetEntity,
            int damageAmount,
            Vector2 hitPosition)
        {
            AttackerEntity = attackerEntity;
            TargetEntity = targetEntity;
            DamageAmount = damageAmount;
            HitPosition = hitPosition;
        }
    }

    /// <summary>
    /// Published when an entity's health reaches zero due to combat.
    /// Subscribe for death effects, scoring, loot drops, etc.
    /// </summary>
    public class EntityKilledEvent : BaseEvent
    {
        public Entity KillerEntity { get; }
        public Entity KilledEntity { get; }
        public Vector2 DeathPosition { get; }

        public EntityKilledEvent(
            Entity killerEntity,
            Entity killedEntity,
            Vector2 deathPosition)
        {
            KillerEntity = killerEntity;
            KilledEntity = killedEntity;
            DeathPosition = deathPosition;
        }
    }
}
