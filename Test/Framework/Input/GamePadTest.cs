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

namespace MonoGame.Tests.Input
{
    public class GamePadTest
    {
        #region GamePadButtons

#if !XNA
        [TestCaseSource("GetButtons")]
        public void GamePadButtonsTest(params Buttons[] buttons)
        {
            var gpb = new GamePadButtons(buttons);

            Assert.AreEqual(buttons.Contains(Buttons.A) ? ButtonState.Pressed : ButtonState.Released, gpb.A);
            Assert.AreEqual(buttons.Contains(Buttons.B) ? ButtonState.Pressed : ButtonState.Released, gpb.B);
            Assert.AreEqual(buttons.Contains(Buttons.Back) ? ButtonState.Pressed : ButtonState.Released, gpb.Back);
            Assert.AreEqual(buttons.Contains(Buttons.X) ? ButtonState.Pressed : ButtonState.Released, gpb.X);
            Assert.AreEqual(buttons.Contains(Buttons.Y) ? ButtonState.Pressed : ButtonState.Released, gpb.Y);
            Assert.AreEqual(buttons.Contains(Buttons.Start) ? ButtonState.Pressed : ButtonState.Released, gpb.Start);
            Assert.AreEqual(buttons.Contains(Buttons.LeftShoulder) ? ButtonState.Pressed : ButtonState.Released, gpb.LeftShoulder);
            Assert.AreEqual(buttons.Contains(Buttons.LeftStick) ? ButtonState.Pressed : ButtonState.Released, gpb.LeftStick);
            Assert.AreEqual(buttons.Contains(Buttons.RightShoulder) ? ButtonState.Pressed : ButtonState.Released, gpb.RightShoulder);
            Assert.AreEqual(buttons.Contains(Buttons.RightStick) ? ButtonState.Pressed : ButtonState.Released, gpb.RightStick);
            Assert.AreEqual(buttons.Contains(Buttons.BigButton) ? ButtonState.Pressed : ButtonState.Released, gpb.BigButton);
        }
#endif

        #endregion

        #region DPad

        [TestCase(ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released)]
        [TestCase(ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Released)]
        [TestCase(ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Released)]
        [TestCase(ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Released)]
        [TestCase(ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Pressed)]
        public void DpadTest(ButtonState up, ButtonState down, ButtonState left, ButtonState right)
        {
            var pad = new GamePadDPad(up, down, left, right);
            Assert.AreEqual(up, pad.Up);
            Assert.AreEqual(down, pad.Down);
            Assert.AreEqual(left, pad.Left);
            Assert.AreEqual(right, pad.Right);

#if !XNA
            var pad2 = new GamePadDPad(up, down, left, right);

            Assert.AreEqual(pad, pad2);
            Assert.AreEqual(pad.GetHashCode(), pad2.GetHashCode());

            var buttons = (Buttons) 0;
            if (up == ButtonState.Pressed) buttons |= Buttons.DPadUp;
            if (down == ButtonState.Pressed) buttons |= Buttons.DPadDown;
            if (left == ButtonState.Pressed) buttons |= Buttons.DPadLeft;
            if (right == ButtonState.Pressed) buttons |= Buttons.DPadRight;

            var pad3 = new GamePadDPad(buttons);
            Assert.AreEqual(pad, pad3);
            Assert.AreEqual(pad.GetHashCode(), pad3.GetHashCode());
#endif
        }

        #endregion

        #region Triggers

        [Test]
        public void TriggersTest([Range(-0.5f, 1.5f, 0.5f)] float left, [Range(1.5f, -0.5f, -0.5f)] float right)
        {
            var triggers = new GamePadTriggers(left, right);
            Assert.AreEqual(MathHelper.Clamp(left, 0f, 1f), triggers.Left);
            Assert.AreEqual(MathHelper.Clamp(right, 0f, 1f), triggers.Right);

#if !XNA
            var triggers2 = new GamePadTriggers(left, right);
            Assert.AreEqual(triggers, triggers2);
            Assert.AreEqual(triggers.GetHashCode(), triggers2.GetHashCode());
#endif
        }

        #endregion

