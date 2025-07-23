# Kobold

Kobold is a 2D game engine written in C#. The name Kobold comes from the first enemy faced in the game Castle of the Winds, one of the first computer games I ever played and one that I have frequently studied in my game development journey. While Kobold is in development, my goal is to make progressively more complex games as a way to drive the creation of more features, and I plan for this effort to culminate in a Castle of the Winds clone.

Originally, Kobold was meant to include an ECS (Entity Component Framework) implementation. However, after implementing my own ECS I have come to the conclusion that the library [Arch](https://github.com/genaray/Arch) provides a very elegant solution that I personally do not think I could improve upon. Therefore, I have decided to use Arch to handle the ECS and am focusing instead on the implementation of the components and systems that are needed to make games.

In the existing game samples, platform-specific code such as rendering, input handling, and audio are handled by [MonoGame](https://monogame.net/). However, I have written Kobold Core as a library completely independent of MonoGame because I may one day decide to implement this low-level code myself and also make the library more plug-and-play with existing engines. The current demarcation between what constitutes "the core engine" and what constitutes "the game host" is basically built around MonoGame's capabilities.

## Core Architectural Principles

*Games will be composed of the systems they require, similar to how entities are composed of components. Not every game will need every system.

*YAGNI (You Ain't Gonna Need It): features will only be implemented as they are required for each game