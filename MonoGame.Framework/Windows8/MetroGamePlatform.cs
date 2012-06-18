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
using System.Diagnostics;

//using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    class MetroGamePlatform : GamePlatform
    {
		//private OpenALSoundController soundControllerInstance = null;
        internal static string LaunchParameters;

        public MetroGamePlatform(Game game)
            : base(game)
        {
            MetroGameWindow.Instance.Game = game;
            this.Window = MetroGameWindow.Instance;

            setLaunchParameters();
			
            // Setup our OpenALSoundController to handle our SoundBuffer pools
            // soundControllerInstance = OpenALSoundController.GetInstance;
        }

        private void setLaunchParameters()
        {
            var arguments = LaunchParameters.Split(' ');

            foreach (var arg in arguments)
            {
                if (arg.Contains("="))
                {
                    var keyVal = arg.Split('=');
                    Game.LaunchParameters.Add(keyVal[0], keyVal[1]);

                }
                else if (arg != string.Empty)
                    Game.LaunchParameters.Add(arg, string.Empty);
            }
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        public override void RunLoop()
        {
            MetroGameWindow.Instance.RunLoop();
        }

        public override void StartRunLoop()
        {
            throw new NotImplementedException();
        }
        
        public override void Exit()
        {
            if (!MetroGameWindow.Instance.IsExiting)
            {
                //Net.NetworkSession.Exit();
                MetroGameWindow.Instance.IsExiting = true;
            }
        }

        public override void BeforeInitialize()
        {
            base.BeforeInitialize();

            // Metro apps are always full screen.
            Game.graphicsDeviceManager.IsFullScreen = true;
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
			// Update our OpenAL sound buffer pools
			//soundControllerInstance.Update();		

            // Let the touch panel update states.
            TouchPanel.UpdateState();

            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            var device = Game.GraphicsDevice;
            if (device != null)
            {
                // For a Metro app we need to re-apply the
                // render target before every draw.  
                // 
                // I guess the OS changes it and doesn't restore it?
                device.ResetRenderTargets();
            }

            return true;
        }

        public override void EnterFullScreen()
        {
            // Metro has no concept of fullscreen vs windowed!
        }

        public override void ExitFullScreen()
        {
            // Metro has no concept of fullscreen vs windowed!
        }
        
        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void Log(string Message)
        {
            Debug.WriteLine(Message);
        }

        public override void Present()
        {
            var device = Game.GraphicsDevice;
            if ( device != null )
                device.Present();
        }

        protected override void OnIsMouseVisibleChanged() 
        {
            MetroGameWindow.Instance.SetCursor(Game.IsMouseVisible);
        }
		
        protected override void Dispose(bool disposing)
        {
            // Make sure we dispose the graphics system.
            var graphicsDeviceManager = Game.graphicsDeviceManager;
            if (graphicsDeviceManager != null)
                graphicsDeviceManager.Dispose();

            MetroGameWindow.Instance.Dispose();
			
			base.Dispose(disposing);
        }
			
    }
}
