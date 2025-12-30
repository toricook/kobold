# Sprite Sheet Editor

Visual tool for creating sprite sheet configuration JSON files used by Kobold's asset system.

## Overview

The Sprite Sheet Editor helps you:
1. Load a sprite sheet image (PNG)
2. Define the grid layout (width, height, spacing, margin)
3. Visually verify the grid aligns with your sprites
4. Export a JSON configuration file

## Usage

### 1. Run the Editor

```bash
cd Kobold.SpriteSheetEditor
dotnet run
```

### 2. Load Image

Place your sprite sheet PNG in the project directory or provide the full path.

Edit `SpriteSheetEditorGame.cs` to change the default image:
```csharp
var texturePath = "your_spritesheet.png";
```

### 3. Adjust Grid

**Keyboard Controls:**
- **Arrow Keys** - Adjust grid cell size
- **G** - Toggle grid visibility
- **W/A/S/D** - Pan camera
- **Q/E** - Zoom out/in
- **0** - Reset zoom
- **Ctrl+S** - Save configuration (future feature)

### 4. Configure Parameters

Edit the code to set:
- `spriteWidth`, `spriteHeight` - Size of each sprite in pixels
- `columns`, `rows` - Grid dimensions
- `spacing` - Gap between sprites
- `margin` - Border around the sprite sheet

### 5. Export Configuration

The editor displays the configuration in the window title. Create a JSON file manually:

```json
{
  "texture": "player.png",
  "spriteWidth": 32,
  "spriteHeight": 32,
  "columns": 8,
  "rows": 4,
  "spacing": 0,
  "margin": 0
}
```

Save as `player.json` in your Content directory.

## Tips

- Use a checkerboard background to verify transparency
- Zoom in to verify pixel-perfect alignment
- Common sprite sizes: 16x16, 32x32, 64x64

## See Also

- **[Asset Manager](../core/asset-manager.md)** - Using sprite sheet configs
- **[Animation](../core/components.md#animation)** - Sprite animation

---

**Back:** [Tools Overview](index.md) ←
