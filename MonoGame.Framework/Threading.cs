// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
#if IOS
using Foundation;
using OpenGLES;
#if ES11
using OpenTK.Graphics.ES11;
#else
using OpenTK.Graphics.ES20;
#endif
#elif DESKTOPGL || ANGLE
using OpenGL;
#endif
#if WINDOWS_PHONE
using System.Windows;
#endif

namespace Microsoft.Xna.Framework
{
    internal class Threading
    {
        public const int kMaxWaitForUIThread = 750; // In milliseconds

#if !WINDOWS_PHONE
        static int mainThreadId;
#endif

#if ANDROID || WINDOWS || DESKTOPGL || ANGLE
        static List<Action> actions = new List<Action>();
        //static Mutex actionsMutex = new Mutex();
#elif IOS
        public static EAGLContext BackgroundContext;
#endif

#if !WINDOWS_PHONE
        static Threading()
        {
#if WINDOWS_STOREAPP
            mainThreadId = Environment.CurrentManagedThreadId;
#else
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
#endif
        }
#endif

        /// <summary>
        /// Checks if the code is currently running on the UI thread.
        /// </summary>
        /// <returns>true if the code is currently running on the UI thread.</returns>
        public static bool IsOnUIThread()
        {
#if WINDOWS_PHONE
            return Deployment.Current.Dispatcher.CheckAccess();
#elif WINDOWS_STOREAPP
            return (mainThreadId == Environment.CurrentManagedThreadId);
#else
            return mainThreadId == Thread.CurrentThread.ManagedThreadId;
#endif
        }

        /// <summary>
        /// Throws an exception if the code is not currently running on the UI thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the code is not currently running on the UI thread.</exception>
        public static void EnsureUIThread()
        {
            if (!IsOnUIThread())
                throw new InvalidOperationException("Operation not called on UI thread.");
        }

#if WINDOWS_PHONE
        internal static void RunOnUIThread(Action action)
        {
            RunOnContainerThread(Deployment.Current.Dispatcher, action);
        }
        
        internal static void RunOnContainerThread(System.Windows.Threading.Dispatcher target, Action action)
        {
            target.BeginInvoke(action);
        }

        internal static void BlockOnContainerThread(System.Windows.Threading.Dispatcher target, Action action)
        {
            if (target.CheckAccess())
            {
                action();
            }
            else
            {
                EventWaitHandle wait = new AutoResetEvent(false);
                target.BeginInvoke(() =>
                {
                    action();
                    wait.Set();
                });
                wait.WaitOne(kMaxWaitForUIThread);
            }
        }
#endif

        /// <summary>
        /// Runs the given action on the UI thread and blocks the current thread while the action is running.
        /// If the current thread is the UI thread, the action will run immediately.
        /// </summary>
        /// <param name="action">The action to be run on the UI thread</param>
        internal static void BlockOnUIThread(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

#if (DIRECTX && !WINDOWS_PHONE) || PSM
            action();
#else
            // If we are already on the UI thread, just call the action and be done with it
            if (IsOnUIThread())
            {
#if WINDOWS_PHONE
                try
                {
                    action();
                }
                catch (UnauthorizedAccessException)
                {
                    // Need to be on a different thread
                    BlockOnContainerThread(Deployment.Current.Dispatcher, action);
                }
#else
                action();
#endif
                return;
            }

#if IOS
            lock (BackgroundContext)
            {
                // Make the context current on this thread if it is not already
                if (!Object.ReferenceEquals(EAGLContext.CurrentContext, BackgroundContext))
                    EAGLContext.SetCurrentContext(BackgroundContext);
                // Execute the action
                action();
                // Must flush the GL calls so the GPU asset is ready for the main context to use it
                GL.Flush();
                GraphicsExtensions.CheckGLError();
            }
#elif WINDOWS_PHONE
            BlockOnContainerThread(Deployment.Current.Dispatcher, action);
#else
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
#if MONOMAC
#if PLATFORM_MACOS_LEGACY
            MonoMac.AppKit.NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
#else
            AppKit.NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
#endif
#else
            Add(() =>
#endif
            {
#if ANDROID
                //if (!Game.Instance.Window.GraphicsContext.IsCurrent)
                ((AndroidGameWindow)Game.Instance.Window).GameView.MakeCurrent();
#endif
                action();
                resetEvent.Set();
            });
            resetEvent.Wait();
#endif
#endif
        }

#if ANDROID || WINDOWS || DESKTOPGL || ANGLE
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
            EnsureUIThread();

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
