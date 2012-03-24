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
		#region Touches

		// Some gestures need to calculate deltas (e.g. Flick), so we
		// record where the most recent set of touches began and ended.
		private readonly Vector2? [] _touchBuffer = new Vector2?[2];

		private void ClearTouchBuffer ()
		{
			_touchBuffer [0] = null;
			_touchBuffer [1] = null;
		}

		private void RollTouchBuffer (Vector2 position)
		{
			_touchBuffer [0] = _touchBuffer [1];
			_touchBuffer [1] = position;
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			FillTouchCollection (touches);
			GamePad.Instance.TouchesBegan (touches, evt, this);

			var location = ((UITouch) touches.AnyObject).LocationInView (this);
			ClearTouchBuffer ();
			RollTouchBuffer (GetOffsetPosition (new Vector2 (location.X, location.Y), true));
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			FillTouchCollection (touches);
			GamePad.Instance.TouchesEnded (touches, evt, this);

			var location = ((UITouch) touches.AnyObject).LocationInView (this);
			RollTouchBuffer (GetOffsetPosition (new Vector2 (location.X, location.Y), true));
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			FillTouchCollection (touches);
			GamePad.Instance.TouchesMoved (touches, evt, this);

			var location = ((UITouch) touches.AnyObject).LocationInView (this);
			RollTouchBuffer (GetOffsetPosition (new Vector2 (location.X, location.Y), true));
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);
			FillTouchCollection (touches);
			GamePad.Instance.TouchesCancelled (touches, evt);

			var location = ((UITouch) touches.AnyObject).LocationInView (this);
			RollTouchBuffer (GetOffsetPosition (new Vector2 (location.X, location.Y), true));
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
				case UITouchPhase.Stationary:
				case UITouchPhase.Moved:
					collection.Update (id, TouchLocationState.Moved, position);

					if (i == 0) {
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

		private readonly GestureRecognizerDelegate _gestureRecognizerDelegate = new GestureRecognizerDelegate ();
		private readonly Dictionary<GestureType, UIGestureRecognizer []> _gestureRecognizers =
			new Dictionary<GestureType, UIGestureRecognizer []> ();

		private void TouchPanel_EnabledGesturesChanged (object sender, EventArgs e)
		{
			SyncTouchRecognizers ();
		}

		private void SyncTouchRecognizers()
		{
			foreach (var entry in _gestureRecognizers) {
				foreach (var recognizer in entry.Value) {
					RemoveGestureRecognizer (recognizer);
				}
			}

			_gestureRecognizers.Clear ();
			foreach (GestureType gestureType in Enum.GetValues(typeof(GestureType))) {
				if (gestureType == GestureType.None)
					continue;

				UIGestureRecognizer [] recognizers;

				if ((gestureType & TouchPanel.EnabledGestures) == 0)
					continue;

				recognizers = CreateGestureRecognizer (gestureType);
				if (recognizers != null) {
					_gestureRecognizers.Add (gestureType, recognizers);
					foreach (var recognizer in recognizers) {
						recognizer.Delegate = _gestureRecognizerDelegate;
						AddGestureRecognizer (recognizer);
					}
				}
			}

			// TODO: A more flexible system for determining
			//       require-fail dependencies may be desirable.
			UIGestureRecognizer[] singleTapRecognizer;
			UIGestureRecognizer[] doubleTapRecognizer;
			if (_gestureRecognizers.TryGetValue(GestureType.Tap, out singleTapRecognizer) &&
			    _gestureRecognizers.TryGetValue(GestureType.DoubleTap, out doubleTapRecognizer)) {
				singleTapRecognizer[0].RequireGestureRecognizerToFail (doubleTapRecognizer[0]);
			}
		}

		private UIGestureRecognizer[] CreateGestureRecognizer (GestureType gestureType)
		{
			switch (gestureType) {
			case GestureType.DoubleTap:
				return new UIGestureRecognizer[] {
					new UITapGestureRecognizer (this, new Selector ("OnDoubleTapGesture")) {
						CancelsTouchesInView = false,
						NumberOfTapsRequired = 2
					}
				};
			case GestureType.Flick:
				return new UIGestureRecognizer[] {
					new UISwipeGestureRecognizer (this, new Selector ("OnSwipeGesture")) {
						CancelsTouchesInView = false,
						Direction =
							UISwipeGestureRecognizerDirection.Left |
							UISwipeGestureRecognizerDirection.Right
					},
					new UISwipeGestureRecognizer (this, new Selector ("OnSwipeGesture")) {
						CancelsTouchesInView = false,
						Direction =
							UISwipeGestureRecognizerDirection.Up |
							UISwipeGestureRecognizerDirection.Down
					}
				};
			case GestureType.FreeDrag:
				return new UIGestureRecognizer[] {
					new UIPanGestureRecognizer (this, new Selector ("OnPanGesture")) {
						CancelsTouchesInView = false
					}
				};
			case GestureType.Hold:
				return new UIGestureRecognizer[] {
					new UILongPressGestureRecognizer (this, new Selector ("OnLongPressGesture")) {
						CancelsTouchesInView = false,
						MinimumPressDuration = 1.0
					}
				};
			case GestureType.Pinch:
				return new UIGestureRecognizer[] {
					new UIPinchGestureRecognizer (this, new Selector ("OnPinchGesture")) {
						CancelsTouchesInView = false
					}
				};
			case GestureType.Rotation:
				return new UIGestureRecognizer[] {
					new UIRotationGestureRecognizer (this, new Selector ("OnRotationGesture")) {
						CancelsTouchesInView = false
					}
				};
			case GestureType.Tap:
				return new UIGestureRecognizer[] {
					new UITapGestureRecognizer (this, new Selector ("OnTapGesture")) {
						CancelsTouchesInView = false,
						NumberOfTapsRequired = 1
					}
				};
			// FIXME: Support these GestureTypes
			case GestureType.DragComplete:
			case GestureType.HorizontalDrag:
			case GestureType.PinchComplete:
			case GestureType.VerticalDrag:
			default:
#if DEBUG
				Console.WriteLine (
					"Warning: Failed to create gesture recognizer of type {0}.",
					gestureType);
#endif
				return null;
			}
		}

		[Export]
		public void OnLongPressGesture (UILongPressGestureRecognizer sender)
		{
			// FIXME: Determine the appropriate action to take here.  The XNA
			//        docs say, "This is a single event, and not continuously
			//        generated while the user is holding the touchpoint."
			//        However, iOS generates Began for that condition, then zero
			//        or more Changed notifications, and then one of the final-
			//        state notifications (Recognized, Failed, etc)
			if (sender.State == UIGestureRecognizerState.Began) {
				var location = sender.LocationInView (sender.View);
				var position = GetOffsetPosition (new Vector2 (location.X, location.Y), true);
				TouchPanel.GestureList.Enqueue (new GestureSample (
					GestureType.Hold, new TimeSpan (DateTime.Now.Ticks),
					position, position,
					Vector2.Zero, Vector2.Zero));
			}
		}

		private Vector2? _previousPanPosition;

		[Export]
		public void OnPanGesture (UIPanGestureRecognizer sender)
		{
			var location = sender.LocationInView (sender.View);
			var position = GetOffsetPosition (new Vector2 (location.X, location.Y), true);

			var delta = position - _previousPanPosition.GetValueOrDefault (position);

			if (sender.State == UIGestureRecognizerState.Ended ||
			    sender.State == UIGestureRecognizerState.Cancelled ||
			    sender.State == UIGestureRecognizerState.Failed) {
				TouchPanel.GestureList.Enqueue (new GestureSample (
					GestureType.DragComplete, new TimeSpan (DateTime.Now.Ticks),
					position, Vector2.Zero,
					delta, Vector2.Zero));

				_previousPanPosition = null;
			} else {
				TouchPanel.GestureList.Enqueue (new GestureSample (
					GestureType.FreeDrag, new TimeSpan (DateTime.Now.Ticks),
					position, Vector2.Zero,
					delta, Vector2.Zero));
				_previousPanPosition = position;
			}
		}

		private readonly Vector2? [] _previousPinchPositions = new Vector2?[2];

		[Export]
		public void OnPinchGesture (UIPinchGestureRecognizer sender)
		{
			var location0 = sender.LocationOfTouch (0, sender.View);
			var position0 = GetOffsetPosition (new Vector2 (location0.X, location0.Y), true);

			PointF location1;
			Vector2 position1;
			if (sender.NumberOfTouches > 1) {
				location1 = sender.LocationOfTouch (1, sender.View);
				position1 = GetOffsetPosition (new Vector2 (location1.X, location1.Y), true);
			} else {
				location1 = location0;
				position1 = position0;
			}

			var delta0 = position0 - _previousPinchPositions [0].GetValueOrDefault (position0);
			var delta1 = position1 - _previousPinchPositions [1].GetValueOrDefault (position1);

			TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.Pinch, new TimeSpan (DateTime.Now.Ticks),
				position0, position1,
				delta0, delta1));

			if (sender.State == UIGestureRecognizerState.Ended ||
			    sender.State == UIGestureRecognizerState.Cancelled ||
			    sender.State == UIGestureRecognizerState.Failed) {
				_previousPinchPositions [0] = null;
				_previousPinchPositions [1] = null;
			} else {
				_previousPinchPositions [0] = position0;
				_previousPinchPositions [1] = position1;
			}
		}

		[Export]
		public void OnRotationGesture (UIRotationGestureRecognizer sender)
		{
			var location0 = sender.LocationOfTouch (0, sender.View);
			var position0 = GetOffsetPosition (new Vector2(location0.X, location0.Y), true);

			var location1 = sender.LocationOfTouch (1, sender.View);
			var position1 = GetOffsetPosition (new Vector2(location1.X, location1.Y), true);

			TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.Rotation, new TimeSpan (DateTime.Now.Ticks),
				position0, position1,
				Vector2.Zero, Vector2.Zero));
		}

		[Export]
		public void OnSwipeGesture (UISwipeGestureRecognizer sender)
		{
			// FIXME: It may not be possible to use
			//        UISwipeGestureRecognizer to correctly
			//        implement GestureType.Flick.  I certainly
			//        haven't had any luck and have resorted to
			//        magic constants here.
			var delta = 8 * (_touchBuffer [1].Value - _touchBuffer [0].Value);

			TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.Flick, new TimeSpan (DateTime.Now.Ticks),
				Vector2.Zero, Vector2.Zero,
				delta, Vector2.Zero));
		}

		[Export]
		public void OnTapGesture (UITapGestureRecognizer sender)
		{
			var location = sender.LocationInView (sender.View);
			var position = GetOffsetPosition (new Vector2 (location.X, location.Y), true);
			TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.Tap, new TimeSpan (DateTime.Now.Ticks),
				position, Vector2.Zero,
				Vector2.Zero, Vector2.Zero));
		}

		[Export]
		public void OnDoubleTapGesture (UITapGestureRecognizer sender)
		{
			var location = sender.LocationInView (sender.View);
			var position = GetOffsetPosition (new Vector2 (location.X, location.Y), true);
			TouchPanel.GestureList.Enqueue (new GestureSample (
				GestureType.DoubleTap, new TimeSpan (DateTime.Now.Ticks),
				position, Vector2.Zero,
				Vector2.Zero, Vector2.Zero));
		}

		// FIXME: The behavior of GestureRecognizerDelegate will almost
		//        certainly need to be adjusted.
		private class GestureRecognizerDelegate : UIGestureRecognizerDelegate {
			public override bool ShouldBegin (UIGestureRecognizer recognizer)
			{
				return true;
			}

			public override bool ShouldReceiveTouch (UIGestureRecognizer recognizer, UITouch touch)
			{
				return true;
			}

			public override bool ShouldRecognizeSimultaneously (
				UIGestureRecognizer gestureRecognizer,
				UIGestureRecognizer otherGestureRecognizer)
			{
				if (gestureRecognizer is UILongPressGestureRecognizer &&
				    otherGestureRecognizer is UITapGestureRecognizer)
					return false;

				if (gestureRecognizer is UITapGestureRecognizer &&
				    otherGestureRecognizer is UILongPressGestureRecognizer)
					return false;

				return true;
			}
		}

		#endregion Gestures
	}
}
