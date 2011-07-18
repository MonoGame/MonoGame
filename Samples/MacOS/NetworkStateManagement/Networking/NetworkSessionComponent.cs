#region File Description
//-----------------------------------------------------------------------------
// NetworkSessionComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;

#endregion

namespace NetworkStateManagement
{
	/// <summary>
	/// Component in charge of owning and updating the current NetworkSession object.
	/// This is responsible for calling NetworkSession.Update at regular intervals,
	/// and also exposes the NetworkSession as a game service which can easily be
	/// looked up by any other code that needs to access it.
	/// </summary>
	class NetworkSessionComponent : GameComponent
	{
	#region Fields

		public const int MaxGamers = 16;
		public const int MaxLocalGamers = 4;
		ScreenManager screenManager;
		NetworkSession networkSession;
		IMessageDisplay messageDisplay;
		bool notifyWhenPlayersJoinOrLeave;
		string sessionEndMessage;

	#endregion

	#region Initialization


		/// <summary>
		/// The constructor is private: external callers should use the Create method.
		/// </summary>
		NetworkSessionComponent (ScreenManager screenManager, 
				NetworkSession networkSession)
		: base(screenManager.Game)
			{
			this.screenManager = screenManager;
			this.networkSession = networkSession;

			// Hook up our session event handlers.
			networkSession.GamerJoined += GamerJoined;
			networkSession.GamerLeft += GamerLeft;
			networkSession.SessionEnded += NetworkSessionEnded;
		}


		/// <summary>
		/// Creates a new NetworkSessionComponent.
		/// </summary>
		public static void Create (ScreenManager screenManager,
				NetworkSession networkSession)
		{
			Game game = screenManager.Game;

			// Register this network session as a service.
			game.Services.AddService (typeof(NetworkSession), networkSession);

			// Create a NetworkSessionComponent, and add it to the Game.
			game.Components.Add (new NetworkSessionComponent (screenManager,
							networkSession));
		}


		/// <summary>
		/// Initializes the component.
		/// </summary>
		public override void Initialize ()
		{
			base.Initialize ();

			// Look up the IMessageDisplay service, which will
			// be used to report gamer join/leave notifications.
			messageDisplay = (IMessageDisplay)Game.Services.GetService (
							typeof(IMessageDisplay));

			if (messageDisplay != null)
				notifyWhenPlayersJoinOrLeave = true;
		}


		/// <summary>
		/// Shuts down the component.
		/// </summary>
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				// Remove the NetworkSessionComponent.
				Game.Components.Remove (this);

				// Remove the NetworkSession service.
				Game.Services.RemoveService (typeof(NetworkSession));

				// Dispose the NetworkSession.
				if (networkSession != null) {
					networkSession.Dispose ();
					networkSession = null;
				}
			}

