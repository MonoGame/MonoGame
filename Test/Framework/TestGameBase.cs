// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Tests.Components;

#if IOS
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace MonoGame.Tests {
	class TestGameBase : Game, IFrameInfoSource {
		private bool _isExiting;

		public TestGameBase ()
		{
#if XNA
            Content.RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
#endif
            // We do all the tests using the reference device to
            // avoid driver glitches and get consistant rendering.
            GraphicsAdapter.UseReferenceDevice = true;

            Services.AddService<IFrameInfoSource>(this);
			SuppressExtraUpdatesAndDraws = true;
		}

		#region IFrameInfoSource Implementation

		private FrameInfo _frameInfo;
		public FrameInfo FrameInfo
		{
			get { return _frameInfo; }
		}

		#endregion IFrameInfoSource Implementation

		public Predicate<FrameInfo> ExitCondition { get; set; }
		public bool SuppressExtraUpdatesAndDraws { get; set; }

		public event EventHandler<FrameInfoEventArgs> InitializeWith;
		public event EventHandler<FrameInfoEventArgs> LoadContentWith;
		public event EventHandler<FrameInfoEventArgs> UnloadContentWith;
		public event EventHandler<FrameInfoEventArgs> DrawWith;
		public event EventHandler<FrameInfoEventArgs> UpdateWith;
		public event EventHandler<FrameInfoEventArgs> UpdateOncePerDrawWith;

		public event EventHandler<FrameInfoEventArgs> PreInitializeWith;
		public event EventHandler<FrameInfoEventArgs> PreLoadContentWith;
		public event EventHandler<FrameInfoEventArgs> PreUnloadContentWith;
		public event EventHandler<FrameInfoEventArgs> PreDrawWith;
		public event EventHandler<FrameInfoEventArgs> PreUpdateWith;

		public void ClearActions ()
		{
			InitializeWith = null;
			LoadContentWith = null;
			UnloadContentWith = null;
			DrawWith = null;
			UpdateWith = null;
			UpdateOncePerDrawWith = null;

			PreInitializeWith = null;
			PreLoadContentWith = null;
			PreUnloadContentWith = null;
			PreDrawWith = null;
			PreUpdateWith = null;
		}

		private void SafeRaise (EventHandler<FrameInfoEventArgs> handler)
		{
			if (handler != null)
				handler (this, new FrameInfoEventArgs(FrameInfo));
		}

	    public void InitializeOnly()
	    {
            if (GraphicsDevice == null)
            {
                var graphicsDeviceManager = Services.GetService(typeof(IGraphicsDeviceManager)) as IGraphicsDeviceManager;
                graphicsDeviceManager.CreateDevice();
            }
            Initialize();
	    }

		protected override void Initialize ()
		{
			SafeRaise (PreInitializeWith);
			base.Initialize ();
			SafeRaise (InitializeWith);
		}

		protected override void LoadContent ()
		{
			SafeRaise (PreLoadContentWith);
			base.LoadContent ();
			SafeRaise (LoadContentWith);
		}

		protected override void UnloadContent ()
		{
			SafeRaise (PreUnloadContentWith);
			base.UnloadContent ();
			SafeRaise (UnloadContentWith);
		}

		public void Run (Predicate<FrameInfo> until = null)
		{
			if (until != null)
				ExitCondition = until;
#if XNA
			try {
				base.Run ();
			} finally {
				// XNA (WinForms) leaves WM_QUIT hanging around
				// in the message queue sometimes, and when it
				// does, all future windows that are created are
				// instantly killed.  So, we manually absorb any
				// WM_QUIT that exists.
				if (_isExiting)
					AbsorbQuitMessage ();
			}
#elif IOS || ANDROID
			RunOnMainThreadAndWait();
#else
			base.Run (GameRunBehavior.Synchronous);
#endif
		}

#if IOS || ANDROID
		private void RunOnMainThreadAndWait()
		{
			var exitEvent = new ManualResetEvent(false);
			var exitHandler = new EventHandler<EventArgs>(
				(sender, e) => exitEvent.Set ());

			Exiting += exitHandler;
			try {
				InvokeRunOnMainThread();
				var maxExecutionTime = TimeSpan.FromSeconds(30);
				if (!exitEvent.WaitOne (maxExecutionTime)) {
					throw new TimeoutException (string.Format (
						"Game.Run timed out.  Maximum execution time is {0}.",
						maxExecutionTime));
				}
			}
			finally {
				Exiting -= exitHandler;
			}
		}
#endif

#if IOS
		private void InvokeRunOnMainThread()
		{
			Exception ex = null;
			UIApplication.SharedApplication.InvokeOnMainThread(() => {
				try {
					base.Run (GameRunBehavior.Asynchronous);
				} catch (Exception innerEx) {
					ex = innerEx;
				}
			});

			if (ex != null)
				throw ex;
		}
#endif

#if ANDROID
		private void InvokeRunOnMainThread()
		{
			throw new NotImplementedException(
				"Android need to implement TestGameBase.InvokeRunOnMainThread");
		}
#endif

		private readonly UpdateGuard _updateGuard = new UpdateGuard ();
		protected override void Update (GameTime gameTime)
		{
			_frameInfo.AdvanceUpdate (gameTime);
			EvaluateExitCondition ();

			if (_isExiting && SuppressExtraUpdatesAndDraws)
				return;

			SafeRaise (PreUpdateWith);

			base.Update (gameTime);

			if (_updateGuard.ShouldUpdate (FrameInfo))
				UpdateOncePerDraw (gameTime);

			SafeRaise (UpdateWith);
		}

		protected virtual void UpdateOncePerDraw (GameTime gameTime)
		{
			SafeRaise (UpdateOncePerDrawWith);
		}

		protected override void Draw (GameTime gameTime)
		{
			_frameInfo.AdvanceDraw (gameTime);
			EvaluateExitCondition ();

			if (_isExiting && SuppressExtraUpdatesAndDraws)
				return;

			SafeRaise (PreDrawWith);
			base.Draw (gameTime);
			SafeRaise (DrawWith);
		}

        protected void DoExit()
        {
#if XNA
            Exit();
#else
            // NOTE: We avoid Game.Exit() here as we marked it
            // obsolute on platforms that disallow exit in 
            // shipping games.
            //
            // We however need it here to halt the app after we
            // complete running all the unit tests.  So we do the
            // next best thing can call the interal platform code
            // directly which produces the same result.
            Platform.Exit();
            SuppressDraw();
#endif
        }

		private void EvaluateExitCondition ()
		{
			if (_isExiting || ExitCondition == null)
				return;

			if (ExitCondition (_frameInfo)) {
				_isExiting = true;
				DoExit();
			}
		}

#if XNA
		[StructLayout (LayoutKind.Sequential)]
		public struct NativeMessage {
			public IntPtr handle;
			public uint msg;
			public IntPtr wParam;
			public IntPtr lParam;
			public uint time;
			public System.Drawing.Point p;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool PeekMessage(
			out NativeMessage msg, IntPtr hWnd,
			uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

		[DllImport ("user32.dll")]
		private static extern int GetMessage (
			out NativeMessage msg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

		const uint WM_QUIT = 0x12;

		protected static void AbsorbQuitMessage ()
		{
			NativeMessage msg;
			if (!PeekMessage (out msg, IntPtr.Zero, 0, 0, 0))
				return;

			do {
				int result = GetMessage (out msg, IntPtr.Zero, 0, 0);
				if (result == -1 || result == 0)
					return;
			} while (msg.msg != WM_QUIT);
		}
#endif
	}
}
