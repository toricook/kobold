# Kobold.Procedural - Procedural Generation

**Kobold.Procedural** provides algorithms for generating procedural game content.

## Overview

Procedural generation creates unique, dynamic content algorithmically rather than hand-crafting every level. Kobold.Procedural currently provides cellular automata-based cave generation.

## Current Generators

### [Cellular Automata Cave Generator](cave-generation.md)

Generates organic cave-like structures using cellular automata algorithms.

**Features:**
- Configurable parameters (density, smoothing, size)
- Cave connection (ensures all areas reachable)
- Small region removal
- Preset configurations (Cave, OpenArea, Maze)
- Outputs TileMap compatible with Kobold.Extensions

**Quick Example:**
```csharp
var config = CellularAutomataConfig.Cave();
config.Width = 64;
config.Height = 48;
config.ConnectCaves = true;

var generator = new CellularAutomataGenerator(config);
var (tileMap, tileSet) = generator.GenerateWithTileSet(wallIsSolid: true);

// Use the generated tilemap
var texture = Assets.LoadTexture("tiles.png");
World.Create(new TilemapComponent(tileMap, tileSet, texture));
```

## Installation

```bash
dotnet add package Kobold.Procedural
```

Or reference the project:
```xml
<ProjectReference Include="..\Kobold.Procedural\Kobold.Procedural.csproj" />
```

## Dependencies

- **Kobold.Extensions** - Outputs TileMap/TileSet

## Future Generators

Potential future additions:
- Dungeon generation (rooms + corridors)
- Noise-based terrain
- Wave Function Collapse
- L-systems for organic structures

## See Also

- **[Cave Generation Guide](cave-generation.md)** - Detailed cellular automata tutorial
- **[Tilemaps](../extensions/tilemaps.md)** - Using generated tilemaps
- **[CaveExplorer Example](../examples/)** - Complete example game

---

**Next:** [Cave Generation](cave-generation.md) - Learn the algorithm →
