using Kobold.Extensions.Physics.Systems;
using Kobold.Extensions.Physics.Components;
using Kobold.Extensions.Collision.Systems;
using Kobold.Extensions.Collision.Components;
using Kobold.Extensions.Input.Systems;
using Kobold.Extensions.Input.Components;
using Kobold.Extensions.Boundaries.Systems;
using Kobold.Extensions.Boundaries.Components;
using Kobold.Extensions.Triggers.Systems;
using Kobold.Extensions.Destruction.Systems;
using Kobold.Extensions.Destruction.Components;
using Kobold.Extensions.Gameplay.Components;
using Kobold.Extensions.GameState.Systems;
﻿using Kobold.Core.Configuration;
using Kobold.Monogame;
using System.Drawing;

namespace Platformer
{
    public class Program
    {
        public static void Main()
        {
            var game = new PlatformerGame();
            using var host = new MonoGameHost(game, new GameConfig(1280, 720, Color.CornflowerBlue));
            host.Run();
        }
    }
}