#region License
// /*
// Microsoft Public License (Ms-PL)
// XnaTouch - Copyright © 2009-2010 The XnaTouch Team
//
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

#region Using clause
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#if WINRT
using Windows.Graphics.Display;
#if !WINDOWS_PHONE
using Windows.UI.Xaml;
#endif
#endif

#endregion Using clause

namespace Microsoft.Xna.Framework.Input.Touch
{
    public static class TouchPanel
    {
        /// <summary>
        /// The maximum number of events to allow in the touch or gesture event lists.
        /// </summary>
        private const int MaxEvents = 100;

        /// <summary>
        /// The reserved touchId for all mouse touch points.
        /// </summary>
        private const int MouseTouchId = 1;

        /// <summary>
        /// The current touch state.
        /// </summary>
        private static readonly List<TouchLocation> _touchState = new List<TouchLocation>();

        /// <summary>
        /// The touch events to be processed and added to the touch state.
        /// </summary>
        private static readonly List<TouchLocation> _touchEvents = new List<TouchLocation>();

        /// <summary>
        /// The current gesture state.
        /// </summary>
        private static readonly List<TouchLocation> _gestureState = new List<TouchLocation>();

        /// <summary>
        /// The gesture events to be processed and added to the gesture state.
        /// </summary>
        private static readonly List<TouchLocation> _gestureEvents = new List<TouchLocation>();


        /// <summary>
        /// The positional scale to apply to touch input.
        /// </summary>
        private static Vector2 _touchScale = Vector2.One;

        /// <summary>
        /// The current size of the display.
        /// </summary>
        private static Point _displaySize = Point.Zero;

        /// <summary>
        /// The next touch location identifier.
        /// The value 1 is reserved for the mouse touch point.
        /// </summary>
        private static int _nextTouchId = 2;

        /// <summary>
        /// The mapping between platform specific touch ids
        /// and the touch ids we assign to touch locations.
        /// </summary>
        private static readonly Dictionary<int, int> _touchIds = new Dictionary<int, int>();

        private static readonly Queue<GestureSample> GestureList = new Queue<GestureSample>();

        private static TouchPanelCapabilities Capabilities = new TouchPanelCapabilities();

        public static TouchPanelCapabilities GetCapabilities()
        {
            Capabilities.Initialize();
            return Capabilities;
        }

        private static bool RefreshState(bool consumeState, List<TouchLocation> state, List<TouchLocation> events)
        {
            var stateChanged = false;

            // Update the existing touch locations.
            for (var i = 0; i < state.Count; i++)
            {
                // Get the next touch location for update.
                var touch = state[i];

                // If this location isn't in the move state yet then skip it.
                if (!consumeState && touch.State != TouchLocationState.Moved)
                    continue;

                // If the touch state has been consumed then we can
                // remove old release locations.
                if (consumeState && touch.State == TouchLocationState.Released)
                {
                    state.RemoveAt(i);
                    i--;
                    continue;
                }

                var foundEvent = false;

                // Remove the next pending event with the 
                // same id and make it the new touch state.
                for (var j = 0; j < events.Count; j++)
                {
                    var newTouch = events[j];

                    // Don't let a release event occur until we're 
                    // ready to consume state again.
                    if (!consumeState && newTouch.State == TouchLocationState.Released)
                        continue;

                    if (newTouch.Id == touch.Id)
                    {
                        stateChanged |= touch.UpdateState(newTouch);
                        foundEvent = true;
                        events.RemoveAt(j);
                        break;
                    }
                }

                // If a new event was found then store it.
                if (foundEvent)
                    state[i] = touch;

                // Else if no event has come in then promote it to
                // the moved state, but only when we're consuming state.
                else if (consumeState)
                    state[i] = touch.AsMovedState();
            }

            // We add new pressed events last so they are not 
            // consumed before the touch state is returned.
            for (var i = 0; i < events.Count; )
            {
                var loc = events[i];

                if (loc.State == TouchLocationState.Pressed)
                {
                    state.Add(loc);
                    events.RemoveAt(i);
                    stateChanged = true;
                    continue;
                }

                i++;
            }

            return stateChanged;
        }

        public static TouchCollection GetState()
        {
            // Process the touch state.
            var consumeState = true;
            while (RefreshState(consumeState, _touchState, _touchEvents))
                consumeState = false;

            return new TouchCollection(_touchState.ToArray());
        }

        internal static void AddEvent(int id, TouchLocationState state, Vector2 position)
        {
            AddEvent(id, state, position, false);
        }

