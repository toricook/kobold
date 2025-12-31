namespace Kobold.Extensions.Combat.Components
{
    /// <summary>
    /// Component for simple AI behavior - chase and attack targets.
    /// Configurable parameters for detection range, movement speed, and attack behavior.
    /// </summary>
    public struct SimpleAIComponent
    {
        /// <summary>How close the target must be before the AI starts chasing (in pixels)</summary>
        public float DetectionRange { get; set; }

        /// <summary>How fast the AI moves when chasing (pixels per second)</summary>
        public float MoveSpeed { get; set; }

        /// <summary>How close the AI needs to be to attack (in pixels)</summary>
        public float AttackRange { get; set; }

        /// <summary>How much damage the AI deals per attack</summary>
        public int AttackDamage { get; set; }

        /// <summary>Time between attacks (in seconds)</summary>
        public float AttackCooldown { get; set; }

        /// <summary>Whether this AI is currently chasing a target</summary>
        public bool IsChasing { get; set; }

        /// <summary>Whether this AI is currently in attack range</summary>
        public bool IsInAttackRange { get; set; }

        public SimpleAIComponent(
            float detectionRange = 200f,
            float moveSpeed = 80f,
            float attackRange = 32f,
            int attackDamage = 10,
            float attackCooldown = 1.0f)
        {
            DetectionRange = detectionRange;
            MoveSpeed = moveSpeed;
            AttackRange = attackRange;
            AttackDamage = attackDamage;
            AttackCooldown = attackCooldown;
            IsChasing = false;
            IsInAttackRange = false;
        }
    }
}
