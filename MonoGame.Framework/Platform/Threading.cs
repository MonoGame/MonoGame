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
#endif
#if DESKTOPGL || ANGLE || GLES
using MonoGame.OpenGL;
#endif

namespace Microsoft.Xna.Framework
{
    internal class Threading
    {
        public const int kMaxWaitForUIThread = 750; // In milliseconds
        public const int MaxPooledResetEvents = 16;

        static int mainThreadId;

        static Stack<ManualResetEventSlim> resetEventPool = new Stack<ManualResetEventSlim>();
        static List<Action> queuedActions = new List<Action>();
        readonly static Action<Action> MetaAction = (a) => a();

        static class StateActionHelper<TState>
        {
            public static readonly Queue<QueuedAction<TState>> Queue = new Queue<QueuedAction<TState>>();
            public static readonly Action DequeueAction = Dequeue;

            public static void Dequeue()
            {
                if (Queue.Count > 0)
                {
                    var item = Queue.Dequeue();
                    item.Action.Invoke(item.State);
                    item.ResetEvent.Set();
                }
            }
        }

        struct QueuedAction<TState>
        {
            public ManualResetEventSlim ResetEvent;
            public Action<TState> Action;
            public TState State;
        }

#if IOS
        public static EAGLContext BackgroundContext;
#endif

        static Threading()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

#if ANDROID
        internal static void ResetThread (int id)
        {
            mainThreadId = id;
        }
#endif

        /// <summary>
        /// Checks if the code is currently running on the UI thread.
        /// </summary>
        /// <returns>true if the code is currently running on the UI thread.</returns>
        public static bool IsOnUIThread()
        {
            return mainThreadId == Thread.CurrentThread.ManagedThreadId;
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

        /// <summary>
        /// Runs the given action on the UI thread and blocks the current thread while the action is running.
        /// If the current thread is the UI thread, the action will run immediately.
        /// </summary>
        /// <param name="action">The action to be run on the UI thread</param>
        internal static void BlockOnUIThread(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            BlockOnUIThread(MetaAction, action);
        }

        /// <summary>
        /// Runs the given action on the UI thread and blocks the current thread while the action is running.
        /// If the current thread is the UI thread, the action will run immediately.
        /// </summary>
        /// <param name="action">The action to be run on the UI thread</param>
        /// <param name="state">The data to pass to <paramref name="action"/></param>.
        internal static void BlockOnUIThread<TState>(Action<TState> action, TState state)
        {
            if (action == null)
                throw new ArgumentNullException("action");

#if DIRECTX || PSM
            action(state);
#else
            // If we are already on the UI thread, just call the action and be done with it
            if (IsOnUIThread())
            {
                action(state);
                return;
            }

            ManualResetEventSlim resetEvent = RentResetEvent();
            var queuedAction = new QueuedAction<TState>
            {
                ResetEvent = resetEvent,
                Action = action,
                State = state
            };

            lock (queuedActions)
            {
                StateActionHelper<TState>.Queue.Enqueue(queuedAction);
                queuedActions.Add(StateActionHelper<TState>.DequeueAction);
            }
            resetEvent.Wait();
            ReturnResetEvent(resetEvent);
#endif
        }

        static ManualResetEventSlim RentResetEvent()
        {
            lock (resetEventPool)
            {
                if (resetEventPool.Count > 0)
                    return resetEventPool.Pop();
            }
            return new ManualResetEventSlim();
        }

        static void ReturnResetEvent(ManualResetEventSlim resetEvent)
        {
            resetEvent.Reset();

            lock (resetEventPool)
            {
                if (resetEventPool.Count < MaxPooledResetEvents)
                    resetEventPool.Push(resetEvent);
            }
        }

        /// <summary>
        /// Runs all pending actions. Must be called from the UI thread.
        /// </summary>
        internal static void Run()
        {
            EnsureUIThread();

#if IOS
            lock (BackgroundContext)
            {
                // Make the context current on this thread if it is not already
                if (!Object.ReferenceEquals(EAGLContext.CurrentContext, BackgroundContext))
                    EAGLContext.SetCurrentContext(BackgroundContext);
#endif

            lock (queuedActions)
            {
                foreach (Action queuedAction in queuedActions)
                {
#if ANDROID
                    //if (!Game.Instance.Window.GraphicsContext.IsCurrent)
                        ((AndroidGameWindow)Game.Instance.Window).GameView.MakeCurrent();
#endif

                    queuedAction.Invoke();
                }
                queuedActions.Clear();
            }

#if IOS
                // Must flush the GL calls so the GPU asset is ready for the main context to use it
                GL.Flush();
                GraphicsExtensions.CheckGLError();
            }
#endif
        }
    }
}
