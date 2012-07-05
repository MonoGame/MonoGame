using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
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

        static TimeSpan MaximumUpdateInterval = TimeSpan.FromMilliseconds(500);

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

            // This looks to be a safe way to update the game timers
            // as the XAML/DirectX 3D Shoooing Game sample also updates
            // the gameplay state during the render callback.

            CompositionTarget.Rendering += (o, a) => OnTick();
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
            // Updates have to happen as they
            // control if we draw or not.
            if (!_updateTimers.Contains(this))
                _updateTimers.Add(this);

            // We only add the timers to the frame and draw
            // lists if they actually have events to process.

            if (FrameAction == null)
                _frameTimers.Remove(this);
            else if (!_frameTimers.Contains(this))
                _frameTimers.Add(this);

            if (Draw == null)
                _drawTimers.Remove(this);
            else if (!_drawTimers.Contains(this))
                _drawTimers.Add(this);

            // The lists need to be resorted now.
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
                timer._gameTime.ElapsedTime = TimeSpan.Zero;
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
        private readonly GameTimerEventArgs _gameTime = new GameTimerEventArgs();

        private static Stopwatch _gameTimer = Stopwatch.StartNew();

        private static MetroFrameworkElementEvents _windowEvents;

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

            // Do we need to initialize the window event handlers?
            if (_windowEvents == null && Window.Current != null)
                _windowEvents = new MetroFrameworkElementEvents(SharedGraphicsDeviceManager.Current.SwapChainPanel);
            if (_windowEvents != null)
                _windowEvents.UpdateState();

            // First call all the frame events... we do this
            // every frame regardless of the elapsed time.
            foreach (var timer in _frameTimers)
                timer.FrameAction(timer, EventArgs.Empty);

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
                // We gotta reapply the render targets on update
                // for some reason...  i guess the OS changes them?
                deviceManager.GraphicsDevice.ResetRenderTargets();

                var doPresent = false;

                foreach (var timer in _drawTimers)
                {
                    if (!timer._doDraw)
                        continue;

                    timer.Draw(timer, timer._gameTime);

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
            // Advance the accumulated elapsed time.
            _accumulatedElapsedTime += elapsed;

            // If not enough time has elapsed to perform an update we will 
            // try again on the next update call.
            if (    _accumulatedElapsedTime == TimeSpan.Zero || 
                    _accumulatedElapsedTime < UpdateInterval )
                return;

            // Do not allow any update to take longer than our maximum.
            if (_accumulatedElapsedTime > MaximumUpdateInterval)
                _accumulatedElapsedTime = MaximumUpdateInterval;

            // TODO: We should be calculating IsRunningSlowly
            // somewhere around here!

            // If we have an interval then we're working in fixed timestep mode.
            if (UpdateInterval > TimeSpan.Zero)
            {
                _gameTime.ElapsedTime = UpdateInterval;
                var stepCount = 0;

                if (FrameAction != null)
                    FrameAction(this, null);

                // Perform as many full fixed length time steps as we can.
                while (_accumulatedElapsedTime >= UpdateInterval)
                {
                    _gameTime.TotalTime += UpdateInterval;
                    _accumulatedElapsedTime -= UpdateInterval;
                    ++stepCount;

                    if (Update != null)
                        Update(this, _gameTime);
                }

                // Draw needs to know the total elapsed time
                // that occured for the fixed length updates.
                _gameTime.ElapsedTime = TimeSpan.FromTicks(UpdateInterval.Ticks * stepCount);
            }
            else
            {
                // Perform a single variable length update.
                _gameTime.ElapsedTime = _accumulatedElapsedTime;
                _gameTime.TotalTime += _accumulatedElapsedTime;
                _accumulatedElapsedTime = TimeSpan.Zero;

                if (Update != null)
                    Update(this, _gameTime);
            }

            // Draw unless the update suppressed it.
            if (_suppressDraw)
                _suppressDraw = false;
            else
                _doDraw = true;
        }

        public void Dispose()
        {
            // Remove us from all timers.
            _frameTimers.Remove(this);
            _updateTimers.Remove(this);
            _drawTimers.Remove(this);
            _allTimers.Remove(this);
        }
    }
}
