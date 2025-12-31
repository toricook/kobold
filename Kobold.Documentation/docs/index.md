---
title: Home
nav_order: 1
---

# Kobold

**Kobold** is an evolving project meant to make developing 2D games super easy. The goal is to have enough functionality here that any 2D game that fits within a common genre (platformer, roguelike, etc.) can be prototyped within hours, or even faster using AI coding agents.[^1]

[^1] At some point, I would like to create some specifications that are meant to be parsed by coding agents for those who are either contributing to or using this project, to ensure high quality and consistent code. This seems like a good time to mention that I am using coding agents to help me build out this repo and review PRs, but I also review changes myself. Furthermore, I believe that all documentation meant to be read by humans should be written by humans.

## Key Principles

- **Entity-Component-System** - The engine is built on this framework using the [Arch](https://github.com/genaray/Arch) library. I have written up an explanation of ECS [here]().
- **Demo-Driven** - As I create demos of new types of games, I will add components and systems that seem useful and reusable to the core or extension libraries
- **Extensible** - The logic that is common to almost all games will be in Core, while logic specific to certain types of games will be in the Extensions (or other) projects, with clear namespaces so that users can import the pieces that they need.
- **Platform-Agnostic** - The Core currently only depends on Arch, and has abstractions for hardware-specific things like input handling and rendering. The Kobold.Monogame project contains [Monogame](https://monogame.net/) implementations of these abstractions, and all of my demo projects so far use the `MonogameHost`. If it makes sense, I might see if I can make some of this stuff work with Unity. I may also write my own implementations of the abstractions. We'll see.