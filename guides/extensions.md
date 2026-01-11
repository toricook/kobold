---
title: Extensions Philosophy
---

# Kobold.Extensions

Optional game features that many games use, but not all.

## Philosophy

Kobold.Extensions provides specialized systems and components that are common in games but not universally required:

- ✅ **Include:** Features many games use but aren't strictly necessary
- ✅ **Include:** Specialized systems that would bloat Kobold.Core
- ❌ **Exclude:** Game-specific logic (that stays in your game project)

## What's Included

### 🗺️ Tilemaps

Grid-based level design and rendering system.

- Multi-layer tilemaps
- Tileset management with tile properties
- Collision detection (solid, platform, trigger)
- Animated tiles
- **[Tiled Map Editor Integration Guide](tiled-integration.md)**

### 💾 Save/Load System

Flexible save system with automatic serialization.

- Automatic serialization of entities and components
- Multiple save slots with metadata
- Auto-save functionality
- GZip compression

### 🎨 UI Framework

Wireframe-first UI system for quick prototyping.

- Interactive buttons with hover/pressed states
- Screen anchoring
- Event-driven interactions
- Easy transition to sprite-based UI

## When to Use Extensions vs Core

| Feature | Location |
|---------|----------|
| Transform, Velocity | **Core** - Every game needs entities to move |
| Collision Detection | **Core** - Most games need collision |
| Tilemaps | **Extensions** - Not all games use tile-based levels |
| Dialogue System | **Extensions** - Only narrative games need this |

## Installation

```bash
dotnet add package Kobold.Extensions
```

Or reference in `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\Kobold.Extensions\Kobold.Extensions.csproj" />
</ItemGroup>
```
