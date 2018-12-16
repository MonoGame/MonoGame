// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Android.Views;

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Manages touch events for Android. Maps new presses to new touch Ids as per Xna WP7 incrementing touch Id behaviour. 
    /// This is required as Android reports touch IDs of 0 to 5, which leads to incorrect handling of touch events.
    /// Motivation and discussion: http://monogame.codeplex.com/discussions/382252
    /// </summary>
    class AndroidTouchEventManager
    {
        readonly AndroidGameWindow _gameWindow;

        public bool Enabled { get; set; }

        public AndroidTouchEventManager(AndroidGameWindow androidGameWindow)
        {
            _gameWindow = androidGameWindow;
        }

        public void OnTouchEvent(MotionEvent e)
        {
            if (!Enabled)
                return;

            Vector2 position = Vector2.Zero;
            position.X = e.GetX(e.ActionIndex);
            position.Y = e.GetY(e.ActionIndex);
            UpdateTouchPosition(ref position);
            int id = e.GetPointerId(e.ActionIndex);
            switch (e.ActionMasked)
            {
                // DOWN                
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    TouchPanel.AddEvent(id, TouchLocationState.Pressed, position);
                    break;
                // UP                
                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    TouchPanel.AddEvent(id, TouchLocationState.Released, position);
                    break;
                // MOVE                
                case MotionEventActions.Move:
                    for (int i = 0; i < e.PointerCount; i++)
                    {
                        id = e.GetPointerId(i);
                        position.X = e.GetX(i);
                        position.Y = e.GetY(i);
                        UpdateTouchPosition(ref position);
                        TouchPanel.AddEvent(id, TouchLocationState.Moved, position);
                    }
                    break;

                // CANCEL, OUTSIDE                
                case MotionEventActions.Cancel:
                case MotionEventActions.Outside:
                    for (int i = 0; i < e.PointerCount; i++)
                    {
                        id = e.GetPointerId(i);
                        TouchPanel.AddEvent(id, TouchLocationState.Released, position);
                    }
                    break;
            }
        }

        void UpdateTouchPosition(ref Vector2 position)
        {
            Rectangle clientBounds = _gameWindow.ClientBounds;

            //Fix for ClientBounds
            position.X -= clientBounds.X;
            position.Y -= clientBounds.Y;
        }
    }
}