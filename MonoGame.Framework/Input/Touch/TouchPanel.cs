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
using Windows.UI.Xaml;
#endif

#endregion Using clause

namespace Microsoft.Xna.Framework.Input.Touch
{
    public static class TouchPanel
    {
        /// <summary>
        /// The currently touch locations for state.
        /// </summary>
        private static readonly List<TouchLocation> _touchLocations = new List<TouchLocation>();

        /// <summary>
        /// The touch events to be processed and added to the current state.
        /// </summary>
        private static readonly List<TouchLocation> _events = new List<TouchLocation>();

        /// <summary>
        /// The touch location being tracked for fake mouse input.
        /// </summary>
        private static int _mouseTouchId = -1;

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
        /// </summary>
        private static int _nextTouchId = 1;

        /// <summary>
        /// The mapping between platform specific touch ids
        /// and the touch ids we assign to touch locations.
        /// </summary>
        private static readonly Dictionary<int, int> _touchIds = new Dictionary<int, int>();

        private static readonly Queue<GestureSample> GestureList = new Queue<GestureSample>();

        private static TouchPanelCapabilities Capabilities = new TouchPanelCapabilities();

        // TODO: Who is this for?  Is it used?
        internal static event EventHandler EnabledGesturesChanged;

        static TouchPanel()
        {
#if !WINDOWS && !LINUX && !MONOMAC

            // Enable fake mouse events on the 
            // non-desktop platforms by default.
            FakeMouseEnabled = true;
#endif
        }

        public static TouchPanelCapabilities GetCapabilities()
        {
            Capabilities.Initialize();
            return Capabilities;
        }

        private static void RefreshState(bool clearReleased)
        {
            if (clearReleased)
            {
                // Remove the previously released touch locations as these
                // should have been processed by the caller by now.
                for (var i = 0; i < _touchLocations.Count;)
                {
                    var touch = _touchLocations[i];

                    if (touch.State == TouchLocationState.Released)
                    {
                        // Remove this location from the gesture and the touch state.
                        _heldEventsProcessed.Remove(touch.Id);
                        _touchLocations.RemoveAt(i);
                        continue;
                    }

                    i++;
                }
            }

            // Update the existing touch locations.
            for (var i=0; i < _touchLocations.Count; i++)
            {
                // Get the next touch location for update.
                var prevTouch = _touchLocations[i];

                // If this location has been released then skip it.
                if (prevTouch.State == TouchLocationState.Released)
                    continue;

                // If no other events are found the default next touch state 
                // will be the current state converted to a move event.
                var nextTouch = prevTouch.AsMovedState();

                // Remove all pending events with the same id keeping
                // the last one as the next new touch state.
                for (var j = 0; j < _events.Count; )
                {
                    if (_events[j].Id == prevTouch.Id)
                    {
                        nextTouch.UpdateState(_events[j]);
                        _events.RemoveAt(j);
                        continue;
                    }
                    
                    j++;
                }

                // Set the new touch state.
                _touchLocations[i] = nextTouch;
            }

            // Add new pressed events last to ensure that they
            // are seen once before becoming a move or release.
            for (var i = 0; i < _events.Count; )
            {
                var loc = _events[i];

                if (loc.State == TouchLocationState.Pressed)
                {
                    _touchLocations.Add(loc);
                    _events.RemoveAt(i);
                    continue;
                }

                i++;
            }

			// Update the gesture state.
			UpdateGestures();
        }

        public static TouchCollection GetState()
        {
            RefreshState(true);

            var state = new TouchCollection(_touchLocations.ToArray());
            return state;
        }

        /// <summary>
        /// When true fake mouse events are sent for 
        /// the first pressed finger.
        /// </summary>
        internal static bool FakeMouseEnabled { get; set; }

        internal static void AddEvent(int id, TouchLocationState state, Vector2 position)
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
                _touchIds[id] = _nextTouchId++;

            // Add the new touch event.
            _events.Add(new TouchLocation(_touchIds[id], state, position * _touchScale));

            // If this is a release unmap the hardware id.
            if (state == TouchLocationState.Released)
                _touchIds.Remove(id);

