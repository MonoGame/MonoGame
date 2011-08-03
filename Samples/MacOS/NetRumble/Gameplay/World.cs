#region File Description
//-----------------------------------------------------------------------------
// World.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
#endregion

namespace NetRumble
{
    /// <summary>
    /// A container for the game-specific logic and code.
    /// </summary>
    public class World : IDisposable
    {
        #region Public Constants

        /// <summary>
        /// The maximum number of players in the game.
        /// </summary>
        public const int MaximumPlayers = 16;

        /// <summary>
        /// The different types of packets sent in the game.
        /// </summary>
        /// <remarks>Frequently used in packets to identify their type.</remarks>
        public enum PacketTypes
        {
            PlayerData,
            ShipData,
            WorldSetup,
            WorldData,
            ShipInput,
            PowerUpSpawn,
            ShipDeath,
            ShipSpawn,
            GameWon,
        };


        #endregion


        #region Constants


        /// <summary>
        /// The score required to win the game.
        /// </summary>
        const int winningScore = 5;

        /// <summary>
        /// The number of asteroids in the game.
        /// </summary>
        const int numberOfAsteroids = 15;

        /// <summary>
        /// The length of time it takes for another power-up to spawn.
        /// </summary>
        const float maximumPowerUpTimer = 10f;

        /// <summary>
        /// The size of all of the barriers in the game.
        /// </summary>
        const int barrierSize = 48;

        /// <summary>
        /// The number of updates between WorldData packets.
        /// </summary>
        const int updatesBetweenWorldDataSend = 30;

        /// <summary>
        /// The number of updates between ship status packets from this machine.
        /// </summary>
        const int updatesBetweenStatusPackets = MaximumPlayers;

        /// <summary>
        /// The number of barriers in each dimension.
        /// </summary>
        static readonly Point barrierCounts = new Point(50, 50);

        /// <summary>
        /// The dimensions of the game world.
        /// </summary>
        static readonly Rectangle dimensions = new Rectangle(0, 0, 
            barrierCounts.X * barrierSize, barrierCounts.Y * barrierSize);


        #endregion


        #region State Data


        /// <summary>
        /// If true, the game has been initialized by receiving a WorldSetup packet.
        /// </summary>
        bool initialized = false;
        public bool Initialized
        {
            get { return initialized; }
        }
        
        /// <summary>
        /// If true, the game is over, and somebody has won.
        /// </summary>
        private bool gameWon = false;
        public bool GameWon
        {
            get { return gameWon; }
            set { gameWon = value; }
        }

        /// <summary>
        /// The index of the player who won the game.
        /// </summary>
        private int winnerIndex = -1;
        public int WinnerIndex
        {
            get { return winnerIndex; }
        }


        /// <summary>
        /// If true, the game is over, because the game ended before somebody won.
        /// </summary>
        /// <remarks></remarks>
        private bool gameExited = false;
        public bool GameExited
        {
            get { return gameExited; }
            set { gameExited = value; }
        }


        // presence support
        private List<int> highScorers = new List<int>();
        public List<int> HighScorers
        {
            get { return highScorers; }
        }

        #endregion


        #region Gameplay Data


        /// <summary>
        /// The number of asteroids in the game.
        /// </summary>
        Asteroid[] asteroids = new Asteroid[numberOfAsteroids];

        /// <summary>
        /// The current power-up in the game.
        /// </summary>
        PowerUp powerUp = null;

        /// <summary>
        /// The amount of time left until the next power-up spawns.
        /// </summary>
        float powerUpTimer = maximumPowerUpTimer / 2f;


        #endregion


        #region Graphics Data


        /// <summary>
        /// The sprite batch used to draw the objects in the world.
        /// </summary>
        private SpriteBatch spriteBatch;

        /// <summary>
        /// The corner-barrier texture.
        /// </summary>
        private Texture2D cornerBarrierTexture;

        /// <summary>
        /// The vertical-barrier texture.
        /// </summary>
        private Texture2D verticalBarrierTexture;

        /// <summary>
        /// The horizontal-barrier texture.
        /// </summary>
        private Texture2D horizontalBarrierTexture;

        /// <summary>
        /// The texture signifying that the player can chat.
        /// </summary>
        private Texture2D chatAbleTexture;

        /// <summary>
        /// The texture signifying that the player has been muted.
        /// </summary>
        private Texture2D chatMuteTexture;

        /// <summary>
        /// The texture signifying that the player is talking right now.
        /// </summary>
        private Texture2D chatTalkingTexture;

        /// <summary>
        /// The texture signifying that the player is ready
        /// </summary>
        private Texture2D readyTexture;

        /// <summary>
        /// The sprite used to draw the player names.
        /// </summary>
        private SpriteFont playerFont;
        public SpriteFont PlayerFont
        {
            get { return playerFont; }
        }
        
        /// <summary>
        /// The list of corner barriers in the game world.
        /// </summary>
        /// <remarks>This list is not owned by this object.</remarks>
        private List<Rectangle> cornerBarriers = new List<Rectangle>();

        /// <summary>
        /// The list of vertical barriers in the game world.
        /// </summary>
        /// <remarks>This list is not owned by this object.</remarks>
        private List<Rectangle> verticalBarriers = new List<Rectangle>();

