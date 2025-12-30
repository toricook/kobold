---
layout: default
title: Event Bus
parent: Core Framework
nav_order: 6
---

# Event Bus - Decoupled Event System

The `EventBus` provides a pub/sub (publish/subscribe) event system for decoupled communication between game systems. It's accessible via `GameEngineBase.Events`.

## Overview

EventBus allows systems to communicate without direct references:
- **Publishers** raise events when something happens
- **Subscribers** listen for and react to events
- Type-safe event handling
- Support for both interface-based and Action-based handlers

## Basic Usage

### 1. Define an Event

Events can be any type - classes, structs, or records:

```csharp
// Record (recommended)
public record PlayerDiedEvent(Entity Player, string Cause);

// Class
public class ScoreChangedEvent
{
    public int NewScore { get; init; }
    public int Delta { get; init; }
}

// Struct
public struct LevelCompletedEvent
{
    public int LevelNumber;
    public float CompletionTime;
}
```

### 2. Subscribe to Events

Subscribe in your game's `Initialize()`:

```csharp
protected override void Initialize()
{
    base.Initialize();

    // Subscribe with Action (lambda)
    Events.Subscribe<PlayerDiedEvent>(e =>
    {
        Console.WriteLine($"Player died from {e.Cause}");
    });

    // Subscribe with method reference
    Events.Subscribe<ScoreChangedEvent>(OnScoreChanged);
}

private void OnScoreChanged(ScoreChangedEvent e)
{
    Console.WriteLine($"Score: {e.NewScore} (+{e.Delta})");
}
```

### 3. Publish Events

Publish events when something happens:

```csharp
// In a collision handler
private void OnCollision(CollisionEvent collision)
{
    if (collision.LayerA == CollisionLayer.Player &&
        collision.LayerB == CollisionLayer.Enemy)
    {
        Events.Publish(new PlayerDiedEvent(collision.EntityA, "Enemy collision"));
    }
}

// When score changes
private void AddScore(int points)
{
    _currentScore += points;
    Events.Publish(new ScoreChangedEvent
    {
        NewScore = _currentScore,
        Delta = points
    });
}
```

## API Reference

### Subscribe

Subscribe to events with an Action:

```csharp
void Subscribe<T>(Action<T> handler) where T : notnull
```

**Example:**
```csharp
Events.Subscribe<PlayerDiedEvent>(e =>
{
    // Handle player death
    ShowGameOverScreen();
});
```

Subscribe with an IEventHandler:

```csharp
void Subscribe<T>(IEventHandler<T> handler) where T : notnull
```

**Example:**
```csharp
public class ScoreDisplay : IEventHandler<ScoreChangedEvent>
{
    public void Handle(ScoreChangedEvent eventData)
    {
        UpdateScoreUI(eventData.NewScore);
    }
}

// Subscribe
Events.Subscribe(new ScoreDisplay());
```

### Publish

Publish an event to all subscribers:

```csharp
void Publish<T>(T eventData) where T : notnull
```

**Example:**
```csharp
Events.Publish(new LevelCompletedEvent
{
    LevelNumber = 1,
    CompletionTime = 45.2f
});
```

### Unsubscribe

Remove a handler:

```csharp
void Unsubscribe<T>(Action<T> handler) where T : notnull
void Unsubscribe<T>(IEventHandler<T> handler) where T : notnull
```

**Example:**
```csharp
Action<PlayerDiedEvent> handler = e => { /* ... */ };
Events.Subscribe(handler);

// Later...
Events.Unsubscribe(handler);
```

### Clear

Remove all subscribers:

```csharp
void Clear()
```

## Built-in Events

### CollisionEvent

Published by `CollisionSystem` when entities collide:

```csharp
public record CollisionEvent
{
    public Entity EntityA;
    public Entity EntityB;
    public Vector2 CollisionPoint;
    public Vector2 Normal;
    public float Penetration;
    public CollisionLayer LayerA;
    public CollisionLayer LayerB;
}
```

**Example:**
```csharp
Events.Subscribe<CollisionEvent>(collision =>
{
    if (collision.LayerA == CollisionLayer.Projectile &&
        collision.LayerB == CollisionLayer.Enemy)
    {
        // Projectile hit enemy
        World.Destroy(collision.EntityA);
        World.Destroy(collision.EntityB);
        Events.Publish(new EnemyKilledEvent());
    }
});
```

## Patterns and Examples

### Game State Events

```csharp
public record GameStateChangedEvent(GameStateType OldState, GameStateType NewState);

// Publish when state changes
Events.Publish(new GameStateChangedEvent(GameStateType.Playing, GameStateType.Paused));

// Subscribe to handle pausing
Events.Subscribe<GameStateChangedEvent>(e =>
{
    if (e.NewState == GameStateType.Paused)
    {
        // Show pause menu
    }
});
```

### Achievement System

```csharp
public record AchievementUnlockedEvent(string AchievementId, string Name);

Events.Subscribe<EnemyKilledEvent>(e =>
{
    _enemiesKilled++;
    if (_enemiesKilled >= 100)
    {
        Events.Publish(new AchievementUnlockedEvent("killer", "Kill 100 Enemies"));
    }
});

Events.Subscribe<AchievementUnlockedEvent>(e =>
{
    ShowNotification($"Achievement Unlocked: {e.Name}");
});
```

