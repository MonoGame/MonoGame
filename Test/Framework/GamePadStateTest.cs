using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    class GamePadStateTest
    {
        /*
        [Test]
        [TestCase(0.0f, 0.0f)]
        [TestCase(1.0f, 0.0f)]
        [TestCase(0.0f, 1.0f)]
        [TestCase(0.5f, 0.0f)]
        [TestCase(0.0f, 0.5f)]
        [TestCase(0.121f, 0.0f)]
        [TestCase(0.0f, 0.121f)]
        [TestCase(0.122f, 0.0f)]
        [TestCase(0.0f, 0.122f)]
        [TestCase(-0.1f, 0.0f)]
        [TestCase(0.0f, -0.1f)]
        [TestCase(1.1f, 0.0f)]
        [TestCase(0.0f, 1.1f)]
        public void ConstructTriggers(float leftTrigger, float rightTrigger)
        {
            var state = new GamePadState(Vector2.Zero, Vector2.Zero, leftTrigger, rightTrigger);

            Assert.AreEqual(MathHelper.Clamp(leftTrigger, 0, 1), state.Triggers.Left);
            Assert.AreEqual(leftTrigger >= 0.122f, state.IsButtonDown(Buttons.LeftTrigger));
            Assert.AreEqual(MathHelper.Clamp(rightTrigger, 0, 1), state.Triggers.Right);
            Assert.AreEqual(rightTrigger >= 0.122f, state.IsButtonDown(Buttons.RightTrigger));
        }
        */

        [Test]
        [TestCase((Buttons)0, new[] { ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released })]
        [TestCase(Buttons.DPadDown, new[] { ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Released })]
        [TestCase(Buttons.DPadLeft, new[] { ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Released })]
        [TestCase(Buttons.DPadRight, new[] { ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Released })]
        [TestCase(Buttons.DPadUp, new[] { ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Pressed })]
        public void ConstructDPadState(Buttons button, ButtonState[] expectedDPadButtonStates)
        {
            var state = new GamePadState(Vector2.Zero, Vector2.Zero, 0f, 0f, button != 0 ? new Buttons[] { button } : new Buttons[] { });

            if (button != 0)
                Assert.True(state.IsButtonDown(button));

            Assert.AreEqual(expectedDPadButtonStates[0], state.DPad.Down, "DPad.Down Pressed or Released");
            Assert.AreEqual(expectedDPadButtonStates[1], state.DPad.Left, "DPad.Left Pressed or Released");
            Assert.AreEqual(expectedDPadButtonStates[2], state.DPad.Right, "DPad.Right Pressed or Released");
            Assert.AreEqual(expectedDPadButtonStates[3], state.DPad.Up, "DPad.Up Pressed or Released");
        }

#if !XNA
        [TestCaseSource("ThumbStickVirtualButtonsIgnoreDeadZoneTestCases")]
        public void ThumbStickVirtualButtonsIgnoreDeadZone(Vector2 left, Vector2 right, GamePadDeadZone deadZone, Buttons expectedButtons)
        {
            var state = new GamePadState(new GamePadThumbSticks(left, right, deadZone), new GamePadTriggers(), new GamePadButtons(), new GamePadDPad());

            Assert.AreEqual(expectedButtons, GetAllPressedButtons(state));
        }

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
            }
        }

        private static Buttons GetAllPressedButtons(GamePadState state)
        {
            Buttons buttons = 0;
            foreach (var button in Enum.GetValues(typeof(Buttons)).Cast<Buttons>().Where(state.IsButtonDown))
                buttons |= button;
            return buttons;
        }
#endif
    }
}