        /// <summary>
        /// The list of horizontal barriers in the game world.
        /// </summary>
        /// <remarks>This list is not owned by this object.</remarks>
        private List<Rectangle> horizontalBarriers = new List<Rectangle>();

        /// <summary>
        /// The particle-effect manager for the game.
        /// </summary>
        ParticleEffectManager particleEffectManager;


        #endregion


        #region Networking Data


        /// <summary>
        /// The network session for the game.
        /// </summary>
        private NetworkSession networkSession;

        /// <summary>
        /// The packet writer for all of the data for the world.
        /// </summary>
        private PacketWriter packetWriter = new PacketWriter();

        /// <summary>
        /// The packet reader for all of the data for the world.
        /// </summary>
        private PacketReader packetReader = new PacketReader();

        /// <summary>
        /// The number of updates that have passed since the world data was sent.
        /// </summary>
        private int updatesSinceWorldDataSend = 0;

        /// <summary>
        /// The number of updates that have passed since a status packet was sent.
        /// </summary>
        private int updatesSinceStatusPacket = 0;


        #endregion


        #region Initialization


        /// <summary>
        /// Construct a new World object.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device used for this game.</param>
        /// <param name="networkSession">The network session for this game.</param>
        public World(GraphicsDevice graphicsDevice, ContentManager contentManager, 
            NetworkSession networkSession)
        {
            // safety-check the parameters, as they must be valid
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }
            if (networkSession == null)
            {
                throw new ArgumentNullException("networkSession");
            }

            // apply the parameter values
            this.networkSession = networkSession;

            // set up the staggered status packet system
            // -- your first update happens based on where you are in the collection
            for (int i = 0; i < networkSession.AllGamers.Count; i++)
            {
                if (networkSession.AllGamers[i].IsLocal)
                {
                    updatesSinceStatusPacket = i;
                    break;
                }
            }

            // create the spritebatch
            spriteBatch = new SpriteBatch(graphicsDevice);

            // create and initialize the particle-effect manager
            particleEffectManager = new ParticleEffectManager(contentManager);
            particleEffectManager.RegisterParticleEffect(
                ParticleEffectType.LaserExplosion,
                "Particles/laserExplosion.xml", 40);
            particleEffectManager.RegisterParticleEffect(
                ParticleEffectType.MineExplosion, 
                "Particles/mineExplosion.xml", 8);
            particleEffectManager.RegisterParticleEffect(
                ParticleEffectType.RocketExplosion, 
                "Particles/rocketExplosion.xml", 24);
            particleEffectManager.RegisterParticleEffect(
                ParticleEffectType.RocketTrail, 
                "Particles/rocketTrail.xml", 16);
            particleEffectManager.RegisterParticleEffect(
                ParticleEffectType.ShipExplosion, 
                "Particles/shipExplosion.xml", 4);
            particleEffectManager.RegisterParticleEffect(
                ParticleEffectType.ShipSpawn, 
                "Particles/shipSpawn.xml", 4);
            Ship.ParticleEffectManager = particleEffectManager;
            RocketProjectile.ParticleEffectManager = particleEffectManager;
            MineProjectile.ParticleEffectManager = particleEffectManager;
            LaserProjectile.ParticleEffectManager = particleEffectManager;

            // load the font
            playerFont = contentManager.Load<SpriteFont>("Fonts/NetRumbleFont");

            // load the gameplay-object textures
            Ship.LoadContent(contentManager);
            Asteroid.LoadContent(contentManager);
            LaserProjectile.LoadContent(contentManager);
            MineProjectile.LoadContent(contentManager);
            RocketProjectile.LoadContent(contentManager);
            DoubleLaserPowerUp.LoadContent(contentManager);
            TripleLaserPowerUp.LoadContent(contentManager);
            RocketPowerUp.LoadContent(contentManager);
            
            // load the non-gameplay-object textures
            chatAbleTexture = contentManager.Load<Texture2D>("Textures/chatAble");
            chatMuteTexture = contentManager.Load<Texture2D>("Textures/chatMute");
            chatTalkingTexture = contentManager.Load<Texture2D>("Textures/chatTalking");
            readyTexture = contentManager.Load<Texture2D>("Textures/ready");
            cornerBarrierTexture = 
                contentManager.Load<Texture2D>("Textures/barrierEnd");
            verticalBarrierTexture = 
                contentManager.Load<Texture2D>("Textures/barrierPurple");
            horizontalBarrierTexture = 
                contentManager.Load<Texture2D>("Textures/barrierRed");

            // clear the collision manager
            CollisionManager.Collection.Clear();

            // add the collision version of the edge barriers
            CollisionManager.Barriers.Clear();
            CollisionManager.Barriers.Add(new Rectangle(dimensions.X, dimensions.Y, 
                dimensions.Width, barrierSize));  // top edge
            CollisionManager.Barriers.Add(new Rectangle(
                dimensions.X, dimensions.Y + dimensions.Height, 
                dimensions.Width, barrierSize));  // bottom edge
            CollisionManager.Barriers.Add(new Rectangle(dimensions.X, dimensions.Y, 
                barrierSize, dimensions.Height)); // left edge
            CollisionManager.Barriers.Add(new Rectangle(
                dimensions.X + dimensions.Width, dimensions.Y, 
                barrierSize, dimensions.Height)); // right edge

