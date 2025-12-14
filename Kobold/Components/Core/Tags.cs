using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Components
{
    // NOTE: These are empty structs used purely as "tags" or "labels" for entities.
    // They contain no data - their presence on an entity is what matters.
    // This is a common ECS pattern for categorizing and querying entities efficiently.

    /// <summary>
    /// Tag component marking an entity as player-controlled.
    /// Used by systems to identify which entities should respond to player input,
    /// be affected by player-specific rules, or be treated specially by AI.
    /// 
    /// Examples:
    /// - InputSystem: Only moves entities with Player tag
    /// - CameraSystem: Follows entities with Player tag
    /// - CollisionSystem: Player vs Enemy collision rules
    /// - UISystem: Updates health bars for Player entities
    /// 
    /// In multiplayer games, you might have Player1, Player2 tags instead,
    /// or use a PlayerID component for more complex player management.
    /// </summary>
    public struct Player { }

    /// <summary>
    /// Tag component marking an entity as an enemy/hostile unit.
    /// Used by systems to identify entities that should attack the player,
    /// be targeted by player weapons, or follow enemy-specific behavior patterns.
    /// 
    /// Examples:
    /// - AISystem: Entities with Enemy tag use hostile AI behaviors
    /// - CollisionSystem: Enemy vs Player projectile collision detection
    /// - ScoringSystem: Award points when Enemy entities are destroyed
    /// - FormationSystem: Enemy entities move in coordinated patterns
    /// - SpawningSystem: Creates entities with Enemy tag during waves
    /// 
    /// Often combined with other components like Health, Weapon, AIBehavior
    /// to create complete enemy entities with different capabilities.
    /// </summary>
    public struct Enemy { }

    /// <summary>
    /// Tag component marking an entity as non-moving/immovable.
    /// Used by physics and collision systems to optimize processing -
    /// static entities don't need velocity updates or complex collision resolution.
    /// 
    /// Examples:
    /// - PhysicsSystem: Skips velocity/acceleration updates for Static entities
    /// - CollisionSystem: Uses simplified collision math (only other object moves)
    /// - BoundarySystem: May skip boundary checking for Static entities
    /// - RenderSystem: Can use different rendering optimizations for static sprites
    /// 
    /// Common static entities:
    /// - Walls, barriers, platforms
    /// - Background decorations
    /// - Fixed turrets or defensive structures
    /// - UI elements positioned in world space
    /// </summary>
    public struct Static { }

    /// <summary>
    /// Tag component marking an entity as movable/physics-affected.
    /// Opposite of Static - indicates this entity should be processed by
    /// movement, physics, and dynamic collision systems.
    /// 
    /// Examples:
    /// - PhysicsSystem: Applies velocity, acceleration, and forces
    /// - CollisionSystem: Full collision detection and response
    /// - BoundarySystem: Handles boundary collisions (wrap, clamp, bounce)
    /// - AISystem: Updates movement for dynamic AI entities
    /// 
    /// Most game entities are Dynamic:
    /// - Players, enemies, NPCs
    /// - Projectiles, particles
    /// - Moving platforms, elevators
    /// - Physics objects like crates, balls
    /// 
    /// Note: You don't usually need both Static and Dynamic tags in the same game.
    /// Choose one approach - either tag the minority case, or use presence/absence of Physics components.
    /// </summary>
    public struct Dynamic { }

    /// <summary>
    /// Tag component marking an entity as a projectile (bullet, missile, laser, etc.).
    /// Used by systems to apply projectile-specific behaviors like automatic movement,
    /// collision damage, and lifetime management.
    /// 
    /// Examples:
    /// - CollisionSystem: Projectiles damage targets and may be destroyed on impact
    /// - BoundarySystem: Projectiles are usually destroyed when leaving screen bounds
    /// - LifetimeSystem: Projectiles often have limited range/duration
    /// - PhysicsSystem: Projectiles may ignore certain physics (like gravity)
    /// - RenderSystem: Projectiles might use additive blending or special effects
    /// 
    /// Projectile entities typically also have:
    /// - Velocity (for movement)
    /// - Lifetime (for range limiting)  
    /// - Damage (for collision effects)
    /// - Owner (to prevent self-damage)
    /// 
    /// Different projectile types can be distinguished using additional components
    /// or by having specific projectile tags (PlayerProjectile, EnemyProjectile).
    /// </summary>
    public struct Projectile { }

    /// <summary>
    /// Tag component marking an entity as a collectible power-up or item.
    /// Used by systems to handle pickup mechanics, temporary effects, and inventory management.
    /// 
    /// Examples:
    /// - CollisionSystem: Player touching PowerUp triggers collection
    /// - EffectSystem: Applies temporary bonuses when collected
    /// - ScoringSystem: Awards points for collecting power-ups
    /// - SpawningSystem: Creates PowerUp entities at specific locations/times
    /// - RenderSystem: PowerUps might pulse, glow, or rotate for visibility
    /// 
    /// PowerUp entities typically also have:
    /// - PowerUpType component (what effect it provides)
    /// - EffectDuration (how long the bonus lasts)
    /// - CollectionValue (points awarded)
    /// - Lifetime (despawn if not collected)
    /// 
    /// Examples: health packs, weapon upgrades, speed boosts, shields, extra lives
    /// </summary>
    public struct PowerUp { }

    /// <summary>
    /// Tag component marking an entity as a non-solid trigger area.
    /// Used by systems to detect when other entities enter/exit specific regions
    /// without causing physical collision responses.
    /// 
    /// Examples:
    /// - CollisionSystem: Detects overlap but doesn't stop movement
    /// - TriggerSystem: Activates events when entities enter trigger zones
    /// - GameLogicSystem: Checkpoints, level transitions, cutscene triggers
    /// - AudioSystem: Plays ambient sounds when player enters area
    /// 
    /// Trigger entities typically also have:
    /// - BoxCollider or other collision shape (defines the trigger area)
    /// - TriggerAction component (what happens when triggered)
    /// - TriggerFilter (which entity types can activate it)
    /// 
    /// Common uses:
    /// - Level exit portals
    /// - Checkpoint save points  
    /// - Cutscene activation zones
    /// - Environmental effect areas (damage zones, speed boosts)
    /// - Audio cue regions
    /// </summary>
    public struct Trigger { }

    /// <summary>
    /// Tag component marking an entity as destructible environment.
    /// Used by systems to handle breakable objects that can be damaged
    /// but aren't enemies (don't move, don't fight back).
    /// 
    /// Examples:
    /// - CollisionSystem: Takes damage from projectiles/explosions
    /// - HealthSystem: Has hit points and can be destroyed
    /// - EffectSystem: Shows damage effects, debris when destroyed
    /// - ScoringSystem: May award points when destroyed
    /// 
    /// Destructible entities typically also have:
    /// - Health component (hit points)
    /// - Collider (for damage detection)
    /// - DropTable (what items spawn when destroyed)
    /// 
    /// Examples: crates, barrels, walls, barriers, destructible terrain
    /// This is different from Static (can't be damaged) and Enemy (actively hostile).
    /// </summary>
    public struct Destructible { }

    /// <summary>
    /// Tag component marking an entity as a user interface element.
    /// Used by systems to handle UI-specific rendering, input, and layout behaviors.
    /// UI entities exist in screen space rather than world space.
    /// 
    /// Examples:
    /// - RenderSystem: Renders in screen coordinates with UI camera/layer
    /// - InputSystem: Uses screen-space mouse coordinates for interaction
    /// - LayoutSystem: Handles UI positioning, anchoring, scaling
    /// - UISystem: Manages visibility based on game state
    /// 
    /// UI entities typically also have:
    /// - UITransform (screen position, anchoring)
    /// - UIRenderer (text, images, buttons)
    /// - UIInteractable (clickable, hoverable)
    /// 
    /// Examples: buttons, text labels, health bars, menus, HUD elements
    /// These should generally be processed separately from world entities.
    /// </summary>
    public struct UI { }

    /// <summary>
    /// Tag component marking an entity as debug/development-only content.
    /// Used by systems to show diagnostic information, development tools,
    /// or testing content that should be hidden in production builds.
    /// 
    /// Examples:
    /// - RenderSystem: Only renders Debug entities in development builds
    /// - InputSystem: Debug entities might respond to special key combinations
    /// - LoggingSystem: Extra verbose logging for Debug entities
    /// - ProfilerSystem: Performance monitoring for Debug entities
    /// 
    /// Debug entities might include:
    /// - Collision visualization (showing hitboxes)
    /// - Performance counters and framerate displays
    /// - AI pathfinding visualization
    /// - Physics force indicators
    /// - Entity inspector tools
    /// - Cheat/testing controls
    /// 
    /// Should be easily disabled for release builds via preprocessor directives
    /// or build configuration flags.
    /// </summary>
    public struct Debug { }
}

