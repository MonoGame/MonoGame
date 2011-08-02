#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace NetRumble
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This public class tracks
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    /// <remarks>
    /// This public class is similar to one in the GameStateManagement sample.
    /// </remarks>
    public class InputState
    {
        #region Fields

        public KeyboardState CurrentKeyboardState;
        public GamePadState CurrentGamePadState;

        public KeyboardState LastKeyboardState;
        public GamePadState LastGamePadState;

        #endregion

        #region Properties


        /// <summary>
        /// Checks for a "menu up" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuUp
        {
            get
            {
                return IsNewKeyPress(Keys.Up) ||
                       (CurrentGamePadState.DPad.Up == ButtonState.Pressed &&
                        LastGamePadState.DPad.Up == ButtonState.Released) ||
                       (CurrentGamePadState.ThumbSticks.Left.Y > 0 &&
                        LastGamePadState.ThumbSticks.Left.Y <= 0);
            }
        }


        /// <summary>
        /// Checks for a "menu down" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuDown
        {
            get
            {
                return IsNewKeyPress(Keys.Down) ||
                       (CurrentGamePadState.DPad.Down == ButtonState.Pressed &&
                        LastGamePadState.DPad.Down == ButtonState.Released) ||
                       (CurrentGamePadState.ThumbSticks.Left.Y < 0 &&
                        LastGamePadState.ThumbSticks.Left.Y >= 0);
            }
        }


        /// <summary>
        /// Checks for a "menu select" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuSelect
        {
            get
            {
                return IsNewKeyPress(Keys.Space) ||
                       IsNewKeyPress(Keys.Enter) ||
                       (CurrentGamePadState.Buttons.A == ButtonState.Pressed &&
                        LastGamePadState.Buttons.A == ButtonState.Released) ||
                       (CurrentGamePadState.Buttons.Start == ButtonState.Pressed &&
                        LastGamePadState.Buttons.Start == ButtonState.Released);
            }
        }


        /// <summary>
        /// Checks for a "menu cancel" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuCancel
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       (CurrentGamePadState.Buttons.B == ButtonState.Pressed &&
                        LastGamePadState.Buttons.B == ButtonState.Released) ||
                       (CurrentGamePadState.Buttons.Back == ButtonState.Pressed &&
                        LastGamePadState.Buttons.Back == ButtonState.Released);
            }
        }


        /// <summary>
        /// Checks for a "pause the game" input action (on either keyboard or gamepad).
        /// </summary>
        public bool PauseGame
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       (CurrentGamePadState.Buttons.Back == ButtonState.Pressed &&
                        LastGamePadState.Buttons.Back == ButtonState.Released) ||
                       (CurrentGamePadState.Buttons.Start == ButtonState.Pressed &&
                        LastGamePadState.Buttons.Start == ButtonState.Released);
            }
        }


        /// <summary>
        /// Checks for a positive "ship color change" input action
        /// </summary>
        public bool ShipColorChangeUp
        {
            get
            {
                return IsNewKeyPress(Keys.Up) ||
                   (CurrentGamePadState.Buttons.RightShoulder == ButtonState.Pressed &&
                    LastGamePadState.Buttons.RightShoulder == ButtonState.Released);
            }
        }


        /// <summary>
        /// Checks for a negative "ship color change" input action.
        /// </summary>
        public bool ShipColorChangeDown
        {
            get
            {
                return IsNewKeyPress(Keys.Down) ||
                    (CurrentGamePadState.Buttons.LeftShoulder == ButtonState.Pressed &&
                     LastGamePadState.Buttons.LeftShoulder == ButtonState.Released);
            }
        }



        /// <summary>
        /// Checks for a positive "ship model change" input action.
        /// </summary>
        public bool ShipModelChangeUp
        {
            get
            {
                return IsNewKeyPress(Keys.Right) ||
                    (CurrentGamePadState.Triggers.Right >= 1f &&
                     LastGamePadState.Triggers.Right < 1f);
            }
        }


        /// <summary>
        /// Checks for a negative "ship model change" input action.
        /// </summary>
        public bool ShipModelChangeDown
        {
            get
            {
                return IsNewKeyPress(Keys.Left) ||
                    (CurrentGamePadState.Triggers.Left >= 1f &&
                     LastGamePadState.Triggers.Left < 1f);
            }
        }


        /// <summary>
        /// Checks for a "mark ready" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MarkReady
        {
            get
            {
                return IsNewKeyPress(Keys.X) ||
                       (CurrentGamePadState.Buttons.X == ButtonState.Pressed &&
                        LastGamePadState.Buttons.X == ButtonState.Released);
            }
        }


        #endregion

        #region Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            LastKeyboardState = CurrentKeyboardState;
            LastGamePadState = CurrentGamePadState;

            CurrentKeyboardState = Keyboard.GetState();
            CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
            return (CurrentKeyboardState.IsKeyDown(key) &&
                    LastKeyboardState.IsKeyUp(key));
        }


        #endregion
    }
}
