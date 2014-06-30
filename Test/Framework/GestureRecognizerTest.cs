using System;
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
            TouchPanelState.Update(GameTimeForFrame(0));
            _tps = new TouchPanelState(new MockWindow());
        }

        [Test]
        public void DoingNothingMakesNoGestures()
        {
            _tps.EnabledGestures = AllGestures;

            TouchPanelState.Update(GameTimeForFrame(1));

            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        public void BasicTapGesture()
        {
            _tps.EnabledGestures = GestureType.Tap;
            var pos = new Vector2(100, 150);

            _tps.AddEvent(1, TouchLocationState.Pressed, pos);
            TouchPanelState.Update(GameTimeForFrame(1));

            Assert.False(_tps.IsGestureAvailable);

            _tps.AddEvent(1, TouchLocationState.Released, pos);
            TouchPanelState.Update(GameTimeForFrame(2));

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
            TouchPanelState.Update(GameTimeForFrame(1));

            Assert.False(_tps.IsGestureAvailable);

            _tps.AddEvent(1, TouchLocationState.Released, pos);
            TouchPanelState.Update(GameTimeForFrame(2));

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
            TouchPanelState.Update(GameTimeForFrame(3));

            Assert.True(_tps.IsGestureAvailable);
            gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);

            Assert.AreEqual(GestureType.DoubleTap, gesture.GestureType);
            Assert.AreEqual(pos, gesture.Position);

            //This release should make no gestures
            _tps.AddEvent(2, TouchLocationState.Released, pos);
            TouchPanelState.Update(GameTimeForFrame(4));

            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        [Description("Do 2 quick taps, but make the second tap not near the first. Should not make a double tap")]
        public void DoubleTapTooFar()
        {
            _tps.EnabledGestures = GestureType.DoubleTap;
            var pos1 = new Vector2(100, 150);
            var pos2 = new Vector2(100, 150 + TouchPanelState.TapJitterTolerance + 1);

            //Do a first tap
            _tps.AddEvent(1, TouchLocationState.Pressed, pos1);
            TouchPanelState.Update(GameTimeForFrame(1));

            Assert.False(_tps.IsGestureAvailable);

            _tps.AddEvent(1, TouchLocationState.Released, pos1);
            TouchPanelState.Update(GameTimeForFrame(2));

            //Now do the second tap in a different location
            _tps.AddEvent(2, TouchLocationState.Pressed, pos2);
            TouchPanelState.Update(GameTimeForFrame(3));

            //Shouldn't make a double tap
            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        [TestCase(GestureType.None), TestCase(GestureType.FreeDrag | GestureType.DragComplete)]
        [Description("Hold a finger down, then perform a tap and double tap with another, this should not make any tap gestures")]
        public void MultiFingerTap(GestureType otherEnabledGestures)
        {
            //TODO: This test is based on current behavior. We need to verify that XNA behaves the same
            //TODO: Need a test for how pinch and tap interact

            _tps.EnabledGestures = otherEnabledGestures | GestureType.Tap | GestureType.DoubleTap;
            var pos = new Vector2(100, 150);

            //Place a finger down, this finger will never be released
            _tps.AddEvent(1, TouchLocationState.Pressed, new Vector2(10));
            TouchPanelState.Update(GameTimeForFrame(1));
            Assert.False(_tps.IsGestureAvailable);

            //Place a new finger down for a tap
            _tps.AddEvent(2, TouchLocationState.Pressed, pos);
            TouchPanelState.Update(GameTimeForFrame(2));

            Assert.False(_tps.IsGestureAvailable);

            //Release it, should not make a tap
            _tps.AddEvent(2, TouchLocationState.Released, pos);
            TouchPanelState.Update(GameTimeForFrame(2));

            Assert.False(_tps.IsGestureAvailable);

            //Press the finger down again, should not make a double tap
            _tps.AddEvent(3, TouchLocationState.Pressed, pos);
            TouchPanelState.Update(GameTimeForFrame(1));

            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        [Description("Do 2 taps with a long time between. Should not make a double tap")]
        public void DoubleTapTooSlow()
        {
            _tps.EnabledGestures = GestureType.DoubleTap;
            var pos = new Vector2(100, 150);

            //Do a first tap
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);
            TouchPanelState.Update(GameTimeForFrame(1));

            Assert.False(_tps.IsGestureAvailable);

            _tps.AddEvent(1, TouchLocationState.Released, pos);
            TouchPanelState.Update(GameTimeForFrame(2));

            //Now wait 500ms (we require it within 300ms)
            for (int frame = 3; frame < 33; frame++)
            {
                TouchPanelState.Update(GameTimeForFrame(frame));
                Assert.False(_tps.IsGestureAvailable);
            }

            _tps.AddEvent(2, TouchLocationState.Pressed, pos);
            TouchPanelState.Update(GameTimeForFrame(33));

            //Shouldn't make a double tap
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
                TouchPanelState.Update(gt);
            } while (gt.TotalGameTime < TouchPanelState.TimeRequiredForHold);

            //The last Update should have generated a hold
            Assert.True(_tps.IsGestureAvailable);

            var gesture = _tps.ReadGesture();
            Assert.AreEqual(GestureType.Hold, gesture.GestureType);
            Assert.AreEqual(pos, gesture.Position);

            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        [Description("Do a Tap, Double Tap, Hold using 2 taps. Should get gestures for each one")]
        public void TapDoubleTapHold()
        {
            _tps.EnabledGestures = GestureType.Tap | GestureType.DoubleTap | GestureType.Hold;

            var pos = new Vector2(100, 100);

            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, pos);
            TouchPanelState.Update(GameTimeForFrame(1));
            Assert.False(_tps.IsGestureAvailable);

            //Release it, should make a tap
            _tps.AddEvent(1, TouchLocationState.Released, pos);
            TouchPanelState.Update(GameTimeForFrame(2));
            Assert.True(_tps.IsGestureAvailable);

            var gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);
            Assert.AreEqual(GestureType.Tap, gesture.GestureType);

            //Place finger again, should make a double tap
            _tps.AddEvent(2, TouchLocationState.Pressed, pos);
            TouchPanelState.Update(GameTimeForFrame(3));
            Assert.True(_tps.IsGestureAvailable);

            gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);
            Assert.AreEqual(GestureType.DoubleTap, gesture.GestureType);


            //Now hold it for a while to make a hold gesture
            var alreadyPassedTime = GameTimeForFrame(2).TotalGameTime;
            GameTime gt;
            int frame = 4;
            do
            {
                Assert.False(_tps.IsGestureAvailable);

                frame++;
                gt = GameTimeForFrame(frame);
                TouchPanelState.Update(gt);
            } while (gt.TotalGameTime < (TouchPanelState.TimeRequiredForHold + alreadyPassedTime));
            
            //The last Update should have generated a hold
            Assert.True(_tps.IsGestureAvailable);
            gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);

            Assert.AreEqual(GestureType.Hold, gesture.GestureType);
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
            TouchPanelState.Update(GameTimeForFrame(1));

            //Move it until it should have made a drag
            int diff = 0;
            int frame = 1;
            while (diff * 10 < TouchPanelState.TapJitterTolerance)
            {
                Assert.False(_tps.IsGestureAvailable);

                diff ++;
                frame++;

                _tps.AddEvent(1, TouchLocationState.Moved, startPos + diff * diffVec);
                TouchPanelState.Update(GameTimeForFrame(frame));
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
            TouchPanelState.Update(GameTimeForFrame(frame));

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
            TouchPanelState.Update(GameTimeForFrame(1));

            //Move it until it should have made a drag
            int diff = 0;
            int frame = 1;
            while (new Vector2(diff).Length() < TouchPanelState.TapJitterTolerance)
            {
                Assert.False(_tps.IsGestureAvailable);

                diff += 5;
                frame++;

                _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(diff));
                TouchPanelState.Update(GameTimeForFrame(frame));
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
            TouchPanelState.Update(GameTimeForFrame(frame));

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
            TouchPanelState.Update(GameTimeForFrame(1));

            //Move it until it should have made a drag
            int diff = 0;
            int frame = 1;
            while (new Vector2(diff, 0).Length() < TouchPanelState.TapJitterTolerance)
            {
                Assert.False(_tps.IsGestureAvailable);

                diff += 5;
                frame++;

                _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(diff, 0));
                TouchPanelState.Update(GameTimeForFrame(frame));
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

        [Test]
        [Description("Start a drag then disable gestures. Gestures events should just stop, no DragComplete gesture should happen")]
        public void DisableGesturesWhileDragging()
        {
            _tps.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
            var startPos = new Vector2(200, 200);


            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, startPos);
            TouchPanelState.Update(GameTimeForFrame(1));
            Assert.False(_tps.IsGestureAvailable);

            //Drag it, should get a drag
            _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(40, 0));
            TouchPanelState.Update(GameTimeForFrame(2));

            Assert.True(_tps.IsGestureAvailable);
            var gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);
            Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);

            //Disable gestures
            _tps.EnabledGestures = GestureType.None;

            _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(80, 0));
            TouchPanelState.Update(GameTimeForFrame(3));

            Assert.False(_tps.IsGestureAvailable);

            //Release that touch, should make no gesture
            _tps.AddEvent(1, TouchLocationState.Released, startPos + new Vector2(80, 0));
            TouchPanelState.Update(GameTimeForFrame(4));
            Assert.False(_tps.IsGestureAvailable);


            //Enable both gestures again, just place the finger down and release it
            //Should make no gesture

            _tps.AddEvent(2, TouchLocationState.Pressed, startPos);
            TouchPanelState.Update(GameTimeForFrame(5));
            Assert.False(_tps.IsGestureAvailable);

            _tps.AddEvent(2, TouchLocationState.Released, startPos);
            TouchPanelState.Update(GameTimeForFrame(6));
            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        [Description("Start a drag then disable gestures. Re-Enable them without releasing the finger. Releasing it then should make a DragComplete")]
        public void DisableGesturesWhileDragging2()
        {
            _tps.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
            var startPos = new Vector2(200, 200);


            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, startPos);
            TouchPanelState.Update(GameTimeForFrame(1));
            Assert.False(_tps.IsGestureAvailable);

            //Drag it, should get a drag
            _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(40, 0));
            TouchPanelState.Update(GameTimeForFrame(2));

            Assert.True(_tps.IsGestureAvailable);
            var gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);
            Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);

            //Disable gestures
            _tps.EnabledGestures = GestureType.None;

            _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(80, 0));
            TouchPanelState.Update(GameTimeForFrame(3));

            Assert.False(_tps.IsGestureAvailable);

            //Enable both gestures again, release the finger
            _tps.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;

            //Release that touch, should make no gesture
            _tps.AddEvent(1, TouchLocationState.Released, startPos + new Vector2(80, 0));
            TouchPanelState.Update(GameTimeForFrame(4));

            Assert.True(_tps.IsGestureAvailable);
            gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);

            Assert.AreEqual(GestureType.DragComplete, gesture.GestureType);
        }

        [Test]
        [Description("Start a drag then disable gestures. Release finger and replace, then re-enable gestures and release. Should not make a DragComplete")]
        public void DisableGesturesWhileDragging3()
        {
            _tps.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
            var startPos = new Vector2(200, 200);

            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, startPos);
            TouchPanelState.Update(GameTimeForFrame(1));
            Assert.False(_tps.IsGestureAvailable);

            //Drag it, should get a drag
            _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(40, 0));
            TouchPanelState.Update(GameTimeForFrame(2));

            Assert.True(_tps.IsGestureAvailable);
            var gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);
            Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);

            //Disable gestures
            _tps.EnabledGestures = GestureType.None;

            _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(80, 0));
            TouchPanelState.Update(GameTimeForFrame(3));
            Assert.False(_tps.IsGestureAvailable);

            //Release the finger, should make no gesture (gestures are disabled)
            _tps.AddEvent(1, TouchLocationState.Released, startPos + new Vector2(80, 0));
            TouchPanelState.Update(GameTimeForFrame(4));
            Assert.False(_tps.IsGestureAvailable);

            //Press it down again
            _tps.AddEvent(2, TouchLocationState.Pressed, startPos + new Vector2(80, 0));
            TouchPanelState.Update(GameTimeForFrame(5));
            Assert.False(_tps.IsGestureAvailable);

            //Enable both gestures again
            _tps.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;

            //Release the second touch, should make no gesture
            _tps.AddEvent(2, TouchLocationState.Released, startPos + new Vector2(80, 0));
            TouchPanelState.Update(GameTimeForFrame(6));

            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        [Description("Enable the tap gesture while dragging with no gestures enabled. No gestures should happen")]
        public void EnableTapWhileDragging()
        {
            //Based on https://github.com/mono/MonoGame/pull/1543#issuecomment-15004057
            
            var pos = new Vector2(10, 10);

            _tps.AddEvent(1, TouchLocationState.Pressed, pos);
            TouchPanelState.Update(GameTimeForFrame(1));

            //Drag it a bit
                        _tps.AddEvent(1, TouchLocationState.Moved, pos + new Vector2(40, 0));
            TouchPanelState.Update(GameTimeForFrame(1));

            _tps.EnabledGestures = GestureType.Tap;

            _tps.AddEvent(1, TouchLocationState.Moved, pos + new Vector2(80, 0));
            TouchPanelState.Update(GameTimeForFrame(2));

            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        public void BasicFlick()
        {
            _tps.EnabledGestures = GestureType.Flick;
            var startPos = new Vector2(200, 200);

            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, startPos);
            TouchPanelState.Update(GameTimeForFrame(1));

            //Move it until it should have made a flick
            int diff = 0;
            int frame = 1;
            while (new Vector2(diff, 0).Length() < TouchPanelState.TapJitterTolerance)
            {
                Assert.False(_tps.IsGestureAvailable);

                diff += 30;
                frame++;

                _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(diff, 0));
                TouchPanelState.Update(GameTimeForFrame(frame));
            }
            Assert.False(_tps.IsGestureAvailable);

            //Now release
            frame++;
            _tps.AddEvent(1, TouchLocationState.Released, startPos + new Vector2(diff, 0));
            TouchPanelState.Update(GameTimeForFrame(frame));

            //Now we should have the flick
            Assert.True(_tps.IsGestureAvailable);
            var gesture = _tps.ReadGesture();
            Assert.False(_tps.IsGestureAvailable);

            Assert.AreEqual(GestureType.Flick, gesture.GestureType);
            Assert.AreEqual(Vector2.Zero, gesture.Position);
            //Could check Delta here, it contains the flick velocity
        }

        [Test]
        [Description("Do a short movement within TapJitterTolerance, this should not make a flick even if it is quick")]
        public void ShortMovementDoesntMakeAFlick()
        {
            _tps.EnabledGestures = GestureType.Flick;
            var startPos = new Vector2(200, 200);

            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, startPos);
            TouchPanelState.Update(GameTimeForFrame(1));

            //Then release it at the edge of the detection size
            _tps.AddEvent(1, TouchLocationState.Released, startPos + new Vector2(TouchPanelState.TapJitterTolerance, 0));
            TouchPanelState.Update(GameTimeForFrame(2));

            //This should not make a flick. If the distance is 1 greater it will.
            Assert.False(_tps.IsGestureAvailable);
        }


        [Test]
        [Description("If Flick and FreeDrag are enabled, both events should be generated without impacting each other. " +
                     "There should be a flick and a DragComplete at the end in that order")]
        public void FlickAndFreeDrag()
        {
            _tps.EnabledGestures = GestureType.Flick | GestureType.FreeDrag | GestureType.DragComplete;
            var startPos = new Vector2(200, 200);
            GestureSample gesture;

            //Place the finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, startPos);
            TouchPanelState.Update(GameTimeForFrame(1));

            //Move it until it should have made a flick
            int diff = 0;
            int frame = 1;
            while (frame < 4)
            {
                diff += 40;
                frame++;

                _tps.AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(diff, 0));
                TouchPanelState.Update(GameTimeForFrame(frame));

                //Each drag should make a FreeDrag
                Assert.True(_tps.IsGestureAvailable);
                gesture = _tps.ReadGesture();
                Assert.False(_tps.IsGestureAvailable);

                Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);
                Assert.AreEqual(startPos + new Vector2(diff, 0), gesture.Position);
            }
            Assert.False(_tps.IsGestureAvailable);

            //Now release
            frame++;
            _tps.AddEvent(1, TouchLocationState.Released, startPos + new Vector2(diff, 0));
            TouchPanelState.Update(GameTimeForFrame(frame));

            //Now we should have the flick
            Assert.True(_tps.IsGestureAvailable);
            gesture = _tps.ReadGesture();

            Assert.AreEqual(GestureType.Flick, gesture.GestureType);
            Assert.AreEqual(Vector2.Zero, gesture.Position);
            
            //And then the DragComplete
            Assert.True(_tps.IsGestureAvailable);
            gesture = _tps.ReadGesture();

            Assert.AreEqual(GestureType.DragComplete, gesture.GestureType);
            Assert.AreEqual(Vector2.Zero, gesture.Position);

            //And that should be it
            Assert.False(_tps.IsGestureAvailable);
        }

        [Test]
        public void BasicPinch()
        {
            //TODO: This test is based on current behavior. We need to verify that XNA behaves the same

            _tps.EnabledGestures = GestureType.Pinch | GestureType.PinchComplete;

            var pos1 = new Vector2(200, 200);
            var pos2 = new Vector2(400, 200);

            //Place a finger down
            _tps.AddEvent(1, TouchLocationState.Pressed, pos1);
            TouchPanelState.Update(GameTimeForFrame(1));
            Assert.False(_tps.IsGestureAvailable);

            //Place the other finger down
            _tps.AddEvent(2, TouchLocationState.Pressed, pos2);
            TouchPanelState.Update(GameTimeForFrame(2));

            //Now we should have a pinch
            Assert.True(_tps.IsGestureAvailable);
            var gesture = _tps.ReadGesture();

            Assert.AreEqual(GestureType.Pinch, gesture.GestureType);
            Assert.AreEqual(pos1, gesture.Position);
            Assert.AreEqual(pos2, gesture.Position2);

            //If we do nothing, we shouldn't get more pinch events
            TouchPanelState.Update(GameTimeForFrame(3));
            Assert.False(_tps.IsGestureAvailable);

            //But if we move a finger, we should get an updated pinch
            pos2 += new Vector2(50, 0);
            _tps.AddEvent(2, TouchLocationState.Moved, pos2);
            TouchPanelState.Update(GameTimeForFrame(4));

            Assert.True(_tps.IsGestureAvailable);
            gesture = _tps.ReadGesture();

            Assert.AreEqual(GestureType.Pinch, gesture.GestureType);
            Assert.AreEqual(pos1, gesture.Position);
            Assert.AreEqual(pos2, gesture.Position2);

            //Now releasing one of the fingers should make a pinch complete event
            pos1 -= new Vector2(0, 50);
            _tps.AddEvent(1, TouchLocationState.Released, pos1);
            TouchPanelState.Update(GameTimeForFrame(5));

            Assert.True(_tps.IsGestureAvailable);
            gesture = _tps.ReadGesture();

            Assert.AreEqual(GestureType.PinchComplete, gesture.GestureType);
            Assert.AreEqual(Vector2.Zero, gesture.Position);
            Assert.AreEqual(Vector2.Zero, gesture.Position2);

            //We should have no more events
            Assert.False(_tps.IsGestureAvailable);
        }
    }
}
