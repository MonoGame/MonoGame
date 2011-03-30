#region File Description

//-----------------------------------------------------------------------------
// Settings.cs
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

#endregion

namespace MemoryMadness
{
    enum ButtonColors
    {
        Red,
        Yellow,
        Blue,
        Green
    }

    enum LevelState
    {
        NotReady,
        Ready,
        Flashing,
        Started,
        InProcess,
        Fault,
        Success,
        FinishedOk,
        FinishedFail
    }

    enum TouchInputState
    {
        Idle,
        GracePeriod
    }

    static class Constants
    {        
        public const string HighscorePopupTitle = "You made a high score!";
        public const string HighscorePopupText = "Enter your name (max 15 characters)";
        public const string HighscorePopupDefault = "Player";
    }

    static class Settings
    {
        // Amount of buttons
        public static int ButtonAmount = 4;

        // Sliding doors animation constants
        public static int DoorsAnimationStep = 5;
        public static Vector2 LeftDoorClosedPosition = new Vector2(0, 233);
        public static Vector2 LeftDoorOpenedPosition = new Vector2(-54, 233);
        public static Vector2 RightDoorClosedPosition = new Vector2(230, 233);
        public static Vector2 RightDoorOpenedPosition = new Vector2(284, 233);

        // Color button locations
        public static Vector2 RedButtonPosition = new Vector2(14,29);
        public static Vector2 GreenButtonPosition = new Vector2(161, 29);
        public static Vector2 BlueButtonPosition = new Vector2(14, 378);
        public static Vector2 YellowButtonPosition = new Vector2(161, 378);

        // Color button positions in the texture strip, with their sizes
        public static Vector2 ButtonSize = new Vector2(111, 123);
        public static Rectangle RedButtonDim = new Rectangle((int)ButtonSize.X * 0, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);
        public static Rectangle RedButtonLit = new Rectangle((int)ButtonSize.X * 1, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);

        public static Rectangle YellowButtonDim = new Rectangle((int)ButtonSize.X * 2, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);
        public static Rectangle YellowButtonLit = new Rectangle((int)ButtonSize.X * 3, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);

        public static Rectangle GreenButtonDim = new Rectangle((int)ButtonSize.X * 4, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);
        public static Rectangle GreenButtonLit = new Rectangle((int)ButtonSize.X * 5, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);

        public static Rectangle BlueButtonDim = new Rectangle((int)ButtonSize.X * 6, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);
        public static Rectangle BlueButtonLit = new Rectangle((int)ButtonSize.X * 7, 0, 
            (int)ButtonSize.X, (int)ButtonSize.Y);
    }
}
