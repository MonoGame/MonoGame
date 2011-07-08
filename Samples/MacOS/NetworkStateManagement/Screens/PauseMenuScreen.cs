#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

#endregion

namespace NetworkStateManagement
{
	/// <summary>
	/// The pause menu comes up over the top of the game,
	/// giving the player options to resume or quit.
	/// </summary>
	class PauseMenuScreen : MenuScreen
	{
	#region Fields

		NetworkSession networkSession;

	#endregion

	#region Initialization


		/// <summary>
		/// Constructor.
		/// </summary>
		public PauseMenuScreen (NetworkSession networkSession)
		: base(Resources.Paused)
			{
			this.networkSession = networkSession;

			// Add the Resume Game menu entry.
			MenuEntry resumeGameMenuEntry = new MenuEntry (Resources.ResumeGame);
			resumeGameMenuEntry.Selected += OnCancel;
			MenuEntries.Add (resumeGameMenuEntry);

			if (networkSession == null) {
				// If this is a single player game, add the Quit menu entry.
				MenuEntry quitGameMenuEntry = new MenuEntry (Resources.QuitGame);
				quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;
				MenuEntries.Add (quitGameMenuEntry);
			} else {
				// If we are hosting a network game, add the Return to Lobby menu entry.
				if (networkSession.IsHost) {
					MenuEntry lobbyMenuEntry = new MenuEntry (Resources.ReturnToLobby);
					lobbyMenuEntry.Selected += ReturnToLobbyMenuEntrySelected;
					MenuEntries.Add (lobbyMenuEntry);
				}

				// Add the End/Leave Session menu entry.
				string leaveEntryText = networkSession.IsHost ? Resources.EndSession : 
								Resources.LeaveSession;

				MenuEntry leaveSessionMenuEntry = new MenuEntry (leaveEntryText);
				leaveSessionMenuEntry.Selected += LeaveSessionMenuEntrySelected;
				MenuEntries.Add (leaveSessionMenuEntry);
			}
		}


	#endregion

	#region Handle Input


		/// <summary>
		/// Event handler for when the Quit Game menu entry is selected.
		/// </summary>
		void QuitGameMenuEntrySelected (object sender, PlayerIndexEventArgs e)
		{
			MessageBoxScreen confirmQuitMessageBox = 
				new MessageBoxScreen (Resources.ConfirmQuitGame);

			confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

			ScreenManager.AddScreen (confirmQuitMessageBox, ControllingPlayer);
		}


		/// <summary>
		/// Event handler for when the user selects ok on the "are you sure
		/// you want to quit" message box. This uses the loading screen to
		/// transition from the game back to the main menu screen.
		/// </summary>
		void ConfirmQuitMessageBoxAccepted (object sender, PlayerIndexEventArgs e)
		{
			LoadingScreen.Load (ScreenManager, false, null, new BackgroundScreen (), 
							new MainMenuScreen ());
		}


		/// <summary>
		/// Event handler for when the Return to Lobby menu entry is selected.
		/// </summary>
		void ReturnToLobbyMenuEntrySelected (object sender, PlayerIndexEventArgs e)
		{
			if (networkSession.SessionState == NetworkSessionState.Playing) {
				networkSession.EndGame ();
			}
		}


		/// <summary>
		/// Event handler for when the End/Leave Session menu entry is selected.
		/// </summary>
		void LeaveSessionMenuEntrySelected (object sender, PlayerIndexEventArgs e)
		{
			NetworkSessionComponent.LeaveSession (ScreenManager, e.PlayerIndex);
		}


	#endregion
	}
}
