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

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

using SDL2;
#endregion

namespace Microsoft.Xna.Framework
{
    class SDL2_GamePlatform : GamePlatform
    {
        #region Internal SDL2 Window
        
        private SDL2_GameWindow INTERNAL_window;
        
        #endregion
        
        #region Internal Sound Controller
        
        private OpenALSoundController soundControllerInstance = null;
        
        #endregion
        
        #region SDL2 OS String
        
        public static readonly string OSVersion = SDL.SDL_GetPlatform();
        
        #endregion
        
        #region Public Properties
        
        public override GameRunBehavior DefaultRunBehavior
        {
            get
            {
                return GameRunBehavior.Synchronous;
            }
        }
        
        public override bool VSyncEnabled
        {
            get
            {
                return INTERNAL_window.IsVSync;
            }
            set
            {
                INTERNAL_window.IsVSync = value;
            }
        }
        
        #endregion
        
        public SDL2_GamePlatform(Game game) : base(game)
        {
            // Set and initialize the SDL2 window
            INTERNAL_window = new SDL2_GameWindow();
            INTERNAL_window.Game = game;
            this.Window = INTERNAL_window;

            // Get our OpenALSoundController to handle the SoundBuffer pools
            soundControllerInstance = OpenALSoundController.GetInstance;
        }


        public override void RunLoop()
        {
            INTERNAL_window.INTERNAL_RunLoop();
        }

        public override void StartRunLoop()
        {
            throw new NotImplementedException("SDL2_GamePlatform does not use this!");
        }
        
        public override void Exit()
        {
            // Stop the game loop
            INTERNAL_window.INTERNAL_StopLoop();
            
            // End the network subsystem
            Net.NetworkSession.Exit();
            
            // Close SDL2_mixer if needed
            Media.Song.closeMixer();
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            if (IsActive != INTERNAL_window.IsActive)
            {
                IsActive = INTERNAL_window.IsActive;
            }
            
            // Update our OpenAL sound buffer pools
            soundControllerInstance.Update();

            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override void EnterFullScreen()
        {
            BeginScreenDeviceChange(true);
            EndScreenDeviceChange(
                "SDL2",
                INTERNAL_window.INTERNAL_glFramebufferWidth,
                INTERNAL_window.INTERNAL_glFramebufferHeight
            );
        }

        public override void ExitFullScreen()
        {
            BeginScreenDeviceChange(false);
            EndScreenDeviceChange(
                "SDL2",
                INTERNAL_window.INTERNAL_glFramebufferWidth,
                INTERNAL_window.INTERNAL_glFramebufferHeight
            );
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            INTERNAL_window.EndScreenDeviceChange(screenDeviceName, clientWidth, clientHeight);
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            INTERNAL_window.BeginScreenDeviceChange(willBeFullScreen);
        }
        
        public override void Log(string Message)
        {
            Console.WriteLine(Message);
        }

        public override void Present()
        {
            base.Present();

            GraphicsDevice device = Game.GraphicsDevice;
            if (device != null)
                device.Present();

            if (INTERNAL_window != null)
            {
                INTERNAL_window.INTERNAL_SwapBuffers();
            }
        }
        
        protected override void OnIsMouseVisibleChanged()
        {
            INTERNAL_window.IsMouseVisible = IsMouseVisible;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (INTERNAL_window != null)
                {
                    INTERNAL_window.INTERNAL_Destroy();
                    INTERNAL_window = null;
                }
                
                if (soundControllerInstance != null)
                {
                    soundControllerInstance.Dispose();
                    soundControllerInstance = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
