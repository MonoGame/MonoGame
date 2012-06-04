#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2011 The MonoGame Team

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
using System.Linq;
using System.Text;

using Sce.Pss.Core;
using Sce.Pss.Core.Environment;
using Sce.Pss.Core.Input;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework
{
    class PSSGamePlatform : GamePlatform
    {
        Game _game;
        
        public PSSGamePlatform(Game game)
            : base(game)
        {
            Window = new PSSGameWindow(game);
            _game = game;
        }

        private bool _initialized;
        public static bool IsPlayingVdeo { get; set; }
  
        private int _frameBufferWidth, _frameBufferHeight;
        
        public override void Exit()
        {
            //TODO: Fix this
            try
            {
                Net.NetworkSession.Exit();
                Window.Close();
            }
            catch
            {
            }
            
            _loop = false;
        }

        private bool _loop = true;
        public override void RunLoop()
        {
			while (_loop)
			{
				SystemEvents.CheckEvents();
                UpdateTouches();
                
                // Update and render the game.
                if (Game != null)
                    Game.Tick();
			}
        }

        public override void StartRunLoop()
        {
			throw new NotImplementedException();
		}

        public override bool BeforeUpdate(GameTime gameTime)
        {
            if (!_initialized)
            {
                Game.DoInitialize();
                _initialized = true;
            }

            // Let the touch panel update states.
            TouchPanel.UpdateState();
            
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return !IsPlayingVdeo;
        }

        public override bool BeforeRun()
        {
            // Get the Accelerometer going
            Accelerometer.SetupAccelerometer();

            return true;
        }

        public override void EnterFullScreen()
        {
        }

        public override void ExitFullScreen()
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            // FIXME: Can't throw NotImplemented if it is called as a standard part of graphics device creation
            //throw new NotImplementedException();
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            // FIXME: Can't throw NotImplemented if it is called as a standard part of graphics device creation
            //throw new NotImplementedException();
        }
		
        public override void Present ()
        {
            _game.GraphicsDevice.Present();
            _frameBufferWidth = _game.GraphicsDevice._graphics.GetFrameBuffer().Width;
            _frameBufferHeight = _game.GraphicsDevice._graphics.GetFrameBuffer().Height;
        }
		//TODO: Will need something to listen to SystemEvents.???(Pause)??? And SystemEvents.OnRestored when they are properly implemented in PSS

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }
		
		public override void Log(string Message) 
		{
#if LOGGING
			Android.Util.Log.Debug("MonoGameDebug", Message);
#endif
		}
		
		public override void ResetElapsedTime ()
		{
			this.Window.ResetElapsedTime();
		}
		
        private Dictionary<int, TouchLocation> _previousTouches = new Dictionary<int, TouchLocation>();
        
        private void UpdateTouches()
        {           
            var pssTouches = Touch.GetData(0);
            foreach (var touch in pssTouches)
            {
                Vector2 position = new Vector2((touch.X + 0.5f) * _frameBufferWidth, (touch.Y + 0.5f) * _frameBufferHeight);
                if (touch.Status == TouchStatus.Down)
                    TouchPanel.AddEvent(new TouchLocation(touch.ID, TouchLocationState.Pressed, position));
                else if (touch.Status == TouchStatus.Move)
                    TouchPanel.AddEvent(new TouchLocation(touch.ID, TouchLocationState.Moved, position));
                else
                    TouchPanel.AddEvent(new TouchLocation(touch.ID, TouchLocationState.Released, position));
            }
        }
    }
}
