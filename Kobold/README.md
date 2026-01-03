# Game Engine

A lightweight 2D game engine built on [Arch ECS](https://github.com/genaray/Arch), designed for rapid development of top-down RPGs, roguelikes, and platformers.

## Vision

This engine prioritizes getting games shipped quickly while maintaining clean architecture. It's built for personal projects but designed to be extensible as needs grow.

### Design Philosophy

- **ECS-First**: Built on Arch for performance and flexibility
- **Platform Agnostic Core**: Game logic independent of rendering/input implementation
- **Pragmatic Abstractions**: MonoGame handles rendering, input, and audio initially - but the architecture allows swapping implementations later
- **Feature-Driven**: Focused on features needed for 2D action RPGs, roguelikes, and platformers rather than general-purpose everything

## Architecture Overview

The engine is split into distinct layers:

### Core Layer (Platform Agnostic)
The heart of the engine - pure game logic with no platform dependencies.

- **ECS Foundation** (Arch)
  - Entity management
  - Component definitions
  - System execution pipeline
  
- **Core Systems**
  - Physics/collision detection
  - Game state management
  - Event system
  - Serialization
  - Asset management (metadata/references only)

- **Game-Specific Features**
  - Tilemap/grid system
  - Pathfinding
  - Inventory system
  - Character stats/progression
  - Turn-based combat system (for roguelikes)
  - Platformer physics (jump curves, slopes, etc.)

### Platform Layer (MonoGame Implementation - for now)
Concrete implementations of platform-specific functionality.

- **Rendering**
  - Sprite rendering
  - Tilemap rendering
  - Camera system
  - Particle effects
  - UI rendering

- **Input Management**
  - Keyboard/mouse input
  - Gamepad support
  - Input mapping/rebinding

- **Audio**
  - Sound effect playback
  - Music management
  - Audio mixing

- **Asset Loading**
  - Texture loading
  - Audio file loading
  - Data file loading

### Future: Custom Implementation Layer
Eventually, replace MonoGame components with custom implementations:
- Custom 2D renderer (potentially using modern graphics APIs)
- Custom audio engine
- Custom input system
- Cross-platform support beyond what MonoGame offers

## Technology Stack

- **Language**: C# (.NET)
- **ECS Framework**: [Arch](https://github.com/genaray/Arch)
- **Platform Layer**: MonoGame (initially)
- **Target Platforms**: Desktop (Windows/Mac/Linux) initially

## Project Structure

```
/Core                    # Platform-agnostic engine core
  /ECS                   # ECS-specific code (systems, components, queries)
  /Physics               # Collision detection, spatial partitioning
  /GameSystems           # Game-specific systems (inventory, stats, etc.)
  /Grid                  # Tilemap and grid-based logic
  /Events                # Event system
  /Serialization         # Save/load functionality

/Platform                # Platform-specific implementations
  /MonoGame              # MonoGame implementation
    /Rendering           # Rendering systems
    /Input               # Input handling
    /Audio               # Audio playback
    /Assets              # Asset loading

/Games                   # Actual game projects using the engine
  /ExampleRoguelike      # Sample roguelike game
  /ExamplePlatformer     # Sample platformer game
```

## Development Approach

1. **Core-First Development**: Implement game logic in Core layer without rendering concerns
2. **Thin Platform Layer**: Keep MonoGame layer as thin as possible - just adapters
3. **Test Core Independently**: Core systems should be testable without graphics
4. **Incremental Migration**: As custom implementations are built, swap them in without changing Core

## Current Status

🚧 **In Development** - Architecture being established, migrating existing code to match this structure.

## Getting Started

*(Coming soon - setup instructions, first game tutorial)*

## Why This Architecture?

**Platform Independence**: By keeping core logic separate, we can:
- Unit test game systems without rendering
- Swap rendering implementations (MonoGame → custom renderer)
- Potentially support different platforms/engines

**ECS Benefits**: Using Arch gives us:
- High performance through data-oriented design
- Flexible entity composition
- Easy parallel system execution
- Clean separation of data and behavior

**Practical Complexity**: MonoGame handles the hard parts (graphics, audio, windowing) so we can focus on game-specific features, but the architecture allows replacing it when needed.

## License

*(Your license here)*