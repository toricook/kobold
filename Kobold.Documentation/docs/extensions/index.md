# Kobold.Extensions - Optional Framework Features

**Kobold.Extensions** provides specialized systems and components that are common in many games but not universally required.

## Philosophy

The core Kobold framework (Kobold.Core) is intentionally minimal. Extensions provide optional features that would bloat the core but are useful for specific game types.

## Current Extensions

### [Tilemaps](tilemaps.md)

Grid-based level design and rendering system.

**Use for:**
- Platformers
- RPGs and roguelikes
- Puzzle games
- Top-down games
- Strategy games

**Features:**
- Multi-layer tilemaps
- Tile properties (solid, animated, damage, etc.)
- Tilemap collision system
- Viewport culling for performance
- Integration with procedural generation

**Quick Example:**
```csharp
var tileMap = new TileMap(width: 32, height: 24, tileWidth: 32, tileHeight: 32);
var tileSet = TileSet.FromTexture(texture, 32, 32);

// Set tiles
tileMap.SetTile(layer: 0, x: 5, y: 10, tileId: 1);

// Create tilemap entity
World.Create(new TilemapComponent(tileMap, tileSet, texture));

// Add tilemap systems
Systems.AddSystem(new TilemapSystem(), 100, true);
Systems.AddSystem(new TilemapCollisionSystem(), 310, true);
```

## Future Extensions

Potential future extensions (not yet implemented):
- Particle systems
- UI framework
- Pathfinding/navigation
- AI behavior trees
- Audio management
- Save/load system

## Installation

```bash
dotnet add package Kobold.Extensions
```

Or reference the project:
```xml
<ProjectReference Include="..\Kobold.Extensions\Kobold.Extensions.csproj" />
```

## See Also

- **[Tilemaps Guide](tilemaps.md)** - Complete tilemap documentation
- **[Procedural Generation](../procedural/)** - Works great with tilemaps

---

**Next:** [Tilemaps](tilemaps.md) - Grid-based level design →
