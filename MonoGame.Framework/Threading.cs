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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
#if IPHONE
using MonoTouch.Foundation;
using MonoTouch.OpenGLES;
#if ES11
using OpenTK.Graphics.ES11;
#else
using OpenTK.Graphics.ES20;
#endif
#elif WINDOWS || LINUX
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK;
#endif

namespace Microsoft.Xna.Framework
{
    internal class Threading
    {
        static int mainThreadId;
#if ANDROID
        static List<Action> actions = new List<Action>();
        static Mutex actionsMutex = new Mutex();
#elif IPHONE
        public static EAGLContext BackgroundContext;
#elif WINDOWS || LINUX
        public static IGraphicsContext BackgroundContext;
        public static IWindowInfo WindowInfo;
#endif
        static Threading()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Runs the given action on the UI thread and blocks the current thread while the action is running.
        /// If the current thread is the UI thread, the action will run immediately.
        /// </summary>
        /// <param name="action">The action to be run on the UI thread</param>
        internal static void BlockOnUIThread(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

#if DIRECTX || PSS
            action();
#else
            // If we are already on the UI thread, just call the action and be done with it
            if (mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                action();
                return;
            }

#if IPHONE
            lock (BackgroundContext)
            {
                if (EAGLContext.CurrentContext != BackgroundContext)
                    EAGLContext.SetCurrentContext(BackgroundContext);
                action();
            }
#elif WINDOWS || LINUX
            lock (BackgroundContext)
            {
                if (GraphicsContext.CurrentContext != BackgroundContext)
                    BackgroundContext.MakeCurrent(WindowInfo);
                action();
            }
#else
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
#if MONOMAC
            MonoMac.AppKit.NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
#else
            Add(() =>
#endif
            {
#if ANDROID
                if (!Game.Instance.Window.GraphicsContext.IsCurrent)
                    Game.Instance.Window.MakeCurrent();
#endif
                action();
                resetEvent.Set();
            });
            resetEvent.Wait();
#endif
#endif
        }

#if ANDROID
        static void Add(Action action)
        {
            lock (actions)
            {
                actions.Add(action);
            }
        }

        /// <summary>
        /// Runs all pending actions.  Must be called from the UI thread.
        /// </summary>
        internal static void Run()
        {
            System.Diagnostics.Debug.Assert(mainThreadId == Thread.CurrentThread.ManagedThreadId, "Threading.Run must be called from the UI thread");
            lock (actions)
            {
                foreach (Action action in actions)
                {
                    action();
                }
                actions.Clear();
            }
        }
#endif
    }
}