#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Microsoft.Xna.Framework
{
    class MetroFrameworkElementEvents
    {
        private readonly List<Keys> _keys = new List<Keys>();

        private FrameworkElement _element;

        public MetroFrameworkElementEvents(FrameworkElement element)
        {
            _element = element;

            // The key events are tied to the window as those will
            // only arrive here if some other control hasn't gotten it.
            Window.Current.CoreWindow.KeyDown += Keyboard_KeyDown;
            Window.Current.CoreWindow.KeyUp += Keyboard_KeyUp;

            // All the other events have to come from the framework element
            // if not we will get events that occured on overlapping controls.
            element.PointerPressed += CoreWindow_PointerPressed;
            element.PointerReleased += CoreWindow_PointerReleased;
            element.PointerMoved += CoreWindow_PointerMoved;
            element.PointerWheelChanged += CoreWindow_PointerWheelChanged;
        }

        public void UpdateState()
        {
            // Update the keyboard state.
            Keyboard.State = new KeyboardState(_keys.ToArray());

            // Update the touch state.
            TouchPanel.UpdateState();
        }

        private void CoreWindow_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            // Wheel events always go to the mouse state.
            Mouse.State.Update(args);
        }

        private void CoreWindow_PointerMoved(object sender, PointerRoutedEventArgs args)
        {
            var currentPoint = args.GetCurrentPoint(null);

            // To convert from DIPs (that all touch and pointer events are returned 
            // in) to pixels (that XNA APIs expect)
            var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
            var pos = new Vector2((float)currentPoint.Position.X, (float)currentPoint.Position.Y) * dipFactor;

            var isTouch = currentPoint.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            var touchIsDown = currentPoint.IsInContact;
            if (isTouch && touchIsDown)
                TouchPanel.AddEvent(new TouchLocation((int)currentPoint.PointerId, TouchLocationState.Moved, pos));

            if (!isTouch || currentPoint.Properties.IsPrimary && touchIsDown)
            {
                // Mouse or stylus event or the primary touch event (simulated as mouse input)
                Mouse.State.Update(args);
            }
        }

        private void CoreWindow_PointerReleased(object sender, PointerRoutedEventArgs args)
        {
			// Release the captured pointer.
            _element.ReleasePointerCapture(args.Pointer);

            var currentPoint = args.GetCurrentPoint(null);

            // To convert from DIPs (that all touch and pointer events are returned 
            // in) to pixels (that XNA APIs expect)
            var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
            var pos = new Vector2((float)currentPoint.Position.X, (float)currentPoint.Position.Y) * dipFactor;

            var isTouch = currentPoint.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            if (isTouch)
                TouchPanel.AddEvent(new TouchLocation((int)currentPoint.PointerId, TouchLocationState.Released, pos));

            if (!isTouch || currentPoint.Properties.IsPrimary)
            {
                // Mouse or stylus event or the primary touch event (simulated as mouse input)
                Mouse.State.Update(args);
            }
        }

        private void CoreWindow_PointerPressed(object sender, PointerRoutedEventArgs args)
        {
			// Capture future pointer events until a release.		
            _element.CapturePointer(args.Pointer);

            var currentPoint = args.GetCurrentPoint(null);

            // To convert from DIPs (that all touch and pointer events are returned 
            // in) to pixels (that XNA APIs expect)
            var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
            var pos = new Vector2((float)currentPoint.Position.X, (float)currentPoint.Position.Y) * dipFactor;

            var isTouch = currentPoint.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            if (isTouch)
                TouchPanel.AddEvent(new TouchLocation((int)currentPoint.PointerId, TouchLocationState.Pressed, pos));

            if (!isTouch || currentPoint.Properties.IsPrimary)
            {
                // Mouse or stylus event or the primary touch event (simulated as mouse input)
                Mouse.State.Update(args);
            }
        }
        
        private static Keys KeyTranslate(Windows.System.VirtualKey inkey)
        {
            switch (inkey)
            {
                // XNA does not have have 'handless' key values.
                // So, we arebitrarily map those to the 'Left' version.                 
                case Windows.System.VirtualKey.Control:
                    return Keys.LeftControl;
                case Windows.System.VirtualKey.Shift:
                    return Keys.LeftShift;
                // Note that the Alt key is now refered to as Menu.
                case Windows.System.VirtualKey.Menu:
                    return Keys.LeftAlt;
                default:
                    return (Keys)inkey;
            }
        }

        private void Keyboard_KeyUp(object sender, KeyEventArgs args)
        {
            var xnaKey = KeyTranslate(args.VirtualKey);

            if (_keys.Contains(xnaKey))
                _keys.Remove(xnaKey);
        }

        private void Keyboard_KeyDown(object sender, KeyEventArgs args)
        {
            var xnaKey = KeyTranslate(args.VirtualKey);

            if (!_keys.Contains(xnaKey))
                _keys.Add(xnaKey);
        }
    }
}
