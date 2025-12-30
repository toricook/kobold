# Utilities - Helper Functions

Kobold provides various utility classes and functions for common game development tasks.

## MathUtils

Static utility class with math helper functions.

**Location:** `Kobold.Core.MathUtils`

### Clamping

**Clamp(value, min, max)** - Constrains a value within a range

```csharp
public static float Clamp(float value, float min, float max)
public static int Clamp(int value, int min, int max)
```

**Example:**
```csharp
// Keep player within screen bounds
var x = MathUtils.Clamp(playerX, 0, 800);
var y = MathUtils.Clamp(playerY, 0, 600);
```

### Random

**RandomRange(min, max)** - Random number in range

```csharp
public static float RandomRange(float min, float max)
public static int RandomRange(int min, int max)
```

**Example:**
```csharp
// Spawn enemy at random X position
var x = MathUtils.RandomRange(0, 800);
var y = 50;
```

**RandomDirection()** - Random unit vector

```csharp
public static Vector2 RandomDirection()
```

**Example:**
```csharp
// Particle with random direction
var direction = MathUtils.RandomDirection();
var velocity = direction * 150f;
```

### Interpolation

**Lerp(a, b, t)** - Linear interpolation

```csharp
public static float Lerp(float a, float b, float t)
public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
```

**Example:**
```csharp
// Smooth camera follow
cameraPos = MathUtils.Lerp(cameraPos, targetPos, 0.1f * deltaTime);
```

**MoveTowards(current, target, maxDelta)** - Move towards target

```csharp
public static float MoveTowards(float current, float target, float maxDelta)
public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDelta)
```

**Example:**
```csharp
// Move towards target at fixed speed
transform.Position = MathUtils.MoveTowards(
    transform.Position,
    targetPosition,
    speed * deltaTime
);
```

### Angles

**DegreesToRadians(degrees)** - Convert degrees to radians

```csharp
public static float DegreesToRadians(float degrees)
```

**RadiansToDegrees(radians)** - Convert radians to degrees

```csharp
public static float RadiansToDegrees(float radians)
```

**WrapAngle(radians)** - Wrap angle to -π to π range

```csharp
public static float WrapAngle(float radians)
```

**AngleDifference(from, to)** - Shortest angle between two angles

```csharp
public static float AngleDifference(float from, float to)
```

**Example:**
```csharp
// Rotate towards target
var currentAngle = transform.Rotation;
var targetAngle = MathF.Atan2(target.Y - pos.Y, target.X - pos.X);
var angleDiff = MathUtils.AngleDifference(currentAngle, targetAngle);

if (MathF.Abs(angleDiff) > 0.01f)
{
    transform.Rotation += MathF.Sign(angleDiff) * rotSpeed * deltaTime;
}
```

## EntityFactory

Helper class for quickly creating common entity patterns.

**Location:** `Kobold.Core.Factories.EntityFactory`

### CreateRectangle

```csharp
public Entity CreateRectangle(World world, Vector2 position, Vector2 size, Color color)
```

**Example:**
```csharp
var rect = EntityFactory.CreateRectangle(
    World,
    new Vector2(100, 100),
    new Vector2(50, 50),
    Color.Blue
);
```

### CreateMovingRectangle

```csharp
public Entity CreateMovingRectangle(
    World world,
    Vector2 position,
    Vector2 size,
    Color color,
    Vector2 velocity
)
```

### CreateText

```csharp
public Entity CreateText(
    World world,
    Vector2 position,
    string text,
    Color color,
    float fontSize = 16
)
```

**Example:**
```csharp
var scoreText = EntityFactory.CreateText(
    World,
    new Vector2(10, 10),
    "Score: 0",
    Color.White,
    24
);
```

## Configuration Classes

### GameConfig

Configure game window and settings.

```csharp
public class GameConfig
{
    public int WindowWidth = 800;
    public int WindowHeight = 600;
    public string WindowTitle = "Kobold Game";
    public Color BackgroundColor = Color.Black;
    public int TargetFPS = 60;
}
```

**Example:**
```csharp
var config = new GameConfig
{
    WindowWidth = 1024,
    WindowHeight = 768,
    WindowTitle = "My Game",
    BackgroundColor = Color.FromArgb(255, 20, 20, 40)
};

MonoGameHost.Run(new MyGame(), config);
```

### PhysicsConfig

Configure physics simulation.

```csharp
public class PhysicsConfig
{
    public bool EnableGravity = false;
    public Vector2 GlobalGravity = new Vector2(0, 980);
    public bool EnableDamping = false;
    public float GlobalDamping = 0.99f;
}
```

**Example:**
```csharp
// Platformer physics
var config = new PhysicsConfig
{
    EnableGravity = true,
    GlobalGravity = new Vector2(0, 500),
    EnableDamping = true,
    GlobalDamping = 0.98f
};

Systems.AddSystem(new PhysicsSystem(config), 100, true);
```

### CollisionMatrix

Configure which layers collide.

```csharp
public class CollisionMatrix
{
    public void SetCollision(CollisionLayer a, CollisionLayer b, bool collides);
    public bool ShouldCollide(CollisionLayer a, CollisionLayer b);
}
```

**Example:**
```csharp
var matrix = new CollisionMatrix();
matrix.SetCollision(CollisionLayer.Player, CollisionLayer.Enemy, true);
matrix.SetCollision(CollisionLayer.Player, CollisionLayer.Pickup, true);
matrix.SetCollision(CollisionLayer.Projectile, CollisionLayer.Enemy, true);

Systems.AddSystem(new CollisionSystem(matrix), 300, true);
```

## Extension Methods

Kobold may include extension methods for common operations (check current implementation).

## See Also

- **[Code Snippets](../examples/code-snippets.md)** - Practical usage examples
- **[MathUtils in action](../guides/physics.md)** - Using math utilities

---

**You've completed the Core documentation!** Continue to [MonoGame Integration](../monogame/) →
