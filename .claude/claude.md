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

## General Review Principles

- Focus on code quality, correctness, and maintainability
- Check for proper error handling and edge cases
- Verify tests are included for new functionality
- Ensure code follows existing patterns and conventions
