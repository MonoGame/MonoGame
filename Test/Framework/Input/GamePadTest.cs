// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NUnit.Framework;

namespace MonoGame.Tests
{
    public class GamePadTest
    {

        [TestCase(ButtonState.Pressed, ButtonState.Pressed, ButtonState.Pressed, ButtonState.Pressed)]
        [TestCase(ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released)]
        [TestCase(ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed, ButtonState.Released)]
        public void DpadTest(ButtonState up, ButtonState down, ButtonState left, ButtonState right)
        {
            var pad = new GamePadDPad(up, down, left, right);
            Assert.AreEqual(up, pad.Up);
            Assert.AreEqual(down, pad.Down);
            Assert.AreEqual(left, pad.Left);
            Assert.AreEqual(right, pad.Right);

            var pad2 = new GamePadDPad
            {
                Up = up,
                Down = down,
                Left = left,
                Right = right
            };

            Assert.AreEqual(pad, pad2);
        }

        [Test]
        public void TriggersTest([Range(0f, 1f, 0.5f)] float left, [Range(0f, 1f, 0.5f)] float right)
        {
            var triggers = new GamePadTriggers(left, right);
            Assert.AreEqual(left, triggers.Left);
            Assert.AreEqual(right, triggers.Right);

            var triggers2 = new GamePadTriggers
            {
                Left = left,
                Right = right
            };
            Assert.AreEqual(triggers, triggers2);
        }

        #region State

        private const int Count = 6;
        public static IEnumerable<Buttons[]> GetButtons()
        {
            return new[]
            {
                // All
                Enum.GetValues(typeof(Buttons)).Cast<Buttons>().ToArray(),
                // None
                new Buttons[0], 
                // Random
                new [] { Buttons.Start, Buttons.LeftStick, Buttons.DPadDown, Buttons.A},
                new [] { Buttons.Back, Buttons.BigButton, Buttons.LeftStick, Buttons.Y, Buttons.B, Buttons.RightShoulder, Buttons.LeftTrigger},
                new [] { Buttons.RightTrigger, Buttons.RightStick, Buttons.RightShoulder, Buttons.DPadDown, Buttons.DPadLeft },
                new [] { Buttons.Start, Buttons.Back, Buttons.LeftShoulder, Buttons.X, Buttons.Y, Buttons.B, Buttons.A }
            };
        }

        [Test, Sequential]
        public void TestState([Random(-1f, 1f, Count)] double leftX, [Random(-1f, 1f, Count)] double leftY, 
            [Random(-1f, 1f, Count)] double rightX, [Random(-1f, 1f, Count)] double rightY, 
            [Random(0f, 1f, Count)] double doubleLT, [Random(0f, 1f, Count)] double doubleRT, 
            [ValueSource("GetButtons")] Buttons[] buttons, [Values(true, false, true, false)] bool isConnected)
        {
            var leftStick = new Vector2((float) leftX, (float) leftY);
            var rightStick = new Vector2((float) rightX, (float) rightY);
            var leftTrigger = (float) doubleLT;
            var rightTrigger = (float) doubleRT;
            Buttons allButtons = 0;
            if (buttons.Any())
                foreach (var button in buttons)
                    allButtons |= button;

            var state = new GamePadState(leftStick, rightStick, leftTrigger, rightTrigger, allButtons);
            state.IsConnected = isConnected;
            Assert.AreEqual(isConnected, state.IsConnected);
            
            Assert.AreEqual(leftStick, state.ThumbSticks.Left);
            Assert.AreEqual(rightStick, state.ThumbSticks.Right);
            Assert.AreEqual(leftTrigger, state.Triggers.Left);
            Assert.AreEqual(rightTrigger, state.Triggers.Right);
            AssertButtons(buttons, state);
        }

        private static void AssertButtons(IEnumerable<Buttons> pressedButtons, GamePadState state)
        {
            Buttons joinedButtons = 0;
            foreach (var button in pressedButtons)
                joinedButtons |= button;

            var gamePadButtons = state.Buttons;
            Assert.AreEqual(joinedButtons, gamePadButtons.buttons);

            // all buttons except for thumbstick position buttons and triggers (they're not controlled via buttons here)
            var allButtons = Enum.GetValues(typeof(Buttons)).OfType<Buttons>()
                .Where(b => !Regex.IsMatch(Enum.GetName(typeof(Buttons), b), "(Thumbstick|Trigger)"));

            foreach (var button in allButtons)
            {
                if (pressedButtons.Contains(button))
                {
                    Assert.IsTrue(state.IsButtonDown(button));
                    Assert.IsFalse(state.IsButtonUp(button));
                }
                else
                {
                    Assert.IsTrue(state.IsButtonUp(button));
                    Assert.IsFalse(state.IsButtonDown(button));
                }
            }
        }

        #endregion
    }
}