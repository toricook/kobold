---
layout: default
title: Game Engine
parent: Core Framework
nav_order: 2
---

# GameEngineBase - Core Game Class

`GameEngineBase` is the abstract base class that every Kobold game extends. It manages the game lifecycle, ECS World, systems, assets, events, and platform implementations.

## Overview

Every Kobold game follows this pattern:

```csharp
public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize(); // Always call base.Initialize() first!

        // Your initialization code here
        // - Create initial entities
        // - Add systems
        // - Load assets
        // - Subscribe to events
    }

    protected override void Update(float deltaTime)
    {
        base.Update(deltaTime); // Calls SystemManager.UpdateAll()

        // Optional: Additional update logic
    }

    protected override void Render()
    {
        base.Render(); // Calls SystemManager.RenderAll()

        // Optional: Additional rendering
    }
}
```

## Game Lifecycle

GameEngineBase defines four lifecycle methods:

###  1. Initialize()

Called once when the game starts. Set up your game here:

```csharp
protected override void Initialize()
{
    base.Initialize(); // Creates World, Systems, Assets, Events

    // Create screen bounds
    World.Create(new ScreenBounds { MinX = 0, MaxX = 800, MinY = 0, MaxY = 600 });

    // Load assets
    var playerTexture = Assets.LoadTexture("player.png");

    // Create initial entities
    var player = World.Create(
        new Transform { Position = new Vector2(400, 300) },
        SpriteRenderer.FullTexture(playerTexture),
        new PlayerControlled()
    );

    // Add systems
    var physicsConfig = new PhysicsConfig { EnableGravity = false };
    Systems.AddSystem(new PhysicsSystem(physicsConfig), 100, true);
    Systems.AddSystem(new CollisionSystem(new CollisionMatrix()), 300, true);
    Systems.AddRenderSystem(new RenderSystem(Renderer));

    // Subscribe to events
    Events.Subscribe<CollisionEvent>(OnCollision);
}
```

**Important:** Always call `base.Initialize()` first! It initializes the core framework.

### 2. Update(deltaTime)

Called every frame with the elapsed time since last frame (in seconds):

```csharp
protected override void Update(float deltaTime)
{
    base.Update(deltaTime); // Updates all systems via SystemManager

    // Optional: Game-specific update logic
    // Example: Check win conditions, spawn enemies, etc.

    var query = World.Query<GameState>();
    if (!query.IsEmpty)
    {
        ref var state = ref query.First().Get<GameState>();
        if (state.Score >= 1000)
        {
            Console.WriteLine("You win!");
        }
    }
}
```

The base implementation calls `SystemManager.UpdateAll(deltaTime)` which runs all registered update systems in order.

### 3. Render()

Called every frame to draw the game:

```csharp
protected override void Render()
{
    base.Render(); // Calls SystemManager.RenderAll()

    // Optional: Additional rendering (debug info, etc.)
}
```

The base implementation calls `SystemManager.RenderAll()` which runs all render systems.

### 4. Shutdown()

Called when the game is closing. Clean up resources:

```csharp
protected override void Shutdown()
{
    base.Shutdown(); // Cleans up World, Systems, Events

    // Optional: Additional cleanup
    // (Kobold handles most cleanup automatically)
}
```

## Protected Properties

GameEngineBase provides these properties for your game:

### World

```csharp
protected World World { get; }
```

The ECS World from Arch library. Use it to:
- Create entities: `World.Create(...)`
- Query entities: `World.Query<T>()`
- Destroy entities: `World.Destroy(entity)`

See [Architecture](architecture.md) for more on the World.

### Systems

```csharp
protected SystemManager Systems { get; }
```

Manages all game systems. Use it to:
- Add update systems: `Systems.AddSystem(system, order, requiresGameplayState)`
- Add render systems: `Systems.AddRenderSystem(system)`
- Get systems: `Systems.GetSystem<T>()`

