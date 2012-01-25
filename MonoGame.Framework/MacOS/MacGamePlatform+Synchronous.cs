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
using System.Runtime.InteropServices;

using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Microsoft.Xna.Framework
{
    partial class MacGamePlatform
    {
        public override void RunLoop()
        {
            var application = NSApplication.SharedApplication;

            using (var appDelegate = new SynchronousApplicationDelegate())
            using (var windowDelegate = new SynchronousWindowDelegate(this))
            {
                var savedAppDelegate = application.Delegate;
                var savedWindowDelegate = _mainWindow.Delegate;

                application.Delegate = appDelegate;
                _mainWindow.Delegate = windowDelegate;

                try
                {
                    // We can't use NSApplicationDelegate.DidFinishLaunching as
                    // is normal, since multiple separate Game instances may be
                    // created serially and only the first one will be able to
                    // react to DidFinishLaunching (which is sent only once).
                    // So, instead invoke asynchronously on the main thread.
                    application.BeginInvokeOnMainThread(() => StartRunLoop());
                    application.Run();
                }
                finally
                {
                    application.Delegate = savedAppDelegate;
                    _mainWindow.Delegate = savedWindowDelegate;
                }
            }
        }

        private class SynchronousApplicationDelegate : NSApplicationDelegate
        {
            public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
            {
                return false;
            }
        }

        private class SynchronousWindowDelegate : MainWindowDelegate
        {
            public SynchronousWindowDelegate(MacGamePlatform owner)
                : base(owner)
            { }

            public override void WillClose(NSNotification notification)
            {
                base.WillClose(notification);

                var application = NSApplication.SharedApplication;
                // Invoke asynchronously on the main thread (even though that's
                // almost certainly the current thread) to give final events a
                // chance to be processed before we stop the application loop.
                application.BeginInvokeOnMainThread(() =>
                {
                    // HACK: For reasons not yet understood, the event loop gets
                    //       starved when stopping in Synchronous mode, so that
                    //       NSApplication.Stop does not cause NSApplication.Run
                    //       to return at once.  An event is needed before this
                    //       will happen.
                    //
                    //       So, here we send a key down, then key up for
                    //       whatever key happens to be represented by 0.  This
                    //       un-hangs NSApplication.Run and allows it to return,
                    //       but it's not the most beautiful thing in the world.
                    var evtDown = NativeMethods.CGEventCreateKeyboardEvent(IntPtr.Zero, 0, true);
                    NativeMethods.CGEventPost(NativeMethods.kCGHIDEventTap, evtDown);
                    NativeMethods.CFRelease(evtDown);

                    var evtUp = NativeMethods.CGEventCreateKeyboardEvent(IntPtr.Zero, 0, false);
                    NativeMethods.CGEventPost(NativeMethods.kCGHIDEventTap, evtUp);
                    NativeMethods.CFRelease(evtUp);

                    application.Stop(this);
                });
            }
        }

        private static class NativeMethods
        {
            public const int kCGHIDEventTap = 0;

            private const string ApplicationServicesLibrary =
                "/System/Library/Frameworks/ApplicationServices.framework/Versions/A/ApplicationServices";

            [DllImport(ApplicationServicesLibrary, CharSet=CharSet.Unicode)]
            public extern static IntPtr CGEventCreateKeyboardEvent(
                IntPtr source,
                int virtualKey,
                bool keyDown);

            [DllImport(ApplicationServicesLibrary, CharSet=CharSet.Unicode)]
            public extern static void CGEventPost(int tap, IntPtr source);

            [DllImport(MonoMac.Constants.CoreFoundationLibrary, CharSet=CharSet.Unicode)]
            public extern static void CFRelease(IntPtr cf);
        }
    }
}
