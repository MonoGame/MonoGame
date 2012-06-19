using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Microsoft.Xna.Framework
{
    public sealed class GameTimer : IDisposable
    {
        static List<GameTimer> _allTimers = new List<GameTimer>();

        static List<GameTimer> _frameTimers = new List<GameTimer>();
        static List<GameTimer> _updateTimers = new List<GameTimer>();
        static List<GameTimer> _drawTimers = new List<GameTimer>();

        static bool _resort;
        
        static int DefaultOrder = 0;

        static TimeSpan DefaultUpdateInterval = TimeSpan.FromTicks(333333);

        static TimeSpan MaximumUpdateInterval = TimeSpan.FromDays(30);

        static TimeSpan VariableStepIncrement = TimeSpan.FromTicks(1);

        int DrawOrder { get; set; }

        public int FrameActionOrder { get; set; }

        public TimeSpan UpdateInterval { get; set; }

        public int UpdateOrder { get; set; }

        public event EventHandler<EventArgs> FrameAction;

        public event EventHandler<GameTimerEventArgs> Draw;

        public event EventHandler<GameTimerEventArgs> Update;

        static GameTimer()
        {
#if WINRT
            CompositionTarget.Rendering += (o, a) =>
            {
                OnTick();
            };
#endif
        }

        public GameTimer()
        {
            _allTimers.Add(this);

            DrawOrder = DefaultOrder;
            FrameActionOrder = DefaultOrder;
            UpdateOrder = DefaultOrder;

            UpdateInterval = DefaultUpdateInterval;
        }

        public void Start()
        {
            // TODO: Throw an exception on start without stop?

            // Updates have to happen as they
            // control if we draw or not.
            if (!_updateTimers.Contains(this))
                _updateTimers.Add(this);

            // We only add the timers to the frame and draw
            // lists if they actually have events to process.

            if (FrameAction == null)
                _frameTimers.Remove(this);
            else
                _frameTimers.Add(this);

            if (Draw == null)
                _drawTimers.Remove(this);
            else
                _drawTimers.Add(this);

            _resort = true;
        }

        public void Stop()
        {
            // Remove the timer from all the lists.
            _frameTimers.Remove(this);
            _updateTimers.Remove(this);
            _drawTimers.Remove(this);
        }

        public static void ResetElapsedTime()
        {
            _gameTimer.Reset();
            _gameTimer.Start();

            foreach (var timer in _allTimers)
            {
                timer._accumulatedElapsedTime = TimeSpan.Zero;
                timer._gameTime.ElapsedGameTime = TimeSpan.Zero;
            }
        }

        public static void SuppressFrame()
        {
            foreach (var timer in _drawTimers)
                timer._suppressDraw = true;
        }

        private bool _suppressDraw;

        private bool _doDraw;
        private TimeSpan _accumulatedElapsedTime;
        private readonly GameTime _gameTime = new GameTime();

        private static Stopwatch _gameTimer = Stopwatch.StartNew();

        static private void OnTick()
        {
            // Resort the timer update lists if they have been changed.
            if (_resort)
            {
                _frameTimers.Sort((f, s) => f.FrameActionOrder - s.FrameActionOrder);
                _drawTimers.Sort((f, s) => f.DrawOrder - s.DrawOrder);
                _updateTimers.Sort((f, s) => f.UpdateOrder - s.UpdateOrder);
                _resort = false;
            }

            // First call all the frame events... we do this
            // every frame regardless of the elapsed time.
            foreach (var timer in _frameTimers)
            {
                if (timer.FrameAction != null)
                    timer.FrameAction(timer, EventArgs.Empty);
            }

            // Next do update events.
            var elapsed = _gameTimer.Elapsed;
            _gameTimer.Reset();
            _gameTimer.Start();
            foreach (var timer in _updateTimers)
                timer.DoUpdate(elapsed);

            // Timers that have been updated can now draw.
            var deviceManager = SharedGraphicsDeviceManager.Current;
            if (deviceManager != null && deviceManager.GraphicsDevice != null)
            {
                var doPresent = false;

                foreach (var timer in _drawTimers)
                {
                    if (!timer._doDraw)
                        continue;

                    if (timer.Draw != null)
                        timer.Draw(timer, new GameTimerEventArgs(timer._gameTime.TotalGameTime, timer._gameTime.ElapsedGameTime));

                    doPresent = true;
                    timer._doDraw = false;
                }

                // If nothing was drawn then we don't present and
                // the swap chain will contain whatever was last rendered.
                if (doPresent)
                    deviceManager.GraphicsDevice.Present();
            }
        }

        private void DoUpdate(TimeSpan elapsed)
        {
            // NOTE: This code is very sensitive and can break very badly
            // with even what looks like a safe change.  Be sure to test 
            // any change fully in both the fixed and variable timestep 
            // modes across multiple devices and platforms.

            // Advance the accumulated elapsed time.
            _accumulatedElapsedTime += elapsed;

            // If we're in the fixed timestep mode and not enough time has elapsed
            // to perform an update we sleep off the the remaining time to save battery
            // life and/or release CPU time to other threads and processes.
            if (_accumulatedElapsedTime < UpdateInterval)
                return;

            // Do not allow any update to take longer than our maximum.
            if (_accumulatedElapsedTime > MaximumUpdateInterval)
                _accumulatedElapsedTime = MaximumUpdateInterval;

            // TODO: We should be calculating IsRunningSlowly
            // somewhere around here!

            //if (IsFixedTimeStep)
            {
                _gameTime.ElapsedGameTime = UpdateInterval;
                var stepCount = 0;

                if (FrameAction != null)
                    FrameAction(this, null);

                // Perform as many full fixed length time steps as we can.
                while (_accumulatedElapsedTime >= UpdateInterval)
                {
                    _gameTime.TotalGameTime += UpdateInterval;
                    _accumulatedElapsedTime -= UpdateInterval;
                    ++stepCount;

                    if (Update != null)
                        Update(this, new GameTimerEventArgs(_gameTime.TotalGameTime, _gameTime.ElapsedGameTime));
                }

                // Draw needs to know the total elapsed time
                // that occured for the fixed length updates.
                _gameTime.ElapsedGameTime = TimeSpan.FromTicks(UpdateInterval.Ticks * stepCount);
            }
            /*
            else
            {
                // Perform a single variable length update.
                _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
                _gameTime.TotalGameTime += _accumulatedElapsedTime;
                _accumulatedElapsedTime = TimeSpan.Zero;

                DoUpdate(_gameTime);
            }
            */

            // Draw unless the update suppressed it.
            if (_suppressDraw)
                _suppressDraw = false;
            else
                _doDraw = true;
        }

        public void Dispose()
        {
            Stop();
            _allTimers.Remove(this);
        }
    }
}