            // add the rendering version of the edge barriers
            cornerBarriers.Clear();
            cornerBarriers.Add(new Rectangle(dimensions.X, dimensions.Y, 
                barrierSize, barrierSize)); // top-left corner
            cornerBarriers.Add(new Rectangle(
                dimensions.X + dimensions.Width, dimensions.Y, 
                barrierSize, barrierSize)); // top-right corner
            cornerBarriers.Add(new Rectangle(
                dimensions.X, dimensions.Y + dimensions.Height, 
                barrierSize, barrierSize)); // bottom-left corner
            cornerBarriers.Add(new Rectangle(
                dimensions.X + dimensions.Width, dimensions.Y + dimensions.Height, 
                barrierSize, barrierSize)); // bottom-right corner
            verticalBarriers.Clear();
            for (int i = 1; i < barrierCounts.Y; i++)
            {
                verticalBarriers.Add(new Rectangle(
                    dimensions.X, dimensions.Y + barrierSize * i, 
                    barrierSize, barrierSize)); // top edge
                verticalBarriers.Add(new Rectangle(
                    dimensions.X + dimensions.Width, dimensions.Y + barrierSize * i,
                    barrierSize, barrierSize)); // bottom edge
            }
            horizontalBarriers.Clear();
            for (int i = 1; i < barrierCounts.X; i++)
            {
                horizontalBarriers.Add(new Rectangle(
                    dimensions.X + barrierSize * i, dimensions.Y,
                    barrierSize, barrierSize)); // left edge
                horizontalBarriers.Add(new Rectangle(
                    dimensions.X + barrierSize * i, dimensions.Y + dimensions.Width,
                    barrierSize, barrierSize)); // right edge
            }
        }


        /// <summary>
        /// Generate the initial state of the game, and send it to everyone.
        /// </summary>
        public void GenerateWorld()
        {
            if ((networkSession != null) && (networkSession.LocalGamers.Count > 0))
            {
                // write the identification value
                packetWriter.Write((int)PacketTypes.WorldSetup);

                // place the ships
                // -- we always write the maximum number of players, making the packet
                //    predictable, in case the player count changes on the client before 
                //    this packet is received
                for (int i = 0; i < MaximumPlayers; i++)
                {
                    Vector2 position = Vector2.Zero;
                    if (i < networkSession.AllGamers.Count)
                    {
                        PlayerData playerData = networkSession.AllGamers[i].Tag
                            as PlayerData;
                        if ((playerData != null) && (playerData.Ship != null))
                        {
                            playerData.Ship.Initialize();
                            position = playerData.Ship.Position =
                                CollisionManager.FindSpawnPoint(playerData.Ship,
                                playerData.Ship.Radius * 5f);
                            playerData.Ship.Score = 0;
                        }
                    }
                    // write the ship position
                    packetWriter.Write(position);
                }

                // place the asteroids
                // -- for simplicity, the same number of asteroids is always the same
                for (int i = 0; i < asteroids.Length; i++)
                {
                    // choose one of three radii
                    float radius = 32f;
                    switch (RandomMath.Random.Next(3))
                    {
                        case 0:
                            radius = 32f;
                            break;
                        case 1:
                            radius = 60f;
                            break;
                        case 2:
                            radius = 96f;
                            break;
                    }
                    // create the asteroid
                    asteroids[i] = new Asteroid(radius);
                    // write the radius
                    packetWriter.Write(asteroids[i].Radius);
                    // choose a variation
                    asteroids[i].Variation = i % Asteroid.Variations;
                    // write the variation
                    packetWriter.Write(asteroids[i].Variation);
                    // initialize the asteroid and it's starting position
                    asteroids[i].Initialize();
                    asteroids[i].Position =
                        CollisionManager.FindSpawnPoint(asteroids[i], 
                        asteroids[i].Radius);
                    // write the starting position and velocity
                    packetWriter.Write(asteroids[i].Position);
                    packetWriter.Write(asteroids[i].Velocity);
                }

                // send the packet to everyone
                networkSession.LocalGamers[0].SendData(packetWriter,
                    SendDataOptions.ReliableInOrder);
            }
        }


