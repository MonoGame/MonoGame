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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Input;

namespace Microsoft.Xna.Framework
{
    public partial class MetroGameWindow : GameWindow
    {
        GestureRecognizer gestureRecognizer;
        GestureType currentManipulationGestureType = GestureType.None;
        Vector2 lastDelta = Vector2.Zero;
        float dipFactor;

        void InitializeTouch()
        {
            // Receives mouse, touch and stylus input events
            _coreWindow.PointerPressed += CoreWindow_PointerPressed;
            _coreWindow.PointerReleased += CoreWindow_PointerReleased;
            _coreWindow.PointerMoved += CoreWindow_PointerMoved;

            gestureRecognizer = new GestureRecognizer();

            // Manipulation is used for pinch, drag and flick
            gestureRecognizer.ManipulationUpdated += gestureRecognizer_ManipulationUpdated;
            gestureRecognizer.ManipulationCompleted += gestureRecognizer_ManipulationCompleted;
            // Tap, DoubleTap
            gestureRecognizer.Tapped += gestureRecognizer_Tapped;
            // Hold
            gestureRecognizer.Holding += gestureRecognizer_Holding;

            TouchPanel.EnabledGesturesChanged += TouchPanel_EnabledGesturesChanged;

            // To convert from DIPs (that all touch and pointer events are returned in) to pixels (that XNA APIs expect)
            dipFactor = DisplayProperties.LogicalDpi / 96.0f;
        }

        void TouchPanel_EnabledGesturesChanged(object sender, EventArgs e)
        {
            GestureSettings gestureSettings = GestureSettings.None;
            GestureType gestureType = TouchPanel.EnabledGestures;

            if ((gestureType & GestureType.Tap) != 0)
                gestureSettings |= GestureSettings.Tap;

            if ((gestureType & GestureType.DoubleTap) != 0)
                gestureSettings |= GestureSettings.DoubleTap;

            if ((gestureType & GestureType.Hold) != 0)
                gestureSettings |= GestureSettings.Hold;

            if ((gestureType & (GestureType.Flick | GestureType.FreeDrag | GestureType.HorizontalDrag | GestureType.VerticalDrag | GestureType.DragComplete)) != 0)
                gestureSettings |= GestureSettings.Drag | GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY;

            if ((gestureType & (GestureType.Pinch | GestureType.PinchComplete)) != 0)
                gestureSettings |= GestureSettings.ManipulationScale;

            gestureRecognizer.GestureSettings = gestureSettings;
        }

