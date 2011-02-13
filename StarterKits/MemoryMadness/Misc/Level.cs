#region File Description

//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Linq;

#endregion

namespace MemoryMadness
{
    class Level : DrawableGameComponent
    {
        #region Fields/Properties

        public int levelNumber;
        LinkedList<ButtonColors[]> sequence;
        LinkedListNode<ButtonColors[]> currentSequenceItem;

        public LevelState CurrentState;
        public bool IsActive;

        /// <summary>
        /// The amount of moves correctly performed by the user so far
        /// </summary>
        public int MovesPerformed { get; set; }

        // Sequence demonstration delays are multiplied by this each level
        const float DifficultyFactor = 0.75f;

        // Define the delay between flashes when the current set of moves is 
        // demonstrated to the player
        TimeSpan delayFlashOn = TimeSpan.FromSeconds(1);
        TimeSpan delayFlashOff = TimeSpan.FromSeconds(0.5);

        // Define the allowed delay between two user inputs
        TimeSpan delayBetweenInputs = TimeSpan.FromSeconds(5);

        // Define the delay per move which will be used to calculate the overall time
        // the player has to input the sample. For example, if this delay is 4 and the
        // current level has 5 steps, the user will have 20 seconds overall to complete
        // the level.
        readonly TimeSpan DelayOverallPerInput = TimeSpan.FromSeconds(4);

        // The display period for the user's own input (feedback)
        readonly TimeSpan InputFlashDuration = TimeSpan.FromSeconds(0.75);

        TimeSpan delayPeriod;
        TimeSpan inputFlashPeriod;
        TimeSpan elapsedPatternInput;
        TimeSpan overallAllowedInputPeriod;

        bool flashOn;

        bool drawUserInput;

        ButtonColors?[] currentTouchSampleColors = new ButtonColors?[4];

        // Define spheres covering the various buttons
        BoundingSphere redShpere;
        BoundingSphere blueShpere;
        BoundingSphere greenShpere;
        BoundingSphere yellowShpere;

        // Rendering members
        SpriteBatch spriteBatch;
        Texture2D buttonsTexture;

        #endregion

        #region Initializaton

        public Level(Game game, SpriteBatch spriteBatch, int levelNumber,
            int movesPerformed, Texture2D buttonsTexture)
            : base(game)
        {
            this.levelNumber = levelNumber;
            this.spriteBatch = spriteBatch;
            CurrentState = LevelState.NotReady;
            this.buttonsTexture = buttonsTexture;
            MovesPerformed = movesPerformed;
        }

        public Level(Game game, SpriteBatch spriteBatch, int levelNumber,
            Texture2D buttonsTexture)
            : this(game, spriteBatch, levelNumber, 0, buttonsTexture)
        {
            
        }

        public override void Initialize()
        {
            //Update delays to match level difficulty
            UpdateDelays();

            // Define button bounding spheres
            DefineBoundingSpheres();

            // Load sequences for current level from definitions XML
            LoadLevelSequences();
        }

        #endregion        

        #region Update and Render

