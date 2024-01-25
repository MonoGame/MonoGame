// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using NUnit.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests.Input
{
    class MouseTest
    {
        [Test]
        [TestCase(0, 0, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released)]
        [TestCase(13, 22, 31, ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed)]
        [TestCase(476, 585, 694, ButtonState.Released, ButtonState.Pressed, ButtonState.Pressed, ButtonState.Released, ButtonState.Released)]
        [TestCase(749, 858, 967, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Released)]
        [TestCase(1, 2, 3, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Pressed)]
        public void Ctor(int x, int y, int scrollWheel, ButtonState leftButton, ButtonState middleButton, ButtonState rightButton, ButtonState xButton1, ButtonState xButton2)
        {
            var state = new MouseState(x, y, scrollWheel, leftButton, middleButton, rightButton, xButton1, xButton2);

            Assert.AreEqual(state.X, x);
            Assert.AreEqual(state.Y, y);
#if !XNA
            Assert.AreEqual(state.Position, new Point(x, y));
#endif
            Assert.AreEqual(state.ScrollWheelValue, scrollWheel);
            Assert.AreEqual(state.LeftButton, leftButton);
            Assert.AreEqual(state.MiddleButton, middleButton);
            Assert.AreEqual(state.RightButton, rightButton);
            Assert.AreEqual(state.XButton1, xButton1);
            Assert.AreEqual(state.XButton2, xButton2);
#if !XNA
            Assert.AreEqual(state.HorizontalScrollWheelValue, 0);
#endif
        }

        [Test]
        [TestCase(0, 0, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, 0)]
        [TestCase(13, 22, 31, ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed, 32)]
        [TestCase(476, 585, 694, ButtonState.Released, ButtonState.Pressed, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, 695)]
        [TestCase(749, 858, 967, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Released, 968)]
        [TestCase(1, 2, 3, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Pressed, 4)]
        public void CtorWithHorizontalScroll(int x, int y, int scrollWheel, ButtonState leftButton, ButtonState middleButton, ButtonState rightButton, ButtonState xButton1, ButtonState xButton2, int horizontalScrollWheel)
        {
#if !XNA
            var state = new MouseState(x, y, scrollWheel, leftButton, middleButton, rightButton, xButton1, xButton2, horizontalScrollWheel);
#else
            var state = new MouseState(x, y, scrollWheel, leftButton, middleButton, rightButton, xButton1, xButton2);
#endif

            Assert.AreEqual(state.X, x);
            Assert.AreEqual(state.Y, y);
#if !XNA
            Assert.AreEqual(state.Position, new Point(x, y));
#endif
            Assert.AreEqual(state.ScrollWheelValue, scrollWheel);
            Assert.AreEqual(state.LeftButton, leftButton);
            Assert.AreEqual(state.MiddleButton, middleButton);
            Assert.AreEqual(state.RightButton, rightButton);
            Assert.AreEqual(state.XButton1, xButton1);
            Assert.AreEqual(state.XButton2, xButton2);
#if !XNA
            Assert.AreEqual(state.HorizontalScrollWheelValue, horizontalScrollWheel);
#endif
        }

#if !XNA
        [TestCase(0, 0, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, 0)]
        [TestCase(13, 22, 31, ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed, 32)]
        [TestCase(476, 585, 694, ButtonState.Released, ButtonState.Pressed, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, 695)]
        [TestCase(749, 858, 967, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Released, 968)]
        [TestCase(1, 2, 3, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Pressed, 4)]
        public void SetGet(int x, int y, int scrollWheel, ButtonState leftButton, ButtonState middleButton, ButtonState rightButton, ButtonState xButton1, ButtonState xButton2, int horizontalScrollWheel)
        {
            var state = new MouseState
            {
                X = x,
                Y = y,
                ScrollWheelValue = scrollWheel,
                LeftButton = leftButton,
                MiddleButton = middleButton,
                RightButton = rightButton,
                XButton1 = xButton1,
                XButton2 = xButton2,
                HorizontalScrollWheelValue = horizontalScrollWheel
            };
            Assert.AreEqual(state.X, x);
            Assert.AreEqual(state.Y, y);
            Assert.AreEqual(state.ScrollWheelValue, scrollWheel);
            Assert.AreEqual(state.LeftButton, leftButton);
            Assert.AreEqual(state.MiddleButton, middleButton);
            Assert.AreEqual(state.RightButton, rightButton);
            Assert.AreEqual(state.XButton1, xButton1);
            Assert.AreEqual(state.XButton2, xButton2);
            Assert.AreEqual(state.HorizontalScrollWheelValue, horizontalScrollWheel);

            var state2 = new MouseState(x, y, scrollWheel, leftButton, middleButton, rightButton, xButton1, xButton2, horizontalScrollWheel);
            Assert.AreEqual(state, state2);
            Assert.AreEqual(state.GetHashCode(), state2.GetHashCode());
        }
#endif

        [Test]
        public void TestGetState()
        {
            Mouse.GetState();
        }
    }
}

