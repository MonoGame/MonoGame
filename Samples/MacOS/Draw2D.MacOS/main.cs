using MonoMac.AppKit;

namespace Microsoft.Xna.Samples.Draw2D
{
	class Program
	{
		static private Game1 game;
		
		static void Main (string[] args)
        {
			NSApplication.Init();          
			
            // Fun begins..
			game = new Game1();
			game.Run();
			
			NSApplication.Main(args);
        }
	}
}
