using MonoMac.AppKit;

using Microsoft.Xna;
using Microsoft.Xna.Samples;
using Microsoft.Xna.Samples.Draw2D;

namespace Microsoft.Xna.Samples.Draw2D
{
	class Program
	{
		private Game1 game;
		
		static void Main (string[] args)
        {
            // Fun begins..
			game = new Game1();
			game.Run();
        }
	}
}
