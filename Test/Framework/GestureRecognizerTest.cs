using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    /// <summary>
    /// Tests the gesture recognition of the TouchPanelState class. (This will be split out in to another class in the future)
    /// </summary>
    [TestFixture]
    class GestureRecognizerTest
    {
        private TouchPanelState _tps;

        private const GestureType AllDrags = GestureType.DragComplete | GestureType.FreeDrag | GestureType.HorizontalDrag | GestureType.VerticalDrag;
        private const GestureType AllGestures =
            GestureType.DoubleTap | GestureType.DragComplete | GestureType.Flick | GestureType.FreeDrag | GestureType.Hold |
            GestureType.HorizontalDrag | GestureType.Pinch | GestureType.PinchComplete | GestureType.Tap | GestureType.VerticalDrag;

        private GameTime GameTimeForFrame(int frameNo)
        {
            return new GameTime(TimeSpan.FromSeconds(frameNo / 60D), TimeSpan.FromSeconds(1 / 60D));
        }



        [SetUp]
        public void SetUp()
        {
            _tps = new TouchPanelState(new MockWindow());
        }

        [Test]
        public void DoingNothingMakesNoGestures()
        {
            _tps.EnabledGestures = AllGestures;

            _tps.Update(GameTimeForFrame(1));

            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        public void BasicTapGesture()
        {
            _tps.EnabledGestures = GestureType.Tap;
            var pos = new Vector2(100, 150);

            _tps.AddEvent(1, TouchLocationState.Pressed, pos);
            _tps.Update(GameTimeForFrame(1));

            Assert.False(_tps.IsGestureAvailable);

            _tps.AddEvent(1, TouchLocationState.Released, pos);
            _tps.Update(GameTimeForFrame(2));

            Assert.True(_tps.IsGestureAvailable);
            var gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);

            Assert.AreEqual(GestureType.Tap, gesture.GestureType);
            Assert.AreEqual(pos, gesture.Position);
        }

        [Test]
        [TestCase(true), TestCase(false)]
        public void BasicDoubleTapGesture(bool enableTap)
        {
            GestureSample gesture;

            _tps.EnabledGestures = GestureType.DoubleTap;
            if (enableTap)
                _tps.EnabledGestures |= GestureType.Tap;
            var pos = new Vector2(100, 150);

            //Do a first tap
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);
            _tps.Update(GameTimeForFrame(1));

            Assert.False(_tps.IsGestureAvailable);

            _tps.AddEvent(1, TouchLocationState.Released, pos);
            _tps.Update(GameTimeForFrame(2));

            //Will make a tap event if tap is enabled
            if (enableTap)
            {
                Assert.True(_tps.IsGestureAvailable);
                gesture = _tps.ReadGesture();
                Assert.False(_tps.IsGestureAvailable);

                Assert.AreEqual(GestureType.Tap, gesture.GestureType);
                Assert.AreEqual(pos, gesture.Position);
            }
            else
            {
                Assert.False(_tps.IsGestureAvailable);
            }

            //Now do the second tap in the same location, this will make a double tap on press (but no tap)
            _tps.AddEvent(2, TouchLocationState.Pressed, pos);
            _tps.Update(GameTimeForFrame(3));

            Assert.True(_tps.IsGestureAvailable);
            gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);

            Assert.AreEqual(GestureType.DoubleTap, gesture.GestureType);
            Assert.AreEqual(pos, gesture.Position);

            //This release should make no gestures
            _tps.AddEvent(2, TouchLocationState.Released, pos);
            _tps.Update(GameTimeForFrame(4));

            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        public void BasicHold()
        {
            _tps.EnabledGestures = GestureType.Hold;
            var pos = new Vector2(100, 150);

            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);

            //We shouldn't generate the hold until the required time has passed
            GameTime gt;
            int frame = 1;
            do
            {
                Assert.False(_tps.IsGestureAvailable);

                frame++;
                gt = GameTimeForFrame(frame);
                _tps.Update(gt);
            } while (gt.TotalGameTime < TouchPanelState.TimeRequiredForHold);

            //The last Update should have generated a hold
            Assert.True(_tps.IsGestureAvailable);

            var gesture = _tps.ReadGesture();
            Assert.AreEqual(GestureType.Hold, gesture.GestureType);
            Assert.AreEqual(pos, gesture.Position);

            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        [TestCase(AllDrags, GestureType.HorizontalDrag), TestCase(GestureType.HorizontalDrag, GestureType.HorizontalDrag)]
        [TestCase(AllDrags, GestureType.VerticalDrag), TestCase(GestureType.VerticalDrag, GestureType.VerticalDrag)]
        public void BasicDirectionalDrag(GestureType enabledGestures, GestureType direction)
        {
            _tps.EnabledGestures = enabledGestures;
            var startPos = new Vector2(200, 200);
            Vector2 diffVec;

            if (direction == GestureType.HorizontalDrag)
                diffVec = new Vector2(10, -1);
            else //Vertical
                diffVec = new Vector2(1, -10);

            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, startPos);
            _tps.Update(GameTimeForFrame(1));

            //Move it until it should have made a drag
            int diff = 0;
            int frame = 1;
            while (diff * 10 < TouchPanelState.TapJitterTolerance)
            {
                Assert.False(_tps.IsGestureAvailable);

                diff ++;
                frame++;

                _tps.AddEvent(1, TouchLocationState.Moved, startPos + diff * diffVec);
                _tps.Update(GameTimeForFrame(frame));
            }

            //We should have a gesture now
            Assert.True(_tps.IsGestureAvailable);
            var gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);

            //Should get the correct type at the new touch location, with the given delta
            Assert.AreEqual(direction, gesture.GestureType);
            Assert.AreEqual(startPos + diff * diffVec, gesture.Position);

            //Delta has only movement in the direction of the drag
            if (direction == GestureType.HorizontalDrag)
                Assert.AreEqual(new Vector2(10, 0), gesture.Delta);
            else //Vertical
                Assert.AreEqual(new Vector2(0, -10), gesture.Delta);

            //If all gestures are enabled (DragComplete is enabled), releasing our touch will generate a DragComplete gesture
            frame++;
            _tps.AddEvent(1, TouchLocationState.Released, startPos + diff * diffVec);
            _tps.Update(GameTimeForFrame(frame));

            if (enabledGestures == AllDrags)
            {
                Assert.True(_tps.IsGestureAvailable);
                gesture = _tps.ReadGesture();
                Assert.False(_tps.IsGestureAvailable);

                Assert.AreEqual(GestureType.DragComplete, gesture.GestureType);
                Assert.AreEqual(Vector2.Zero, gesture.Position); //This is (0,0) in XNA too. It's weird though!
            }
            else
            {
                Assert.False(_tps.IsGestureAvailable);
            }
        }

        [Test]
        [TestCase(AllDrags), TestCase(GestureType.FreeDrag | GestureType.DragComplete), TestCase(GestureType.FreeDrag)]
        [Description("Drag on an angle, it should generate a FreeDrag event, not a directional one")]
        public void BasicFreeDragTest(GestureType enabledGestures)
        {
            _tps.EnabledGestures = enabledGestures;
            var startPos = new Vector2(200, 200);

            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, startPos);
            _tps.Update(GameTimeForFrame(1));

            //Move it until it should have made a drag
            int diff = 0;
            int frame = 1;
            while (new Vector2(diff).Length() < TouchPanelState.TapJitterTolerance)
            {
                Assert.False(_tps.IsGestureAvailable);

                diff += 5;
                frame++;

                _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(diff));
                _tps.Update(GameTimeForFrame(frame));
            }

            //We should have a gesture now
            Assert.True(_tps.IsGestureAvailable);
            var gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);

            //Should get the correct type at the new touch location, with the given delta
            Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);
            Assert.AreEqual(startPos + new Vector2(diff), gesture.Position);

            Assert.AreEqual(new Vector2(5), gesture.Delta);

            //If DragComplete is enabled, releasing our touch will generate a DragComplete gesture
            frame++;
            _tps.AddEvent(1, TouchLocationState.Released, startPos + new Vector2(diff));
            _tps.Update(GameTimeForFrame(frame));

            if ((enabledGestures & GestureType.DragComplete) == GestureType.DragComplete)
            {
                Assert.True(_tps.IsGestureAvailable);
                gesture = _tps.ReadGesture();
                Assert.False(_tps.IsGestureAvailable);

                Assert.AreEqual(GestureType.DragComplete, gesture.GestureType);
                Assert.AreEqual(Vector2.Zero, gesture.Position); //This is (0,0) in XNA too. It's weird though!
            }
            else
            {
                Assert.False(_tps.IsGestureAvailable);
            }
        }

        [Test]
        [Description("If the user does a horizontal drag, this should be picked up as a free drag instead if horizontal is not enabled")]
        public void FreeDragIfDirectionalDisabled()
        {
            _tps.EnabledGestures = GestureType.FreeDrag | GestureType.VerticalDrag;
            var startPos = new Vector2(200, 200);

            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, startPos);
            _tps.Update(GameTimeForFrame(1));

            //Move it until it should have made a drag
            int diff = 0;
            int frame = 1;
            while (new Vector2(diff, 0).Length() < TouchPanelState.TapJitterTolerance)
            {
                Assert.False(_tps.IsGestureAvailable);

                diff += 5;
                frame++;

                _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(diff, 0));
                _tps.Update(GameTimeForFrame(frame));
            }

            //We should have a gesture now
            Assert.True(_tps.IsGestureAvailable);
            var gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);

            //Should get the correct type at the new touch location, with the given delta
            Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);
            Assert.AreEqual(startPos + new Vector2(diff, 0), gesture.Position);

            Assert.AreEqual(new Vector2(5, 0), gesture.Delta);
        }
    }
}
