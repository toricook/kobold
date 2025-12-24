using Rapid.NET;
using Rapid.NET.Wpf;

namespace Experiments
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Rapid.NET will scan for [Script] attributes and provide a UI
            LaunchMethods.RunFromArgs(args);
        }
    }
}