        void gestureRecognizer_Holding(GestureRecognizer sender, HoldingEventArgs args)
        {
            if (args.PointerDeviceType == PointerDeviceType.Touch)
            {
                if ((args.HoldingState == HoldingState.Started) && TouchPanel.EnabledGestures.HasFlag(GestureType.Hold))
                {
                    TimeSpan timeStamp = new TimeSpan(DateTime.Now.Ticks);
                    Vector2 pos = new Vector2((float)args.Position.X, (float)args.Position.Y) * dipFactor;
                    TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Hold, timeStamp, pos, Vector2.Zero, Vector2.Zero, Vector2.Zero));
                }
            }
        }

        void gestureRecognizer_Tapped(GestureRecognizer sender, TappedEventArgs args)
        {
            if (args.PointerDeviceType == PointerDeviceType.Touch)
            {
                if (TouchPanel.EnabledGestures.HasFlag(GestureType.Tap))
                {
                    GestureType gestureType = GestureType.Tap;
                    if (args.TapCount == 2)
                        gestureType = GestureType.DoubleTap;
                    TimeSpan timeStamp = new TimeSpan(DateTime.Now.Ticks);
                    Vector2 pos = new Vector2((float)args.Position.X, (float)args.Position.Y) * dipFactor;
                    TouchPanel.GestureList.Enqueue(new GestureSample(gestureType, timeStamp, pos, Vector2.Zero, Vector2.Zero, Vector2.Zero));
                }
            }
        }

        void gestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            if (args.PointerDeviceType == PointerDeviceType.Touch)
            {
                TimeSpan timeStamp = new TimeSpan(DateTime.Now.Ticks);
                if ((args.Cumulative.Expansion != 0.0f) && TouchPanel.EnabledGestures.HasFlag(GestureType.PinchComplete))
                {
                    TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.PinchComplete, timeStamp, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero));
                }
                else
                {
                    if (TouchPanel.EnabledGestures.HasFlag(GestureType.Flick))
                    {
                        Vector2 delta = Vector2.Zero;
                        if (currentManipulationGestureType != GestureType.VerticalDrag)
                            delta.X = (float)(args.Velocities.Linear.X * dipFactor * 1000.0);
                        if (currentManipulationGestureType != GestureType.HorizontalDrag)
                            delta.Y = (float)(args.Velocities.Linear.Y * dipFactor * 1000.0);
                        // Place a minimum on the flick so tiny movements don't register
                        if (delta.LengthSquared() > (100.0f * 100.0f))
                            TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Flick, timeStamp, Vector2.Zero, Vector2.Zero, delta, Vector2.Zero));
                    }
                    if (TouchPanel.EnabledGestures.HasFlag(GestureType.DragComplete))
                        TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.DragComplete, timeStamp, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero));
                }

                currentManipulationGestureType = GestureType.None;
            }
        }

        void gestureRecognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            if (args.PointerDeviceType == PointerDeviceType.Touch)
            {
                GestureType gestureType = GestureType.None;
                TimeSpan timeStamp = new TimeSpan(DateTime.Now.Ticks);
                Vector2 pos = new Vector2((float)args.Position.X, (float)args.Position.Y) * dipFactor;
                Vector2 pos2 = Vector2.Zero;
                Vector2 delta = new Vector2((float)args.Delta.Translation.X, (float)args.Delta.Translation.Y) * dipFactor;
                Vector2 delta2 = Vector2.Zero;
                if ((args.Delta.Expansion != 0.0f) && TouchPanel.EnabledGestures.HasFlag(GestureType.Pinch))
                {
                    gestureType = GestureType.Pinch;
                    // Hack: The manipulation events only give us the centroid position and the distance between the two points
                    delta.X = (float)args.Delta.Expansion * dipFactor;
                    delta.Y = 0.0f;
                }
                else
                {
                    if (TouchPanel.EnabledGestures.HasFlag(GestureType.FreeDrag))
                    {
                        gestureType = GestureType.FreeDrag;
                    }
                    else if ((TouchPanel.EnabledGestures & (GestureType.HorizontalDrag | GestureType.VerticalDrag)) != 0)
                    {
                        if (currentManipulationGestureType == GestureType.HorizontalDrag)
                        {
                            delta.Y = 0.0f;
                            gestureType = GestureType.HorizontalDrag;
                        }
                        else if (currentManipulationGestureType == GestureType.VerticalDrag)
                        {
                            delta.X = 0.0f;
                            gestureType = GestureType.VerticalDrag;
                        }
                        else
                        {
                            bool horizontal = Math.Abs(args.Delta.Translation.X) > Math.Abs(args.Delta.Translation.Y);
                            if (horizontal)
                            {
                                if (TouchPanel.EnabledGestures.HasFlag(GestureType.HorizontalDrag))
                                {
                                    delta.Y = 0.0f;
                                    gestureType = GestureType.HorizontalDrag;
                                }
                            }
                            else
                            {
                                if (TouchPanel.EnabledGestures.HasFlag(GestureType.VerticalDrag))
                                {
                                    delta.X = 0.0f;
                                    gestureType = GestureType.VerticalDrag;
                                }
                            }
                        }
                    }
                }

                if (gestureType != GestureType.None)
                {
                    // Complete the previous gesture if the gesture type changed
                    if ((gestureType != currentManipulationGestureType) && (currentManipulationGestureType != GestureType.None))
                    {
                        GestureType completeGesture = GestureType.DragComplete;
                        if (currentManipulationGestureType == GestureType.Pinch)
                            completeGesture = GestureType.PinchComplete;
                        TouchPanel.GestureList.Enqueue(new GestureSample(completeGesture, timeStamp, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero));
                    }
                    // Add the new gesture and remember what it was
                    TouchPanel.GestureList.Enqueue(new GestureSample(gestureType, timeStamp, pos, pos2, delta, delta2));
                }
                currentManipulationGestureType = gestureType;
                lastDelta = delta;
            }
        }

        void CoreWindow_PointerMoved(CoreWindow sender, PointerEventArgs args)
        {
            Vector2 pos = new Vector2((float)args.CurrentPoint.Position.X, (float)args.CurrentPoint.Position.Y) * dipFactor;
            bool isTouch = args.CurrentPoint.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            if (isTouch)
            {
                // Touch panel event
                TouchPanel.Collection.Update((int)args.CurrentPoint.PointerId, TouchLocationState.Moved, pos);
            }
            if (!isTouch || args.CurrentPoint.Properties.IsPrimary)
            {
                // Mouse or stylus event or the primary touch event (simulated as mouse input)
                Mouse.State.Update(args);
            }
            gestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints());
        }

        void CoreWindow_PointerReleased(CoreWindow sender, PointerEventArgs args)
        {
            Vector2 pos = new Vector2((float)args.CurrentPoint.Position.X, (float)args.CurrentPoint.Position.Y) * dipFactor;
            bool isTouch = args.CurrentPoint.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            if (isTouch)
            {
                // Touch panel event
                TouchPanel.Collection.Update((int)args.CurrentPoint.PointerId, TouchLocationState.Released, pos);
            }
            if (!isTouch || args.CurrentPoint.Properties.IsPrimary)
            {
                // Mouse or stylus event or the primary touch event (simulated as mouse input)
                Mouse.State.Update(args);
            }
            gestureRecognizer.ProcessUpEvent(args.CurrentPoint);
        }

        void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            Vector2 pos = new Vector2((float)args.CurrentPoint.Position.X, (float)args.CurrentPoint.Position.Y) * dipFactor;
            bool isTouch = args.CurrentPoint.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            if (isTouch)
            {
                // Touch panel event
                TouchPanel.Collection.Add((int)args.CurrentPoint.PointerId, pos);
            }
            if (!isTouch || args.CurrentPoint.Properties.IsPrimary)
            {
                // Mouse or stylus event or the primary touch event (simulated as mouse input)
                Mouse.State.Update(args);
            }
            gestureRecognizer.ProcessDownEvent(args.CurrentPoint);
        }

    }
}
