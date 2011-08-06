#region File Description
//-----------------------------------------------------------------------------
// BeeKeeper.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


#endregion

namespace HoneycombRush
{
    /// <summary>
    /// Represents the beekeeper, the player's avatar.
    /// </summary>
    public class BeeKeeper : TexturedDrawableGameComponent
    {
        #region Enums


        /// <summary>
        /// Represents the direction in which the beekeeper is walking.
        /// </summary>
        enum WalkingDirection
        {
            Down = 0,
            Up = 8,
            Left = 16,
            Right = 24,
            LeftDown = 32,
            RightDown = 40,
            LeftUp = 48,
            RightUp = 56
        }


        #endregion

        #region Fields/Properties


        // Animation name constants
        const string LegAnimationKey = "LegAnimation";
        const string BodyAnimationKey = "BodyAnimation";
        const string SmokeAnimationKey = "SmokeAnimation";
        const string ShootingAnimationKey = "ShootingAnimation";
        const string BeekeeperCollectingHoneyAnimationKey = "BeekeeperCollectingHoney";
        const string BeekeeperDepositingHoneyAnimationKey = "BeekeeperDepositingHoney";

        Vector2 bodySize = new Vector2(85, 132);
        Vector2 velocity;
        Vector2 smokeAdjustment;
        SpriteEffects lastEffect;
        SpriteEffects currentEffect;

        // Beekeeper state variables
        bool needToShootSmoke;
        bool isStung;
        bool isFlashing;
        bool isDrawnLastStungInterval;
        bool isDepositingHoney;

        TimeSpan stungTime;
        TimeSpan stungDuration;
        TimeSpan flashingDuration;
        TimeSpan depositHoneyUpdatingInterval = TimeSpan.FromMilliseconds(200);
        TimeSpan depositHoneyUpdatingTimer = TimeSpan.Zero;
        TimeSpan shootSmokePuffTimer = TimeSpan.Zero;
        readonly TimeSpan shootSmokePuffTimerInitialValue = TimeSpan.FromMilliseconds(325);

        Texture2D smokeAnimationTexture;
        Texture2D smokePuffTexture;
        const int MaxSmokePuffs = 20;
        /// <summary>
        /// Contains all smoke puffs which are currently active
        /// </summary>
        public Queue<SmokePuff> FiredSmokePuffs { get; private set; }
        /// <summary>
        /// Serves as a pool of available smoke puff objects.
        /// </summary>
        Stack<SmokePuff> availableSmokePuffs;

        int stungDrawingInterval = 5;
        int stungDrawingCounter = 0;
        int honeyDepositFrameCount;
        int depositHoneyTimerCounter = -1;
        int collectingHoneyFrameCounter;

        AsyncCallback depositHoneyCallback;

        WalkingDirection newDirection = WalkingDirection.Up;
        WalkingDirection direction = WalkingDirection.Up;
        int lastFrameCounter;

        public bool IsStung
        {
            get
            {
                return isStung;
            }
        }

        public bool IsFlashing
        {
            get
            {
                return isFlashing;
            }
        }

        /// <summary>
        /// Mark the beekeeper as shooting or not shooting smoke.
        /// </summary>        
        public bool IsShootingSmoke
        {
            set
            {
                if (!isStung)
                {
                    needToShootSmoke = value;
                    if (value)
                    {
                        AudioManager.PlaySound("SmokeGun_Loop");
                    }
                    else
                    {
                        shootSmokePuffTimer = TimeSpan.Zero;
                    }
                }
            }
        }

        public override Rectangle Bounds
        {
            get
            {
                int height = (int)bodySize.Y / 10 * 8;
                int width = (int)bodySize.X / 10 * 5;

                int offsetY = ((int)bodySize.Y - height) / 2;
                int offsetX = ((int)bodySize.X - width) / 2;

                return new Rectangle((int)position.X + offsetX, (int)position.Y + offsetY, width, height);
            }

        }

