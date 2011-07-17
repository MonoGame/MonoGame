#region File Description
//-----------------------------------------------------------------------------
// JoinSessionScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;

#endregion

namespace NetworkStateManagement
{
	/// <summary>
	/// This menu screen displays a list of available network sessions,
	/// and lets the user choose which one to join.
	/// </summary>
	class JoinSessionScreen : MenuScreen
	{
	#region Fields

		const int MaxSearchResults = 8;
		AvailableNetworkSessionCollection availableSessions;

	#endregion

	#region Initialization


		/// <summary>
		/// Constructs a menu screen listing the available network sessions.
		/// </summary>
		public JoinSessionScreen (AvailableNetworkSessionCollection availableSessions)
		: base(Resources.JoinSession)
			{
			this.availableSessions = availableSessions;

			foreach (AvailableNetworkSession availableSession in availableSessions) {
				// Create menu entries for each available session.
				MenuEntry menuEntry = new AvailableSessionMenuEntry (availableSession);
				menuEntry.Selected += AvailableSessionMenuEntrySelected;
				MenuEntries.Add (menuEntry);

				// Matchmaking can return up to 25 available sessions at a time, but
				// we don't have room to fit that many on the screen. In a perfect
				// world we should make the menu scroll if there are too many, but it
				// is easier to just not bother displaying more than we have room for.
				if (MenuEntries.Count >= MaxSearchResults)
					break;
			}

			// Add the Back menu entry.
			MenuEntry backMenuEntry = new MenuEntry (Resources.Back);
			backMenuEntry.Selected += BackMenuEntrySelected;
			MenuEntries.Add (backMenuEntry);
		}


	#endregion

	#region Event Handlers


		/// <summary>
		/// Event handler for when an available session menu entry is selected.
		/// </summary>
		void AvailableSessionMenuEntrySelected (object sender, PlayerIndexEventArgs e)
		{
			// Which menu entry was selected?
			AvailableSessionMenuEntry menuEntry = (AvailableSessionMenuEntry)sender;
			AvailableNetworkSession availableSession = menuEntry.AvailableSession;

			try {
				// Begin an asynchronous join network session operation.
				IAsyncResult asyncResult = NetworkSession.BeginJoin (availableSession, 
								null, null);

				// Activate the network busy screen, which will display
				// an animation until this operation has completed.
				NetworkBusyScreen busyScreen = new NetworkBusyScreen (asyncResult);

				busyScreen.OperationCompleted += JoinSessionOperationCompleted;

				ScreenManager.AddScreen (busyScreen, ControllingPlayer);
			} catch (Exception exception) {
				NetworkErrorScreen errorScreen = new NetworkErrorScreen (exception);

				ScreenManager.AddScreen (errorScreen, ControllingPlayer);
			}
		}


		/// <summary>
		/// Event handler for when the asynchronous join network session
		/// operation has completed.
		/// </summary>
		void JoinSessionOperationCompleted (object sender, OperationCompletedEventArgs e)
		{
			try {
				// End the asynchronous join network session operation.
				NetworkSession networkSession = NetworkSession.EndJoin (e.AsyncResult);

				// Create a component that will manage the session we just joined.
				NetworkSessionComponent.Create (ScreenManager, networkSession);

				// Go to the lobby screen. We pass null as the controlling player,
				// because the lobby screen accepts input from all local players
				// who are in the session, not just a single controlling player.
				ScreenManager.AddScreen (new LobbyScreen (networkSession), null);

				availableSessions.Dispose ();
			} catch (Exception exception) {
				NetworkErrorScreen errorScreen = new NetworkErrorScreen (exception);

				ScreenManager.AddScreen (errorScreen, ControllingPlayer);
			}
		}


		/// <summary>
		/// Event handler for when the Back menu entry is selected.
		/// </summary>
		void BackMenuEntrySelected (object sender, PlayerIndexEventArgs e)
		{
			availableSessions.Dispose ();

			ExitScreen ();
		}


	#endregion
	}
}
