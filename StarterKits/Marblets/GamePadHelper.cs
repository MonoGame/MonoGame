#region File Description
//-----------------------------------------------------------------------------
// GamePadHelper.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
#endregion

namespace Marblets
{
    /// <summary>
    /// Useful class that wraps some game pad stuff to give you indication of single 
    /// button presses by remembering previous state. Right now its one shot which means
    /// if you call a Pressed function that will 'remove' the press.
    /// 
    /// Keyboard support should be mapped in here based on PlayerIndex.
    /// PlayerIndex.One Key mapping is (Keys for player one to use)
    /// PlayerIndex.Two Key mapping is (Keys for Player two to use)
    /// Players Three => Infinity are not supported on a keyboard!
    /// </summary>
    public enum GamePadKey
    {
        /// <summary>
        /// Start button
        /// </summary>
        Start = 0,
        /// <summary>
        /// Back buton
        /// </summary>
        Back,
        /// <summary>
        /// A button
        /// </summary>
        A,
        /// <summary>
        /// B button
        /// </summary>
        B,
        /// <summary>
        /// X button
        /// </summary>
        X,
        /// <summary>
        /// Y button
        /// </summary>
        Y,
        /// <summary>
        /// Up Dpad
        /// </summary>
        Up,
        /// <summary>
        /// Down Dpad
        /// </summary>
        Down,
        /// <summary>
        /// Left Dpad
        /// </summary>
        Left,
        /// <summary>
        /// Right Dpad
        /// </summary>
        Right,
    };

    /// <summary>
    /// XNA gamepads only give you 'isUp/Down' options for buttons and key presses. 
    /// This class allows you to detect an up/down press combination
    /// </summary>
    public class GamePadHelper
    {
        private PlayerIndex player;
        private Dictionary<GamePadKey, Keys> keyMapping =
                                             new Dictionary<GamePadKey, Keys>();

        private Game game;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="player">Which player.</param>
        public GamePadHelper(PlayerIndex player)
        {
            //Need to store the player. If you try to store a reference to the GamePad
            //here it seems to 'forget'
            this.player = player;

            // Setup Dictionary with defaults
            keyMapping.Add(GamePadKey.Start, Keys.Home);
            keyMapping.Add(GamePadKey.Back, Keys.End);
            keyMapping.Add(GamePadKey.A, Keys.A);
            keyMapping.Add(GamePadKey.B, Keys.B);
            keyMapping.Add(GamePadKey.X, Keys.X);
            keyMapping.Add(GamePadKey.Y, Keys.Y);
            keyMapping.Add(GamePadKey.Up, Keys.Up);
            keyMapping.Add(GamePadKey.Down, Keys.Down);
            keyMapping.Add(GamePadKey.Left, Keys.Left);
            keyMapping.Add(GamePadKey.Right, Keys.Right);
        }


        private bool AWasReleased;
        private bool BWasReleased;
        private bool YWasReleased;
        private bool XWasReleased;
        private bool StartWasReleased;
        private bool BackWasReleased;
        private bool UpWasReleased;
        private bool DownWasReleased;
        private bool LeftWasReleased;
        private bool RightWasReleased;

        private bool AKeyWasReleased;
        private bool BKeyWasReleased;
        private bool YKeyWasReleased;
        private bool XKeyWasReleased;
        private bool StartKeyWasReleased;
        private bool BackKeyWasReleased;
        private bool UpKeyWasReleased;
        private bool DownKeyWasReleased;
        private bool LeftKeyWasReleased;
        private bool RightKeyWasReleased;

        private GamePadState state;
        private KeyboardState keyState;


        /// <summary>
        /// Has the A button been pressed
        /// </summary>
        public bool APressed
        {
            get
            {
                return (
                       (checkPressed(state.Buttons.A, ref AWasReleased)) ||
                       (checkPressed(keyState.IsKeyDown(keyMapping[GamePadKey.A]),
                        ref AKeyWasReleased))
                        );
            }
        }

        /// <summary>
        /// Has the B button been pressed
        /// </summary>
        public bool BPressed
        {
            get
            {
                return (
                       (checkPressed(state.Buttons.B, ref BWasReleased)) ||
                       (checkPressed(keyState.IsKeyDown(keyMapping[GamePadKey.B]),
                        ref BKeyWasReleased))
                        );
            }
        }

        /// <summary>
        /// Has the Y button been pressed
        /// </summary>
        public bool YPressed
        {
            get
            {
                return (
                       (checkPressed(state.Buttons.Y, ref YWasReleased)) ||
                       (checkPressed(keyState.IsKeyDown(keyMapping[GamePadKey.Y]),
                        ref YKeyWasReleased))
                        );
            }
        }

        /// <summary>
        /// Has the X button been pressed
        /// </summary>
        public bool XPressed
        {
            get
            {
                return (
                       (checkPressed(state.Buttons.X, ref XWasReleased)) ||
                       (checkPressed(keyState.IsKeyDown(keyMapping[GamePadKey.X]),
                        ref XKeyWasReleased))
                        );
            }
        }

        /// <summary>
        /// Has the start button been pressed
        /// </summary>
        public bool StartPressed
        {
            get
            {
                return (
                       (checkPressed(state.Buttons.Start, ref StartWasReleased)) ||
                       (checkPressed(keyState.IsKeyDown(keyMapping[GamePadKey.Start]),
                        ref StartKeyWasReleased))
                        );
            }
        }

