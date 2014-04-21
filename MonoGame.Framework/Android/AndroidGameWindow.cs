#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

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
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Media;
using Android.Util;
using Android.Views;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using OpenTK.Platform.Android;

using OpenTK;
using OpenTK.Platform;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    [CLSCompliant(false)]
    public class AndroidGameWindow : GameWindow , View.IOnTouchListener, ISurfaceHolderCallback, View.IOnKeyListener, IDisposable
    {
        private MonoGameAndroidGameView _gameView;
        private Rectangle clientBounds;
        private Game _game;
        private DisplayOrientation supportedOrientations = DisplayOrientation.Default;
        private DisplayOrientation _currentOrientation;
        private AndroidTouchEventManager _touchManager = null;
        private bool _contextWasLost = false;
        private IResumeManager _resumer;
        private bool _isResuming;

		public override IntPtr Handle { get { return IntPtr.Zero; } }

        public bool TouchEnabled
        {
            get { return _touchManager.Enabled; }
            set { _touchManager.Enabled = value; }
        }

        public void SetResumer(IResumeManager resumer)
        {
            _resumer = resumer;
        }

        public AndroidGameWindow(Context context, Game game)
        {
            _game = game;
            TouchPanelState = new TouchPanelState(this);
            Initialize(context);

        }

        private void Initialize(Context context)
        {
			_gameView = new MonoGameAndroidGameView(context, this);
            _gameView.Load += OnLoad;
            _gameView.SetOnKeyListener(this);
            _gameView.SetOnTouchListener(this);
            _gameView.RenderFrame += OnRenderFrame;
            _gameView.UpdateFrame += OnUpdateFrame;
			clientBounds = new Rectangle(0, 0, context.Resources.DisplayMetrics.WidthPixels, context.Resources.DisplayMetrics.HeightPixels);

            _gameView.RequestFocus();
            _gameView.FocusableInTouchMode = true;

            _touchManager = new AndroidTouchEventManager(_game);

#if OUYA
            GamePad.Initialize();
#endif
        }
        
        private void OnLoad (object sender, EventArgs eventArgs)
        {
            _gameView.MakeCurrent();
        }

        #region IOnKeyListener

        bool View.IOnKeyListener.OnKey(View view, Keycode keycode, KeyEvent keyEvent)
        {
            switch (keyEvent.Action)
            {
                case KeyEventActions.Down:
                    return OnKeyDown(keycode, keyEvent);
                case KeyEventActions.Up:
                    return OnKeyUp(keycode, keyEvent);
                //TODO case KeyEventActions.Multiple:
                    //I don't know
            }

            return false;
        }

        #endregion

        private bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
#if OUYA
            if (GamePad.OnKeyDown(keyCode, e))
                return true;
#endif

            Keyboard.KeyDown(keyCode);
            // we need to handle the Back key here because it doesnt work any other way
#if !OUYA
            if (keyCode == Keycode.Back)
                GamePad.Instance.SetBack();
#endif

            if (keyCode == Keycode.VolumeUp)
            {
                AudioManager audioManager = (AudioManager)Game.Activity.GetSystemService(Context.AudioService);
                audioManager.AdjustStreamVolume(Stream.Music, Adjust.Raise, VolumeNotificationFlags.ShowUi);
            }

            if (keyCode == Keycode.VolumeDown)
            {
                AudioManager audioManager = (AudioManager)Game.Activity.GetSystemService(Context.AudioService);
                audioManager.AdjustStreamVolume(Stream.Music, Adjust.Lower, VolumeNotificationFlags.ShowUi);
            }

            return true;
        }

        private bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
#if OUYA
            if (GamePad.OnKeyUp(keyCode, e))
                return true;
#endif
            Keyboard.KeyUp(keyCode);
            return true;
        }

#if OUYA
        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            if (GamePad.OnGenericMotionEvent(e))
                return true;

            return base.OnGenericMotionEvent(e);
        }
