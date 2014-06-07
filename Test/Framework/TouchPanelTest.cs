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
        private TouchPanelState _tps;

        [SetUp]
        public void SetUp()
        {
            _tps = new TouchPanelState(new MockWindow());
        }

        [Test]
        public void InitiallyHasNoTouches()
        {
            var state = _tps.GetState();

            Assert.AreEqual(0, state.Count);
            Assert.AreEqual(true, state.IsReadOnly);
        }

        [Test]
        public void PressedStartsATouch()
        {
            var pos = new Vector2(100, 50);
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);

            var state = _tps.GetState();

            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, touch.State);
            Assert.AreEqual(pos, touch.Position);
        }

        [Test]
        [Description("In XNA if you press then move your finger before GetState is called, the initial touch position is the latest position of the touch")]
        public void PressedThenMoveStartsATouchAtNewPosition()
        {
            var pos = new Vector2(100, 50);
            var pos2 = new Vector2(100, 100);
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);
            _tps.AddEvent(1, TouchLocationState.Moved, pos2);

            var state = _tps.GetState();

            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, touch.State);
            Assert.AreEqual(pos2, touch.Position);
        }

        [Test]
        [TestCase(TouchLocationState.Invalid)]
        [TestCase(TouchLocationState.Moved)]
        [TestCase(TouchLocationState.Released)]
        public void NonPressedDoesntStartATouch(TouchLocationState providedState)
        {
            _tps.AddEvent(1, providedState, new Vector2(100, 50));

            var state = _tps.GetState();

            Assert.AreEqual(0, state.Count);
        }

        [Test]
        public void PressedAgesToMovedAfterGetState()
        {
            var pos = new Vector2(100, 50);
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);

            var initialState = _tps.GetState();
            var initialTouch = initialState[0];

            var state = _tps.GetState();

            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Moved, touch.State);
            Assert.AreEqual(pos, touch.Position);
            Assert.AreEqual(initialTouch.Id, touch.Id);
        }

        [Test]
        public void MovingTouchUpdatesPosition()
        {
            var pos1 = new Vector2(100, 50);
            _tps.AddEvent(1, TouchLocationState.Pressed, pos1);

            var state = _tps.GetState();
            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, touch.State);
            Assert.AreEqual(pos1, touch.Position);

            var pos2 = new Vector2(100, 50);
            _tps.AddEvent(1, TouchLocationState.Moved, pos2);

            state = _tps.GetState();
            Assert.AreEqual(1, state.Count);

            touch = state[0];
            Assert.AreEqual(TouchLocationState.Moved, touch.State);
            Assert.AreEqual(pos2, touch.Position); //Location should be updated
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ReleasingTouchMakesItGoAway(bool moveInBetween)
        {
            //Touch the screen, we should get one touch with the given location in the pressed state
            var pos = new Vector2(100, 50);
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);
            
            var state = _tps.GetState();
            Assert.AreEqual(1, state.Count);

            var initialTouch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, initialTouch.State);
            Assert.AreEqual(pos, initialTouch.Position);

            //Now optionally move the touch, should give the same touch in the new location in the moved state
            if (moveInBetween)
            {
                var pos2 = new Vector2(100, 100);
                _tps.AddEvent(1, TouchLocationState.Moved, pos2);

                var movedState = _tps.GetState();
                Assert.AreEqual(1, movedState.Count);

                var touch = movedState[0];
                Assert.AreEqual(TouchLocationState.Moved, touch.State);
                Assert.AreEqual(pos2, touch.Position);
                Assert.AreEqual(initialTouch.Id, touch.Id);
            }

            //Release the touch, it should then show up as released touch
            var pos3 = new Vector2(100, 150);
            _tps.AddEvent(1, TouchLocationState.Released, pos3);

            var endState = _tps.GetState();
            Assert.AreEqual(1, endState.Count);

            var endTouch = endState[0];
            Assert.AreEqual(TouchLocationState.Released, endTouch.State);
            Assert.AreEqual(pos3, endTouch.Position);
            Assert.AreEqual(initialTouch.Id, endTouch.Id);

            //Finally get the TouchState again, we should now have no touches
            var finalState = _tps.GetState();
            Assert.AreEqual(0, finalState.Count);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Description("In XNA if you press and release your finger before GetState is called, the touch never shows up")]
        public void TouchBetweenGetStateCallsMakesNoTouch(bool moveInBetween)
        {
            var pos = new Vector2(100, 50);
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);
            if (moveInBetween) //Moving shouldn't change the behavior
                _tps.AddEvent(1, TouchLocationState.Moved, pos);
            _tps.AddEvent(1, TouchLocationState.Released, pos);

            var state = _tps.GetState();
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
