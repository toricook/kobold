# Kobold Architecture - Entity Component System

Kobold is built on the **Entity Component System (ECS)** architectural pattern, providing high-performance, data-oriented game development. This guide explains what ECS is, why Kobold uses it, and how it works.

## What is Entity Component System?

ECS is an architectural pattern that separates data (Components) from logic (Systems) and treats game objects as compositions of data (Entities).

### The Three Pillars

**Entities** - Unique identifiers for game objects
- Just an ID number (not a class or object)
- Created with `World.Create()`
- Lightweight and fast to create/destroy

**Components** - Data containers (structs)
- Pure data, no logic
- Examples: `Transform`, `Velocity`, `SpriteRenderer`
- Attached to entities to give them properties

**Systems** - Logic processors
- Contain all game logic
- Query entities with specific components
- Process matched entities each frame

### Traditional OOP vs ECS

**Traditional Object-Oriented:**
```csharp
class Player : GameObject
{
    Vector2 position;
    Sprite sprite;
    int health;

    void Update() { /* update logic */ }
    void Render() { /* render logic */ }
}

class Enemy : GameObject
{
    // Duplicate code from Player...
}
```

Problems:
- Rigid inheritance hierarchies
- Code duplication
- Poor cache locality
- Difficult to extend

**Entity Component System:**
```csharp
// Components (data only)
struct Transform { Vector2 Position; }
struct Health { int Value; }
struct SpriteRenderer { ITexture Texture; }

// Entities (just IDs with components)
var player = World.Create(
    new Transform(), new Health(), new SpriteRenderer()
);

var enemy = World.Create(
    new Transform(), new Health(), new SpriteRenderer(), new AI()
);

// Systems (logic only)
class MovementSystem
{
    void Update()
    {
        // Process all entities with Transform + Velocity
        var query = World.Query<Transform, Velocity>();
        foreach (var entity in query) { /* move */ }
    }
}
```

Benefits:
- Composition over inheritance
- No code duplication
- Excellent cache performance
- Easy to add/remove behaviors

## Kobold's ECS Implementation

