// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.Content;
using Android.Media;
using Android.Views;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using OpenTK.Platform.Android;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Our override of OpenTK.AndroidGameView. Provides Touch and Key Input handling.
    /// </summary>
    internal class MonoGameAndroidGameView : AndroidGameView, View.IOnTouchListener, ISurfaceHolderCallback
    {
        private readonly AndroidGameWindow _gameWindow;
        private readonly AndroidTouchEventManager _touchManager;

        public MonoGameAndroidGameView(Context context, AndroidGameWindow androidGameWindow)
            : base(context)
        {
            _gameWindow = androidGameWindow;
            _touchManager = new AndroidTouchEventManager(androidGameWindow);

            Initialize();
        }

        private void Initialize()
        {
            SetOnTouchListener(this);
        }

        public bool TouchEnabled
        {
            get { return _touchManager.Enabled; }
            set { _touchManager.Enabled = value; }
        }

        #region IOnTouchListener implementation

        bool IOnTouchListener.OnTouch(View v, MotionEvent e)
        {
            _touchManager.OnTouchEvent(e);
            return true;
        }

        #endregion

        #region ISurfaceHolderCallback implementation

        //AndroidGameWindow also implements ISurfaceHolderCallback and has these methods.
        //That is why these get called even though we never register as a SurfaceHolderCallback

        void ISurfaceHolderCallback.SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
        {
            SurfaceChanged(holder, format, width, height);
            Android.Util.Log.Debug("MonoGame", "MonoGameAndroidGameView.SurfaceChanged: format = " + format + ", width = " + width + ", height = " + height);

            if (_gameWindow.Game.GraphicsDevice != null)
                _gameWindow.Game.graphicsDeviceManager.ResetClientBounds();
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
        }

        #endregion

        #region AndroidGameWindow

        protected override void OnLoad(EventArgs eventArgs)
        {
            MakeCurrent();
        }

        protected override void CreateFrameBuffer()
        {
            _gameWindow.CreateFrameBuffer();
        }

        protected override void DestroyFrameBuffer()
        {
            _gameWindow.DestroyFrameBuffer();
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

        #region base method access

        public void BaseCreateFrameBuffer()
        {
            base.CreateFrameBuffer();
        }

        public void BaseDestroyFrameBuffer()
        {
            base.DestroyFrameBuffer();
        }

        #endregion
    }
}