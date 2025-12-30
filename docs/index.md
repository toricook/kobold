---
layout: default
title: Home
nav_order: 1
description: "Kobold is a lightweight ECS game framework with platform-agnostic core"
permalink: /
---

# Kobold Game Framework
{: .fs-9 }

A lightweight, high-performance 2D game framework built on the Entity Component System (ECS) architecture.
{: .fs-6 .fw-300 }

[Get Started](getting-started/installation){: .btn .btn-primary .fs-5 .mb-4 .mb-md-0 .mr-2 }
[View on GitHub](https://github.com/toricook/Kobold){: .btn .fs-5 .mb-4 .mb-md-0 }

---

**Kobold** is a lightweight, high-performance 2D game framework built on the Entity Component System (ECS) architecture. The core framework is **platform-agnostic**, with rendering and platform-specific functionality abstracted behind interfaces. While MonoGame is the primary and recommended platform host, the architecture allows for alternative implementations or even integration with other engines.

## Key Features

- **Entity Component System (ECS)** - Built on the high-performance [Arch](https://github.com/genaray/Arch) library for fast, cache-friendly game logic
- **Platform-Agnostic Core** - Zero platform dependencies in Kobold.Core. All rendering and platform functionality abstracted behind interfaces (`ITexture`, `IRenderer`)
- **MonoGame Integration** - Complete MonoGame host implementation (Kobold.Monogame) provides first-class support with clean platform abstractions
- **Physics & Collision** - Built-in 2D physics system with AABB collision detection and configurable collision layers
- **Tilemap Support** - Complete tilemap system for grid-based games (platformers, RPGs, roguelikes)
- **Procedural Generation** - Cellular automata-based cave and level generation
- **Asset Management** - Automatic texture caching and sprite sheet support with JSON configurations
- **Event System** - Decoupled pub/sub event bus for clean game architecture
- **Extensible** - Clear separation between core framework and optional extensions

## Quick Example

Here's a minimal Kobold game that displays a moving sprite:

```csharp
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Monogame;
using System.Numerics;

public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // Create a player entity with transform, sprite, and velocity
        var playerTexture = Assets.LoadTexture("player.png");
        World.Create(
            new Transform { Position = new Vector2(400, 300) },
            SpriteRenderer.FullTexture(playerTexture),
            new Velocity { Value = new Vector2(100, 0) }
        );

        // Add physics system to move entities
        Systems.AddSystem(new PhysicsSystem(), 100, true);
        Systems.AddRenderSystem(new RenderSystem(Renderer));
    }
}

// Launch the game
MonoGameHost.Run(new MyGame(), new GameConfig
{
    WindowWidth = 800,
    WindowHeight = 600,
    WindowTitle = "My Kobold Game"
});
```

## Documentation Sections

### [Getting Started](getting-started/)
New to Kobold? Start here to install the framework and build your first game.
- [Installation](getting-started/installation.md) - Install Kobold and set up your project
- [Quick Start](getting-started/quick-start.md) - 5-minute tutorial to get running
- [Your First Game](getting-started/your-first-game.md) - Build a complete simple game

### [Core Framework](core/)
Learn about the Kobold.Core library - the heart of the framework.
- [Architecture](core/architecture.md) - Understanding ECS and Kobold's design
- [Game Engine](core/game-engine.md) - GameEngineBase and game lifecycle
- [Components](core/components.md) - Transform, Velocity, Renderers, and more
- [Systems](core/systems.md) - Physics, Collision, Rendering, Animation
- [Asset Manager](core/asset-manager.md) - Loading and managing textures
- [Event Bus](core/event-bus.md) - Pub/sub event system
- [Utilities](core/utilities.md) - MathUtils and helper functions

### [MonoGame Integration](monogame/)
How Kobold integrates with MonoGame for rendering and input.
- [Overview](monogame/index.md) - MonoGame platform implementation
- [Setup](monogame/setup.md) - Setting up MonoGame projects with Kobold

### [Extensions](extensions/)
Optional framework extensions for specialized functionality.
- [Tilemaps](extensions/tilemaps.md) - Grid-based level design and collision

### [Procedural Generation](procedural/)
Generate procedural content for your games.
- [Cave Generation](procedural/cave-generation.md) - Cellular automata algorithm

### [Tools](tools/)
Development tools to streamline your workflow.
- [Sprite Sheet Editor](tools/sprite-sheet-editor.md) - Visual sprite sheet configuration
- [Experiments Sandbox](tools/experiments.md) - Interactive feature testing

### [Guides](guides/)
Practical how-to guides and best practices.
- [Physics Configuration](guides/physics.md) - Set up physics for different game types
- [Collision Layers](guides/collision.md) - Manage collision filtering
- [Input Handling](guides/input.md) - Handle keyboard and mouse input
- [Camera Systems](guides/camera.md) - Camera following and bounds

### [Examples](examples/)
Working examples and code snippets.
- [Code Snippets](examples/code-snippets.md) - Common patterns and recipes
- [Example Projects](examples/) - Asteroids, Pong, CaveExplorer walkthroughs

## Framework Architecture

Kobold is designed around a **platform-agnostic core** that abstracts rendering and platform-specific functionality. While MonoGame is the current (and primary) implementation, the architecture allows for alternative platform hosts.

### Design Vision: Platform Abstraction

**Kobold.Core** defines interfaces and abstractions for rendering, input, and content loading - it has no dependencies on any specific platform or rendering engine. This means:

- The core ECS framework, components, and game logic are **completely platform-independent**
- Rendering uses abstract interfaces (`ITexture`, `IRenderer`) rather than concrete platform types
- In theory, you could implement a custom platform host (Raylib, custom renderer, etc.) or potentially even Unity
- The framework forces clean separation between game logic (platform-agnostic) and rendering (platform-specific)

**Kobold.Monogame** is the reference implementation that provides MonoGame-specific rendering, input, and content loading. It's well-tested and recommended for most projects, but it's just one possible host.

### Package Structure

```
Kobold.Core           (Platform-agnostic foundation)
    │
    ├── Kobold.Monogame      (MonoGame platform host)
    │
    ├── Kobold.Extensions    (Optional features)
    │   └── Tilemaps
    │
    └── Kobold.Procedural    (Optional generators)
        └── Cellular Automata

Tools:
    ├── Kobold.SpriteSheetEditor
    └── Kobold.Experiments
```

**Kobold.Core** provides the essential framework - ECS, components, systems, platform abstractions (`ITexture`, `IRenderer`), and utilities. Zero platform dependencies.

**Kobold.Monogame** implements the platform layer (`ITexture`, `IRenderer`, input, content loading) using MonoGame. This is the standard way to use Kobold.

**Kobold.Extensions** contains optional systems like tilemaps that are common but not universally needed.

**Kobold.Procedural** provides procedural generation algorithms for creating dynamic content.

## Why Kobold?

**Performance** - ECS architecture with the Arch library provides cache-friendly, data-oriented performance ideal for games.

**Simplicity** - Clean APIs and sensible defaults let you focus on your game, not boilerplate.

**Platform Abstraction** - Core framework has zero platform dependencies. Rendering, input, and content loading are abstracted behind interfaces (`ITexture`, `IRenderer`), allowing custom platform implementations beyond MonoGame.

**Flexibility** - Clean separation between platform-agnostic game logic and platform-specific rendering. Core/Extensions split keeps the framework lean. Easy to extend or replace components.

**MonoGame Ready** - Kobold.Monogame provides a complete, well-tested MonoGame implementation that feels natural to MonoGame developers.

**Modern C#** - Uses .NET 8 and modern C# features for clean, expressive code.

## Example Projects

The repository includes several example games demonstrating Kobold features:

- **Asteroids** - Classic arcade game showing physics, collision, screen wrapping
- **Pong** - Simple paddle game demonstrating input and collision
- **CaveExplorer** - Procedural cave generation with tilemap collision

## License

MIT License - See repository for details.

## Getting Help

- [GitHub Issues](https://github.com/toricook/Kobold/issues) - Bug reports and feature requests
- [Documentation](https://toricook.github.io/Kobold/) - Full framework documentation

---

**Ready to start?** Head to [Getting Started](getting-started/) to build your first Kobold game!
