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
    internal class InputEvents
    {
        private readonly TouchQueue _touchQueue;
        private readonly List<Keys> _keys = new List<Keys>();

        public InputEvents(CoreWindow window, UIElement inputElement, TouchQueue touchQueue)
        {
            _touchQueue = touchQueue;

            // The key events are always tied to the window as those will
            // only arrive here if some other control hasn't gotten it.
            window.KeyDown += CoreWindow_KeyDown;
            window.KeyUp += CoreWindow_KeyUp;
            window.VisibilityChanged += CoreWindow_VisibilityChanged;

            if (inputElement != null)
            {
                // If we have an input UIElement then we bind input events
                // to it else we'll get events for overlapping XAML controls.
                inputElement.PointerPressed += UIElement_PointerPressed;
                inputElement.PointerReleased += UIElement_PointerReleased;
                inputElement.PointerCanceled += UIElement_PointerReleased;
                inputElement.PointerMoved += UIElement_PointerMoved;
                inputElement.PointerWheelChanged += UIElement_PointerWheelChanged;
            }
            else
            {
                // If we only have a CoreWindow then use it for input events.
                window.PointerPressed += CoreWindow_PointerPressed;
                window.PointerReleased += CoreWindow_PointerReleased;
                window.PointerMoved += CoreWindow_PointerMoved;
                window.PointerWheelChanged += CoreWindow_PointerWheelChanged;
            }
        }

        #region UIElement Events

        private void UIElement_PointerPressed(object sender, PointerRoutedEventArgs args)
        {
            //Capture this pointer so we continue getting events even if it is dragged off us
            ((UIElement)sender).CapturePointer(args.Pointer);

            var pointerPoint = args.GetCurrentPoint(null);
            PointerPressed(pointerPoint, sender as UIElement, args.Pointer);
            args.Handled = true;
        }

        private void UIElement_PointerMoved(object sender, PointerRoutedEventArgs args)
        {
            var pointerPoint = args.GetCurrentPoint(null);
            PointerMoved(pointerPoint);
            args.Handled = true;
        }

        private void UIElement_PointerReleased(object sender, PointerRoutedEventArgs args)
        {
            ((UIElement)sender).ReleasePointerCapture(args.Pointer);

            var pointerPoint = args.GetCurrentPoint(null);
            PointerReleased(pointerPoint, sender as UIElement, args.Pointer);
            args.Handled = true;
        }

        private void UIElement_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            var pointerPoint = args.GetCurrentPoint(null);
            UpdateMouse(pointerPoint);
            args.Handled = true;
        }

        #endregion // UIElement Events

        #region CoreWindow Events

        private void CoreWindow_PointerPressed(object sender, PointerEventArgs args)
        {
            PointerPressed(args.CurrentPoint, null, null);
            args.Handled = true;
        }

        private void CoreWindow_PointerMoved(object sender, PointerEventArgs args)
        {
            PointerMoved(args.CurrentPoint);
            args.Handled = true;
        }

        private void CoreWindow_PointerReleased(object sender, PointerEventArgs args)
        {
            PointerReleased(args.CurrentPoint, null, null);
            args.Handled = true;
        }

        private void CoreWindow_PointerWheelChanged(object sender, PointerEventArgs args)
        {
            UpdateMouse(args.CurrentPoint);
            args.Handled = true;
        }

        #endregion // CoreWindow Events

        private void PointerPressed(PointerPoint pointerPoint, UIElement target, Pointer pointer)
        {
            // To convert from DIPs (device independent pixels) to screen resolution pixels.
            var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
            var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;

            var isTouch = pointerPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Touch;

            _touchQueue.Enqueue((int)pointerPoint.PointerId, TouchLocationState.Pressed, pos, !isTouch);
            
            if (!isTouch)
            {
                // Mouse or stylus event.
                UpdateMouse(pointerPoint);

                // Capture future pointer events until a release.		
                if (target != null)
                    target.CapturePointer(pointer);
            }
        }

        private void PointerMoved(PointerPoint pointerPoint)
        {
            // To convert from DIPs (device independent pixels) to actual screen resolution pixels.
            var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
            var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;

            var isTouch = pointerPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Touch;
            var touchIsDown = pointerPoint.IsInContact;

            if (touchIsDown)
            {
                _touchQueue.Enqueue((int)pointerPoint.PointerId, TouchLocationState.Moved, pos, !isTouch);
            }

            if (!isTouch)
            {
                // Mouse or stylus event.
                UpdateMouse(pointerPoint);
            }
        }

        private void PointerReleased(PointerPoint pointerPoint, UIElement target, Pointer pointer)
        {
            // To convert from DIPs (device independent pixels) to screen resolution pixels.
            var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
            var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;

            var isTouch = pointerPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Touch;

            _touchQueue.Enqueue((int)pointerPoint.PointerId, TouchLocationState.Released, pos, !isTouch);

            if (!isTouch)
            {
                // Mouse or stylus event.
                UpdateMouse(pointerPoint);

                // Release the captured pointer.
                if (target != null)
                    target.ReleasePointerCapture(pointer);
            }
        }

        private static void UpdateMouse(PointerPoint point)
        {
            // To convert from DIPs (device independent pixels) to screen resolution pixels.
            var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
            var x = (int)(point.Position.X * dipFactor);
            var y = (int)(point.Position.Y * dipFactor);

            var state = point.Properties;

            Mouse.PrimaryWindow.MouseState.X = x;
            Mouse.PrimaryWindow.MouseState.Y = y;
            Mouse.PrimaryWindow.MouseState.ScrollWheelValue += state.MouseWheelDelta;
            Mouse.PrimaryWindow.MouseState.LeftButton = state.IsLeftButtonPressed ? ButtonState.Pressed : ButtonState.Released;
            Mouse.PrimaryWindow.MouseState.RightButton = state.IsRightButtonPressed ? ButtonState.Pressed : ButtonState.Released;
            Mouse.PrimaryWindow.MouseState.MiddleButton = state.IsMiddleButtonPressed ? ButtonState.Pressed : ButtonState.Released;
        }

        public void UpdateState()
        {
            // Update the keyboard state.
            Keyboard.SetKeys(_keys);
        }

        private static Keys KeyTranslate(Windows.System.VirtualKey inkey, CorePhysicalKeyStatus keyStatus)
        {
            switch (inkey)
            {
                // WinRT does not distinguish between left/right keys
                // We have to check for special keys such as control/shift/alt/ etc
                case Windows.System.VirtualKey.Control:
                    // we can detect right Control by checking the IsExtendedKey value.
                    return (keyStatus.IsExtendedKey) ? Keys.RightControl : Keys.LeftControl;
                case Windows.System.VirtualKey.Shift:
                    // we can detect right shift by checking the scancode value.
                    // left shift is 0x2A, right shift is 0x36. IsExtendedKey is always false.
                    return (keyStatus.ScanCode==0x36) ? Keys.RightShift : Keys.LeftShift;
                // Note that the Alt key is now refered to as Menu.
                // ALT key doesn't get fired by KeyUp/KeyDown events.
                // One solution could be to check CoreWindow.GetKeyState(...) on every tick.
                case Windows.System.VirtualKey.Menu:                    
                    return Keys.LeftAlt;

                default:
                    return (Keys)inkey;
            }
        }

        private void CoreWindow_KeyUp(object sender, KeyEventArgs args)
        {
            var xnaKey = KeyTranslate(args.VirtualKey, args.KeyStatus);

            if (_keys.Contains(xnaKey))
                _keys.Remove(xnaKey);
        }

        private void CoreWindow_KeyDown(object sender, KeyEventArgs args)
        {
            var xnaKey = KeyTranslate(args.VirtualKey, args.KeyStatus);

            if (!_keys.Contains(xnaKey))
                _keys.Add(xnaKey);
        }

        private void CoreWindow_VisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            //Forget about the held keys when we disappear as we don't receive key events for them while we are in the background
            if (!args.Visible)
                _keys.Clear();
        }
    }
}
