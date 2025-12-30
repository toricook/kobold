# Systems Reference

Systems contain the logic that processes entities with specific components each frame. This page documents all built-in systems in Kobold.Core.

## System Basics

All systems implement `ISystem` or `IRenderSystem`:

```csharp
public interface ISystem
{
    void Update(float deltaTime, World world);
}

public interface IRenderSystem
{
    void Render(World world);
}
```

Systems are added in your game's `Initialize()` method:

```csharp
protected override void Initialize()
{
    base.Initialize();

    // Add update systems with execution order
    Systems.AddSystem(new PhysicsSystem(), order: 100, requiresGameplayState: true);
    Systems.AddSystem(new CollisionSystem(matrix), order: 300, requiresGameplayState: true);

    // Add render systems
    Systems.AddRenderSystem(new RenderSystem(Renderer));
}
```

## System Execution Order

Use these constants for consistent ordering:

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

## Built-in Systems

### PhysicsSystem

Moves entities based on velocity and applies physics simulation.

**Location:** `Kobold.Core.Systems.PhysicsSystem`

**Processes:** Entities with `Transform` and `Velocity` components

**Configuration:**
```csharp
public class PhysicsConfig
{
    public bool EnableGravity = false;
    public Vector2 GlobalGravity = new Vector2(0, 980);  // Pixels/sec²
    public bool EnableDamping = false;
    public float GlobalDamping = 0.99f;                  // 0-1
}
```

**Usage:**
```csharp
var config = new PhysicsConfig
{
    EnableGravity = true,
    GlobalGravity = new Vector2(0, 500),  // Platformer gravity
    EnableDamping = true,
    GlobalDamping = 0.98f
};

Systems.AddSystem(new PhysicsSystem(config), SystemOrder.PHYSICS, true);
```

**What it does:**
1. Applies gravity to entities with `Physics` component (if enabled)
2. Applies damping to velocity (if enabled)
3. Moves entities: `position += velocity * deltaTime`

**Example:**
```csharp
// Entity affected by gravity
World.Create(
    new Transform { Position = new Vector2(400, 100) },
    new Velocity { Value = Vector2.Zero },
    new Physics { Mass = 1.0f, Damping = 0.99f }
);
```

---

### CollisionSystem

Detects and resolves collisions between entities with BoxCollider.

**Location:** `Kobold.Core.Systems.CollisionSystem`

**Processes:** Entities with `Transform` and `BoxCollider` components

**Configuration:**
```csharp
public class CollisionMatrix
{
    public void SetCollision(CollisionLayer layerA, CollisionLayer layerB, bool collides);
    public bool ShouldCollide(CollisionLayer layerA, CollisionLayer layerB);
}
```

**Usage:**
```csharp
var matrix = new CollisionMatrix();
matrix.SetCollision(CollisionLayer.Player, CollisionLayer.Enemy, true);
matrix.SetCollision(CollisionLayer.Projectile, CollisionLayer.Enemy, true);
matrix.SetCollision(CollisionLayer.Player, CollisionLayer.Pickup, true);

var config = new CollisionSystemConfig
{
    EnableCollisionResponse = true,  // Apply physics-based impulses
    EnableCollisionEvents = true     // Publish CollisionEvent
};

Systems.AddSystem(new CollisionSystem(matrix, config), SystemOrder.COLLISION, true);
```

**Features:**
- AABB (box-box) collision detection
- Collision filtering via CollisionMatrix
- Optional collision response (separates overlapping entities)
- Publishes `CollisionEvent` to EventBus

**CollisionEvent:**
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
// Subscribe to collisions
Events.Subscribe<CollisionEvent>(OnCollision);

void OnCollision(CollisionEvent collision)
{
    if (collision.LayerA == CollisionLayer.Player &&
        collision.LayerB == CollisionLayer.Enemy)
    {
        // Player hit enemy
        World.Destroy(collision.EntityB);
    }
}
```

---

### RenderSystem

Draws all entities with rendering components.

**Location:** `Kobold.Core.Systems.RenderSystem`

**Processes:** Entities with `Transform` and rendering components (`SpriteRenderer`, `RectangleRenderer`, `TextRenderer`, `TriangleRenderer`)

**Usage:**
```csharp
Systems.AddRenderSystem(new RenderSystem(Renderer));
```

**What it does:**
1. Collects all renderable entities
2. Sorts by layer (lower layers render first/behind)
3. Applies camera transformation if Camera exists
4. Calls appropriate IRenderer methods for each entity type

**Rendering Order:**
- Background (layer 0)
- Game objects (layer 100)
- UI (layer 200)
- Debug (layer 300)

**Example:**
```csharp
// Background (renders behind everything)
World.Create(
    new Transform { Position = Vector2.Zero },
    SpriteRenderer.Background(bgTexture)  // Layer 0
);

