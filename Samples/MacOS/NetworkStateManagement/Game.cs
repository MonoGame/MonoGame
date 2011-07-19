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
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

#endregion

namespace NetworkStateManagement
{
	/// <summary>
	/// Sample showing how to manage the different game states involved in
	/// implementing a networked game, with menus for creating, searching,
	/// and joining sessions, a lobby screen, and the game itself. This main
	/// game class is extremely simple: all the interesting stuff happens
	/// in the ScreenManager component.
	/// </summary>
	public class NetworkStateManagementGame : Microsoft.Xna.Framework.Game
	{
	#region Fields

		GraphicsDeviceManager graphics;
		ScreenManager screenManager;


		// By preloading any assets used by UI rendering, we avoid framerate glitches
		// when they suddenly need to be loaded in the middle of a menu transition.
		static readonly string[] preloadAssets = 
	{
		"gradient",
		"cat",
		"chat_ready",
		"chat_able",
		"chat_talking",
		"chat_mute",
	};


	#endregion

	#region Initialization

        /// <summary>
		/// The main game constructor.
		/// </summary>		
#if ANDROID 
		public NetworkStateManagementGame  (Activity activity) : base (activity)
#else 
        public NetworkStateManagementGame  ()  
#endif
		{
			Content.RootDirectory = "Content";

			graphics = new GraphicsDeviceManager (this);            
#if ANDROID
            graphics.IsFullScreen = true;
#else
            graphics.PreferredBackBufferWidth = 1067;
			graphics.PreferredBackBufferHeight = 600;
#endif

			// Create components.
			screenManager = new ScreenManager (this);

			Components.Add (screenManager);
			Components.Add (new MessageDisplayComponent (this));
			Components.Add (new GamerServicesComponent (this));

			// Activate the first screens.
			screenManager.AddScreen (new BackgroundScreen (), null);
			screenManager.AddScreen (new MainMenuScreen (), null);

			// Listen for invite notification events.
			NetworkSession.InviteAccepted += (sender, e) => NetworkSessionComponent.InviteAccepted (screenManager, e);

			// To test the trial mode behavior while developing your game,
			// uncomment this line:

			// Guide.SimulateTrialMode = true;
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

}