			base.Dispose (disposing);
		}


	#endregion

	#region Update


		/// <summary>
		/// Updates the network session.
		/// </summary>
		public override void Update (GameTime gameTime)
		{
			if (networkSession == null)
				return;

			try {
				networkSession.Update ();

				// Has the session ended?
				if (networkSession.SessionState == NetworkSessionState.Ended) {
					LeaveSession ();
				}
			} catch (Exception exception) {
				// Handle any errors from the network session update.
				Debug.WriteLine ("NetworkSession.Update threw " + exception);

				sessionEndMessage = Resources.ErrorNetwork;

				LeaveSession ();
			}
		}


	#endregion

	#region Event Handlers


		/// <summary>
		/// Event handler called when a gamer joins the session.
		/// Displays a notification message.
		/// </summary>
		void GamerJoined (object sender, GamerJoinedEventArgs e)
		{
			if (notifyWhenPlayersJoinOrLeave) {
				messageDisplay.ShowMessage (Resources.MessageGamerJoined, 
					e.Gamer.Gamertag);
			}
		}


		/// <summary>
		/// Event handler called when a gamer leaves the session.
		/// Displays a notification message.
		/// </summary>
		void GamerLeft (object sender, GamerLeftEventArgs e)
		{
			if (notifyWhenPlayersJoinOrLeave) {
				messageDisplay.ShowMessage (Resources.MessageGamerLeft, 
					e.Gamer.Gamertag);
			}
		}


		/// <summary>
		/// Event handler called when the network session ends.
		/// Stores the end reason, so this can later be displayed to the user.
		/// </summary>
		void NetworkSessionEnded (object sender, NetworkSessionEndedEventArgs e)
		{
			switch (e.EndReason) {
			case NetworkSessionEndReason.ClientSignedOut:
				sessionEndMessage = null;
				break;

			case NetworkSessionEndReason.HostEndedSession:
				sessionEndMessage = Resources.ErrorHostEndedSession;
				break;

			case NetworkSessionEndReason.RemovedByHost:
				sessionEndMessage = Resources.ErrorRemovedByHost;
				break;

			case NetworkSessionEndReason.Disconnected:
			default:
				sessionEndMessage = Resources.ErrorDisconnected;
				break;
			}

			notifyWhenPlayersJoinOrLeave = false;
		}


		/// <summary>
		/// Event handler called when the system delivers an invite notification.
		/// This can occur when the user accepts an invite that was sent to them by
		/// a friend (pull mode), or if they choose the "Join Session In Progress"
		/// option in their friends screen (push mode). The handler leaves the
		/// current session (if any), then joins the session referred to by the
		/// invite. It is not necessary to prompt the user before doing this, as
		/// the Guide will already have taken care of the necessary confirmations
		/// before the invite was delivered to you.
		/// </summary>
		public static void InviteAccepted (ScreenManager screenManager,
					InviteAcceptedEventArgs e)
		{
			// If we are already in a network session, leave it now.
			NetworkSessionComponent self = FindSessionComponent (screenManager.Game);

			if (self != null)
				self.Dispose ();

			try			{
				// Which local profiles should we include in this session?
				IEnumerable<SignedInGamer> localGamers = 
					ChooseGamers (NetworkSessionType.PlayerMatch, e.Gamer.PlayerIndex);

				// Begin an asynchronous join-from-invite operation.
				IAsyncResult asyncResult = NetworkSession.BeginJoinInvited (localGamers, 
									null, null);

				// Use the loading screen to replace whatever screens were previously
				// active. This will completely reset the screen state, regardless of
				// whether we were in the menus or playing a game when the invite was
				// delivered. When the loading screen finishes, it will activate the
				// network busy screen, which displays an animation as it waits for
				// the join operation to complete.
				NetworkBusyScreen busyScreen = new NetworkBusyScreen (asyncResult);

				busyScreen.OperationCompleted += JoinInvitedOperationCompleted;

				LoadingScreen.Load (screenManager, false, null, new BackgroundScreen (), 
							busyScreen);
			} catch (Exception exception) {
				NetworkErrorScreen errorScreen = new NetworkErrorScreen (exception);

				LoadingScreen.Load (screenManager, false, null, new BackgroundScreen (), 
							new MainMenuScreen (), 
							errorScreen);
			}
		}


		/// <summary>
		/// Event handler for when the asynchronous join-from-invite
		/// operation has completed.
		/// </summary>
		static void JoinInvitedOperationCompleted (object sender,
						OperationCompletedEventArgs e)
		{
			ScreenManager screenManager = ((GameScreen)sender).ScreenManager;

			try			{
				// End the asynchronous join-from-invite operation.
				NetworkSession networkSession = 
					NetworkSession.EndJoinInvited (e.AsyncResult);

				// Create a component that will manage the session we just created.
				NetworkSessionComponent.Create (screenManager, networkSession);

				// Go to the lobby screen.
				screenManager.AddScreen (new LobbyScreen (networkSession), null);
			} catch (Exception exception) {
				screenManager.AddScreen (new MainMenuScreen (), null);
				screenManager.AddScreen (new NetworkErrorScreen (exception), null);
			}
		}


	#endregion

	#region Methods


		/// <summary>
		/// Checks whether the specified session type is online.
		/// Online sessions cannot be used by local profiles, or if
		/// parental controls are enabled, or when running in trial mode.
		/// </summary>
		public static bool IsOnlineSessionType (NetworkSessionType sessionType)
		{
			switch (sessionType) {
			case NetworkSessionType.Local:
			case NetworkSessionType.SystemLink:
				return false;

			case NetworkSessionType.PlayerMatch:
			//case NetworkSessionType.Ranked:
				return true;

			default:
				throw new NotSupportedException ();
			}
		}


		/// <summary>
		/// Decides which local gamer profiles should be included in a network session.
		/// This is passed the index of the primary gamer (the profile who selected the
		/// relevant menu option, or who is responding to an invite). The primary gamer
		/// will always be included in the session. Other gamers may also be added if
		/// there are suitable profiles signed in. To control how many gamers can be
		/// returned by this method, adjust the MaxLocalGamers constant.
		/// </summary>
		public static IEnumerable<SignedInGamer> ChooseGamers (
							NetworkSessionType sessionType,
							PlayerIndex playerIndex)
		{
			List<SignedInGamer> gamers = new List<SignedInGamer> ();

			// Look up the primary gamer, and make sure they are signed in.
			SignedInGamer primaryGamer = Gamer.SignedInGamers [playerIndex];

			if (primaryGamer == null)
				throw new GamerPrivilegeException ();

			gamers.Add (primaryGamer);

			// Check whether any other profiles should also be included.
			foreach (SignedInGamer gamer in Gamer.SignedInGamers) {
				// Never include more profiles than the MaxLocalGamers constant.
				if (gamers.Count >= MaxLocalGamers)
					break;

				// Don't want two copies of the primary gamer!
				if (gamer == primaryGamer)
					continue;

				// If this is an online session, make sure the profile is signed
				// in to Live, and that it has the privilege for online gameplay.
				if (IsOnlineSessionType (sessionType)) {
					if (!gamer.IsSignedInToLive)
						continue;

					if (!gamer.Privileges.AllowOnlineSessions)
						continue;
				}

				if (primaryGamer.IsGuest && !gamer.IsGuest && gamers [0] == primaryGamer) {
					// Special case: if the primary gamer is a guest profile,
					// we should insert some other non-guest at the start of the
					// output list, because guests aren't allowed to host sessions.
					gamers.Insert (0, gamer);
				} else {
					gamers.Add (gamer);
				}
			}

			return gamers;
		}


		/// <summary>
		/// Public method called when the user wants to leave the network session.
		/// Displays a confirmation message box, then disposes the session, removes
		/// the NetworkSessionComponent, and returns them to the main menu screen.
		/// </summary>
		public static void LeaveSession (ScreenManager screenManager,
					PlayerIndex playerIndex)
		{
			NetworkSessionComponent self = FindSessionComponent (screenManager.Game);

			if (self != null) {
				// Display a message box to confirm the user really wants to leave.
				string message;

				if (self.networkSession.IsHost)
					message = Resources.ConfirmEndSession;
				else
					message = Resources.ConfirmLeaveSession;

				MessageBoxScreen confirmMessageBox = new MessageBoxScreen (message);

				// Hook the messge box ok event to actually leave the session.
				confirmMessageBox.Accepted += delegate
		{
			self.LeaveSession ();
		};

				screenManager.AddScreen (confirmMessageBox, playerIndex);
			}
		}


		/// <summary>
		/// Internal method for leaving the network session. This disposes the 
		/// session, removes the NetworkSessionComponent, and returns the user
		/// to the main menu screen.
		/// </summary>
		void LeaveSession ()
		{
			// Destroy this NetworkSessionComponent.
			Dispose ();

			// If we have a sessionEndMessage string explaining why the session has
			// ended (maybe this was a network disconnect, or perhaps the host kicked
			// us out?) create a message box to display this reason to the user.
			MessageBoxScreen messageBox;

			if (!string.IsNullOrEmpty (sessionEndMessage))
				messageBox = new MessageBoxScreen (sessionEndMessage, false);
			else
				messageBox = null;

			// At this point we want to return the user all the way to the main
			// menu screen. But what if they just joined a session? In that case
			// they went through this flow of screens:
			//
			//  - MainMenuScreen
			//  - CreateOrFindSessionsScreen
			//  - JoinSessionScreen (if joining, skipped if creating a new session)
			//  - LobbyScreeen
			//
			// If we have these previous screens on the history stack, and the user
			// backs out of the LobbyScreen, the right thing is just to pop off the
			// intermediate screens, returning them to the existing MainMenuScreen
			// instance without bothering to reload everything. But if the user is
			// in gameplay, or has been in gameplay and then returned to the lobby,
			// the screen stack will have been emptied.
			//
			// To do the right thing in both cases, we scan through the screen history
			// stack looking for a MainMenuScreen. If we find one, we pop any
			// subsequent screens so as to return back to it, while if we don't
			// find it, we just reset everything via the LoadingScreen.

			GameScreen[] screens = screenManager.GetScreens ();

			// Look for the MainMenuScreen.
			for (int i = 0; i < screens.Length; i++) {
				if (screens [i] is MainMenuScreen) {
					// If we found one, pop everything since then to return back to it.
					for (int j = i + 1; j < screens.Length; j++)
						screens [j].ExitScreen ();

					// Display the why-did-the-session-end message box.
					if (messageBox != null)
						screenManager.AddScreen (messageBox, null);

					return;
				}
			}

			// If we didn't find an existing MainMenuScreen, reload everything.
			// The why-did-the-session-end message box will be displayed after
			// the loading screen has completed.
			LoadingScreen.Load (screenManager, false, null, new BackgroundScreen (), 
							new MainMenuScreen (), 
							messageBox);
		}


		/// <summary>
		/// Searches through the Game.Components collection to
		/// find the NetworkSessionComponent (if any exists).
		/// </summary>
		static NetworkSessionComponent FindSessionComponent (Game game)
		{
			return game.Components.OfType<NetworkSessionComponent> ().FirstOrDefault ();
		}


	#endregion
	}
}