        /// <summary>
        /// Initialize the world with the data from the WorldSetup packet.
        /// </summary>
        /// <param name="packetReader">The packet reader with the world data.</param>
        public void Initialize()
        {
            // reset the game status
            gameWon = false;
            winnerIndex = -1;
            gameExited = false;

            // initialize the ships with the data from the packet
            for (int i = 0; i < MaximumPlayers; i++)
            {
                // read each of the positions
                Vector2 position = packetReader.ReadVector2();
                // use the position value if we know of that many players
                if (i < networkSession.AllGamers.Count)
                {
                    PlayerData playerData = networkSession.AllGamers[i].Tag 
                        as PlayerData;
                    if ((playerData != null) && (playerData.Ship != null))
                    {
                        // initialize the ship with the provided position
                        playerData.Ship.Position = position;
                        playerData.Ship.Score = 0;
                        playerData.Ship.Initialize();
                    }
                }
            }

            // initialize the ships with the data from the packet
            for (int i = 0; i < asteroids.Length; i++)
            {
                float radius = packetReader.ReadSingle();
                if (asteroids[i] == null)
                {
                    asteroids[i] = new Asteroid(radius);
                }
                asteroids[i].Variation = packetReader.ReadInt32();
                asteroids[i].Position = packetReader.ReadVector2();
                asteroids[i].Initialize();
                asteroids[i].Velocity = packetReader.ReadVector2();
            }

            // set the initialized state
            initialized = true;
        }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the world.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="paused">If true, the game is paused.</param>
        public void Update(float elapsedTime, bool paused)
        {
            if (gameWon)
            {
                // update the particle-effect manager
                particleEffectManager.Update(elapsedTime);

                // make sure the collision manager is empty
                CollisionManager.Collection.ApplyPendingRemovals();
                if (CollisionManager.Collection.Count > 0)
                {
                    CollisionManager.Collection.Clear();
                }
            }
            else
            {
                // process all incoming packets
                ProcessPackets();

                // if the game is in progress, update the state of it
                if (initialized && (networkSession != null) &&
                    (networkSession.SessionState == NetworkSessionState.Playing))
                {
                    // presence support
                    int highScore = int.MinValue;
                    int highScoreIndex = -1;
                    for (int i = 0; i < networkSession.AllGamers.Count; i++)
                    {
                        NetworkGamer networkGamer = networkSession.AllGamers[i];
                        PlayerData playerData = networkGamer.Tag as PlayerData;
                        if ((playerData != null) && (playerData.Ship != null))
                        {
                            int playerScore = playerData.Ship.Score;
                            if (playerScore == highScore)
                            {
                                highScorers.Add(i);
                            }
                            else if (playerScore > highScore)
                            {
                                highScorers.Clear();
                                highScorers.Add(i);
                                highScore = playerScore;
                                highScoreIndex = i;
                            }
                        }
                    }

                    // the host has singular responsibilities to the game world, 
                    // that need to be done once, by one authority
                    if (networkSession.IsHost)
                    {
                        // get the local player, for frequent re-use
                        LocalNetworkGamer localGamer = networkSession.Host
                            as LocalNetworkGamer;

                        // check for victory
                        // if victory has been achieved, send a packet to everyone
                        if (highScore >= winningScore)
                        {
                            packetWriter.Write((int)PacketTypes.GameWon);
                            packetWriter.Write(highScoreIndex);
                            localGamer.SendData(packetWriter, 
                                SendDataOptions.ReliableInOrder);
                        }

                        // respawn each player, if it is time to do so
                        for (int i = 0; i < networkSession.AllGamers.Count; i++)
                        {
                            NetworkGamer networkGamer = networkSession.AllGamers[i];
                            PlayerData playerData = networkGamer.Tag as PlayerData;
                            if ((playerData != null) && (playerData.Ship != null) &&
                                !playerData.Ship.Active &&
                                (playerData.Ship.RespawnTimer <= 0f))
                            {
                                // write the ship-spawn packet
                                packetWriter.Write((int)PacketTypes.ShipSpawn);
                                packetWriter.Write(i);
                                packetWriter.Write(CollisionManager.FindSpawnPoint(
                                    playerData.Ship, playerData.Ship.Radius));
                                localGamer.SendData(packetWriter,
                                    SendDataOptions.ReliableInOrder);
                            }

                        }

                        // respawn the power-up if it is time to do so
                        if (powerUp == null)
                        {
                            powerUpTimer -= elapsedTime;
                            if (powerUpTimer < 0)
                            {
                                // write the power-up-spawn packet
                                packetWriter.Write((int)PacketTypes.PowerUpSpawn);
                                packetWriter.Write(RandomMath.Random.Next(3));
                                packetWriter.Write(CollisionManager.FindSpawnPoint(null,
                                    PowerUp.PowerUpRadius * 3f));
                                localGamer.SendData(packetWriter, 
                                    SendDataOptions.ReliableInOrder);
                            }
                        }
                        else
                        {
                            powerUpTimer = maximumPowerUpTimer;
                        }

                        // send everyone an update on the state of the world
                        if (updatesSinceWorldDataSend >= updatesBetweenWorldDataSend)
                        {
                            packetWriter.Write((int)PacketTypes.WorldData);
                            // write each of the asteroids
                            for (int i = 0; i < asteroids.Length; i++)
                            {
                                packetWriter.Write(asteroids[i].Position);
                                packetWriter.Write(asteroids[i].Velocity);
                            }
                            localGamer.SendData(packetWriter,
                                SendDataOptions.InOrder);
                            updatesSinceWorldDataSend = 0;
                        }
                        else
                        {
                            updatesSinceWorldDataSend++;
                        }
                    }

                    // update each asteroid
                    foreach (Asteroid asteroid in asteroids)
                    {
                        if (asteroid.Active)
                        {
                            asteroid.Update(elapsedTime);
                        }
                    }

                    // update the power-up
                    if (powerUp != null)
                    {
                        if (powerUp.Active)
                        {
                            powerUp.Update(elapsedTime);
                        }
                        else
                        {
                            powerUp = null;
                        }
                    }

                    // process the local player's input
                    if (!paused)
                    {
                        ProcessLocalPlayerInput();
                    }

                    // update each ship
                    foreach (NetworkGamer networkGamer in networkSession.AllGamers)
                    {
                        PlayerData playerData = networkGamer.Tag as PlayerData;
                        if ((playerData != null) && (playerData.Ship != null))
                        {
                            if (playerData.Ship.Active)
                            {
                                playerData.Ship.Update(elapsedTime);
                                // check for death 
                                // -- only check on local machines - the local player is
                                //    the authority on the death of their own ship
                                if (networkGamer.IsLocal && (playerData.Ship.Life < 0))
                                {
                                    SendLocalShipDeath();
                                }
                            }
                            else if (playerData.Ship.RespawnTimer > 0f)
                            {
                                playerData.Ship.RespawnTimer -= elapsedTime;
                                if (playerData.Ship.RespawnTimer < 0f)
                                {
                                    playerData.Ship.RespawnTimer = 0f;
                                }
                            }
                        }
                    }

                    // update the other players with the current state of the local ship
                    if (updatesSinceStatusPacket >= updatesBetweenStatusPackets)
                    {
                        updatesSinceStatusPacket = 0;
                        SendLocalShipData();
                    }
                    else
                    {
                        updatesSinceStatusPacket++;
                    }

                    // update the collision manager
                    CollisionManager.Update(elapsedTime);

                    // update the particle-effect manager
                    particleEffectManager.Update(elapsedTime);
                }
            }
        }