        public override Rectangle CentralCollisionArea
        {
            get
            {
                Rectangle bounds = Bounds;
                int height = (int)bounds.Height / 10 * 5;
                int width = (int)bounds.Width / 10 * 8;

                int offsetY = ((int)bounds.Height - height) / 2;
                int offsetX = ((int)bounds.Width - width) / 2;

                return new Rectangle((int)bounds.X + offsetX, (int)bounds.Y + offsetY, width, height);
            }
        }

        public bool IsDepositingHoney
        {
            get
            {
                return isDepositingHoney;
            }
        }

        public bool IsCollectingHoney { get; set; }

        public Vector2 Position
        {
            get { return position; }
        }

        public Rectangle ThumbStickArea { get; set; }

        public bool IsInMotion { get; set; }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new beekeeper instance.
        /// </summary>
        /// <param name="game">The game object.</param>
        /// <param name="gamePlayScreen">The gameplay screen.</param>
        public BeeKeeper(Game game, GameplayScreen gamePlayScreen)
            : base(game, gamePlayScreen)
        {
        }

        /// <summary>
        /// Initialize the beekepper.
        /// </summary>
        public override void Initialize()
        {
            // Initialize the animation 
            AnimationDefinitions[LegAnimationKey].PlayFromFrameIndex(0);
            AnimationDefinitions[BodyAnimationKey].PlayFromFrameIndex(0);
            AnimationDefinitions[SmokeAnimationKey].PlayFromFrameIndex(0);
            AnimationDefinitions[ShootingAnimationKey].PlayFromFrameIndex(0);
            AnimationDefinitions[BeekeeperCollectingHoneyAnimationKey].PlayFromFrameIndex(0);
            AnimationDefinitions[BeekeeperDepositingHoneyAnimationKey].PlayFromFrameIndex(0);

            Point bodyAnimationFrameSize = AnimationDefinitions[LegAnimationKey].frameSize;
            bodySize = new Vector2(bodyAnimationFrameSize.X, bodyAnimationFrameSize.Y) * scaledSpriteBatch.ScaleVector;

            isStung = false;
            stungDuration = TimeSpan.FromSeconds(1);
            flashingDuration = TimeSpan.FromSeconds(2);

            availableSmokePuffs = new Stack<SmokePuff>(MaxSmokePuffs);
            FiredSmokePuffs = new Queue<SmokePuff>(MaxSmokePuffs);

            base.Initialize();
        }

