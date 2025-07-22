using Kobold.Core.Configuration;
using Kobold.Monogame;
using System.Drawing;

namespace Asteroids
{
    public class Program
    {
        public static void Main()
        {
            var asteroids = new AsteroidsGame();
            using var host = new MonoGameHost(asteroids, new GameConfig(Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT,
                Color.Black));
            host.Run();
        }
    }

    public static class Constants
    {
        public static int SCREEN_WIDTH = 800;
        public static int SCREEN_HEIGHT = 600;
    }
}