using Kobold.Monogame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pong
{
    public class Program
    {
        public static void Main()
        {
            var pongGame = new PongGame(); // No dependencies needed here
            using var host = new MonoGameHost(pongGame);
            host.Run();
        }
    }
}