// Game object (middle layer)
World.Create(
    new Transform { Position = new Vector2(400, 300) },
    SpriteRenderer.GameObject(playerTexture)  // Layer 100
);

// UI (renders on top)
World.Create(
    new Transform { Position = new Vector2(10, 10) },
    new TextRenderer { Text = "Score: 0", Layer = RenderLayers.UI }  // Layer 200
);
```

---

### AnimationSystem

Updates sprite animations.

**Location:** `Kobold.Core.Systems.AnimationSystem`

**Processes:** Entities with `Animation` and `SpriteRenderer` components

**Usage:**
```csharp
Systems.AddSystem(new AnimationSystem(), SystemOrder.GAME_LOGIC, true);
```

**What it does:**
1. Advances animation time
2. Changes frames when frame duration expires
3. Loops or stops at end based on clip settings
4. Updates SpriteRenderer.SourceRect to current frame

**Example:**
```csharp
var spriteSheet = Assets.LoadSpriteSheet("player.json");
var texture = Assets.LoadTexture("player.png");

var animation = new Animation();
animation.Clips["walk"] = spriteSheet.CreateAnimationClip("walk");
animation.Clips["idle"] = spriteSheet.CreateAnimationClip("idle");
animation.Play("walk");

World.Create(
    new Transform { Position = new Vector2(400, 300) },
    SpriteRenderer.FromSpriteSheet(texture, spriteSheet.GetFrame(0)),
    animation
);

// Switch animations in Update()
ref var anim = ref entity.Get<Animation>();
if (velocity.IsMoving)
    anim.Play("walk");
else
    anim.Play("idle");
```

---

### BoundarySystem

Handles screen boundary behavior (wrap, bounce, clamp, destroy).

**Location:** `Kobold.Core.Systems.BoundarySystem`

**Processes:** Entities with `Transform` (and optionally `Velocity` for bouncing)

**Usage:**
```csharp
Systems.AddSystem(new BoundarySystem(BoundaryBehavior.Wrap), SystemOrder.GAME_LOGIC, true);
```

**Boundary Behaviors:**
- `None` - Do nothing
- `Wrap` - Wrap to opposite side (Asteroids-style)
- `Clamp` - Stop at edge
- `Bounce` - Bounce off edges (reverses velocity)
- `Destroy` - Destroy entity when it leaves screen

**Requires:** An entity with `ScreenBounds` component defining the boundaries

**Example:**
```csharp
// Define screen bounds
World.Create(new ScreenBounds
{
    MinX = 0,
    MaxX = 800,
    MinY = 0,
    MaxY = 600
});

// Wrap behavior (Asteroids-style)
Systems.AddSystem(new BoundarySystem(BoundaryBehavior.Wrap), 200, true);

// Per-entity custom behavior
World.Create(
    new Transform { Position = new Vector2(400, 300) },
    new Velocity { Value = new Vector2(100, 0) },
    new CustomBoundaryBehavior { Behavior = BoundaryBehavior.Bounce }
);
```

---

### DestructionSystem

Removes entities marked with `PendingDestruction`.

**Location:** `Kobold.Core.Systems.DestructionSystem`

**Processes:** Entities with `PendingDestruction` component

**Usage:**
```csharp
Systems.AddSystem(new DestructionSystem(), SystemOrder.CLEANUP, true);
```

**What it does:**
- Destroys all entities marked with `PendingDestruction`
- Runs at the end of the frame (cleanup phase)
- Safer than calling `World.Destroy()` during system updates

**Example:**
```csharp
// Mark entity for destruction
World.Add(entity, new PendingDestruction());

// DestructionSystem will remove it at end of frame
```

---

### InputSystem

Processes input and updates player-controlled entities.

**Location:** `Kobold.Core.Systems.InputSystem`

**Processes:** Custom - you typically query entities yourself

**Usage:**
```csharp
Systems.AddSystem(new InputSystem(InputManager), SystemOrder.INPUT, true);
```

Note: InputSystem is often custom per-game. You can also handle input directly in your GameEngineBase.Update() method.

**Example:**
```csharp
public class PlayerInputSystem : ISystem
{
    private readonly IInputManager _input;

    public PlayerInputSystem(IInputManager input)
    {
        _input = input;
    }

