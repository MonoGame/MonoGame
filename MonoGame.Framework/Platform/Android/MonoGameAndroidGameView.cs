// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Egl;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    public class MonoGameAndroidGameView : SurfaceView, ISurfaceHolderCallback, View.IOnTouchListener
    {
        // What is the state of the app, for tracking surface recreation inside this class.
        // This acts as a replacement for the all-out monitor wait approach which caused code to be quite fragile.
        enum InternalState
        {
            Pausing_UIThread,  // set by android UI thread and the game thread process it and transitions into 'Paused' state
            Resuming_UIThread, // set by android UI thread and the game thread process it and transitions into 'Running' state
            Exiting,           // set either by game or android UI thread and the game thread process it and transitions into 'Exited' state          

            Paused_GameThread,  // set by game thread after processing 'Pausing' state
            Running_GameThread, // set by game thread after processing 'Resuming' state
            Exited_GameThread,  // set by game thread after processing 'Exiting' state

            ForceRecreateSurface, // also used to create the surface the 1st time or when screen orientation changes
        }

        bool disposed = false;
        ISurfaceHolder mHolder;
        System.Drawing.Size size;

        ManualResetEvent _waitForPausedStateProcessed = new ManualResetEvent(false);
        ManualResetEvent _waitForResumedStateProcessed = new ManualResetEvent(false);
        ManualResetEvent _waitForExitedStateProcessed = new ManualResetEvent(false);

        AutoResetEvent _waitForMainGameLoop = new AutoResetEvent(false);
        AutoResetEvent _workerThreadUIRenderingWait = new AutoResetEvent(false);

        object _lockObject = new object();

        volatile InternalState _internalState = InternalState.Exited_GameThread;

        bool androidSurfaceAvailable = false;
        bool needToForceRecreateSurface = false;

        bool glSurfaceAvailable;
        bool glContextAvailable;
        bool lostglContext;
        System.Diagnostics.Stopwatch stopWatch;
        double tick = 0;

        bool loaded = false;

        Task renderTask;
        CancellationTokenSource cts = null;
        private readonly AndroidTouchEventManager _touchManager;
        private readonly AndroidGameWindow _gameWindow;
        private readonly Game _game;

        // Events that are triggered on the game thread
        public static event EventHandler OnPauseGameThread;
        public static event EventHandler OnResumeGameThread;

        public bool TouchEnabled
        {
            get { return _touchManager.Enabled; }
            set
            {
                _touchManager.Enabled = value;
                SetOnTouchListener(value ? this : null);
            }
        }

        public bool IsResuming { get; private set; }

        public MonoGameAndroidGameView(Context context, AndroidGameWindow gameWindow, Game game)
            : base(context)
        {
            _gameWindow = gameWindow;
            _game = game;
            _touchManager = new AndroidTouchEventManager(gameWindow);
            Init();
        }

        private void Init()
        {
            // default
            mHolder = Holder;
            // Add callback to get the SurfaceCreated etc events
            mHolder.AddCallback(this);
            mHolder.SetType(SurfaceType.Gpu);
        }

        public void SurfaceChanged(ISurfaceHolder holder, global::Android.Graphics.Format format, int width, int height)
        {
            // Set flag to recreate gl surface or rendering can be bad on orientation change or if app 
            // is closed in one orientation and re-opened in another.
            lock (_lockObject)
            {
                // can only be triggered when main loop is running, is unsafe to overwrite other states
                if (_internalState == InternalState.Running_GameThread)
                {
                    _internalState = InternalState.ForceRecreateSurface;
                }
                else
                {
                    needToForceRecreateSurface = true;
                }

            }
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            lock (_lockObject)
            {
                androidSurfaceAvailable = true;
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            lock (_lockObject)
            {
                androidSurfaceAvailable = false;
            }
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            _touchManager.OnTouchEvent(e);
            return true;
        }

        public virtual void SwapBuffers()
        {
            EnsureUndisposed();
            if (!egl.EglSwapBuffers(eglDisplay, eglSurface))
            {
                if (egl.EglGetError() == 0)
                {
                    if (lostglContext)
                        System.Diagnostics.Debug.WriteLine("Lost EGL context" + GetErrorAsString());
                    lostglContext = true;
                }
            }

        }

        public virtual void MakeCurrent()
        {
            EnsureUndisposed();
            if (!egl.EglMakeCurrent(eglDisplay, eglSurface,
                    eglSurface, eglContext))
            {
                System.Diagnostics.Debug.WriteLine("Error Make Current" + GetErrorAsString());
            }

        }

        public virtual void ClearCurrent()
        {
            EnsureUndisposed();
            if (!egl.EglMakeCurrent(eglDisplay, EGL10.EglNoSurface,
                EGL10.EglNoSurface, EGL10.EglNoContext))
            {
                System.Diagnostics.Debug.WriteLine("Error Clearing Current" + GetErrorAsString());
            }
        }

        double updates;

        public bool LogFPS { get; set; }
        public bool RenderOnUIThread { get; set; }

        public virtual void Run()
        {
            Run(0.0);
        }

        public virtual void Run(double updatesPerSecond)
        {
            cts = new CancellationTokenSource();
            if (LogFPS)
            {
                targetFps = currentFps = 0;
                avgFps = 1;
            }
            updates = 1000 / updatesPerSecond;

            //var syncContext = new SynchronizationContext ();
            var syncContext = SynchronizationContext.Current;

            // We always start a new task, regardless if we render on UI thread or not.
            renderTask = Task.Factory.StartNew(() =>
           {
               WorkerThreadFrameDispatcher(syncContext);

           }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith((t) =>
               {
                   OnStopped(EventArgs.Empty);
               });
        }

        public virtual void Pause()
        {
            EnsureUndisposed();

            // if triggered in quick succession and blocked by graphics device creation, 
            // pause can be triggered twice, without resume in between on some phones.
            if (_internalState != InternalState.Running_GameThread)
            {
                return;
            }

            // this guarantees that resume finished processing, since we cannot wait inside resume because we deadlock as surface wouldn't get created
            if (RenderOnUIThread == false)
            {
                _waitForResumedStateProcessed.WaitOne();
            }

            _waitForMainGameLoop.Reset();  // in case it was enabled

            // happens if pause is called immediately after resume so that the surfaceCreated callback was not called yet.
            bool isAndroidSurfaceAvalible = false; // use local because the wait below must be outside lock
            lock (_lockObject)
            {
                isAndroidSurfaceAvalible = androidSurfaceAvailable;
                if (!isAndroidSurfaceAvalible)
                {
                    _internalState = InternalState.Paused_GameThread; // prepare for next game loop iteration
                }
            }

            lock (_lockObject)
            {
                // processing the pausing state only if the surface was created already
                if (androidSurfaceAvailable)
                {
                    _waitForPausedStateProcessed.Reset();
                    _internalState = InternalState.Pausing_UIThread;
                }
            }

            if (RenderOnUIThread == false)
            {
                _waitForPausedStateProcessed.WaitOne();
            }
        }

        public virtual void Resume()
        {
            EnsureUndisposed();

            lock (_lockObject)
            {
                _waitForResumedStateProcessed.Reset();
                _internalState = InternalState.Resuming_UIThread;
            }

            _waitForMainGameLoop.Set();

            try
            {
                if (!IsFocused)
                    RequestFocus();
            }
            catch { }

            // do not wait for state transition here since surface creation must be triggered first
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
            base.Dispose(disposing);
        }

        public void Stop()
        {
            EnsureUndisposed();
            if (cts != null)
            {
                lock (_lockObject)
                {
                    _internalState = InternalState.Exiting;
                }

                cts.Cancel();

                if (RenderOnUIThread == false)
                {
                    _waitForExitedStateProcessed.Reset();
                }

            }
        }

        FrameEventArgs renderEventArgs = new FrameEventArgs();

        protected void WorkerThreadFrameDispatcher(SynchronizationContext uiThreadSyncContext)
        {
            Threading.ResetThread(Thread.CurrentThread.ManagedThreadId);
            try
            {
                stopWatch = System.Diagnostics.Stopwatch.StartNew();
                tick = 0;
                prevUpdateTime = DateTime.Now;

                while (!cts.IsCancellationRequested)
                {
                    // either use UI thread to render one frame or this worker thread
                    bool pauseThread = false;
                    if (RenderOnUIThread)
                    {
                        uiThreadSyncContext.Send((s) =>
                       {
                           pauseThread = RunIteration(cts.Token);
                       }, null);
                    }
                    else
                    {
                        pauseThread = RunIteration(cts.Token);
                    }


                    if (pauseThread)
                    {
                        _waitForPausedStateProcessed.Set();
                        _waitForMainGameLoop.WaitOne(); // pause this thread
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("AndroidGameView", ex.ToString());
            }
            finally
            {
                bool c = cts.IsCancellationRequested;

                cts = null;

                if (glSurfaceAvailable)
                    DestroyGLSurface();

                if (glContextAvailable)
                {
                    DestroyGLContext();
                    ContextLostInternal();
                }

                lock (_lockObject)
                {
                    _internalState = InternalState.Exited_GameThread;
                }
            }

        }

        DateTime prevUpdateTime;
        DateTime prevRenderTime;
        DateTime curUpdateTime;
        DateTime curRenderTime;
        FrameEventArgs updateEventArgs = new FrameEventArgs();

        void processStateDefault()
        {
            Log.Error("AndroidGameView", "Default case for switch on InternalState in main game loop, exiting");

            lock (_lockObject)
            {
                _internalState = InternalState.Exited_GameThread;
            }
        }

        void processStateRunning(CancellationToken token)
        {
            // do not run game if surface is not available
            lock (_lockObject)
            {
                if (!androidSurfaceAvailable)
                {
                    return;
                }
            }


            // check if app wants to exit
            if (token.IsCancellationRequested)
            {
                // change state to exit and skip game loop
                lock (_lockObject)
                {
                    _internalState = InternalState.Exiting;
                }

                return;
            }

            try
            {
                UpdateAndRenderFrame();
            }
            catch (MonoGameGLException ex)
            {
                Log.Error("AndroidGameView", "GL Exception occurred during RunIteration {0}", ex.Message);
            }

            if (updates > 0)
            {
                var t = updates - (stopWatch.Elapsed.TotalMilliseconds - tick);
                if (t > 0)
                {
                    if (LogFPS)
                    {
                        Log.Verbose("AndroidGameView", "took {0:F2}ms, should take {1:F2}ms, sleeping for {2:F2}", stopWatch.Elapsed.TotalMilliseconds - tick, updates, t);
                    }

                }
            }

        }

        void processStatePausing()
        {
            if (glSurfaceAvailable)
            {
                // Surface we are using needs to go away
                DestroyGLSurface();

                if (loaded)
                    OnUnload(EventArgs.Empty);
            }

            // trigger callbacks, must pause openAL device here
            var d = OnPauseGameThread;
            if (d != null)
                d(this, EventArgs.Empty);

            // go to next state
            lock (_lockObject)
            {
                _internalState = InternalState.Paused_GameThread;
            }
        }

        void processStateResuming()
        {
            bool isSurfaceAvalible = false;
            lock (_lockObject)
            {
                isSurfaceAvalible = androidSurfaceAvailable;
            }

            // must sleep outside lock!
            if (!RenderOnUIThread && !isSurfaceAvalible)
            {
                Thread.Sleep(50); // sleep so UI thread easier acquires lock
                return;
            }

            // this can happen if pause is triggered immediately after resume so that SurfaceCreated callback doesn't get called yet,
            // in this case we skip the resume process and pause sets a new state.   
            lock (_lockObject)
            {
                if (!androidSurfaceAvailable)
                    return;

                // create surface if context is available
                if (glContextAvailable && !lostglContext)
                {
                    try
                    {
                        CreateGLSurface();
                    }
                    catch (Exception ex)
                    {
                        // We failed to create the surface for some reason
                        Log.Verbose("AndroidGameView", ex.ToString());
                    }
                }

                // create context if not available
                if ((!glContextAvailable || lostglContext))
                {
                    // Start or Restart due to context loss
                    bool contextLost = false;
                    if (lostglContext || glContextAvailable)
                    {
                        // we actually lost the context
                        // so we need to free up our existing 
                        // objects and re-create one.
                        DestroyGLContext();
                        contextLost = true;

                        ContextLostInternal();
                    }

                    CreateGLContext();
                    CreateGLSurface();

                    if (!loaded && glContextAvailable)
                        OnLoad(EventArgs.Empty);

                    if (contextLost && glContextAvailable)
                    {
                        // we lost the gl context, we need to let the programmer
                        // know so they can re-create textures etc.
                        ContextSetInternal();
                    }

                }
                else if (glSurfaceAvailable) // finish state if surface created, may take a frame or two until the android UI thread callbacks fire
                {
                    // trigger callbacks, must resume openAL device here
                    var d = OnResumeGameThread;
                    if (d != null)
                        d(this, EventArgs.Empty);

                    // go to next state
                    _internalState = InternalState.Running_GameThread;
                }
            }
        }

        void processStateExiting()
        {
            // go to next state
            lock (_lockObject)
            {
                _internalState = InternalState.Exited_GameThread;
            }
        }

        void processStateForceSurfaceRecreation()
        {
            // needed at app start
            lock (_lockObject)
            {
                if (!androidSurfaceAvailable || !glContextAvailable)
                {
                    return;
                }
            }

            DestroyGLSurface();
            CreateGLSurface();

            // go to next state
            lock (_lockObject)
            {
                _internalState = InternalState.Running_GameThread;
            }
        }

        // Return true to trigger worker thread pause
        bool RunIteration(CancellationToken token)
        {
            // set main game thread global ID
            Threading.ResetThread(Thread.CurrentThread.ManagedThreadId);

            InternalState currentState = InternalState.Exited_GameThread;

            lock (_lockObject)
            {
                if (needToForceRecreateSurface && _internalState == InternalState.Running_GameThread)
                {
                    _internalState = InternalState.ForceRecreateSurface;
                    needToForceRecreateSurface = false;
                }
                currentState = _internalState;
            }

            switch (currentState)
            {
                // exit states
                case InternalState.Exiting: // when ui thread wants to exit
                    processStateExiting();
                    break;

                case InternalState.Exited_GameThread: // when game thread processed exiting event
                    lock (_lockObject)
                    {
                        _waitForExitedStateProcessed.Set();
                        cts.Cancel();
                    }
                    break;

                // pause states
                case InternalState.Pausing_UIThread: // when ui thread wants to pause              
                    processStatePausing();
                    break;

                case InternalState.Paused_GameThread: // when game thread processed pausing event

                    // this must be processed outside of this loop, in the new task thread!
                    return true; // trigger pause of worker thread

                // other states
                case InternalState.Resuming_UIThread: // when ui thread wants to resume
                    processStateResuming();

                    // pause must wait for resume in case pause/resume is called in very quick succession
                    lock (_lockObject)
                    {
                        _waitForResumedStateProcessed.Set();
                    }
                    break;

                case InternalState.Running_GameThread: // when we are running game 
                    processStateRunning(token);

                    break;

                case InternalState.ForceRecreateSurface:
                    processStateForceSurfaceRecreation();
                    break;

                // default case, error
                default:
                    processStateDefault();
                    cts.Cancel();
                    break;
            }

            return false;
        }

        void UpdateFrameInternal(FrameEventArgs e)
        {
            OnUpdateFrame(e);
            if (UpdateFrame != null)
            {
                UpdateFrame(this, e);
            }

        }

        protected virtual void OnUpdateFrame(FrameEventArgs e)
        {

        }

        // this method is called on the main thread
        void UpdateAndRenderFrame()
        {
            curUpdateTime = DateTime.Now;
            if (prevUpdateTime.Ticks != 0)
            {
                var t = (curUpdateTime - prevUpdateTime).TotalMilliseconds;
                updateEventArgs.Time = t < 0 ? 0 : t;
            }

            try
            {
                UpdateFrameInternal(updateEventArgs);
            }
            catch (Content.ContentLoadException ex)
            {
                if (RenderOnUIThread)
                    throw ex;
                else
                {
                    Game.Activity.RunOnUiThread (() =>
                    {
                        throw ex;
                    });
                }
            }

            prevUpdateTime = curUpdateTime;

            curRenderTime = DateTime.Now;
            if (prevRenderTime.Ticks == 0)
            {
                var t = (curRenderTime - prevRenderTime).TotalMilliseconds;
                renderEventArgs.Time = t < 0 ? 0 : t;
            }

            RenderFrameInternal(renderEventArgs);

            prevRenderTime = curRenderTime;
        }

        void RenderFrameInternal(FrameEventArgs e)
        {
            if (LogFPS)
            {
                Mark();
            }

            OnRenderFrame(e);

            if (RenderFrame != null)
                RenderFrame(this, e);
        }

        protected virtual void OnRenderFrame(FrameEventArgs e)
        {

        }

        int frames = 0;
        double prev = 0;
        double avgFps = 0;
        double currentFps = 0;
        double targetFps = 0;

        void Mark()
        {
            double cur = stopWatch.Elapsed.TotalMilliseconds;
            if (cur < 2000)
            {
                return;
            }
            frames++;

            if (cur - prev >= 995)
            {
                avgFps = 0.8 * avgFps + 0.2 * frames;

                Log.Verbose("AndroidGameView", "frames {0} elapsed {1}ms {2:F2} fps",
                    frames,
                    cur - prev,
                    avgFps);

                frames = 0;
                prev = cur;
            }
        }

        protected void EnsureUndisposed()
        {
            if (disposed)
                throw new ObjectDisposedException("");
        }

        protected void DestroyGLContext()
        {
            if (eglContext != null)
            {
                if (!egl.EglDestroyContext(eglDisplay, eglContext))
                    throw new Exception("Could not destroy EGL context" + GetErrorAsString());
                eglContext = null;
            }
            if (eglDisplay != null)
            {
                if (!egl.EglTerminate(eglDisplay))
                    throw new Exception("Could not terminate EGL connection" + GetErrorAsString());
                eglDisplay = null;
            }

            glContextAvailable = false;
        }

        protected void DestroyGLSurface()
        {
            if (!(eglSurface == null || eglSurface == EGL10.EglNoSurface))
            {
                if (!egl.EglMakeCurrent(eglDisplay, EGL10.EglNoSurface,
                        EGL10.EglNoSurface, EGL10.EglNoContext))
                {
                    Log.Verbose("AndroidGameView", "Could not unbind EGL surface" + GetErrorAsString());
                }

                if (!egl.EglDestroySurface(eglDisplay, eglSurface))
                {
                    Log.Verbose("AndroidGameView", "Could not destroy EGL surface" + GetErrorAsString());
                }
            }
            eglSurface = null;
            glSurfaceAvailable = false;

        }

        internal struct SurfaceConfig
        {
            public int Red;
            public int Green;
            public int Blue;
            public int Alpha;
            public int Depth;
            public int Stencil;
            public int SampleBuffers;
            public int Samples;

            public int[] ToConfigAttribs()
            {
                List<int> attribs = new List<int>();
                if (Red != 0)
                {
                    attribs.Add(EGL11.EglRedSize);
                    attribs.Add(Red);
                }
                if (Green != 0)
                {
                    attribs.Add(EGL11.EglGreenSize);
                    attribs.Add(Green);
                }
                if (Blue != 0)
                {
                    attribs.Add(EGL11.EglBlueSize);
                    attribs.Add(Blue);
                }
                if (Alpha != 0)
                {
                    attribs.Add(EGL11.EglAlphaSize);
                    attribs.Add(Alpha);
                }
                if (Depth != 0)
                {
                    attribs.Add(EGL11.EglDepthSize);
                    attribs.Add(Depth);
                }
                if (Stencil != 0)
                {
                    attribs.Add(EGL11.EglStencilSize);
                    attribs.Add(Stencil);
                }
                if (SampleBuffers != 0)
                {
                    attribs.Add(EGL11.EglSampleBuffers);
                    attribs.Add(SampleBuffers);
                }
                if (Samples != 0)
                {
                    attribs.Add(EGL11.EglSamples);
                    attribs.Add(Samples);
                }
                attribs.Add(EGL11.EglRenderableType);
                attribs.Add(4);
                attribs.Add(EGL11.EglNone);

                return attribs.ToArray();
            }

            static int GetAttribute(EGLConfig config, IEGL10 egl, EGLDisplay eglDisplay,int attribute)
            {
                int[] data = new int[1];
                egl.EglGetConfigAttrib(eglDisplay, config, attribute, data);
                return data[0];
            }

            public static SurfaceConfig FromEGLConfig (EGLConfig config, IEGL10 egl, EGLDisplay eglDisplay)
            {
                return new SurfaceConfig()
                {
                    Red = GetAttribute(config, egl, eglDisplay, EGL11.EglRedSize),
                    Green = GetAttribute(config, egl, eglDisplay, EGL11.EglGreenSize),
                    Blue = GetAttribute(config, egl, eglDisplay, EGL11.EglBlueSize),
                    Alpha = GetAttribute(config, egl, eglDisplay, EGL11.EglAlphaSize),
                    Depth = GetAttribute(config, egl, eglDisplay, EGL11.EglDepthSize),
                    Stencil = GetAttribute(config, egl, eglDisplay, EGL11.EglStencilSize),
                    SampleBuffers = GetAttribute(config, egl, eglDisplay, EGL11.EglSampleBuffers),
                    Samples = GetAttribute(config, egl, eglDisplay, EGL11.EglSamples)
                };
            }

            public override string ToString()
            {
                return string.Format("Red:{0} Green:{1} Blue:{2} Alpha:{3} Depth:{4} Stencil:{5} SampleBuffers:{6} Samples:{7}", Red, Green, Blue, Alpha, Depth, Stencil, SampleBuffers, Samples);
            }
        }

        protected void CreateGLContext()
        {
            lostglContext = false;

            egl = EGLContext.EGL.JavaCast<IEGL10>();

            eglDisplay = egl.EglGetDisplay(EGL10.EglDefaultDisplay);
            if (eglDisplay == EGL10.EglNoDisplay)
                throw new Exception("Could not get EGL display" + GetErrorAsString());

            int[] version = new int[2];
            if (!egl.EglInitialize(eglDisplay, version))
                throw new Exception("Could not initialize EGL display" + GetErrorAsString());

            int depth = 0;
            int stencil = 0;
            int sampleBuffers = 0;
            int samples = 0;
            switch (_game.graphicsDeviceManager.PreferredDepthStencilFormat)
            {
                case DepthFormat.Depth16:
                    depth = 16;
                    break;
                case DepthFormat.Depth24:
                    depth = 24;
                    break;
                case DepthFormat.Depth24Stencil8:
                    depth = 24;
                    stencil = 8;
                    break;
                case DepthFormat.None:
                    break;
            }

            if (_game.graphicsDeviceManager.PreferMultiSampling)
            {
                sampleBuffers = 1;
                samples = 4;
            }

            List<SurfaceConfig> configs = new List<SurfaceConfig>();
            if (depth > 0)
            {
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = depth, Stencil = stencil, SampleBuffers = sampleBuffers, Samples = samples });
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = depth, Stencil = stencil });
                configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5, Depth = depth, Stencil = stencil });
                configs.Add(new SurfaceConfig() { Depth = depth, Stencil = stencil });
                if (depth > 16)
                {
                    configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = 16 });
                    configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5, Depth = 16 });
                    configs.Add(new SurfaceConfig() { Depth = 16 });
                }
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5 });
            }
            else
            {
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, SampleBuffers = sampleBuffers, Samples = samples });
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5 });
            }
            configs.Add(new SurfaceConfig() { Red = 4, Green = 4, Blue = 4 });
            int[] numConfigs = new int[1];
            EGLConfig[] results = new EGLConfig[1];

            if (!egl.EglGetConfigs(eglDisplay, null, 0, numConfigs)) {
                throw new Exception("Could not get config count. " + GetErrorAsString());
            }

            EGLConfig[] cfgs = new EGLConfig[numConfigs[0]];
            egl.EglGetConfigs(eglDisplay, cfgs, numConfigs[0], numConfigs);
            Log.Verbose("AndroidGameView", "Device Supports");
            foreach (var c in cfgs) {
                Log.Verbose("AndroidGameView", string.Format(" {0}", SurfaceConfig.FromEGLConfig(c, egl, eglDisplay)));
            }

            bool found = false;
            numConfigs[0] = 0;
            foreach (var config in configs)
            {
                Log.Verbose("AndroidGameView", string.Format("Checking Config : {0}", config));
                found = egl.EglChooseConfig(eglDisplay, config.ToConfigAttribs(), results, 1, numConfigs);
                Log.Verbose("AndroidGameView", "EglChooseConfig returned {0} and {1}", found, numConfigs[0]);
                if (!found || numConfigs[0] <= 0)
                {
                    Log.Verbose("AndroidGameView", "Config not supported");
                    continue;
                }
                Log.Verbose("AndroidGameView", string.Format("Selected Config : {0}", config));
                break;
            }

            if (!found || numConfigs[0] <= 0)
                throw new Exception("No valid EGL configs found" + GetErrorAsString());
            var createdVersion = new MonoGame.OpenGL.GLESVersion();
            foreach (var v in MonoGame.OpenGL.GLESVersion.GetSupportedGLESVersions ()) {
                Log.Verbose("AndroidGameView", "Creating GLES {0} Context", v);
                eglContext = egl.EglCreateContext(eglDisplay, results[0], EGL10.EglNoContext, v.GetAttributes());
                if (eglContext == null || eglContext == EGL10.EglNoContext)
                {
                    Log.Verbose("AndroidGameView", string.Format("GLES {0} Not Supported. {1}", v, GetErrorAsString()));
                    eglContext = EGL10.EglNoContext;
                    continue;
                }
                createdVersion = v;
                break;
            }
            if (eglContext == null || eglContext == EGL10.EglNoContext)
            {
                eglContext = null;
                throw new Exception("Could not create EGL context" + GetErrorAsString());
            }
            Log.Verbose("AndroidGameView", "Created GLES {0} Context", createdVersion);
            eglConfig = results[0];
            glContextAvailable = true;
        }

        private string GetErrorAsString()
        {
            switch (egl.EglGetError())
            {
                case EGL10.EglSuccess:
                    return "Success";

                case EGL10.EglNotInitialized:
                    return "Not Initialized";

                case EGL10.EglBadAccess:
                    return "Bad Access";
                case EGL10.EglBadAlloc:
                    return "Bad Allocation";
                case EGL10.EglBadAttribute:
                    return "Bad Attribute";
                case EGL10.EglBadConfig:
                    return "Bad Config";
                case EGL10.EglBadContext:
                    return "Bad Context";
                case EGL10.EglBadCurrentSurface:
                    return "Bad Current Surface";
                case EGL10.EglBadDisplay:
                    return "Bad Display";
                case EGL10.EglBadMatch:
                    return "Bad Match";
                case EGL10.EglBadNativePixmap:
                    return "Bad Native Pixmap";
                case EGL10.EglBadNativeWindow:
                    return "Bad Native Window";
                case EGL10.EglBadParameter:
                    return "Bad Parameter";
                case EGL10.EglBadSurface:
                    return "Bad Surface";

                default:
                    return "Unknown Error";
            }
        }

        protected void CreateGLSurface()
        {
            if (!glSurfaceAvailable)
            {
                try
                {
                    // If there is an existing surface, destroy the old one
                    DestroyGLSurface();

                    eglSurface = egl.EglCreateWindowSurface(eglDisplay, eglConfig, (Java.Lang.Object)this.Holder, null);
                    if (eglSurface == null || eglSurface == EGL10.EglNoSurface)
                        throw new Exception("Could not create EGL window surface" + GetErrorAsString());

                    if (!egl.EglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext))
                        throw new Exception("Could not make EGL current" + GetErrorAsString());

                    glSurfaceAvailable = true;

                    // Must set viewport after creation, the viewport has correct values in it already as we call it, but
                    // the surface is created after the correct viewport is already applied so we must do it again.
                    if (_game.GraphicsDevice != null)
                        _game.graphicsDeviceManager.ResetClientBounds();

                    if (MonoGame.OpenGL.GL.GetError == null)
                        MonoGame.OpenGL.GL.LoadEntryPoints();
                }
                catch (Exception ex)
                {
                    Log.Error("AndroidGameView", ex.ToString());
                    glSurfaceAvailable = false;
                }
            }
        }

        protected EGLSurface CreatePBufferSurface(EGLConfig config, int[] attribList)
        {
            IEGL10 egl = EGLContext.EGL.JavaCast<IEGL10>();
            EGLSurface result = egl.EglCreatePbufferSurface(eglDisplay, config, attribList);
            if (result == null || result == EGL10.EglNoSurface)
                throw new Exception("EglCreatePBufferSurface");
            return result;
        }

        protected void ContextSetInternal()
        {
            if (lostglContext)
            {
                if (_game.GraphicsDevice != null)
                {
                    _game.GraphicsDevice.Initialize();

                    IsResuming = true;
                    if (_gameWindow.Resumer != null)
                    {
                        _gameWindow.Resumer.LoadContent();
                    }

                    // Reload textures on a different thread so the resumer can be drawn
                    System.Threading.Thread bgThread = new System.Threading.Thread(
                        o =>
                        {
                            Android.Util.Log.Debug("MonoGame", "Begin reloading graphics content");
                            Microsoft.Xna.Framework.Content.ContentManager.ReloadGraphicsContent();
                            Android.Util.Log.Debug("MonoGame", "End reloading graphics content");

                            // DeviceReset events
                            _game.graphicsDeviceManager.OnDeviceReset(EventArgs.Empty);
                            _game.GraphicsDevice.OnDeviceReset();

                            IsResuming = false;
                        });

                    bgThread.Start();
                }
            }
            OnContextSet(EventArgs.Empty);
        }

        protected void ContextLostInternal()
        {
            OnContextLost(EventArgs.Empty);
            _game.graphicsDeviceManager.OnDeviceResetting(EventArgs.Empty);
            if (_game.GraphicsDevice != null)
                _game.GraphicsDevice.OnDeviceResetting();
        }

        protected virtual void OnContextLost(EventArgs eventArgs)
        {

        }

        protected virtual void OnContextSet(EventArgs eventArgs)
        {

        }

        protected virtual void OnUnload(EventArgs eventArgs)
        {

        }

        protected virtual void OnLoad(EventArgs eventArgs)
        {

        }

        protected virtual void OnStopped(EventArgs eventArgs)
        {

        }

        #region Key and Motion

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            bool handled = false;
            if (GamePad.OnKeyDown(keyCode, e))
                return true;

            handled = Keyboard.KeyDown(keyCode);

            // we need to handle the Back key here because it doesn't work any other way
            if (keyCode == Keycode.Back)
            {
                GamePad.Back = true;
                handled = true;
            }

            if (keyCode == Keycode.VolumeUp)
            {
                AudioManager audioManager = (AudioManager)Context.GetSystemService(Context.AudioService);
                audioManager.AdjustStreamVolume(Stream.Music, Adjust.Raise, VolumeNotificationFlags.ShowUi);
                return true;
            }

            if (keyCode == Keycode.VolumeDown)
            {
                AudioManager audioManager = (AudioManager)Context.GetSystemService(Context.AudioService);
                audioManager.AdjustStreamVolume(Stream.Music, Adjust.Lower, VolumeNotificationFlags.ShowUi);
                return true;
            }

            return handled;
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
                GamePad.Back = false;
            if (GamePad.OnKeyUp(keyCode, e))
                return true;
            return Keyboard.KeyUp(keyCode);
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            if (GamePad.OnGenericMotionEvent(e))
                return true;

            return base.OnGenericMotionEvent(e);
        }

        #endregion

        #region Properties

        private IEGL10 egl;
        private EGLDisplay eglDisplay;
        private EGLConfig eglConfig;
        private EGLContext eglContext;
        private EGLSurface eglSurface;

        /// <summary>The visibility of the window. Always returns true.</summary>
        /// <value></value>
        /// <exception cref="T:System.ObjectDisposed">The instance has been disposed</exception>
        public virtual bool Visible
        {
            get
            {
                EnsureUndisposed();
                return true;
            }
            set
            {
                EnsureUndisposed();
            }
        }

        /// <summary>The size of the current view.</summary>
        /// <value>A <see cref="T:System.Drawing.Size" /> which is the size of the current view.</value>
        /// <exception cref="T:System.ObjectDisposed">The instance has been disposed</exception>
        public virtual System.Drawing.Size Size
        {
            get
            {
                EnsureUndisposed();
                return size;
            }
            set
            {
                EnsureUndisposed();
                if (size != value)
                {
                    size = value;
                    OnResize(EventArgs.Empty);
                }
            }
        }

        private void OnResize(EventArgs eventArgs)
        {

        }

        #endregion

        public event FrameEvent RenderFrame;
        public event FrameEvent UpdateFrame;

        public delegate void FrameEvent(object sender, FrameEventArgs e);

        public class FrameEventArgs : EventArgs
        {
            double elapsed;

            /// <summary>
            /// Constructs a new FrameEventArgs instance.
            /// </summary>
            public FrameEventArgs()
            {
            }

            /// <summary>
            /// Constructs a new FrameEventArgs instance.
            /// </summary>
            /// <param name="elapsed">The amount of time that has elapsed since the previous event, in seconds.</param>
            public FrameEventArgs(double elapsed)
            {
                Time = elapsed;
            }

            /// <summary>
            /// Gets a <see cref="System.Double"/> that indicates how many seconds of time elapsed since the previous event.
            /// </summary>
            public double Time
            {
                get { return elapsed; }
                internal set
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException();
                    elapsed = value;
                }
            }
        }

        public BackgroundContext CreateBackgroundContext()
        {
            return new BackgroundContext(this);
        }

        public class BackgroundContext
        {

            EGLContext eglContext;
            MonoGameAndroidGameView view;
            EGLSurface surface;

            public BackgroundContext(MonoGameAndroidGameView view)
            {
                this.view = view;
                foreach (var v in MonoGame.OpenGL.GLESVersion.GetSupportedGLESVersions())
                {
                    eglContext = view.egl.EglCreateContext(view.eglDisplay, view.eglConfig, EGL10.EglNoContext, v.GetAttributes());
                    if (eglContext == null || eglContext == EGL10.EglNoContext)
                    {
                        continue;
                    }
                    break;
                }
                if (eglContext == null || eglContext == EGL10.EglNoContext)
                {
                    eglContext = null;
                    throw new Exception("Could not create EGL context" + view.GetErrorAsString());
                }
                int[] pbufferAttribList = new int[] { EGL10.EglWidth, 64, EGL10.EglHeight, 64, EGL10.EglNone };
                surface = view.CreatePBufferSurface(view.eglConfig, pbufferAttribList);
                if (surface == EGL10.EglNoSurface)
                    throw new Exception("Could not create Pbuffer Surface" + view.GetErrorAsString());
            }

            public void MakeCurrent()
            {
                view.ClearCurrent();
                view.egl.EglMakeCurrent(view.eglDisplay, surface, surface, eglContext);
            }
        }
    }
}