        internal static void AddEvent(int id, TouchLocationState state, Vector2 position, bool isMouse)
        {
            // Different platforms return different touch identifiers
            // based on the specifics of their implementation and the
            // system drivers.
            //
            // Sometimes these ids are suitable for our use, but other
            // times it can recycle ids or do cute things like return
            // the same id for double tap events.
            //
            // We instead provide consistent ids by generating them
            // ourselves on the press and looking them up on move 
            // and release events.
            // 
            if (state == TouchLocationState.Pressed)
            {
                if (isMouse)
                {
                    // Mouse pointing devices always use a reserved Id
                    _touchIds[id] = MouseTouchId;
                }
                else
                {
                    _touchIds[id] = _nextTouchId++;
                }
            }

            // Try to find the touch id.
            int touchId;
            if (!_touchIds.TryGetValue(id, out touchId))
            {
                // If we got here that means either the device is sending
                // us bad, out of order, or old touch events.  In any case
                // just ignore them.
                return;
            }

            if (!isMouse || EnableMouseTouchPoint || EnableMouseGestures)
            {
                // Add the new touch event keeping the list from getting
                // too large if no one happens to be requesting the state.
                var evt = new TouchLocation(touchId, state, position * _touchScale);

                if (!isMouse || EnableMouseTouchPoint)
                {
                    _touchEvents.Add(evt);
                    if (_touchEvents.Count > MaxEvents)
                        _touchEvents.RemoveRange(0, _touchEvents.Count - MaxEvents);
                }

                // If we have gestures enabled then start to collect 
                // events for those too.
                if (EnabledGestures != GestureType.None && (!isMouse || EnableMouseGestures))
                {
                    _gestureEvents.Add(evt);
                    if (_gestureEvents.Count > MaxEvents)
                        _gestureEvents.RemoveRange(0, _gestureEvents.Count - MaxEvents);
                }
            }

            // If this is a release unmap the hardware id.
            if (state == TouchLocationState.Released)
                _touchIds.Remove(id);
        }

        /// <summary>
        /// This will release all touch locations.  It should only be 
        /// called on platforms where touch state is reset all at once.
        /// </summary>
        internal static void ReleaseAllTouches()
        {
            // Clear any pending events.
            _touchEvents.Clear();
            _gestureEvents.Clear();

            // Submit a new event for each non-released location.
            foreach (var touch in _touchState)
            {
                if (touch.State != TouchLocationState.Released)
                    _touchEvents.Add(new TouchLocation(touch.Id, TouchLocationState.Released, touch.Position));
            }
            foreach (var touch in _gestureState)
            {
                if (touch.State != TouchLocationState.Released)
                    _gestureEvents.Add(new TouchLocation(touch.Id, TouchLocationState.Released, touch.Position));
            }

            // Release all the touch id mappings.
            _touchIds.Clear();
        }

        private static void UpdateTouchScale()
        {
                // Get the window size.
                //
                // TODO: This will be alot smoother once we get XAML working with Game.
                var windowSize = Vector2.One;
                if (Game.Instance != null)
                    windowSize = new Vector2(   Game.Instance.Window.ClientBounds.Width,
                                                Game.Instance.Window.ClientBounds.Height);
#if WINDOWS_STOREAPP
                else
                {
                    var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
                    windowSize = new Vector2(   (float)Window.Current.CoreWindow.Bounds.Width * dipFactor,
                                                (float)Window.Current.CoreWindow.Bounds.Height * dipFactor);
                }
#endif

                // Recalculate the touch scale.
                _touchScale = new Vector2(  (float)DisplayWidth / windowSize.X,
                                            (float)DisplayHeight / windowSize.Y);
        }

		public static GestureSample ReadGesture()
        {
            // Return the next gesture.
			return GestureList.Dequeue();			
        }

        public static IntPtr WindowHandle { get; set; }

        public static int DisplayHeight
        {
            get
            {
                return _displaySize.Y;
            }
            set
            {
                _displaySize.Y = value;
                UpdateTouchScale();
            }
        }

        public static DisplayOrientation DisplayOrientation
        {
            get;
            set;
        }

        public static int DisplayWidth
        {
            get
            {
                return _displaySize.X;
            }
            set
            {
                _displaySize.X = value;
                UpdateTouchScale();
            }
        }
		
        public static GestureType EnabledGestures { get; set; }

        public static bool EnableMouseTouchPoint { get; set; }

        public static bool EnableMouseGestures { get; set; }

