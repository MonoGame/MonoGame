#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

#endregion


namespace GameStateManagement
{
	/// <summary>
	/// Sample showing how to manage different game states, with transitions
	/// between menu screens, a loading screen, the game itself, and a pause
	/// menu. This main game class is extremely simple: all the interesting
	/// stuff happens in the ScreenManager component.
	/// </summary>
	public class GameStateManagementGame : Microsoft.Xna.Framework.Game
	{
	#region Fields

		GraphicsDeviceManager graphics;
		ScreenManager screenManager;


		// By preloading any assets used by UI rendering, we avoid framerate glitches
		// when they suddenly need to be loaded in the middle of a menu transition.
		static readonly string[] preloadAssets = 
	{
		"gradient",
	};


	#endregion

	#region Initialization


		/// <summary>
		/// The main game constructor.
		/// </summary>
		public GameStateManagementGame ()
			{
			Content.RootDirectory = "Content";

			graphics = new GraphicsDeviceManager (this);
			graphics.PreferredBackBufferWidth = 853;
			graphics.PreferredBackBufferHeight = 480;

			// Create the screen manager component.
			screenManager = new ScreenManager (this);

			Components.Add (screenManager);

			// Activate the first screens.
			screenManager.AddScreen (new BackgroundScreen (), null);
			screenManager.AddScreen (new MainMenuScreen (), null);
		}


		/// <summary>
		/// Loads graphics content.
		/// </summary>
		protected override void LoadContent ()
		{
			foreach (string asset in preloadAssets) {
				Content.Load<object> (asset);
			}
		}


	#endregion

	#region Draw


		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.Black);

			// The real drawing happens inside the screen manager component.
			base.Draw (gameTime);
		}


	#endregion
	}


	#region Entry Point
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main (string[] args)
		{
			NSApplication.Init ();
			
			using (var p = new NSAutoreleasePool ()) {
				NSApplication.SharedApplication.Delegate = new AppDelegate();
				NSApplication.Main(args);
			}


		}
	}
	
	class AppDelegate : NSApplicationDelegate
	{
		
		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			using (GameStateManagementGame game = new GameStateManagementGame ()) {
				game.Run ();
			}
		}
		
		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}
	}	
	
	#endregion
}
