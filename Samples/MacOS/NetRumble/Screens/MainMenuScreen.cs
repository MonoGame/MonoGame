#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
#endregion

namespace NetRumble
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    public class MainMenuScreen : MenuScreen
    {
        #region State Data


        /// <summary>
        /// The potential states of the main menu.
        /// </summary>
        enum MainMenuState
        {
            Empty,
            SignedOut,
            SignedInLocal,
            SignedInLive,
        }

        /// <summary>
        /// Flag that tracks last known value of IsTrialMode
        /// </summary>
        bool trialMode;
        bool updateState;

        /// <summary>
        /// The current state of the main menu.
        /// </summary>
        MainMenuState state = MainMenuState.Empty;
        MainMenuState State
        {
            get { return state; }
            set
            {
                // exit early from trivial sets
                if (state == value && trialMode == Guide.IsTrialMode && !updateState)
                {
                    return;
                }
                updateState = false;  // reset the flag, in case it was set
                state = value;
                trialMode = Guide.IsTrialMode;
                if (MenuEntries != null)
                {
                    switch (state)
                    {
                        case MainMenuState.SignedInLive:
                            {
                                MenuEntries.Clear();
                                MenuEntries.Add("Quick Match");
                                MenuEntries.Add("Create Xbox LIVE Session");
                                MenuEntries.Add("Join Xbox LIVE Session");
                                MenuEntries.Add("Create System Link Session");
                                MenuEntries.Add("Join System Link Session");
                                if (Guide.IsTrialMode)
                                {
                                    MenuEntries.Add("Unlock Full Game");
                                }
                                else if (ScreenManager.invited != null)
                                {
                                    MenuEntries.Add("Join Invited Game");
                                }
                                MenuEntries.Add("Exit");
                                break;
                            }
                        case MainMenuState.SignedInLocal:
                            {
                                MenuEntries.Clear();
                                MenuEntries.Add("Create System Link Session");
                                MenuEntries.Add("Join System Link Session");
                                MenuEntries.Add("Exit");
                                break;
                            }
                        case MainMenuState.SignedOut:
                            {
                                MenuEntries.Clear();
                                MenuEntries.Add("Sign In");
                                MenuEntries.Add("Exit");
                                break;
                            }
                    }
                }
            }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new MainMenu object.
        /// </summary>
        public MainMenuScreen() : base()
        {
            // set the transition times
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);

            // capture current value of trial mode flag
            trialMode = Guide.IsTrialMode;
            updateState = false;
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Updates the screen. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
            bool coveredByOtherScreen)
        {
            bool signedIntoLive = false;
            if (Gamer.SignedInGamers.Count > 0)
            {
                foreach (SignedInGamer signedInGamer in Gamer.SignedInGamers)
                {
                    if (signedInGamer.IsSignedInToLive)
                    {
                        signedIntoLive = true;
                        break;
                    }
                }
                State = signedIntoLive ? MainMenuState.SignedInLive : 
                    MainMenuState.SignedInLocal;
            }
            else
            {
                State = MainMenuState.SignedOut;
            }
            
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
        
        
        /// <summary>
        /// Responds to user menu selections.
        /// </summary>
        protected override void OnSelectEntry(int entryIndex)
        {
            switch (state)
            {
                case MainMenuState.SignedInLive:
                    {
                        switch (entryIndex)
                        {
                            case 0: // Quick Match
                                QuickMatchSession();
                                break;

                            case 1: // Create Xbox LIVE Session
                                CreateSession(NetworkSessionType.PlayerMatch);
                                break;

                            case 2: // Join Xbox LIVE Session
                                FindSession(NetworkSessionType.PlayerMatch);
                                break;

                            case 3: // Create System Link Session
                                CreateSession(NetworkSessionType.SystemLink);
                                break;

                            case 4: // Join System Link Session
                                FindSession(NetworkSessionType.SystemLink);
                                break;

                            case 5: // Exit or Unlock Full Game (depending on trialMode flag)
                                if (trialMode)
                                    ShowOffer();
                                else if (ScreenManager.invited != null)
                                    JoinInvitedGame();
                                else
                                    OnCancel();
                                break;

                            case 6: // Exit
                                OnCancel();
                                break;
                        }
                        break;
                    }
                case MainMenuState.SignedInLocal:
                    {
                        switch (entryIndex)
                        {
                            case 0: // Create System Link Session
                                CreateSession(NetworkSessionType.SystemLink);
                                break;

                            case 1: // Join System Link Session
                                FindSession(NetworkSessionType.SystemLink);
                                break;

                            case 2: // Exit
                                OnCancel();
                                break;
                        }
                        break;
                    }
                case MainMenuState.SignedOut:
                    {
                        switch (entryIndex)
                        {
                            case 0: // Sign In
                                if (!Guide.IsVisible)
                                {
                                    Guide.ShowSignIn(1, false);
                                }
                                break;

                            case 1: // Exit
                                OnCancel();
                                break;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Show the marketplace offer for a trial mode game.
        /// </summary>
        private void ShowOffer()
        {
            // NOTE: This chooses the first signed in gamer, since we don't have any information
            // about which gamepad clicked on the button.
            Guide.ShowMarketplace(Gamer.SignedInGamers[0].PlayerIndex);
        }


        /// <summary>
        /// Attempt to join a session using an invite that was received.
        /// </summary>
        public void JoinInvitedGame()
        {
            try
            {
                // begin to join the game we were invited to
                IAsyncResult asyncResult = NetworkSession.BeginJoinInvited(1, null, null);

                // create the busy screen
                NetworkBusyScreen busyScreen = new NetworkBusyScreen("Joining the session...", asyncResult);
                busyScreen.OperationCompleted += InvitedSessionJoined;
                ScreenManager.AddScreen(busyScreen);
            }
            catch
            {
                // could not begin to join invited game, so default to the pre-existing MainMenuScreen
            }

            ScreenManager.invited = null;
            updateState = true;
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel()
        {
            const string message = "Exit Net Rumble?";
            MessageBoxScreen messageBox = new MessageBoxScreen(message);
            messageBox.Accepted += ExitMessageBoxAccepted;
            ScreenManager.AddScreen(messageBox);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ExitMessageBoxAccepted(object sender, EventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        /// <summary>
        /// Screen-specific update to gamer rich presence.
        /// </summary>
        public override void UpdatePresence()
        {
            foreach (SignedInGamer signedInGamer in Gamer.SignedInGamers)
            {
                if (signedInGamer.IsSignedInToLive)
                {
                    signedInGamer.Presence.PresenceMode = GamerPresenceMode.AtMenu;
                }
            }
        }

        #endregion


        #region Networking Methods


        private void QuickMatchSession()
        {
            // start the search
            try
            {
                IAsyncResult asyncResult = NetworkSession.BeginFind(
                    NetworkSessionType.PlayerMatch, 1, null, null, null);

                // create the busy screen
                NetworkBusyScreen busyScreen = new NetworkBusyScreen(
                    "Searching for a session...", asyncResult);
                busyScreen.OperationCompleted += QuickMatchSearchCompleted;
                ScreenManager.AddScreen(busyScreen);
            }
            catch (NetworkException ne)
            {
                const string message = "Failed searching for the session.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine("Failed to search for session:  " +
                    ne.Message);
            }
            catch (GamerPrivilegeException gpe)
            {
                const string message =
                    "You do not have permission to search for a session.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine(
                    "Insufficient privilege to search for session:  " + gpe.Message);
            }
        }


        /// <summary>
        /// Start creating a session of the given type.
        /// </summary>
        /// <param name="sessionType">The type of session to create.</param>
        void CreateSession(NetworkSessionType sessionType)
        {
            // create the session
            try
            {
                IAsyncResult asyncResult = NetworkSession.BeginCreate(sessionType, 1,
                    World.MaximumPlayers, null, null);

                // create the busy screen
                NetworkBusyScreen busyScreen = new NetworkBusyScreen(
                    "Creating a session...", asyncResult);
                busyScreen.OperationCompleted += SessionCreated;
                ScreenManager.AddScreen(busyScreen);
            }
            catch (NetworkException ne)
            {
                const string message = "Failed creating the session.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine("Failed to create session:  " +
                    ne.Message);
            }
            catch (GamerPrivilegeException gpe)
            {
                const string message =
                    "You do not have permission to create a session.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine(
                    "Insufficient privilege to create session:  " + gpe.Message);
            }
        }


        /// <summary>
        /// Start searching for a session of the given type.
        /// </summary>
        /// <param name="sessionType">The type of session to look for.</param>
        void FindSession(NetworkSessionType sessionType)
        {
            // create the new screen
            SearchResultsScreen searchResultsScreen =
               new SearchResultsScreen(sessionType);
            searchResultsScreen.ScreenManager = this.ScreenManager;
            ScreenManager.AddScreen(searchResultsScreen);

            // start the search
            try
            {
                IAsyncResult asyncResult = NetworkSession.BeginFind(sessionType, 1, null,
                    null, null);

                // create the busy screen
                NetworkBusyScreen busyScreen = new NetworkBusyScreen(
                    "Searching for a session...", asyncResult);
                busyScreen.OperationCompleted += searchResultsScreen.SessionsFound;
                ScreenManager.AddScreen(busyScreen);
            }
            catch (NetworkException ne)
            {
                const string message = "Failed searching for the session.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine("Failed to search for session:  " +
                    ne.Message);
            }
            catch (GamerPrivilegeException gpe)
            {
                const string message =
                    "You do not have permission to search for a session.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine(
                    "Insufficient privilege to search for session:  " + gpe.Message);
            }
        }


        /// <summary>
        /// Callback to receive the network-session search results from quick-match.
        /// </summary>
        void QuickMatchSearchCompleted(object sender, OperationCompletedEventArgs e)
        {
            try
            {
                AvailableNetworkSessionCollection availableSessions =
                    NetworkSession.EndFind(e.AsyncResult);
                if ((availableSessions != null) && (availableSessions.Count > 0))
                {
                    // join the session
                    try
                    {
                        IAsyncResult asyncResult = NetworkSession.BeginJoin(
                            availableSessions[0], null, null);

                        // create the busy screen
                        NetworkBusyScreen busyScreen = new NetworkBusyScreen(
                            "Joining the session...", asyncResult);
                        busyScreen.OperationCompleted += QuickMatchSessionJoined;
                        ScreenManager.AddScreen(busyScreen);
                    }
                    catch (NetworkException ne)
                    {
                        const string message = "Failed joining the session.";
                        MessageBoxScreen messageBox = new MessageBoxScreen(message);
                        messageBox.Accepted += FailedMessageBox;
                        messageBox.Cancelled += FailedMessageBox;
                        ScreenManager.AddScreen(messageBox);

                        System.Console.WriteLine("Failed to join session:  " +
                            ne.Message);
                    }
                    catch (GamerPrivilegeException gpe)
                    {
                        const string message =
                            "You do not have permission to join a session.";
                        MessageBoxScreen messageBox = new MessageBoxScreen(message);
                        messageBox.Accepted += FailedMessageBox;
                        messageBox.Cancelled += FailedMessageBox;
                        ScreenManager.AddScreen(messageBox);

                        System.Console.WriteLine(
                            "Insufficient privilege to join session:  " + gpe.Message);
                    }
                }
                else
                {
                    const string message = "No matches were found.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message);
                    messageBox.Accepted += FailedMessageBox;
                    messageBox.Cancelled += FailedMessageBox;
                    ScreenManager.AddScreen(messageBox);
                }
            }
            catch (GamerPrivilegeException gpe){
                MessageBoxScreen messageBox = new MessageBoxScreen(gpe.Message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);
            }
        }


        /// <summary>
        /// Callback when a session is created.
        /// </summary>
        void SessionCreated(object sender, OperationCompletedEventArgs e)
        {
            NetworkSession networkSession = null;
            try
            {
                networkSession = NetworkSession.EndCreate(e.AsyncResult);
            }
            catch (NetworkException ne)
            {
                const string message = "Failed creating the session.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine("Failed to create session:  " +
                    ne.Message);
            }
            catch (GamerPrivilegeException gpe)
            {
                const string message =
                    "You do not have permission to create a session. ";
                MessageBoxScreen messageBox = new MessageBoxScreen(message+gpe.Message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine(
                    "Insufficient privilege to create session:  " + gpe.Message);
            }
            if (networkSession != null)
            {
                networkSession.AllowHostMigration = true;
                networkSession.AllowJoinInProgress = false;
                LoadLobbyScreen(networkSession);
            }
        }


        /// <summary>
        /// Callback when a session is quick-matched.
        /// </summary>
        void QuickMatchSessionJoined(object sender, OperationCompletedEventArgs e)
        {
            NetworkSession networkSession = null;
            try
            {
                networkSession = NetworkSession.EndJoin(e.AsyncResult);
            }
            catch (NetworkException ne)
            {
                const string message = "Failed joining the session.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine("Failed to join session:  " +
                    ne.Message);
            }
            catch (GamerPrivilegeException gpe)
            {
                const string message =
                    "You do not have permission to join a session.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine(
                    "Insufficient privilege to join session:  " + gpe.Message);
            }
            if (networkSession != null)
            {
                LoadLobbyScreen(networkSession);
            }
        }


        /// <summary>
        /// Load the lobby screen with the new session.
        /// </summary>
        void LoadLobbyScreen(NetworkSession networkSession)
        {
            if (networkSession != null)
            {
                LobbyScreen lobbyScreen = new LobbyScreen(networkSession);
                lobbyScreen.ScreenManager = this.ScreenManager;
                ScreenManager.AddScreen(lobbyScreen);
            }
        }


        /// <summary>
        /// Finishes the asynchronous process of joining a game from an invitation,
        /// joining the lobby of a hosted game if the join was successful.
        /// </summary>
        void InvitedSessionJoined(object sender, OperationCompletedEventArgs e)
        {
            NetworkSession networkSession = null;
            try
            {
                networkSession = NetworkSession.EndJoinInvited(e.AsyncResult);
            }
            catch (NetworkSessionJoinException je)
            {
                const string message = "Failed joining the session (";
                MessageBoxScreen messageBox = new MessageBoxScreen(message + je.JoinError.ToString() + ").");
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine("Failed to join session:  " +
                    je.Message);
            }
            catch (Exception ge)
            {
                const string message = "Failed joining the session (";
                MessageBoxScreen messageBox = new MessageBoxScreen(message + ge.Message + ").");
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine("Failed to join session:  " +
                    ge.Message);
            }

            // Start the lobby if we got the session!
            // Otherwise the MainMenuScreen will be available.
            if (networkSession != null)
            {
                LobbyScreen lobbyScreen = new LobbyScreen(networkSession);
                lobbyScreen.ScreenManager = ScreenManager;
                ScreenManager.AddScreen(lobbyScreen);
            }
        }


        /// <summary>
        /// Event handler for when the user selects ok on the network-operation-failed
        /// message box.
        /// </summary>
        void FailedMessageBox(object sender, EventArgs e) { }


        #endregion
    }
}
