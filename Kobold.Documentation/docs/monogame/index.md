# Kobold.Monogame - MonoGame Integration

**Kobold.Monogame** provides the MonoGame platform implementation for Kobold.Core's abstractions, enabling rendering, input, and content loading.

## Overview

Kobold.Core defines platform-agnostic interfaces:
- `IRenderer` - Drawing graphics
- `IInputManager` - Keyboard/mouse input
- `IContentLoader` - Loading assets from disk

Kobold.Monogame implements these for MonoGame:
- `MonoGameRenderer` - Renders using SpriteBatch
- `MonoGameInputManager` - Handles MonoGame input
- `MonoGameFileContentLoader` - Loads PNG textures directly from files (bypassing Content Pipeline)
- `MonoGameHost` - Bootstraps and runs your game

## MonoGameHost

The main class that runs your Kobold game with MonoGame.

**Usage:**
```csharp
MonoGameHost.Run(new MyGame(), new GameConfig
{
    WindowWidth = 800,
    WindowHeight = 600,
    WindowTitle = "My Game"
});
```

**What it does:**
1. Creates MonoGame Game instance
2. Instantiates platform implementations (Renderer, Input, ContentLoader)
3. Injects them into your GameEngineBase
4. Runs the MonoGame game loop (Update/Render)

## Platform Implementations

### MonoGameRenderer

Implements `IRenderer` using MonoGame's `SpriteBatch`.

**Features:**
- Sprite rendering with rotation, scale, tint
- Rectangle, text, triangle, line drawing
- Pixel-perfect rendering (no sub-pixel coords)
- Fallback pixel font for text rendering

**Methods:**
- `DrawRectangle(position, size, color)`
- `DrawTexture(texture, position, scale)`
- `DrawSprite(texture, position, sourceRect, scale, rotation, tint)`
- `DrawText(text, position, color, fontSize)`
- `DrawTriangle(points, position, rotation, color)`
- `DrawLine(start, end, color, thickness)`

### MonoGameInputManager

Implements `IInputManager` using MonoGame's input APIs.

**Features:**
- Keyboard state tracking (current and previous frames)
- Mouse position and button states
- Key press detection (triggered once per press)

**Methods:**
- `IsKeyPressed(KeyCode)` - True on first frame of press
- `IsKeyDown(KeyCode)` - True while held
- `IsKeyUp(KeyCode)` - True while not pressed
- `GetMousePosition()` - Mouse coordinates
- `IsMouseButtonPressed(MouseButton)` - Mouse click detection

### MonoGameFileContentLoader

Implements `IContentLoader` for loading PNG textures directly from the file system at runtime.

**Features:**
- Loads raw PNG files from disk (bypasses MonoGame Content Pipeline)
- Uses `Texture2D.FromStream` for runtime loading
- Supports relative and absolute paths
- Auto-adds .png extension if missing
- Configurable content root directory

**Usage:**
```csharp
var loader = new MonoGameFileContentLoader(graphicsDevice, "Content");
var texture = loader.LoadTexture("player.png");  // Loads from Content/player.png
```

**Note:** This implementation does NOT use the MonoGame Content Pipeline (XNB files). For a pipeline-based loader, use `MonoGamePipelineContentLoader` (not yet implemented).

### MonoGameTexture

Wraps MonoGame's `Texture2D` to implement `ITexture`.

**Properties:**
- `Width` - Texture width in pixels
- `Height` - Texture height in pixels

## Quick Start

### 1. Create Game Class

```csharp
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using Kobold.Monogame;

public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // Create entities
        var texture = Assets.LoadTexture("player.png");
        World.Create(
            new Transform { Position = new Vector2(400, 300) },
            SpriteRenderer.FullTexture(texture)
        );

        // Add systems
        Systems.AddRenderSystem(new RenderSystem(Renderer));
    }
}
```

### 2. Run with MonoGameHost

```csharp
// Program.cs
MonoGameHost.Run(new MyGame(), new GameConfig
{
    WindowWidth = 800,
    WindowHeight = 600,
    WindowTitle = "My Kobold Game"
});
```

### 3. Add Content Files

Place your PNG files in the `Content/` directory at your project root.

## Dependencies

Kobold.Monogame requires:
- **Kobold.Core** - Core framework
- **MonoGame.Framework.DesktopGL** - MonoGame (cross-platform)
- **.NET 8.0** - Runtime

## Platform Support

MonoGame supports:
- Windows (DesktopGL)
- macOS (DesktopGL)
- Linux (DesktopGL)

Kobold can potentially support other MonoGame backends with custom implementations.

## See Also

- **[Setup Guide](setup.md)** - Detailed MonoGame project setup
- **[Core Framework](../core/)** - Platform-agnostic abstractions

---

**Next:** [Setup Guide](setup.md) for detailed project configuration →