Kobold uses the [Arch](https://github.com/genaray/Arch) library, a high-performance ECS implementation for C#.

### The World

The `World` is the ECS container that holds all entities and components. Access it via `GameEngineBase.World`:

```csharp
public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // World is automatically created by GameEngineBase
        var entity = World.Create(new Transform(), new Velocity());
    }
}
```

### Creating Entities

Entities are created with `World.Create()` and one or more components:

```csharp
// Entity with single component
var entity1 = World.Create(new Transform { Position = new Vector2(100, 100) });

// Entity with multiple components
var entity2 = World.Create(
    new Transform { Position = new Vector2(200, 200) },
    new Velocity { Value = new Vector2(50, 0) },
    SpriteRenderer.FullTexture(texture),
    BoxCollider.Square(32)
);
```

The return value is an `Entity` struct (from Arch) which you can store if you need to reference it later.

### Components

Components are structs (value types) that store data. Kobold provides many built-in components:

```csharp
// Core components
public struct Transform
{
    public Vector2 Position;
    public float Rotation;  // Radians
    public Vector2 Scale;
}

public struct Velocity
{
    public Vector2 Value;  // Pixels per second
}

// Rendering components
public struct SpriteRenderer
{
    public ITexture Texture;
    public Rectangle? SourceRect;
    public Vector2 Scale;
    public Color Tint;
    public int Layer;
}
```

**Best Practices:**
- Keep components as simple data containers
- No methods (except static factories and simple helpers)
- Use `struct` not `class` for performance
- Make fields public for ECS access

### Querying Entities

Systems query entities that have specific components:

```csharp
// Query all entities with Transform and Velocity
var query = World.Query<Transform, Velocity>();
foreach (var entity in query)
{
    // Get component references
    ref var transform = ref entity.Get<Transform>();
    ref var velocity = ref entity.Get<Velocity>();

    // Modify components
    transform.Position += velocity.Value * deltaTime;
}
```

**Query Types:**

```csharp
// 1 component
World.Query<Transform>();

// 2 components
World.Query<Transform, Velocity>();

// 3+ components
World.Query<Transform, Velocity, SpriteRenderer>();

// With filters (entities that DON'T have a component)
World.Query<Transform>().Where(e => !e.Has<Velocity>());
```

### Adding/Removing Components

Components can be added or removed from existing entities:

```csharp
// Add component
World.Add(entity, new Velocity { Value = new Vector2(100, 0) });

// Remove component
World.Remove<Velocity>(entity);

// Check if has component
bool hasVelocity = World.Has<Velocity>(entity);

// Get component value
if (World.TryGet<Transform>(entity, out var transform))
{
    Console.WriteLine($"Position: {transform.Position}");
}
```

### Destroying Entities

```csharp
// Immediate destruction
World.Destroy(entity);

// Deferred destruction (preferred in systems)
World.Add(entity, new PendingDestruction());
// DestructionSystem will remove it at the end of the frame
```

## Systems

Systems contain the logic that processes entities. Kobold provides many built-in systems and allows custom systems.

### ISystem Interface

```csharp
public interface ISystem
{
    void Update(float deltaTime, World world);
}
```

### Example System

```csharp
public class GravitySystem : ISystem
{
    private readonly Vector2 _gravity;

    public GravitySystem(Vector2 gravity)
    {
        _gravity = gravity;
    }

    public void Update(float deltaTime, World world)
    {
        var query = world.Query<Velocity, Physics>();
        foreach (var entity in query)
        {
            ref var velocity = ref entity.Get<Velocity>();
            ref var physics = ref entity.Get<Physics>();

            if (!physics.IsStatic)
            {
                velocity.Value += _gravity * physics.Mass * deltaTime;
            }
        }
    }
}
```

### Adding Systems

Systems are added in `Initialize()` with an execution order:

```csharp
protected override void Initialize()
{
    base.Initialize();

    // Lower order = runs first
    Systems.AddSystem(new InputSystem(InputManager), 0, true);
    Systems.AddSystem(new PhysicsSystem(), 100, true);
    Systems.AddSystem(new CollisionSystem(), 300, true);

    // Render systems (called separately during Render)
    Systems.AddRenderSystem(new RenderSystem(Renderer));
}
```

The third parameter (`requiresGameplayState`) determines if the system should pause when the game is paused.

### System Execution Order

Kobold defines standard execution order constants:

```csharp
public static class SystemOrder
{
    public const int INPUT = 0;
    public const int PHYSICS = 100;
    public const int AI = 200;
    public const int COLLISION = 300;
    public const int GAME_LOGIC = 400;
    public const int UI = 500;
    public const int CLEANUP = 600;
}
```

Use these to ensure consistent system ordering.

## Data-Oriented Design Benefits

ECS provides several performance advantages:

### Cache Locality

Components are stored in contiguous memory arrays (archetypes in Arch), improving CPU cache hit rates:

```
Traditional OOP:
[Player object] -> scattered memory locations
[Enemy object]  -> different memory locations

ECS:
[Transform, Transform, Transform, ...] -> contiguous array
[Velocity, Velocity, Velocity, ...]    -> contiguous array
```

### Parallel Processing

Systems can process entity queries in parallel since components are independent:

```csharp
// Future Kobold feature (Arch supports this)
World.Query<Transform, Velocity>().ForEach((ref Transform t, ref Velocity v) =>
{
    t.Position += v.Value * deltaTime;
}).Run(); // Parallel execution
```

### No Virtual Calls

Components are structs and systems query concrete types - no virtual method overhead.

## Composition Over Inheritance

ECS makes it trivial to compose complex behaviors from simple components:

```csharp
// Flying enemy
World.Create(
    new Transform(),
    new Velocity(),
    new AI { Type = AIType.Chase },
    new SpriteRenderer(),
    new Health { Value = 3 }
);

// Walking enemy (just add gravity)
World.Create(
    new Transform(),
    new Velocity(),
    new AI { Type = AIType.Patrol },
    new SpriteRenderer(),
    new Health { Value = 5 },
    new Physics { Mass = 1.0f }  // PhysicsSystem will apply gravity
);

// Invincible flying enemy (remove Health)
World.Create(
    new Transform(),
    new Velocity(),
    new AI { Type = AIType.Boss },
    new SpriteRenderer()
    // No Health component = can't be damaged
);
```

No inheritance hierarchies needed!

## Best Practices

### 1. Keep Components Small

```csharp
// Good - focused components
struct Transform { Vector2 Position; float Rotation; }
struct Velocity { Vector2 Value; }

// Bad - kitchen sink component
struct GameObject { Vector2 Position; Vector2 Velocity; Sprite Sprite; int Health; }
```

### 2. Use Tags for Categorization

```csharp
struct PlayerControlled { }  // Empty tag component
struct Enemy { }

// Query only player entities
var query = World.Query<Transform, PlayerControlled>();
```

### 3. Favor Queries Over Entity References

```csharp
// Good - data-oriented
var players = World.Query<Transform, PlayerControlled>();
foreach (var player in players) { /* ... */ }

// Okay for specific relationships
Entity _followTarget;  // Camera follows this entity
```

### 4. System Order Matters

Always add systems in logical order:
1. Input
2. AI/Game Logic
3. Physics
4. Collision
5. Destruction
6. Rendering

### 5. Use Deferred Destruction

```csharp
// During system update - don't destroy immediately
World.Add(entity, new PendingDestruction());

// DestructionSystem cleans up at the end of the frame
```

## Advanced Topics

### Singleton Components

For global game state, create a singleton entity:

```csharp
var gameState = World.Create(new GameState { Score = 0, Lives = 3 });

// Access anywhere
var query = World.Query<GameState>();
ref var state = ref query.First().Get<GameState>();
state.Score += 100;
```

### Entity Relationships

For parent-child or other relationships:

```csharp
struct Parent { public Entity Value; }
struct Child { public Entity ParentEntity; }

// Create parent
var parent = World.Create(new Transform());

// Create children
World.Create(
    new Transform(),
    new Child { ParentEntity = parent }
);
```

### Component Pools

Arch automatically pools component arrays for performance. You don't need to manage this.

## Learn More

- **[Game Engine](game-engine.md)** - How GameEngineBase manages the World
- **[Components Reference](components.md)** - All built-in components
- **[Systems Reference](systems.md)** - All built-in systems
- **[Arch Library](https://github.com/genaray/Arch)** - The underlying ECS library

---

**Next:** Learn about [GameEngineBase](game-engine.md) and the game lifecycle →
