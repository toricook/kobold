# Kobold Core

The core game engine library built on [Arch ECS](https://github.com/genaray/Arch).

## Architecture Philosophy

Kobold is designed to be **platform-agnostic** with clear separation between game logic and platform implementations.

### Core Layer (This Project)
Contains platform-independent game logic that works anywhere:
- **ECS Foundation** - Entity management, component definitions, system execution
- **Core Systems** - Physics, collision, game state, events, asset management
- **Game Features** - Tilemap, pathfinding, inventory, character stats

### Platform Layer (Kobold.Monogame, etc.)
Platform-specific implementations:
- Rendering (sprites, tilemaps, camera, particles, UI)
- Input (keyboard, mouse, gamepad)
- Audio (sound effects, music)
- Asset Loading (textures, audio files)

**Principle:** Any dependency on external libraries (like MonoGame) must be abstracted. Core code has no MonoGame references.

## Project Structure

```
Core/                    # Platform-agnostic engine core
├── ECS/                # ECS-specific code (systems, components, queries)
├── Physics/            # Collision detection, spatial partitioning
├── GameSystems/        # Game-specific systems (inventory, stats, etc.)
└── Events/             # Event system
```

## Why This Architecture?

**Platform Independence**: Core logic can be unit tested without rendering, and rendering implementations can be swapped.

**ECS Benefits**: Using Arch gives us high performance through data-oriented design, flexible entity composition, and clean separation of data and behavior.

For detailed API documentation, see the [generated docs](https://toricook.github.io/Kobold/).