        /// <summary>
        /// Process the local player's input.
        /// </summary>
        private void ProcessLocalPlayerInput()
        {
            if ((networkSession != null) && (networkSession.LocalGamers.Count > 0))
            {
                // create the new input structure
                ShipInput shipInput = new ShipInput(
                    GamePad.GetState(
                       networkSession.LocalGamers[0].SignedInGamer.PlayerIndex),
                    Keyboard.GetState(
                       networkSession.LocalGamers[0].SignedInGamer.PlayerIndex));

                // send it out
                // -- the local machine will receive and apply it from the network just
                //    like the other clients
                shipInput.Serialize(packetWriter);
                networkSession.LocalGamers[0].SendData(packetWriter,
                    SendDataOptions.InOrder);
            }
        }


        /// <summary>
        /// Send the current state of the ship to the other players.
        /// </summary>
        private void SendLocalShipData()
        {
            if ((networkSession != null) && (networkSession.LocalGamers.Count > 0))
            {
                PlayerData playerData = networkSession.LocalGamers[0].Tag as PlayerData;
                if ((playerData != null) && (playerData.Ship != null))
                {
                    packetWriter.Write((int)World.PacketTypes.ShipData);
                    packetWriter.Write(playerData.Ship.Position);
                    packetWriter.Write(playerData.Ship.Velocity);
                    packetWriter.Write(playerData.Ship.Rotation);
                    packetWriter.Write(playerData.Ship.Life);
                    packetWriter.Write(playerData.Ship.Shield);
                    packetWriter.Write(playerData.Ship.Score);
                    networkSession.LocalGamers[0].SendData(packetWriter,
                        SendDataOptions.InOrder);
                }
            }
        }


        /// <summary>
        /// Send a notification of the death of the local ship to the other players.
        /// </summary>
        private void SendLocalShipDeath()
        {
            if ((networkSession != null) && (networkSession.LocalGamers.Count > 0))
            {
                LocalNetworkGamer localNetworkGamer = networkSession.LocalGamers[0]
                    as LocalNetworkGamer;
                PlayerData playerData = localNetworkGamer.Tag as PlayerData;
                if ((playerData != null) && (playerData.Ship != null))
                {
                    // send a ship-death notification
                    packetWriter.Write((int)PacketTypes.ShipDeath);
                    // determine the player behind the last damage taken
                    int lastDamagedByPlayer = -1;
                    Ship lastDamagedByShip = playerData.Ship.LastDamagedBy as Ship;
                    if ((lastDamagedByShip != null) &&
                        (lastDamagedByShip != playerData.Ship))
                    {
                        for (int i = 0; i < networkSession.AllGamers.Count; i++)
                        {
                            PlayerData sourcePlayerData =
                                networkSession.AllGamers[i].Tag as PlayerData;
                            if ((sourcePlayerData != null) &&
                                (sourcePlayerData.Ship != null) &&
                                (sourcePlayerData.Ship == lastDamagedByShip))
                            {
                                lastDamagedByPlayer = i;
                                break;
                            }
                        }
                    }
                    packetWriter.Write(lastDamagedByPlayer);
                    localNetworkGamer.SendData(packetWriter,
                        SendDataOptions.ReliableInOrder);
                }
            }
        }


        #endregion


        #region Packet Handling Methods


