// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
#if WINRT
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

#endif

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Provides basic graphics device initialization, game logic, and rendering code.
    /// </summary>
    public class Game : IDisposable
    {
        private ContentManager _content;
        private GamePlatform _platform;
        private TimeSpan _accumulatedElapsedTime;
        private GameTime _gameTime;
        private Stopwatch _gameTimer;
        private long _previousTicks;
        private int _updateFrameLag;

        private readonly SortingFilteringCollection<IDrawable> _drawables =
            new SortingFilteringCollection<IDrawable>(d => d.Visible, (d, handler) => d.VisibleChanged += handler,
                (d, handler) => d.VisibleChanged -= handler,
                (d1, d2) => Comparer<int>.Default.Compare(d1.DrawOrder, d2.DrawOrder),
                (d, handler) => d.DrawOrderChanged += handler, (d, handler) => d.DrawOrderChanged -= handler);

        private readonly SortingFilteringCollection<IUpdateable> _updateables =
            new SortingFilteringCollection<IUpdateable>(u => u.Enabled, (u, handler) => u.EnabledChanged += handler,
                (u, handler) => u.EnabledChanged -= handler,
                (u1, u2) => Comparer<int>.Default.Compare(u1.UpdateOrder, u2.UpdateOrder),
                (u, handler) => u.UpdateOrderChanged += handler, (u, handler) => u.UpdateOrderChanged -= handler);

        private static readonly Action<IDrawable, GameTime> DrawAction = (drawable, gameTime) => drawable.Draw(gameTime);

        private IGraphicsDeviceManager _graphicsDeviceManager;
        private IGraphicsDeviceService _graphicsDeviceService;
        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166667); // 60fps
        private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(0.02);
        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);

        private static readonly Action<IUpdateable, GameTime> UpdateAction =
            (updateable, gameTime) => updateable.Update(gameTime);

        private bool _suppressDraw;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of this class, which provides basic graphics device initialization, game logic,
        /// rendering code, and a game loop.
        /// </summary>
        public Game()
        {
            Instance = this;

            LaunchParameters = new LaunchParameters();
            _gameTime = new GameTime();
            Services = new GameServiceContainer();
            Components = new GameComponentCollection();
            _content = new ContentManager(Services);

            _platform = GamePlatform.Create(this);
            _platform.Activated += OnActivated;
            _platform.Deactivated += OnDeactivated;
            Services.AddService(typeof (GamePlatform), _platform);

#if WINDOWS_STOREAPP && !WINDOWS_PHONE81
            Platform.ViewStateChanged += Platform_ApplicationViewChanged;
#endif
        }

        ~Game()
        {
            Dispose(false);
        }

        /// <summary>
        /// Provides platform specfic debugging logger.
        /// </summary>
        /// <param name="message"></param>
        [Conditional("DEBUG")]
        public void Log(string message)
        {
            if (_platform != null)
                _platform.Log(message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            Raise(Disposed, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose loaded game components
                    for (var i = 0; i < Components.Count; i++)
                    {
                        var disposable = Components[i] as IDisposable;
                        if (disposable != null)
                            disposable.Dispose();
                    }
                    Components = null;

                    if (_content != null)
                    {
                        _content.Dispose();
                        _content = null;
                    }

                    if (_graphicsDeviceManager != null)
                    {
                        (_graphicsDeviceManager as GraphicsDeviceManager).Dispose();
                        _graphicsDeviceManager = null;
                    }

                    if (_platform != null)
                    {
                        _platform.Activated -= OnActivated;
                        _platform.Deactivated -= OnDeactivated;
                        Services.RemoveService(typeof (GamePlatform));
#if WINDOWS_STOREAPP && !WINDOWS_PHONE81
                        Platform.ViewStateChanged -= Platform_ApplicationViewChanged;
#endif
                        _platform.Dispose();
                        _platform = null;
                    }

                    ContentTypeReaderManager.ClearTypeCreators();

                    SoundEffect.PlatformShutdown();
                }
#if ANDROID
                Activity = null;
#endif
                _isDisposed = true;
                Instance = null;
            }
        }

        [DebuggerNonUserCode]
        private void AssertNotDisposed()
        {
            if (_isDisposed)
            {
                var name = GetType().Name;
                throw new ObjectDisposedException(name,
                    string.Format("The {0} object was used after being Disposed.", name));
            }
        }