See [Systems](systems.md) for more details.

### Assets

```csharp
protected AssetManager Assets { get; }
```

Manages textures and sprite sheets. Use it to:
- Load textures: `Assets.LoadTexture("path.png")`
- Load sprite sheets: `Assets.LoadSpriteSheet("config.json")`
- Preload assets: `Assets.PreloadTextures(...)`

See [Asset Manager](asset-manager.md) for more details.

### Events

```csharp
protected EventBus Events { get; }
```

Pub/sub event system. Use it to:
- Subscribe: `Events.Subscribe<T>(handler)`
- Publish: `Events.Publish(eventData)`
- Unsubscribe: `Events.Unsubscribe<T>(handler)`

See [Event Bus](event-bus.md) for more details.

### Platform Implementations

These are set by the host (MonoGameHost) via dependency injection:

```csharp
protected IRenderer Renderer { get; private set; }
protected IInputManager InputManager { get; private set; }
protected IContentLoader ContentLoader { get; private set; }
```

You typically don't call these directly - systems use them. But they're available if needed:

```csharp
// Check input directly
if (InputManager.IsKeyPressed(KeyCode.Escape))
{
    // Quit game
}

// Get mouse position
var mousePos = InputManager.GetMousePosition();
```

## Dependency Injection

GameEngineBase uses setter injection for platform implementations:

```csharp
public void SetRenderer(IRenderer renderer)
public void SetInputManager(IInputManager inputManager)
public void SetContentLoader(IContentLoader contentLoader)
```

These are called by the host (MonoGameHost) before Initialize():

```csharp
// MonoGameHost does this automatically:
var game = new MyGame();
game.SetRenderer(new MonoGameRenderer(spriteBatch));
game.SetInputManager(new MonoGameInputManager());
game.SetContentLoader(new MonoGameContentLoader("Content"));
game.Initialize();
```

You don't need to call these manually when using MonoGameHost.

## Complete Example

Here's a complete game showing all lifecycle methods:

```csharp
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using Kobold.Core.Events;
using Kobold.Monogame;
using System.Numerics;
using System.Drawing;

public class SpaceShooter : GameEngineBase
{
    private Entity _player;
    private int _score = 0;

    protected override void Initialize()
    {
        base.Initialize();

        // Set up screen bounds
        World.Create(new ScreenBounds
        {
            MinX = 0, MaxX = 800,
            MinY = 0, MaxY = 600
        });

        // Load assets
        var playerTexture = Assets.LoadTexture("player.png");
        var enemyTexture = Assets.LoadTexture("enemy.png");

        // Create player
        _player = World.Create(
            new Transform { Position = new Vector2(400, 500) },
            SpriteRenderer.FullTexture(playerTexture),
            new Velocity(),
            new PlayerControlled(),
            BoxCollider.FromRenderSize(playerTexture.Width, playerTexture.Height)
        );

        // Add systems
        var physicsConfig = new PhysicsConfig { EnableGravity = false };
        Systems.AddSystem(new InputSystem(InputManager), 0, true);
        Systems.AddSystem(new PhysicsSystem(physicsConfig), 100, true);
        Systems.AddSystem(new CollisionSystem(CreateCollisionMatrix()), 300, true);
        Systems.AddSystem(new BoundarySystem(BoundaryBehavior.Clamp), 200, true);
        Systems.AddRenderSystem(new RenderSystem(Renderer));

        // Subscribe to events
        Events.Subscribe<CollisionEvent>(OnCollision);
    }

    protected override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // Handle player input
        if (World.TryGet(_player, out Velocity velocity))
        {
            var moveSpeed = 300f;
            var moveDir = Vector2.Zero;

            if (InputManager.IsKeyDown(KeyCode.Left)) moveDir.X -= 1;
            if (InputManager.IsKeyDown(KeyCode.Right)) moveDir.X += 1;

            velocity.Value = moveDir * moveSpeed;
            World.Set(_player, velocity);
        }

        // Shoot on spacebar
        if (InputManager.IsKeyPressed(KeyCode.Space))
        {
            FireProjectile();
        }
    }

    protected override void Render()
    {
        base.Render();

        // Could add debug rendering here if needed
    }

    protected override void Shutdown()
    {
        Console.WriteLine($"Final Score: {_score}");
        base.Shutdown();
    }

    private void OnCollision(CollisionEvent collision)
    {
        // Handle collisions
        if (collision.LayerA == CollisionLayer.Projectile &&
            collision.LayerB == CollisionLayer.Enemy)
        {
            World.Destroy(collision.EntityA);
            World.Destroy(collision.EntityB);
            _score += 100;
        }
    }

    private void FireProjectile()
    {
        if (!World.TryGet(_player, out Transform playerTransform))
            return;

        var bulletTexture = Assets.LoadTexture("bullet.png");
        World.Create(
            new Transform { Position = playerTransform.Position },
            SpriteRenderer.FullTexture(bulletTexture),
            new Velocity { Value = new Vector2(0, -500) },
            BoxCollider.Square(8),
            new Lifetime { TimeRemaining = 3f }
        );
    }

    private CollisionMatrix CreateCollisionMatrix()
    {
        var matrix = new CollisionMatrix();
        matrix.SetCollision(CollisionLayer.Player, CollisionLayer.Enemy, true);
        matrix.SetCollision(CollisionLayer.Projectile, CollisionLayer.Enemy, true);
        return matrix;
    }
}

// Entry point
MonoGameHost.Run(new SpaceShooter(), new GameConfig
{
    WindowWidth = 800,
    WindowHeight = 600,
    WindowTitle = "Space Shooter"
});
```

