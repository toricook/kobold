# Kobold Game Framework

A lightweight, platform-agnostic ECS (Entity Component System) game framework for .NET.

## Quick Links

- [Getting Started](Kobold.Documentation/docs/getting-started/index.md)
- [Core Concepts](Kobold.Documentation/docs/core/index.md)
- [API Reference](api/index.md)

## What is Kobold?

Kobold is a modular game framework built on the Arch ECS library, designed to be simple, flexible, and platform-agnostic. It provides:

- **Core Framework** - Platform-agnostic game engine base
- **MonoGame Integration** - Full MonoGame rendering and input support
- **Extension Libraries** - Tilemaps, procedural generation, and more
- **Development Tools** - Sprite sheet editor and experimentation tools

## Features

- Entity Component System architecture using Arch
- Flexible rendering abstraction
- Input management system
- Physics and collision detection
- Event bus for game events
- Asset management
- Tilemap support
- Procedural generation tools

## Getting Started

```csharp
// Create a simple game
public class MyGame : GameEngineBase
{
    public override void Initialize()
    {
        base.Initialize();
        // Your initialization code
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        // Your update logic
    }

    public override void Render()
    {
        base.Render();
        // Rendering handled by systems
    }
}
```

Check out the [Getting Started Guide](Kobold.Documentation/docs/getting-started/index.md) for more details.

## License

Copyright © 2025 Kobold Game Framework
