// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
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

using MonoMac.Foundation;
using MonoMac.QTKit;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    public sealed class VideoPlayer
    {
		private Video  _video;
		private MediaState _state;
		private bool _isLooped;
		private Game _game;
        private MacGamePlatform _platform;
		
		// TODO Needed to bind OpenGL to Quicktime private QTVisualContextRef  textureContext;
    	// TODO Needed to grab frame as a texture private CVOpenGLTextureRef  currentFrame;
		
        public VideoPlayer()
        {
			_state = MediaState.Stopped;
			_game = Game.Instance;
            _platform = (MacGamePlatform)_game.Services.GetService(typeof(MacGamePlatform));
        }

        public Texture2D GetTexture()
        {
			// Coming Soon
			throw new NotImplementedException();
			// TODO when Xamarin implement the relevant functions return currentFrame;
        }

        public void Pause()
        {
			if (_video != null )
			{
				_video.MovieView.Pause(new NSObject());
				_state = MediaState.Paused;
			}
        }
		
		 public MediaState State
        {
            get
            {
				return _state;
            }
        }
		
		private void PlayVideo()
		{				
			if (_video != null )
			{
				
				//_video.MovieView.SetFrameOrigin(new System.Drawing.PointF(0,0));
				//_video.MovieView.SetFrameSize(new System.Drawing.SizeF(_game.GraphicsDevice.PresentationParameters.BackBufferWidth,_game.GraphicsDevice.PresentationParameters.BackBufferHeight));
				_video.MovieView.Frame = new System.Drawing.RectangleF(0,0, _game.GraphicsDevice.PresentationParameters.BackBufferWidth, _game.GraphicsDevice.PresentationParameters.BackBufferHeight);
				
				/*  A primitive way of launching the media player
				 * MonoMac.AppKit.NSWorkspace workspace = MonoMac.AppKit.NSWorkspace.SharedWorkspace; 
				workspace.OpenUrls(new[]{NSUrl.FromString(_video.FileName)},
                                     @"com.apple.quicktimeplayer", 
                                     MonoMac.AppKit.NSWorkspaceLaunchOptions.Async,
                                     new NSAppleEventDescriptor(),
                                     new[]{""}); */
				
				// TODO when Xamarin implement the relevant functions var theError = QTOpenGLTextureContextCreate( null, null, _game.Window.PixelFormat, _game.Window.OpenGLContext, out textureContext);
				
				_game.Window.AddSubview(_video.MovieView);			
								
				NSNotificationCenter.DefaultCenter.AddObserver( new NSString("QTMovieDidEndNotification"), (notification) => 
				{
					Stop();
					if (_isLooped)
						PlayVideo();
					
				});
				_video.MovieView.Play(new NSObject());	
				
				_state = MediaState.Playing;
                // FIXME: I'm not crazy about keeping track of IsPlayingVideo in MacGamePlatform, but where else can
                //        this concept be expressed?
				_platform.IsPlayingVideo = true;
			}
		}

        public void Play(Microsoft.Xna.Framework.Media.Video video)
        {	
			_video = video;
			PlayVideo();		
        }

        public void Stop()
        {
			if ( _video != null )
			{
				MonoMac.Foundation.NSObject o = new MonoMac.Foundation.NSObject();
				_video.MovieView.Pause(o);
				_video.MovieView.GotoBeginning(o);
				_state = MediaState.Stopped;
                // FIXME: I'm not crazy about keeping track of IsPlayingVideo in MacGamePlatform, but where else can
                //        this concept be expressed?
                _platform.IsPlayingVideo = false;
				_video.MovieView.RemoveFromSuperview();
			}
        }
		
		public void Resume()
		{
			if ( _video != null )
			{
				_video.MovieView.Play(new NSObject());
			}
		}
		
        public bool IsLooped
        {
            get
            {
				return _isLooped;
            }
            set
            {
				_isLooped = value;
            }
        }

        public Microsoft.Xna.Framework.Media.Video Video
        {
            get
            {
                return _video;
            }
        }
		
		public float Volume 
		{ 
			get
			{
				return _video.Volume;
			}
			set
			{
				_video.Volume = value;
			}
		}
		
		public TimeSpan PlayPosition 
		{ 
			get
			{
				return _video.CurrentPosition;
			}
		}
	}
}