        /// <summary>
        /// Has the back button been pressed
        /// </summary>
        public bool BackPressed
        {
            get
            {
                return (
                       (checkPressed(state.Buttons.Back, ref BackWasReleased)) ||
                       (checkPressed(keyState.IsKeyDown(keyMapping[GamePadKey.Back]),
                        ref BackKeyWasReleased))
                        );
            }
        }

        /// <summary>
        /// Has the up dpad been pressed
        /// </summary>
        public bool UpPressed
        {
            get
            {
                return (
                       (checkPressed(state.DPad.Up, ref UpWasReleased)) ||
                       (checkPressed(keyState.IsKeyDown(keyMapping[GamePadKey.Up]),
                        ref UpKeyWasReleased))
                        );
            }
        }

        /// <summary>
        /// Has the down dpad been pressed
        /// </summary>
        public bool DownPressed
        {
            get
            {
                return (
                       (checkPressed(state.DPad.Down, ref DownWasReleased)) ||
                       (checkPressed(keyState.IsKeyDown(keyMapping[GamePadKey.Down]),
                        ref DownKeyWasReleased))
                        );
            }
        }

        /// <summary>
        /// Has the left dpad been pressed
        /// </summary>
        public bool LeftPressed
        {
            get
            {
                return (
                       (checkPressed(state.DPad.Left, ref LeftWasReleased)) ||
                       (checkPressed(keyState.IsKeyDown(keyMapping[GamePadKey.Left]),
                        ref LeftKeyWasReleased))
                        );
            }
        }

        /// <summary>
        /// Has the right dpad been pressed
        /// </summary>
        public bool RightPressed
        {
            get
            {
                return (
                       (checkPressed(state.DPad.Right, ref RightWasReleased)) ||
                       (checkPressed(keyState.IsKeyDown(keyMapping[GamePadKey.Right]),
                        ref RightKeyWasReleased))
                        );
            }
        }

        private bool checkPressed(ButtonState buttonState, ref bool controlWasReleased)
        {
            //Buttons are considered pressed when their state = Pressed or their key 
            //equivalent is down
            return checkPressed(buttonState == ButtonState.Pressed,
                                ref controlWasReleased);
        }

        private bool checkPressed(bool pressed, ref bool controlWasReleased)
        {
            bool returnValue = controlWasReleased && pressed;
            if (game != null && game.IsActive)
            {
                //If the item is currently pressed then reset the 'released' indicators
                if (returnValue)
                {
                    controlWasReleased = false;
                }
            }
            else
            {
                return false;  // Control can never be pressed, game is not the active 
                // application!
            }

            return returnValue;
        }


        /// <summary>
        /// Updates the states. Should be called once per frame in the game loop 
        /// otherwise the IsPressed functions won't work
        /// </summary>
        public void Update(Game game)
        {
            state = GamePad.GetState(player);

            keyState = Keyboard.GetState();

            this.game = game;

            if (state.IsConnected)
            {
                //Check which buttons have been released so we can detect presses
                if ((state.Buttons.A == ButtonState.Released))
                    AWasReleased = true;
                if ((state.Buttons.B == ButtonState.Released))
                    BWasReleased = true;
                if ((state.Buttons.Y == ButtonState.Released))
                    YWasReleased = true;
                if ((state.Buttons.X == ButtonState.Released))
                    XWasReleased = true;
                if ((state.Buttons.Start == ButtonState.Released))
                    StartWasReleased = true;
                if ((state.Buttons.Back == ButtonState.Released))
                    BackWasReleased = true;
                if ((state.DPad.Up == ButtonState.Released))
                    UpWasReleased = true;
                if ((state.DPad.Down == ButtonState.Released))
                    DownWasReleased = true;
                if ((state.DPad.Left == ButtonState.Released))
                    LeftWasReleased = true;
                if ((state.DPad.Right == ButtonState.Released))
                    RightWasReleased = true;
            }

            //Check which keys on the keyboard have been released so we can detect 
            //presses
            if (!keyState.IsKeyDown(keyMapping[GamePadKey.A]))
                AKeyWasReleased = true;
            if (!keyState.IsKeyDown(keyMapping[GamePadKey.B]))
                BKeyWasReleased = true;
            if (!keyState.IsKeyDown(keyMapping[GamePadKey.Y]))
                YKeyWasReleased = true;
            if (!keyState.IsKeyDown(keyMapping[GamePadKey.X]))
                XKeyWasReleased = true;
            if (!keyState.IsKeyDown(keyMapping[GamePadKey.Start]))
                StartKeyWasReleased = true;
            if (!keyState.IsKeyDown(keyMapping[GamePadKey.Back]))
                BackKeyWasReleased = true;
            if (!keyState.IsKeyDown(keyMapping[GamePadKey.Up]))
                UpKeyWasReleased = true;
            if (!keyState.IsKeyDown(keyMapping[GamePadKey.Down]))
                DownKeyWasReleased = true;
            if (!keyState.IsKeyDown(keyMapping[GamePadKey.Left]))
                LeftKeyWasReleased = true;
            if (!keyState.IsKeyDown(keyMapping[GamePadKey.Right]))
                RightKeyWasReleased = true;
        }
    }
}
