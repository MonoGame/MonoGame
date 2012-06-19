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
        static List<GameTimer> _frameTimers = new List<GameTimer>();
        static List<GameTimer> _updateTimers = new List<GameTimer>();
        static List<GameTimer> _drawTimers = new List<GameTimer>();

        
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
            DrawOrder = DefaultOrder;
            FrameActionOrder = DefaultOrder;
            UpdateOrder = DefaultOrder;

            UpdateInterval = DefaultUpdateInterval;
        }

        public void Start()
        {
            if (!_updateTimers.Contains(this))
            {
                _frameTimers.Add(this);
                _updateTimers.Add(this);
                _drawTimers.Add(this);
            }
        }

        public void Stop()
        {
            _frameTimers.Remove(this);
            _updateTimers.Remove(this);
            _drawTimers.Remove(this);
        }

        public static void ResetElapsedTime()
        {
        }

        public static void SuppressFrame()
        {
        }

        private bool _doDraw;

        private TimeSpan _accumulatedElapsedTime;
        private readonly GameTime _gameTime = new GameTime();
        private Stopwatch _gameTimer = Stopwatch.StartNew();

        static private void OnTick()
        {
            // First call all the frame events... we do this
            // every frame regardless of the elapsed time.
            foreach (var timer in _frameTimers)
            {
                if (timer.FrameAction != null)
                    timer.FrameAction(timer, EventArgs.Empty);
            }

            // Next do update events.
            foreach (var timer in _updateTimers)
                timer.DoUpdate();

            // Timers that have been updated can now draw.
            if (    SharedGraphicsDeviceManager.Current != null &&
                    SharedGraphicsDeviceManager.Current.GraphicsDevice != null)
            {
                var present = false;

                foreach (var timer in _drawTimers)
                {
                    if (!timer._doDraw)
                        continue;

                    if (timer.Draw != null)
                        timer.Draw(timer, new GameTimerEventArgs(timer._gameTime.TotalGameTime, timer._gameTime.ElapsedGameTime));
                
                    present = true;
                    timer._doDraw = false;
                }

                // Present the frame if we draw anything.
                if (present)
                    SharedGraphicsDeviceManager.Current.GraphicsDevice.Present();
            }
        }

        private void DoUpdate()
        {
            // NOTE: This code is very sensitive and can break very badly
            // with even what looks like a safe change.  Be sure to test 
            // any change fully in both the fixed and variable timestep 
            // modes across multiple devices and platforms.

            // Advance the accumulated elapsed time.
            _accumulatedElapsedTime += _gameTimer.Elapsed;
            _gameTimer.Reset();
            _gameTimer.Start();

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
            //if (_suppressDraw)
                //_suppressDraw = false;
            //else
                _doDraw = true;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
