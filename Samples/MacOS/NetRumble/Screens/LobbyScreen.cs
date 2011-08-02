#region File Description
//-----------------------------------------------------------------------------
// LobbyScreen.cs
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
    /// The lobby screen is shows the players in the game, outside the gameplay.
    /// </summary>
    public class LobbyScreen : MenuScreen, IDisposable
    {
        #region Constants


        /// <summary>
        /// The instructions shown to the player at the bottom of the lobby screen.
        /// </summary>
        const string instructions = 
            "Press X to mark/unmark ready, LB/RB to toggle color, LT/RT to toggle ship";


        #endregion


        #region Gameplay Data


        /// <summary>
        /// The primary object for this game.
        /// </summary>
        private World world;


        #endregion


        #region Networking Data


        /// <summary>
        /// The network session for this game.
        /// </summary>
        private NetworkSession networkSession;

        /// <summary>
        /// The packet writer used to send data from this screen.
        /// </summary>
        private PacketWriter packetWriter = new PacketWriter();
        
        /// <summary>
        /// Event handler for the session-ended event.
        /// </summary>
        EventHandler<NetworkSessionEndedEventArgs> sessionEndedHandler;

        /// <summary>
        /// Event handler for the game-ended event.
        /// </summary>
        EventHandler<GameStartedEventArgs> gameStartedHandler;

        /// <summary>
        /// Event handler for the gamer-left event.
        /// </summary>
        EventHandler<GamerJoinedEventArgs> gamerJoinedHandler;


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new LobbyScreen object.
        /// </summary>
        public LobbyScreen(NetworkSession networkSession) : base()
        {
            // safety-check the parameter
            if (networkSession == null)
            {
                throw new ArgumentNullException("networkSession");
            }

            // apply the parameters
            this.networkSession = networkSession;

            // add the single menu entry
            MenuEntries.Add("");

            // set the transition time
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);

            gamerJoinedHandler = new EventHandler<GamerJoinedEventArgs>(
                networkSession_GamerJoined);
            gameStartedHandler = new EventHandler<GameStartedEventArgs>(
                networkSession_GameStarted);
            sessionEndedHandler = new EventHandler<NetworkSessionEndedEventArgs>(
                networkSession_SessionEnded);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            // create the world object
            world = new World(ScreenManager.GraphicsDevice, ScreenManager.Content, 
                networkSession);

            // set the networking events
            networkSession.GamerJoined += gamerJoinedHandler;
            networkSession.GameStarted += gameStartedHandler;
            networkSession.SessionEnded += sessionEndedHandler;
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Updates the lobby. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime,
            bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (networkSession != null)
            {
                // update the network session
                try
                {
                    networkSession.Update();
                }
                catch (NetworkException ne)
                {
                    System.Console.WriteLine(
                        "Network failed to update:  " + ne.ToString());
                    if (networkSession != null)
                    {
                        networkSession.Dispose();
                        networkSession = null;
                    }
                }
            }

            // update the world
            if ((world != null) && !otherScreenHasFocus && !coveredByOtherScreen)
            {
                if (world.GameWon)
                {
                    // unload the existing world
                    world.Dispose();
                    world = null;
                    // make sure that all of the ships have cleaned up
                    foreach (NetworkGamer networkGamer in networkSession.AllGamers)
                    {
                        PlayerData playerData = networkGamer.Tag as PlayerData;
                        if ((playerData != null) && (playerData.Ship != null))
                        {
                            playerData.Ship.Die(null, true);
                        }
                    }
                    // make sure the collision manager is up-to-date
                    CollisionManager.Collection.ApplyPendingRemovals();
                    // create a new world
                    world = new World(ScreenManager.GraphicsDevice,
                        ScreenManager.Content, networkSession);
                }
                else if (world.GameExited)
                {
                    if (!IsExiting)
                    {
                        ExitScreen();
                    }
                    if (world != null)
                    {
                        world.Dispose();
                        world = null;
                    }
                    if (networkSession != null)
                    {
                        networkSession.Dispose();
                        networkSession = null;
                    }
                    base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
                    return;
                }
                else
                {
                    world.Update((float)gameTime.ElapsedGameTime.TotalSeconds, true);
                }
            }

            // update the menu entry text
            if (otherScreenHasFocus == false)
            {
                if ((networkSession.LocalGamers.Count > 0) && 
                    (networkSession.SessionState == NetworkSessionState.Lobby))
                {
                    if (!networkSession.LocalGamers[0].IsReady)
                    {
                        MenuEntries[0] = "Press X to Mark as Ready";
                    }
                    else if (!networkSession.IsEveryoneReady)
                    {
                        MenuEntries[0] = "Waiting for all players to mark as ready...";
                    }
                    else if (!networkSession.IsHost)
                    {
                        MenuEntries[0] = "Waiting for the host to start game...";
                    }
                    else
                    {
                        MenuEntries[0] = "Starting the game...";
                        networkSession.StartGame();
                    }
                }
                else if (networkSession.SessionState == NetworkSessionState.Playing)
                {
                    MenuEntries[0] = "Game starting...";
                }
                // if the game is playing and the world is initialized, then start up
                if ((networkSession.SessionState == NetworkSessionState.Playing) && 
                    (world != null) && world.Initialized)
                {
                    GameplayScreen gameplayScreen = 
                        new GameplayScreen(networkSession, world);
                    gameplayScreen.ScreenManager = this.ScreenManager;
                    ScreenManager.AddScreen(gameplayScreen);
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            // safety-check the parameter
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if ((networkSession != null) && (networkSession.LocalGamers.Count > 0))
            {
                // update the ready state
                if (input.MarkReady)
                {
                    networkSession.LocalGamers[0].IsReady = 
                        !networkSession.LocalGamers[0].IsReady;
                }

                // update the player data
                PlayerData playerData = networkSession.LocalGamers[0].Tag as PlayerData;
                if (playerData != null)
                {
                    bool playerDataChanged = false;
                    if (input.ShipColorChangeUp)
                    {
                        playerData.ShipColor = Ship.GetNextUniqueColorIndex(
                            playerData.ShipColor, networkSession);
                        playerDataChanged = true;
                    }
                    else if (input.ShipColorChangeDown)
                    {
                        playerData.ShipColor = Ship.GetPreviousUniqueColorIndex(
                            playerData.ShipColor, networkSession);
                        playerDataChanged = true;
                    }
                    if (input.ShipModelChangeUp)
                    {
                        playerData.ShipVariation = 
                            (byte)((playerData.ShipVariation + 1) % 4);
                        playerDataChanged = true;
                    }
                    else if (input.ShipModelChangeDown)
                    {
                        if (playerData.ShipVariation == 0)
                        {
                            playerData.ShipVariation = 3;
                        }
                        else
                        {
                            playerData.ShipVariation--;
                        }
                        playerDataChanged = true;
                    }
                    // if the data changed, send an update to the others
                    if (playerDataChanged)
                    {
                        packetWriter.Write((int)World.PacketTypes.PlayerData);
                        playerData.Serialize(packetWriter);
                        networkSession.LocalGamers[0].SendData(packetWriter, 
                            SendDataOptions.ReliableInOrder);
                    }
                }
            }

            base.HandleInput(input);
        }


        /// <summary>
        /// Responds to user menu selections.
        /// </summary>
        protected override void OnSelectEntry(int entryIndex) { }


        /// <summary>
        /// Force the end of a network session so that a new one can be joined.
        /// </summary>
        public void EndSession()
        {
            if (networkSession != null)
            {
                networkSession.Dispose();
                networkSession = null;
            }
        }


        /// <summary>
        /// Exit this screen.
        /// </summary>
        public override void ExitScreen()
        {
            if (!IsExiting && (networkSession != null))
            {
                networkSession.GamerJoined -= gamerJoinedHandler;
                networkSession.GameStarted -= gameStartedHandler;
                networkSession.SessionEnded -= sessionEndedHandler;
            }
            base.ExitScreen();
        }


        /// <summary>
        /// Screen-specific update to gamer rich presence.
        /// </summary>
        public override void UpdatePresence()
        {
            if (!IsExiting && (networkSession != null))
            {
                foreach (LocalNetworkGamer localGamer in networkSession.LocalGamers)
                {
                    SignedInGamer signedInGamer = localGamer.SignedInGamer;
                    if (signedInGamer.IsSignedInToLive)
                    {
                        if (networkSession.IsHost)
                        {
                            signedInGamer.Presence.PresenceMode = GamerPresenceMode.WaitingForPlayers;
                        }
                        else
                        {
                            signedInGamer.Presence.PresenceMode = GamerPresenceMode.WaitingInLobby;
                        }
                    }
                }
            }
        }

        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the lobby screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // draw in four columns
            Vector2[] columnPositions = new Vector2[4];
            columnPositions[0] = new Vector2(
                ScreenManager.GraphicsDevice.Viewport.Width * 0.2f, 
                ScreenManager.GraphicsDevice.Viewport.Height * 0.70f);
            columnPositions[1] = new Vector2(
                ScreenManager.GraphicsDevice.Viewport.Width * 0.4f, 
                ScreenManager.GraphicsDevice.Viewport.Height * 0.70f);
            columnPositions[2] = new Vector2(
                ScreenManager.GraphicsDevice.Viewport.Width * 0.6f,
                ScreenManager.GraphicsDevice.Viewport.Height * 0.70f);
            columnPositions[3] = new Vector2(
                ScreenManager.GraphicsDevice.Viewport.Width * 0.8f, 
                ScreenManager.GraphicsDevice.Viewport.Height * 0.70f);

            ScreenManager.SpriteBatch.Begin();

            // draw all of the players data
            if (networkSession != null)
            {
                for (int i = 0; i < networkSession.AllGamers.Count; i++)
                {
                    world.DrawPlayerData((float)gameTime.TotalGameTime.TotalSeconds, 
                        networkSession.AllGamers[i], columnPositions[i % 4], 
                        ScreenManager.SpriteBatch, true);
                    columnPositions[i % 4].Y += 
                        ScreenManager.GraphicsDevice.Viewport.Height * 0.03f;
                }
            }

            // draw the instructions
            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, instructions, 
                new Vector2(ScreenManager.TitleSafeArea.X, 
                ScreenManager.TitleSafeArea.Y + ScreenManager.TitleSafeArea.Height - 
                ScreenManager.Font.LineSpacing), Color.White);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }



        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel()
        {
            if (!IsExiting)
            {
                ExitScreen();
            }
            if (world != null)
            {
                world.Dispose();
                world = null;
            }
            if (networkSession != null)
            {
                networkSession.Dispose();
                networkSession = null;
            }
        }


        #endregion


        #region Networking Event Handlers


        /// <summary>
        /// Handle the end of the network session.
        /// </summary>
        void networkSession_SessionEnded(object sender, NetworkSessionEndedEventArgs e)
        {
            if (!IsExiting)
            {
                ExitScreen();
            }
            if (world != null)
            {
                world.Dispose();
                world = null;
            }
            if (networkSession != null)
            {
                networkSession.Dispose();
                networkSession = null;
            }
        }


        /// <summary>
        /// Handle the start of the game session.
        /// </summary>
        void networkSession_GameStarted(object sender, GameStartedEventArgs e)
        {
            // if we're the host, generate the data
            if ((networkSession != null) && networkSession.IsHost && (world != null))
            {
                world.GenerateWorld();
            }
        }
        
        
        /// <summary>
        /// Handle a new player joining the session.
        /// </summary>
        void networkSession_GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            // make sure the data exists for the new gamer
            for (int i = 0; i < networkSession.AllGamers.Count; i++)
            {
                if (networkSession.AllGamers[i] == e.Gamer)
                {
                    PlayerData playerData = new PlayerData();
                    e.Gamer.Tag = playerData;
                    playerData.ShipVariation = (byte)(i % 4);
                    playerData.ShipColor = (byte)i;
                }
            }

            // send my own data to the new gamer
            if ((networkSession.LocalGamers.Count > 0) && !e.Gamer.IsLocal)
            {
                PlayerData playerData = networkSession.LocalGamers[0].Tag as PlayerData;
                if (playerData != null)
                {
                    packetWriter.Write((int)World.PacketTypes.PlayerData);
                    playerData.Serialize(packetWriter);
                    networkSession.LocalGamers[0].SendData(packetWriter, 
                        SendDataOptions.ReliableInOrder, e.Gamer);
                }
            }
        }


        #endregion

    
        #region IDisposable Implementation


        /// <summary>
        /// Finalizes the LobbyScreen object, calls Dispose(false)
        /// </summary>
        ~LobbyScreen()
        {
            Dispose(false);
        }


        /// <summary>
        /// Disposes the LobbyScreen object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        
        /// <summary>
        /// Disposes this object.
        /// </summary>
        /// <param name="disposing">
        /// True if this method was called as part of the Dispose method.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    if (world != null)
                    {
                        world.Dispose();
                        world = null;
                    }
                    if (packetWriter != null)
                    {
                        packetWriter.Close();
                        packetWriter = null;
                    }
                }
            }
        }


        #endregion
    }
}