## GameConfig

Configure your game window and settings via `GameConfig`:

```csharp
var config = new GameConfig
{
    WindowWidth = 1024,
    WindowHeight = 768,
    WindowTitle = "My Awesome Game",
    BackgroundColor = Color.FromArgb(255, 20, 20, 40), // Dark blue
    TargetFPS = 60
};

MonoGameHost.Run(new MyGame(), config);
```

## Best Practices

### 1. Always Call base Methods

```csharp
protected override void Initialize()
{
    base.Initialize(); // REQUIRED!
    // Your code here
}
```

### 2. Initialize in the Right Place

- **Initialize()** - Create entities, add systems, load assets
- **Constructor** - Avoid! Use Initialize() instead
- **Update()** - Respond to gameplay events, check conditions
- **Render()** - Usually just call base.Render()

### 3. Store Important Entity References

```csharp
private Entity _player;
private Entity _camera;

protected override void Initialize()
{
    base.Initialize();

    _player = World.Create(/* ... */);
    _camera = World.Create(/* ... */);

    // Can reference these entities throughout the game
}
```

### 4. Use Systems for Recurring Logic

Don't put recurring logic in Update() - create a system instead:

```csharp
// Bad - clutters Update method
protected override void Update(float deltaTime)
{
    base.Update(deltaTime);

    var query = World.Query<Transform, Velocity>();
    foreach (var entity in query)
    {
        // Move entities...
    }
}

// Good - create a system
Systems.AddSystem(new PhysicsSystem(), 100, true);
```

### 5. Handle Cleanup Automatically

Most cleanup is automatic. Only override Shutdown() if you have custom resources:

```csharp
protected override void Shutdown()
{
    // Save high scores, close connections, etc.
    SaveHighScores();

    base.Shutdown(); // Always call at the end
}
```

## See Also

- **[Architecture](architecture.md)** - Understanding ECS
- **[Systems](systems.md)** - Built-in systems reference
- **[Asset Manager](asset-manager.md)** - Loading and managing assets
- **[Event Bus](event-bus.md)** - Event-driven architecture

---

**Next:** Learn about [Components](components.md) →
