# Cave Generation - Cellular Automata

Generate organic cave-like structures using cellular automata algorithms.

## How It Works

1. **Initialize:** Randomly place walls based on density
2. **Smooth:** Apply cellular automata rules (like Conway's Game of Life) for N iterations
3. **Connect:** Optionally connect separate cave regions with corridors
4. **Clean:** Remove small isolated regions

## Quick Start

```csharp
using Kobold.Procedural;
using Kobold.Extensions.Tilemaps;

// Create configuration
var config = CellularAutomataConfig.Cave();
config.Width = 64;
config.Height = 48;
config.Seed = 12345;  // For reproducible results

// Generate cave
var generator = new CellularAutomataGenerator(config);
var (tileMap, tileSet) = generator.GenerateWithTileSet(wallIsSolid: true);

// Use in game
var texture = Assets.LoadTexture("tiles.png");
World.Create(new TilemapComponent(tileMap, tileSet, texture));
```

## Configuration

### CellularAutomataConfig

```csharp
public class CellularAutomataConfig
{
    public int Width, Height;           // Map size in tiles
    public int Iterations;              // Smoothing passes (4-6 recommended)
    public float InitialWallProbability; // Starting density (0.4-0.5 for caves)
    public int BirthThreshold;          // Neighbors to become wall (4)
    public int DeathThreshold;          // Neighbors to stay wall (3)
    public int Seed;                    // Random seed (0 = random)
    public int WallTileId, FloorTileId; // Tile IDs
    public int TileWidth, TileHeight;   // Pixel dimensions
    public bool EdgeIsWall;             // Treat boundaries as walls
    public bool ConnectCaves;           // Connect separate regions
    public int MinCaveSize;             // Minimum tiles to keep region
}
```

### Presets

```csharp
// Organic caves (recommended)
var config = CellularAutomataConfig.Cave();

// Open areas with sparse obstacles
var config = CellularAutomataConfig.OpenArea();

// Dense maze-like patterns
var config = CellularAutomataConfig.Maze();
```

## Examples

### Basic Cave

```csharp
var config = new CellularAutomataConfig
{
    Width = 48,
    Height = 32,
    TileWidth = 32,
    TileHeight = 32,
    Iterations = 5,
    InitialWallProbability = 0.45f,
    BirthThreshold = 4,
    DeathThreshold = 3,
    ConnectCaves = true,
    MinCaveSize = 50
};

var generator = new CellularAutomataGenerator(config);
var (tileMap, tileSet) = generator.GenerateWithTileSet();
```

### Reproducible Generation

```csharp
var config = CellularAutomataConfig.Cave();
config.Seed = 42;  // Same seed = same cave

var gen1 = new CellularAutomataGenerator(config);
var gen2 = new CellularAutomataGenerator(config);

// Both generate identical caves
```

### Large Open Cave

```csharp
var config = new CellularAutomataConfig
{
    Width = 80,
    Height = 60,
    Iterations = 6,
    InitialWallProbability = 0.35f,  // Less dense
    ConnectCaves = true
};
```

## Complete Example

See [CaveExplorer](../examples/) game for a complete implementation with player movement, tilemap collision, and camera following.

## Tips

- **More iterations** = smoother, rounder caves
- **Higher density** (0.5+) = smaller caves, more walls
- **Lower density** (0.3-0.4) = larger open caves
- **ConnectCaves = true** ensures all areas reachable
- **MinCaveSize** removes tiny disconnected rooms

## See Also

- **[Tilemaps](../extensions/tilemaps.md)** - Using generated tilemaps
- **[CaveExplorer Example](../examples/)** - Complete game

---

**Back:** [Procedural Overview](index.md) ←
