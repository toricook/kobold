---
layout: default
title: Your First Game
parent: Getting Started
nav_order: 3
---

# Your First Game - Complete Tutorial

Build a simple but complete game to learn Kobold fundamentals. We'll create a space shooter with enemies, projectiles, and scoring.

## What You'll Build

- Player ship with movement
- Enemies that spawn periodically
- Shooting mechanics
- Collision detection
- Score tracking
- Game over screen

## Prerequisites

- Kobold installed ([Installation Guide](installation.md))
- Basic C# knowledge

## Step 1: Project Setup

```bash
dotnet new console -n SpaceShooter
cd SpaceShooter
dotnet add package Kobold.Core
dotnet add package Kobold.Monogame
dotnet add package MonoGame.Framework.DesktopGL

mkdir Content
```

Create simple placeholder graphics in `Content/`:
- `player.png` (32x32 pixel ship)
- `enemy.png` (32x32 pixel enemy)
- `bullet.png` (8x8 pixel projectile)

## Step 2: Game Class

Create `SpaceShooterGame.cs`:

```csharp
using Kobold.Core;
using Kobold.Core.Components;
using Kobold.Core.Systems;
using Kobold.Core.Events;
using Kobold.Core.Configuration;
using System.Numerics;
using System.Drawing;

public class SpaceShooterGame : GameEngineBase
{
    private Entity _player;
    private int _score = 0;
    private float _enemySpawnTimer = 0;
    private float _enemySpawnInterval = 2.0f;

    protected override void Initialize()
    {
        base.Initialize();

        // Screen bounds
        World.Create(new ScreenBounds { MinX = 0, MaxX = 800, MinY = 0, MaxY = 600 });

        // Load assets
        var playerTexture = Assets.LoadTexture("player.png");
        var enemyTexture = Assets.LoadTexture("enemy.png");

        // Create player
        _player = World.Create(
            new Transform { Position = new Vector2(400, 500) },
            SpriteRenderer.FullTexture(playerTexture),
            new Velocity(),
            new PlayerControlled(),
            BoxCollider.FromRenderSize(32, 32)
        );

        // Add systems
        var physicsConfig = new PhysicsConfig { EnableGravity = false };
        Systems.AddSystem(new PhysicsSystem(physicsConfig), 100, true);

        var collisionMatrix = CreateCollisionMatrix();
        Systems.AddSystem(new CollisionSystem(collisionMatrix), 300, true);
        Systems.AddSystem(new BoundarySystem(BoundaryBehavior.Clamp), 200, true);
        Systems.AddRenderSystem(new RenderSystem(Renderer));

        // Subscribe to collisions
        Events.Subscribe<CollisionEvent>(OnCollision);
    }

    protected override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // Player movement
        HandlePlayerInput();

        // Spawn enemies
        _enemySpawnTimer += deltaTime;
        if (_enemySpawnTimer >= _enemySpawnInterval)
        {
            SpawnEnemy();
            _enemySpawnTimer = 0;
        }
    }

    private void HandlePlayerInput()
    {
        if (!World.TryGet(_player, out Velocity velocity))
            return;

        var moveSpeed = 300f;
        var moveX = 0f;

        if (InputManager.IsKeyDown(KeyCode.A)) moveX = -1;
        if (InputManager.IsKeyDown(KeyCode.D)) moveX = 1;

        velocity.Value = new Vector2(moveX * moveSpeed, 0);
        World.Set(_player, velocity);

        if (InputManager.IsKeyPressed(KeyCode.Space))
        {
            FireBullet();
        }
    }

    private void FireBullet()
    {
        if (!World.TryGet(_player, out Transform playerTransform))
            return;

        var bulletTexture = Assets.LoadTexture("bullet.png");
        World.Create(
            new Transform { Position = playerTransform.Position },
            SpriteRenderer.FullTexture(bulletTexture),
            new Velocity { Value = new Vector2(0, -500) },
            BoxCollider.Square(8),
            new Lifetime { TimeRemaining = 3f },
            new Tags { Values = new[] { "Bullet" } }
        );
    }

    private void SpawnEnemy()
    {
        var x = MathUtils.RandomRange(50, 750);
        var enemyTexture = Assets.LoadTexture("enemy.png");

        World.Create(
            new Transform { Position = new Vector2(x, 50) },
            SpriteRenderer.FullTexture(enemyTexture),
            new Velocity { Value = new Vector2(0, 100) },
            BoxCollider.FromRenderSize(32, 32),
            new Tags { Values = new[] { "Enemy" } }
        );
    }

    private void OnCollision(CollisionEvent collision)
    {
        // Bullet hits enemy
        if (HasTag(collision.EntityA, "Bullet") && HasTag(collision.EntityB, "Enemy"))
        {
            World.Destroy(collision.EntityA);
            World.Destroy(collision.EntityB);
            _score += 100;
            Console.WriteLine($"Score: {_score}");
        }

        // Enemy hits player
        if (HasTag(collision.EntityA, "Enemy") && World.Has<PlayerControlled>(collision.EntityB))
        {
            Console.WriteLine("Game Over!");
            World.Destroy(collision.EntityA);
        }
    }

    private bool HasTag(Entity entity, string tag)
    {
        if (!World.TryGet(entity, out Tags tags))
            return false;
        return tags.Values?.Contains(tag) ?? false;
    }

    private CollisionMatrix CreateCollisionMatrix()
    {
        var matrix = new CollisionMatrix();
        matrix.SetCollision(CollisionLayer.Default, CollisionLayer.Default, true);
        return matrix;
    }
}
```

## Step 3: Entry Point

Edit `Program.cs`:

```csharp
using Kobold.Monogame;
using Kobold.Core.Configuration;
using System.Drawing;

MonoGameHost.Run(new SpaceShooterGame(), new GameConfig
{
    WindowWidth = 800,
    WindowHeight = 600,
    WindowTitle = "Space Shooter",
    BackgroundColor = Color.Black
});
```

## Step 4: Run the Game

```bash
dotnet run
```

**Controls:**
- A/D - Move left/right
- Space - Shoot

## What You Learned

1. **Game Structure** - Extending GameEngineBase
2. **Entities & Components** - Creating player, enemies, bullets
3. **Systems** - Physics, Collision, Rendering
4. **Input Handling** - Keyboard controls
5. **Collision Events** - Detecting and responding to collisions
6. **Spawning** - Creating entities during gameplay
7. **Timers** - Enemy spawn timing

## Enhancements

Try adding:
- Score display (TextRenderer)
- Lives system
- Power-ups
- Different enemy types
- Particle effects
- Sound effects

## Next Steps

- **[Core Architecture](../core/architecture.md)** - Deep dive into ECS
- **[Components](../core/components.md)** - All available components
- **[Systems](../core/systems.md)** - All available systems
- **[Examples](../examples/)** - More complex examples

---

**Congratulations!** You've built your first Kobold game! →
