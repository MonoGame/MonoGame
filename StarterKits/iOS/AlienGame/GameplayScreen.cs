//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace AlienGameSample
{
    /// <summary>
    /// This component draws the entire background for the game.  It handles
    /// drawing the ground, clouds, sun, and moon.  also handles animating them
    /// and day/night transitions
    /// </summary>
    public class GameplayScreen : GameScreen
    {
        //
        // Game Play Members
        //
        Player          player;
        List<Alien>    aliens;
        List<Bullet>    alienBullets;
        List<Bullet>    playerBullets;
        Rectangle       worldBounds;
        bool            gameOver;
        int             baseLevelKillCount;
        int             levelKillCount;
        float           alienSpawnTimer;
        float           alienSpawnRate;
        float           alienMaxAccuracy;
        float           alienSpeedMin;
        float           alienSpeedMax;
        int             alienScore;
        int             nextLife;
        int             hitStreak;
        int             highScore;
        Random          random;

        //
        // Rendering Members
        //
        Texture2D       cloud1Texture;
        Texture2D       cloud2Texture;
        Texture2D       sunTexture;
        Texture2D       moonTexture;
        Texture2D       groundTexture;
        Texture2D       tankTexture;
        Texture2D       alienTexture;
        Texture2D       badguy_blue;
        Texture2D       badguy_red;
        Texture2D       badguy_green;
        Texture2D       badguy_orange;
        Texture2D       mountainsTexture;
        Texture2D       hillsTexture;
        Texture2D       bulletTexture;
        Texture2D       laserTexture;

        SpriteFont      scoreFont;
        SpriteFont      menuFont;

        Vector2         cloud1Position;        
        Vector2         cloud2Position;
        Vector2         sunPosition;
        
        // Level changes, nighttime transitions, etc
        float           transitionFactor; // 0.0f == day, 1.0f == night
        float           transitionRate; // > 0.0f == day to night

        ParticleSystem  particles;

        //
        // Audio Members
        //
        SoundEffect     alienFired;
        SoundEffect     alienDied;
        SoundEffect     playerFired;
        SoundEffect     playerDied;
		
		Song backMusic;

        public GameplayScreen()
        {
            random = new Random();

            worldBounds = new Rectangle(0, 0, 320, 480);

            player = new Player();
            playerBullets = new List<Bullet>();

            aliens = new List<Alien>();
            alienBullets = new List<Bullet>();

            gameOver = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);
        }

        #region LOADING AND UNLOADING

        /// <summary>
        /// Loads all of the content needed by the game.  All of this should have already been cached
        /// into the ContentManager by the LoadingScreen.
        /// </summary>
        public override void LoadContent()
        {
			backMusic = ScreenManager.Game.Content.Load<Song>("backmusic");
            cloud1Texture = ScreenManager.Game.Content.Load<Texture2D>("cloud1");
            cloud2Texture = ScreenManager.Game.Content.Load<Texture2D>("cloud2");
            sunTexture = ScreenManager.Game.Content.Load<Texture2D>("sun");
            moonTexture = ScreenManager.Game.Content.Load<Texture2D>("moon");
            groundTexture = ScreenManager.Game.Content.Load<Texture2D>("ground");
            tankTexture = ScreenManager.Game.Content.Load<Texture2D>("tank");
            mountainsTexture = ScreenManager.Game.Content.Load<Texture2D>("mountains_blurred");
            hillsTexture = ScreenManager.Game.Content.Load<Texture2D>("hills");
            alienTexture = ScreenManager.Game.Content.Load<Texture2D>("alien1");
            badguy_blue = ScreenManager.Game.Content.Load<Texture2D>("badguy_blue");
            badguy_red = ScreenManager.Game.Content.Load<Texture2D>("badguy_red");
            badguy_green = ScreenManager.Game.Content.Load<Texture2D>("badguy_green");
            badguy_orange = ScreenManager.Game.Content.Load<Texture2D>("badguy_orange");
            bulletTexture = ScreenManager.Game.Content.Load<Texture2D>("bullet");
            laserTexture = ScreenManager.Game.Content.Load<Texture2D>("laser");

            alienFired = ScreenManager.Game.Content.Load<SoundEffect>("Tank_Fire");
            alienDied = ScreenManager.Game.Content.Load<SoundEffect>("Alien_Hit");
            playerFired = ScreenManager.Game.Content.Load<SoundEffect>("Tank_Fire");
            playerDied = ScreenManager.Game.Content.Load<SoundEffect>("Player_Hit");

            scoreFont = ScreenManager.Game.Content.Load<SpriteFont>("ScoreFont");
            menuFont = ScreenManager.Game.Content.Load<SpriteFont>("menufont");

            player.Width = tankTexture.Width;
            player.Height = tankTexture.Height;

            cloud1Position = new Vector2(224 - cloud1Texture.Width, 32);
            cloud2Position = new Vector2(64, 80);

            sunPosition = new Vector2(16, 16);

            particles = new ParticleSystem(ScreenManager.Game.Content, ScreenManager.SpriteBatch);

            LoadHighscore();

            base.LoadContent();

            // Automatically start the game once all loading is done
            Start();
        }

        /// <summary>
        /// Save the scores and dispose of the particles
        /// </summary>
        public override void UnloadContent()
        {
            particles = null;

            base.UnloadContent();
        }

        /// <summary>
        /// Saves the current highscore to a text file. The StorageDevice was selected during the loading screen.
        /// </summary>
        private void SaveHighscore()
        {
            StorageDevice device = (StorageDevice)ScreenManager.Game.Services.GetService(typeof(StorageDevice));
            if (device != null)
            {
                StorageContainer container = device.OpenContainer("AlienGame");
                StreamWriter writer = new StreamWriter(Path.Combine(container.Path, "highscores.txt"));
                writer.Write(highScore.ToString(System.Globalization.CultureInfo.InvariantCulture));
                writer.Flush();
                writer.Close();
                container.Dispose();
            }
        }

        /// <summary>
        /// Loads the high score from a text file.  The StorageDevice was selected during the loading screen.
        /// </summary>
        private void LoadHighscore()
        {
            StorageDevice device = (StorageDevice)ScreenManager.Game.Services.GetService(typeof(StorageDevice));
            if (device != null)
            {
                StorageContainer container = device.OpenContainer("AlienGame");
                if (File.Exists(Path.Combine(container.Path, "highscores.txt")))
                {
                    StreamReader reader = null;

                    try
                    {
                        reader = new StreamReader(Path.Combine(container.Path, "highscores.txt"));
                        highScore = Int32.Parse(reader.ReadToEnd(), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch(FormatException)
                    {
                        highScore = 10000;
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                }

                container.Dispose();
            }
        }

        #endregion

        #region UPDATE AND SIMULATION

        /// <summary>
        /// Runs one frame of update for the game.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (IsActive)
            {
                // Move the player
                if (player.IsAlive == true)
                {
                    player.Position += player.Velocity * 128.0f * elapsed;
                    player.FireTimer -= elapsed;

                    if (player.Position.X <= 0.0f)
                        player.Position = new Vector2(0.0f, player.Position.Y);

                    if (player.Position.X + player.Width >= worldBounds.Right)
                        player.Position = new Vector2(worldBounds.Right - player.Width, player.Position.Y);
                }

                Respawn(elapsed);

                UpdateAliens(elapsed);

                UpdateBullets(elapsed);

                CheckHits();

                if (player.IsAlive && player.Velocity.LengthSquared() > 0.1f)
                    particles.CreatePlayerDust(player);

                particles.Update(elapsed);
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Input helper method provided by GameScreen.  Packages up the various input
        /// values for ease of use.  Here it checks for pausing and handles controlling
        /// the player's tank.
        /// </summary>
        /// <param name="input">The state of the gamepads</param>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.PauseGame)
            {
                if (gameOver == true)
                {
					MediaPlayer.Stop();
					
                    foreach (GameScreen screen in ScreenManager.GetScreens())
                        screen.ExitScreen();

                    ScreenManager.AddScreen(new BackgroundScreen());
                    ScreenManager.AddScreen(new MainMenuScreen());
                }
                else
                {
                    ScreenManager.AddScreen(new PauseMenuScreen());
                }
            }
            else
            {
				bool touchShoot = false;
				
				#if TARGET_IPHONE_SIMULATOR
                // This section handles tank movement.  We only allow one "movement" action
                // to occur at once so that touchpad devices don't get double hits.
                player.Velocity.X = MathHelper.Min(input.CurrentGamePadStates.ThumbSticks.Left.X * 2.0f, 1.0f);
				touchShoot = input.MenuSelect;
				#else
				// Add the accelerometer support
				player.Velocity.X = MathHelper.Min(Accelerometer.GetState().Acceleration.X * 2.0f, 1.0f);
				
				// tap the screen to shoot				
				foreach (TouchLocation location in input.TouchStates)
				{
				    switch (location.State)
				    {
				        case TouchLocationState.Pressed:
				            touchShoot = true;
				            break;
				        case TouchLocationState.Moved:
				            break;
				        case TouchLocationState.Released:
				            break;
				    }
				}
				#endif
				
                
				
				        
				if (touchShoot)
                {
					//Mouse.SetPosition(0,0);
                    if (player.FireTimer <= 0.0f && player.IsAlive && !gameOver)
                    {
                        Bullet bullet = CreatePlayerBullet();
                        bullet.Position = new Vector2((int)(player.Position.X + player.Width / 2) - bulletTexture.Width / 2, player.Position.Y - 4);
                        bullet.Velocity = new Vector2(0, -256.0f);
                        player.FireTimer = 0.5f;
                        particles.CreatePlayerFireSmoke(player);
                        playerFired.Play();
                    }
                }
            }
        }

        /// <summary>
        /// Handles respawning the player if we are playing a game and the player is dead.
        /// </summary>
        /// <param name="elapsed">Time elapsed since Respawn was called last.</param>
        void Respawn(float elapsed)
        {
            if (gameOver)
                return;

            if (!player.IsAlive)
            {
                player.RespawnTimer -= elapsed;
                if (player.RespawnTimer <= 0.0f)
                {
                    // See if there are any bullets close...
                    int left = worldBounds.Width / 2 - tankTexture.Width / 2 - 8;
                    int right = worldBounds.Width / 2 + tankTexture.Width / 2 + 8;

                    for (int i = 0; i < alienBullets.Count; ++i)
                    {
                        if (alienBullets[i].IsAlive == false)
                            continue;

                        if (alienBullets[i].Position.X >= left || alienBullets[i].Position.X <= right)
                            return;
                    }

                    player.IsAlive = true;
                    player.Position = new Vector2(worldBounds.Width / 2 - player.Width / 2, worldBounds.Bottom - (int)(groundTexture.Height*1.3f) + 2 - player.Height);
                    player.Velocity = Vector2.Zero;
                    player.Lives--;
                }
            }
        }

        /// <summary>
        /// Moves all of the bullets (player and alien) and prunes "dead" bullets.
        /// </summary>
        /// <param name="elapsed"></param>
        void UpdateBullets(float elapsed)
        {
            for (int i = 0; i < playerBullets.Count; ++i)
            {
                if (playerBullets[i].IsAlive == false)
                    continue;

                playerBullets[i].Position += playerBullets[i].Velocity * elapsed;

                if (playerBullets[i].Position.Y < -32)
                {
                    playerBullets[i].IsAlive = false;
                    hitStreak = 0;
                }
            }

            for (int i = 0; i < alienBullets.Count; ++i)
            {
                if (alienBullets[i].IsAlive == false)
                    continue;

                alienBullets[i].Position += alienBullets[i].Velocity * elapsed;

                if (alienBullets[i].Position.Y > worldBounds.Height - groundTexture.Height - laserTexture.Height)
                    alienBullets[i].IsAlive = false;
            }
        }

        /// <summary>
        /// Moves the aliens and performs there "thinking" by determining if they
        /// should shoot and where.
        /// </summary>
        /// <param name="elapsed">The elapsed time since UpdateAliens was called last.</param>
        private void UpdateAliens(float elapsed)
        {
            // See if it's time to spawn an alien;
            alienSpawnTimer -= elapsed;
            if (alienSpawnTimer <= 0.0f)
            {
                SpawnAlien();
                alienSpawnTimer += alienSpawnRate;
            }

            for (int i = 0; i < aliens.Count; ++i)
            {
                if (aliens[i].IsAlive == false)
                    continue;

                aliens[i].Position += aliens[i].Velocity * elapsed;
                if ((aliens[i].Position.X < -aliens[i].Width - 64 && aliens[i].Velocity.X < 0.0f) ||
                    (aliens[i].Position.X > worldBounds.Width + 64 && aliens[i].Velocity.X > 0.0f))
                {
                    aliens[i].IsAlive = false;
                    continue;
                }

                aliens[i].FireTimer -= elapsed;

                if (aliens[i].FireTimer <= 0.0f && aliens[i].FireCount > 0)
                {
                    if (player.IsAlive)
                    {
                        Bullet bullet = CreateAlienBullet();
                        bullet.Position.X = aliens[i].Position.X + aliens[i].Width / 2 - laserTexture.Width / 2;
                        bullet.Position.Y = aliens[i].Position.Y + aliens[i].Height;
                        if ((float)random.NextDouble() <= aliens[i].Accuracy)
                        {
                            bullet.Velocity = Vector2.Normalize(player.Position - aliens[i].Position) * 64.0f;
                        }
                        else
                        {
                            bullet.Velocity = new Vector2(-8.0f + 16.0f * (float)random.NextDouble(), 64.0f);
                        }

                        alienFired.Play();
                    }

                    aliens[i].FireCount--;            
                }
            }
        }

        /// <summary>
        /// Performs all bullet and player/alien collision detection.  Also handles game logic
        /// when a hit occurs, such as killing something, adding score, ending the game, etc.
        /// </summary>
        void CheckHits()
        {
            if (gameOver)
                return;

            for (int i = 0; i < playerBullets.Count; ++i)
            {
                if (playerBullets[i].IsAlive == false)
                    continue;

                for (int a = 0; a < aliens.Count; ++a)
                {
                    if (aliens[a].IsAlive == false)
                        continue;

                    if ((playerBullets[i].Position.X >= aliens[a].Position.X && playerBullets[i].Position.X <= aliens[a].Position.X + aliens[a].Width) &&
                        (playerBullets[i].Position.Y >= aliens[a].Position.Y && playerBullets[i].Position.Y <= aliens[a].Position.Y + aliens[a].Height))
                    {
                        playerBullets[i].IsAlive = false;
                        aliens[a].IsAlive = false;

                        hitStreak++;

                        player.Score += aliens[a].Score * (hitStreak / 5 + 1);

                        if (player.Score > highScore)
                            highScore = player.Score;

                        if (player.Score > nextLife)
                        {
                            player.Lives++;
                            nextLife += nextLife;
                        }

                        levelKillCount--;
                        if (levelKillCount <= 0)
                            AdvanceLevel();

                        particles.CreateAlienExplosion(new Vector2(aliens[a].Position.X + aliens[a].Width / 2, aliens[a].Position.Y + aliens[a].Height / 2));

                        alienDied.Play();
                    }
                }
            }

            if (player.IsAlive == false)
                return;

            for (int i = 0; i < alienBullets.Count; ++i)
            {
                if (alienBullets[i].IsAlive == false)
                    continue;

                if ((alienBullets[i].Position.X >= player.Position.X + 2 && alienBullets[i].Position.X <= player.Position.X + player.Width - 2) &&
                    (alienBullets[i].Position.Y >= player.Position.Y + 2 && alienBullets[i].Position.Y <= player.Position.Y + player.Height))
                {
                    alienBullets[i].IsAlive = false;

                    player.IsAlive = false;

                    hitStreak = 0;

                    player.RespawnTimer = 3.0f;
                    particles.CreatePlayerExplosion(new Vector2(player.Position.X + player.Width / 2, player.Position.Y + player.Height / 2));

                    playerDied.Play();

                    if (player.Lives <= 0)
                    {
                        gameOver = true;
						SaveHighscore();
                    }
                }

            }
        }

        /// <summary>
        /// Starts a new game session, setting all game state to initial values.
        /// </summary>
        void Start()
        {		
			MediaPlayer.Volume = 0.2f;
			MediaPlayer.Play(backMusic);
			
            if (gameOver)
            {
                player.Score = 0;
                player.Lives = 3;
                player.RespawnTimer = 0.0f;

                gameOver = false;

                aliens.Clear();
                alienBullets.Clear();
                playerBullets.Clear();

                Respawn(0.0f);
            }

            transitionRate = 0.0f;
            transitionFactor = 0.0f;
            levelKillCount = 5;
            baseLevelKillCount = 5;
            alienScore = 25;
            alienSpawnRate = 1.0f;

            alienMaxAccuracy = 0.25f;

            alienSpeedMin = 24.0f;
            alienSpeedMax = 32.0f;

            alienSpawnRate = 2.0f;
            alienSpawnTimer = alienSpawnRate;

            nextLife = 5000;
        }

        /// <summary>
        /// Advances the difficulty of the game one level.
        /// </summary>
        void AdvanceLevel()
        {
            baseLevelKillCount += 5;
            levelKillCount = baseLevelKillCount;
            alienScore += 25;
            alienSpawnRate -= 0.3f;
            alienMaxAccuracy += 0.1f;
            if (alienMaxAccuracy > 0.75f)
                alienMaxAccuracy = 0.75f;

            alienSpeedMin *= 1.35f;
            alienSpeedMax *= 1.35f;

            if (alienSpawnRate < 0.33f)
                alienSpawnRate = 0.33f;

            if (transitionFactor == 1.0f)
            {
                transitionRate = -0.5f;
            }
            else
            {
                transitionRate = 0.5f;
            }
        }

        /// <summary>
        /// Returns an instance of a usable player bullet.  Prefers reusing an existing (dead)
        /// bullet over creating a new instance.
        /// </summary>
        /// <returns>A bullet ready to place into the world.</returns>
        Bullet CreatePlayerBullet()
        {
            Bullet b = null;

            for (int i = 0; i < playerBullets.Count; ++i)
            {
                if (playerBullets[i].IsAlive == false)
                {
                    b = playerBullets[i];
                    break;
                }
            }

            if (b == null)
            {
                b = new Bullet();
                playerBullets.Add(b);
            }

            b.IsAlive = true;

            return b;
        }

        /// <summary>
        /// Returns an instance of a usable alien bullet.  Prefers reusing an existing (dead)
        /// bullet over creating a new instance.
        /// </summary>
        /// <returns>A bullet ready to place into the world.</returns>
        Bullet CreateAlienBullet()
        {
            Bullet b = null;

            for (int i = 0; i < alienBullets.Count; ++i)
            {
                if (alienBullets[i].IsAlive == false)
                {
                    b = alienBullets[i];
                    break;
                }
            }

            if (b == null)
            {
                b = new Bullet();
                alienBullets.Add(b);
            }

            b.IsAlive = true;

            return b;
        }

        /// <summary>
        /// Returns an instance of a usable alien instance.  Prefers reusing an existing (dead)
        /// alien over creating a new instance.
        /// </summary>
        /// <returns>An alien ready to place into the world.</returns>
        Alien CreateAlien()
        {
            Alien b = null;

            for (int i = 0; i < aliens.Count; ++i)
            {
                if (aliens[i].IsAlive == false)
                {
                    b = aliens[i];
                    break;
                }
            }

            if (b == null)
            {
                b = new Alien();
                aliens.Add(b);
            }

            b.IsAlive = true;

            return b;
        }

        /// <summary>
        /// Creats an instance of an alien, sets the initial state, and places it into the world.
        /// </summary>
        private void SpawnAlien()
        {
            Alien newAlien = CreateAlien();

            if (random.Next(2) == 1)
            {
                newAlien.Position.X = -64.0f;
                newAlien.Velocity.X = random.Next((int)alienSpeedMin, (int)alienSpeedMax);
            }
            else
            {
                newAlien.Position.X = worldBounds.Width + 32;
                newAlien.Velocity.X = -random.Next((int)alienSpeedMin, (int)alienSpeedMax);
            }

            newAlien.Position.Y = 31.2f + 105.0f * (float)random.NextDouble();

            // Aliens
            if (transitionFactor > 0.0f)
            {
                switch (random.Next(4))
                {
                    case 0:
                        newAlien.Texture = badguy_blue;
                        break;
                    case 1:
                        newAlien.Texture = badguy_red;
                        break;
                    case 2:
                        newAlien.Texture = badguy_green;
                        break;
                    case 3:
                        newAlien.Texture = badguy_orange;
                        break;
                }
            }
            else
            {
                newAlien.Texture = alienTexture;
            }

            newAlien.Width = newAlien.Texture.Width;
            newAlien.Height = newAlien.Texture.Height;
            newAlien.IsAlive = true;
            newAlien.Score = alienScore;

            float duration = 320.0f / newAlien.Velocity.Length();

            newAlien.FireTimer = duration * (float)random.NextDouble();
            newAlien.FireCount = 1;

            newAlien.Accuracy = alienMaxAccuracy;
        }

        #endregion

        #region DRAWING

        /// <summary>
        /// Draw the game world, effects, and hud
        /// </summary>
        /// <param name="gameTime">The elapsed time since last Draw</param>
        public override void Draw(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            ScreenManager.SpriteBatch.Begin();

            DrawBackground(elapsedTime);

            DrawAliens();

            DrawPlayer();

            DrawBullets();

            particles.Draw();

            DrawForeground(elapsedTime);
            
            DrawHud();	
			
			#if TARGET_IPHONE_SIMULATOR
			// Draw the GamePad
			GamePad.Draw(gameTime,ScreenManager.SpriteBatch);
			#endif
			
            ScreenManager.SpriteBatch.End();
        }

        /// <summary>
        /// Draws the player's tank
        /// </summary>
        void DrawPlayer()
        {
            if (!gameOver && player.IsAlive)
            {
                ScreenManager.SpriteBatch.Draw(tankTexture, player.Position, Color.White);
            }
        }

        /// <summary>
        /// Draws all of the aliens.
        /// </summary>
        void DrawAliens()
        {
            for (int i = 0; i < aliens.Count; ++i)
            {
                if (aliens[i].IsAlive)
                    ScreenManager.SpriteBatch.Draw(aliens[i].Texture, new Rectangle((int)aliens[i].Position.X, (int)aliens[i].Position.Y, (int)aliens[i].Width, (int)aliens[i].Height), Color.White);
            }
        }

        /// <summary>
        /// Draw both the player and alien bullets.
        /// </summary>
        private void DrawBullets()
        {
            for (int i = 0; i < playerBullets.Count; ++i)
            {
                if (playerBullets[i].IsAlive)
                    ScreenManager.SpriteBatch.Draw(bulletTexture, playerBullets[i].Position, Color.White);
            }

            for (int i = 0; i < alienBullets.Count; ++i)
            {
                if (alienBullets[i].IsAlive)
                    ScreenManager.SpriteBatch.Draw(laserTexture, alienBullets[i].Position, Color.White);
            }
        }

        /// <summary>
        /// Draw the foreground, which is basically the clouds.  I think I had planned on one point
        /// having foreground grass that was drawn in front of the tank.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time since last Draw</param>
        private void DrawForeground(float elapsedTime)
        {
            // Move the clouds.  Movement seems like a Update thing to do, but this animations
            // have no impact over gameplay.
            cloud1Position += new Vector2(24.0f, 0.0f) * elapsedTime;
            if (cloud1Position.X > 320.0f)
                cloud1Position.X = -cloud1Texture.Width * 2.0f;

            cloud2Position += new Vector2(16.0f, 0.0f) * elapsedTime;
            if (cloud2Position.X > 320.0f)
                cloud2Position.X = -cloud1Texture.Width * 2.0f;

            ScreenManager.SpriteBatch.Draw(cloud1Texture, cloud1Position, Color.White);
            ScreenManager.SpriteBatch.Draw(cloud2Texture, cloud2Position, Color.White);         
        }

        /// <summary>
        /// Draw the grass, hills, mountains, and sun/moon.  Handles transitioning
        /// between day and night as well.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time since last Draw</param>
        private void DrawBackground(float elapsedTime)
        {

            transitionFactor += transitionRate * elapsedTime;
            if (transitionFactor < 0.0f)
            {
                transitionFactor = 0.0f;
                transitionRate = 0.0f;
            }
            if (transitionFactor > 1.0f)
            {
                transitionFactor = 1.0f;
                transitionRate = 0.0f;
            }

            Vector3 day = Color.White.ToVector3();
            Vector3 night = new Color(80, 80, 180).ToVector3();
            Vector3 dayClear = Color.CornflowerBlue.ToVector3();
            Vector3 nightClear = night;

            Color clear = new Color(Vector3.Lerp(dayClear, nightClear, transitionFactor));
            Color tint = new Color(Vector3.Lerp(day,night,transitionFactor));

            // Clear the background, using the day/night color
            ScreenManager.Game.GraphicsDevice.Clear(clear);
            
            // Draw the mountains
			ScreenManager.SpriteBatch.Draw(mountainsTexture, new Rectangle(0, 480 - (int)(mountainsTexture.Height*1.3f),320,(int)(mountainsTexture.Height*1.3f)), tint);

            // Draw the hills
            ScreenManager.SpriteBatch.Draw(hillsTexture, new Rectangle(0, 480 - (int)(hillsTexture.Height*1.3f),320,(int)(hillsTexture.Height*1.3f)), tint);

            // Draw the ground
            ScreenManager.SpriteBatch.Draw(groundTexture, new Rectangle(0, 480 - (int)(groundTexture.Height*1.3f),320,(int)(groundTexture.Height*1.3f)), tint);

            // Draw the sun or moon (based on time)
            ScreenManager.SpriteBatch.Draw(sunTexture, sunPosition, new Color(255,255,255, (byte)(255.0f * (1.0f - transitionFactor))));
            ScreenManager.SpriteBatch.Draw(moonTexture, sunPosition, new Color(255,255,255, (byte)(255.0f * transitionFactor)));
        }

        /// <summary>
        /// Draw the hud, which consists of the score elements and the GAME OVER tag.
        /// </summary>
        void DrawHud()
        {
            if (gameOver)
            {
                Vector2 size = menuFont.MeasureString("GAME OVER");
                DrawString(menuFont, "GAME OVER", new Vector2(ScreenManager.Game.GraphicsDevice.Viewport.Width / 2 - size.X / 2, ScreenManager.Game.GraphicsDevice.Viewport.Height / 2 - size.Y / 2), new Color(255,64,64));
            }
            else
            {
                int bonus = 100 * (hitStreak / 5);
                string bonusString = (bonus > 0 ? " (" + bonus.ToString(System.Globalization.CultureInfo.CurrentCulture) + "%)" : "");
                // Score
                DrawString(scoreFont, "SCORE: " + player.Score.ToString(System.Globalization.CultureInfo.CurrentCulture) + bonusString, new Vector2(6, 4), Color.Yellow);

                string text = "LIVES: " + player.Lives.ToString(System.Globalization.CultureInfo.CurrentCulture);
                Vector2 size = scoreFont.MeasureString(text);

                // Lives
                DrawString(scoreFont, text, new Vector2(314 - (int)size.X, 4), Color.Yellow);

                DrawString(scoreFont, "LEVEL: " + (((baseLevelKillCount - 5) / 5) + 1).ToString(System.Globalization.CultureInfo.CurrentCulture), new Vector2(6, 460), Color.Yellow);

                text = "HIGH SCORE: " + highScore.ToString(System.Globalization.CultureInfo.CurrentCulture);

                size = scoreFont.MeasureString(text);

                DrawString(scoreFont, text, new Vector2(314 - (int)size.X, 460), Color.Yellow);
            }
        }

        /// <summary>
        /// A simple helper to draw shadowed text.
        /// </summary>
        void DrawString(SpriteFont font, string text, Vector2 position, Color color)
        {
            ScreenManager.SpriteBatch.DrawString(font, text, new Vector2(position.X + 1, position.Y + 1), Color.Black);
            ScreenManager.SpriteBatch.DrawString(font, text, position, color);
        }

        #endregion
    }

    /// <summary>
    /// Represents either an alien or player bullet
    /// </summary>
    public class Bullet
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public bool IsAlive;
    }

    /// <summary>
    /// The player's state
    /// </summary>
    public class Player
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Width;
        public float Height;
        public bool IsAlive;
        public float FireTimer;
        public float RespawnTimer;
        public string Name;
        public Texture2D Picture;
        public int Score;
        public int Lives;
    }

    /// <summary>
    /// Data for an alien.  The only difference between the ships
    /// and the badguys are the texture used.
    /// </summary>
    public class Alien
    {
        public Vector2 Position;
        public Texture2D Texture;
        public Vector2 Velocity;
        public float Width;
        public float Height;
        public int Score;
        public bool IsAlive;
        public float FireTimer;
        public float Accuracy;
        public int FireCount;
    }
}