        /// <summary>
        /// Process incoming packets on the local gamer.
        /// </summary>
        private void ProcessPackets()
        {
            if ((networkSession != null) && (networkSession.LocalGamers.Count > 0))
            {
                // process all packets found, every frame
                while (networkSession.LocalGamers[0].IsDataAvailable)
                {
                    NetworkGamer sender;
                    networkSession.LocalGamers[0].ReceiveData(packetReader, out sender);
                    // read the type of packet...
                    PacketTypes packetType = (PacketTypes)packetReader.ReadInt32();
                    // ... and dispatch appropriately
                    switch (packetType)
                    {
                        case PacketTypes.PlayerData:
                            UpdatePlayerData(sender);
                            break;

                        case PacketTypes.WorldSetup:
                            // apply the world setup data, but only once
                            if (!Initialized)
                            {
                                Initialize();
                            }
                            break;
                            
                        case PacketTypes.ShipData:
                            if ((sender != null) && !sender.IsLocal)
                            {
                                UpdateShipData(sender);
                            }
                            break;

                        case PacketTypes.WorldData:
                            if (!networkSession.IsHost && Initialized)
                            {
                                UpdateWorldData();
                            }
                            break;

                        case PacketTypes.ShipInput:
                            if (sender != null)
                            {
                                PlayerData playerData = sender.Tag as PlayerData;
                                if ((playerData != null) && (playerData.Ship != null))
                                {
                                    playerData.Ship.ShipInput = 
                                        new ShipInput(packetReader);
                                }
                            }
                            break;

                        case PacketTypes.ShipSpawn:
                            SpawnShip();
                            break;

                        case PacketTypes.PowerUpSpawn:
                            SpawnPowerup();
                            break;

                        case PacketTypes.ShipDeath:
                            KillShip(sender);
                            break;

                        case PacketTypes.GameWon:
                            gameWon = true;
                            winnerIndex = packetReader.ReadInt32();
                            if (networkSession.IsHost && (networkSession.SessionState ==
                                NetworkSessionState.Playing))
                            {
                                networkSession.EndGame();
                            }
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Spawn a ship based on the data in the packet.
        /// </summary>
        private void SpawnShip()
        {
            int whichGamer = packetReader.ReadInt32();
            if (whichGamer < networkSession.AllGamers.Count)
            {
                NetworkGamer networkGamer = networkSession.AllGamers[whichGamer];
                PlayerData playerData = networkGamer.Tag as PlayerData;
                if ((playerData != null) && (playerData.Ship != null))
                {
                    playerData.Ship.Position = packetReader.ReadVector2();
                    playerData.Ship.Initialize();
                }
            }
        }


        /// <summary>
        /// Spawn a power-up based on the data in the packet.
        /// </summary>
        private void SpawnPowerup()
        {
            int whichPowerUp = packetReader.ReadInt32();
            if (powerUp == null)
            {
                switch (whichPowerUp)
                {
                    case 0:
                        powerUp = new DoubleLaserPowerUp();
                        break;
                    case 1:
                        powerUp = new TripleLaserPowerUp();
                        break;
                    case 2:
                        powerUp = new RocketPowerUp();
                        break;
                }
            }
            if (powerUp != null)
            {
                powerUp.Position = packetReader.ReadVector2();
                powerUp.Initialize();
            }
        }


        /// <summary>
        /// Kill the sender's ship based on data in the packet.
        /// </summary>
        /// <param name="sender">The sender of the packet.</param>
        private void KillShip(NetworkGamer sender)
        {
            if (sender != null)
            {
                PlayerData playerData = sender.Tag as PlayerData;
                if ((playerData != null) && (playerData.Ship != null) &&
                    playerData.Ship.Active)
                {
                    GameplayObject source = null;
                    // read the index of the source of the last damage taken
                    int sourcePlayerIndex = packetReader.ReadInt32();
                    if ((sourcePlayerIndex >= 0) &&
                        (sourcePlayerIndex < networkSession.AllGamers.Count))
                    {
                        PlayerData sourcePlayerData =
                            networkSession.AllGamers[sourcePlayerIndex].Tag 
                            as PlayerData;
                        source = sourcePlayerData != null ? sourcePlayerData.Ship : 
                            null;
                    }
                    // kill the ship
                    playerData.Ship.Die(source, false);
                }
            }
        }


        /// <summary>
        /// Update the player data for the sender based on the data in the packet.
        /// </summary>
        /// <param name="sender">The sender of the packet.</param>
        private void UpdatePlayerData(NetworkGamer sender)
        {
            if ((networkSession != null) && (networkSession.LocalGamers.Count > 0) && 
                (sender != null))
            {
                PlayerData playerData = sender.Tag as PlayerData;
                if (playerData != null)
                {
                    playerData.Deserialize(packetReader);
                    // see if we're still unique
                    // -- this can happen legitimately as we receive introductory data
                    foreach (LocalNetworkGamer localNetworkGamer in
                        networkSession.LocalGamers)
                    {
                        PlayerData localPlayerData =
                            localNetworkGamer.Tag as PlayerData;
                        if ((localPlayerData != null) && 
                            !Ship.HasUniqueColorIndex(localNetworkGamer,
                               networkSession))
                        {
                            localPlayerData.ShipColor = Ship.GetNextUniqueColorIndex(
                                localPlayerData.ShipColor, networkSession);
                            packetWriter.Write((int)World.PacketTypes.PlayerData);
                            localPlayerData.Serialize(packetWriter);
                            networkSession.LocalGamers[0].SendData(packetWriter, 
                                SendDataOptions.ReliableInOrder);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Update ship state based on the data in the packet.
        /// </summary>
        /// <param name="sender">The sender of the packet.</param>
        private void UpdateShipData(NetworkGamer sender)
        {
            if (sender != null)
            {
                PlayerData playerData = sender.Tag as PlayerData;
                if ((playerData != null) && (playerData.Ship != null))
                {
                    playerData.Ship.Position = packetReader.ReadVector2();
                    playerData.Ship.Velocity = packetReader.ReadVector2();
                    playerData.Ship.Rotation = packetReader.ReadSingle();
                    playerData.Ship.Life = packetReader.ReadSingle();
                    playerData.Ship.Shield = packetReader.ReadSingle();
                    playerData.Ship.Score = packetReader.ReadInt32();
                }
            }
        }


        /// <summary>
        /// Update the world data based on the data in the packet.
        /// </summary>
        private void UpdateWorldData()
        {
            // safety-check the parameters, as they must be valid
            if (packetReader == null)
            {
                throw new ArgumentNullException("packetReader");
            }

            for (int i = 0; i < asteroids.Length; i++)
            {
                asteroids[i].Position = packetReader.ReadVector2();
                asteroids[i].Velocity = packetReader.ReadVector2();
            }
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draws the objects in the world.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="center">The center of the current view.</param>
        public void Draw(float elapsedTime, Vector2 center)
        {
            Matrix transform = Matrix.CreateTranslation(
                new Vector3(-center.X, -center.Y, 0f));

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, 
                null, null, null, null, transform);

            // draw the barriers
            foreach (Rectangle rectangle in cornerBarriers)
            {
                spriteBatch.Draw(cornerBarrierTexture, rectangle, Color.White);
            }
            foreach (Rectangle rectangle in verticalBarriers)
            {
                spriteBatch.Draw(verticalBarrierTexture, rectangle, Color.White);
            }
            foreach (Rectangle rectangle in horizontalBarriers)
            {
                spriteBatch.Draw(horizontalBarrierTexture, rectangle, Color.White);
            }
            
            // draw the asteroids
            foreach (Asteroid asteroid in asteroids)
            {
                if (asteroid.Active)
                {
                    asteroid.Draw(elapsedTime, spriteBatch);
                }
            }

            // draw the powerup
            if ((powerUp != null) && powerUp.Active)
            {
                powerUp.Draw(elapsedTime, spriteBatch);
            }

            // draw the ships
            foreach (NetworkGamer networkGamer in networkSession.AllGamers)
            {
                PlayerData playerData = networkGamer.Tag as PlayerData;
                if ((playerData != null) && (playerData.Ship != null) && 
                    playerData.Ship.Active)
                {
                    playerData.Ship.Draw(elapsedTime, spriteBatch);
                }
            }

            // draw the alpha-blended particles
            particleEffectManager.Draw(spriteBatch, SpriteBlendMode.AlphaBlend);

            spriteBatch.End();

            // draw the additive particles
            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive, 
                null, null, null, null, transform);

            particleEffectManager.Draw(spriteBatch, SpriteBlendMode.Additive);

            spriteBatch.End();
        }


        /// <summary>
        /// Draw the specified player's data in the screen - gamertag, etc.
        /// </summary>
        /// <param name="totalTime">The total time spent in the game.</param>
        /// <param name="networkGamer">The player to be drawn.</param>
        /// <param name="position">The center of the desired location.</param>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        /// <param name="lobby">If true, drawn "lobby style"</param>
        public void DrawPlayerData(float totalTime, NetworkGamer networkGamer, 
            Vector2 position, SpriteBatch spriteBatch, bool lobby)
        {
            // safety-check the parameters, as they must be valid
            if (networkGamer == null)
            {
                throw new ArgumentNullException("networkGamer");
            }
            if (spriteBatch == null)
            {
                throw new ArgumentNullException("spriteBatch");
            }

            // get the player data
            PlayerData playerData = networkGamer.Tag as PlayerData;
            if (playerData == null)
            {
                return;
            }

            // draw the gamertag
            float playerStringScale = 1.0f;
            if (networkGamer.IsLocal)
            {
                // pulse the scale of local gamers
                playerStringScale = 1f + 0.08f * (1f + (float)Math.Sin(totalTime * 4f));
            }
            string playerString = networkGamer.Gamertag;
            Color playerColor = playerData.Ship == null ? 
                Ship.ShipColors[playerData.ShipColor] : playerData.Ship.Color;
            Vector2 playerStringSize = playerFont.MeasureString(playerString);
            Vector2 playerStringPosition = position;
            spriteBatch.DrawString(playerFont, playerString, playerStringPosition,
                playerColor, 0f, 
                new Vector2(playerStringSize.X / 2f, playerStringSize.Y / 2f), 
                playerStringScale, SpriteEffects.None, 0f);

            // draw the chat texture
            Texture2D chatTexture = null;
            if (networkGamer.IsMutedByLocalUser)
            {
                chatTexture = chatMuteTexture;
            }
            else if (networkGamer.IsTalking)
            {
                chatTexture = chatTalkingTexture;
            }
            else if (networkGamer.HasVoice)
            {
                chatTexture = chatAbleTexture;
            }
            if (chatTexture != null)
            {
                float chatTextureScale = 0.9f * playerStringSize.Y / 
                    (float)chatTexture.Height;
                Vector2 chatTexturePosition = new Vector2(playerStringPosition.X - 
                    1.2f * playerStringSize.X / 2f -
                    1.1f * chatTextureScale * (float)chatTexture.Width / 2f,
                    playerStringPosition.Y);
                spriteBatch.Draw(chatTexture, chatTexturePosition, null,
                    Color.White, 0f, new Vector2((float)chatTexture.Width / 2f, 
                    (float)chatTexture.Height / 2f), chatTextureScale, 
                    SpriteEffects.None, 0f);
            }

            // if we're in "lobby mode", draw a sample version of the ship,
            // and the ready texture
            if (lobby)
            {
                // draw the ship
                if (playerData.Ship != null)
                {
                    float oldShipShield = playerData.Ship.Shield;
                    float oldShipRadius = playerData.Ship.Radius;
                    Vector2 oldShipPosition = playerData.Ship.Position;
                    float oldShipRotation = playerData.Ship.Rotation;
                    playerData.Ship.Shield = 0f;
                    playerData.Ship.Radius = 0.6f * (float)playerStringSize.Y;
                    playerData.Ship.Position = new Vector2(playerStringPosition.X +
                        1.2f * playerStringSize.X / 2f + 1.1f * playerData.Ship.Radius,
                        playerStringPosition.Y);
                    playerData.Ship.Rotation = 0f;
                    playerData.Ship.Draw(0f, spriteBatch);
                    playerData.Ship.Rotation = oldShipRotation;
                    playerData.Ship.Position = oldShipPosition;
                    playerData.Ship.Shield = oldShipShield;
                    playerData.Ship.Radius = oldShipRadius;
                }
                
                // draw the ready texture
                if ((readyTexture != null) && networkGamer.IsReady)
                {
                    float readyTextureScale = 0.9f * playerStringSize.Y / 
                        (float)readyTexture.Height;
                    Vector2 readyTexturePosition = new Vector2(playerStringPosition.X +
                        1.2f * playerStringSize.X / 2f + 
                        2.2f * playerData.Ship.Radius + 
                        1.1f * readyTextureScale * (float)readyTexture.Width / 2f,
                        playerStringPosition.Y);
                    spriteBatch.Draw(readyTexture, readyTexturePosition, null,
                        Color.White, 0f, new Vector2((float)readyTexture.Width / 2f, 
                        (float)readyTexture.Height / 2f), readyTextureScale,
                        SpriteEffects.None, 0f);
                }

            }
            else
            {
                // if we're not in "lobby mode", draw the score
                if (playerData.Ship != null)
                {
                    string scoreString = String.Empty;
                    if (playerData.Ship.Active)
                    {
                        scoreString = playerData.Ship.Score.ToString();
                    }
                    else
                    {
                        int respawnTimer = 
                            (int)Math.Ceiling(playerData.Ship.RespawnTimer);
                        scoreString = "Respawning in: " + respawnTimer.ToString();
                    }
                    Vector2 scoreStringSize = playerFont.MeasureString(scoreString);
                    Vector2 scoreStringPosition = new Vector2(position.X,
                        position.Y + 0.9f * playerStringSize.Y);
                    spriteBatch.DrawString(playerFont, scoreString, scoreStringPosition,
                        playerColor, 0f, new Vector2(scoreStringSize.X / 2f, 
                        scoreStringSize.Y / 2f), 1f, SpriteEffects.None, 0f);
                }
            }
        }


        #endregion


        #region IDisposable Implementation


        /// <summary>
        /// Finalizes the World object, calls Dispose(false)
        /// </summary>
        ~World()
        {
            Dispose(false);
        }


        /// <summary>
        /// Disposes the World object.
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
                    if (packetReader != null)
                    {
                        packetReader.Close();
                        packetReader = null;
                    }

                    if (packetWriter != null)
                    {
                        packetWriter.Close();
                        packetWriter = null;
                    }

                    if (spriteBatch != null)
                    {
                        spriteBatch.Dispose();
                        spriteBatch = null;
                    }

                    cornerBarrierTexture = null;
                    verticalBarrierTexture = null;
                    horizontalBarrierTexture = null;

                    Ship.UnloadContent();
                    Asteroid.UnloadContent();
                    LaserProjectile.UnloadContent();
                    MineProjectile.UnloadContent();
                    RocketProjectile.UnloadContent();
                    DoubleLaserPowerUp.UnloadContent();
                    TripleLaserPowerUp.UnloadContent();

                    Ship.ParticleEffectManager = null;
                    RocketProjectile.ParticleEffectManager = null;
                    MineProjectile.ParticleEffectManager = null;
                    LaserProjectile.ParticleEffectManager = null;
                    particleEffectManager.UnregisterParticleEffect(
                        ParticleEffectType.MineExplosion);
                    particleEffectManager.UnregisterParticleEffect(
                        ParticleEffectType.RocketExplosion);
                    particleEffectManager.UnregisterParticleEffect(
                        ParticleEffectType.RocketTrail);
                    particleEffectManager.UnregisterParticleEffect(
                        ParticleEffectType.ShipExplosion);
                    particleEffectManager.UnregisterParticleEffect(
                        ParticleEffectType.ShipSpawn);
                }
            }
        }


        #endregion
    }
}
