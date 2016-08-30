// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using NUnit.Framework;

namespace MonoGame.Tests.Input
{
    class KeyboardTest
    {

        [TestCase(new[] { Keys.Up, Keys.A, Keys.Left, Keys.Oem8, Keys.Apps })]
        public void CtorParams(Keys[] keys)
        {
            var state = new KeyboardState(keys);
            CollectionAssert.AreEquivalent(state.GetPressedKeys(), keys);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                var keyDown = keys.Contains(key);
                Assert.AreEqual(keyDown ? KeyState.Down : KeyState.Up, state[key]);
                Assert.AreEqual(keyDown, state.IsKeyDown(key));
                Assert.AreEqual(!keyDown, state.IsKeyUp(key));
            }
        }

#if !XNA
        [TestCase(new[] { Keys.Up, Keys.A, Keys.Left, Keys.Oem8, Keys.Apps }, true, false)]
        [TestCase(new[] { Keys.Right, Keys.Down, Keys.LeftAlt, Keys.LeftShift }, true, true)]
        [TestCase(new[] { Keys.Delete, Keys.U, Keys.RightWindows, Keys.L, Keys.NumPad2 }, false, false)]
        [TestCase(new[] { Keys.F9, Keys.F12, Keys.VolumeUp, Keys.OemAuto, Keys.NumPad3 }, false, false)]
        [TestCase(new[] { Keys.OemMinus, Keys.OemTilde, Keys.Tab, Keys.Zoom }, true, false)]
        public void TestState(Keys[] keys, bool capsLock, bool numLock)
        {
            var keyList = keys.ToList();
            var state = new KeyboardState(keys, capsLock, numLock);

            CollectionAssert.AreEquivalent(state.GetPressedKeys(), keys);
            Assert.AreEqual(state.CapsLock, capsLock);
            Assert.AreEqual(state.NumLock, numLock);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                var keyDown = keyList.Contains(key);
                Assert.AreEqual(state.IsKeyDown(key), keyDown);
                Assert.AreEqual(state.IsKeyUp(key), !keyDown);
            }
        }

#endif

        [Test]
        public void TestGetState()
        {
            Keyboard.GetState();
        }
    }
}
