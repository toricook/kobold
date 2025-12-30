---
title: Entity Component System (ECS) Framework
nav_order: 1
parent: Explainers
---

# Entity Component System (ECS) Framework

## What is it?

There are probably a lot of explainers out there for ECS that are written better than this doc here. For me, the easiest way to understand ECS is by comparing it to the more well known Object Oriented Programming (OOP). Say you were making a dungeon crawler using OOP. You might start off by creating a class for your player character. It could look something like this:

```csharp

class Player
{
    Vector2 position;
    Sprite sprite;
    int health;

    void Update() { /* update logic */ }
    void Render() { /* render logic */ }
}
```

But then when you go to make your enemies, you realize they share a lot of qualities with the player -- they also have health, a position, and a sprite. So perhaps instead you go and make a base class for both of them like this:

```csharp

abstract class Actor
{
    Vector2 position;
    Sprite sprite;
    int health;

    void Update() { /* update logic */ }
    void Render() { /* render logic */ }
}
```

Then a player is just an implementation of an Actor, maybe with some additional logic to do input handling so that the real life player can control their movements. And an enemy is also an Actor, but it has some kind of AI movement logic attached. So far, so good. But then what if you want to make another kind of enemy that's invulnerable, so it has no need for a health field? Or what about one that's invisible, so it doesn't have a sprite or need a Render() method? Well, maybe an Actor shouldn't actually be renderable or have health by default -- these are separate concerns, after all. Maybe we need a new abstract base class for an Actor that can take damage, or an Actor that is also visible, or an Actor that is both. You can see how we can quickly end up with a ton of class hierarchies that are almost-but-not-quite the same and we would end up duplicating a lot of code. You may have heard of the principle of "Composition over Inheritance", which helps get around issues like this. Instead of an "is-a" relationship (a player IS An actor), you create "has-a" relationships (a player HAS A renderable component and HAS A health component). ECS follows this principle.

But it's also not JUST a way to get composition over inheritance. ECS also uses the principle of Data Oriented Design. To understand what this priciple is, consider our player and all of our enemies in our dungeon crawler. Let's say we are also using a velocity field on each one in order to calculate whether this entity should move or not. In each frame, we have to iterate over all the entities, check their velocity, then multiply it by the time and update thier position. Then once we have the new positions, we need to iterate over all of the entities again and render them to the screen using their sprite component. But the inefficiency here comes from the bigass data object that we are iterating over each time. The system responsible for moving each entity doesn't care about the sprite, and the system responsibile for rendering (probably) doesn't care about the velocity. Yet they both need to load ALL of that data about the entity into memory just to do something with one little piece of it. This is the problem that Data Oriented Programming attempts to solve.

So now we are ready to learn what ECS is. It stands for Entity Component System (though I think it's a bit clearer to write it like Entity, Component, System to make it clear that these are the 3 elements of the framework) -- all words that I used in the above explanation to help motivate the defition. In ECS, each "thing" that exists in your game is not an implementation of a class but an *entity*, which is literally just a unique ID. The things that make each entity behave in a certain way are called *components* -- the are the "has-a" objects like sprites and health. But components are always just data -- they are structs (value types). The components are acted on by *systems*, where each system needs access only to a certain type of component to do its job. The "magic" in the performance comes from the way that systems can access an array of just the components they need and then iterate over and act on those components without caring about the entities that those components are attached to.

With ECS, we would write the code from the dungeon crawler example like this:

```csharp

struct Position
{
    float x;
    float y;
}
```

```csharp

struct Health
{
    float value;
}
```

```csharp

struct Sprite
{
    ITexture texture;
}
```

and then create each entity by just creating some object that has a unique ID and the right collection of components. But before I can get into *how* to create the entities, and how to create the systems that actually do things with these components, I need to give a quick intro to Arch.

## Quick Arch Intro