### Power-up System

```csharp
public record PowerUpCollectedEvent(PowerUpType Type, float Duration);

Events.Subscribe<CollisionEvent>(collision =>
{
    if (World.Has<PowerUp>(collision.EntityA) &&
        World.Has<PlayerControlled>(collision.EntityB))
    {
        var powerUp = World.Get<PowerUp>(collision.EntityA);
        Events.Publish(new PowerUpCollectedEvent(powerUp.Type, powerUp.Duration));
        World.Destroy(collision.EntityA);
    }
});

Events.Subscribe<PowerUpCollectedEvent>(e =>
{
    ApplyPowerUpToPlayer(e.Type, e.Duration);
    ShowPowerUpUI(e.Type, e.Duration);
});
```

## Best Practices

### 1. Use Records for Events

Records provide value equality and clean syntax:

```csharp
// Good
public record PlayerScored(int Points, Entity Player);

// Less good
public class PlayerScored
{
    public int Points { get; set; }
    public Entity Player { get; set; }
}
```

### 2. Unsubscribe When Done

```csharp
// One-shot event handler
Action<BossDefeatedEvent> handler = null;
handler = e =>
{
    ShowVictoryScreen();
    Events.Unsubscribe(handler);  // Unsubscribe after first call
};
Events.Subscribe(handler);
```

### 3. Don't Overuse Events

Events are great for cross-cutting concerns, but don't use them for everything:

**Good use cases:**
- UI updates in response to game events
- Achievement/stat tracking
- Audio triggers
- Cross-system notifications

**Bad use cases:**
- Core game loop logic (use systems instead)
- Tight coupling between systems (use direct queries)
- Performance-critical paths (events have overhead)

### 4. Event Naming

Use past tense for events that have happened:

```csharp
// Good
PlayerDiedEvent
ScoreChangedEvent
LevelCompletedEvent

// Not as clear
PlayerDieEvent
ScoreChangeEvent
LevelCompleteEvent
```

### 5. Hierarchical Events

You can use inheritance for event filtering:

```csharp
public record GameEvent;  // Base event
public record PlayerEvent : GameEvent;
public record EnemyEvent : GameEvent;

public record PlayerDiedEvent : PlayerEvent;
public record PlayerScoredEvent : PlayerEvent;

// Subscribe to all player events
Events.Subscribe<PlayerEvent>(e =>
{
    Console.WriteLine($"Player event: {e.GetType().Name}");
});
```

## Complete Example

```csharp
public class SpaceShooter : GameEngineBase
{
    private int _score = 0;
    private int _lives = 3;

    protected override void Initialize()
    {
        base.Initialize();

        // Subscribe to game events
        Events.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
        Events.Subscribe<PlayerHitEvent>(OnPlayerHit);
        Events.Subscribe<CollisionEvent>(OnCollision);

        // Create systems
        Systems.AddSystem(new CollisionSystem(CreateCollisionMatrix()), 300, true);

        // Create player
        World.Create(
            new Transform { Position = new Vector2(400, 500) },
            SpriteRenderer.FullTexture(Assets.LoadTexture("player.png")),
            new PlayerControlled(),
            BoxCollider.Square(32)
        );
    }

    private void OnCollision(CollisionEvent collision)
    {
        if (collision.LayerA == CollisionLayer.Player &&
            collision.LayerB == CollisionLayer.Enemy)
        {
            Events.Publish(new PlayerHitEvent(collision.EntityA));
        }

        if (collision.LayerA == CollisionLayer.Projectile &&
            collision.LayerB == CollisionLayer.Enemy)
        {
            World.Destroy(collision.EntityA);  // Destroy projectile
            World.Destroy(collision.EntityB);  // Destroy enemy
            Events.Publish(new EnemyDestroyedEvent(collision.EntityB));
        }
    }

    private void OnEnemyDestroyed(EnemyDestroyedEvent e)
    {
        _score += 100;
        Events.Publish(new ScoreChangedEvent(_score, 100));

        if (_score >= 1000)
        {
            Events.Publish(new AchievementUnlockedEvent("score_master", "Reach 1000 points"));
        }
    }

    private void OnPlayerHit(PlayerHitEvent e)
    {
        _lives--;
        Events.Publish(new LivesChangedEvent(_lives));

        if (_lives <= 0)
        {
            Events.Publish(new GameOverEvent(_score));
        }
    }
}

// Event definitions
public record EnemyDestroyedEvent(Entity Enemy);
public record PlayerHitEvent(Entity Player);
public record ScoreChangedEvent(int NewScore, int Delta);
public record LivesChangedEvent(int Lives);
public record AchievementUnlockedEvent(string Id, string Name);
public record GameOverEvent(int FinalScore);
```

## See Also

- **[Architecture](architecture.md)** - Event-driven design in ECS
- **[Systems](systems.md)** - Using events in systems
- **[Collision System](systems.md#collisionsystem)** - CollisionEvent details

---

**Next:** Learn about [Utilities](utilities.md) →
