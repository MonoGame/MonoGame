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
    }
}
