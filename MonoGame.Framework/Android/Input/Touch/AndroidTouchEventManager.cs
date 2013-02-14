// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2012 The MonoGame Team
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
// #endregion License
// 

using System;
using System.Diagnostics;
using Android.Views;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Manages touch events for Android. Maps new presses to new touch Ids as per Xna WP7 incrementing touch Id behaviour. 
    /// This is required as Android reports touch IDs of 0 to 5, which leads to incorrect handling of touch events.
    /// Motivation and discussion: http://monogame.codeplex.com/discussions/382252
    /// </summary>
    class AndroidTouchEventManager
    {
        Game _game;

        public bool Enabled { get; set; }

        public AndroidTouchEventManager(Game game)
        {
            _game = game;
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
            Rectangle clientBounds = _game.Window.ClientBounds;

            //Fix for ClientBounds
            position.X -= clientBounds.X;
            position.Y -= clientBounds.Y;
        }
    }
}