        public static bool IsGestureAvailable
        {
            get
            {
                // Process the pending gesture events.
                while (_gestureEvents.Count > 0)
                {
                    var stateChanged = RefreshState(true, _gestureState, _gestureEvents);
                    UpdateGestures(stateChanged);
                }

                return GestureList.Count > 0;				
            }
        }
		
		#region Gesture Recognition
		
        /// <summary>
        /// Maximum distance a touch location can wiggle and 
        /// not be considered to have moved.
        /// </summary>
        private const float TapJitterTolerance = 35.0f;

		private static readonly TimeSpan _maxTicksToProcessHold = TimeSpan.FromMilliseconds(1024);
		
		/// <summary>
		/// The pinch touch locations.
		/// </summary>
        private static readonly TouchLocation [] _pinchTouch = new TouchLocation[2];

        /// <summary>
        /// If true the pinch touch locations are valid and
        /// a pinch gesture has begun.
        /// </summary>
		private static bool _pinchGestureStarted;
		

		private static bool GestureIsEnabled(GestureType gestureType)
		{
            return (EnabledGestures & gestureType) != 0;
		}

        /// <summary>
        /// Used to disable emitting of tap gestures.
        /// </summary>
        static private bool _tapDisabled;

        /// <summary>
        /// Used to disable emitting of hold gestures.
        /// </summary>
        static private bool _holdDisabled;
        

        private static void UpdateGestures(bool stateChanged)
		{
            // These are observed XNA gesture rules which we follow below.  Please
            // add to them if a new case is found.
            //
            //  - Tap occurs on release.
            //  - DoubleTap occurs on the first press after a Tap.
            //  - Tap, Double Tap, and Hold are disabled if a drag begins or more than one finger is pressed.
            //  - Drag occurs when one finger is down and actively moving.
            //  - Pinch occurs if 2 or more fingers are down and at least one is moving.
            //  - If you enter a Pinch during a drag a DragComplete is fired.
            //  - Drags are classified as horizontal, vertical, free, or none and stay that way.
            //

            // First get a count of touch locations which 
            // are not in the released state.
		    var heldLocations = 0;
            foreach (var touch in _gestureState)
                heldLocations += touch.State != TouchLocationState.Released ? 1 : 0;

            // As soon as we have more than one held point then 
            // tap and hold gestures are disabled until all the 
            // points are released.
            if (heldLocations > 1)
            {
                _tapDisabled = true;
                _holdDisabled = true;
            }

		    // Process the touch locations for gestures.
            foreach (var touch in _gestureState)
		    {
		        switch(touch.State)
		        {
		            case TouchLocationState.Pressed:
		            case TouchLocationState.Moved:
                    {
                        // The DoubleTap event is emitted on first press as
                        // opposed to Tap which happens on release.
		                if( touch.State == TouchLocationState.Pressed &&
                            ProcessDoubleTap(touch))
		                    break;

		                // Any time more than one finger is down and pinch is
                        // enabled then we exclusively do pinch processing.
                        if (GestureIsEnabled(GestureType.Pinch) && heldLocations > 1)
                        {
                            // Save or update the first pinch point.
                            if (    _pinchTouch[0].State == TouchLocationState.Invalid || 
                                    _pinchTouch[0].Id == touch.Id)
                                _pinchTouch[0] = touch;

                            // Save or update the second pinch point.
                            else if (   _pinchTouch[1].State == TouchLocationState.Invalid ||
                                        _pinchTouch[1].Id == touch.Id)
                                _pinchTouch[1] = touch;

                            // NOTE: Actual pinch processing happens outside and
                            // below this loop to ensure both points are updated
                            // before gestures are emitted.
                            break;
                        }

                        // If we're not dragging try to process a hold event.
		                var dist = Vector2.Distance(touch.Position, touch.PressPosition);
                        if (_dragGestureStarted == GestureType.None && dist < TapJitterTolerance)
                        {
                            ProcessHold(touch);
                            break;
                        }

                        // If the touch state has changed then do a drag gesture.
                        if (stateChanged)
                            ProcessDrag(touch);
		                break;
					}
					
		            case TouchLocationState.Released:
                    {
                        // If the touch state hasn't changed then this
                        // is an old release event... skip it.
                        if (!stateChanged)
                            break;

                        // If this is one of the pinch locations then we
                        // need to fire off the complete event and stop
                        // the pinch gesture operation.
                        if (    _pinchGestureStarted &&
		                        (   touch.Id == _pinchTouch[0].Id ||
		                            touch.Id == _pinchTouch[1].Id))
		                {
		                    if (GestureIsEnabled(GestureType.PinchComplete))
		                        GestureList.Enqueue(new GestureSample(
		                                                GestureType.PinchComplete, touch.Timestamp,
		                                                Vector2.Zero, Vector2.Zero,
		                                                Vector2.Zero, Vector2.Zero));

                            _pinchGestureStarted = false;
		                    _pinchTouch[0] = TouchLocation.Invalid;
                            _pinchTouch[1] = TouchLocation.Invalid;
		                    break;
		                }

                        // If there are still other pressed locations then there
                        // is nothing more we can do with this release.
                        if (heldLocations != 0)
                            break;

                        // From testing XNA it seems we need a velocity 
                        // of about 100 to classify this as a flick.
		                var dist = Vector2.Distance(touch.Position, touch.PressPosition);
                        if (    dist > TapJitterTolerance && 
                                touch.Velocity.Length() > 100.0f &&
                                GestureIsEnabled(GestureType.Flick))
                        {
                            GestureList.Enqueue(new GestureSample(
                                                    GestureType.Flick, touch.Timestamp,
                                                    Vector2.Zero, Vector2.Zero,
                                                    touch.Velocity, Vector2.Zero));

                            // If we got a flick then stop the drag operation
                            // so that no DragComplete occurs.
                            _dragGestureStarted = GestureType.None;
                            break;
                        }

                        // If a drag is active then we need to finalize it.
                        if (_dragGestureStarted != GestureType.None)
                        {
                            if (GestureIsEnabled(GestureType.DragComplete))
                                GestureList.Enqueue(new GestureSample(
                                                        GestureType.DragComplete, touch.Timestamp,
                                                        Vector2.Zero, Vector2.Zero,
                                                        Vector2.Zero, Vector2.Zero));

                            _dragGestureStarted = GestureType.None;
                            break;
                        }

                        // If all else fails try to process it as a tap.
                        ProcessTap(touch);
		                break;					
                    }
		        }
		    }

            // If the touch state hasn't changed then there is no 
			// cleanup to do and no pinch to process.
            if (!stateChanged)
                return;

            // If we have two pinch points then update the pinch state.
            if (    GestureIsEnabled(GestureType.Pinch) && 
                    _pinchTouch[0].State != TouchLocationState.Invalid  &&
                    _pinchTouch[1].State != TouchLocationState.Invalid)
                ProcessPinch(_pinchTouch);
            else
            {
                // Make sure a partial pinch state 
                // is not left hanging around.
                _pinchGestureStarted = false;
                _pinchTouch[0] = TouchLocation.Invalid;
                _pinchTouch[1] = TouchLocation.Invalid;
            }

            // If all points are released then clear some states.
            if (heldLocations == 0)
            {
                _tapDisabled = false;
                _holdDisabled = false;
                _dragGestureStarted = GestureType.None;
            }
		}