#endif

        internal void CreateFrameBuffer()
        {
            Android.Util.Log.Debug("MonoGame", "AndroidGameWindow.CreateFrameBuffer");
            try
            {
                _gameView.GLContextVersion = GLContextVersion.Gles2_0;
                try
                {
                    int depth = 0;
                    int stencil = 0;
                    switch (this._game.graphicsDeviceManager.PreferredDepthStencilFormat)
                    {
                        case DepthFormat.Depth16: 
                        depth = 16;
                        break;
                        case DepthFormat.Depth24:
                        depth = 24;
                        break;
                        case DepthFormat.Depth24Stencil8: 
                        depth = 24;
                        stencil = 8;
                        break;
                        case DepthFormat.None: break;
                    }
                    Android.Util.Log.Debug("MonoGame", string.Format("Creating Color:Default Depth:{0} Stencil:{1}", depth, stencil));
                    _gameView.GraphicsMode = new AndroidGraphicsMode(new ColorFormat(8, 8, 8, 8), depth, stencil, 0, 0, false);
                    _gameView.BaseCreateFrameBuffer();
                }
                catch(Exception)
                {
                    Android.Util.Log.Debug("MonoGame", "Failed to create desired format, falling back to defaults");
                    // try again using a more basic mode with a 16 bit depth buffer which hopefully the device will support 
                    _gameView.GraphicsMode = new AndroidGraphicsMode(new ColorFormat(0, 0, 0, 0), 16, 0, 0, 0, false);
                    try {
                        _gameView.BaseCreateFrameBuffer();
                    } catch (Exception) {
                        // ok we are right back to getting the default
                        _gameView.GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);
                        _gameView.BaseCreateFrameBuffer();
                    }
                }
                Android.Util.Log.Debug("MonoGame", "Created format {0}", _gameView.GraphicsContext.GraphicsMode);
                All status = GL.CheckFramebufferStatus(All.Framebuffer);
                Android.Util.Log.Debug("MonoGame", "Framebuffer Status: " + status.ToString());
            } 
            catch (Exception) 
            {
                throw new NotSupportedException("Could not create OpenGLES 2.0 frame buffer");
            }
            if (_game.GraphicsDevice != null && _contextWasLost)
            {
                _game.GraphicsDevice.Initialize();

                _isResuming = true;
                if (_resumer != null)
                {
                    _resumer.LoadContent();
                }

                // Reload textures on a different thread so the resumer can be drawn
                System.Threading.Thread bgThread = new System.Threading.Thread(
                    o =>
                    {
                        Android.Util.Log.Debug("MonoGame", "Begin reloading graphics content");
                        Microsoft.Xna.Framework.Content.ContentManager.ReloadGraphicsContent();
                        Android.Util.Log.Debug("MonoGame", "End reloading graphics content");

                        // DeviceReset events
                        _game.graphicsDeviceManager.OnDeviceReset(EventArgs.Empty);
                        _game.GraphicsDevice.OnDeviceReset();

                        _contextWasLost = false;
                        _isResuming = false;
                    });

                bgThread.Start();
            }

            _gameView.MakeCurrent();
        }

        internal void DestroyFrameBuffer()
        {
            // DeviceResetting events
            _game.graphicsDeviceManager.OnDeviceResetting(EventArgs.Empty);
            if(_game.GraphicsDevice != null) 
                _game.GraphicsDevice.OnDeviceResetting();

            Android.Util.Log.Debug("MonoGame", "AndroidGameWindow.DestroyFrameBuffer");

            _gameView.BaseDestroyFrameBuffer();

            _contextWasLost = _gameView.GraphicsContext == null || _gameView.GraphicsContext.IsDisposed;
        }

        #region AndroidGameView Methods

        private void OnRenderFrame(object sender, FrameEventArgs frameEventArgs)
        {
            if (_gameView.GraphicsContext == null || _gameView.GraphicsContext.IsDisposed)
                return;

            if (!_gameView.GraphicsContext.IsCurrent)
                _gameView.MakeCurrent();

            Threading.Run();
        }

        private void OnUpdateFrame(object sender, FrameEventArgs frameEventArgs)
        {
            if (!_gameView.GraphicsContext.IsCurrent)
                _gameView.MakeCurrent();

            Threading.Run();

            if (_game != null)
            {
                if (!_isResuming && _game.Platform.IsActive && !ScreenReceiver.ScreenLocked) //Only call draw if an update has occured
                {
                    _game.Tick();
                }
                else if (_game.GraphicsDevice != null)
                {
                    _game.GraphicsDevice.Clear(Color.Black);
                    if (_isResuming && _resumer != null)
                    {
                        _resumer.Draw();
                    }
                    _game.Platform.Present();
                }
            }
        }

        #endregion


        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            supportedOrientations = orientations;
        }

        /// <summary>
        /// In Xna, setting SupportedOrientations = DisplayOrientation.Default (which is the default value)
        /// has the effect of setting SupportedOrientations to landscape only or portrait only, based on the
        /// aspect ratio of PreferredBackBufferWidth / PreferredBackBufferHeight
        /// </summary>
        /// <returns></returns>
        internal DisplayOrientation GetEffectiveSupportedOrientations()
        {
            if (supportedOrientations == DisplayOrientation.Default)
            {
                var deviceManager = (_game.Services.GetService(typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager);
                if (deviceManager == null)
                    return DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

                if (deviceManager.PreferredBackBufferWidth > deviceManager.PreferredBackBufferHeight)
                {
                    return DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
                }
                else
                {
                    return DisplayOrientation.Portrait | DisplayOrientation.PortraitDown;
                }
            }
            else
            {
                return supportedOrientations;
            }
        }

        /// <summary>
        /// Updates the screen orientation. Filters out requests for unsupported orientations.
        /// </summary>
        internal void SetOrientation(DisplayOrientation newOrientation, bool applyGraphicsChanges)
        {
            DisplayOrientation supported = GetEffectiveSupportedOrientations();

            // If the new orientation is not supported, force a supported orientation
            if ((supported & newOrientation) == 0)
            {
                if ((supported & DisplayOrientation.LandscapeLeft) != 0)
                    newOrientation = DisplayOrientation.LandscapeLeft;
                else if ((supported & DisplayOrientation.LandscapeRight) != 0)
                    newOrientation = DisplayOrientation.LandscapeRight;
                else if ((supported & DisplayOrientation.Portrait) != 0)
                    newOrientation = DisplayOrientation.Portrait;
                else if ((supported & DisplayOrientation.PortraitDown) != 0)
                    newOrientation = DisplayOrientation.PortraitDown;
            }

            DisplayOrientation oldOrientation = CurrentOrientation;

            SetDisplayOrientation(newOrientation);
            TouchPanel.DisplayOrientation = newOrientation;

            if (applyGraphicsChanges && oldOrientation != CurrentOrientation && _game.graphicsDeviceManager != null)
                _game.graphicsDeviceManager.ApplyChanges();
        }

        #region IOnTouchListener implementation
        bool View.IOnTouchListener.OnTouch (View v, MotionEvent e)
        {
            _touchManager.OnTouchEvent(e);
            return true;
        }
        #endregion

        public override string ScreenDeviceName 
        {
            get 
            {
                throw new System.NotImplementedException ();
            }
        }
   

        public override Rectangle ClientBounds 
        {
            get 
            {
                return clientBounds;
            }
        }
        
        internal void ChangeClientBounds(Rectangle bounds)
        {
            clientBounds = bounds;
            OnClientSizeChanged();
        }

        public override bool AllowUserResizing 
        {
            get 
            {
                return false;
            }
            set 
            {
                // Do nothing; Ignore rather than raising an exception
            }
        }

        // A copy of ScreenOrientation from Android 2.3
        // This allows us to continue to support 2.2 whilst
        // utilising the 2.3 improved orientation support.
        enum ScreenOrientationAll
        {
            Unspecified = -1,
            Landscape = 0,
            Portrait = 1,
            User = 2,
            Behind = 3,
            Sensor = 4,
            Nosensor = 5,
            SensorLandscape = 6,
            SensorPortrait = 7,
            ReverseLandscape = 8,
            ReversePortrait = 9,
            FullSensor = 10,
        }

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                return _currentOrientation;
            }
        }

        
        private void SetDisplayOrientation(DisplayOrientation value)
        {
            if (value != _currentOrientation)
            {
                DisplayOrientation supported = GetEffectiveSupportedOrientations();
                ScreenOrientation requestedOrientation = ScreenOrientation.Unspecified;
                bool wasPortrait = _currentOrientation == DisplayOrientation.Portrait || _currentOrientation == DisplayOrientation.PortraitDown;
                bool requestPortrait = false;

                bool didOrientationChange = false;
                // Android 2.3 and above support reverse orientations
                int sdkVer = (int)Android.OS.Build.VERSION.SdkInt;
                if (sdkVer >= 10)
                {
                    // Check if the requested orientation is supported. Default means all are supported.
                    if ((supported & value) != 0)
                    {
                        didOrientationChange = true;
                        _currentOrientation = value;
                        switch (value)
                        {
                            case DisplayOrientation.LandscapeLeft:
                                requestedOrientation = (ScreenOrientation)ScreenOrientationAll.Landscape;
                                requestPortrait = false;
                                break;
                            case DisplayOrientation.LandscapeRight:
                                requestedOrientation = (ScreenOrientation)ScreenOrientationAll.ReverseLandscape;
                                requestPortrait = false;
                                break;
                            case DisplayOrientation.Portrait:
                                requestedOrientation = (ScreenOrientation)ScreenOrientationAll.Portrait;
                                requestPortrait = true;
                                break;
                            case DisplayOrientation.PortraitDown:
                                requestedOrientation = (ScreenOrientation)ScreenOrientationAll.ReversePortrait;
                                requestPortrait = true;
                                break;
                        }
                    }
                }
                else
                {
                    // Check if the requested orientation is either of the landscape orientations and any landscape orientation is supported.
                    if ((value == DisplayOrientation.LandscapeLeft || value == DisplayOrientation.LandscapeRight) &&
                        ((supported & (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight)) != 0))
                    {
                        didOrientationChange = true;
                        _currentOrientation = DisplayOrientation.LandscapeLeft;
                        requestedOrientation = ScreenOrientation.Landscape;
                        requestPortrait = false;
                    }
                    // Check if the requested orientation is either of the portrain orientations and any portrait orientation is supported.
                    else if ((value == DisplayOrientation.Portrait || value == DisplayOrientation.PortraitDown) &&
                            ((supported & (DisplayOrientation.Portrait | DisplayOrientation.PortraitDown)) != 0))
                    {
                        didOrientationChange = true;
                        _currentOrientation = DisplayOrientation.Portrait;
                        requestedOrientation = ScreenOrientation.Portrait;
                        requestPortrait = true;
                    }
                }

                if (didOrientationChange)
                {
                    // Android doesn't fire Released events for existing touches
                    // so we need to clear them out.
                    if (wasPortrait != requestPortrait)
                    {
                        TouchPanelState.ReleaseAllTouches();
                    }

                    Game.Activity.RequestedOrientation = requestedOrientation;

                    OnOrientationChanged();
                }
            }
        }

        #region ISurfaceHolderCallback

        void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
        {
            _gameView.SurfaceChanged(holder, format, width, height);
            Android.Util.Log.Debug("MonoGame", "AndroidGameWindow.SurfaceChanged: format = " + format + ", width = " + width + ", height = " + height);

            if (_game.GraphicsDevice != null)
                _game.graphicsDeviceManager.ResetClientBounds();
        }

        void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder)
        {
            _gameView.SurfaceDestroyed(holder);
            Android.Util.Log.Debug("MonoGame", "AndroidGameWindow.SurfaceDestroyed");
        }

        void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder)
        {
            _gameView.SurfaceCreated(holder);
            Android.Util.Log.Debug("MonoGame", "AndroidGameWindow.SurfaceCreated: surfaceFrame = " + holder.SurfaceFrame.ToString());
        }

        #endregion

	    public void Dispose()
	    {
		    if (_gameView != null)
		    {
			    _gameView.Dispose();
			    _gameView = null;
		    }
	    }

		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
		}

		public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
		{
		}

	    protected override void SetTitle(string title)
	    {
	    }
    }

    internal class MonoGameAndroidGameView : AndroidGameView
    {
        private readonly AndroidGameWindow _gameWindow;

        public MonoGameAndroidGameView(Context context, AndroidGameWindow androidGameWindow)
            : base(context)
        {
            _gameWindow = androidGameWindow;
        }

        protected override void CreateFrameBuffer()
        {
            _gameWindow.CreateFrameBuffer();
        }

        protected override void DestroyFrameBuffer()
        {
            _gameWindow.DestroyFrameBuffer();
        }

        public void BaseCreateFrameBuffer()
        {
            base.CreateFrameBuffer();
        }

        public void BaseDestroyFrameBuffer()
        {
            base.DestroyFrameBuffer();
        }
    }
}