        #region Thumbsticks

#if !XNA
        [TestCaseSource("ThumbStickVirtualButtonsIgnoreDeadZoneTestCases")]
        public void ThumbStickVirtualButtonsIgnoreDeadZone(Vector2 left, Vector2 right, GamePadDeadZone deadZone, Buttons expectedButtons)
        {
            var state = new GamePadState(new GamePadThumbSticks(left, right, deadZone, deadZone), new GamePadTriggers(), new GamePadButtons(), new GamePadDPad());

            Assert.AreEqual(expectedButtons, GetAllPressedButtons(state));
        }
#endif

        private static IEnumerable<TestCaseData> ThumbStickVirtualButtonsIgnoreDeadZoneTestCases
        {
            get
            {
                yield return new TestCaseData(
                    Vector2.Zero, Vector2.Zero, GamePadDeadZone.Circular,
                    (Buttons)0);

                yield return new TestCaseData(
                    new Vector2(1f, 0.1f), new Vector2(0.1f, 1f), GamePadDeadZone.Circular,
                    Buttons.LeftThumbstickRight | Buttons.RightThumbstickUp);

                yield return new TestCaseData(
                    new Vector2(1f, 0.1f), new Vector2(0.1f, 1f), GamePadDeadZone.None,
                    Buttons.LeftThumbstickRight | Buttons.RightThumbstickUp);

                yield return new TestCaseData(
                    new Vector2(1f, 0.1f), new Vector2(0.1f, 1f), GamePadDeadZone.IndependentAxes,
                    Buttons.LeftThumbstickRight | Buttons.RightThumbstickUp);

                yield return new TestCaseData(
                    new Vector2(0.5f, -0.5f), new Vector2(-0.5f, 0.5f), GamePadDeadZone.Circular,
                    Buttons.LeftThumbstickRight | Buttons.LeftThumbstickDown | Buttons.RightThumbstickLeft | Buttons.RightThumbstickUp);

                yield return new TestCaseData(
                    new Vector2(0.1f, -0.1f), new Vector2(-0.1f, 0.1f), GamePadDeadZone.None,
                    (Buttons)0);

                yield return new TestCaseData(
                    new Vector2(0.4f, -0.4f), new Vector2(-0.4f, 0.4f), GamePadDeadZone.IndependentAxes,
                    Buttons.LeftThumbstickRight | Buttons.LeftThumbstickDown | Buttons.RightThumbstickLeft | Buttons.RightThumbstickUp);

                yield return new TestCaseData(
                    new Vector2(0.4f, 0f), new Vector2(0f, 0.4f), GamePadDeadZone.Circular,
                    Buttons.LeftThumbstickRight | Buttons.RightThumbstickUp);
            }
        }

        private static Buttons GetAllPressedButtons(GamePadState state)
        {
            Buttons buttons = 0;
            foreach (var button in Enum.GetValues(typeof(Buttons)).Cast<Buttons>().Where(state.IsButtonDown))
                buttons |= button;
            return buttons;
        }

#if !XNA

        [Test]
        public void ThumbsticksCircularDeadZoneClamping()
        {
            var left = Vector2.One;
            var right = Vector2.One;

            var sticks = new GamePadThumbSticks(left, right, GamePadDeadZone.None, GamePadDeadZone.None);
            Assert.AreEqual(sticks.Left.X, 1);
            Assert.AreEqual(sticks.Left.Y, 1);
            Assert.AreEqual(sticks.Right.X, 1);
            Assert.AreEqual(sticks.Right.Y, 1);

            sticks = new GamePadThumbSticks(left, right, GamePadDeadZone.IndependentAxes, GamePadDeadZone.IndependentAxes);
            Assert.AreEqual(sticks.Left.X, 1);
            Assert.AreEqual(sticks.Left.Y, 1);
            Assert.AreEqual(sticks.Right.X, 1);
            Assert.AreEqual(sticks.Right.Y, 1);

            sticks = new GamePadThumbSticks(left, right, GamePadDeadZone.Circular, GamePadDeadZone.Circular);
            Assert.Less(sticks.Left.X, 1);
            Assert.Less(sticks.Left.Y, 1);
            Assert.Less(sticks.Right.X, 1);
            Assert.Less(sticks.Right.Y, 1);
        }

#endif

