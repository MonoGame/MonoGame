// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using NUnitLite;
using NUnit.Framework;

#if PLATFORM_MACOS_LEGACY
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
#endif

namespace MonoGame.Tests
{
    static class Program
    {
        [Apartment(ApartmentState.STA)]
        static int Main(string [] args)
        {
            using (var pool = new NSAutoreleasePool())
            {
                NSApplication.Init();

                // The Mac test runner is the best-behaved of
                // all the runners because we are able to take
                // advantage of the default activation policy
                // for command line utilities.  This means that
                // all the tests can run and execute without
                // stealing focus or bringing the test window to
                // the foreground.
                //
                // Best of all, to get this functionality, we
                // simply don't call anything at all.  (Acting
                // like a normal application is a bit trickier.)
                // SetInteractive ();

                return new AutoRun().Execute(args);
            }
        }

        static void SetInteractive()
        {
            var application = NSApplication.SharedApplication;
            application.ActivationPolicy = NSApplicationActivationPolicy.Regular;
            application.ActivateIgnoringOtherApps (true);
        }
    }
}
