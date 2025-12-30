using Kobold.Core.Configuration;
using Kobold.Monogame;
using System.Drawing;

namespace CaveExplorer
{
    public class Program
    {
        public static void Main()
        {
            var game = new CaveExplorerGame();
            using var host = new MonoGameHost(game, new GameConfig(Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT,
                Color.Black));
            host.Run();
        }
    }

    public static class Constants
    {
        public static int SCREEN_WIDTH = 1024;
        public static int SCREEN_HEIGHT = 768;
    }
}
