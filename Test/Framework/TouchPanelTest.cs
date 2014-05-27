using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    internal class TouchPanelTest
    {
        [Test]
        public void InitiallyHasNoTouches()
        {
            var tps = new TouchPanelState(new MockWindow());

            var state = tps.GetState();

            Assert.AreEqual(0, state.Count);
            Assert.AreEqual(true, state.IsReadOnly);
        }

        [Test]
        public void PressedStartsATouch()
        {
            var tps = new TouchPanelState(new MockWindow());

            var pos = new Vector2(100, 50);
            tps.AddEvent(1, TouchLocationState.Pressed, pos);

            var state = tps.GetState();

            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, touch.State);
            Assert.AreEqual(pos, touch.Position);
        }

        [Test]
        [TestCase(TouchLocationState.Invalid)]
        [TestCase(TouchLocationState.Moved)]
        [TestCase(TouchLocationState.Released)]
        public void NonPressedDoesntStartATouch(TouchLocationState providedState)
        {
            var tps = new TouchPanelState(new MockWindow());

            tps.AddEvent(1, providedState, new Vector2(100, 50));

            var state = tps.GetState();

            Assert.AreEqual(0, state.Count);
        }

        [Test]
        public void PressedAgesToMovedAfterGetState()
        {
            var tps = new TouchPanelState(new MockWindow());

            var pos = new Vector2(100, 50);
            tps.AddEvent(1, TouchLocationState.Pressed, pos);

            tps.GetState(); //Throw the first one away
            var state = tps.GetState();

            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Moved, touch.State);
            Assert.AreEqual(pos, touch.Position);
        }

        [Test]
        public void MovingTouchUpdatesPosition()
        {
            var tps = new TouchPanelState(new MockWindow());

            var pos1 = new Vector2(100, 50);
            tps.AddEvent(1, TouchLocationState.Pressed, pos1);

            var state = tps.GetState();
            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, touch.State);
            Assert.AreEqual(pos1, touch.Position);

            var pos2 = new Vector2(100, 50);
            tps.AddEvent(1, TouchLocationState.Moved, pos2);

            state = tps.GetState();
            Assert.AreEqual(1, state.Count);

            touch = state[0];
            Assert.AreEqual(TouchLocationState.Moved, touch.State);
            Assert.AreEqual(pos2, touch.Position); //Location should be updated
        }

        [Test]
        public void TouchBetweenGetStateCallsMakesNoTouch()
        {
            var tps = new TouchPanelState(new MockWindow());

            var state = tps.GetState();
            Assert.AreEqual(0, state.Count); //No touches to start with

            var pos = new Vector2(100, 50);
            tps.AddEvent(1, TouchLocationState.Pressed, pos);
            tps.AddEvent(1, TouchLocationState.Released, pos);

            state = tps.GetState();
            Assert.AreEqual(0, state.Count); //Should miss the touch that happened between
        }

        private class MockWindow : GameWindow
        {
            public override bool AllowUserResizing { get; set; }

            public override Rectangle ClientBounds
            {
                get { throw new NotImplementedException(); }
            }

            public override Point Position { get; set; }

            public override DisplayOrientation CurrentOrientation
            {
                get { throw new NotImplementedException(); }
            }

            public override IntPtr Handle
            {
                get { throw new NotImplementedException(); }
            }

            public override string ScreenDeviceName
            {
                get { throw new NotImplementedException(); }
            }

            public override void BeginScreenDeviceChange(bool willBeFullScreen)
            {
                throw new NotImplementedException();
            }

            public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
            {
                throw new NotImplementedException();
            }

            protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
            {
                throw new NotImplementedException();
            }

            protected override void SetTitle(string title)
            {
                throw new NotImplementedException();
            }
        }

    }
}
