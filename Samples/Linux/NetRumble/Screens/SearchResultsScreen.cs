#region File Description
//-----------------------------------------------------------------------------
// SearchResultsScreen.cs
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
    /// The search-results screen shows the results of a network-session
    /// search, allowing the player to pick the game to join.
    /// </summary>
    public class SearchResultsScreen : MenuScreen
    {
        #region Constants


        /// <summary>
        /// The maximum number of session results to display.
        /// </summary>
        const int maximumSessions = 8;


        #endregion


        #region Networking Data


        /// <summary>
        /// The type of networking session that was requested.
        /// </summary>
        private NetworkSessionType sessionType;


        /// <summary>
        /// The collection of search results.
        /// </summary>
        private AvailableNetworkSessionCollection availableSessions = null;


        #endregion


        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        /// <param name="sessionType">The type of session searched for.</param>
        public SearchResultsScreen(NetworkSessionType sessionType) : base()
        {
            // apply the parameters
            this.sessionType = sessionType;

            // set the transition times
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Updates the screen. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime,
            bool otherScreenHasFocus, bool coveredByOtherScreen)
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
                if (!signedIntoLive &&
                    ((sessionType == NetworkSessionType.PlayerMatch) ||
                     (sessionType == NetworkSessionType.Ranked)) && !IsExiting)
                {
                    ExitScreen();
                }
            }
            else if (!IsExiting)
            {
                ExitScreen();
            }

            if (coveredByOtherScreen && !IsExiting)
            {
                ExitScreen();
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        /// <summary>
        /// Responds to user menu selections.
        /// </summary>
        protected override void OnSelectEntry(int entryIndex)
        {
            if ((availableSessions != null) && (entryIndex >= 0) && 
                (entryIndex < availableSessions.Count))
            {
                // start to join
                try
                {
                    IAsyncResult asyncResult = NetworkSession.BeginJoin(
                        availableSessions[entryIndex], null, null);

                    // create the busy screen
                    NetworkBusyScreen busyScreen = new NetworkBusyScreen(
                        "Joining the session...", asyncResult);
                    busyScreen.OperationCompleted += LoadLobbyScreen;
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
        }


        /// <summary>
        /// When the user cancels the screen.
        /// </summary>
        protected override void OnCancel()
        {
            if (availableSessions != null)
            {
                ExitScreen();
            }
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            string alternateString = String.Empty;

            // set an alternate string if there are no search results yet
            if (availableSessions == null)
            {
                alternateString = "Searching...";
            }
            else if (availableSessions.Count <= 0)
            {
                alternateString = "No sessions found.";
            }

            if (String.IsNullOrEmpty(alternateString))
            {
                base.Draw(gameTime);
            }
            else
            {
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);

                Vector2 position = new Vector2(0f, viewportSize.Y * 0.65f);

                // Make the menu slide into place during transitions, using a
                // power curve to make things look more interesting (this makes
                // the movement slow down as it nears the end).
                float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

                if (ScreenState == ScreenState.TransitionOn)
                    position.Y += transitionOffset * 256;
                else
                    position.Y += transitionOffset * 512;

                // Draw each menu entry in turn.
                ScreenManager.SpriteBatch.Begin();

                Vector2 origin = new Vector2(0, ScreenManager.Font.LineSpacing / 2);
                Vector2 size = ScreenManager.Font.MeasureString(alternateString);
                position.X = viewportSize.X / 2f - size.X / 2f;
                ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, 
                                                     alternateString, position, 
                                                     Color.White, 0, origin, 1.0f,
                                                     SpriteEffects.None, 0);

                ScreenManager.SpriteBatch.End();
            }
        }


        #endregion


        #region Networking Methods


        /// <summary>
        /// Callback to receive the network-session search results.
        /// </summary>
        internal void SessionsFound(object sender, OperationCompletedEventArgs e)
        {
            try
            {
                availableSessions = NetworkSession.EndFind(e.AsyncResult);
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
                    "You do not have permission to search for a session. ";
                MessageBoxScreen messageBox = new MessageBoxScreen(message + gpe.Message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine(
                    "Insufficient privilege to search for session:  " + gpe.Message);
            } 
            MenuEntries.Clear();
            if (availableSessions != null)
            {
                foreach (AvailableNetworkSession availableSession in
                    availableSessions)
                {
                    if (availableSession.CurrentGamerCount < World.MaximumPlayers)
                    {
                        MenuEntries.Add(availableSession.HostGamertag + " (" +
                            availableSession.CurrentGamerCount.ToString() + "/" +
                            World.MaximumPlayers.ToString() + ")");
                    }
                    if (MenuEntries.Count >= maximumSessions)
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Callback to load the lobby screen with the new session.
        /// </summary>
        private void LoadLobbyScreen(object sender, OperationCompletedEventArgs e)
        {
            NetworkSession networkSession = null;
            try
            {
                networkSession = NetworkSession.EndJoin(e.AsyncResult);
            }
            catch (NetworkException ne)
            {
                const string message = "Failed joining session.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                messageBox.Accepted += FailedMessageBox;
                messageBox.Cancelled += FailedMessageBox;
                ScreenManager.AddScreen(messageBox);

                System.Console.WriteLine("Failed joining session:  " + ne.Message);
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
                LobbyScreen lobbyScreen = new LobbyScreen(networkSession);
                lobbyScreen.ScreenManager = this.ScreenManager;
                ScreenManager.AddScreen(lobbyScreen);
            }
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        private void FailedMessageBox(object sender, EventArgs e)
        {
            ExitScreen();
        }


        #endregion
    }
}
