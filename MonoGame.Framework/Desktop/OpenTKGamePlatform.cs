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

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    class OpenTKGamePlatform : GamePlatform
    {
        private OpenTKGameWindow _view;
		private OpenALSoundController soundControllerInstance = null;
        
        
        public override bool VSyncEnabled
        {
            get
            {
                return _view.Window.VSync == OpenTK.VSyncMode.On ? true : false;
            }
            
            set
            {
                _view.Window.VSync = value ? OpenTK.VSyncMode.On : OpenTK.VSyncMode.Off;
            }
        }
        
		public OpenTKGamePlatform(Game game)
            : base(game)
        {
            _view = new OpenTKGameWindow();
            _view.Game = game;
            this.Window = _view;
			
			// Setup our OpenALSoundController to handle our SoundBuffer pools
			soundControllerInstance = OpenALSoundController.GetInstance;
            
#if LINUX
            // also set up SdlMixer to play background music. If one of these functions fails, we will not get any background music (but that should rarely happen)
            Tao.Sdl.Sdl.SDL_InitSubSystem(Tao.Sdl.Sdl.SDL_INIT_AUDIO);
            Tao.Sdl.SdlMixer.Mix_OpenAudio(44100, (short)Tao.Sdl.Sdl.AUDIO_S16SYS, 2, 1024);			
#endif
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        public override void RunLoop()
        {
            ResetWindowBounds(false);
            _view.Window.Run(0);
        }

        public override void StartRunLoop()
        {
            throw new NotImplementedException();
        }
        
        public override void Exit()
        {
            if (!_view.Window.IsExiting)
            {
                Net.NetworkSession.Exit();
                _view.Window.Exit();
            }
#if LINUX
            Tao.Sdl.SdlMixer.Mix_CloseAudio();
#endif
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
			// Update our OpenAL sound buffer pools
			soundControllerInstance.Update();

            // Let the touch panel update states.
            TouchPanel.UpdateState();

            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override void EnterFullScreen()
        {
            ResetWindowBounds(false);
        }

        public override void ExitFullScreen()
        {
            ResetWindowBounds(false);
        }

        internal void ResetWindowBounds(bool toggleFullScreen)
        {
            Rectangle bounds;

            bounds = Window.ClientBounds;

            //Changing window style forces a redraw. Some games
            //have fail-logic and toggle fullscreen in their draw function,
            //so temporarily become inactive so it won't execute.

            bool wasActive = IsActive;
            IsActive = false;

            var graphicsDeviceManager = (GraphicsDeviceManager)
                Game.Services.GetService(typeof(IGraphicsDeviceManager));

            if (graphicsDeviceManager.IsFullScreen)
            {
                bounds = new Rectangle(0, 0,graphicsDeviceManager.PreferredBackBufferWidth,graphicsDeviceManager.PreferredBackBufferHeight);

                if (OpenTK.DisplayDevice.Default.Width != graphicsDeviceManager.PreferredBackBufferWidth ||
                    OpenTK.DisplayDevice.Default.Height != graphicsDeviceManager.PreferredBackBufferHeight)
                {
                    OpenTK.DisplayDevice.Default.ChangeResolution(graphicsDeviceManager.PreferredBackBufferWidth,
                            graphicsDeviceManager.PreferredBackBufferHeight,
                            OpenTK.DisplayDevice.Default.BitsPerPixel,
                            OpenTK.DisplayDevice.Default.RefreshRate);
                }
            }
            else
            {
                
                // switch back to the normal screen resolution
                OpenTK.DisplayDevice.Default.RestoreResolution();
                // now update the bounds 
                bounds.Width = graphicsDeviceManager.PreferredBackBufferWidth;
                bounds.Height = graphicsDeviceManager.PreferredBackBufferHeight;
            }

            

            // Now we set our Presentation Parameters
            var device = (GraphicsDevice)graphicsDeviceManager.GraphicsDevice;
            // FIXME: Eliminate the need for null checks by only calling
            //        ResetWindowBounds after the device is ready.  Or,
            //        possibly break this method into smaller methods.
            if (device != null)
            {
                PresentationParameters parms = device.PresentationParameters;
                parms.BackBufferHeight = (int)bounds.Height;
                parms.BackBufferWidth = (int)bounds.Width;
            }

            if (toggleFullScreen)
                _view.ToggleFullScreen();

            // we only change window bounds if we are not fullscreen
            // or if fullscreen mode was just entered
            if (!graphicsDeviceManager.IsFullScreen || toggleFullScreen)
                _view.ChangeClientBounds(bounds);

            IsActive = wasActive;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            
        }

        public override void Log(string Message)
        {
            Console.WriteLine(Message);
        }

        public override void Present()
        {
            base.Present();

            _view.Window.SwapBuffers();
        }
		
        protected override void Dispose(bool disposing)
        {
            _view.Dispose ();
			
			base.Dispose(disposing);
        }
			
    }
}
