# MonoGame Project Setup

This guide walks through setting up a Kobold game project with MonoGame.

## Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download)
- **IDE** - Visual Studio, VS Code, or JetBrains Rider

## Project Setup

### Option 1: Using NuGet Packages (Recommended for Distribution)

```bash
# Create new console application
dotnet new console -n MyKoboldGame
cd MyKoboldGame

# Add Kobold packages
dotnet add package Kobold.Core
dotnet add package Kobold.Monogame
dotnet add package MonoGame.Framework.DesktopGL

# Optional packages
dotnet add package Kobold.Extensions  # Tilemaps
dotnet add package Kobold.Procedural  # Procedural generation
```

### Option 2: Using Source (Recommended for Development)

Clone the Kobold repository and reference projects directly:

```xml
<!-- MyKoboldGame.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Kobold\Kobold\Kobold.csproj" />
    <ProjectReference Include="..\Kobold\Kobold.Monogame\Kobold.Monogame.csproj" />
  </ItemGroup>
</Project>
```

## Project Structure

```
MyKoboldGame/
├── Program.cs
├── MyGame.cs
├── Content/
│   ├── player.png
│   ├── enemy.png
│   └── background.png
└── MyKoboldGame.csproj
```

## Content Files

MonoGame projects need a `Content/` directory for assets.

### Create Content Directory

```bash
mkdir Content
```

### Add Content Files

Place PNG files in `Content/`:
- `Content/player.png`
- `Content/enemy.png`
- `Content/background.png`

### Configure Content Copying

Update your `.csproj` to copy content files to output:

```xml
<ItemGroup>
  <Content Include="Content\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

## Code Setup

### Program.cs

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

### MyGame.cs

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

        // Load assets
        var playerTexture = Assets.LoadTexture("player.png");

        // Create entities
        World.Create(
            new Transform { Position = new Vector2(400, 300) },
            SpriteRenderer.FullTexture(playerTexture)
        );

        // Add systems
        Systems.AddRenderSystem(new RenderSystem(Renderer));
    }
}
```

## Running Your Game

```bash
dotnet run
```

You should see a window with your sprite!

## Troubleshooting

**"Could not load file or assembly 'MonoGame.Framework'"**
- Ensure MonoGame.Framework.DesktopGL is installed
- Check target framework is .NET 8.0

**"Texture not found"**
- Verify PNG files are in `Content/` directory
- Check `.csproj` has Content copying configured
- Ensure file names match (case-sensitive on Linux/macOS)

**Window doesn't appear**
- Verify `MonoGameHost.Run()` is called in Program.cs
- Check OutputType is `Exe` in .csproj

## See Also

- **[Quick Start](../getting-started/quick-start.md)** - 5-minute tutorial
- **[MonoGame Documentation](https://docs.monogame.net/)** - Official MonoGame docs

---

**Ready to build?** Check out the [Getting Started](../getting-started/) tutorials!
