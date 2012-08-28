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
using MonoGame.Framework.Touch;

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
        /// The currently touch state.
        /// </summary>
        private static List<TouchLocation> _touchLocations = new List<TouchLocation>();
        private static List<TouchLocation> _unhandledTouchLocations = new List<TouchLocation>();

        /// <summary>
        /// The touch events to be processed and added to the current state.
        /// </summary>
        private static List<TouchLocation> _events = new List<TouchLocation>();

        /// <summary>
        /// The current touch state.
        /// </summary>
        private static TouchCollection _state = new TouchCollection();

        /// <summary>
        /// The positional scale to apply to touch input.
        /// </summary>
        private static Vector2 _touchScale = Vector2.One;

        /// <summary>
        /// The current size of the display.
        /// </summary>
        private static Point _displaySize = Point.Zero;

        internal static event EventHandler<EventArgs> DisplaySizeChanged;
        internal static Queue<GestureSample> GestureList = new Queue<GestureSample>();
		internal static event EventHandler EnabledGesturesChanged;
        internal static TouchPanelCapabilities Capabilities = new TouchPanelCapabilities();

        static TouchPanel()
        {
            TouchPanel.DisplaySizeChanged += UpdateTouchScale;
        }

        public static TouchPanelCapabilities GetCapabilities()
        {
            Capabilities.Initialize();
            return Capabilities;
        }

        private static void RefreshState()
        {
            // Remove the previously released touch locations.
            for (int i = 0; i < _touchLocations.Count;)
            {
                if (_touchLocations[i].State == TouchLocationState.Released)
                {
                    _heldEventsProcessed.Remove(_touchLocations[i].Id);
                    _processedDoubleTaps.Remove(_touchLocations[i].Id);
                    _touchLocations.RemoveAt(i);
                }
                else
                    i++;
            }

            // Update the existing touch locations.
            for (int i = 0; i < _touchLocations.Count; i++)
            {
                TouchLocation prevTouch = _touchLocations[i];
                TouchLocation? nextTouch = null;
                for (int j = 0; j < _events.Count; j++)
                {
                    if (_events[j].Id == prevTouch.Id)
                    {
                        nextTouch = _events[j];
                        // Remove an event. If we hit multiple events for the same ID we remove them all, as we only need the last one for processing.
                        _events.RemoveAt(j--);
                    }
                }

                if (nextTouch.HasValue)
                {
                    // Set the new touch location state.
                    _touchLocations[i] = new TouchLocation(nextTouch.Value.Id,
                                                                nextTouch.Value.State, nextTouch.Value.Position, nextTouch.Value.Pressure,
                                                                prevTouch.State, prevTouch.Position, prevTouch.Pressure, prevTouch.TouchHistory);
                }
                else
                {
                    _touchLocations[i] = new TouchLocation(prevTouch.Id,
                                                                TouchLocationState.Moved, prevTouch.Position, prevTouch.Pressure,
                                                                prevTouch.State, prevTouch.Position, prevTouch.Pressure, prevTouch.TouchHistory);
                }
            }

            // Add any new pressed events.
            for (var i = 0; i < _events.Count; i++)
            {
                var loc = _events[i];
                if (loc.State == TouchLocationState.Pressed)
                {
                    _touchLocations.Add(loc);
                    loc.touchHistory = new TouchInfo(loc.Id, loc.Position);
                    _events.RemoveAt(i--);
                }
            }

            // Add the touchlocations to the list of unhandled touchlocations
            _unhandledTouchLocations.AddRange(_touchLocations.ToArray());

            //Update our gestures
            UpdateGestures();
        }

        public static TouchCollection GetState()
        {
            RefreshState();
            
            _state = new TouchCollection(_unhandledTouchLocations.ToArray());
            _unhandledTouchLocations.Clear();
            return _state;
        }
        
        internal static void AddEvent(int id, TouchLocationState state, Vector2 position)
        {
#if WINRT
            // TODO:
            //
            // As of 6/20/2012 the WinRT Simulator will generate duplicate
            // touch location ids... filter these out for now.
            //
            // Lets be sure to remove this once that is fixed... this safety
            // here is inefficient and unnessasary.
            //
            if (    state == TouchLocationState.Pressed &&
                    _events.FindIndex(e => e.Id == id) != -1)
                return;
#endif

            // Apply the touch scale to the new location.
            _events.Add(new TouchLocation(id, state, position * _touchScale));
        }

        internal static void UpdateTouchScale (object sender, EventArgs args)
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

        internal static void UpdateState()
        {
            // This function should be removed now that it is no longer needed
            RefreshState();
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
                    DisplaySizeChanged(null, EventArgs.Empty);
                }
            }
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
                    DisplaySizeChanged(null, EventArgs.Empty);
                }
            }
        }

        public static DisplayOrientation DisplayOrientation { get; set; }
		
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
                // We update the gesture list so we don't have to update when a gesture is read
                RefreshState();

				return ( GestureList.Count > 0 );				
            }
        }
		
		#region Gesture Recognition
		
        // Tolerance to prevent small movements from cancelling a touch/held recognition.
        private const int _tapJitterTolerance = 5;
        private static readonly TimeSpan _flickMovementThreshold = TimeSpan.FromMilliseconds(55);
		private const long _maxTicksToProcessHold = 10250000;
		private const long _maxTicksToProcessDoubleTap = 1300000;
		private const int _minVelocityToCompleteSwipe = 15;
		
		// For pinch, we'll need to "save" a touch so we can
		// send both at the same time
		private static TouchLocation?[] _savedPinchTouches = new TouchLocation?[2];
		private static bool _pinchComplete = false;
		private static TouchLocation _previousTouchLoc;
		
		private static bool GestureIsEnabled(GestureType gestureType)
		{
			return (EnabledGestures & gestureType) != 0;
		}

        static GestureSample? _tempTap; // Use a member variable rather than initialize a new GestureSample every time this is called.

		private static void UpdateGestures()
		{			
            var touchLocations = _touchLocations;
			for (int x = 0; x < touchLocations.Count; x++)
			{
				var touch = touchLocations[x];
				
				switch(touch.State)
				{
					case TouchLocationState.Pressed:
					case TouchLocationState.Moved:
                    
                    if(touch.State == TouchLocationState.Pressed)
                    {
                        if (ProcessDoubleTap(touch))
                            break;
                    }
                    
					// Any time that two fingers are detected in XNA, it's considered a pinch
					// Save the touch and combine it with the next one to create a pinch gesture
					if (GestureIsEnabled(GestureType.Pinch) &&
					    touchLocations.Count > 1 &&
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
					
					if (touch.State == TouchLocationState.Moved)
                    {
                        if (touch.TouchHistory.TotalDistanceMoved < _tapJitterTolerance)
						    ProcessHold(touch);
					    else
						    ProcessDrag(touch);
                    }
					
					break;
					
					// Released. Can be a tap/Doubletap, or the end of a
					// previous gesture.
					case TouchLocationState.Released:
					case TouchLocationState.Invalid:
						if (_savedPinchTouches[0] != null && 
					    	_savedPinchTouches[1] != null &&
					    	(touch.Id == _savedPinchTouches[0].Value.Id ||
						    touch.Id == _savedPinchTouches[1].Value.Id))
						{
							ProcessPinchComplete();
							_pinchComplete = true;
							break;
						}
						else
						{
                            // Once a drag has began, that touch can no longer
                            // generate a tap event.
                            if (!_processedDrags.Contains(touch.Id))
                            {
                                if (ProcessTap(touch))
                                    break;
                            }						
							if (ProcessFlick(touch))
								break;

                            if (ProcessDragComplete(touch))
                                break;
						}
						break;
					
				}
			}
			
			// Fire off any tap gestures that are ready.
			for (int x = 0; x < _pendingTaps.Count;)
			{
				_tempTap = _pendingTaps[x];
				
				if (DateTime.Now.Ticks - _tempTap.Value.Timestamp.Ticks >= _maxTicksToProcessDoubleTap)
				{
					GestureList.Enqueue(_tempTap.Value);
					_pendingTaps.RemoveAt(x);
					continue;
				}
				
				x++;
			}
			
			if (_pinchComplete)
			{
				_savedPinchTouches[0] = _savedPinchTouches[1] = null;
				_pinchComplete = false;
			}
		}
		
		static List<int> _heldEventsProcessed = new List<int>();

		private static bool ProcessHold(TouchLocation touch)
		{
			if (!GestureIsEnabled(GestureType.Hold))
				return false;
			
			if (touch.TouchHistory.Lifetime.Ticks < _maxTicksToProcessHold)
				return false;
			
			// Only a single held event gets sent per touch.
			// Make sure we only send one.
			if (_heldEventsProcessed.Contains(touch.Id))
				return false;
			
			_heldEventsProcessed.Add(touch.Id);
			
			TouchPanel.GestureList.Enqueue(
                new GestureSample(  GestureType.Hold, 
                                    TimeSpan.FromTicks(DateTime.Now.Ticks),
			                        touch.Position, Vector2.Zero,
			                        Vector2.Zero, Vector2.Zero));
			
			return true;
				
		}
        
        static List<int> _processedDoubleTaps = new List<int>();

        private static bool ProcessDoubleTap(TouchLocation touch)
        {
            if (!GestureIsEnabled(GestureType.DoubleTap))
                return false;
            
            // Another tap must be pending in order to properly
            // process a double tap.
            if (_pendingTaps.Count == 0)
                return false;
            
            // Using a member variable rather than initializing a new GestureSample each call.
            _tempTap = null;
            foreach (var tap in _pendingTaps)
            {
                var diff = tap.Position - touch.Position;
                
                // Check that the distance between the two isn't too great.
                if (diff.Length() > 35)
                    continue;
                
                // Also check that this tap happened within a certain threshold
                var lifetime = touch.TouchHistory.Lifetime - tap.Timestamp;
                if (lifetime.Ticks > _maxTicksToProcessDoubleTap)
                    continue;
                
                //Otherwise, we are ready to "convert" this tap into a doubletap.
                _tempTap = tap;
            }
            
            // Check that the test passed
            if (_tempTap == null)
                return false;
            
            // Remove/cancel the original tap event.
            _pendingTaps.Remove(_tempTap.Value);
            
            // "Replace" it with a doubletap.
            TouchPanel.GestureList.Enqueue(new GestureSample(
                           GestureType.DoubleTap, new TimeSpan (DateTime.Now.Ticks),
                           touch.Position, Vector2.Zero,
                           Vector2.Zero, Vector2.Zero));
            
            // This touch will eventually enter a "released" state. When it does, because
            // it was part of a doubletap, we don't want to process a third tap at the end.
            // Save it's ID to prevent this from showing up in the future.
            _processedDoubleTaps.Add(touch.Id);
            
            return true;
        }

        static List<GestureSample> _pendingTaps = new List<GestureSample>();

		private static bool ProcessTap(TouchLocation touch)
		{
            // TODO: This check means that double taps won't work unless
            // the tap event is enabled as well... is this correct behavior?
			if (!GestureIsEnabled(GestureType.Tap))
				return false;

            if (touch.TouchHistory.TotalDistanceMoved > _tapJitterTolerance)
				return false;

			if (touch.TouchHistory.Lifetime.Ticks > _maxTicksToProcessHold)
				return false;
            
            // Check that this touch isn't the end of a previously
            // processed doubletap.
            if (_processedDoubleTaps.Contains(touch.Id))
                return false;

            // Queue up the tap. Because of how DoubleTap works, we can't
            // just send this off to the GesturePanel right away.
			_pendingTaps.Add(new GestureSample (
			GestureType.Tap, new TimeSpan (DateTime.Now.Ticks),
			touch.Position, Vector2.Zero,
			Vector2.Zero, Vector2.Zero));
					
			return true;
		}

        static List<int> _processedDrags = new List<int>();

		private static bool ProcessDrag(TouchLocation touch)
		{
			if (touch.State != TouchLocationState.Moved)
				return false;
			
			if (!GestureIsEnabled(GestureType.HorizontalDrag) && 
			    !GestureIsEnabled(GestureType.VerticalDrag) && 
			    !GestureIsEnabled(GestureType.FreeDrag))
				return false;
			
			// Make sure that our previous location was valid. If not, we are still
			// dragging, but we need a delta of 0.
			var prevPosition = touch.TryGetPreviousLocation(out _previousTouchLoc) ? _previousTouchLoc.Position : touch.Position;
			var delta = touch.Position - prevPosition;
			
			// TODO: Find XNA's drag tolerance.
			if (delta == Vector2.Zero)
				return false;
			
			// Free drag takes priority over a directional one.
			GestureType gestureType = GestureType.FreeDrag;
			if (!GestureIsEnabled(GestureType.FreeDrag))
			{
				// Horizontal drag takes precedence over a vertical one.
				if (GestureIsEnabled(GestureType.HorizontalDrag))
				{
					// Direction delta come back with it's 'other' component set to 0.
					if (Math.Abs(delta.X) >= Math.Abs(delta.Y))
					{
						delta.Y = 0;
						gestureType = GestureType.HorizontalDrag;
					}
					else if (GestureIsEnabled(GestureType.VerticalDrag))
					{
						delta.X = 0;
						gestureType = GestureType.VerticalDrag;
					}
					else
						return false;
				}
			}	
			
            // Save this ID as having processed a drag so that its release will never be processed
            // as a flick or tap.
            if(!_processedDrags.Contains(touch.Id))
                _processedDrags.Add(touch.Id);

			TouchPanel.GestureList.Enqueue(new GestureSample(
										   gestureType, new TimeSpan (DateTime.Now.Ticks),
										   touch.Position, Vector2.Zero,
										   delta, Vector2.Zero));
			
			return true;
			
		}
		
		private static bool ProcessDragComplete(TouchLocation touch)
		{
			if (!GestureIsEnabled(GestureType.DragComplete))
				return false;
			
			if (!touch.TryGetPreviousLocation(out _previousTouchLoc) || 
			    _previousTouchLoc.State != TouchLocationState.Moved)
				return false;
			
			TouchPanel.GestureList.Enqueue (new GestureSample (
			GestureType.DragComplete, new TimeSpan (DateTime.Now.Ticks),
			Vector2.Zero, Vector2.Zero,
			Vector2.Zero, Vector2.Zero));

            // Remove this touch id from the list of processed drags.
            _processedDrags.Remove(touch.Id);
			
			return true;
		}

		private static bool ProcessFlick(TouchLocation touch)
		{
			if (!GestureIsEnabled(GestureType.Flick))
			    return false;
            
            // Attempt to get a "total distance traveled" by the last couple of updates.
            var prevPositions = touch.TouchHistory.PreviousPositions;
            var totalDistance = 0.0f;
            var counter = 0;
            var averageDirection = Vector2.Zero;
            var prevDirection = Vector2.Zero;

            // Check our last couple of positions and see if we are good to build
            // a flick gesture from the input
            for (int x = prevPositions.Count - 1; x != 0; x--)
            {
                // Check that this touch happened within an appropriate amount of time of the last.
                // if not, break early because the other touches will only be older.
                var timeOfTouch = prevPositions[x-1].Item2;

                var difTime = prevPositions[x].Item2 - timeOfTouch;
                if (difTime > _flickMovementThreshold)
                    break;

                var distanceBetweenVecs = prevPositions[x].Item1 - prevPositions[x-1].Item1;

                // Check that the angle between them isn't too great. Only generate flicks from
                // smooth, continuous motions.
                if (prevDirection != Vector2.Zero)
                {
                    var angle = (float)Math.Abs(Math.Acos(Vector2.Dot(Vector2.Normalize(prevDirection), Vector2.Normalize(distanceBetweenVecs))));
                    if (angle > Math.PI / 4)
                        break;
                }

                averageDirection += distanceBetweenVecs;
                prevDirection = distanceBetweenVecs;
                totalDistance += distanceBetweenVecs.Length();
                counter++;
            }

            // Bail early if we didn't find any valid touches to build a flick..
            if (counter == 0)
                return false;

            // Check that the "average distance per move" is enough to pass the threshold.
            var avgDistance = totalDistance / counter;
			if ( avgDistance < _minVelocityToCompleteSwipe )
                return false;
			
			//TODO: Calculate a better flick velocity
			var fakeVelocity = Vector2.Normalize(averageDirection) * avgDistance * 20;
			
			TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.Flick, new TimeSpan (DateTime.Now.Ticks),
				Vector2.Zero, Vector2.Zero,
				fakeVelocity, Vector2.Zero));
			
			return true;
		}
		
		private static bool ProcessPinch(TouchLocation? [] touches)
		{
			if (!GestureIsEnabled(GestureType.Pinch))
				return false;
			
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
			
			TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.Pinch, new TimeSpan (DateTime.Now.Ticks),
				touch0.Position, touch1.Position,
				delta0, delta1));
			
			return true;
		}
		
		private static bool ProcessPinchComplete()
		{
			if (!GestureIsEnabled(GestureType.PinchComplete))
				return false;
			
			TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.PinchComplete, new TimeSpan (DateTime.Now.Ticks),
				Vector2.Zero, Vector2.Zero,
				Vector2.Zero, Vector2.Zero));
			
			return true;
			
		}

		#endregion
    }
}