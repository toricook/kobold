# Example Projects

The Kobold repository includes several complete example games demonstrating framework features.

## Asteroids

Classic arcade space shooter.

**Demonstrates:**
- Physics with rotation and thrust
- Collision detection
- Screen wrapping (BoundarySystem)
- Projectile systems
- Entity destruction

**Key Code:**
- Player ship with rotation controls
- Asteroid splitting on collision
- Weapon firing system

## Pong

Classic paddle game.

**Demonstrates:**
- Simple physics (ball bouncing)
- Player input
- Collision detection
- Score tracking
- Basic AI opponent

## CaveExplorer

Procedural cave exploration game.

**Demonstrates:**
- Procedural generation (cellular automata)
- Tilemap rendering
- Tilemap collision
- Camera following with bounds
- Player movement

**Key Code:**
- Cave generation with `CellularAutomataGenerator`
- Tilemap setup with collision
- Camera constrained to map bounds

## Code Snippets

For quick reference patterns, see [Code Snippets](code-snippets.md).

## Running Examples

```bash
# Clone repository
git clone https://github.com/toricook/Kobold.git
cd Kobold

# Run an example
cd Asteroids
dotnet run

cd ../Pong
dotnet run

cd ../CaveExplorer
dotnet run
```

## Learning Path

1. **Start with Pong** - Simplest example
2. **Try Asteroids** - More complex physics
3. **Explore CaveExplorer** - Tilemaps and procedural generation

---

**See Also:** [Code Snippets](code-snippets.md) for copy-paste patterns →
