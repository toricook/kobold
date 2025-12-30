# Getting Started with Kobold

Welcome to Kobold! This guide will help you install the framework, understand the basics, and build your first game.

## What is Kobold?

Kobold is a 2D game framework built on the Entity Component System (ECS) architecture and MonoGame. It provides:

- High-performance ECS using the Arch library
- Built-in physics and collision systems
- Sprite rendering and animation
- Asset management and sprite sheet support
- Event-driven architecture
- Platform abstraction layer

## Prerequisites

Before you start, make sure you have:

- **.NET 8 SDK** or later - [Download here](https://dotnet.microsoft.com/download)
- **MonoGame** - Installed via NuGet (handled automatically)
- **A code editor** - Visual Studio, VS Code, or Rider
- **Basic C# knowledge** - Familiarity with C# syntax and OOP concepts

## Quick Navigation

Choose your path:

### [Installation](installation.md)
Set up Kobold and create your first project. Start here if you're brand new to Kobold.

### [Quick Start](quick-start.md)
A 5-minute tutorial that gets you up and running with a minimal working example. Perfect if you learn by doing.

### [Your First Game](your-first-game.md)
Build a complete simple game from scratch. Learn the fundamentals through a hands-on tutorial.

## Installation Overview

Kobold is distributed as NuGet packages:

```bash
# Core framework (required)
dotnet add package Kobold.Core

# MonoGame integration (required for rendering)
dotnet add package Kobold.Monogame

# Optional extensions
dotnet add package Kobold.Extensions  # Tilemaps and more
dotnet add package Kobold.Procedural  # Procedural generation
```

Or reference the projects directly if working from source:

```xml
<ItemGroup>
  <ProjectReference Include="..\Kobold\Kobold.csproj" />
  <ProjectReference Include="..\Kobold.Monogame\Kobold.Monogame.csproj" />
</ItemGroup>
```

## Your First Kobold Game

Here's the minimal code to create a Kobold game:

```csharp
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using Kobold.Monogame;
using System.Numerics;
using System.Drawing;

public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // Create a simple rectangle entity
        World.Create(
            new Transform { Position = new Vector2(400, 300) },
            new RectangleRenderer
            {
                Size = new Vector2(50, 50),
                Color = Color.Green
            }
        );

        // Add the render system
        Systems.AddRenderSystem(new RenderSystem(Renderer));
    }
}

// Program.cs
MonoGameHost.Run(new MyGame(), new GameConfig
{
    WindowWidth = 800,
    WindowHeight = 600,
    WindowTitle = "My First Kobold Game"
});
```

This creates a window with a green square in the center. Simple!

## Key Concepts

### Entity Component System (ECS)

Kobold uses ECS architecture where:

- **Entities** are just IDs (created with `World.Create()`)
- **Components** are data structs (like `Transform`, `Velocity`)
- **Systems** contain logic (like `PhysicsSystem`, `RenderSystem`)

This separation makes games performant, modular, and easy to reason about.

### Game Lifecycle

Every Kobold game extends `GameEngineBase` and follows this lifecycle:

1. **Initialize()** - Set up your game, create initial entities, add systems
2. **Update(deltaTime)** - Called every frame, updates all systems
3. **Render()** - Called every frame, renders all entities
4. **Shutdown()** - Clean up resources when the game closes

### The World

The `World` property is your ECS world from the Arch library. Use it to:

- Create entities: `World.Create(component1, component2, ...)`
- Query entities: `World.Query(...)`
- Destroy entities: `World.Destroy(entity)`

### Systems

Systems update entities each frame. Add them in `Initialize()`:

```csharp
// Update systems (called during Update)
Systems.AddSystem(new PhysicsSystem(), order: 100, requiresGameplayState: true);

// Render systems (called during Render)
Systems.AddRenderSystem(new RenderSystem(Renderer));
```

## Next Steps

1. **[Installation](installation.md)** - Detailed installation guide
2. **[Quick Start](quick-start.md)** - 5-minute hands-on tutorial
3. **[Your First Game](your-first-game.md)** - Build a complete game
4. **[Core Architecture](../core/architecture.md)** - Deep dive into ECS

## Need Help?

- Check the [examples](../examples/code-snippets.md) for common patterns
- Review the [core documentation](../core/) for API reference
- See example games: Asteroids, Pong, CaveExplorer

---

**Ready to install?** Continue to [Installation](installation.md) →
