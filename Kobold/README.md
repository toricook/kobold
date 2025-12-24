# Kobold.Core

**Essential game engine foundations that every game needs.**

Kobold.Core provides the fundamental building blocks for creating games using an Entity Component System (ECS) architecture. This package contains only the core features that virtually every game requires, keeping it lightweight and focused.

## Philosophy

Kobold.Core follows the principle of **minimal essential functionality**:
- ✅ Include: Features that nearly every game needs
- ❌ Exclude: Specialized features that only some games require (those go in Kobold.Extensions)

## What's Included

### Core Architecture
- **Entity Component System (ECS)** - Built on Arch for high-performance entity management
- **Component System** - Transform, Velocity, Physics, and other fundamental components
- **System Manager** - Orchestrates game systems and update loops
- **Event Bus** - Decoupled communication between systems

### Essential Systems
- **PhysicsSystem** - Basic physics simulation (velocity, gravity, damping)
- **CollisionSystem** - Collision detection with layers and filtering
- **BoundarySystem** - Screen boundary handling (wrap, clamp, destroy)
- **DestructionSystem** - Entity lifecycle management
- **InputSystem** - Generic input handling
- **RenderSystem** - Rendering abstraction layer

### Core Components
- **Transform** - Position, rotation, scale
- **Velocity** - Movement and direction
- **Physics** - Mass, restitution, damping
- **BoxCollider** - AABB collision detection
- **Renderable** - Visual representation
- **Lifetime** - Timed entity destruction

### Utilities
- **MathUtils** - Common game math operations
- **EventBus** - Event-driven communication
- **Configuration** - Game and system configuration

## When to Use Kobold.Core

Use Kobold.Core when you need:
- A lightweight ECS game framework
- Basic physics and collision detection
- Input handling and rendering abstractions
- Event-driven architecture
- To build a simple 2D game with minimal dependencies

## When to Use Kobold.Extensions

If you need specialized features, check out **Kobold.Extensions**:
- **Tilemaps** - Grid-based level design
- **Particle Systems** - Visual effects
- **AI/Pathfinding** - Enemy behaviors
- **Dialogue Systems** - Conversations and narrative
- **Animation Systems** - Sprite animations
- And more...

## Design Principles

1. **Minimal Dependencies** - Only essential third-party packages
2. **Performance First** - Optimized for game update loops
3. **Extensible** - Easy to extend without modifying core code
4. **Well-Tested** - Comprehensive unit and system tests
5. **Platform Agnostic** - Rendering/input abstracted for any platform

## Architecture

```
Kobold.Core/
├── Abstractions/       # Interfaces for platform-specific implementations
├── Components/         # Core ECS components
│   ├── Core/          # Transform, Tags, GameState
│   ├── Physics/       # Velocity, Physics, Colliders
│   ├── Rendering/     # Renderable, RenderLayers
│   └── Gameplay/      # Lifetime, Boundary behaviors
├── Systems/           # Core game systems
├── Events/            # Event system and core events
├── Configuration/     # Configuration classes
└── MathUtils.cs       # Math utilities

Platform-Specific Implementations:
└── Kobold.Monogame/   # MonoGame rendering and input
```

## Example Usage

```csharp
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Systems;

// Create a simple game entity
var world = World.Create();
var eventBus = new EventBus();

var player = world.Create(
    new Transform(new Vector2(100, 100)),
    new Velocity(Vector2.Zero),
    new Physics { Mass = 1.0f },
    new BoxCollider(new Vector2(32, 32)),
    new PlayerControlled(speed: 200f)
);

// Set up core systems
var physics = new PhysicsSystem(world, new PhysicsConfig());
var collision = new CollisionSystem(world, eventBus);
var input = new InputSystem(inputManager, world);

// Game loop
void Update(float deltaTime)
{
    input.Update(deltaTime);
    physics.Update(deltaTime);
    collision.Update(deltaTime);
}
```

## Getting Started

1. **Install Kobold.Core**
   ```bash
   dotnet add package Kobold.Core
   ```

2. **Choose a platform implementation** (e.g., Kobold.Monogame)
   ```bash
   dotnet add package Kobold.Monogame
   ```

3. **Start building your game**
   - Create entities with core components
   - Add game systems to your update loop
   - Implement game-specific logic

## Related Packages

- **Kobold.Extensions** - Optional features for common game mechanics
- **Kobold.Monogame** - MonoGame platform implementation

## Contributing

Kobold.Core should remain focused on essential features. Before adding new features:
1. Ask: "Will virtually every game need this?"
2. If yes → Add to Kobold.Core
3. If no → Add to Kobold.Extensions
4. If unsure → Start in Extensions, promote later if widely used

## License

[Add your license here]
