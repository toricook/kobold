---
layout: default
title: Installation
parent: Getting Started
nav_order: 1
---

# Installation Guide

Detailed guide for installing Kobold and setting up your development environment.

## Prerequisites

Before installing Kobold, ensure you have:

### .NET 8 SDK

**Download:** [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

**Verify installation:**
```bash
dotnet --version
# Should show 8.0.x or later
```

### IDE (Choose One)

- **Visual Studio 2022** - Full-featured IDE (Windows/Mac)
- **Visual Studio Code** - Lightweight, cross-platform
- **JetBrains Rider** - Premium cross-platform IDE

### MonoGame (Installed Automatically via NuGet)

MonoGame.Framework.DesktopGL will be installed when you add the Kobold.Monogame package.

## Installation Methods

### Method 1: NuGet Packages (Recommended)

Use this method for:
- Creating new games
- Production projects
- When you don't need Kobold source code

```bash
# Create new project
dotnet new console -n MyKoboldGame
cd MyKoboldGame

# Add Kobold packages
dotnet add package Kobold.Core
dotnet add package Kobold.Monogame
dotnet add package MonoGame.Framework.DesktopGL

# Optional packages
dotnet add package Kobold.Extensions  # For tilemaps
dotnet add package Kobold.Procedural  # For procedural generation
```

### Method 2: Source Code (For Development)

Use this method for:
- Contributing to Kobold
- Modifying the framework
- Learning how Kobold works

```bash
# Clone repository
git clone https://github.com/toricook/Kobold.git
cd Kobold

# Create your game project
dotnet new console -n MyGame
cd MyGame

# Reference Kobold projects
# Edit MyGame.csproj and add:
```

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Kobold\Kobold.csproj" />
    <ProjectReference Include="..\Kobold.Monogame\Kobold.Monogame.csproj" />
  </ItemGroup>
</Project>
```

## Project Setup

### 1. Create Game Class

Create `MyGame.cs`:

```csharp
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using System.Numerics;

public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // Your game initialization here
        Systems.AddRenderSystem(new RenderSystem(Renderer));
    }
}
```

### 2. Create Entry Point

Edit `Program.cs`:

```csharp
using Kobold.Monogame;
using Kobold.Core.Configuration;
using System.Drawing;

MonoGameHost.Run(new MyGame(), new GameConfig
{
    WindowWidth = 800,
    WindowHeight = 600,
    WindowTitle = "My Kobold Game",
    BackgroundColor = Color.Black
});
```

### 3. Add Content Directory

```bash
mkdir Content
```

Place PNG files in `Content/` directory.

### 4. Configure Content Copying

Edit your `.csproj` to copy content files:

```xml
<ItemGroup>
  <Content Include="Content\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### 5. Run Your Game

```bash
dotnet run
```

## Verify Installation

Create a simple test to verify everything works:

```csharp
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using Kobold.Monogame;
using System.Numerics;
using System.Drawing;

public class TestGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // Create a green square
        World.Create(
            new Transform { Position = new Vector2(400, 300) },
            new RectangleRenderer
            {
                Size = new Vector2(50, 50),
                Color = Color.Green
            }
        );

        Systems.AddRenderSystem(new RenderSystem(Renderer));
    }
}

MonoGameHost.Run(new TestGame(), new GameConfig
{
    WindowWidth = 800,
    WindowHeight = 600,
    WindowTitle = "Kobold Test"
});
```

If you see a window with a green square, installation is successful!

## Troubleshooting

### "Could not load file or assembly 'MonoGame.Framework'"

**Solution:** Install MonoGame.Framework.DesktopGL
```bash
dotnet add package MonoGame.Framework.DesktopGL
```

### ".NET 8.0 SDK not found"

**Solution:** Download and install from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

### "Window doesn't appear"

**Solutions:**
1. Verify `OutputType` is `Exe` in .csproj
2. Ensure you're calling `MonoGameHost.Run()` in Program.cs
3. Check for exceptions in console output

### Content files not found

**Solutions:**
1. Verify PNG files are in `Content/` directory
2. Check `.csproj` has content copying configured
3. Ensure file paths match exactly (case-sensitive on Linux/macOS)

## Next Steps

Now that Kobold is installed:

1. **[Quick Start](quick-start.md)** - 5-minute tutorial
2. **[Your First Game](your-first-game.md)** - Build a complete game
3. **[Core Documentation](../core/)** - Learn the framework

## Platform-Specific Notes

### Windows

No special configuration needed. MonoGame uses DirectX or OpenGL backends.

### macOS

MonoGame uses OpenGL. May require allowing the app in Security & Privacy settings.

### Linux

Install MonoGame dependencies:
```bash
sudo apt-get install mono-complete
```

## See Also

- **[MonoGame Setup](../monogame/setup.md)** - Detailed MonoGame configuration
- **[Quick Start](quick-start.md)** - Get coding in 5 minutes

---

**Installation complete?** Continue to [Quick Start](quick-start.md) →