		private static void ProcessHold(TouchLocation touch)
		{
            if (!GestureIsEnabled(GestureType.Hold) || _holdDisabled)
				return;

            var elapsed = TimeSpan.FromTicks(DateTime.Now.Ticks) - touch.PressTimestamp;
            if (elapsed < _maxTicksToProcessHold)
				return;

		    _holdDisabled = true;

			GestureList.Enqueue(
                new GestureSample(  GestureType.Hold, 
                                    touch.Timestamp,
			                        touch.Position, Vector2.Zero,
			                        Vector2.Zero, Vector2.Zero));			
		}

        private static bool ProcessDoubleTap(TouchLocation touch)
        {
            if (!GestureIsEnabled(GestureType.DoubleTap) || _tapDisabled)
                return false;
                           
            // If the new tap is too far away from the last then
            // this cannot be a double tap event.
            var dist = Vector2.Distance(touch.Position, _lastTap.Position);
            if (dist > TapJitterTolerance)
                return false;
                
            // Check that this tap happened within the standard 
            // double tap time threshold of 300 milliseconds.
            var elapsed = touch.Timestamp - _lastTap.Timestamp;
            if (elapsed.TotalMilliseconds > 300)
                return false;

            GestureList.Enqueue(new GestureSample(
                           GestureType.DoubleTap, touch.Timestamp,
                           touch.Position, Vector2.Zero,
                           Vector2.Zero, Vector2.Zero));

            // Disable taps until after the next release.
            _tapDisabled = true;

            return true;
        }

        private static TouchLocation _lastTap;

