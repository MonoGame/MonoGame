using System;
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
    }
}
