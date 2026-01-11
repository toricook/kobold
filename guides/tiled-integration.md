---
title: Tiled Map Integration
---

# Tiled Map Integration

Guide for creating Tiled maps for the Kobold engine.

## Directory Structure

Organize your Tiled assets in your project's `Assets` folder:

```
Assets/
├── your-map.tmx          # Tiled map file
├── tileset.tsx           # External tileset file
├── tileset.png           # Tileset texture
└── other-tileset.png     # Additional tilesets
```

The loader will automatically find .tsx and .png files relative to the .tmx file location.

## Creating a Map

1. Create a new map in Tiled (File → New → New Map)
2. Set tile size and map dimensions
3. Create external tilesets (.tsx) for reusability
4. Add tile layers for terrain, decorations, etc.
5. Add object layers for collisions, portals, and spawn points
6. Save as .tmx and place in your Assets folder

## Object Layer Properties

Object layers define game entities like collisions, portals, and spawn points. Configure them using custom properties.

### Layer-Level Properties

Set these on the object layer to apply defaults to all objects in that layer:

| Property | Values | Description |
|----------|--------|-------------|
| `layer_type` | `"collision"`, `"portal"`, `"spawn"` | What type of entities to create |
| `is_trigger` | `true`, `false` | Trigger colliders detect overlaps but don't block movement |
| `requires_input` | `true`, `false` | Portal requires button press to activate (portals only) |
| `y_sort` | `true`, `false` | Enable Y-sorting for top-down depth (defaults to `false`) |

### Object-Level Properties

Set these on individual objects to override layer defaults:

| Property | Type | Description |
|----------|------|-------------|
| `is_trigger` | bool | Override layer's trigger behavior |
| `requires_input` | bool | Override layer's input requirement |
| `y_sort` | bool | Override layer's Y-sorting behavior |
| `target_map` | string | Map file to load (portals only, e.g., "forest_level") |
| `target_spawn` | string | Spawn point ID in target map (portals only) |
| `spawn_id` | string | Identifier for this spawn point (spawns only) |

### Defaults

- `layer_type="collision"` → `is_trigger=false` (solid colliders)
- `layer_type="portal"` → `is_trigger=true`, `requires_input=false` (automatic trigger portals)
- `layer_type="spawn"` → no collider (just a position marker)
- No `layer_type` → basic entities with transforms only (logs warning)
- `y_sort` → defaults to `false` for all layers

## Y-Sorting for Top-Down Games

For top-down 2D games, objects need to render in the correct depth order. Enable `y_sort` on tile layers and object layers to sort by Y position.

**When to use Y-sorting:**
- Tile layers with tall objects (trees, buildings, furniture)
- Object layers with visual decorations
- The player sprite (set `YSort = true` on Sprite component)

**When NOT to use Y-sorting:**
- Ground/floor tile layers (render flat at fixed depth)
- Collision-only object layers (not rendered)

Objects with higher Y values (lower on screen) render in front of objects with lower Y values (higher on screen).

**Example:**
```
Tile Layer: "Ground"
  (y_sort defaults to false - renders flat)

Tile Layer: "Trees"
  y_sort = true

Object Layer: "Decorations"
  y_sort = true
```

## Common Scenarios

### Solid Collision Walls
```
Layer: "Walls"
  layer_type = "collision"

Draw rectangles around walls, obstacles, etc.
```

### Automatic Portals (Map Transitions)
```
Layer: "Portals"
  layer_type = "portal"
  requires_input = false

Object: "to_forest"
  target_map = "forest_area"
  target_spawn = "entrance"
```

### Door Portals (Require Input)
```
Layer: "Doors"
  layer_type = "portal"
  requires_input = true

Object: "house_door"
  target_map = "house_interior"
  target_spawn = "front_door"
```

### Spawn Points
```
Layer: "Spawns"
  layer_type = "spawn"

Object: "entrance"
  spawn_id = "entrance"

Object: "back_exit"
  spawn_id = "back_exit"
```

### Trigger Zones (Events)
```
Layer: "TriggerZones"
  layer_type = "collision"
  is_trigger = true

Use for detecting when player enters areas (cutscenes, events, etc.)
```

## Property Naming Conventions

- Use `snake_case` for all property names
- Portal naming: Set `target_spawn` to match a `spawn_id` in the target map
- Bidirectional portals require manual setup (create a portal in each map)

## Tips

- Group similar objects in layers (e.g., all walls in one collision layer)
- Use layer properties to avoid repeating object properties
- Object properties always override layer properties
- Test your maps by loading them in your game - missing properties will be logged as warnings
