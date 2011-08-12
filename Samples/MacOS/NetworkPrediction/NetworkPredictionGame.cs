#region File Description
//-----------------------------------------------------------------------------
// NetworkPredictionGame.cs
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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
#endregion

namespace NetworkPrediction
{
    /// <summary>
    /// Sample showing how to use prediction and smoothing to compensate
    /// for the effects of network latency, and for the low packet send
    /// rates needed to conserve network bandwidth.
    /// </summary>
    public class NetworkPredictionGame : Microsoft.Xna.Framework.Game
    {
        #region Constants

        const int screenWidth = 1067;
        const int screenHeight = 600;

        const int maxGamers = 16;
        const int maxLocalGamers = 4;

        #endregion

        #region Fields


        // Graphics objects.
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;


        // Current and previous input states.
        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;

        KeyboardState previousKeyboardState;
        GamePadState previousGamePadState;


        // Network objects.
        NetworkSession networkSession;

        PacketWriter packetWriter = new PacketWriter();
        PacketReader packetReader = new PacketReader();
        
        string errorMessage;


        // What kind of network latency and packet loss are we simulating?
        enum NetworkQuality
        {
            Typical,    // 100 ms latency, 10% packet loss
            Poor,       // 200 ms latency, 20% packet loss
            Perfect,    // 0 latency, 0% packet loss
        }

        NetworkQuality networkQuality;

        // How often should we send network packets?
        int framesBetweenPackets = 6;

        // How recently did we send the last network packet?
        int framesSinceLastSend;

        // Is prediction and/or smoothing enabled?
        bool enablePrediction = true;
        bool enableSmoothing = true;


        #endregion

        #region Initialization


        public NetworkPredictionGame()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;            

            Content.RootDirectory = "Content";

            Components.Add(new GamerServicesComponent(this));
        }


