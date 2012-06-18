#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2011 The MonoGame Team

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
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework
{
    public class Game : IDisposable
    {
        private const float DefaultTargetFramesPerSecond = 60.0f;

        private GameComponentCollection _components;
        private GameServiceContainer _services;
        internal GamePlatform Platform;

        private SortingFilteringCollection<IDrawable> _drawables =
            new SortingFilteringCollection<IDrawable>(
                d => d.Visible,
                (d, handler) => d.VisibleChanged += handler,
                (d, handler) => d.VisibleChanged -= handler,
                (d1 ,d2) => Comparer<int>.Default.Compare(d1.DrawOrder, d2.DrawOrder),
                (d, handler) => d.DrawOrderChanged += handler,
                (d, handler) => d.DrawOrderChanged -= handler);

        private SortingFilteringCollection<IUpdateable> _updateables =
            new SortingFilteringCollection<IUpdateable>(
                u => u.Enabled,
                (u, handler) => u.EnabledChanged += handler,
                (u, handler) => u.EnabledChanged -= handler,
                (u1, u2) => Comparer<int>.Default.Compare(u1.UpdateOrder, u2.UpdateOrder),
                (u, handler) => u.UpdateOrderChanged += handler,
                (u, handler) => u.UpdateOrderChanged -= handler);

        private IGraphicsDeviceManager _graphicsDeviceManager;
        private IGraphicsDeviceService _graphicsDeviceService;

        private bool _initialized = false;
        private bool _isFixedTimeStep = true;

        private TimeSpan _targetElapsedTime = TimeSpan.FromSeconds(1 / DefaultTargetFramesPerSecond);

        private int previousDisplayWidth;
        private int previousDisplayHeight;

        public Game()
        {
            _instance = this;
            LaunchParameters = new LaunchParameters();
            _services = new GameServiceContainer();
            _components = new GameComponentCollection();
            Content = new ContentManager(_services);

            Platform = GamePlatform.Create(this);
            Platform.Activated += Platform_Activated;
            Platform.Deactivated += Platform_Deactivated;
            _services.AddService(typeof(GamePlatform), Platform);
        }

        ~Game()
        {
            Dispose(false);
        }

		[System.Diagnostics.Conditional("DEBUG")]
		internal void Log(string Message)
		{
			if (Platform != null) Platform.Log(Message);
		}

        #region IDisposable Implementation

        private bool _isDisposed;
        public void Dispose()
        {
            Dispose(true);
            Raise(Disposed, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Platform.Dispose();
            }
            _isDisposed = true;
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void AssertNotDisposed()
        {
            if (_isDisposed)
            {
                string name = GetType().Name;
                throw new ObjectDisposedException(
                    name, string.Format("The {0} object was used after being Disposed.", name));
            }
        }

        #endregion IDisposable Implementation

        #region Properties

#if ANDROID
        public static AndroidGameActivity Activity { get; set; }
#endif
        private static Game _instance = null;
        internal static Game Instance { get { return Game._instance; } }

        public LaunchParameters LaunchParameters { get; private set; }

        public GameComponentCollection Components
        {
            get { return _components; }
        }

        public TimeSpan InactiveSleepTime
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool IsActive
        {
            get { return Platform.IsActive; }
        }

        public bool IsMouseVisible
        {
            get { return Platform.IsMouseVisible; }
            set { Platform.IsMouseVisible = value; }
        }

        public TimeSpan TargetElapsedTime
        {
            get { return _targetElapsedTime; }
            set
            {
                // Give GamePlatform implementations an opportunity to override
                // the new value.
                value = Platform.TargetElapsedTimeChanging(value);

                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(
                        "value must be positive and non-zero.");

                if (value != _targetElapsedTime)
                {
                    _targetElapsedTime = value;
                    Platform.TargetElapsedTimeChanged();
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

        public ContentManager Content { get; set; }

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

#if ANDROID
        public AndroidGameWindow Window
        {
            get { return Platform.Window; }
        }
#else
        public GameWindow Window
        {
            get { return Platform.Window; }
        }
#endif

        #endregion Properties

        #region Internal Properties

        // FIXME: Internal members should be eliminated.
        // Currently Game.Initialized is used by the Mac game window class to
        // determine whether to raise DeviceResetting and DeviceReset on
        // GraphicsDeviceManager.
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
            Platform.Exit();
        }

        public void ResetElapsedTime()
        {
            Platform.ResetElapsedTime();
            _gameTime.ResetElapsedTime();
        }

        public void Run()
        {
            Run(Platform.DefaultRunBehavior);
        }

        public void Run(GameRunBehavior runBehavior)
        {
            AssertNotDisposed();
            if (!Platform.BeforeRun())
                return;

            // In an original XNA game the GraphicsDevice property is null
            // during initialization but before the Game's Initialize method is
            // called the property is available so we can only assume that it
            // should be created somewhere in here.  We cannot set the viewport
            // values correctly based on the Preferred settings which is causing
            // some problems on some Microsoft samples which we are not handling
            // correctly.
            graphicsDeviceManager.CreateDevice();
            applyChanges(graphicsDeviceManager);

            Platform.BeforeInitialize();
            Initialize();
            _initialized = true;

            BeginRun();
            switch (runBehavior)
            {
            case GameRunBehavior.Asynchronous:
                Platform.AsyncRunLoopEnded += Platform_AsyncRunLoopEnded;
                Platform.StartRunLoop();
                break;
            case GameRunBehavior.Synchronous:
                Platform.RunLoop();
                EndRun();
                OnExiting(this, EventArgs.Empty);
                break;
            default:
                throw new NotImplementedException(string.Format(
                    "Handling for the run behavior {0} is not implemented.", runBehavior));
            }
        }

        private DateTime _now;
        private DateTime _lastUpdate = DateTime.UtcNow;
        private readonly GameTime _gameTime = new GameTime();
        private readonly GameTime _fixedTimeStepTime = new GameTime();
        private TimeSpan _totalTime = TimeSpan.Zero;

        public void Tick()
        {
            bool doDraw = false;

            _now = DateTime.UtcNow;

            _gameTime.Update(_now - _lastUpdate);
            _lastUpdate = _now;

            if (IsFixedTimeStep)
            {
                _totalTime += _gameTime.ElapsedGameTime;
                int max = (500/TargetElapsedTime.Milliseconds);    //Only do updates for half a second worth of updates
                int iterations = 0;

                max = max <= 0 ? 1 : max;   //Make sure at least 1 update is called

                while (_totalTime >= TargetElapsedTime)
                {
                    _fixedTimeStepTime.Update(TargetElapsedTime);
                    _totalTime -= TargetElapsedTime;
                    DoUpdate(_fixedTimeStepTime);
                    doDraw = true;
                        
                    iterations++;
                    if (iterations >= max)  //Reset catchup if to many updates have been called
                    {
                        _totalTime = TimeSpan.Zero;
                    }
                }
            }
            else
            {
                DoUpdate(_gameTime);
                doDraw = true;
            }

            if (doDraw)
            {
                DoDraw(_gameTime);
                GraphicsDevice.Present();
            }

            if (IsFixedTimeStep)
            {
                var currentTime = (DateTime.UtcNow - _lastUpdate) + _totalTime;

                if (currentTime < TargetElapsedTime)
                {
                    System.Threading.Thread.Sleep((TargetElapsedTime - currentTime).Milliseconds);
                }
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
            // According to the information given on MSDN (see link below), all
            // GameComponents in Components at the time Initialize() is called
            // are initialized.
            // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.game.initialize.aspx

            // 1. Categorize components into IUpdateable and IDrawable lists.
            // 2. Subscribe to Added/Removed events to keep the categorized
            //    lists synced and to Initialize future components as they are
            //    added.
            // 3. Initialize all existing components
            CategorizeComponents();
            _components.ComponentAdded += Components_ComponentAdded;
            _components.ComponentRemoved += Components_ComponentRemoved;
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

        private static readonly Action<IDrawable, GameTime> DrawAction =
            (drawable, gameTime) => drawable.Draw(gameTime);

        protected virtual void Draw(GameTime gameTime)
        {
#if ANDROID
            // TODO: It should be possible to move this call to
            //       PrimaryThreadLoader.DoLoads into
            //       AndroidGamePlatform.BeforeDraw and remove the need for the
            //       #if ANDROID check.
            PrimaryThreadLoader.DoLoads();
#endif
            if (GraphicsDevice.DisplayMode.Width != previousDisplayWidth ||
                GraphicsDevice.DisplayMode.Height != previousDisplayHeight)
            {
                previousDisplayHeight = GraphicsDevice.DisplayMode.Height;
                previousDisplayWidth = GraphicsDevice.DisplayMode.Width;
                graphicsDeviceManager.ResetClientBounds();
            }
            
            _drawables.ForEachFilteredItem(DrawAction, gameTime);
        }

        private static readonly Action<IUpdateable, GameTime> UpdateAction =
            (updateable, gameTime) => updateable.Update(gameTime);

        protected virtual void Update(GameTime gameTime)
        {
            _updateables.ForEachFilteredItem(UpdateAction, gameTime);
        }

        protected virtual void OnExiting(object sender, EventArgs args)
        {
            Raise(Exiting, args);
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
            AssertNotDisposed();

            var platform = (GamePlatform)sender;
            platform.AsyncRunLoopEnded -= Platform_AsyncRunLoopEnded;
            EndRun();
            OnExiting(this, EventArgs.Empty);
        }

        private void Platform_Activated(object sender, EventArgs e)
        {
            AssertNotDisposed();
            Raise(Activated, e);
        }

        private void Platform_Deactivated(object sender, EventArgs e)
        {
            AssertNotDisposed();
            Raise(Deactivated, e);
        }

        #endregion Event Handlers

        #region Internal Methods

        // FIXME: We should work toward eliminating internal methods.  They
        //        break entirely the possibility that additional platforms could
        //        be added by third parties without changing MonoGame itself.

        internal void applyChanges(GraphicsDeviceManager manager)
        {
			Platform.BeginScreenDeviceChange(GraphicsDevice.PresentationParameters.IsFullScreen);
            if (GraphicsDevice.PresentationParameters.IsFullScreen)
                Platform.EnterFullScreen();
            else
                Platform.ExitFullScreen();

            // FIXME: Is this the correct/best way to set the viewport?  There
            //        are/were several snippets like this through the project.
            var viewport = new Viewport();

            viewport.X = 0;
            viewport.Y = 0;
#if WINDOWS || LINUX
            viewport.Width = manager.PreferredBackBufferWidth;// GraphicsDevice.PresentationParameters.BackBufferWidth;
            viewport.Height = manager.PreferredBackBufferHeight;// GraphicsDevice.PresentationParameters.BackBufferHeight;
#else
            viewport.Width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            viewport.Height = GraphicsDevice.PresentationParameters.BackBufferHeight;
#endif

            GraphicsDevice.Viewport = viewport;
			Platform.EndScreenDeviceChange(string.Empty, viewport.Width, viewport.Height);
        }

        internal void DoUpdate(GameTime gameTime)
        {
            AssertNotDisposed();
            if (Platform.BeforeUpdate(gameTime))
                Update(gameTime);
        }

        internal void DoDraw(GameTime gameTime)
        {
            AssertNotDisposed();
            // Draw and EndDraw should not be called if BeginDraw returns false.
            // http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
            // http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
            if (Platform.BeforeDraw(gameTime) && BeginDraw())
            {
                Draw(gameTime);
                EndDraw();
            }
        }

        internal void DoInitialize()
        {
            AssertNotDisposed();
            Platform.BeforeInitialize();
            Initialize();
        }

#if LINUX
        internal void ResizeWindow(bool changed)
        {
            ((LinuxGamePlatform)Platform).ResetWindowBounds(changed);
        }
#endif

        #endregion Internal Methods

        private GraphicsDeviceManager graphicsDeviceManager
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
            // TODO: Would be nice to get rid of this copy, but since it only
            //       happens once per game, it's fairly low priority.
            var copy = new IGameComponent[Components.Count];
            Components.CopyTo(copy, 0);
            foreach (var component in copy)
                component.Initialize();
        }

        private void CategorizeComponents()
        {
            DecategorizeComponents();
            for (int i = 0; i < Components.Count; ++i)
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

        /// <summary>
        /// The SortingFilteringCollection class provides efficient, reusable
        /// sorting and filtering based on a configurable sort comparer, filter
        /// predicate, and associate change events.
        /// </summary>
        class SortingFilteringCollection<T> : ICollection<T>
        {
            private readonly List<T> _items;
            private readonly List<AddJournalEntry> _addJournal;
            private readonly Comparison<AddJournalEntry> _addJournalSortComparison;
            private readonly List<int> _removeJournal;
            private readonly List<T> _cachedFilteredItems;
            private bool _shouldRebuildCache;

            private readonly Predicate<T> _filter;
            private readonly Comparison<T> _sort;
            private readonly Action<T, EventHandler<EventArgs>> _filterChangedSubscriber;
            private readonly Action<T, EventHandler<EventArgs>> _filterChangedUnsubscriber;
            private readonly Action<T, EventHandler<EventArgs>> _sortChangedSubscriber;
            private readonly Action<T, EventHandler<EventArgs>> _sortChangedUnsubscriber;

            public SortingFilteringCollection(
                Predicate<T> filter,
                Action<T, EventHandler<EventArgs>> filterChangedSubscriber,
                Action<T, EventHandler<EventArgs>> filterChangedUnsubscriber,
                Comparison<T> sort,
                Action<T, EventHandler<EventArgs>> sortChangedSubscriber,
                Action<T, EventHandler<EventArgs>> sortChangedUnsubscriber)
            {
                _items = new List<T>();
                _addJournal = new List<AddJournalEntry>();
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

            private int CompareAddJournalEntry(AddJournalEntry x, AddJournalEntry y)
            {
                int result = _sort((T)x.Item, (T)y.Item);
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
                    for (int i = 0; i < _items.Count; ++i)
                        if (_filter(_items[i]))
                            _cachedFilteredItems.Add(_items[i]);

                    _shouldRebuildCache = false;
                }

                for (int i = 0; i < _cachedFilteredItems.Count; ++i)
                    action(_cachedFilteredItems[i], userData);

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
                _addJournal.Add(new AddJournalEntry(_addJournal.Count, item));
                InvalidateCache();
            }

            public bool Remove(T item)
            {
                if (_addJournal.Remove(AddJournalEntry.CreateKey(item)))
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
                for (int i = 0; i < _items.Count; ++i)
                {
                    _filterChangedUnsubscriber(_items[i], Item_FilterPropertyChanged);
                    _sortChangedUnsubscriber(_items[i], Item_SortPropertyChanged);
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

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return ((System.Collections.IEnumerable)_items).GetEnumerator();
            }

            private static readonly Comparison<int> RemoveJournalSortComparison =
                (x, y) => Comparer<int>.Default.Compare(y, x); // Sort high to low
            private void ProcessRemoveJournal()
            {
                if (_removeJournal.Count == 0)
                    return;

                // Remove items in reverse.  (Technically there exist faster
                // ways to bulk-remove from a variable-length array, but List<T>
                // does not provide such a method.)
                _removeJournal.Sort(RemoveJournalSortComparison);
                for (int i = 0; i < _removeJournal.Count; ++i)
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

                int iAddJournal = 0;
                int iItems = 0;

                while (iItems < _items.Count && iAddJournal < _addJournal.Count)
                {
                    var addJournalItem = (T)_addJournal[iAddJournal].Item;
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
                    var addJournalItem = (T)_addJournal[iAddJournal].Item;
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

                _addJournal.Add(new AddJournalEntry(_addJournal.Count, item));
                _removeJournal.Add(index);

                // Until the item is back in place, we don't care about its
                // events.  We will re-subscribe when _addJournal is processed.
                UnsubscribeFromItemEvents(item);
                InvalidateCache();
            }
        }

        // For iOS, the AOT compiler can't seem to handle a
        // List<AddJournalEntry<T>>, so unfortunately we'll use object
        // for storage.
        private struct AddJournalEntry
        {
            public readonly int Order;
            public readonly object Item;

            public AddJournalEntry(int order, object item)
            {
                Order = order;
                Item = item;
            }

            public static AddJournalEntry CreateKey(object item)
            {
                return new AddJournalEntry(-1, item);
            }

            public override int GetHashCode()
            {
                return Item.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is AddJournalEntry))
                    return false;

                return object.Equals(Item, ((AddJournalEntry)obj).Item);
            }
        }
    }

    public enum GameRunBehavior
    {
        Asynchronous,
        Synchronous
    }
}