    public void Update(float deltaTime, World world)
    {
        var players = world.Query<Transform, Velocity, PlayerControlled>();
        foreach (var player in players)
        {
            ref var velocity = ref player.Get<Velocity>();

            var moveDir = Vector2.Zero;
            if (_input.IsKeyDown(KeyCode.W)) moveDir.Y -= 1;
            if (_input.IsKeyDown(KeyCode.S)) moveDir.Y += 1;
            if (_input.IsKeyDown(KeyCode.A)) moveDir.X -= 1;
            if (_input.IsKeyDown(KeyCode.D)) moveDir.X += 1;

            if (moveDir != Vector2.Zero)
            {
                velocity.Value = Vector2.Normalize(moveDir) * 200f;
            }
            else
            {
                velocity.Value = Vector2.Zero;
            }
        }
    }
}
```

---

### GameStateSystem

Manages game state transitions and pausing.

**Location:** `Kobold.Core.Systems.GameStateSystem`

**Processes:** Entities with `GameState` component

**Usage:**
```csharp
Systems.AddSystem(new GameStateSystem(), SystemOrder.GAME_LOGIC, false);  // Always runs
```

**What it does:**
- Manages pausing/unpausing based on GameState
- Systems with `requiresGameplayState = true` pause when not in Playing state
- Systems with `requiresGameplayState = false` always run

**Example:**
```csharp
// Create game state entity
var gameState = World.Create(new GameState
{
    State = GameStateType.Playing
});

// Pause game
ref var state = ref World.Get<GameState>(gameState);
state.State = GameStateType.Paused;  // Gameplay systems stop updating
```

---

## Creating Custom Systems

### Basic Custom System

```csharp
public class GravityWellSystem : ISystem
{
    private readonly Vector2 _wellPosition;
    private readonly float _strength;

    public GravityWellSystem(Vector2 position, float strength)
    {
        _wellPosition = position;
        _strength = strength;
    }

    public void Update(float deltaTime, World world)
    {
        var query = world.Query<Transform, Velocity>();
        foreach (var entity in query)
        {
            ref var transform = ref entity.Get<Transform>();
            ref var velocity = ref entity.Get<Velocity>();

            var direction = Vector2.Normalize(_wellPosition - transform.Position);
            var distance = Vector2.Distance(transform.Position, _wellPosition);

            if (distance > 10)  // Avoid division by zero
            {
                var force = direction * (_strength / distance);
                velocity.Value += force * deltaTime;
            }
        }
    }
}

// Add to game
Systems.AddSystem(new GravityWellSystem(new Vector2(400, 300), 10000f), 150, true);
```

### Custom Render System

```csharp
public class DebugColliderRenderSystem : IRenderSystem
{
    private readonly IRenderer _renderer;

    public DebugColliderRenderSystem(IRenderer renderer)
    {
        _renderer = renderer;
    }

    public void Render(World world)
    {
        var query = world.Query<Transform, BoxCollider>();
        foreach (var entity in query)
        {
            ref var transform = ref entity.Get<Transform>();
            ref var collider = ref entity.Get<BoxCollider>();

            var bounds = collider.GetWorldBounds(transform);

            // Draw collider outline
            _renderer.DrawRectangle(
                new Vector2(bounds.Left, bounds.Top),
                new Vector2(bounds.Width, bounds.Height),
                Color.Lime
            );
        }
    }
}
```

## Best Practices

### 1. Respect System Order

Always add systems in logical order:
```csharp
Systems.AddSystem(new InputSystem(...), SystemOrder.INPUT, true);
Systems.AddSystem(new PhysicsSystem(), SystemOrder.PHYSICS, true);
Systems.AddSystem(new CollisionSystem(...), SystemOrder.COLLISION, true);
Systems.AddSystem(new DestructionSystem(), SystemOrder.CLEANUP, true);
```

### 2. Use requiresGameplayState Correctly

- `true` - System pauses when game is paused (gameplay systems)
- `false` - System always runs (UI, menu systems)

### 3. Don't Destroy During Iteration

```csharp
// Bad - can cause issues
foreach (var entity in query)
{
    World.Destroy(entity.Entity);  // Dangerous!
}

// Good - mark for deferred destruction
foreach (var entity in query)
{
    World.Add(entity.Entity, new PendingDestruction());
}
```

### 4. Cache Expensive Queries

```csharp
public class MySystem : ISystem
{
    private QueryDescription _query;

    public MySystem()
    {
        _query = new QueryDescription().WithAll<Transform, Velocity>();
    }

    public void Update(float deltaTime, World world)
    {
        foreach (var entity in world.Query(_query))
        {
            // Process...
        }
    }
}
```

## See Also

- **[Components](components.md)** - Components that systems process
- **[Architecture](architecture.md)** - Understanding ECS
- **[Game Engine](game-engine.md)** - Adding systems to your game

---

**Next:** Learn about [Asset Management](asset-manager.md) →
