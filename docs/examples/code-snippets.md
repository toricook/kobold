# Code Snippets - Common Patterns

This page contains copy-paste ready code snippets for common game development patterns in Kobold. Use these as starting points for your own implementations.

## Table of Contents

- [Creating Entities](#creating-entities)
- [Movement and Physics](#movement-and-physics)
- [Input Handling](#input-handling)
- [Collision Detection](#collision-detection)
- [Rendering](#rendering)
- [Animation](#animation)
- [Cameras](#cameras)
- [Timers and Lifecycle](#timers-and-lifecycle)
- [Events](#events)
- [Utilities](#utilities)

## Creating Entities

### Simple Moving Sprite

```csharp
var texture = Assets.LoadTexture("player.png");
var player = World.Create(
    new Transform { Position = new Vector2(400, 300) },
    SpriteRenderer.FullTexture(texture),
    new Velocity { Value = new Vector2(100, 0) }
);
```

### Rectangle with Physics

```csharp
var entity = World.Create(
    new Transform { Position = new Vector2(100, 100) },
    new RectangleRenderer
    {
        Size = new Vector2(50, 50),
        Color = Color.Green,
        Layer = 100
    },
    new Velocity { Value = Vector2.Zero },
    new Physics
    {
        Mass = 1.0f,
        Damping = 0.98f,
        Restitution = 0.8f
    },
    BoxCollider.Square(50)
);
```

### Sprite from Sprite Sheet

```csharp
var spriteSheet = Assets.LoadSpriteSheet("characters.json");
var texture = Assets.LoadTexture("characters.png");

var entity = World.Create(
    new Transform { Position = new Vector2(200, 200) },
    SpriteRenderer.FromSpriteSheet(
        texture,
        spriteSheet.GetFrame(0),
        scale: 2.0f
    )
);
```

### Entity with Animation

```csharp
var spriteSheet = Assets.LoadSpriteSheet("player.json");
var texture = Assets.LoadTexture("player.png");

var animation = new Animation();
animation.Clips["walk"] = spriteSheet.CreateAnimationClip("walk");
animation.Clips["idle"] = spriteSheet.CreateAnimationClip("idle");
animation.Play("idle");

var player = World.Create(
    new Transform { Position = new Vector2(400, 300) },
    SpriteRenderer.FromSpriteSheet(texture, spriteSheet.GetFrame(0)),
    animation
);
```

## Movement and Physics

### Apply Force/Impulse

```csharp
// Query entities with velocity
var query = World.Query<Velocity>();
foreach (var entity in query)
{
    ref var velocity = ref entity.Get<Velocity>();

    // Apply an impulse (instant velocity change)
    velocity.Value += new Vector2(100, -200);

    // Or set velocity directly
    velocity.Value = new Vector2(150, 0);
}
```

### Continuous Movement

```csharp
// In your game's Update method
protected override void Update(float deltaTime)
{
    base.Update(deltaTime);

    var query = World.Query<Transform, Velocity>();
    foreach (var entity in query)
    {
        ref var transform = ref entity.Get<Transform>();
        ref var velocity = ref entity.Get<Velocity>();

        transform.Position += velocity.Value * deltaTime;
    }
}
```

### Move Towards Target

```csharp
var query = World.Query<Transform, Velocity>();
foreach (var entity in query)
{
    ref var transform = ref entity.Get<Transform>();
    ref var velocity = ref entity.Get<Velocity>();

    var target = new Vector2(500, 300);
    var direction = Vector2.Normalize(target - transform.Position);
    var speed = 100f;

    velocity.Value = direction * speed;
}
```

### Rotate to Face Direction

```csharp
ref var transform = ref entity.Get<Transform>();
ref var velocity = ref entity.Get<Velocity>();

if (velocity.IsMoving)
{
    transform.LookAt(transform.Position + velocity.Direction);
}
```

## Input Handling

### Basic Keyboard Input

```csharp
protected override void Update(float deltaTime)
{
    base.Update(deltaTime);

    if (InputManager.IsKeyPressed(KeyCode.Space))
    {
        // Fire weapon (triggered once per key press)
    }

    if (InputManager.IsKeyDown(KeyCode.W))
    {
        // Move forward (continuous while held)
    }
}
```

### Player Movement (8-Direction)

```csharp
var query = World.Query<Transform, Velocity, PlayerControlled>();
foreach (var entity in query)
{
    ref var velocity = ref entity.Get<Velocity>();

    var moveDir = Vector2.Zero;
    if (InputManager.IsKeyDown(KeyCode.W)) moveDir.Y -= 1;
    if (InputManager.IsKeyDown(KeyCode.S)) moveDir.Y += 1;
    if (InputManager.IsKeyDown(KeyCode.A)) moveDir.X -= 1;
    if (InputManager.IsKeyDown(KeyCode.D)) moveDir.X += 1;

    if (moveDir != Vector2.Zero)
    {
        moveDir = Vector2.Normalize(moveDir);
        velocity.Value = moveDir * 200f; // 200 pixels per second
    }
    else
    {
        velocity.Value = Vector2.Zero;
    }
}
```

### Mouse Input

```csharp
var mousePos = InputManager.GetMousePosition();

if (InputManager.IsMouseButtonPressed(MouseButton.Left))
{
    // Spawn something at mouse position
    World.Create(
        new Transform { Position = mousePos },
        new RectangleRenderer { Size = new Vector2(10, 10), Color = Color.Red }
    );
}
```

## Collision Detection

### Subscribe to Collision Events

```csharp
protected override void Initialize()
{
    base.Initialize();

    // Subscribe to collision events
    Events.Subscribe<CollisionEvent>(OnCollision);
}

private void OnCollision(CollisionEvent collision)
{
    // Check collision layers
    if (collision.LayerA == CollisionLayer.Player &&
        collision.LayerB == CollisionLayer.Enemy)
    {
        // Player hit enemy
        World.Destroy(collision.EntityB);
    }

    if (collision.LayerA == CollisionLayer.Projectile &&
        collision.LayerB == CollisionLayer.Enemy)
    {
        // Projectile hit enemy
        World.Destroy(collision.EntityA); // Destroy projectile
        World.Destroy(collision.EntityB); // Destroy enemy
    }
}
```

### Custom Collision Response

```csharp
private void OnCollision(CollisionEvent collision)
{
    // Apply knockback
    if (World.Has<Velocity>(collision.EntityB))
    {
        ref var velocity = ref World.Get<Velocity>(collision.EntityB);
        velocity.Value += collision.Normal * 300f; // Knockback force
    }
}
```

### Point-in-Rectangle Check

```csharp
var query = World.Query<Transform, BoxCollider>();
var mousePos = InputManager.GetMousePosition();

foreach (var entity in query)
{
    ref var transform = ref entity.Get<Transform>();
    ref var collider = ref entity.Get<BoxCollider>();

    if (collider.Contains(mousePos))
    {
        // Mouse is over this entity
    }
}
```

## Rendering

### Layer-based Rendering

```csharp
// Background (renders behind)
World.Create(
    new Transform { Position = Vector2.Zero },
    SpriteRenderer.Background(backgroundTexture)
);

// Game objects (middle layer)
World.Create(
    new Transform { Position = new Vector2(100, 100) },
    SpriteRenderer.GameObject(playerTexture)
);

// UI (renders on top)
World.Create(
    new Transform { Position = new Vector2(10, 10) },
    new TextRenderer
    {
        Text = "Score: 0",
        Color = Color.White,
        FontSize = 24,
        Layer = RenderLayers.UI
    }
);
```

### Sprite with Tint and Rotation

```csharp
World.Create(
    new Transform { Position = new Vector2(200, 200), Rotation = MathF.PI / 4 },
    new SpriteRenderer
    {
        Texture = texture,
        SourceRect = null, // Use full texture
        Scale = new Vector2(2, 2),
        Tint = Color.FromArgb(255, 255, 100, 100), // Reddish tint
        Layer = 100
    }
);
```

## Animation

### Play Animation Based on State

```csharp
var query = World.Query<Velocity, Animation>();
foreach (var entity in query)
{
    ref var velocity = ref entity.Get<Velocity>();
    ref var animation = ref entity.Get<Animation>();

    if (velocity.IsMoving)
    {
        if (animation.CurrentClip != "walk")
            animation.Play("walk");
    }
    else
    {
        if (animation.CurrentClip != "idle")
            animation.Play("idle");
    }
}
```

### Create Animation from Grid

```csharp
var walkClip = AnimationClip.FromGrid(
    gridWidth: 8,          // 8 frames wide
    gridHeight: 1,         // 1 frame tall
    frameWidth: 32,
    frameHeight: 32,
    startFrame: 0,
    frameCount: 8,
    frameDuration: 0.1f,   // 10 FPS
    loop: true
);
```

## Cameras

### Follow Player

```csharp
// Create camera entity
var camera = World.Create(
    new Camera
    {
        Position = Vector2.Zero,
        ViewportWidth = 800,
        ViewportHeight = 600,
        SmoothSpeed = 5f,
        FollowTarget = playerEntity
    }
);
```

### Camera with Bounds

```csharp
var camera = World.Create(
    new Camera
    {
        Position = Vector2.Zero,
        ViewportWidth = 800,
        ViewportHeight = 600,
        MinBounds = new Vector2(0, 0),
        MaxBounds = new Vector2(2000, 1500) // Map size
    }
);
```

## Timers and Lifecycle

### Timed Destruction

```csharp
// Create a particle that lives for 2 seconds
World.Create(
    new Transform { Position = new Vector2(300, 300) },
    new RectangleRenderer { Size = new Vector2(5, 5), Color = Color.Yellow },
    new Lifetime { TimeRemaining = 2.0f }
);
```

### Delayed Action

```csharp
// Custom component
public struct DelayedAction
{
    public float TimeRemaining;
    public Action Action;
}

// Usage
World.Create(
    new DelayedAction
    {
        TimeRemaining = 3.0f,
        Action = () => Console.WriteLine("3 seconds elapsed!")
    }
);

// In a custom system
var query = World.Query<DelayedAction>();
foreach (var entity in query)
{
    ref var delayed = ref entity.Get<DelayedAction>();
    delayed.TimeRemaining -= deltaTime;

    if (delayed.TimeRemaining <= 0)
    {
        delayed.Action?.Invoke();
        World.Destroy(entity.Entity);
    }
}
```

## Events

### Publish Custom Event

```csharp
// Define event
public record PlayerScoredEvent(int Points, Entity Player);

// Publish
Events.Publish(new PlayerScoredEvent(100, playerEntity));

// Subscribe
Events.Subscribe<PlayerScoredEvent>(e =>
{
    Console.WriteLine($"Player scored {e.Points} points!");
});
```

### One-time Event Handler

```csharp
void OnGameStart()
{
    Action<PlayerDiedEvent> handler = null;
    handler = (e) =>
    {
        Console.WriteLine("Game Over!");
        Events.Unsubscribe(handler); // Unsubscribe after first call
    };

    Events.Subscribe(handler);
}
```

## Utilities

### Random Position in Rectangle

```csharp
var randomX = MathUtils.RandomRange(0, 800);
var randomY = MathUtils.RandomRange(0, 600);
var randomPos = new Vector2(randomX, randomY);
```

### Random Direction

```csharp
var randomDirection = MathUtils.RandomDirection();
var velocity = randomDirection * 150f; // Random direction, fixed speed
```

### Lerp Position

```csharp
ref var transform = ref entity.Get<Transform>();
var target = new Vector2(500, 300);

transform.Position = MathUtils.Lerp(
    transform.Position,
    target,
    0.05f // Interpolation factor (0-1)
);
```

### Clamp to Bounds

```csharp
ref var transform = ref entity.Get<Transform>();

transform.Position = new Vector2(
    MathUtils.Clamp(transform.Position.X, 0, 800),
    MathUtils.Clamp(transform.Position.Y, 0, 600)
);
```

### Angle Between Vectors

```csharp
var from = transform.Forward;
var to = Vector2.Normalize(target - transform.Position);

var angleDiff = MathUtils.AngleDifference(
    MathF.Atan2(from.Y, from.X),
    MathF.Atan2(to.Y, to.X)
);
```

## Complete Examples

### Spawning Projectiles

```csharp
private void FireProjectile(Vector2 position, Vector2 direction)
{
    var projectileTexture = Assets.LoadTexture("bullet.png");

    World.Create(
        new Transform
        {
            Position = position,
            Rotation = MathF.Atan2(direction.Y, direction.X)
        },
        SpriteRenderer.FullTexture(projectileTexture),
        new Velocity { Value = direction * 400f },
        BoxCollider.FromRenderSize(projectileTexture.Width, projectileTexture.Height),
        new Lifetime { TimeRemaining = 5.0f },
        new Tags { Values = new[] { "Projectile" } }
    );
}
```

### Power-up System

```csharp
// Power-up component
public struct PowerUp
{
    public PowerUpType Type;
    public float Duration;
}

// Create power-up entity
World.Create(
    new Transform { Position = new Vector2(300, 300) },
    SpriteRenderer.FullTexture(powerUpTexture),
    BoxCollider.Square(32),
    new PowerUp { Type = PowerUpType.SpeedBoost, Duration = 5.0f }
);

// Collision handler
private void OnCollision(CollisionEvent collision)
{
    if (World.Has<PowerUp>(collision.EntityA) &&
        World.Has<PlayerControlled>(collision.EntityB))
    {
        var powerUp = World.Get<PowerUp>(collision.EntityA);
        ApplyPowerUp(collision.EntityB, powerUp);
        World.Destroy(collision.EntityA);
    }
}
```

---

**Need more examples?** Check out the example games:
- [Asteroids Walkthrough](asteroids-walkthrough.md)
- [Pong Walkthrough](pong-walkthrough.md)
- [Cave Explorer Walkthrough](cave-explorer-walkthrough.md)
