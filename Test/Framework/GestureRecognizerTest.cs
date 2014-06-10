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
            _tps.Update(GameTimeForFrame(2)); //Not sure if this should be necessary

            Assert.True(_tps.IsGestureAvailable);
            var gesture = _tps.ReadGesture();

            Assert.AreEqual(GestureType.Tap, gesture.GestureType);
            Assert.AreEqual(pos, gesture.Position);
        }
    }
}
