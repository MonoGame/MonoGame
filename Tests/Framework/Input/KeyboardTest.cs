// MonoGame - Copyright (C) MonoGame Foundation, Inc
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

        [TestCase(new[] { Keys.Up, Keys.A, Keys.Left, Keys.Oem8, Keys.Apps })]
        public void TestGetPressedKeysGarbageless(Keys[] keys)
        {
            var state = new KeyboardState(keys);

            int count = state.GetPressedKeyCount();
            Assert.AreEqual(keys.Length, count);

            Keys[] newKeysArray = new Keys[count];

            state.GetPressedKeys(newKeysArray);

            CollectionAssert.AreEquivalent(keys, newKeysArray);
        }

        [Test]
        public void TestBitwiseOperations()
        {
            var azState = new KeyboardState(Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z);

            var numState = new KeyboardState(Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9);

            var randState = new KeyboardState(Keys.A, Keys.D0, Keys.F1);

            var orState = azState | numState;
            Assert.AreEqual(true, orState.IsKeyDown(Keys.A));
            Assert.AreEqual(true, orState.IsKeyDown(Keys.Z));
            Assert.AreEqual(true, orState.IsKeyDown(Keys.D0));
            Assert.AreEqual(true, orState.IsKeyDown(Keys.D9));
            Assert.AreEqual(false, orState.IsKeyDown(Keys.F1));

            var invState = ~orState;
            Assert.AreEqual(false, invState.IsKeyDown(Keys.A));
            Assert.AreEqual(false, invState.IsKeyDown(Keys.Z));
            Assert.AreEqual(false, invState.IsKeyDown(Keys.D0));
            Assert.AreEqual(false, invState.IsKeyDown(Keys.D9));
            Assert.AreEqual(true, invState.IsKeyDown(Keys.F1));

            var andState = invState & randState;
            Assert.AreEqual(false, andState.AnyKeyDown(azState));
            Assert.AreEqual(false, andState.AnyKeyDown(numState));
            Assert.AreEqual(true, andState.AnyKeyDown(randState));

            var emptyState = andState & ~andState;
            Assert.AreEqual(emptyState, KeyboardState.Empty);

            var aState = new KeyboardState(Keys.A);
            var shfitAState = new KeyboardState(Keys.LeftShift, Keys.A);
            var bState = new KeyboardState(Keys.B);
            var shiftBState = new KeyboardState(Keys.LeftShift, Keys.B);
            var shiftState = new KeyboardState(Keys.LeftShift);
            var ctrlCState = new KeyboardState(Keys.LeftControl, Keys.C);

            Assert.AreEqual(true, aState.AnyKeyDown());
            Assert.AreEqual(false, shiftState.AnyKeyDown());
            Assert.AreEqual(true, shfitAState.AnyKeyDown());

            Assert.AreEqual(true, aState.AnyKeyOrModifierDown());
            Assert.AreEqual(true, shiftState.AnyKeyOrModifierDown());
            Assert.AreEqual(true, shfitAState.AnyKeyOrModifierDown());

            // when keys == [A]
            //   A =         true
            //   Shift + A = true
            //   Shift =     false
            //   B =         false
            //   Shift + B = false
            // when keys == [Shift]
            //   Shift     = false
            Assert.AreEqual(true, aState.AnyKeyDown(aState));
            Assert.AreEqual(true, shfitAState.AnyKeyDown(aState));
            Assert.AreEqual(false, shiftState.AnyKeyDown(aState));
            Assert.AreEqual(false, bState.AnyKeyDown(aState));
            Assert.AreEqual(false, shiftBState.AnyKeyDown(aState));
            Assert.AreEqual(false, ctrlCState.AnyKeyDown(aState));
            Assert.AreEqual(false, shiftState.AnyKeyDown(shiftState));

            // when keys == [A]
            //   A =         true
            //   Shift + A = true
            //   Shift =     false
            //   B =         false
            //   Shift + B = false
            // when keys == [Shift]
            //   Shift     = true
            Assert.AreEqual(true, aState.AnyKeyOrModifierDown(aState));
            Assert.AreEqual(true, shfitAState.AnyKeyOrModifierDown(aState));
            Assert.AreEqual(false, shiftState.AnyKeyOrModifierDown(aState));
            Assert.AreEqual(false, bState.AnyKeyOrModifierDown(aState));
            Assert.AreEqual(false, shiftBState.AnyKeyOrModifierDown(aState));
            Assert.AreEqual(false, ctrlCState.AnyKeyOrModifierDown(aState));
            Assert.AreEqual(true, shiftState.AnyKeyOrModifierDown(shiftState));

            // when keys == [A]
            //   A =         false
            //   Shift + A = false
            //   Shift =     false
            //   B =         true
            //   Shift + B = true
            Assert.AreEqual(false, aState.AnyKeyDownExcept(aState));
            Assert.AreEqual(false, shfitAState.AnyKeyDownExcept(aState));
            Assert.AreEqual(false, shiftState.AnyKeyDownExcept(aState));
            Assert.AreEqual(true, bState.AnyKeyDownExcept(aState));
            Assert.AreEqual(true, shiftBState.AnyKeyDownExcept(aState));
            Assert.AreEqual(true, ctrlCState.AnyKeyDownExcept(aState));

            // when keys == [A]
            //   A =         false
            //   Shift + A = true
            //   Shift =     true
            //   B =         true
            //   Shift + B = true
            Assert.AreEqual(false, aState.AnyKeyOrModifierDownExcept(aState));
            Assert.AreEqual(true, shfitAState.AnyKeyOrModifierDownExcept(aState));
            Assert.AreEqual(true, shiftState.AnyKeyOrModifierDownExcept(aState));
            Assert.AreEqual(true, bState.AnyKeyOrModifierDownExcept(aState));
            Assert.AreEqual(true, shiftBState.AnyKeyOrModifierDownExcept(aState));
            Assert.AreEqual(true, ctrlCState.AnyKeyOrModifierDownExcept(aState));
        }
    }
}