        /// <summary>
        /// Load your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            font = Content.Load<SpriteFont>("Font");
        }


        #endregion

        #region Update


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            if (networkSession == null)
            {
                // If we are not in a network session, update the
                // menu screen that will let us create or join one.
                UpdateMenuScreen();
            }
            else
            {
                // If we are in a network session, update it.
                UpdateNetworkSession(gameTime);
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Menu screen provides options to create or join network sessions.
        /// </summary>
        void UpdateMenuScreen()
        {
            if (IsActive)
            {
                if (Gamer.SignedInGamers.Count == 0)
                {
                    // If there are no profiles signed in, we cannot proceed.
                    // Show the Guide so the user can sign in.
                    Guide.ShowSignIn(maxLocalGamers, false);
                }
                else
                if (IsPressed(Keys.A, Buttons.A))
                {
                    // Create a new session?
                    CreateSession();
                }
                else if (IsPressed(Keys.B, Buttons.B))
                {
                    // Join an existing session?
                    JoinSession();
                }
            }
        }


        /// <summary>
        /// Starts hosting a new network session.
        /// </summary>
        void CreateSession()
        {
            DrawMessage("Creating session...");

            try
            {
                networkSession = NetworkSession.Create(NetworkSessionType.SystemLink,
                                                       maxLocalGamers, maxGamers);

                HookSessionEvents();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }


        /// <summary>
        /// Joins an existing network session.
        /// </summary>
        void JoinSession()
        {
            DrawMessage("Joining session...");

            try
            {
                // Search for sessions.
                using (AvailableNetworkSessionCollection availableSessions =
                            NetworkSession.Find(NetworkSessionType.SystemLink,
                                                maxLocalGamers, null))
                {
                    if (availableSessions.Count == 0)
                    {
                        errorMessage = "No network sessions found.";
                        return;
                    }

                    // Join the first session we found.
                    networkSession = NetworkSession.Join(availableSessions[0]);

                    HookSessionEvents();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }


        /// <summary>
        /// After creating or joining a network session, we must subscribe to
        /// some events so we will be notified when the session changes state.
        /// </summary>
        void HookSessionEvents()
        {
            networkSession.GamerJoined += GamerJoinedEventHandler;
            networkSession.SessionEnded += SessionEndedEventHandler;
        }


        /// <summary>
        /// This event handler will be called whenever a new gamer joins the session.
        /// We use it to allocate a Tank object, and associate it with the new gamer.
        /// </summary>
        void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            int gamerIndex = networkSession.AllGamers.IndexOf(e.Gamer);

            e.Gamer.Tag = new Tank(gamerIndex, Content, screenWidth, screenHeight);
        }


        /// <summary>
        /// Event handler notifies us when the network session has ended.
        /// </summary>
        void SessionEndedEventHandler(object sender, NetworkSessionEndedEventArgs e)
        {
            errorMessage = e.EndReason.ToString();

            networkSession.Dispose();
            networkSession = null;
        }


        /// <summary>
        /// Updates the state of the network session, moving the tanks
        /// around and synchronizing their state over the network.
        /// </summary>
        void UpdateNetworkSession(GameTime gameTime)
        {
            // Is it time to send outgoing network packets?
            bool sendPacketThisFrame = false;

            framesSinceLastSend++;

            if (framesSinceLastSend >= framesBetweenPackets)
            {
                sendPacketThisFrame = true;
                framesSinceLastSend = 0;
            }

            // Update our locally controlled tanks, sending
            // their latest state at periodic intervals.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                UpdateLocalGamer(gamer, gameTime, sendPacketThisFrame);
            }

            // Pump the underlying session object.
            try
            {
                networkSession.Update();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                networkSession.Dispose();
                networkSession = null;
            }

            // Make sure the session has not ended.
            if (networkSession == null)
                return;

            // Read any packets telling us the state of remotely controlled tanks.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                ReadIncomingPackets(gamer, gameTime);
            }

            // Apply prediction and smoothing to the remotely controlled tanks.
            foreach (NetworkGamer gamer in networkSession.RemoteGamers)
            {
                Tank tank = gamer.Tag as Tank;

                tank.UpdateRemote(framesBetweenPackets, enablePrediction);
            }

            // Update the latency and packet loss simulation options.
            UpdateOptions();
        }


        /// <summary>
        /// Helper for updating a locally controlled gamer.
        /// </summary>
        void UpdateLocalGamer(LocalNetworkGamer gamer, GameTime gameTime,
                                                       bool sendPacketThisFrame)
        {
            // Look up what tank is associated with this local player.
            Tank tank = gamer.Tag as Tank;

            // Read the inputs controlling this tank.
            PlayerIndex playerIndex = gamer.SignedInGamer.PlayerIndex;

            Vector2 tankInput;
            Vector2 turretInput;

            ReadTankInputs(playerIndex, out tankInput, out turretInput);

            // Update the tank.
            tank.UpdateLocal(tankInput, turretInput);

            // Periodically send our state to everyone in the session.
            if (sendPacketThisFrame)
            {
                tank.WriteNetworkPacket(packetWriter, gameTime);

                gamer.SendData(packetWriter, SendDataOptions.InOrder);
            }
        }


        /// <summary>
        /// Helper for reading incoming network packets.
        /// </summary>
        void ReadIncomingPackets(LocalNetworkGamer gamer, GameTime gameTime)
        {
            // Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                gamer.ReceiveData(packetReader, out sender);

                // Discard packets sent by local gamers: we already know their state!
                if (sender.IsLocal)
                    continue;

                // Look up the tank associated with whoever sent this packet.
                Tank tank = sender.Tag as Tank;

                // Estimate how long this packet took to arrive.
                TimeSpan latency = networkSession.SimulatedLatency +
                                   TimeSpan.FromTicks(sender.RoundtripTime.Ticks / 2);

                // Read the state of this tank from the network packet.
                tank.ReadNetworkPacket(packetReader, gameTime, latency,
                                       enablePrediction, enableSmoothing);
            }
        }


        /// <summary>
        /// Updates the latency and packet loss simulation options. Only the
        /// host can alter these values, which are then synchronized over the
        /// network by storing them into NetworkSession.SessionProperties. Any
        /// changes to the SessionProperties data are automatically replicated
        /// on all the client machines, so there is no need to manually send
        /// network packets to transmit this data.
        /// </summary>
        void UpdateOptions()
        {
            if (networkSession.IsHost)
            {
                // Change the network quality simultation?
                if (IsPressed(Keys.A, Buttons.A))
                {
                    networkQuality++;

                    if (networkQuality > NetworkQuality.Perfect)
                        networkQuality = 0;
                }

                // Change the packet send rate?
                if (IsPressed(Keys.B, Buttons.B))
                {
                    if (framesBetweenPackets == 6)
                        framesBetweenPackets = 3;
                    else if (framesBetweenPackets == 3)
                        framesBetweenPackets = 1;
                    else
                        framesBetweenPackets = 6;
                }

                // Toggle prediction on or off?
                if (IsPressed(Keys.X, Buttons.X))
                    enablePrediction = !enablePrediction;

                // Toggle smoothing on or off?
                if (IsPressed(Keys.Y, Buttons.Y))
                    enableSmoothing = !enableSmoothing;

                // Stores the latest settings into NetworkSession.SessionProperties.
                networkSession.SessionProperties[0] = (int)networkQuality;
                networkSession.SessionProperties[1] = framesBetweenPackets;
                networkSession.SessionProperties[2] = enablePrediction ? 1 : 0;
                networkSession.SessionProperties[3] = enableSmoothing ? 1 : 0;
            }
            else
            {
                // Client machines read the latest settings from the session properties.
                networkQuality = (NetworkQuality)networkSession.SessionProperties[0];
                framesBetweenPackets = networkSession.SessionProperties[1].Value;
                enablePrediction = networkSession.SessionProperties[2] != 0;
                enableSmoothing = networkSession.SessionProperties[3] != 0;
            }

            // Update the SimulatedLatency and SimulatedPacketLoss properties.
            switch (networkQuality)
            {
                case NetworkQuality.Typical:
                    networkSession.SimulatedLatency = TimeSpan.FromMilliseconds(100);
                    networkSession.SimulatedPacketLoss = 0.1f;
                    break;

                case NetworkQuality.Poor:
                    networkSession.SimulatedLatency = TimeSpan.FromMilliseconds(200);
                    networkSession.SimulatedPacketLoss = 0.2f;
                    break;

                case NetworkQuality.Perfect:
                    networkSession.SimulatedLatency = TimeSpan.Zero;
                    networkSession.SimulatedPacketLoss = 0;
                    break;
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (networkSession == null)
            {
                // If we are not in a network session, draw the
                // menu screen that will let us create or join one.
                DrawMenuScreen();
            }
            else
            {
                // If we are in a network session, draw it.
                DrawNetworkSession();
            }

            base.Draw(gameTime);
        }


        /// <summary>
        /// Draws the startup screen used to create and join network sessions.
        /// </summary>
        void DrawMenuScreen()
        {
            string message = string.Empty;

            if (!string.IsNullOrEmpty(errorMessage))
                message += "Error:\n" + errorMessage.Replace(". ", ".\n") + "\n\n";

            message += "A = create session\n" +
                       "B = join session";

            spriteBatch.Begin();

            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);
            
            spriteBatch.End();
        }


        /// <summary>
        /// Draws the state of an active network session.
        /// </summary>
        void DrawNetworkSession()
        {
            spriteBatch.Begin();

            DrawOptions();

            // For each person in the session...
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                // Look up the tank object belonging to this network gamer.
                Tank tank = gamer.Tag as Tank;

                // Draw the tank.
                tank.Draw(spriteBatch);

                // Draw a gamertag label.
                spriteBatch.DrawString(font, gamer.Gamertag, tank.Position,
                                       Color.Black, 0, new Vector2(100, 150),
                                       0.6f, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }


        /// <summary>
        /// Draws the current latency and packet loss simulation settings.
        /// </summary>
        void DrawOptions()
        {
            string quality =
                string.Format("Network simulation = {0} ms, {1}% packet loss",
                              networkSession.SimulatedLatency.TotalMilliseconds,
                              networkSession.SimulatedPacketLoss * 100);

            string sendRate = string.Format("Packets per second = {0}",
                                            60 / framesBetweenPackets);

            string prediction = string.Format("Prediction = {0}",
                                              enablePrediction ? "on" : "off");

            string smoothing = string.Format("Smoothing = {0}",
                                             enableSmoothing ? "on" : "off");

            // If we are the host, include prompts telling how to change the settings.
            if (networkSession.IsHost)
            {
                quality += " (A to change)";
                sendRate += " (B to change)";
                prediction += " (X to toggle)";
                smoothing += " (Y to toggle)";
            }

            // Draw combined text to the screen.
            string message = quality + "\n" +
                             sendRate + "\n" +
                             prediction + "\n" +
                             smoothing;

            spriteBatch.DrawString(font, message, new Vector2(161, 321), Color.Black);
            spriteBatch.DrawString(font, message, new Vector2(160, 320), Color.White);
        }


        /// <summary>
        /// Helper draws notification messages before calling blocking network methods.
        /// </summary>
        void DrawMessage(string message)
        {
            if (!BeginDraw())
                return;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);

            spriteBatch.End();

            EndDraw();
        }

        
        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input.
        /// </summary>
        private void HandleInput()
        {
            previousKeyboardState = currentKeyboardState;
            previousGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (IsActive && IsPressed(Keys.Escape, Buttons.Back))
            {
                Exit();
            }
        }


        /// <summary>
        /// Checks if the specified button is pressed on either keyboard or gamepad.
        /// </summary>
        bool IsPressed(Keys key, Buttons button)
        {
            return ((currentKeyboardState.IsKeyDown(key) &&
                     previousKeyboardState.IsKeyUp(key)) ||
                    (currentGamePadState.IsButtonDown(button) &&
                     previousGamePadState.IsButtonUp(button)));
        }


        /// <summary>
        /// Reads input data from keyboard and gamepad, and returns
        /// this as output parameters ready for use by the tank update.
        /// </summary>
        static void ReadTankInputs(PlayerIndex playerIndex, out Vector2 tankInput,
                                                            out Vector2 turretInput)
        {
            // Read the gamepad.
            GamePadState gamePad = GamePad.GetState(playerIndex);

            tankInput = gamePad.ThumbSticks.Left;
            turretInput = gamePad.ThumbSticks.Right;

            // Read the keyboard.
            KeyboardState keyboard = Keyboard.GetState(playerIndex);

            if (keyboard.IsKeyDown(Keys.Left))
                tankInput.X = -1;
            else if (keyboard.IsKeyDown(Keys.Right))
                tankInput.X = 1;

            if (keyboard.IsKeyDown(Keys.Up))
                tankInput.Y = 1;
            else if (keyboard.IsKeyDown(Keys.Down))
                tankInput.Y = -1;

            if (keyboard.IsKeyDown(Keys.K))
                turretInput.X = -1;
            else if (keyboard.IsKeyDown(Keys.OemSemicolon))
                turretInput.X = 1;

            if (keyboard.IsKeyDown(Keys.O))
                turretInput.Y = 1;
            else if (keyboard.IsKeyDown(Keys.L))
                turretInput.Y = -1;

            // Normalize the input vectors.
            if (tankInput.Length() > 1)
                tankInput.Normalize();

            if (turretInput.Length() > 1)
                turretInput.Normalize();
        }


        #endregion
    }
    
}
