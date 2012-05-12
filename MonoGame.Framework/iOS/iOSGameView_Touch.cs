#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework {
	partial class iOSGameView {
		
		private const long _maxTicksToProcessFlick = 1500000;
		
		private const int _minVelocityToCompleteSwipe = 50;
		
		static GestureType EnabledGestures
		{
			get { return TouchPanel.EnabledGestures; }
		}
		
		private static bool GestureIsEnabled(GestureType gestureType)
		{
			return (EnabledGestures & gestureType) != 0;
		}
		
		#region Touches

		// Some gestures need to calculate deltas (e.g. Flick), so we
		// record where the most recent set of touches began and ended.
		//private readonly Vector2? [] _touchBuffer = new Vector2?[2];
		
		Vector2? _currentTouch;
		Vector2? _previousTouch;

		private void ClearTouchBuffer ()
		{
			_currentTouch = _previousTouch = null;
		}

		private void RollTouchBuffer (Vector2 position)
		{
			if (position == _previousTouch)
				return;
			
			_previousTouch = _currentTouch;
			_currentTouch = position;
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			FillTouchCollection (touches);
			GamePad.Instance.TouchesBegan (touches, evt, this);

			var location = ((UITouch) touches.AnyObject).LocationInView (this);

			ClearTouchBuffer ();
			RollTouchBuffer (GetOffsetPosition (new Vector2 (location.X, location.Y), true));
			
			updateGestures();
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			FillTouchCollection (touches);
			GamePad.Instance.TouchesEnded (touches, evt, this);

			var location = ((UITouch) touches.AnyObject).LocationInView (this);
			RollTouchBuffer (GetOffsetPosition (new Vector2 (location.X, location.Y), true));
			
			updateGestures();
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			FillTouchCollection (touches);
			GamePad.Instance.TouchesMoved (touches, evt, this);

			var location = ((UITouch) touches.AnyObject).LocationInView (this);
			RollTouchBuffer (GetOffsetPosition (new Vector2 (location.X, location.Y), true));
			
			updateGestures();
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);
			FillTouchCollection (touches);
			GamePad.Instance.TouchesCancelled (touches, evt);

			var location = ((UITouch) touches.AnyObject).LocationInView (this);
			RollTouchBuffer (GetOffsetPosition (new Vector2 (location.X, location.Y), true));
			
			updateGestures();
		}
		
		// For pinch, we'll need to "save" a touch so we can
		// send both at the same time
		private TouchLocation?[] _savedPinchTouches = new TouchLocation?[2];
		private bool _pinchComplete = false;
		
		private void updateGestures()
		{				
			for(int x = 0; x < TouchPanel.Collection.Count; x++)
			{
				var touch = TouchPanel.Collection[x];
				
				switch(touch.State)
				{
					case TouchLocationState.Pressed:					
					case TouchLocationState.Moved:
					// Any time that two fingers are detected in XNA, it's considered a pinch
					// Save the touch and combine it with the next one to create a pinch gesture
					if (GestureIsEnabled(GestureType.Pinch) &&
					    TouchPanel.Collection.Count > 1 &&
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
					else if (touch.State == TouchLocationState.Moved)
						ProcessDrag(touch);
					
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
					
						if (touch.PrevState == TouchLocationState.Pressed)
						{
							if (ProcessTap(touch))
								break;
						}
						else if (touch.PrevState == TouchLocationState.Moved)
						{
							if (ProcessFlick(touch))
								break;
						
							if (ProcessDragComplete(touch))
								break;
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

		// TODO: Review FillTouchCollection
		private void FillTouchCollection (NSSet touches)
		{
			if (touches.Count == 0)
				return;

			TouchCollection collection = TouchPanel.Collection;
			var touchesArray = touches.ToArray<UITouch> ();
			for (int i = 0; i < touchesArray.Length; ++i) {
				var touch = touchesArray [i];

				//Get position touch
				var location = touch.LocationInView (touch.View);
				var position = GetOffsetPosition (new Vector2 (location.X, location.Y), true);
				var id = touch.Handle.ToInt32 ();

				switch (touch.Phase) {
				//case UITouchPhase.Stationary:
				case UITouchPhase.Moved:
					
					TouchLocation loc;
					// Don't process a "moved" if we didn't!
					if (collection.FindIndexById(id, out loc) == -1 || loc.Position == position)
						break;
					
					collection.Update(id, TouchLocationState.Moved, position);
					
					if (i == 0) 
					{
						Mouse.State.X = (int) position.X;
						Mouse.State.Y = (int) position.Y;
					}
					break;
				case UITouchPhase.Began:
					collection.Add (id, position);
					if (i == 0) {
						Mouse.State.X = (int) position.X;
						Mouse.State.Y = (int) position.Y;
						Mouse.State.LeftButton = ButtonState.Pressed;
					}
					break;
				case UITouchPhase.Ended	:
					collection.Update (id, TouchLocationState.Released, position);

					if (i == 0) {
						Mouse.State.X = (int) position.X;
						Mouse.State.Y = (int) position.Y;
						Mouse.State.LeftButton = ButtonState.Released;
					}
					break;
				case UITouchPhase.Cancelled:
					collection.Update (id, TouchLocationState.Invalid, position);
					break;
				default:
					break;
				}
			}
		}
		
		// TODO: Review GetOffsetPosition, hopefully it can be removed now.
		public Vector2 GetOffsetPosition (Vector2 position, bool useScale)
		{
			if (useScale)
				return position * Layer.ContentsScale;
			return position;
		}

		#endregion Touches

		#region Gestures
		
		private bool ProcessTap(TouchLocation touch)
		{
			if (!GestureIsEnabled(GestureType.Tap))
				return false;
			
			// TODO: Use a timer to cancel/catch a double tap?
			if (touch.State == TouchLocationState.Released && 
			    touch.PrevState == TouchLocationState.Pressed )
			{
				TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.Tap, new TimeSpan (DateTime.Now.Ticks),
				touch.Position, Vector2.Zero,
				Vector2.Zero, Vector2.Zero));
					
				return true;
			}
			
			return false;
		}
		
		private bool ProcessDrag(TouchLocation touch)
		{
			if (!GestureIsEnabled(GestureType.HorizontalDrag) && 
			    !GestureIsEnabled(GestureType.VerticalDrag) && 
			    !GestureIsEnabled(GestureType.FreeDrag))
				return false;
			
			// If pinch is enabled, WP7 considers any 2 touches on screen a pinch.
			// Regardless of what either one is doing.
			// TODO: Move "pinch" priority higher than most.
			if (GestureIsEnabled(GestureType.Pinch) && 
			    TouchPanel.Collection.Count > 1)
				return false;
			
			// Make sure that our previous location was valid. If not, we are still
			// dragging, but we need a delta of 0.
			var prevPosition = touch.PrevPosition.LengthSquared() > 0 ? 
									touch.PrevPosition : touch.Position;
			
			var delta = touch.Position - prevPosition;
			
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
			
			
			var gestureSample = new GestureSample(
			gestureType, new TimeSpan (DateTime.Now.Ticks),
			touch.Position, Vector2.Zero,
			delta, Vector2.Zero);
			
			// TODO: iOS events come in quicker than we can process them, leading to
			// multiple gestures eventually overflowing the queue. They need to be merged,
			// but for now we are throwing away the extra inputs. needs to be fixed before shipping.
			
			//TODO: Handle this better later.
			foreach (var gesture in TouchPanel.GestureList)
			{
				if (gesture.GestureType == gestureType)
					return false;
			}
			
			TouchPanel.GestureList.Enqueue(gestureSample);
			
			return true;
			
		}
		
		private bool ProcessDragComplete(TouchLocation touch)
		{
			if (!GestureIsEnabled(GestureType.DragComplete))
				return false;
			
			if (touch.PrevState != TouchLocationState.Moved)
				return false;
			
			TouchPanel.GestureList.Enqueue (new GestureSample (
			GestureType.DragComplete, new TimeSpan (DateTime.Now.Ticks),
			Vector2.Zero, Vector2.Zero,
			Vector2.Zero, Vector2.Zero));
			
			return true;
		}
		
		private bool ProcessHold(TouchLocation touch)
		{
			if (!GestureIsEnabled(GestureType.Hold))
				return false;
			
			TouchPanel.GestureList.Enqueue (new GestureSample (
			GestureType.Tap, new TimeSpan (DateTime.Now.Ticks),
			touch.Position, Vector2.Zero,
			Vector2.Zero, Vector2.Zero));
			
			return true;
		}
		
		private bool ProcessFlick(TouchLocation touch)
		{
			if (!GestureIsEnabled(GestureType.Flick))
			    return false;
			
			if (touch.Lifetime > _maxTicksToProcessFlick)
				return false;
			
			//TODO: Get a better flick velocity. Consider using the touch buffer.		
			var velocity = (touch.Position - touch.startingPosition);
			
			if ( velocity.Length() < _minVelocityToCompleteSwipe )
				return false;
			
			//Magical hack. This was here before.
			velocity *= 8;
			
			TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.Flick, new TimeSpan (DateTime.Now.Ticks),
				Vector2.Zero, Vector2.Zero,
				velocity, Vector2.Zero));
			
			return true;
		}

		
		private bool ProcessPinch(TouchLocation? [] touches)
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
		
		private bool ProcessPinchComplete()
		{
			if (!GestureIsEnabled(GestureType.PinchComplete))
				return false;
			
			TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.PinchComplete, new TimeSpan (DateTime.Now.Ticks),
				Vector2.Zero, Vector2.Zero,
				Vector2.Zero, Vector2.Zero));
			
			return true;
			
		}

		#endregion Gestures
	}
}
