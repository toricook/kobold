# Kobold.Core - Framework Overview

**Kobold.Core** is the heart of the Kobold framework. It provides the Entity Component System architecture, game engine base, physics, collision, rendering pipeline, and all the core components and systems you need to build 2D games.

## What's in Kobold.Core?

Kobold.Core is platform-agnostic and contains:

- **ECS Architecture** - Built on the high-performance Arch library
- **GameEngineBase** - Abstract base class for your games
- **Components** - Data containers for entities (Transform, Velocity, Renderers, etc.)
- **Systems** - Logic processors (Physics, Collision, Rendering, Animation)
- **Platform Abstractions** - Interfaces for rendering, input, and content loading
- **Asset Management** - Texture and sprite sheet loading with caching
- **Event System** - Decoupled pub/sub event bus
- **Utilities** - Math helpers and common functions

## Core Concepts

### [Architecture](architecture.md)

Learn about the Entity Component System pattern and how Kobold implements it using the Arch library. Understand entities, components, systems, and queries.

**Key Topics:**
- What is ECS and why use it?
- Arch library integration
- World management
- Entity queries
- Data-oriented design

### [Game Engine](game-engine.md)

The `GameEngineBase` class is the foundation of every Kobold game. It manages the game lifecycle, systems, assets, and events.

**Key Topics:**
- Game lifecycle (Initialize, Update, Render, Shutdown)
- Dependency injection for platform implementations
- World, Systems, Assets, Events access
- Configuration via `GameConfig`

### [Components](components.md)

Components are data structs attached to entities. Kobold provides many built-in components for common game needs.

**Available Components:**
- **Transform** - Position, rotation, scale
- **Velocity** - Movement vector
- **Physics** - Mass, restitution, damping
- **BoxCollider** - Rectangular collision bounds
- **Camera** - Viewport and following
- **SpriteRenderer** - Texture rendering
- **Animation** - Sprite animation
- **RectangleRenderer, TextRenderer, TriangleRenderer** - Primitive rendering
- **Lifetime** - Timed destruction
- **Tags** - Entity labeling
- **GameState** - Game state management

### [Systems](systems.md)

Systems contain the logic that processes entities with specific components each frame.

**Built-in Systems:**
- **PhysicsSystem** - Moves entities based on velocity and physics
- **CollisionSystem** - Detects and resolves collisions
- **RenderSystem** - Draws entities to screen
- **AnimationSystem** - Updates sprite animations
- **BoundarySystem** - Handles screen boundaries (wrap, bounce, destroy)
- **DestructionSystem** - Removes entities marked for destruction
- **GameStateSystem** - Manages game state transitions

### [Asset Manager](asset-manager.md)

The `AssetManager` class handles loading, caching, and managing textures and sprite sheets.

**Features:**
- Automatic texture caching
- Sprite sheet support with JSON configuration
- Named regions and animations
- Preloading and unloading

### [Event Bus](event-bus.md)

The `EventBus` provides a decoupled pub/sub system for game events.

**Features:**
- Type-safe event publishing and subscription
- Interface-based or Action-based handlers
- Collision events, custom events
- Hierarchical event types

### [Utilities](utilities.md)

Helper classes and functions for common game development tasks.

**Available Utilities:**
- **MathUtils** - Clamp, lerp, random, angles, vectors
- **EntityFactory** - Quick entity creation helpers
- **Configuration classes** - PhysicsConfig, GameConfig, CollisionMatrix

## Platform Abstractions

Kobold.Core defines interfaces for platform-specific functionality, allowing different implementations (MonoGame, SDL, custom renderers):

- **IRenderer** - Drawing primitives, sprites, text
- **IInputManager** - Keyboard and mouse input
- **IContentLoader** - Loading assets from disk
- **ITexture** - Texture representation

See [Kobold.Monogame](../monogame/) for the MonoGame implementation.

## Quick Reference

### Creating Entities

```csharp
// Create an entity with components
var entity = World.Create(
    new Transform { Position = new Vector2(100, 100) },
    new Velocity { Value = new Vector2(50, 0) },
    SpriteRenderer.FullTexture(texture)
);
```

### Adding Systems

```csharp
protected override void Initialize()
{
    base.Initialize();

    // Add update systems (order matters!)
    Systems.AddSystem(new PhysicsSystem(), 100, true);
    Systems.AddSystem(new CollisionSystem(collisionMatrix), 300, true);

    // Add render systems
    Systems.AddRenderSystem(new RenderSystem(Renderer));
}
```

### Querying Entities

```csharp
// Get all entities with Transform and Velocity
var query = World.Query<Transform, Velocity>();
foreach (var entity in query)
{
    ref var transform = ref entity.Get<Transform>();
    ref var velocity = ref entity.Get<Velocity>();

    // Modify components...
}
```

### Loading Assets

```csharp
// Load a texture
var texture = Assets.LoadTexture("player.png");

// Load a sprite sheet
var spriteSheet = Assets.LoadSpriteSheet("sprites.json");
var frame = spriteSheet.GetFrame(0);
```

### Publishing Events

```csharp
// Subscribe to events
Events.Subscribe<CollisionEvent>(OnCollision);

// Publish events
Events.Publish(new CustomEvent { Data = "something happened" });
```

## Namespaces

- `Kobold.Core` - Core classes (GameEngineBase, AssetManager, MathUtils)
- `Kobold.Core.Abstractions.Core` - Core interfaces
- `Kobold.Core.Abstractions.Rendering` - Rendering interfaces
- `Kobold.Core.Abstractions.Input` - Input interfaces
- `Kobold.Core.Components` - All component structs
- `Kobold.Core.Components.Core` - Core components (Transform, Camera, Tags)
- `Kobold.Core.Components.Physics` - Physics components
- `Kobold.Core.Components.Rendering` - Rendering components
- `Kobold.Core.Systems` - All system classes
- `Kobold.Core.Events` - Event system
- `Kobold.Core.Assets` - Asset management
- `Kobold.Core.Configuration` - Configuration classes
- `Kobold.Core.Factories` - Entity factories

## Dependencies

Kobold.Core depends on:
- **.NET 8.0** - Target framework
- **Arch 2.1.0** - High-performance ECS library

## Next Steps

- **[Architecture](architecture.md)** - Understand ECS in depth
- **[Game Engine](game-engine.md)** - Learn the game lifecycle
- **[Components](components.md)** - Explore all available components
- **[Systems](systems.md)** - Understand built-in systems
- **[Examples](../examples/code-snippets.md)** - See common patterns

---

**Ready to dive deeper?** Start with [Architecture](architecture.md) to understand how Kobold's ECS works.