		private static void ProcessTap(TouchLocation touch)
		{
            if (!GestureIsEnabled(GestureType.Tap) || _tapDisabled)
				return;

            // If the release is too far away from the press 
            // position then this cannot be a tap event.
            var dist = Vector2.Distance(touch.PressPosition, touch.Position);
            if (dist > TapJitterTolerance)
				return;

            // If we pressed and held too long then don't 
            // generate a tap event for it.
            var elapsed = TimeSpan.FromTicks(DateTime.Now.Ticks) - touch.PressTimestamp;
            if (elapsed > _maxTicksToProcessHold)
				return;            
            
            // Store the last tap for 
            // double tap processing.          
		    _lastTap = touch;

            // Fire off the tap event immediately.
            var tap = new GestureSample(
		        GestureType.Tap, touch.Timestamp,
		        touch.Position, Vector2.Zero,
		        Vector2.Zero, Vector2.Zero);
            GestureList.Enqueue(tap);
		}

        private static GestureType _dragGestureStarted = GestureType.None;

		private static void ProcessDrag(TouchLocation touch)
		{
		    var dragH = GestureIsEnabled(GestureType.HorizontalDrag);
		    var dragV = GestureIsEnabled(GestureType.VerticalDrag);
		    var dragF  = GestureIsEnabled(GestureType.FreeDrag);

            if (!dragH && !dragV && !dragF)
				return;

            // Make sure this is a move event and that we have
            // a previous touch location.
            TouchLocation prevTouch;
            if (    touch.State != TouchLocationState.Moved ||
                    !touch.TryGetPreviousLocation(out prevTouch))
                return;
		
            var delta = touch.Position - prevTouch.Position;

            // If we're free dragging then stick to it.
            if (_dragGestureStarted != GestureType.FreeDrag)
            {
		        var isHorizontalDelta = Math.Abs(delta.X) > Math.Abs(delta.Y * 2.0f);
                var isVerticalDelta = Math.Abs(delta.Y) > Math.Abs(delta.X * 2.0f);
                var classify = _dragGestureStarted == GestureType.None;

                // Once we enter either vertical or horizontal drags
                // we stick to it... regardless of the delta.
                if (dragH && ((classify && isHorizontalDelta) || _dragGestureStarted == GestureType.HorizontalDrag))
                {
                    delta.Y = 0;
                    _dragGestureStarted = GestureType.HorizontalDrag;
                }
                else if (dragV && ((classify && isVerticalDelta) || _dragGestureStarted == GestureType.VerticalDrag))
                {
                    delta.X = 0;
                    _dragGestureStarted = GestureType.VerticalDrag;
                }

                // If the delta isn't either horizontal or vertical
                //then it could be a free drag if not classified.
                else if (dragF && classify)
                {
                    _dragGestureStarted = GestureType.FreeDrag;
                }
                else
                {
                    // If we couldn't classify the drag then
                    // it is nothing... set it to complete.
                    _dragGestureStarted = GestureType.DragComplete;
                }
            }

            // If the drag could not be classified then no gesture.
            if (    _dragGestureStarted == GestureType.None || 
                    _dragGestureStarted == GestureType.DragComplete)
                return;

            _tapDisabled = true;
            _holdDisabled = true;

			GestureList.Enqueue(new GestureSample(
                                    _dragGestureStarted, touch.Timestamp,
								    touch.Position, Vector2.Zero,
								    delta, Vector2.Zero));
		}
		
		private static void ProcessPinch(TouchLocation [] touches)
		{
			TouchLocation prevPos0;
			TouchLocation prevPos1;
			
			if (!touches[0].TryGetPreviousLocation(out prevPos0))
				prevPos0 = touches[0];
			
			if (!touches[1].TryGetPreviousLocation(out prevPos1))
				prevPos1 = touches[1];
			
			var delta0 = touches[0].Position - prevPos0.Position;
			var delta1 = touches[1].Position - prevPos1.Position;

            // Get the newest timestamp.
		    var timestamp = touches[0].Timestamp > touches[1].Timestamp ? touches[0].Timestamp : touches[1].Timestamp;

            // If we were already in a drag state then fire
            // off the drag completion event.
            if (_dragGestureStarted != GestureType.None)
            {
                if (GestureIsEnabled(GestureType.DragComplete))
                    GestureList.Enqueue(new GestureSample(
                                            GestureType.DragComplete, timestamp,
                                            Vector2.Zero, Vector2.Zero,
                                            Vector2.Zero, Vector2.Zero));

                _dragGestureStarted = GestureType.None;
            }

			GestureList.Enqueue (new GestureSample (
				GestureType.Pinch,
                timestamp,                
				touches[0].Position, touches[1].Position,
				delta0, delta1));

		    _pinchGestureStarted = true;
            _tapDisabled = true;
		    _holdDisabled = true;
		}
		
		#endregion
    }
}