        public override void Update(GameTime gameTime)
        {
            if (!IsActive)
            {
                base.Update(gameTime);
                return;
            }

            switch (CurrentState)
            {
                case LevelState.NotReady:
                    // Nothing to update in this state
                    break;
                case LevelState.Ready:
                    // Wait for a while before demonstrating the level's move set
                    delayPeriod += gameTime.ElapsedGameTime;
                    if (delayPeriod >= delayFlashOn)
                    {
                        // Initiate flashing sequence
                        currentSequenceItem = sequence.First;
                        PlaySequenceStepSound();
                        CurrentState = LevelState.Flashing;
                        delayPeriod = TimeSpan.Zero;
                        flashOn = true;
                    }
                    break;
                case LevelState.Flashing:
                    // Display the level's move set. When done, start accepting
                    // user input
                    delayPeriod += gameTime.ElapsedGameTime;
                    if ((delayPeriod >= delayFlashOn) && (flashOn))
                    {
                        delayPeriod = TimeSpan.Zero;
                        flashOn = false;
                    }
                    if ((delayPeriod >= delayFlashOff) && (!flashOn))
                    {
                        delayPeriod = TimeSpan.Zero;
                        currentSequenceItem = currentSequenceItem.Next;
                        PlaySequenceStepSound();
                        flashOn = true;
                    }
                    if (currentSequenceItem == null)
                    {
                        InitializeUserInputStage();
                    }
                    break;
                case LevelState.Started:
                case LevelState.InProcess:
                    delayPeriod += gameTime.ElapsedGameTime;
                    inputFlashPeriod += gameTime.ElapsedGameTime;
                    elapsedPatternInput += gameTime.ElapsedGameTime;
                    if ((delayPeriod >= delayBetweenInputs) ||
                        (elapsedPatternInput >= overallAllowedInputPeriod))
                    {
                        // The user was not quick enough
                        inputFlashPeriod = TimeSpan.Zero;
                        CurrentState = LevelState.FinishedFail;
                    }
                    if (inputFlashPeriod >= InputFlashDuration)
                    {
                        drawUserInput = false;
                    }
                    break;
                case LevelState.Fault:
                    inputFlashPeriod += gameTime.ElapsedGameTime;
                    if (inputFlashPeriod >= InputFlashDuration)
                    {
                        drawUserInput = false;
                        CurrentState = LevelState.FinishedFail;
                    }
                    break;
                case LevelState.Success:
                    inputFlashPeriod += gameTime.ElapsedGameTime;
                    if (inputFlashPeriod >= InputFlashDuration)
                    {
                        drawUserInput = false;
                        CurrentState = LevelState.FinishedOk;
                    }
                    break;
                case LevelState.FinishedOk:
                    // Gameplay screen will advance the level
                    break;
                case LevelState.FinishedFail:
                    // Gameplay screen will reset the level
                    break;
                default:
                    break;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (IsActive)
            {
                spriteBatch.Begin();

                Rectangle redButtonRectangle = Settings.RedButtonDim;
                Rectangle greenButtonRectangle = Settings.GreenButtonDim;
                Rectangle blueButtonRectangle = Settings.BlueButtonDim;
                Rectangle yellowButtonRectangle = Settings.YellowButtonDim;

                // Draw the darkened buttons
                DrawDarkenedButtons(redButtonRectangle, greenButtonRectangle, 
                    blueButtonRectangle, yellowButtonRectangle);

                switch (CurrentState)
                {
                    case LevelState.NotReady:
                    case LevelState.Ready:
                        // Nothing extra to draw
                        break;
                    case LevelState.Flashing:
                        if ((currentSequenceItem != null) && (flashOn))
                        {
                            ButtonColors[] toDraw = currentSequenceItem.Value;
                            DrawLitButtons(toDraw);
                        }
                        break;
                    case LevelState.Started:
                    case LevelState.InProcess:
                    case LevelState.Fault:
                    case LevelState.Success:
                        if (drawUserInput)
                        {
                            List<ButtonColors> toDraw =
                                new List<ButtonColors>(currentTouchSampleColors.Length);

                            foreach (var touchColor in currentTouchSampleColors)
                            {
                                if (touchColor.HasValue)
                                {
                                    toDraw.Add(touchColor.Value);
                                }
                            }
                            
                            DrawLitButtons(toDraw.ToArray());
                        }
                        break;
                    case LevelState.FinishedOk:
                        break;
                    case LevelState.FinishedFail:
                        break;
                    default:
                        break;
                }

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        #endregion

        #region Public functionality

        /// <summary>
        /// Handle user presses.
        /// </summary>
        /// <param name="touchPoints">The locations touched by the user. The list
        /// is expected to contain at least one member.</param>
        public void RegisterTouch(List<TouchLocation> touchPoints)
        {
            if ((CurrentState == LevelState.Started ||
                CurrentState == LevelState.InProcess))
            {
                ButtonColors[] stepColors = sequence.First.Value;
                
                bool validTouchRegistered = false;

                if (touchPoints.Count > 0)
                {
                    // Reset current touch sample
                    for (int i = 0; i < Settings.ButtonAmount; i++)
                    {
                        currentTouchSampleColors[i] = null;
                    }

                    // Go over the touch points and populate the current touch sample
                    for (int i = 0; i < touchPoints.Count; i++)
                    {
                        var gestureBox = new BoundingBox(
                            new Vector3(touchPoints[i].Position.X - 5,
                                touchPoints[i].Position.Y - 5, 0),
                            new Vector3(touchPoints[i].Position.X + 10,
                                touchPoints[i].Position.Y + 10, 0));

                        if (redShpere.Intersects(gestureBox))
                        {
                            currentTouchSampleColors[i] = ButtonColors.Red;
                            AudioManager.PlaySound("red");
                        }
                        else if (yellowShpere.Intersects(gestureBox))
                        {
                            currentTouchSampleColors[i] = ButtonColors.Yellow;
                            AudioManager.PlaySound("yellow");
                        }
                        else if (blueShpere.Intersects(gestureBox))
                        {
                            currentTouchSampleColors[i] = ButtonColors.Blue;
                            AudioManager.PlaySound("blue");
                        }
                        else if (greenShpere.Intersects(gestureBox))
                        {
                            currentTouchSampleColors[i] = ButtonColors.Green;
                            AudioManager.PlaySound("green");
                        }

                        CurrentState = LevelState.InProcess;
                    }

                    List<ButtonColors> colorsHit =
                        new List<ButtonColors>(currentTouchSampleColors.Length);

                    // Check if the user pressed at least one of the colored buttons
                    foreach (var hitColor in currentTouchSampleColors)
                    {
                        if (hitColor.HasValue)
                        {
                            validTouchRegistered = true;
                            colorsHit.Add(hitColor.Value);
                        }
                    }                    
                    
                    // Find the buttons which the user failed to touch
                    List<ButtonColors> missedColors =
                        new List<ButtonColors>(stepColors.Length);

                    foreach (var stepColor in stepColors)
                    {
                        if (!colorsHit.Contains(stepColor))
                        {
                            missedColors.Add(stepColor);
                        }
                    }

                    // If the user failed to perform the current move, fail the level
                    // Do nothing if no buttons were touched
                    if (((missedColors.Count > 0) || 
                        (touchPoints.Count != stepColors.Length)) && validTouchRegistered)
                        CurrentState = LevelState.Fault;

                    if (validTouchRegistered)
                    {
                        // Show user pressed buttons, reset timeout period 
                        // for button flash
                        drawUserInput = true;
                        inputFlashPeriod = TimeSpan.Zero;

                        MovesPerformed++;
                        sequence.Remove(stepColors);

                        if ((sequence.Count == 0) && (CurrentState != LevelState.Fault))
                        {
                            CurrentState = LevelState.Success;
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Functionality

        /// <summary>
        /// Load sequences for the current level from definitions XML
        /// </summary>
        private void LoadLevelSequences()
        {
            XDocument doc = XDocument.Load(@"Content\Gameplay\LevelDefinitions.xml");
            var definitions = doc.Document.Descendants(XName.Get("Level"));

            XElement levelDefinition = null;

            foreach (var definition in definitions)
            {
                if (int.Parse(
                    definition.Attribute(XName.Get("Number")).Value) == levelNumber)
                {
                    levelDefinition = definition;
                    break;
                }
            }            

            int skipMoves = 0; // Used to skip moves if we are resuming a level mid-play

            // If definitions are found, create sequences
            if (null != levelDefinition)
            {
                sequence = new LinkedList<ButtonColors[]>();

                foreach (var pattern in
                    levelDefinition.Descendants(XName.Get("Pattern")))
                {
                    if (skipMoves < MovesPerformed)
                    {
                        skipMoves++;
                        continue;
                    }

                    string[] values = pattern.Value.Split(',');
                    ButtonColors[] colors = new ButtonColors[values.Length];

                    // Add each color to a sequence
                    for (int i = 0; i < values.Length; i++)
                    {
                        colors[i] = (ButtonColors)Enum.Parse(
                            typeof(ButtonColors), values[i], true);
                    }

                    // Add each sequence to the sequence list
                    sequence.AddLast(colors);
                }

                if (MovesPerformed == 0)
                {
                    CurrentState = LevelState.Ready;
                    delayPeriod = TimeSpan.Zero;
                }
                else
                {
                    InitializeUserInputStage();
                }
            }
        }

        /// <summary>
        /// Define button bounding spheres
        /// </summary>
        private void DefineBoundingSpheres()
        {
            redShpere = new BoundingSphere(
                new Vector3(
                    Settings.RedButtonPosition.X + Settings.ButtonSize.X / 2,
                    Settings.RedButtonPosition.Y + Settings.ButtonSize.Y / 2, 0),
                Settings.ButtonSize.X / 2);
            blueShpere = new BoundingSphere(
                new Vector3(
                    Settings.BlueButtonPosition.X + Settings.ButtonSize.X / 2,
                    Settings.BlueButtonPosition.Y + Settings.ButtonSize.Y / 2, 0),
                Settings.ButtonSize.X / 2);
            greenShpere = new BoundingSphere(
                new Vector3(
                    Settings.GreenButtonPosition.X + Settings.ButtonSize.X / 2,
                    Settings.GreenButtonPosition.Y + Settings.ButtonSize.Y / 2, 0),
                Settings.ButtonSize.X / 2);
            yellowShpere = new BoundingSphere(
                new Vector3(
                    Settings.YellowButtonPosition.X + Settings.ButtonSize.X / 2,
                    Settings.YellowButtonPosition.Y + Settings.ButtonSize.Y / 2, 0),
                Settings.ButtonSize.X / 2);
        }

        /// <summary>
        /// Update delays to match level difficulty
        /// </summary>
        private void UpdateDelays()
        {
            delayFlashOn = TimeSpan.FromTicks(
                (long)(delayFlashOn.Ticks *
                Math.Pow(DifficultyFactor, levelNumber - 1)));

            delayFlashOff = TimeSpan.FromTicks(
                (long)(delayFlashOff.Ticks *
                Math.Pow(DifficultyFactor, levelNumber - 1)));
        }

        /// <summary>
        /// Sets various members to allow the user to supply input.
        /// </summary>
        private void InitializeUserInputStage()
        {
            elapsedPatternInput = TimeSpan.Zero;
            overallAllowedInputPeriod = TimeSpan.Zero;
            CurrentState = LevelState.Started;
            drawUserInput = false;
            // Calculate total allowed timeout period for the entire level
            overallAllowedInputPeriod = TimeSpan.FromSeconds(
                DelayOverallPerInput.TotalSeconds * sequence.Count);
        }

        /// <summary>
        /// Draws the set of lit buttons according to the colors specified.
        /// </summary>
        /// <param name="toDraw">The array of colors representing the lit 
        /// buttons.</param>
        private void DrawLitButtons(ButtonColors[] toDraw)
        {
            Vector2 position = Vector2.Zero;
            Rectangle rectangle = Rectangle.Empty;

            for (int i = 0; i < toDraw.Length; i++)
            {
                switch (toDraw[i])
                {
                    case ButtonColors.Red:
                        position = Settings.RedButtonPosition;
                        rectangle = Settings.RedButtonLit;
                        break;
                    case ButtonColors.Yellow:
                        position = Settings.YellowButtonPosition;
                        rectangle = Settings.YellowButtonLit;
                        break;
                    case ButtonColors.Blue:
                        position = Settings.BlueButtonPosition;
                        rectangle = Settings.BlueButtonLit;
                        break;
                    case ButtonColors.Green:
                        position = Settings.GreenButtonPosition;
                        rectangle = Settings.GreenButtonLit;
                        break;
                }

                spriteBatch.Draw(buttonsTexture, position, rectangle, Color.White);
            }
        }

        /// <summary>
        /// Draw the darkened buttons
        /// </summary>
        /// <param name="redButtonRectangle">Red button rectangle 
        /// in source texture.</param>
        /// <param name="greenButtonRectangle">Green button rectangle 
        /// in source texture.</param>
        /// <param name="blueButtonRectangle">Blue button rectangle
        /// in source texture.</param>
        /// <param name="yellowButtonRectangle">Yellow button rectangle
        /// in source texture.</param>
        private void DrawDarkenedButtons(Rectangle redButtonRectangle, 
            Rectangle greenButtonRectangle, Rectangle blueButtonRectangle, 
            Rectangle yellowButtonRectangle)
        {
            spriteBatch.Draw(buttonsTexture, Settings.RedButtonPosition,
                redButtonRectangle, Color.White);
            spriteBatch.Draw(buttonsTexture, Settings.GreenButtonPosition,
                greenButtonRectangle, Color.White);
            spriteBatch.Draw(buttonsTexture, Settings.BlueButtonPosition,
                blueButtonRectangle, Color.White);
            spriteBatch.Draw(buttonsTexture, Settings.YellowButtonPosition,
                yellowButtonRectangle, Color.White);
        }

        /// <summary>
        /// Plays a sound appropriate to the current color flashed by the computer
        /// </summary>
        private void PlaySequenceStepSound()
        {
            if (currentSequenceItem == null)
            {
                return;
            }

            for (int i = 0; i < currentSequenceItem.Value.Length; ++i)
            {
                switch (currentSequenceItem.Value[i])
                {
                    case ButtonColors.Red:
                        AudioManager.PlaySound("red");
                        break;
                    case ButtonColors.Yellow:
                        AudioManager.PlaySound("yellow");
                        break;
                    case ButtonColors.Blue:
                        AudioManager.PlaySound("blue");
                        break;
                    case ButtonColors.Green:
                        AudioManager.PlaySound("green");
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
