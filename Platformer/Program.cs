using Kobold.Core.Configuration;
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