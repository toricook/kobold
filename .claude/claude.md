# Claude Code Reviewer Guidelines

## Documentation Review Requirements

### During Pull Request Reviews

When reviewing pull requests, **verify that significant changes have corresponding documentation updates** in the `docs/` directory.

**Scope: Documentation is only required for Kobold framework projects**, which include:
- `Kobold` (core library)
- `Kobold.Monogame`
- `Kobold.Procedural`
- `Kobold.Extensions`
- `Kobold.SpriteSheetEditor`
- `Kobold.Experiments`
- Any other projects prefixed with `Kobold.`

**Do NOT require documentation for game/example projects** such as:
- `Asteroids`
- `Pong`
- `CaveExplorer`
- Other games built using the framework

Check for documentation when the PR includes changes to Kobold framework projects with:
- New public APIs, classes, or methods
- New features or functionality
- Breaking changes or API modifications
- New projects or modules
- Significant behavior changes

**What to look for:**
- Has `docs/index.md` or relevant documentation files been updated?
- Are new features/APIs documented?
- Are breaking changes clearly documented?
- Are examples updated if behavior changed?

**If documentation is missing:**
- Request documentation updates before approving the PR
- Suggest specific areas that need documentation
- Point out which new features/APIs need to be documented

### During Regular Development

**Do NOT** require documentation updates for:
- Work-in-progress commits on feature branches
- Commits that are not part of a PR
- Draft PRs (unless explicitly ready for review)
- Internal/private implementation details

Documentation requirements apply **only at the PR review stage**, not during ongoing development work.

### XML Documentation Comments

**ALWAYS add XML documentation comments** to public classes, methods, properties, and other members for DocFX generation.

**Format:**
```csharp
/// <summary>
/// Brief description of what the class/method does.
/// </summary>
/// <param name="paramName">Description of parameter</param>
/// <returns>Description of return value</returns>
public class ExampleClass { }
```

**Policy:**
- XML comments are **required** for all public APIs in Kobold framework projects
- XML comments provide auto-generated API documentation via DocFX
- **DO NOT** create separate markdown documentation files in `docs/` unless explicitly requested by the user
- XML comments are the primary documentation mechanism for code-level documentation

## General Review Principles

- Focus on code quality, correctness, and maintainability
- Check for proper error handling and edge cases
- Verify tests are included for new functionality
- Ensure code follows existing patterns and conventions

---

# Project Architecture & Development Guidelines

## Overview

Kobold is a **2D game engine** built on top of the **Arch ECS (Entity Component System) framework**. The solution is designed to be modular, reusable, and platform-agnostic.

## Project Structure

### Kobold (Core Project)
- **Purpose**: Contains code that **all games require**
- **Scope**: Fundamental engine features that are universally needed
- **Examples**: Core ECS systems, basic components, essential services
- **Rule**: If every game needs it, it belongs here

### Kobold.Extensions
- **Purpose**: Contains **optional packages** that only some games need
- **Scope**: Specialized features and systems for specific use cases
- **Examples**: Tilemaps, advanced collision, procedural generation, specific rendering systems
- **Rule**: If it's useful but not universally required, it belongs here

### Demo Project
- **Purpose**: **Testing features only** - NOT for implementation
- **Scope**: Example games and test scenarios to validate engine functionality
- **Rule**: **NEVER create reusable systems or components here**
- When implementing features for the Demo project, all reusable code should be in Kobold (core) or Kobold.Extensions

### Other Framework Projects
- **Kobold.Monogame**: MonoGame implementation (see abstraction principle below)
- **Kobold.Procedural**: Procedural generation systems
- **Kobold.SpriteSheetEditor**: Editor tooling
- **Kobold.Experiments**: Experimental features

### Game Projects (Not Framework)
- Examples: Asteroids, Pong, CaveExplorer
- These are games built **using** the framework, not part of the framework itself
- Do not create framework-level components here

## Architecture Principles

### ECS Framework Built on Arch
- Kobold is built on the **Arch ECS framework**
- Follow ECS patterns: Entities, Components, Systems
- Leverage Arch's performance and capabilities

### Abstraction Over Implementation
**Critical Principle**: Any dependency on external libraries (like MonoGame) must be abstracted.

- **Pattern**: Create abstractions in Kobold core, implementations in specific projects
- **Example**: MonoGame-specific code goes in `Kobold.Monogame` as an implementation of core abstractions
- **Goal**: The engine could theoretically replace MonoGame with another rendering library
- **Benefit**: Platform independence, testability, flexibility

**Examples of Abstraction**:
- Rendering abstractions in `Kobold` → MonoGame implementation in `Kobold.Monogame`
- Input abstractions in `Kobold` → MonoGame input in `Kobold.Monogame`
- Asset loading abstractions in `Kobold` → MonoGame content pipeline in `Kobold.Monogame`

## Development Guidelines for Claude Code

### When Asked to Implement Features in Demo
1. **DO**: Create reusable components/systems in `Kobold` or `Kobold.Extensions`
2. **DO**: Use those components in the Demo project to test them
3. **DON'T**: Create systems and components directly in the Demo project
4. **REASON**: We're building a reusable engine, not just a single game

### When Adding Framework Features
1. Ask: "Will every game need this?"
   - **Yes** → Add to `Kobold` core
   - **No** → Add to `Kobold.Extensions`

2. Ask: "Does this depend on MonoGame (or another library)?"
   - **Yes** → Create abstraction in core, implementation in `Kobold.Monogame`
   - **No** → Can be implemented directly in core/extensions

### When Creating Systems
- Systems should operate on components via ECS patterns
- Prefer composition over inheritance
- Keep systems focused and single-purpose
- Use Arch queries for efficient entity filtering

### When Creating Components
- Components should be data-only (or minimal logic)
- Keep components simple and composable
- Avoid framework-specific types in component definitions when possible
