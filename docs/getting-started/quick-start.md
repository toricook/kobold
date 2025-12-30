# Quick Start - 5 Minutes to Your First Game

This guide will get you up and running with Kobold in about 5 minutes. We'll create a simple game with a moving, bouncing sprite.

## Step 1: Create a New Project (1 minute)

```bash
# Create a new console application
dotnet new console -n MyKoboldGame
cd MyKoboldGame

# Add required packages (if using NuGet)
dotnet add package Kobold.Core
dotnet add package Kobold.Monogame
dotnet add package MonoGame.Framework.DesktopGL
```

Or if working from the Kobold source repository, add project references instead.

## Step 2: Create Your Game Class (2 minutes)

Replace the contents of `Program.cs` with:

```csharp
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using Kobold.Core.Configuration;
using Kobold.Monogame;
using System.Numerics;
using System.Drawing;

public class MyGame : GameEngineBase
{
    protected override void Initialize()
    {
        base.Initialize();

        // Create a screen bounds entity
        World.Create(
            new ScreenBounds
            {
                MinX = 0,
                MaxX = 800,
                MinY = 0,
                MaxY = 600
            }
        );

        // Create a bouncing ball
        World.Create(
            new Transform { Position = new Vector2(400, 300) },
            new RectangleRenderer
            {
                Size = new Vector2(30, 30),
                Color = Color.Blue,
                Layer = 100
            },
            new Velocity { Value = new Vector2(200, 150) }
        );

        // Add systems
        var physicsConfig = new PhysicsConfig
        {
            EnableGravity = false
        };
        Systems.AddSystem(new PhysicsSystem(physicsConfig), 100, true);
        Systems.AddSystem(new BoundarySystem(BoundaryBehavior.Bounce), 200, true);
        Systems.AddRenderSystem(new RenderSystem(Renderer));
    }
}

// Entry point
MonoGameHost.Run(new MyGame(), new GameConfig
{
    WindowWidth = 800,
    WindowHeight = 600,
    WindowTitle = "My Kobold Game",
    BackgroundColor = Color.Black
});
```

## Step 3: Run Your Game (30 seconds)

```bash
dotnet run
```

You should see a window with a blue square bouncing around the screen!

## What's Happening?

Let's break down what this code does:

### Entities and Components

```csharp
World.Create(
    new Transform { Position = new Vector2(400, 300) },
    new RectangleRenderer { Size = new Vector2(30, 30), Color = Color.Blue },
    new Velocity { Value = new Vector2(200, 150) }
);
```

This creates an entity (the blue square) with three components:
- **Transform** - Position in the world (center of screen)
- **RectangleRenderer** - Visual representation (30x30 blue square)
- **Velocity** - Movement direction and speed (200 pixels/sec right, 150 down)

### Systems

```csharp
Systems.AddSystem(new PhysicsSystem(physicsConfig), 100, true);
Systems.AddSystem(new BoundarySystem(BoundaryBehavior.Bounce), 200, true);
Systems.AddRenderSystem(new RenderSystem(Renderer));
```

Systems process entities each frame:
- **PhysicsSystem** (order 100) - Moves entities based on their Velocity
- **BoundarySystem** (order 200) - Bounces entities off screen edges
- **RenderSystem** - Draws entities to the screen

The order matters - physics runs first, then boundary checking, then rendering.

## Next Steps - Add Interactivity

Let's make the ball respond to keyboard input. Add this method to your `MyGame` class:

```csharp
protected override void Update(float deltaTime)
{
    base.Update(deltaTime);

    // Query for entities with Transform and Velocity
    var query = World.Query<Transform, Velocity>();
    foreach (var entity in query)
    {
        ref var velocity = ref entity.Get<Velocity>();

        // Arrow keys to control direction
        if (InputManager.IsKeyDown(KeyCode.Left))
            velocity.Value = new Vector2(-200, velocity.Value.Y);
        if (InputManager.IsKeyDown(KeyCode.Right))
            velocity.Value = new Vector2(200, velocity.Value.Y);
        if (InputManager.IsKeyDown(KeyCode.Up))
            velocity.Value = new Vector2(velocity.Value.X, -200);
        if (InputManager.IsKeyDown(KeyCode.Down))
            velocity.Value = new Vector2(velocity.Value.X, 200);
    }
}
```

Now you can control the ball's direction with arrow keys while it bounces!

## Experiment Further

Try these modifications:

**Change colors:**
```csharp
Color = Color.Red  // or Green, Yellow, etc.
```

**Add more balls:**
```csharp
// Create multiple entities in a loop
for (int i = 0; i < 10; i++)
{
    World.Create(
        new Transform { Position = new Vector2(100 + i * 60, 300) },
        new RectangleRenderer { Size = new Vector2(20, 20), Color = Color.FromArgb(255, i * 25, 100) },
        new Velocity { Value = MathUtils.RandomDirection() * 150 }
    );
}
```

**Add gravity:**
```csharp
var physicsConfig = new PhysicsConfig
{
    EnableGravity = true,
    GlobalGravity = new Vector2(0, 500)  // Downward gravity
};
```

**Wrap instead of bounce:**
```csharp
Systems.AddSystem(new BoundarySystem(BoundaryBehavior.Wrap), 200, true);
```

## What You've Learned

In just 5 minutes, you've learned:

- How to create a Kobold project
- The Entity Component System pattern
- Creating entities with components
- Adding and configuring systems
- The game lifecycle (Initialize, Update, Render)
- Querying and modifying entities

## Where to Go Next

- **[Your First Game](your-first-game.md)** - Build a complete game with more features
- **[Core Architecture](../core/architecture.md)** - Deep dive into ECS concepts
- **[Components Reference](../core/components.md)** - All available components
- **[Systems Reference](../core/systems.md)** - All available systems
- **[Code Snippets](../examples/code-snippets.md)** - Common patterns and recipes

## Common Issues

**"Could not load file or assembly 'MonoGame.Framework'"**
- Make sure you added the MonoGame.Framework.DesktopGL package

**Window doesn't appear:**
- Check that you're calling `MonoGameHost.Run()` at the program entry point
- Ensure your project type is an executable (not a library)

**Nothing is rendering:**
- Verify you added `RenderSystem` to the Systems
- Check that entities have both `Transform` and a renderer component (`SpriteRenderer`, `RectangleRenderer`, etc.)

---

**Having issues?** Check the [full installation guide](installation.md) or see [troubleshooting tips](../guides/).
