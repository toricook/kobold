# Physics Configuration Guide

Configure physics simulation for different game types.

## Physics Profiles

### Space Game (Asteroids-style)

```csharp
var config = new PhysicsConfig
{
    EnableGravity = false,      // No gravity in space
    EnableDamping = true,
    GlobalDamping = 0.995f      // Minimal friction
};

Systems.AddSystem(new PhysicsSystem(config), 100, true);
```

**Characteristics:**
- Objects drift continuously
- Rotation-based thrust
- Inertia feels realistic

### Platformer

```csharp
var config = new PhysicsConfig
{
    EnableGravity = true,
    GlobalGravity = new Vector2(0, 500),  // Downward
    EnableDamping = true,
    GlobalDamping = 0.98f                 // Air resistance
};
```

**Characteristics:**
- Falling acceleration
- Jump mechanics
- Ground friction via damping

### Top-Down Game (RPG/Twin-stick shooter)

```csharp
var config = new PhysicsConfig
{
    EnableGravity = false,      // No vertical gravity
    EnableDamping = true,
    GlobalDamping = 0.85f       // High friction for responsive controls
};
```

**Characteristics:**
- Instant stop when releasing controls
- No vertical forces
- Responsive movement

### Racing Game

```csharp
var config = new PhysicsConfig
{
    EnableGravity = false,
    EnableDamping = true,
    GlobalDamping = 0.97f       // Moderate deceleration
};

// Per-entity max speed
ref var velocity = ref entity.Get<Velocity>();
velocity.ClampToMaxSpeed(400f);
```

## Per-Entity Physics

```csharp
// Heavy object
World.Create(
    new Transform(),
    new Velocity(),
    new Physics
    {
        Mass = 10.0f,           // Heavier = more gravity effect
        Restitution = 0.3f,     // Low bounce
        Damping = 0.95f         // Overrides global damping
    }
);

// Bouncy ball
World.Create(
    new Transform(),
    new Velocity(),
    new Physics
    {
        Mass = 1.0f,
        Restitution = 0.9f,     // Very bouncy
        Damping = 0.99f         // Little air resistance
    }
);
```

## See Also

- **[PhysicsSystem](../core/systems.md#physicssystem)** - System reference
- **[Physics Component](../core/components.md#physics)** - Component API

---

**Back:** [Guides](index.md) ←
