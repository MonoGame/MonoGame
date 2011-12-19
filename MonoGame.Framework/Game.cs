// FIXME: Figure out what license is appropriate here.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    public class Game : IDisposable
    {
        // FIXME:  What does this constant actually represent?  The target FPS?
        private const float FramesPerSecond = 60.0f; // ~60 frames per second

        // I do believe we can take out the next three variables.
        //  After the release this should be looked at as the time is
        //  passed from the GameWindow which controlls when updating is to
        //  be done.
        private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;

        private GameComponentCollection _components;
        private ContentManager _content;
        private GameServiceContainer _services;
        private GamePlatform _platform;

        private SortingFilteringCollection<IDrawable> _drawables =
            new SortingFilteringCollection<IDrawable>(
                d => d.Visible,
                (d, handler) => d.VisibleChanged += handler,
                (d, handler) => d.VisibleChanged -= handler,
                (d1, d2) => d1.DrawOrder - d2.DrawOrder,
                (d, handler) => d.DrawOrderChanged += handler,
                (d, handler) => d.DrawOrderChanged -= handler);

        private SortingFilteringCollection<IUpdateable> _updateables =
            new SortingFilteringCollection<IUpdateable>(
                u => u.Enabled,
                (u, handler) => u.EnabledChanged += handler,
                (u, handler) => u.EnabledChanged -= handler,
                (u1, u2) => u1.UpdateOrder - u2.UpdateOrder,
                (u, handler) => u.UpdateOrderChanged += handler,
                (u, handler) => u.UpdateOrderChanged -= handler);

        private IGraphicsDeviceManager _graphicsDeviceManager;
        private IGraphicsDeviceService _graphicsDeviceService;

        private bool _initialized = false;
        private bool _isActive = false;
        private bool _isFixedTimeStep = true;
        private bool _isMouseVisible = false;
        internal bool _playingVideo = false;

        private TimeSpan _targetElapsedTime = TimeSpan.FromSeconds(1 / FramesPerSecond);

        // Mac-specific variables
        private bool _shouldDraw = true;

        public Game()
        {
            _services = new GameServiceContainer();
            _components = new GameComponentCollection();
            _content = new ContentManager(_services);

            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();

            _platform = GamePlatform.Create(this);
            _platform.Activated += Platform_Activated;
            _platform.Deactivated += Platform_Deactivated;
        }

        ~Game()
        {
            Dispose(false);
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            Raise(Disposed, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _platform.Dispose();
            }
        }

        #endregion IDisposable Implementation

        #region Properties

        public GameComponentCollection Components
        {
            get { return _components; }
        }

        public bool IsActive
        {
            get { return _platform.IsActive; }
        }

        public bool IsMouseVisible
        {
            get { return _platform.IsMouseVisible; }
            set { _platform.IsMouseVisible = value; }
        }

        public TimeSpan TargetElapsedTime
        {
            get { return _targetElapsedTime; }
            set
            {
                // Give GamePlatform implementations an opportunity to override the new value.
                value = _platform.TargetElapsedTimeChanging(value);

                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(
                        "value must be positive and non-zero.");

                if (value != _targetElapsedTime)
                {
                    _targetElapsedTime = value;
                    _platform.TargetElapsedTimeChanged();
                }
            }
        }

        public bool IsFixedTimeStep
        {
            get { return _isFixedTimeStep; }
            set { _isFixedTimeStep = value; }
        }

        public GameServiceContainer Services {
            get { return _services; }
        }

        public ContentManager Content
        {
            get { return _content; }
        }

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (_graphicsDeviceService == null)
                {
                    _graphicsDeviceService = (IGraphicsDeviceService)
                        Services.GetService(typeof(IGraphicsDeviceService));

                    if (_graphicsDeviceService == null)
                        throw new InvalidOperationException("No Graphics Device Service");
                }
                return _graphicsDeviceService.GraphicsDevice;
            }
        }

        public GameWindow Window
        {
            get { return _platform.Window; }
        }

        #endregion Properties

        #region Internal Properties

        // FIXME: Internal members should be eliminated.
        internal bool Initialized
        {
            get { return _initialized; }
        }

        #endregion Internal Properties

        #region Events

        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<EventArgs> Exiting;

        #endregion

        #region Public Methods

        public void Exit()
        {
            Raise(Exiting, EventArgs.Empty);
            _platform.Exit();
        }

        public void ResetElapsedTime()
        {
            _lastUpdate = DateTime.Now;
        }

        public void Run(bool asynchronous=false)
        {
            _lastUpdate = DateTime.Now;

            // In an original XNA game the GraphicsDevice property is null
            // during initialization but before the Game's Initialize method is
            // called the property is available so we can only assume that it
            // should be created somewhere in here.  We cannot set the viewport
            // values correctly based on the Preferred settings which is causing
            // some problems on some Microsoft samples which we are not handling
            // correctly.
            graphicsDeviceManager.CreateDevice();

            var manager = (GraphicsDeviceManager)Services.GetService(
                typeof(IGraphicsDeviceManager));

            applyChanges(manager);

            // 1. Sort components into IUpdateable and IDrawable lists.
            // 2. Subscribe to Added/Removed events to keep these lists synced.
            CategorizeComponents();
            _components.ComponentAdded += Components_ComponentAdded;
            _components.ComponentRemoved += Components_ComponentRemoved;

            if (!_platform.BeforeRun())
                return;

            Initialize();
            _initialized = true;

            BeginRun();
            if (asynchronous)
            {
                _platform.AsyncRunLoopEnded += Platform_AsyncRunLoopEnded;
                _platform.StartRunLoop();
            }
            else
            {
                _platform.RunLoop();
                EndRun();
            }
        }

        #endregion

        #region Protected Methods

        protected virtual bool BeginDraw() { return true; }
        protected virtual void EndDraw() { }

        protected virtual void BeginRun() { }
        protected virtual void EndRun() { }

        protected virtual void LoadContent() { }
        protected virtual void UnloadContent() { }

        protected virtual void Initialize()
        {
            _platform.BeforeInitialize();

            // According to the information given on MSDN (see link below), all
            // GameComponents in Components at the time Initialize() is called
            // are initialized.
            // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.game.initialize.aspx
            InitializeExistingComponents();

            _graphicsDeviceService = (IGraphicsDeviceService)
                Services.GetService(typeof(IGraphicsDeviceService));

            // FIXME: If this test fails, is LoadContent ever called?  This
            //        seems like a condition that warrants an exception more
            //        than a silent failure.
            if (_graphicsDeviceService != null &&
                _graphicsDeviceService.GraphicsDevice != null)
            {
                LoadContent();
            }
        }

        protected virtual void Draw(GameTime gameTime)
        {
            if (!_platform.BeforeDraw(gameTime) || _playingVideo)
                return;

            _drawables.ForEachFilteredItem(d => d.Draw(gameTime));
        }

        protected virtual void Update(GameTime gameTime)
        {
            if (!_platform.BeforeUpdate(gameTime))
                return;

            _updateables.ForEachFilteredItem(u => u.Update(gameTime));
        }

        #endregion Protected Methods

        #region Event Handlers

        private void Components_ComponentAdded(
            object sender, GameComponentCollectionEventArgs e)
        {
            // Since we only subscribe to ComponentAdded after the graphics
            // devices are set up, it is safe to just blindly call Initialize.
            e.GameComponent.Initialize();
            CategorizeComponent(e.GameComponent);
        }

        private void Components_ComponentRemoved(
            object sender, GameComponentCollectionEventArgs e)
        {
            DecategorizeComponent(e.GameComponent);
        }

        private void Platform_AsyncRunLoopEnded(object sender, EventArgs e)
        {
            _platform.AsyncRunLoopEnded -= Platform_AsyncRunLoopEnded;
            EndRun();
        }

        private void Platform_Activated (object sender, EventArgs e)
        {
            Raise(Activated, e);
        }

        private void Platform_Deactivated (object sender, EventArgs e)
        {
            Raise(Deactivated, e);
        }

        #endregion Event Handlers

        #region Internal Methods

        internal void applyChanges(GraphicsDeviceManager manager)
        {
            var viewport = new Viewport();

            viewport.X = 0;
            viewport.Y = 0;
            viewport.Width = manager.PreferredBackBufferWidth;
            viewport.Height = manager.PreferredBackBufferHeight;

            GraphicsDevice.Viewport = viewport;

            if (GraphicsDevice.PresentationParameters.IsFullScreen)
                _platform.EnterFullScreen();
            else
                _platform.ExitFullScreen();
        }

        internal void DoUpdate(GameTime gameTime)
        {
            // FIXME: Should we really be checking _shouldDraw before doing an
            //        update?  And if the answer is "yes", shouldn't it be
            //        called _shouldDrawAndUpdate.
            // FIXME: What is the difference between _shouldDraw and _isActive?
            if (_isActive && _shouldDraw)
                Update(gameTime);
        }

        internal void DoDraw(GameTime gameTime)
        {
            // Draw and EndDraw should not be called if BeginDraw returns false.
            // http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
            // http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
            // FIXME: What is the difference between _shouldDraw and _isActive?
            if (_isActive && _shouldDraw && BeginDraw())
            {
                Draw(gameTime);
                EndDraw();
            }
        }

        // FIXME: Can EnterForeground be pushed into GamePlatform
        //        implementations that need it, rather than hanging around
        //        as an internal method here?
        internal void EnterForeground()
        {
            // FIXME: What is the difference between _shouldDraw and _isActive?
            _shouldDraw = true;
            _isActive = true;
            _platform.EnterForeground();
            Raise(Activated, EventArgs.Empty);
        }


        // FIXME: Can EnterBackground be pushed into GamePlatform
        //        implementations that need it, rather than hanging around
        //        as an internal method here?
        internal void EnterBackground()
        {
            // FIXME: What is the difference between _shouldDraw and _isActive?
            _shouldDraw = false;
            _isActive = false;
            _platform.EnterBackground();
            Raise(Deactivated, EventArgs.Empty);
        }

        #endregion Internal Methods

        internal GraphicsDeviceManager graphicsDeviceManager
        {
            get
            {
                if (_graphicsDeviceManager == null)
                {
                    _graphicsDeviceManager = (IGraphicsDeviceManager)
                        Services.GetService(typeof(IGraphicsDeviceManager));

                    if (_graphicsDeviceManager == null)
                        throw new InvalidOperationException ("No Graphics Device Manager");
                }
                return (GraphicsDeviceManager)_graphicsDeviceManager;
            }
        }

        // NOTE: InitializeExistingComponents really should only be called once.
        //       Game.Initialize is the only method in a position to guarantee
        //       that no component will get a duplicate Initialize call.
        //       Further calls to Initialize occur immediately in response to
        //       Components.ComponentAdded.
        private void InitializeExistingComponents()
        {
            for (int i = Components.Count - 1; i >= 0; --i)
                Components[i].Initialize();
        }

        private void CategorizeComponents()
        {
            DecategorizeComponents();
            for (int i = Components.Count - 1; i >= 0; --i)
                CategorizeComponent(Components[i]);
        }

        // FIXME: I am open to a better name for this method.  It does the
        //        opposite of CategorizeComponents.
        private void DecategorizeComponents()
        {
            _updateables.Clear();
            _drawables.Clear();
        }

        private void CategorizeComponent(IGameComponent component)
        {
            if (component is IUpdateable)
                _updateables.Add((IUpdateable)component);
            if (component is IDrawable)
                _drawables.Add((IDrawable)component);
        }

        // FIXME: I am open to a better name for this method.  It does the
        //        opposite of CategorizeComponent.
        private void DecategorizeComponent(IGameComponent component)
        {
            if (component is IUpdateable)
                _updateables.Remove((IUpdateable)component);
            if (component is IDrawable)
                _drawables.Remove((IDrawable)component);
        }

        private void Raise<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs e)
            where TEventArgs : EventArgs
        {
            if (handler != null)
                handler(this, e);
        }

        class SortingFilteringCollection<T> : IComparer<T>
        {
            private List<T> _items;
            private List<T> _addJournal;
            private List<int> _removeJournal;
            private List<T> _cachedFilteredItems;
            private bool _shouldRebuildCache;

            private Predicate<T> _filter;
            private Comparison<T> _sort;
            private Action<T, EventHandler> _filterChangedSubscriber;
            private Action<T, EventHandler> _filterChangedUnsubscriber;
            private Action<T, EventHandler> _sortChangedSubscriber;
            private Action<T, EventHandler> _sortChangedUnsubscriber;

            public SortingFilteringCollection(
                Predicate<T> filter,
                Action<T, EventHandler> filterChangedSubscriber,
                Action<T, EventHandler> filterChangedUnsubscriber,
                Comparison<T> sort,
                Action<T, EventHandler> sortChangedSubscriber,
                Action<T, EventHandler> sortChangedUnsubscriber)
            {
                _items = new List<T>();
                _addJournal = new List<T>();
                _removeJournal = new List<int>();
                _cachedFilteredItems = new List<T>();
                _shouldRebuildCache = true;

                _filter = filter;
                _filterChangedSubscriber = filterChangedSubscriber;
                _filterChangedUnsubscriber = filterChangedUnsubscriber;
                _sort = sort;
                _sortChangedSubscriber = sortChangedSubscriber;
                _sortChangedUnsubscriber = sortChangedUnsubscriber;
            }

            public void ForEachFilteredItem(Action<T> action)
            {
                if (_shouldRebuildCache)
                {
                    ProcessRemoveJournal();
                    ProcessAddJournal();

                    // Rebuild the cache
                    _cachedFilteredItems.Clear();
                    _items.ForEach(item => {
                        if (_filter(item))
                            _cachedFilteredItems.Add(item);
                    });

                    _shouldRebuildCache = false;
                }

                _cachedFilteredItems.ForEach(action);

                // If the cache was invalidated as a result of processing items,
                // now is a good time to clear it and give the GC (more of) a
                // chance to do its thing.
                if (_shouldRebuildCache)
                    _cachedFilteredItems.Clear();
            }

            public void Add(T item)
            {
                // NOTE: We subscribe to item events after items in _addJournal
                //       have been merged.
                _addJournal.Add(item);
                InvalidateCache();
            }

            public bool Remove(T item)
            {
                if (_addJournal.Remove(item))
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
                _items.ForEach(item => {
                    _filterChangedUnsubscriber(item, Item_FilterPropertyChanged);
                    _sortChangedUnsubscriber(item, Item_SortPropertyChanged);
                });

                _addJournal.Clear();
                _removeJournal.Clear();
                _items.Clear();

                InvalidateCache();
            }

            private void ProcessRemoveJournal()
            {
                if (_removeJournal.Count == 0)
                    return;

                // Remove items in reverse.  (Technically there exist faster
                // ways to bulk-remove from a variable-length array, but List<T>
                // does not provide such a method.)
                _removeJournal.Sort((x, y) => y - x); // Sort high to low
                _removeJournal.ForEach(index => { _items.RemoveAt(index); });
                _removeJournal.Clear();
            }

            private void ProcessAddJournal()
            {
                if (_addJournal.Count == 0)
                    return;

                // Prepare the _addJournal to be merge-sorted with _items.
                // _items is already sorted (because it is always sorted).
                _addJournal.Sort(_sort);

                int iAddJournal = 0;
                int iItems = 0;

                while (iItems < _items.Count && iAddJournal < _addJournal.Count)
                {
                    var addJournalItem = _addJournal[iAddJournal];
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
                    var addJournalItem = _addJournal[iAddJournal];
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
                var item = (T)sender;
                var index = _items.IndexOf(item);

                _addJournal.Add(item);
                _removeJournal.Add(index);

                // Until the item is back in place, we don't care about its
                // events.  We will re-subscribe when _addJournal is processed.
                UnsubscribeFromItemEvents(item);
                InvalidateCache();
            }

            #region IComparer<T> implementation
            int IComparer<T>.Compare(T x, T y)
            {
                return _sort(x, y);
            }
            #endregion
        }
    }
}