            /*
            // Do fake mouse events if that feature is enabled.
            if (!FakeMouseEnabled)
                _mouseTouchId = -1;
            else if (_mouseTouchId == -1 || id == _mouseTouchId)
            {
                if (state == TouchLocationState.Released)
                {
                    _mouseTouchId = -1;
                    Mouse.SetTouchMouse(ButtonState.Released, (int)position.X, (int)position.Y);
                }
                else
                {
                    // Store the touch point to track and set the new state.
                    _mouseTouchId = id;
                    Mouse.SetTouchMouse(ButtonState.Pressed, (int)position.X, (int)position.Y);
                }
            }
            */
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
#if WINRT
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
                if (_displaySize.Y != value)
                {
                    _displaySize.Y = value;
					UpdateTouchScale();
                }
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
                if (_displaySize.X != value)
                {
                    _displaySize.X = value;
                    UpdateTouchScale();
                }
            }
        }
		
		private static GestureType _enabledGestures = GestureType.None;
        public static GestureType EnabledGestures
        {
            get
			{ 
				return _enabledGestures;
			}
            set
			{
				if (_enabledGestures != value)
                {
                    _enabledGestures = value;
                    if (EnabledGesturesChanged != null)
					    EnabledGesturesChanged(null, EventArgs.Empty);
                }
			}
        }

        public static bool IsGestureAvailable
        {
            get
            {
                // Gather more events if we've run out.
                if (GestureList.Count == 0 && _events.Count > 0)
                    RefreshState(false);

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
		
		// For pinch, we'll need to "save" a touch so we can
		// send both at the same time
		private static TouchLocation?[] _savedPinchTouches = new TouchLocation?[2];
		private static bool _pinchComplete = false;
		
		private static bool GestureIsEnabled(GestureType gestureType)
		{
            return (_enabledGestures & gestureType) != 0;
		}

		private static void UpdateGestures()
		{
		    foreach (var touch in _touchLocations)
		    {
		        switch(touch.State)
		        {
		            case TouchLocationState.Invalid:
		                // TODO: How can this happen?
		                break;

		            case TouchLocationState.Pressed:
		            case TouchLocationState.Moved:
                    
		                if( touch.State == TouchLocationState.Pressed &&
		                    ProcessDoubleTap(touch))
		                    break;
                    
		                // Any time that two fingers are detected in XNA, it's considered a pinch
		                // Save the touch and combine it with the next one to create a pinch gesture
		                if (GestureIsEnabled(GestureType.Pinch) &&
		                    _touchLocations.Count > 1 &&
		                    !_pinchComplete)
		                {
		                    if (_savedPinchTouches[0] == null || _savedPinchTouches[0].Value.Id == touch.Id)
		                        _savedPinchTouches[0] = touch;
		                    else if (_savedPinchTouches[1] == null || _savedPinchTouches[1].Value.Id == touch.Id)
		                    {
		                        _savedPinchTouches[1] = touch;
		                        ProcessPinch(_savedPinchTouches);
		                    }
						
		                    break;
		                }

		                if (touch.State == TouchLocationState.Moved && _touchLocations.Count == 1)
		                {
		                    // If we're already processing this location as a 
		                    // drag then keep at it.
		                    if (_dragGestureId == touch.Id)
		                        ProcessDrag(touch);
                        
		                    // If we're not dragging then try to start one.
		                    if (_dragGestureId == -1)
		                    {
		                        var dist = Vector2.Distance(touch.Position, touch.PressPosition);
		                        if (dist < TapJitterTolerance)
		                            ProcessHold(touch);
		                        else
		                            ProcessDrag(touch);
		                    }
		                }
					
		                break;
					
		                // Released. Can be a tap/Doubletap, or the end of a
		                // previous gesture.
		            case TouchLocationState.Released:
		                if (_savedPinchTouches[0] != null && 
		                    _savedPinchTouches[1] != null &&
		                    (touch.Id == _savedPinchTouches[0].Value.Id ||
		                     touch.Id == _savedPinchTouches[1].Value.Id))
		                {
		                    if (GestureIsEnabled(GestureType.PinchComplete))
		                        GestureList.Enqueue(new GestureSample(
		                                                GestureType.PinchComplete, touch.Timestamp,
		                                                Vector2.Zero, Vector2.Zero,
		                                                Vector2.Zero, Vector2.Zero));

		                    _pinchComplete = true;
		                    _dragGestureId = -1;
		                }
		                else
		                {
		                    // Once a drag has began, that touch can no longer
		                    // generate a tap event.
		                    if (_dragGestureId == -1 && ProcessTap(touch))
		                        break;

		                    // From testing XNA it seems we need a velocity 
		                    // of about 100 to classify this as a flick.
                            if (    GestureIsEnabled(GestureType.Flick) &&
                                    touch.Velocity.Length() > 100.0f)
                            {
                                GestureList.Enqueue(new GestureSample(
                                                        GestureType.Flick, touch.Timestamp,
                                                        Vector2.Zero, Vector2.Zero,
                                                        touch.Velocity, Vector2.Zero));
                            }
                            else if (   GestureIsEnabled(GestureType.DragComplete) && 
                                        _dragGestureId == touch.Id)
                            {
                                GestureList.Enqueue(new GestureSample(
                                                        GestureType.DragComplete, touch.Timestamp,
                                                        Vector2.Zero, Vector2.Zero,
                                                        Vector2.Zero, Vector2.Zero));
                            }

                            if (_dragGestureId == touch.Id)
                                _dragGestureId = -1;
		                }
		                break;					
		        }
		    }

		    if (_pinchComplete)
			{
				_savedPinchTouches[0] = _savedPinchTouches[1] = null;
				_pinchComplete = false;
			}
		}

        static readonly List<int> _heldEventsProcessed = new List<int>();

		private static void ProcessHold(TouchLocation touch)
		{
			if (!GestureIsEnabled(GestureType.Hold))
				return;

            var elapsed = TimeSpan.FromTicks(DateTime.Now.Ticks) - touch.PressTimestamp;
            if (elapsed < _maxTicksToProcessHold)
				return;
			
			// Only a single held event gets sent per touch.
			// Make sure we only send one.
			if (_heldEventsProcessed.Contains(touch.Id))
				return;
			
			_heldEventsProcessed.Add(touch.Id);
			
			GestureList.Enqueue(
                new GestureSample(  GestureType.Hold, 
                                    touch.Timestamp,
			                        touch.Position, Vector2.Zero,
			                        Vector2.Zero, Vector2.Zero));			
		}

        private static int _lastDoubleTapId;

        private static bool ProcessDoubleTap(TouchLocation touch)
        {
            if (!GestureIsEnabled(GestureType.DoubleTap))
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
                         
            // "Replace" it with a doubletap.
            GestureList.Enqueue(new GestureSample(
                           GestureType.DoubleTap, touch.Timestamp,
                           touch.Position, Vector2.Zero,
                           Vector2.Zero, Vector2.Zero));
            
            // This touch will eventually enter a "released" state. When it does, because
            // it was part of a doubletap, we don't want to process a third tap at the end.
            // Save it's ID to prevent this from showing up in the future.
            _lastDoubleTapId = touch.Id;

            return true;
        }

        private static TouchLocation _lastTap;

		private static bool ProcessTap(TouchLocation touch)
		{
            // TODO: This check means that double taps won't work unless
            // the tap event is enabled as well... is this correct behavior?
			if (!GestureIsEnabled(GestureType.Tap))
				return false;

            // If the release is too far away from the press 
            // position then this cannot be a tap event.
            var dist = Vector2.Distance(touch.PressPosition, touch.Position);
            if (dist > TapJitterTolerance)
				return false;

            // If we pressed and held too long then don't 
            // generate a tap event for it.
            var elapsed = TimeSpan.FromTicks(DateTime.Now.Ticks) - touch.PressTimestamp;
            if (elapsed > _maxTicksToProcessHold)
				return false;
            
            // Check that this touch isn't the end of a previously
            // processed doubletap.
            if (_lastDoubleTapId == touch.Id)
                return false;
            
            // Store the last tap for 
            // double tap processing.          
		    _lastTap = touch;

            // Fire off the tap event immediately.
            var tap = new GestureSample(
		        GestureType.Tap, touch.Timestamp,
		        touch.Position, Vector2.Zero,
		        Vector2.Zero, Vector2.Zero);
            GestureList.Enqueue(tap);
          
			return true;
		}

        private static int _dragGestureId = -1;

		private static void ProcessDrag(TouchLocation touch)
		{
		    var dragH = GestureIsEnabled(GestureType.HorizontalDrag);
		    var dragV = GestureIsEnabled(GestureType.VerticalDrag);
		    var drag  = GestureIsEnabled(GestureType.FreeDrag);

            if (!dragH && !dragV && !drag)
				return;

            // Make sure this is a move event and that we have
            // a previous touch location.
            TouchLocation prevTouch;
            if (    touch.State != TouchLocationState.Moved ||
                    !touch.TryGetPreviousLocation(out prevTouch))
                return;
		
            var delta = touch.Position - prevTouch.Position;
					
			// Free drag takes priority over a directional one.
			var gestureType = GestureType.FreeDrag;
            if (!drag)
			{
				// Horizontal drag takes precedence over a vertical one.
                if (dragH)
				{
					// Direction delta come back with it's 'other' component set to 0.
					if (Math.Abs(delta.X) >= Math.Abs(delta.Y))
					{
						delta.Y = 0;
						gestureType = GestureType.HorizontalDrag;
					}
                    else if (dragV)
					{
						delta.X = 0;
						gestureType = GestureType.VerticalDrag;
					}
					else
						return;
				}
			}
            
            _dragGestureId = touch.Id;

			GestureList.Enqueue(new GestureSample(
                                    gestureType, touch.Timestamp,
								    touch.Position, Vector2.Zero,
								    delta, Vector2.Zero));
		}
		
		private static void ProcessPinch(TouchLocation? [] touches)
		{
			if (!GestureIsEnabled(GestureType.Pinch))
				return;
			
			var touch0 = touches[0].Value;
			var touch1 = touches[1].Value;
			
			TouchLocation prevPos0;
			TouchLocation prevPos1;
			
			if (!touch0.TryGetPreviousLocation(out prevPos0))
				prevPos0 = touch0;
			
			if (!touch1.TryGetPreviousLocation(out prevPos1))
				prevPos1 = touch1;
			
			var delta0 = touch0.Position - prevPos0.Position;
			var delta1 = touch1.Position - prevPos1.Position;

			GestureList.Enqueue (new GestureSample (
				GestureType.Pinch, 
                touch0.Timestamp > touch1.Timestamp ? touch0.Timestamp : touch1.Timestamp,                
				touch0.Position, touch1.Position,
				delta0, delta1));

            // Make sure neither touch location can fire off a hold event.
            if (!_heldEventsProcessed.Contains(touch0.Id))
                _heldEventsProcessed.Add(touch0.Id);
            if (!_heldEventsProcessed.Contains(touch1.Id))
                _heldEventsProcessed.Add(touch1.Id);
		}
		
		#endregion
    }
}