// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.Content;
using Android.Media;
using Android.Views;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.Android;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Our override of OpenTK.AndroidGameView. Provides Touch and Key Input handling.
    /// </summary>
    internal class MonoGameAndroidGameView : AndroidGameView, View.IOnTouchListener, ISurfaceHolderCallback
    {
        private readonly AndroidGameWindow _gameWindow;
        private readonly Game _game;
        private readonly AndroidTouchEventManager _touchManager;

        public bool IsResuming { get; private set; }
        private bool _lostContext;

        public MonoGameAndroidGameView(Context context, AndroidGameWindow androidGameWindow, Game game)
            : base(context)
        {
            _gameWindow = androidGameWindow;
            _game = game;
            _touchManager = new AndroidTouchEventManager(androidGameWindow);
        }

        public bool TouchEnabled
        {
            get { return _touchManager.Enabled; }
            set
            {
                _touchManager.Enabled = value;
                SetOnTouchListener(value ? this : null);
            }
        }

        #region IOnTouchListener implementation

        bool IOnTouchListener.OnTouch(View v, MotionEvent e)
        {
            _touchManager.OnTouchEvent(e);
            return true;
        }

        #endregion

        #region ISurfaceHolderCallback implementation

        //AndroidGameView also implements ISurfaceHolderCallback and has these methods.
        //That is why these get called even though we never register as a SurfaceHolderCallback

        private bool _isSurfaceChanged = false;
        private int _prevSurfaceWidth = 0;
        private int _prevSurfaceHeight = 0;

        void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
        {
            if ((int)Android.OS.Build.VERSION.SdkInt >= 19)
            {
                if (!_isSurfaceChanged)
                {
                    _isSurfaceChanged = true;
                    _prevSurfaceWidth = width;
                    _prevSurfaceHeight = height;
                }
                else
                {
                    // Forcing reinitialization of the view if SurfaceChanged() is called more than once to fix shifted drawing on KitKat.
                    // See https://github.com/mono/MonoGame/issues/2492.
                    if (!ScreenReceiver.ScreenLocked && Game.Instance.Platform.IsActive &&
                        (_prevSurfaceWidth != width || _prevSurfaceHeight != height))
                    {
                        _prevSurfaceWidth = width;
                        _prevSurfaceHeight = height;

                        base.SurfaceDestroyed(holder);
                        base.SurfaceCreated(holder);
                    }
                }
            }

            SurfaceChanged(holder, format, width, height);
            Android.Util.Log.Debug("MonoGame", "MonoGameAndroidGameView.SurfaceChanged: format = " + format + ", width = " + width + ", height = " + height);

            if (_game.GraphicsDevice != null)
                _game.graphicsDeviceManager.ResetClientBounds();
        }

        void ISurfaceHolderCallback.SurfaceDestroyed(ISurfaceHolder holder)
        {
            SurfaceDestroyed(holder);
            Android.Util.Log.Debug("MonoGame", "MonoGameAndroidGameView.SurfaceDestroyed");
        }

        void ISurfaceHolderCallback.SurfaceCreated(ISurfaceHolder holder)
        {
            SurfaceCreated(holder);
            Android.Util.Log.Debug("MonoGame", "MonoGameAndroidGameView.SurfaceCreated: surfaceFrame = " + holder.SurfaceFrame.ToString());
            _isSurfaceChanged = false;
        }

        #endregion

        #region AndroidGameView

        protected override void OnLoad(EventArgs eventArgs)
        {
            MakeCurrent();
        }

        public override void Resume()
        {
            if (!ScreenReceiver.ScreenLocked && Game.Instance.Platform.IsActive)
                base.Resume();
        }


        protected override void OnContextLost(EventArgs e)
        {
            base.OnContextLost(e);
            // OnContextLost is called when the underlying OpenGL context is destroyed
            // this usually happens on older devices when other opengl apps are run 
            // or the lock screen is enabled. Modern devices can preserve the opengl 
            // context along with all the textures and shaders it has attached.
            Android.Util.Log.Debug("MonoGame", "MonoGameAndroidGameView Context Lost");

            // DeviceResetting events
            _game.graphicsDeviceManager.OnDeviceResetting(EventArgs.Empty);
            if (_game.GraphicsDevice != null)
                _game.GraphicsDevice.OnDeviceResetting();

            _lostContext = true;
        }

        protected override void OnContextSet(EventArgs e)
        {
            // This is called when a Context is created. This will happen in
            // two ways, first when the activity is first created. Second if 
            // (and only if) the context is lost 
            // When an acivity is now paused we correctly preserve the context
            // rather than destoying it along with the Surface which is what 
            // used to happen. 

            base.OnContextSet(e);
            Android.Util.Log.Debug("MonoGame", "MonoGameAndroidGameView Context Set");

            if (_lostContext)
            {
                _lostContext = false;

                if (_game.GraphicsDevice != null)
                {
                    _game.GraphicsDevice.Initialize();

                    IsResuming = true;
                    if (_gameWindow.Resumer != null)
                    {
                        _gameWindow.Resumer.LoadContent();
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

                            IsResuming = false;
                        });

                    bgThread.Start();
                }
            }
        }
        protected override void CreateFrameBuffer()
        {
            Android.Util.Log.Debug("MonoGame", "MonoGameAndroidGameView.CreateFrameBuffer");
            try
            {
                GLContextVersion = GLContextVersion.Gles2_0;
                try
                {
                    int depth = 0;
                    int stencil = 0;
                    switch (_game.graphicsDeviceManager.PreferredDepthStencilFormat)
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
                    GraphicsMode = new AndroidGraphicsMode(new ColorFormat(8, 8, 8, 8), depth, stencil, 0, 0, false);
                    base.CreateFrameBuffer();
                }
                catch (Exception)
                {
                    Android.Util.Log.Debug("MonoGame", "Failed to create desired format, falling back to defaults");
                    // try again using a more basic mode with a 16 bit depth buffer which hopefully the device will support 
                    GraphicsMode = new AndroidGraphicsMode(new ColorFormat(0, 0, 0, 0), 16, 0, 0, 0, false);
                    try
                    {
                        base.CreateFrameBuffer();
                    }
                    catch (Exception)
                    {
                        // ok we are right back to getting the default
                        GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);
                        base.CreateFrameBuffer();
                    }
                }
                Android.Util.Log.Debug("MonoGame", "Created format {0}", GraphicsContext.GraphicsMode);
                All status = GL.CheckFramebufferStatus(All.Framebuffer);
                Android.Util.Log.Debug("MonoGame", "Framebuffer Status: " + status.ToString());
            }
            catch (Exception)
            {
                throw new NotSupportedException("Could not create OpenGLES 2.0 frame buffer");
            }

            MakeCurrent();
        }

        #endregion

        #region Key and Motion

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
#if OUYA
            if (GamePad.OnKeyDown(keyCode, e))
                return true;
#endif

            Keyboard.KeyDown(keyCode);
            // we need to handle the Back key here because it doesnt work any other way
#if !OUYA
            if (keyCode == Keycode.Back)
                GamePad.Back = true;
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

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
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

        #endregion
    }
}