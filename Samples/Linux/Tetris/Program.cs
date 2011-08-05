using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetris
{
	static class Program
    {
        private static Engine game;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            game = new Engine();
            game.Run();
        }
    }
}

