using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
// TODO: Mac implements its own GameWindow class that cannot 
// be overloaded in MockWindow...  if you hate this hack, go fix it.
#if !MONOMAC

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

        [Test]
        [Description("Do multiple touches and check they behave as expected")]
        public void SimpleMultiTouchTest()
        {
            //Start with one touch
            var pos = new Vector2(100, 50);
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);

            var state = _tps.GetState();
            Assert.AreEqual(1, state.Count);

            var initialTouch1 = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, initialTouch1.State);
            Assert.AreEqual(pos, initialTouch1.Position);


            //Start a second touch
            var pos2 = new Vector2(150, 100);
            _tps.AddEvent(2, TouchLocationState.Pressed, pos2);

            state = _tps.GetState();
            Assert.AreEqual(2, state.Count);

            //First touch should now be moved, same location
            var touch1 = state.First(x => x.Id == initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Moved, touch1.State);
            Assert.AreEqual(pos, touch1.Position);

            //Second touch should be pressed in its position
            var initialTouch2 = state.First(x => x.Id != initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Pressed, initialTouch2.State);
            Assert.AreEqual(pos2, initialTouch2.Position);


            //Move the second touch
            var pos3 = new Vector2(150, 150);
            _tps.AddEvent(2, TouchLocationState.Moved, pos3);
            
            state = _tps.GetState();
            Assert.AreEqual(2, state.Count);

            //touch1 should be the same
            touch1 = state.First(x => x.Id == initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Moved, touch1.State);
            Assert.AreEqual(pos, touch1.Position);

            //touch2 should be moved in its new location
            var touch2 = state.First(x => x.Id == initialTouch2.Id);
            Assert.AreEqual(TouchLocationState.Moved, touch2.State);
            Assert.AreEqual(pos3, touch2.Position);


            //Release the second touch
            var pos4 = new Vector2(150, 200);
            _tps.AddEvent(2, TouchLocationState.Released, pos4);

            state = _tps.GetState();
            Assert.AreEqual(2, state.Count);

            //touch1 should be the same
            touch1 = state.First(x => x.Id == initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Moved, touch1.State);
            Assert.AreEqual(pos, touch1.Position);

            //touch2 should be released in its new location
            touch2 = state.First(x => x.Id == initialTouch2.Id);
            Assert.AreEqual(TouchLocationState.Released, touch2.State);
            Assert.AreEqual(pos4, touch2.Position);


            //Move the first touch, second touch shouldn't be there any more
            var pos5 = new Vector2(100, 200);
            _tps.AddEvent(1, TouchLocationState.Moved, pos5);

            state = _tps.GetState();
            Assert.AreEqual(1, state.Count);

            //touch1 should be moved to the new position
            touch1 = state.First(x => x.Id == initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Moved, touch1.State);
            Assert.AreEqual(pos5, touch1.Position);

            //No more touch2


            //Release the first touch
            var pos6 = new Vector2(100, 250);
            _tps.AddEvent(1, TouchLocationState.Released, pos6);

            state = _tps.GetState();
            Assert.AreEqual(1, state.Count);

            //touch1 should be released at the new position
            touch1 = state.First(x => x.Id == initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Released, touch1.State);
            Assert.AreEqual(pos6, touch1.Position);


            //Now we should have no touches
            state = _tps.GetState();
            Assert.AreEqual(0, state.Count);
        }

        [Test]
        [Description("Internally TouchPanelState uses a queue of TouchLocation updating state. If it gets longer than 100, it throws away the first ones, which is bad")]
        public void TooManyEventsLosesOldOnes()
        {
            //To test this, we will start a touch, read the state.
            //Then release the touch, start a new touch and move it around lots
            //Then read the state, the first touch should be released

            //Start a touch
            var pos = new Vector2(1);
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);

            var state = _tps.GetState();
            Assert.AreEqual(1, state.Count);

            var initialTouch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, initialTouch.State);
            Assert.AreEqual(pos, initialTouch.Position);


            //Release the touch, make a new one and move it around lots
            _tps.AddEvent(1, TouchLocationState.Released, pos);

            _tps.AddEvent(2, TouchLocationState.Pressed, new Vector2(2));
            for (var i = 3; i < 200; i++)
                _tps.AddEvent(2, TouchLocationState.Moved, new Vector2(i));

            //We should now have the first touch in the release state and the second touch in the pressed state at 199,199
            state = _tps.GetState();
            Assert.AreEqual(2, state.Count);

            var newInitialTouch = state.First(x => x.Id == initialTouch.Id);
            Assert.AreEqual(TouchLocationState.Released, newInitialTouch.State);
            Assert.AreEqual(pos, newInitialTouch.Position);

            var secondTouch = state.First(x => x.Id != initialTouch.Id);
            Assert.AreEqual(TouchLocationState.Pressed, secondTouch.State);
            Assert.AreEqual(new Vector2(199), secondTouch.Position);
        }

        [Test]
        [TestCase(false), TestCase(true)]
        public void ReleaseAllTouchesTest(bool testBetween)
        {
            //Create multiple touches in different states

            //Start a touch
            var pos = new Vector2(1);
            var pos2 = new Vector2(2);
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);

            var state = _tps.GetState();
            Assert.AreEqual(1, state.Count);

            var initialTouch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, initialTouch.State);
            Assert.AreEqual(pos, initialTouch.Position);

            _tps.AddEvent(2, TouchLocationState.Pressed, pos2);

            if (testBetween)
            {
                state = _tps.GetState();
                Assert.AreEqual(2, state.Count);
                var touch = state.First(x => x.Id == initialTouch.Id);
                var touch2 = state.First(x => x.Id != initialTouch.Id);

                Assert.AreEqual(TouchLocationState.Moved, touch.State);
                Assert.AreEqual(TouchLocationState.Pressed, touch2.State);
            }

            //Call ReleaseAllTouches
            _tps.ReleaseAllTouches();

            //All should be in Released state
            state = _tps.GetState();
            foreach (var touch in state)
                Assert.AreEqual(TouchLocationState.Released, touch.State);

            //If we saw the second touch happen then we should see it be released, otherwise we should only know of the first touch
            Assert.AreEqual(testBetween ? 2 : 1, state.Count);

            //Then it should be empty
            state = _tps.GetState();
            Assert.AreEqual(0, state.Count);
        }
    }

#endif // !MONOMAC
}