#if ANDROID
        [CLSCompliant(false)]
        public static AndroidGameActivity Activity { get; internal set; }
#endif

        /// <summary>
        /// Gets an instance of the game.
        /// </summary>
        internal static Game Instance { get; private set; }

        /// <summary>
        /// Gets the start up parameters in LaunchParameters.
        /// </summary>
        public LaunchParameters LaunchParameters { get; private set; }

        /// <summary>
        /// Gets the collection of GameComponents owned by the game.
        /// </summary>
        public GameComponentCollection Components { get; private set; }

        /// <summary>
        /// Gets or sets the time to sleep when the game is inactive.
        /// </summary>
        public TimeSpan InactiveSleepTime
        {
            get { return _inactiveSleepTime; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("The time must be positive.", default(Exception));

                _inactiveSleepTime = value;
            }
        }

        /// <summary>
        /// The maximum amount of time we will frameskip over and only perform Update calls with no Draw calls.
        /// MonoGame extension.
        /// </summary>
        public TimeSpan MaxElapsedTime
        {
            get { return _maxElapsedTime; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("The time must be positive.", default(Exception));
                if (value < _targetElapsedTime)
                {
                    throw new ArgumentOutOfRangeException("The time must be at least TargetElapsedTime",
                        default(Exception));
                }

                _maxElapsedTime = value;
            }
        }

        /// <summary>
        /// Indicates whether the game is currently the active application.
        /// </summary>
        public bool IsActive
        {
            get { return _platform.IsActive; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the mouse cursor should be visible.
        /// </summary>
        public bool IsMouseVisible
        {
            get { return _platform.IsMouseVisible; }
            set { _platform.IsMouseVisible = value; }
        }

        /// <summary>
        /// Gets or sets the target time between calls to Update when IsFixedTimeStep is true.
        /// </summary>
        public TimeSpan TargetElapsedTime
        {
            get { return _targetElapsedTime; }
            set
            {
                // Give GamePlatform implementations an opportunity to override
                // the new value.
                value = _platform.TargetElapsedTimeChanging(value);

                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("The time must be positive and non-zero.", default(Exception));

                if (value != _targetElapsedTime)
                {
                    _targetElapsedTime = value;
                    _platform.TargetElapsedTimeChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use fixed time steps.
        /// </summary>
        public bool IsFixedTimeStep { get; set; } = true;

        /// <summary>
        /// Gets the <see cref="GameServiceContainer" /> holding all the service providers attached to the Game.
        /// </summary>
        public GameServiceContainer Services { get; }

        /// <summary>
        /// Gets or sets the current ContentManager.
        /// </summary>
        public ContentManager Content
        {
            get { return _content; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                _content = value;
            }
        }

        /// <summary>
        /// Gets the current <see cref="GraphicsDevice" />.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (_graphicsDeviceService == null)
                {
                    _graphicsDeviceService =
                        (IGraphicsDeviceService) Services.GetService(typeof (IGraphicsDeviceService));

                    if (_graphicsDeviceService == null)
                        throw new InvalidOperationException("No Graphics Device Service");
                }
                return _graphicsDeviceService.GraphicsDevice;
            }
        }

        /// <summary>
        /// Gets the underlying operating system window.
        /// </summary>
        [CLSCompliant(false)]
        public GameWindow Window
        {
            get { return _platform.Window; }
        }

        /// <summary>
        /// Gets or sets if the game is initialized.
        /// </summary>
        private bool Initialized { get; set; }

        [CLSCompliant(false)]
        public ApplicationExecutionState PreviousExecutionState { get; internal set; }

        internal GraphicsDeviceManager GraphicsDeviceManager
        {
            get
            {
                if (_graphicsDeviceManager == null)
                {
                    _graphicsDeviceManager =
                        (IGraphicsDeviceManager) Services.GetService(typeof (IGraphicsDeviceManager));

                    if (_graphicsDeviceManager == null)
                        throw new InvalidOperationException("No Graphics Device Manager");
                }
                return (GraphicsDeviceManager) _graphicsDeviceManager;
            }
        }

        /// <summary>
        /// Raised when the game gains focus.
        /// </summary>
        public event EventHandler<EventArgs> Activated;

        /// <summary>
        /// Raised when the game loses focus.
        /// </summary>
        public event EventHandler<EventArgs> Deactivated;

        /// <summary>
        /// Raised when the game is being disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposed;

        /// <summary>
        /// Raised when the game is exiting.
        /// </summary>
        public event EventHandler<EventArgs> Exiting;

#if WINDOWS_STOREAPP && !WINDOWS_PHONE81
        [CLSCompliant(false)]
        public event EventHandler<ViewStateChangedEventArgs> ApplicationViewChanged;
#endif

#if WINRT
#endif

        /// <summary>
        /// Exits the game.
        /// </summary>
#if IOS || WINDOWS_STOREAPP && !WINDOWS_PHONE81
        [Obsolete("The current platform does not allow games to exit.", true)]
#endif
        public void Exit()
        {
            _platform.Exit();
            _suppressDraw = true;
        }

        /// <summary>
        /// Resets the elapsed time counter.
        /// </summary>
        public void ResetElapsedTime()
        {
            _platform.ResetElapsedTime();
            _gameTimer.Reset();
            _gameTimer.Start();
            _accumulatedElapsedTime = TimeSpan.Zero;
            _gameTime.ElapsedGameTime = TimeSpan.Zero;
            _previousTicks = 0L;
        }

        /// <summary>
        /// Suppress drawing.
        /// </summary>
        public void SuppressDraw()
        {
            _suppressDraw = true;
        }

        /// <summary>
        /// Run the game through what would happen in a single tick of the game clock; this method is designed for debugging
        /// only.
        /// </summary>
        public void RunOneFrame()
        {
            if (_platform == null)
                return;

            if (!_platform.BeforeRun())
                return;

            if (!Initialized)
            {
                DoInitialize();
                _gameTimer = Stopwatch.StartNew();
                Initialized = true;
            }

            BeginRun();
            Tick();
            EndRun();
        }

        /// <summary>
        /// Call this method to initialize the game using the default run behavior.
        /// </summary>
        public void Run()
        {
            Run(_platform.DefaultRunBehavior);
        }

        /// <summary>
        /// Call this method to initialize the game, begin running the game loop, and start processing events for the game.
        /// </summary>
        public void Run(GameRunBehavior runBehavior)
        {
            AssertNotDisposed();
            if (!_platform.BeforeRun())
            {
                BeginRun();
                _gameTimer = Stopwatch.StartNew();
                return;
            }

            if (!Initialized)
            {
                DoInitialize();
                Initialized = true;
            }

            BeginRun();
            _gameTimer = Stopwatch.StartNew();
            switch (runBehavior)
            {
                case GameRunBehavior.Asynchronous:
                    _platform.AsyncRunLoopEnded += Platform_AsyncRunLoopEnded;
                    _platform.StartRunLoop();
                    break;

                case GameRunBehavior.Synchronous:
                    _platform.RunLoop();
                    EndRun();
                    DoExiting();
                    break;

                default:
                    throw new ArgumentException(string.Format("Handling for the run behavior {0} is not implemented.",
                        runBehavior));
            }
        }

        /// <summary>
        /// Updates the game's clock and calls Update and Draw.
        /// </summary>
        public void Tick()
        {
            // NOTE: This code is very sensitive and can break very badly
            // with even what looks like a safe change.  Be sure to test
            // any change fully in both the fixed and variable timestep
            // modes across multiple devices and platforms.

            RetryTick:

            // Advance the accumulated elapsed time.
            var currentTicks = _gameTimer.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
            _previousTicks = currentTicks;

            // If we're in the fixed timestep mode and not enough time has elapsed
            // to perform an update we sleep off the the remaining time to save battery
            // life and/or release CPU time to other threads and processes.
            if (IsFixedTimeStep && _accumulatedElapsedTime < TargetElapsedTime)
            {
                var sleepTime = (int) (TargetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;

                // NOTE: While sleep can be inaccurate in general it is
                // accurate enough for frame limiting purposes if some
                // fluctuation is an acceptable result.
#if WINRT
                Task.Delay(sleepTime).Wait();
#else
                System.Threading.Thread.Sleep(sleepTime);
#endif
                goto RetryTick;
            }

            // Do not allow any update to take longer than our maximum.
            if (_accumulatedElapsedTime > _maxElapsedTime)
                _accumulatedElapsedTime = _maxElapsedTime;

            if (IsFixedTimeStep)
            {
                _gameTime.ElapsedGameTime = TargetElapsedTime;
                var stepCount = 0;

                // Perform as many full fixed length time steps as we can.
                while (_accumulatedElapsedTime >= TargetElapsedTime)
                {
                    _gameTime.TotalGameTime += TargetElapsedTime;
                    _accumulatedElapsedTime -= TargetElapsedTime;
                    ++stepCount;

                    DoUpdate(_gameTime);
                }

                //Every update after the first accumulates lag
                _updateFrameLag += Math.Max(0, stepCount - 1);

                //If we think we are running slowly, wait until the lag clears before resetting it
                if (_gameTime.IsRunningSlowly)
                {
                    if (_updateFrameLag == 0)
                        _gameTime.IsRunningSlowly = false;
                }
                else if (_updateFrameLag >= 5)
                {
                    //If we lag more than 5 frames, start thinking we are running slowly
                    _gameTime.IsRunningSlowly = true;
                }

                //Every time we just do one update and one draw, then we are not running slowly, so decrease the lag
                if (stepCount == 1 && _updateFrameLag > 0)
                    _updateFrameLag--;

                // Draw needs to know the total elapsed time
                // that occured for the fixed length updates.
                _gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
            }
            else
            {
                // Perform a single variable length update.
                _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
                _gameTime.TotalGameTime += _accumulatedElapsedTime;
                _accumulatedElapsedTime = TimeSpan.Zero;

                DoUpdate(_gameTime);
            }

            // Draw unless the update suppressed it.
            if (_suppressDraw)
                _suppressDraw = false;
            else
                DoDraw(_gameTime);
        }

        /// <summary>
        /// Starts the drawing of a frame. This method is followed by calls to Draw and EndDraw.
        /// </summary>
        /// <returns></returns>
        protected virtual bool BeginDraw()
        {
            return true;
        }

        /// <summary>
        /// Ends the drawing of a frame. This method is preceeded by calls to Draw and BeginDraw.
        /// </summary>
        protected virtual void EndDraw()
        {
            _platform.Present();
        }

        /// <summary>
        /// Called after all components are initialized but before the first update in the game loop.
        /// </summary>
        protected virtual void BeginRun()
        {
        }

        /// <summary>
        /// Called after the game loop has stopped running before exiting.
        /// </summary>
        protected virtual void EndRun()
        {
        }

        /// <summary>
        /// Called when graphics resources need to be loaded.
        /// </summary>
        protected virtual void LoadContent()
        {
        }

        /// <summary>
        /// Called when graphics resources need to be unloaded. Override this method to unload any game-specific graphics
        /// resources.
        /// </summary>
        protected virtual void UnloadContent()
        {
        }

        /// <summary>
        /// Called after the Game and GraphicsDevice are created, but before LoadContent.
        /// </summary>
        protected virtual void Initialize()
        {
            // TODO: We shouldn't need to do this here.
            ApplyChanges(GraphicsDeviceManager);

            // According to the information given on MSDN (see link below), all
            // GameComponents in Components at the time Initialize() is called
            // are initialized.
            // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.game.initialize.aspx
            // Initialize all existing components
            InitializeExistingComponents();

            _graphicsDeviceService = (IGraphicsDeviceService) Services.GetService(typeof (IGraphicsDeviceService));

            // FIXME: If this test fails, is LoadContent ever called?  This
            //        seems like a condition that warrants an exception more
            //        than a silent failure.
            if (_graphicsDeviceService != null && _graphicsDeviceService.GraphicsDevice != null)
                LoadContent();
        }

        protected virtual void Draw(GameTime gameTime)
        {
            _drawables.ForEachFilteredItem(DrawAction, gameTime);
        }

        protected virtual void Update(GameTime gameTime)
        {
            _updateables.ForEachFilteredItem(UpdateAction, gameTime);
        }

        /// <summary>
        /// Raises an Exiting event. Override this method to add code to handle when the game is exiting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnExiting(object sender, EventArgs args)
        {
            Raise(Exiting, args);
        }

        /// <summary>
        /// Raises the Activated event. Override this method to add code to handle when the game gains focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnActivated(object sender, EventArgs args)
        {
            AssertNotDisposed();
            Raise(Activated, args);
        }

        /// <summary>
        /// Raises the Deactivated event. Override this method to add code to handle when the game loses focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnDeactivated(object sender, EventArgs args)
        {
            AssertNotDisposed();
            Raise(Deactivated, args);
        }

        private void OnComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            e.GameComponent.Initialize();
            InsertComponentToCollection(e.GameComponent);
        }

        private void OnComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            RemoveComponentFromCollection(e.GameComponent);
        }

        private void Platform_AsyncRunLoopEnded(object sender, EventArgs e)
        {
            AssertNotDisposed();

            var platform = (GamePlatform) sender;
            platform.AsyncRunLoopEnded -= Platform_AsyncRunLoopEnded;
            EndRun();
            DoExiting();
        }

#if WINDOWS_STOREAPP && !WINDOWS_PHONE81
        private void Platform_ApplicationViewChanged(object sender, ViewStateChangedEventArgs e)
        {
            AssertNotDisposed();
            Raise(ApplicationViewChanged, e);
        }
#endif

        // FIXME: We should work toward eliminating internal methods.  They
        //        break entirely the possibility that additional platforms could
        //        be added by third parties without changing MonoGame itself.

        internal void ApplyChanges(GraphicsDeviceManager manager)
        {
            _platform.BeginScreenDeviceChange(GraphicsDevice.PresentationParameters.IsFullScreen);

#if !(WINDOWS && DIRECTX)

            if (GraphicsDevice.PresentationParameters.IsFullScreen)
                _platform.EnterFullScreen();
            else
                _platform.ExitFullScreen();
#endif
            var viewport = new Viewport(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight);

            GraphicsDevice.Viewport = viewport;
            _platform.EndScreenDeviceChange(string.Empty, viewport.Width, viewport.Height);
        }

        internal void DoUpdate(GameTime gameTime)
        {
            AssertNotDisposed();
            if (_platform.BeforeUpdate(gameTime))
            {
                // Once per frame, we need to check currently
                // playing sounds to see if they've stopped,
                // and return them back to the pool if so.
                SoundEffectInstancePool.Update();

                Update(gameTime);

                //The TouchPanel needs to know the time for when touches arrive
                TouchPanelState.CurrentTimestamp = gameTime.TotalGameTime;
            }
        }

        internal void DoDraw(GameTime gameTime)
        {
            AssertNotDisposed();
            // Draw and EndDraw should not be called if BeginDraw returns false.
            // http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
            // http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
            if (_platform.BeforeDraw(gameTime) && BeginDraw())
            {
                Draw(gameTime);
                EndDraw();
            }
        }

        internal void DoInitialize()
        {
            AssertNotDisposed();
            _platform.BeforeInitialize();
            Initialize();
            OrganizeComponents();
            Components.ComponentAdded += OnComponentAdded;
            Components.ComponentRemoved += OnComponentRemoved;
        }

        internal void DoExiting()
        {
            OnExiting(this, EventArgs.Empty);
            UnloadContent();
        }

        // NOTE: InitializeExistingComponents really should only be called once.
        //       Game.Initialize is the only method in a position to guarantee
        //       that no component will get a duplicate Initialize call.
        //       Further calls to Initialize occur immediately in response to
        //       Components.ComponentAdded.
        private void InitializeExistingComponents()
        {
            // TODO: Would be nice to get rid of this copy, but since it only
            //       happens once per game, it's fairly low priority.
            var copy = new IGameComponent[Components.Count];
            Components.CopyTo(copy, 0);
            foreach (var component in copy)
                component.Initialize();
        }

        private void OrganizeComponents()
        {
            ClearComponents();
            foreach (var x in Components)
                InsertComponentToCollection(x);
        }

        private void ClearComponents()
        {
            _updateables.Clear();
            _drawables.Clear();
        }

        private void InsertComponentToCollection(IGameComponent component)
        {
            if (component is IUpdateable)
                _updateables.Add((IUpdateable) component);
            if (component is IDrawable)
                _drawables.Add((IDrawable) component);
        }

        private void RemoveComponentFromCollection(IGameComponent component)
        {
            if (component is IUpdateable)
                _updateables.Remove((IUpdateable) component);
            if (component is IDrawable)
                _drawables.Remove((IDrawable) component);
        }

        private void Raise<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs e) where TEventArgs : EventArgs
        {
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// The SortingFilteringCollection class provides efficient, reusable
        /// sorting and filtering based on a configurable sort comparer, filter
        /// predicate, and associate change events.
        /// </summary>
        private class SortingFilteringCollection<T> : ICollection<T>
        {
            private static readonly Comparison<int> RemoveJournalSortComparison =
                (x, y) => Comparer<int>.Default.Compare(y, x); // Sort high to low

            private readonly List<AddJournalEntry<T>> _addJournal;
            private readonly Comparison<AddJournalEntry<T>> _addJournalSortComparison;
            private readonly List<T> _cachedFilteredItems;
            private readonly Predicate<T> _filter;
            private readonly Action<T, EventHandler<EventArgs>> _filterChangedSubscriber;
            private readonly Action<T, EventHandler<EventArgs>> _filterChangedUnsubscriber;
            private readonly List<T> _items;
            private readonly List<int> _removeJournal;
            private readonly Comparison<T> _sort;
            private readonly Action<T, EventHandler<EventArgs>> _sortChangedSubscriber;
            private readonly Action<T, EventHandler<EventArgs>> _sortChangedUnsubscriber;
            private bool _shouldRebuildCache;

            public SortingFilteringCollection(Predicate<T> filter,
                Action<T, EventHandler<EventArgs>> filterChangedSubscriber,
                Action<T, EventHandler<EventArgs>> filterChangedUnsubscriber, Comparison<T> sort,
                Action<T, EventHandler<EventArgs>> sortChangedSubscriber,
                Action<T, EventHandler<EventArgs>> sortChangedUnsubscriber)
            {
                _items = new List<T>();
                _addJournal = new List<AddJournalEntry<T>>();
                _removeJournal = new List<int>();
                _cachedFilteredItems = new List<T>();
                _shouldRebuildCache = true;

                _filter = filter;
                _filterChangedSubscriber = filterChangedSubscriber;
                _filterChangedUnsubscriber = filterChangedUnsubscriber;
                _sort = sort;
                _sortChangedSubscriber = sortChangedSubscriber;
                _sortChangedUnsubscriber = sortChangedUnsubscriber;

                _addJournalSortComparison = CompareAddJournalEntry;
            }

            public void Add(T item)
            {
                // NOTE: We subscribe to item events after items in _addJournal
                //       have been merged.
                _addJournal.Add(new AddJournalEntry<T>(_addJournal.Count, item));
                InvalidateCache();
            }

            public bool Remove(T item)
            {
                if (_addJournal.Remove(AddJournalEntry<T>.CreateKey(item)))
                    return true;

                var index = _items.IndexOf(item);
                if (index >= 0)
                {
                    UnsubscribeFromItemEvents(item);
                    _removeJournal.Add(index);
                    InvalidateCache();
                    return true;
                }
                return false;
            }

            public void Clear()
            {
                foreach (var t in _items)
                {
                    _filterChangedUnsubscriber(t, Item_FilterPropertyChanged);
                    _sortChangedUnsubscriber(t, Item_SortPropertyChanged);
                }

                _addJournal.Clear();
                _removeJournal.Clear();
                _items.Clear();

                InvalidateCache();
            }

            public bool Contains(T item)
            {
                return _items.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _items.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return _items.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable) _items).GetEnumerator();
            }

            private int CompareAddJournalEntry(AddJournalEntry<T> x, AddJournalEntry<T> y)
            {
                var result = _sort(x.Item, y.Item);
                if (result != 0)
                    return result;
                return x.Order - y.Order;
            }

            public void ForEachFilteredItem<TUserData>(Action<T, TUserData> action, TUserData userData)
            {
                if (_shouldRebuildCache)
                {
                    ProcessRemoveJournal();
                    ProcessAddJournal();

                    // Rebuild the cache
                    _cachedFilteredItems.Clear();
                    for (var i = 0; i < _items.Count; ++i)
                    {
                        if (_filter(_items[i]))
                            _cachedFilteredItems.Add(_items[i]);
                    }

                    _shouldRebuildCache = false;
                }

                foreach (var i in _cachedFilteredItems)
                    action(i, userData);

                // If the cache was invalidated as a result of processing items,
                // now is a good time to clear it and give the GC (more of) a
                // chance to do its thing.
                if (_shouldRebuildCache)
                    _cachedFilteredItems.Clear();
            }

            private void ProcessRemoveJournal()
            {
                if (_removeJournal.Count == 0)
                    return;

                // Remove items in reverse.  (Technically there exist faster
                // ways to bulk-remove from a variable-length array, but List<T>
                // does not provide such a method.)
                _removeJournal.Sort(RemoveJournalSortComparison);
                for (var i = 0; i < _removeJournal.Count; ++i)
                    _items.RemoveAt(_removeJournal[i]);
                _removeJournal.Clear();
            }

            private void ProcessAddJournal()
            {
                if (_addJournal.Count == 0)
                    return;

                // Prepare the _addJournal to be merge-sorted with _items.
                // _items is already sorted (because it is always sorted).
                _addJournal.Sort(_addJournalSortComparison);

                var iAddJournal = 0;
                var iItems = 0;

                while (iItems < _items.Count && iAddJournal < _addJournal.Count)
                {
                    var addJournalItem = _addJournal[iAddJournal].Item;
                    // If addJournalItem is less than (belongs before)
                    // _items[iItems], insert it.
                    if (_sort(addJournalItem, _items[iItems]) < 0)
                    {
                        SubscribeToItemEvents(addJournalItem);
                        _items.Insert(iItems, addJournalItem);
                        ++iAddJournal;
                    }
                    // Always increment iItems, either because we inserted and
                    // need to move past the insertion, or because we didn't
                    // insert and need to consider the next element.
                    ++iItems;
                }

                // If _addJournal had any "tail" items, append them all now.
                for (; iAddJournal < _addJournal.Count; ++iAddJournal)
                {
                    var addJournalItem = _addJournal[iAddJournal].Item;
                    SubscribeToItemEvents(addJournalItem);
                    _items.Add(addJournalItem);
                }

                _addJournal.Clear();
            }

            private void SubscribeToItemEvents(T item)
            {
                _filterChangedSubscriber(item, Item_FilterPropertyChanged);
                _sortChangedSubscriber(item, Item_SortPropertyChanged);
            }

            private void UnsubscribeFromItemEvents(T item)
            {
                _filterChangedUnsubscriber(item, Item_FilterPropertyChanged);
                _sortChangedUnsubscriber(item, Item_SortPropertyChanged);
            }

            private void InvalidateCache()
            {
                _shouldRebuildCache = true;
            }

            private void Item_FilterPropertyChanged(object sender, EventArgs e)
            {
                InvalidateCache();
            }

            private void Item_SortPropertyChanged(object sender, EventArgs e)
            {
                var item = (T) sender;
                var index = _items.IndexOf(item);

                _addJournal.Add(new AddJournalEntry<T>(_addJournal.Count, item));
                _removeJournal.Add(index);

                // Until the item is back in place, we don't care about its
                // events.  We will re-subscribe when _addJournal is processed.
                UnsubscribeFromItemEvents(item);
                InvalidateCache();
            }
        }

        private struct AddJournalEntry<T>
        {
            public readonly T Item;
            public readonly int Order;

            public AddJournalEntry(int order, T item)
            {
                Order = order;
                Item = item;
            }

            public static AddJournalEntry<T> CreateKey(T item)
            {
                return new AddJournalEntry<T>(-1, item);
            }

            public override int GetHashCode()
            {
                return Item.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is AddJournalEntry<T>))
                    return false;

                return Equals(Item, ((AddJournalEntry<T>) obj).Item);
            }
        }
    }
}