---
title: Architecture Philosophy
---

# Architecture Philosophy

Kobold is designed to be **platform-agnostic** with clear separation between game logic and platform implementations.

## Core Layer (Kobold)

Contains platform-independent game logic that works anywhere:

- **ECS Foundation** - Entity management, component definitions, system execution
- **Core Systems** - Physics, collision, game state, events, asset management
- **Game Features** - Tilemap, pathfinding, inventory, character stats

## Platform Layer (Kobold.Monogame, etc.)

Platform-specific implementations:

- **Rendering** - Sprites, tilemaps, camera, particles, UI
- **Input** - Keyboard, mouse, gamepad
- **Audio** - Sound effects, music
- **Asset Loading** - Textures, audio files

## Key Principle

**Any dependency on external libraries (like MonoGame) must be abstracted.**

This means:
- Core code has no MonoGame references
- Core logic can be unit tested without rendering
- Rendering implementations can be swapped

## Project Structure

```
Kobold/                  # Platform-agnostic engine core
├── ECS/                # ECS-specific code (systems, components, queries)
├── Physics/            # Collision detection, spatial partitioning
├── GameSystems/        # Game-specific systems (inventory, stats, etc.)
└── Events/             # Event system

Kobold.Monogame/        # MonoGame implementation
├── Rendering/          # Rendering systems
├── Input/              # Input handling
└── Audio/              # Audio playback

Kobold.Extensions/      # Optional features
├── Tilemaps/           # Grid-based levels
├── SaveSystem/         # Save/load functionality
└── UI/                 # UI framework
```

## Why This Architecture?

**Platform Independence**: Core logic can be unit tested without rendering, and rendering implementations can be swapped.

**ECS Benefits**: Using [Arch](https://github.com/genaray/Arch) gives us:
- High performance through data-oriented design
- Flexible entity composition
- Clean separation of data and behavior
