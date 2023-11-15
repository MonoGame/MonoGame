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

using Foundation;
using ObjCRuntime;
using UIKit;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework {
	partial class iOSGameView {
		
		static GestureType EnabledGestures
		{
			get { return TouchPanel.EnabledGestures; }
		}
		
		#region Touches

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			FillTouchCollection (touches, evt);
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			FillTouchCollection (touches, evt);
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			FillTouchCollection (touches, evt);
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);
			FillTouchCollection (touches, evt);
		}
		
		// Process and fill touch events - coalesced if available, otherwise, use the last-frame.
		private void FillTouchCollection(NSSet touches, UIEvent evt)
		{
			if (touches.Count == 0)
				return;

			var touchesArray = touches.ToArray<UITouch> ();
			for (int touchIndex = 0; touchIndex < touchesArray.Length; ++touchIndex)
			{
					var touch = touchesArray[touchIndex];
					var id = touch.Handle.GetHashCode();
					FillTouch(touch, id, false);
					var coalescedTouches = evt.GetCoalescedTouches(touch);
					if (coalescedTouches != null)
					{
							// Per the document https://developer.apple.com/documentation/uikit/uievent/1613808-coalescedtouches,
							// there may be a few coalesced touch events between two subsequence frames. The frequence of these
							// events is perhaps more than the display frequency, and perhaps max out at 240Hz for Apple Pencil
							// and so on.
							for (int coalescedIndex = 0; coalescedIndex < coalescedTouches.Length; ++coalescedIndex)
							{
									FillTouch(coalescedTouches[coalescedIndex], id, true);
							}
					}

			}
	}

	private void FillTouch(UITouch touch, int id, bool coalesced)
	{
		//Get position touch
		var location = touch.LocationInView (touch.View);
		var position = GetOffsetPosition (new Vector2 ((float)location.X, (float)location.Y), true);
		switch (touch.Phase) 
		{
				case UITouchPhase.Stationary:
					if (coalesced)
							TouchPanel.AddCoalescedEvent(id, TouchLocationState.Moved, position);
					break;
				case UITouchPhase.Moved:
					if (coalesced)
							TouchPanel.AddCoalescedEvent(id, TouchLocationState.Moved, position);
					else
						TouchPanel.AddEvent(id, TouchLocationState.Moved, position);					
						break;
				case UITouchPhase.Began:
					if (coalesced)
						TouchPanel.AddCoalescedEvent(id, TouchLocationState.Pressed, position);
					else
						TouchPanel.AddEvent(id, TouchLocationState.Pressed, position);
					break;
				case UITouchPhase.Ended	:
					if (coalesced)
							TouchPanel.AddCoalescedEvent(id, TouchLocationState.Released, position);
					else
						TouchPanel.AddEvent(id, TouchLocationState.Released, position);
					break;
				case UITouchPhase.Cancelled:
					if (coalesced)
							TouchPanel.AddCoalescedEvent(id, TouchLocationState.Released, position);
					else
						TouchPanel.AddEvent(id, TouchLocationState.Released, position);
					break;
				default:
					break;
			}
		}
		
		// UI touch events are returned in content space while MonoGame uses screen-space pixel coordinates.
		public Vector2 GetOffsetPosition (Vector2 position, bool useScale)
		{
			if (useScale)
				return position * (float)Layer.ContentsScale;
			return position;
		}

		#endregion Touches
	}
}
