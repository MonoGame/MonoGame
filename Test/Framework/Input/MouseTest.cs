using NUnit.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests
{
    class MouseTest
    {
        [Test]
        [TestCase(0, 0, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released)]
        [TestCase(13, 22, 31, ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed, ButtonState.Released, ButtonState.Pressed)]
        [TestCase(476, 585, 694, ButtonState.Released, ButtonState.Pressed, ButtonState.Pressed, ButtonState.Released, ButtonState.Released)]
        [TestCase(749, 858, 967, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Released)]
        [TestCase(1, 2, 3, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Pressed, ButtonState.Pressed)]
        public void TestState(int x, int y, int scrollWheel, ButtonState leftButton, ButtonState middleButton, ButtonState rightButton, ButtonState xButton1, ButtonState xButton2)
        {
            var state = new MouseState(x, y, scrollWheel, leftButton, middleButton, rightButton, xButton1, xButton2);

            Assert.AreEqual(state.X, x);
            Assert.AreEqual(state.Y, y);
            Assert.AreEqual(state.Position, new Point(x, y));
            Assert.AreEqual(state.ScrollWheelValue, scrollWheel);
            Assert.AreEqual(state.LeftButton, leftButton);
            Assert.AreEqual(state.MiddleButton, middleButton);
            Assert.AreEqual(state.RightButton, rightButton);
            Assert.AreEqual(state.XButton1, xButton1);
            Assert.AreEqual(state.XButton2, xButton2);
            Assert.AreEqual(state, new MouseState(x, y, scrollWheel, leftButton, middleButton, rightButton, xButton1, xButton2));
        }

        [Test]
        public void TestGetState()
        {
            Mouse.GetState();
        }
    }
}

