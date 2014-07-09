#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;

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
            Content.RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
			Services.AddService<IFrameInfoSource> (this);
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

		protected override void Initialize ()
		{
			SafeRaise (InitializeWith);
			base.Initialize ();
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

		public new void Run (Predicate<FrameInfo> until = null)
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

		private static void AbsorbQuitMessage ()
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
