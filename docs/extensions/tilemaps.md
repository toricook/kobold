# Tilemaps - Grid-Based Level Design

The Tilemaps system provides grid-based level design, rendering, and collision for 2D games.

## Overview

**TileMap** - Grid data structure storing tile IDs
**TileSet** - Tile metadata (visuals, properties, collision)
**TilemapComponent** - ECS component attaching tilemap to entity
**TilemapSystem** - Updates animations
**TilemapCollisionSystem** - Handles tile-entity collision

## Quick Start

```csharp
// Create tilemap
var tileMap = new TileMap(
    width: 32,      // 32 tiles wide
    height: 24,     // 24 tiles tall
    tileWidth: 32,  // 32 pixels per tile
    tileHeight: 32,
    layerCount: 2   // 2 layers (background + foreground)
);

// Create tileset
var tileSet = TileSet.FromTexture(
    texturePath: "tiles.png",
    tileWidth: 32,
    tileHeight: 32,
    spacing: 0,
    margin: 0
);

// Configure tile properties
tileSet.SetTileProperties(1, new TileProperties { IsSolid = true });
tileSet.SetTileProperties(2, new TileProperties
{
    IsSolid = false,
    CollisionLayer = TileCollisionLayer.Water
});

// Fill tilemap
tileMap.Fill(layer: 0, x: 0, y: 0, width: 32, height: 24, tileId: 0);  // Background
tileMap.SetTile(layer: 1, x: 5, y: 10, tileId: 1);  // Place solid tile

// Create entity
var texture = Assets.LoadTexture("tiles.png");
World.Create(new TilemapComponent(tileMap, tileSet, texture));

// Add systems
Systems.AddSystem(new TilemapSystem(), 100, true);
Systems.AddSystem(new TilemapCollisionSystem(), 310, true);
Systems.AddRenderSystem(new TilemapRenderSystem(Renderer));
```

## TileMap API

**Constructor:**
```csharp
TileMap(int width, int height, int tileWidth, int tileHeight, int layerCount = 1)
```

**Methods:**
- `GetTile(layer, x, y)` - Get tile ID at position
- `SetTile(layer, x, y, tileId)` - Set tile at position
- `Fill(layer, x, y, width, height, tileId)` - Fill region
- `WorldToTile(worldX, worldY)` - Convert world coords to tile coords
- `TileToWorld(tileX, tileY)` - Convert tile coords to world coords
- `GetTilesInBounds(layer, x, y, width, height)` - Query tiles in region
- `Clone()` - Create a copy

## TileSet API

**Factory:**
```csharp
TileSet.FromTexture(texturePath, tileWidth, tileHeight, spacing, margin)
```

**Methods:**
- `SetTileProperties(tileId, properties)` - Configure tile behavior
- `GetTileProperties(tileId)` - Query tile properties
- `IsSolid(tileId)` - Quick solid check
- `GetTileSourceRect(tileId, tilesPerRow)` - Calculate sprite sheet position
- `SetTileRange(startId, count, properties)` - Configure multiple tiles

## TileProperties

```csharp
public struct TileProperties
{
    public bool IsSolid;                  // Blocks movement
    public TileCollisionLayer CollisionLayer;  // Collision type
    public float Friction;                // Movement resistance
    public int Damage;                    // Harm to entities
    public string Type;                   // Custom identifier
    public bool IsAnimated;               // Animated tile
    public int[] AnimationFrames;         // Frame tile IDs
    public float AnimationSpeed;          // Frames per second
    public Dictionary<string, object> CustomProperties;  // Game-specific data
}
```

**CollisionLayers:**
- `Solid` - Blocks all movement
- `Platform` - One-way platform (land from above)
- `Trigger` - Passes through, publishes event
- `Water` - Special behavior
- `Ice` - Low friction
- `Ladder` - Climbable

## Tilemap Collision

**TilemapCollisionSystem** handles entity-tile collision:

```csharp
// Automatically resolves collisions with solid tiles
// Publishes events for triggers
Events.Subscribe<TileTriggerEvent>(e =>
{
    Console.WriteLine($"Entity {e.Entity} entered trigger tile at {e.TileX}, {e.TileY}");
});

Events.Subscribe<TileDamageEvent>(e =>
{
    // Entity took damage from tile
});
```

## Complete Example

See [CaveExplorer](../examples/) game for a complete tilemap + procedural generation example.

## See Also

- **[Procedural Generation](../procedural/cave-generation.md)** - Generate tilemaps procedurally
- **[Collision System](../core/systems.md#collisionsystem)** - Entity collision

---

**Next:** [Procedural Generation](../procedural/) →
