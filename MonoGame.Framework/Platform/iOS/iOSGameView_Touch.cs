// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Drawing;

using Foundation;
using ObjCRuntime;
using UIKit;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    partial class iOSGameView
    {

        static GestureType EnabledGestures
        {
            get { return TouchPanel.EnabledGestures; }
        }

        #region Touches

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            FillTouchCollection(touches, evt);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            FillTouchCollection(touches, evt);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            FillTouchCollection(touches, evt);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            FillTouchCollection(touches, evt);
        }

        // Process and fill touch events: process high-frequency events if they are available, otherwise,
        // use the last-frame touch events.
        private void FillTouchCollection(NSSet touches, UIEvent evt)
        {
            if (touches.Count == 0)
                return;

            var touchesArray = touches.ToArray<UITouch>();
            for (int touchIndex = 0; touchIndex < touchesArray.Length; ++touchIndex)
            {
                var touch = touchesArray[touchIndex];
                var id = touch.Handle.GetHashCode();
                FillTouch(touch, id, false);

                if (TouchPanel.EnableHighFrequencyTouch)
                {
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
        }

        private void FillTouch(UITouch touch, int id, bool coalesced)
        {
            //Get position touch
            var location = touch.LocationInView(touch.View);
            var position = GetOffsetPosition(new Vector2((float)location.X, (float)location.Y), true);
            switch (touch.Phase)
            {
            case UITouchPhase.Stationary:
                if (coalesced)
                    TouchPanel.AddHighResolutionTouchEvent(id, TouchLocationState.Moved, position);
                break;
            case UITouchPhase.Moved:
                if (coalesced)
                    TouchPanel.AddHighResolutionTouchEvent(id, TouchLocationState.Moved, position);
                else
                    TouchPanel.AddEvent(id, TouchLocationState.Moved, position);
                break;
            case UITouchPhase.Began:
                if (coalesced)
                    TouchPanel.AddHighResolutionTouchEvent(id, TouchLocationState.Pressed, position);
                else
                    TouchPanel.AddEvent(id, TouchLocationState.Pressed, position);
                break;
            case UITouchPhase.Ended:
                if (coalesced)
                    TouchPanel.AddHighResolutionTouchEvent(id, TouchLocationState.Released, position);
                else
                    TouchPanel.AddEvent(id, TouchLocationState.Released, position);
                break;
            case UITouchPhase.Cancelled:
                if (coalesced)
                    TouchPanel.AddHighResolutionTouchEvent(id, TouchLocationState.Released, position);
                else
                    TouchPanel.AddEvent(id, TouchLocationState.Released, position);
                break;
            default:
                break;
            }
        }

        // UI touch events are returned in content space while MonoGame uses screen-space pixel coordinates.
        public Vector2 GetOffsetPosition(Vector2 position, bool useScale)
        {
            if (useScale)
                return position * (float)Layer.ContentsScale;
            return position;
        }

        #endregion Touches
    }
}
