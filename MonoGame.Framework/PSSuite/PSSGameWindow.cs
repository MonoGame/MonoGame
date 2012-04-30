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

#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using Sce.Pss.Core;
using Sce.Pss.Core.Graphics;
#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    public class PSSGameWindow : GameWindow
    {
		private GraphicsContext _graphics;
		private Rectangle clientBounds;
		private Game _game;
		private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
		private DateTime _now;
        private DisplayOrientation _currentOrientation;
		// TODO private GestureDetector gesture = null;
		private bool _needsToResetElapsedTime = false;
		private bool _isFirstTime = true;
		private TimeSpan _extraElapsedTime;

        public PSSGameWindow(Game game)
        {
            _game = game;
            Initialize();
        }		
						
        private void Initialize()
        {
			_graphics = new GraphicsContext();
			clientBounds = new Rectangle(0, 0, _graphics.Screen.Width, _graphics.Screen.Height);

            // Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();

            // Initialize _lastUpdate
            _lastUpdate = DateTime.Now;
			
			//TODO gesture = new GestureDetector(new GestureListener((AndroidGameActivity)this.Context));
        }

		public void ResetElapsedTime ()
		{
			_needsToResetElapsedTime = true;
		}

		public void Close ()
		{
			if (_graphics != null)
			{
				_graphics.Dispose();
				_graphics = null;
			}
		}
		
		void GameWindow_Closed(object sender,EventArgs e)
        {
			try
			{
        		_game.Exit();
			}
			catch(NullReferenceException)
			{
				// just in case the game is null
			}
		}

        ~PSSGameWindow()
		{
			//
		}
		

        #region AndroidGameView Methods

        internal void OnRenderFrame()
        {
            if (_game != null) {
                _drawGameTime.Update(_now - _lastUpdate);                
                _game.DoDraw(_drawGameTime);
				_lastUpdate = _now;
            }
            try
            {
                _graphics.SwapBuffers();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in swap buffers", ex.ToString());
            }
        }

        internal void OnUpdateFrame()
		{
			if (_game != null )
			{
				_now = DateTime.Now;
				
				if (_isFirstTime) {
					// Initialize GameTime
					_updateGameTime = new GameTime ();
					_drawGameTime = new GameTime ();
					_lastUpdate = DateTime.Now;
					_isFirstTime = false;
				}

				if (_needsToResetElapsedTime) {
					_drawGameTime.ResetElapsedTime();
					_needsToResetElapsedTime = false;
				}
				
				_updateGameTime.Update(_now - _lastUpdate);
				
				TimeSpan catchup = _updateGameTime.ElapsedGameTime;
				if (catchup > _game.TargetElapsedTime) {
					while (catchup > _game.TargetElapsedTime) {
						catchup -= _game.TargetElapsedTime;
						_updateGameTime.ElapsedGameTime = _game.TargetElapsedTime;
						_game.DoUpdate (_updateGameTime);
						_extraElapsedTime += catchup;
					}
					if (_extraElapsedTime > _game.TargetElapsedTime) {
						_game.DoUpdate (_updateGameTime);
						_extraElapsedTime = TimeSpan.Zero;
					}
				}
				else {
					_game.DoUpdate (_updateGameTime);
				}
			}
		}
		
		#endregion
		
		#region GameWindow Overrides
		protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
		{
			// Do nothing.  PSS doesn't do orientation.
		}

		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
		}
		
		public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
		{
		}

		protected override void SetTitle(string title)
		{
			//FIXME window.Title = title;
		}
		
		public override bool AllowUserResizing
		{
			get 
			{
				return false;
			}
			set 
			{
				// Do nothing; Ignore rather than raising and exception
			}
		}

		public override Rectangle ClientBounds 
		{
			get 
			{
				return clientBounds;
			}
		}
		
		public override DisplayOrientation CurrentOrientation
		{
			get { return DisplayOrientation.LandscapeLeft; }
		}
		
		public override IntPtr Handle { get { return IntPtr.Zero; } }
		
		public override string ScreenDeviceName { get { return ""; } } //FIXME
		#endregion
		

        private Dictionary<IntPtr, TouchLocation> _previousTouches = new Dictionary<IntPtr, TouchLocation>();
		/* TODO
		#region IOnTouchListener implementation
		public bool OnTouch (View v, MotionEvent e)
        {
			return OnTouchEvent(e);
		}
		#endregion

        internal void UpdateTouchPosition(ref Vector2 position)
        {
            if (this._game.Window.CurrentOrientation == DisplayOrientation.LandscapeRight)
            {
                // we need to fudge the position
                position.X = this.Width - position.X;
                position.Y = this.Height - position.Y;
            }
            position.X = (position.X / Width) * _game.GraphicsDevice.Viewport.Width;
            position.Y = (position.Y / Height) * _game.GraphicsDevice.Viewport.Height;
            //Android.Util.Log.Info("MonoGameInfo", String.Format("Touch {0}x{1}", position.X, position.Y));
        }

        public override bool OnTouchEvent(MotionEvent e)
        {			
            TouchLocation tlocation;
            TouchCollection collection = TouchPanel.Collection;            
            Vector2 position = Vector2.Zero;            
            position.X = e.GetX(e.ActionIndex);            
            position.Y = e.GetY(e.ActionIndex);     
			UpdateTouchPosition(ref position);
			int id = e.GetPointerId(e.ActionIndex);            
            int index;
            switch (e.ActionMasked)
            {
                // DOWN                
                case 0:
                case 5:
                    index = collection.FindById(e.GetPointerId(e.ActionIndex), out tlocation);
                    if (index < 0)
                    {
                        tlocation = new TouchLocation(id, TouchLocationState.Pressed, position);
                        collection.Add(tlocation);
                    }
                    else
                    {
                        tlocation.State = TouchLocationState.Pressed;
                        tlocation.Position = position;
                    }
                    break;
                // UP                
                case 1:
                case 6:
                    index = collection.FindById(e.GetPointerId(e.ActionIndex), out tlocation);
                    if (index >= 0)
                    {
                        tlocation.State = TouchLocationState.Released;
                        collection[index] = tlocation;
                    }	
				break;
                // MOVE                
                case 2:
                    for (int i = 0; i < e.PointerCount; i++)
                    {
                        id = e.GetPointerId(i);
                        position.X = e.GetX(i);
                        position.Y = e.GetY(i);
                        UpdateTouchPosition(ref position);
                        index = collection.FindById(id, out tlocation);
                        if (index >= 0)
                        {
                            tlocation.State = TouchLocationState.Moved;
                            tlocation.Position = position;
                            collection[index] = tlocation;
                        }
                    }
					break;
                // CANCEL, OUTSIDE                
                case 3:
                case 4:
                    index = collection.FindById(id, out tlocation);
                    if (index >= 0)
                    {
                        tlocation.State = TouchLocationState.Invalid;
                        collection[index] = tlocation;
                    }
                    break;
            }
			
			
			if (gesture != null)
			{
				GestureListener.CheckForDrag(e, position);
				gesture.OnTouchEvent(e);
			}

            return true;
        }
        */
    }
}

