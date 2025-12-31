# Collision Handling Guide

Set up collision detection, layers, and event handling.

## Basic Collision Setup

```csharp
// 1. Create collision matrix
var matrix = new CollisionMatrix();
matrix.SetCollision(CollisionLayer.Player, CollisionLayer.Enemy, true);
matrix.SetCollision(CollisionLayer.Projectile, CollisionLayer.Enemy, true);

// 2. Add collision system
Systems.AddSystem(new CollisionSystem(matrix), 300, true);

// 3. Subscribe to collision events
Events.Subscribe<CollisionEvent>(OnCollision);

// 4. Create entities with colliders
World.Create(
    new Transform(),
    BoxCollider.Square(32),
    new Tags { Values = new[] { "Player" } }
);
```

## Collision Layers

```csharp
public enum CollisionLayer
{
    Default,
    Player,
    Enemy,
    Projectile,
    Environment,
    PlayerProjectile,
    EnemyProjectile,
    Pickup,
    Trigger
}
```

## Collision Matrix Patterns

### Player vs World

```csharp
var matrix = new CollisionMatrix();
matrix.SetCollision(CollisionLayer.Player, CollisionLayer.Environment, true);
matrix.SetCollision(CollisionLayer.Player, CollisionLayer.Enemy, true);
matrix.SetCollision(CollisionLayer.Player, CollisionLayer.Pickup, true);
```

### Projectiles

```csharp
// Player projectiles hit enemies
matrix.SetCollision(CollisionLayer.PlayerProjectile, CollisionLayer.Enemy, true);

// Enemy projectiles hit player
matrix.SetCollision(CollisionLayer.EnemyProjectile, CollisionLayer.Player, true);

// Projectiles don't hit their own team
matrix.SetCollision(CollisionLayer.PlayerProjectile, CollisionLayer.Player, false);
matrix.SetCollision(CollisionLayer.EnemyProjectile, CollisionLayer.Enemy, false);
```

## Handling Collisions

```csharp
private void OnCollision(CollisionEvent collision)
{
    // Player hit enemy
    if (collision.LayerA == CollisionLayer.Player &&
        collision.LayerB == CollisionLayer.Enemy)
    {
        Events.Publish(new PlayerHitEvent(collision.EntityA));
    }

    // Projectile hit enemy
    if (collision.LayerA == CollisionLayer.Projectile &&
        collision.LayerB == CollisionLayer.Enemy)
    {
        World.Destroy(collision.EntityA);  // Destroy projectile
        World.Destroy(collision.EntityB);  // Destroy enemy
    }

    // Player collected pickup
    if (collision.LayerA == CollisionLayer.Player &&
        collision.LayerB == CollisionLayer.Pickup)
    {
        ApplyPickup(collision.EntityA, collision.EntityB);
        World.Destroy(collision.EntityB);
    }
}
```

## See Also

- **[CollisionSystem](../core/systems.md#collisionsystem)** - System API
- **[BoxCollider](../core/components.md#boxcollider)** - Collider component

---

**Back:** [Guides](index.md) ←
