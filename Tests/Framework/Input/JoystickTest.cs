// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Input;
using NUnit.Framework;

namespace MonoGame.Tests.Input
{
    public class JoystickTest
    {
        [TestCase(new [] { 12345, -12345 }, new [] { ButtonState.Pressed, ButtonState.Released }, true)]
        [TestCase(new [] { -7324, -32000 }, new [] { ButtonState.Pressed, ButtonState.Pressed }, false)]
        public void TestState(int[] axes, ButtonState[] buttons, bool isConnected)
        {
            var hats = new[]
            {
                new JoystickHat
                {
                    Left = ButtonState.Pressed,
                    Up = ButtonState.Released,
                    Right = ButtonState.Released,
                    Down = ButtonState.Pressed
                },
                new JoystickHat
                {
                    Left = ButtonState.Pressed,
                    Up = ButtonState.Pressed,
                    Right = ButtonState.Pressed,
                    Down = ButtonState.Released
                }
            };

            var state = new JoystickState
            {
                Axes = axes,
                Buttons = buttons,
                Hats = hats,
                IsConnected = isConnected
            };

            Assert.AreEqual(axes, state.Axes);
            Assert.AreEqual(buttons, state.Buttons);
            Assert.AreEqual(hats, state.Hats);
            Assert.AreEqual(isConnected, state.IsConnected);
        }

        [Test]
        public void JoyStickHatTest([Values(ButtonState.Pressed, ButtonState.Released)] ButtonState left, 
            [Values(ButtonState.Pressed, ButtonState.Released)] ButtonState right, 
            [Values(ButtonState.Pressed, ButtonState.Released)] ButtonState up, 
            [Values(ButtonState.Pressed, ButtonState.Released)] ButtonState down)
        {
            var hat = new JoystickHat
            {
                Left = left,
                Right = right,
                Up = up,
                Down = down,
            };

            Assert.AreEqual(left, hat.Left);
            Assert.AreEqual(right, hat.Right);
            Assert.AreEqual(up, hat.Up);
            Assert.AreEqual(down, hat.Down);
        }
    }
}