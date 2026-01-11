---
_disableToc: false
---

# Kobold Game Framework

A lightweight, platform-agnostic ECS (Entity Component System) game framework for .NET, built on [Arch ECS](https://github.com/genaray/Arch).

## What is Kobold?

Kobold is designed for rapid 2D game development with clean architecture. It provides:

- **Platform-Agnostic Core** - Game logic independent of rendering/input implementation
- **ECS Architecture** - High-performance entity component system using Arch
- **MonoGame Integration** - Full rendering, input, and audio support
- **Extension Libraries** - Tilemaps, save/load, UI, and more
- **Self-Documenting** - Comprehensive XML comments for API documentation

## Quick Example

```csharp
using Kobold;
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using Kobold.Monogame;
using System.Numerics;

public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // Create a player entity
        World.Create(
            new Transform { Position = new Vector2(400, 300) },
            new Velocity { Value = Vector2.Zero },
            new BoxCollider { Size = new Vector2(32, 32) }
        );

        // Add systems
        Systems.AddSystem(new PhysicsSystem(World));
        Systems.AddRenderSystem(new RenderSystem(Renderer));
    }
}

// Run the game
MonoGameHost.Run(new MyGame(), new GameConfig
{
    WindowWidth = 800,
    WindowHeight = 600,
    WindowTitle = "My Kobold Game"
});
```

## Key Features

### 🎮 Core Engine
- Entity-Component-System architecture
- Physics and collision detection
- Event system
- Asset management
- Input handling abstraction
- Rendering abstraction

### 🔧 Extensions
- **Tilemaps** - Grid-based level design with [Tiled](https://www.mapeditor.org/) support
- **Save/Load** - Automatic serialization with compression
- **UI Framework** - Wireframe prototyping to sprite-based UI
- **More coming** - Dialogue, pathfinding, particles, etc.

### 🖥️ Platform Support
- **MonoGame** - Desktop (Windows, macOS, Linux)
- **Extensible** - Build your own platform implementations

## Documentation

### Guides
- [Architecture Philosophy](guides/architecture.md) - Understanding Kobold's design
- [Extensions Overview](guides/extensions.md) - Optional features and when to use them
- [Tiled Map Integration](guides/tiled-integration.md) - Creating maps with Tiled Map Editor

### API Reference
Browse the complete API documentation:
- [Kobold (Core)](api/Kobold.html) - Platform-agnostic engine
- [Kobold.Monogame](api/Kobold.Monogame.html) - MonoGame implementation
- [Kobold.Extensions](api/Kobold.Extensions.html) - Optional features

## Getting Started

### Installation

```bash
# Create a new project
dotnet new console -n MyGame
cd MyGame

# Add Kobold packages
dotnet add package Kobold
dotnet add package Kobold.Monogame
dotnet add package Kobold.Extensions
```

### Your First Game

See the [Demo project](https://github.com/toricook/Kobold/tree/main/Demo) for a complete example with Tiled map loading.

## Philosophy

Kobold follows these principles:

1. **Platform Agnostic** - Core logic has no rendering dependencies
2. **ECS First** - Built on Arch for performance and flexibility
3. **Self-Documenting** - XML comments provide comprehensive API docs
4. **Minimal Documentation** - Code should be clear; guides only for workflows (like Tiled integration)

## Contributing

Contributions are welcome! Check out the [repository](https://github.com/toricook/Kobold) to get started.

## License

Copyright © 2025 Kobold Game Framework