        [TestCaseSource("PublicConstructorClampsValuesTestCases")]
        public void PublicConstructorClampsValues(Vector2 left, Vector2 right, Vector2 expectedLeft, Vector2 expectedRight)
        {
            var gamePadThumbSticks = new GamePadThumbSticks(left, right);
            Assert.That(gamePadThumbSticks.Left, Is.EqualTo(expectedLeft).Using(Vector2Comparer.Epsilon));
            Assert.That(gamePadThumbSticks.Right, Is.EqualTo(expectedRight).Using(Vector2Comparer.Epsilon));
        }

        private static IEnumerable<TestCaseData> PublicConstructorClampsValuesTestCases
        {
            get
            {
                yield return new TestCaseData(
                    Vector2.Zero, Vector2.Zero,
                    Vector2.Zero, Vector2.Zero);

                yield return new TestCaseData(
                    Vector2.One, Vector2.One,
                    Vector2.One, Vector2.One);

                yield return new TestCaseData(
                    -Vector2.One, -Vector2.One,
                    -Vector2.One, -Vector2.One);

                yield return new TestCaseData(
                    new Vector2(-2.7f, 5.6f), new Vector2(3.5f, -4.8f),
                    new Vector2(-1f, 1f), new Vector2(1f, -1f));

                yield return new TestCaseData(
                    new Vector2(34f, 65f), new Vector2(-33f, -17f),
                    new Vector2(1f, 1f), new Vector2(-1f, -1f));
            }
        }

        #endregion

        #region State

        [TestCase((Buttons)0, new[] { ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released })]
        [TestCase(Buttons.DPadDown, new[] { ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Released })]
        [TestCase(Buttons.DPadLeft, new[] { ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Released })]
        [TestCase(Buttons.DPadRight, new[] { ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Released })]
        [TestCase(Buttons.DPadUp, new[] { ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Pressed })]
        public void ConstructDPadState(Buttons button, ButtonState[] expectedDPadButtonStates)
        {
            var state = new GamePadState(Vector2.Zero, Vector2.Zero, 0f, 0f, button != 0 ? button : new Buttons());

            if (button != 0)
                Assert.True(state.IsButtonDown(button));

            Assert.AreEqual(expectedDPadButtonStates[0], state.DPad.Down, "DPad.Down Pressed or Released");
            Assert.AreEqual(expectedDPadButtonStates[1], state.DPad.Left, "DPad.Left Pressed or Released");
            Assert.AreEqual(expectedDPadButtonStates[2], state.DPad.Right, "DPad.Right Pressed or Released");
            Assert.AreEqual(expectedDPadButtonStates[3], state.DPad.Up, "DPad.Up Pressed or Released");
        }


        [TestCase((Buttons)0, new[] { ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released })]
        [TestCase(Buttons.DPadDown, new[] { ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Released })]
        [TestCase(Buttons.DPadLeft, new[] { ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Released })]
        [TestCase(Buttons.DPadRight, new[] { ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Released })]
        [TestCase(Buttons.DPadUp, new[] { ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Pressed })]
        public void ConstructDPadStateWithButtonsArray(Buttons button, ButtonState[] expectedDPadButtonStates)
        {
            var state = new GamePadState(Vector2.Zero, Vector2.Zero, 0f, 0f, button != 0 ? new Buttons[] {button} : new Buttons[] {});

            if (button != 0)
                Assert.True(state.IsButtonDown(button));

            Assert.AreEqual(expectedDPadButtonStates[0], state.DPad.Down, "DPad.Down Pressed or Released");
            Assert.AreEqual(expectedDPadButtonStates[1], state.DPad.Left, "DPad.Left Pressed or Released");
            Assert.AreEqual(expectedDPadButtonStates[2], state.DPad.Right, "DPad.Right Pressed or Released");
            Assert.AreEqual(expectedDPadButtonStates[3], state.DPad.Up, "DPad.Up Pressed or Released");
        }

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
#if !XNA
            state.IsConnected = isConnected;
            Assert.AreEqual(isConnected, state.IsConnected);
#endif

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

#if !XNA
            var gamePadButtons = state.Buttons;
            Assert.AreEqual(joinedButtons, gamePadButtons._buttons);
#endif

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
