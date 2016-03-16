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
using System.Diagnostics;
using System.Drawing;
using Microsoft.Xna.Framework;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;


namespace MonoGame.Framework
{
    class WinFormsGamePlatform : GamePlatform
    {
        //internal static string LaunchParameters;

        private WinFormsGameWindow _window;
        private readonly List<XnaKeys> _keyState;

        public WinFormsGamePlatform(Game game)
            : base(game)
        {
            _keyState = new List<XnaKeys>();
            Keyboard.SetKeys(_keyState);

            _window = new WinFormsGameWindow(this);
            _window.KeyState = _keyState;

            Mouse.SetWindows(_window._form);

            Window = _window;
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        protected override void OnIsMouseVisibleChanged()
        {
            _window.MouseVisibleToggled();
        }

        public override bool BeforeRun()
        {
            _window.UpdateWindows();
            return base.BeforeRun();
        }

        public override void BeforeInitialize()
        {
            _window.Initialize(Game.graphicsDeviceManager.PreferredBackBufferWidth, Game.graphicsDeviceManager.PreferredBackBufferHeight);

            base.BeforeInitialize();

            #if (WINDOWS && DIRECTX)

            if (Game.graphicsDeviceManager.IsFullScreen)
            {
                EnterFullScreen();
            }
            else
            {
                ExitFullScreen();
            }
#endif
        }

        public override void RunLoop()
        {
            _window.RunLoop();
        }

        public override void StartRunLoop()
        {
            throw new NotSupportedException("The Windows platform does not support asynchronous run loops");
        }
        
        public override void Exit()
        {
            if (_window != null)
                _window.Dispose();
            _window = null;
            Window = null;
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override void EnterFullScreen()
        {
#if (WINDOWS && DIRECTX)
            if (_alreadyInFullScreenMode)
            {
                return;
            }

            if (Game.graphicsDeviceManager.HardwareModeSwitch)
            {
                 Game.GraphicsDevice.PresentationParameters.IsFullScreen = true;
                 Game.GraphicsDevice.CreateSizeDependentResources(true);
                 Game.GraphicsDevice.ApplyRenderTargets(null);
                _window._form.WindowState = FormWindowState.Maximized;
            }
            else
            {
                _window.IsBorderless = true;
                _window._form.WindowState = FormWindowState.Maximized;
            }

            _alreadyInWindowedMode = false;
            _alreadyInFullScreenMode = true;
#endif
        }

        public override void ExitFullScreen()
        {
#if (WINDOWS && DIRECTX)
            if (_alreadyInWindowedMode)
            {
               return;
            }

            if (Game.graphicsDeviceManager.HardwareModeSwitch)
            {
                _window._form.WindowState = FormWindowState.Normal;
                Game.GraphicsDevice.PresentationParameters.IsFullScreen = false;
                Game.GraphicsDevice.CreateSizeDependentResources(true);
                Game.GraphicsDevice.ApplyRenderTargets(null);
            }
            else
            {
                _window._form.WindowState = FormWindowState.Normal;
                _window.IsBorderless = false;
            }
            ResetWindowBounds();

            _alreadyInWindowedMode = true;
            _alreadyInFullScreenMode = false;
#endif
        }

        public void ResetWindowBounds()
        {
            _window.ChangeClientSize(new Size(Game.graphicsDeviceManager.PreferredBackBufferWidth, Game.graphicsDeviceManager.PreferredBackBufferHeight));
        }
        
        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void Log(string message)
        {
            Debug.WriteLine(message);
        }

        public override void Present()
        {
            var device = Game.GraphicsDevice;
            if ( device != null )
                device.Present();
        }
		
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_window != null)
                {
                    _window.Dispose();
                    _window = null;
                    Window = null;
                }
                Microsoft.Xna.Framework.Media.MediaManagerState.CheckShutdown();
            }
            Keyboard.SetKeys(null);

            base.Dispose(disposing);
        }
    }
}