// USAGE PATTERNS:
//
// 1. SINGLE TAG QUERIES:
//    Find all players: new QueryDescription().WithAll<Player>()
//    Find all enemies: new QueryDescription().WithAll<Enemy>()
//
// 2. MULTIPLE TAG QUERIES:  
//    Find dynamic enemies: new QueryDescription().WithAll<Enemy, Dynamic>()
//    Find non-UI entities: new QueryDescription().WithNone<UI>()
//
// 3. COMPLEX FILTERING:
//    Find moving enemies that aren't projectiles:
//    new QueryDescription().WithAll<Enemy, Dynamic>().WithNone<Projectile>()
//
// 4. SYSTEM-SPECIFIC PROCESSING:
//    InputSystem: Only process entities with Player tag
//    AISystem: Only process entities with Enemy tag  
//    PhysicsSystem: Only process entities with Dynamic tag
//    CollisionSystem: Handle Player vs Enemy, Player vs PowerUp, etc.
//
// 5. ADDING TAGS TO ENTITIES:
//    // Create a player entity
//    var player = world.Create(
//        new Transform(playerStartPos),
//        new Player(),           // Tag as player
//        new Dynamic(),          // Tag as movable
//        new Velocity(),
//        new Health(100)
//    );
//
//    // Create an enemy projectile  
//    var bullet = world.Create(
//        new Transform(enemyPos),
//        new Enemy(),            // Belongs to enemy
//        new Projectile(),       // Is a projectile
//        new Dynamic(),          // Moves
//        new Velocity(bulletVel),
//        new Lifetime(5.0f)
//    );
//
// DESIGN PRINCIPLES:
// - Tags should be mutually exclusive categories OR orthogonal properties
// - Good: Player/Enemy/Neutral (mutually exclusive entity types)
// - Good: Static/Dynamic (mutually exclusive movement types)  
// - Good: UI tag (orthogonal - can combine with other tags if needed)
// - Bad: FastEnemy, SlowEnemy (use a Speed component instead)
// - Bad: RedEnemy, BlueEnemy (use a Color component instead)
//
// Tags are for BEHAVIORAL differences, not just cosmetic ones.