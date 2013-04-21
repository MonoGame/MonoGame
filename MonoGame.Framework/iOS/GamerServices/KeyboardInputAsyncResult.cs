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
using System.Threading;

using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework {
	class KeyboardInputAsyncResult : IAsyncResult {
		private readonly KeyboardInputViewController _viewController;
		private readonly AsyncCallback _callback;
		public KeyboardInputAsyncResult (
			KeyboardInputViewController viewController, AsyncCallback callback, object asyncState)
		{
			if (viewController == null)
				throw new ArgumentNullException ("viewController");

			_viewController = viewController;
			_callback = callback;
			_asyncState = asyncState;
			_asyncWaitHandle = new Lazy<ManualResetEvent>(
				() => new ManualResetEvent(false),
				LazyThreadSafetyMode.ExecutionAndPublication);

			_viewController.View.InputAccepted += ViewController_InputAccepted;
			_viewController.View.InputCanceled += ViewController_InputCanceled;
		}

		private string _result;
		public string GetResult ()
		{
			// FIXME: CFRunLoop.CFDefaultRunLoopMode is currently
			//        returning null.  So for now, cast from the
			//        CLR string version.
			var mode = (NSString)CFRunLoop.ModeDefault;
			while (!_isCompleted)
				CFRunLoop.Main.RunInMode (mode, 0.1, false);
			return _result;
		}

		private void ViewController_InputAccepted (object sender, EventArgs e)
		{
			UnsubscribeViewEvents ();
			MarkComplete (_viewController.View.Text);
		}

		private void ViewController_InputCanceled (object sender, EventArgs e)
		{
			UnsubscribeViewEvents ();
			MarkComplete (null);
		}

		private void MarkComplete (string result)
		{
			_result = result;
			_isCompleted = true;
			if (_asyncWaitHandle.IsValueCreated)
				_asyncWaitHandle.Value.Set ();

			if (_callback != null)
				UIApplication.SharedApplication.BeginInvokeOnMainThread(() => _callback (this));
		}

		private void UnsubscribeViewEvents ()
		{
			_viewController.View.InputAccepted -= ViewController_InputAccepted;
			_viewController.View.InputCanceled -= ViewController_InputCanceled;
		}

		#region IAsyncResult Members

		private readonly object _asyncState;
		public object AsyncState {
			get { return _asyncState; }
		}

		private readonly Lazy<ManualResetEvent> _asyncWaitHandle;
		public WaitHandle AsyncWaitHandle {
			get { return _asyncWaitHandle.Value; }
		}

		public bool CompletedSynchronously {
			get { return false; }
		}

		private bool _isCompleted;
		public bool IsCompleted {
			get { return _isCompleted; }
		}

		#endregion IAsyncResult Members
	}
}