        /// <summary>
        /// Loads content that will be used later on by the beekeeper.
        /// </summary>
        protected override void LoadContent()
        {
            smokeAnimationTexture = Game.Content.Load<Texture2D>("Textures/SmokeAnimationStrip");
            smokePuffTexture = Game.Content.Load<Texture2D>("Textures/SmokePuff");
            position = new Vector2(Game.GraphicsDevice.Viewport.Width / 2 - (int)bodySize.X / 2,
                                   Game.GraphicsDevice.Viewport.Height / 2 - (int)bodySize.Y / 2);

            // Create smoke puffs for the smoke puff pool
            for (int i = 0; i < MaxSmokePuffs; i++)
            {
                availableSmokePuffs.Push(new SmokePuff(Game, gamePlayScreen, smokePuffTexture));
            }

            base.LoadContent();
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the beekeeper's status.
        /// </summary>
        /// <param name="gameTime">Game time information</param>
        public override void Update(GameTime gameTime)
        {
            if (!(gamePlayScreen.IsActive && gamePlayScreen.IsStarted))
            {
                base.Update(gameTime);
                return;
            }

            if (IsCollectingHoney)
            {
                // We want this animation to use a sub animation 
                // So must calculate when to call the sub animation
                if (collectingHoneyFrameCounter > 3)
                {
                    AnimationDefinitions[BeekeeperCollectingHoneyAnimationKey].Update(gameTime, true, true);
                }
                else
                {
                    AnimationDefinitions[BeekeeperCollectingHoneyAnimationKey].Update(gameTime, true, false);
                }

                collectingHoneyFrameCounter++;
            }
            else
            {
                collectingHoneyFrameCounter = 0;
            }


            if (isDepositingHoney)
            {
                if (depositHoneyUpdatingTimer == TimeSpan.Zero)
                {
                    depositHoneyUpdatingTimer = gameTime.TotalGameTime;
                }

                AnimationDefinitions[BeekeeperDepositingHoneyAnimationKey].Update(gameTime, true);
            }

            // The oldest smoke puff might have expired and should therefore be recycled
            if ((FiredSmokePuffs.Count > 0) && (FiredSmokePuffs.Peek().IsGone))
            {
                availableSmokePuffs.Push(FiredSmokePuffs.Dequeue());
            }

            // If the beeKeeper is stung by a bee we want to create a flashing 
            // effect. 
            if (isStung || isFlashing)
            {
                stungDrawingCounter++;

                if (stungDrawingCounter > stungDrawingInterval)
                {
                    stungDrawingCounter = 0;
                    isDrawnLastStungInterval = !isDrawnLastStungInterval;
                }
                // if time is up, end the flashing effect
                if (stungTime + stungDuration < gameTime.TotalGameTime)
                {
                    isStung = false;

                    if (stungTime + stungDuration + flashingDuration < gameTime.TotalGameTime)
                    {
                        isFlashing = false;
                        stungDrawingCounter = -1;
                    }

                    AnimationDefinitions[LegAnimationKey].Update(gameTime, IsInMotion);
                }
            }
            else
            {
                AnimationDefinitions[LegAnimationKey].Update(gameTime, IsInMotion);
            }

            if (needToShootSmoke)
            {
                AnimationDefinitions[SmokeAnimationKey].Update(gameTime, needToShootSmoke);

                shootSmokePuffTimer -= gameTime.ElapsedGameTime;
                if (shootSmokePuffTimer <= TimeSpan.Zero)
                {
                    ShootSmoke();
                    shootSmokePuffTimer = shootSmokePuffTimerInitialValue;
                }
            }

            base.Update(gameTime);
        }


        #endregion

        #region Render


        /// <summary>
        /// Renders the beekeeper.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (!(gamePlayScreen.IsActive && gamePlayScreen.IsStarted))
            {
                base.Draw(gameTime);
                return;
            }

            // Make sure not to draw the beekeeper while flashing
            if (isStung || isFlashing)
            {
                if (stungDrawingCounter != stungDrawingInterval)
                {
                    if (isDrawnLastStungInterval)
                    {
                        return;
                    }
                }
            }

            scaledSpriteBatch.Begin();

            // if stung we want to show another animation
            if (isStung)
            {
                scaledSpriteBatch.Draw(Game.Content.Load<Texture2D>("Textures/hit"), position, Color.White);
                scaledSpriteBatch.End();
                return;
            }

            // If collecting honey, draw the appropriate animation
            if (IsCollectingHoney)
            {
                AnimationDefinitions[BeekeeperCollectingHoneyAnimationKey].Draw(scaledSpriteBatch, position,
                    SpriteEffects.None);
                scaledSpriteBatch.End();
                return;
            }


            if (isDepositingHoney)
            {
                if (velocity != Vector2.Zero)
                {
                    isDepositingHoney = false;
                    AudioManager.StopSound("DepositingIntoVat_Loop");
                }

                // We want the deposit duration to sync with the deposit  
                // animation
                // So we manage the timing ourselves
                if (depositHoneyUpdatingTimer != TimeSpan.Zero &&
                    depositHoneyUpdatingTimer + depositHoneyUpdatingInterval < gameTime.TotalGameTime)
                {
                    depositHoneyTimerCounter++;
                    depositHoneyUpdatingTimer = TimeSpan.Zero;
                }

                AnimationDefinitions[BeekeeperDepositingHoneyAnimationKey].Draw(scaledSpriteBatch, position,
                    SpriteEffects.None);

                if (depositHoneyTimerCounter == honeyDepositFrameCount - 1)
                {
                    isDepositingHoney = false;
                    depositHoneyCallback.Invoke(null);
                    AnimationDefinitions[BeekeeperDepositingHoneyAnimationKey].PlayFromFrameIndex(0);
                }

                scaledSpriteBatch.End();
                return;
            }

            bool hadDirectionChanged = false;

            // See if the direction changed
            if (newDirection != direction)
            {
                hadDirectionChanged = true;
                direction = newDirection;
            }

            if (hadDirectionChanged)
            {
                // Update the animation
                lastFrameCounter = 0;
                AnimationDefinitions[LegAnimationKey].PlayFromFrameIndex(lastFrameCounter + (int)direction);
                AnimationDefinitions[ShootingAnimationKey].PlayFromFrameIndex(lastFrameCounter + (int)direction);
                AnimationDefinitions[BodyAnimationKey].PlayFromFrameIndex(lastFrameCounter + (int)direction);
            }
            else
            {
                // Because our animation is 8 cells, but the row is 16 cells,
                // we need to reset the counter after 8 rounds

                if (lastFrameCounter == 8)
                {
                    lastFrameCounter = 0;
                    AnimationDefinitions[LegAnimationKey].PlayFromFrameIndex(lastFrameCounter + (int)direction);
                    AnimationDefinitions[ShootingAnimationKey].PlayFromFrameIndex(
                        lastFrameCounter + (int)direction);
                    AnimationDefinitions[BodyAnimationKey].PlayFromFrameIndex(lastFrameCounter + (int)direction);
                }
                else
                {
                    lastFrameCounter++;
                }
            }


            AnimationDefinitions[LegAnimationKey].Draw(scaledSpriteBatch, position, 1f, SpriteEffects.None);


            if (needToShootSmoke)
            {
                // Draw the body
                AnimationDefinitions[ShootingAnimationKey].Draw(scaledSpriteBatch, position, 1f, SpriteEffects.None);

                // If true we need to draw smoke
                if (smokeAdjustment != Vector2.Zero)
                {
                    AnimationDefinitions[SmokeAnimationKey].Draw(scaledSpriteBatch, position + smokeAdjustment, 1f,
                        currentEffect);
                }
            }
            else
            {
                AnimationDefinitions[BodyAnimationKey].Draw(scaledSpriteBatch, position, 1f, SpriteEffects.None);
            }
            scaledSpriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Checks if a given rectanlge intersects with one of the smoke puffs fired by the beekeeper.
        /// </summary>
        /// <param name="checkRectangle">The rectangle to check for collisions with smoke puffs.</param>
        /// <returns>One of the smoke puffs with which the supplied regtangle collides, or null if it collides with
        /// none.</returns>
        public SmokePuff CheckSmokeCollision(Rectangle checkRectangle)
        {
            foreach (SmokePuff smokePuff in FiredSmokePuffs)
            {
                if (checkRectangle.HasCollision(smokePuff.CentralCollisionArea))
                {
                    return smokePuff;
                }
            }

            return null;
        }

        /// <summary>
        /// Maek the beekeeper as being stung by a bee.
        /// </summary>
        /// <param name="occurTime">The time at which the beekeeper was stung.</param>
        public void Stung(TimeSpan occurTime)
        {
            if (!isStung && !isFlashing)
            {
                isStung = true;
                isFlashing = true;
                stungTime = occurTime;
                needToShootSmoke = false;
            }
        }

        /// <summary>
        /// Updates the beekeeper's position.
        /// </summary>
        /// <param name="movement">A vector which contains the desired adjustment to 
        /// the beekeeper's position.</param>
        public void SetMovement(Vector2 movement)
        {
            if (!IsStung)
            {
                velocity = movement;
                position += velocity;
            }
        }

        /// <summary>
        /// Makes sure the beekeeper's direction matches his movement direction.
        /// </summary>
        /// <param name="movementDirection">A vector indicating the beekeeper's movement 
        /// direction.</param>
        public void SetDirection(Vector2 movementDirection)
        {
            DetermineDirection(movementDirection, ref newDirection, ref smokeAdjustment);
            currentEffect = GetSpriteEffect(movementDirection);
        }

        /// <summary>
        /// Starts the process of transferring honey to the honey vat.
        /// </summary>
        /// <param name="honeyDepositFrameCount">The amount of frames in the honey
        /// depositing animation.</param>
        /// <param name="callback">Callback to invoke once the process is 
        /// complete.</param>
        public void StartTransferHoney(int honeyDepositFrameCount, AsyncCallback callback)
        {
            depositHoneyCallback = callback;
            this.honeyDepositFrameCount = honeyDepositFrameCount;
            isDepositingHoney = true;
            depositHoneyTimerCounter = 0;


            AudioManager.PlaySound("DepositingIntoVat_Loop");
        }

        /// <summary>
        /// Marks the honey transfer process as complete.
        /// </summary>
        public void EndTransferHoney()
        {
            isDepositingHoney = false;
        }


        #endregion

        #region Private Methods


        /// <summary>
        /// Shoots a puff of smoke. If too many puffs of smoke have already been fired, the oldest one vanishes and
        /// is replaced with a new one.        
        /// </summary>
        private void ShootSmoke()
        {
            SmokePuff availableSmokePuff;

            if (availableSmokePuffs.Count > 0)
            {
                // Take a smoke puff from the pool
                availableSmokePuff = availableSmokePuffs.Pop();
            }
            else
            {
                // Take the oldest smoke puff and use it
                availableSmokePuff = FiredSmokePuffs.Dequeue();
            }

            Vector2 beeKeeperCenter = Bounds.Center.GetVector();
            Vector2 smokeInitialPosition = beeKeeperCenter;

            availableSmokePuff.Fire(smokeInitialPosition, GetSmokeVelocityVector());
            FiredSmokePuffs.Enqueue(availableSmokePuff);
        }

        /// <summary>
        /// Used to return a vector which will serve as shot smoke velocity.
        /// </summary>
        /// <returns>A vector which serves as the initial velocity of smoke puffs being shot.</returns>
        private Vector2 GetSmokeVelocityVector()
        {
            Vector2 initialVector;

            switch (direction)
            {
                case WalkingDirection.Down:
                    initialVector = new Vector2(0, 1);
                    break;
                case WalkingDirection.Up:
                    initialVector = new Vector2(0, -1);
                    break;
                case WalkingDirection.Left:
                    initialVector = new Vector2(-1, 0);
                    break;
                case WalkingDirection.Right:
                    initialVector = new Vector2(1, 0);
                    break;
                case WalkingDirection.LeftDown:
                    initialVector = new Vector2(-1, 1);
                    break;
                case WalkingDirection.RightDown:
                    initialVector = new Vector2(1, 1);
                    break;
                case WalkingDirection.LeftUp:
                    initialVector = new Vector2(-1, -1);
                    break;
                case WalkingDirection.RightUp:
                    initialVector = new Vector2(1, -1);
                    break;
                default:
                    throw new InvalidOperationException("Determining the vector for an invalid walking direction");
            }

            return initialVector * 2f + velocity * 1f;
        }

        /// <summary>
        /// Returns an effect appropriate to the supplied vector which either does
        /// nothing or flips the beekeeper horizontally.
        /// </summary>
        /// <param name="movementDirection">A vector depicting the beekeeper's 
        /// movement.</param>
        /// <returns>A sprite effect that should be applied to the beekeeper.</returns>
        private SpriteEffects GetSpriteEffect(Vector2 movementDirection)
        {
            // Checks if there is any movement input
            if (movementDirection != Vector2.Zero)
            {
                // If beekeeper is facing left
                if (movementDirection.X < 0)
                {
                    lastEffect = SpriteEffects.FlipHorizontally;
                }
                else if (movementDirection.X > 0)
                {
                    lastEffect = SpriteEffects.None;
                }
            }

            return lastEffect;
        }

        /// <summary>
        /// Returns movement information according to the current virtual thumbstick input.
        /// </summary>
        /// <param name="movement">Vector indicating the current beekeeper movement.</param>
        /// <param name="tempDirection">Enum describing the inpot direction.</param>
        /// <param name="smokeAdjustment">Adjustment to smoke position according to input direction.</param>
        private void DetermineDirection(Vector2 movement, ref WalkingDirection tempDirection,
            ref Vector2 smokeAdjustment)
        {
            if (movement == Vector2.Zero)
            {
                return;
            }

            if (Math.Abs(movement.X) > Math.Abs(movement.Y))
            {
                DetermineDirectionDominantX(movement, ref tempDirection, ref smokeAdjustment);
            }
            else
            {
                DetermineDirectionDominantY(movement, ref tempDirection, ref smokeAdjustment);
            }
        }

        /// <summary>
        /// Returns movement information according to the current virtual thumbstick input, given that advancement
        /// along the X axis is greater than along the Y axis.
        /// </summary>
        /// <param name="movement">Vector indicating the current beekeeper movement.</param>
        /// <param name="tempDirection">Enum describing the input direction.</param>
        /// <param name="smokeAdjustment">Adjustment to smoke position according to input direction.</param>
        private void DetermineDirectionDominantX(Vector2 movement, ref WalkingDirection tempDirection,
            ref Vector2 smokeAdjustment)
        {
            if (movement.X > 0)
            {
                if (movement.Y > 0.25f)
                {
                    tempDirection = WalkingDirection.RightDown;
                    smokeAdjustment = new Vector2(UIConstants.SprayRightOffset, UIConstants.SprayDownOffset);
                }
                else if (movement.Y < -0.25f)
                {
                    tempDirection = WalkingDirection.RightUp;
                    smokeAdjustment = new Vector2(UIConstants.SprayRightOffset, UIConstants.SprayUpOffset);
                }
                else
                {
                    tempDirection = WalkingDirection.Right;
                    smokeAdjustment = new Vector2(UIConstants.SprayRightOffset, UIConstants.SprayMiddleOffset);
                }
            }
            else
            {
                if (movement.Y > 0.25f)
                {
                    tempDirection = WalkingDirection.LeftDown;
                    smokeAdjustment = new Vector2(-UIConstants.SprayLeftOffset, UIConstants.SprayDownOffset);
                }
                else if (movement.Y < -0.25f)
                {
                    tempDirection = WalkingDirection.LeftUp;
                    smokeAdjustment = new Vector2(-UIConstants.SprayLeftOffset, UIConstants.SprayUpOffset);
                }
                else
                {
                    tempDirection = WalkingDirection.Left;
                    smokeAdjustment = new Vector2(-UIConstants.SprayLeftOffset, UIConstants.SprayMiddleOffset);
                }
            }
        }

        /// <summary>
        /// Returns movement information according to the current virtual thumbstick input, given that advancement
        /// along the Y axis is greater than along the X axis.
        /// </summary>
        /// <param name="movement">Vector indicating the current beekeeper movement.</param>      
        /// <param name="tempDirection">Enum describing the input direction.</param>
        /// <param name="smokeAdjustment">Adjustment to smoke position according to input direction.</param>
        private void DetermineDirectionDominantY(Vector2 movement, ref WalkingDirection tempDirection,
            ref Vector2 smokeAdjustment)
        {
            if (movement.Y > 0)
            {
                if (movement.X > 0.25f)
                {
                    tempDirection = WalkingDirection.RightDown;
                    smokeAdjustment = new Vector2(UIConstants.SprayRightOffset, UIConstants.SprayDownOffset);
                }
                else if (movement.X < -0.25f)
                {
                    tempDirection = WalkingDirection.LeftDown;
                    smokeAdjustment = new Vector2(-UIConstants.SprayLeftOffset, UIConstants.SprayDownOffset);
                }
                else
                {
                    tempDirection = WalkingDirection.Down;
                    smokeAdjustment = Vector2.Zero;
                }
            }
            else
            {
                if (movement.X > 0.25f)
                {
                    tempDirection = WalkingDirection.RightUp;
                    smokeAdjustment = new Vector2(UIConstants.SprayRightOffset, UIConstants.SprayUpOffset);
                }
                else if (movement.X < -0.25f)
                {
                    tempDirection = WalkingDirection.LeftUp;
                    smokeAdjustment = new Vector2(-UIConstants.SprayLeftOffset, UIConstants.SprayUpOffset);
                }
                else
                {
                    tempDirection = WalkingDirection.Up;
                    smokeAdjustment = Vector2.Zero;
                }
            }
        }


        #endregion
    }
}
