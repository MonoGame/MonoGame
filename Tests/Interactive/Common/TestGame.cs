// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Framework.Utilities;
using MonoGame.InteractiveTests.TestUI;
using Color = Microsoft.Xna.Framework.Color;
using GD = MonoGame.InteractiveTests.GameDebug;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Defines a test abstraction for `Game`s that can be run in an harness manually
    /// or automatically.
    /// 
    /// In your `Game::Update/Draw` etc, run your tests and call `UpdateTestResult`
    /// with an appropriate test result.
    /// </summary>
    public class TestGame : Game
    {
        /// <summary>Holds time between frames. </summary>
        private readonly Stopwatch _frameTime = new();

        /// <summary>Holds time between update to end of draw (cost per frame). </summary>
        private readonly Stopwatch _updateToDrawTime = new();

        private int _drawCount;
        private Stopwatch _consoleOutputStopwatch = Stopwatch.StartNew();
        protected Universe _universe;
        protected SpriteFont _font;
        protected Label _helpLabel;
        protected Label _fpsLabel;

        public TestGame()
        {
            var graphics = new GraphicsDeviceManager(this);
            graphics.SupportedOrientations =
                DisplayOrientation.Portrait |
                DisplayOrientation.LandscapeLeft |
                DisplayOrientation.LandscapeRight;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            if (TouchPanel.IsGestureAvailable)
            {
                // Set up gestures for the touch panel (mobile devices) if needed.
                TouchPanel.EnabledGestures = GestureType.DoubleTap | GestureType.Tap;
            }
        }

        /// <summary>
        /// Implementations can call this to update test result and continue orchestration
        /// of other tests.
        /// </summary>
        protected void UpdateTestResult(Universe universe, TestResult result)
        {
            if (result.State is TestState.Success or TestState.Fail) { OnExit(universe); }
        }

        /// <summary>
        /// Implementations can call this to cleanly exit the test (when run in
        /// interactive mode) and go back to the home screen.
        /// </summary>
        protected void OnExit(Universe universe)
        {
            universe.Stop();
            OnExiting(this, new ExitingEventArgs());
        }

        /// <summary>
        /// Setup common items including hit-testing for UI elements in the <see cref="Universe"/>
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            if (TouchPanel.GetState(Window) != null)
            {
                TouchPanel.EnabledGestures = GestureType.DoubleTap | GestureType.Tap;
                TouchPanel.EnableHighFrequencyTouch = true;
            }

            Window.AllowUserResizing = true;
        }

        /// <summary>
        /// Sets up the basic Universe component with Exit button.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            _font = Content.Load<SpriteFont>(@"Fonts\Default");

            InitializeGui();
        }

        /// <summary>
        /// Perform any updates per frame (may be called multiple times per frame to catch up if needed)
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _updateToDrawTime.Start();
        }

        /// <summary>
        /// Renders a single frame for a <see cref="Game"/>. Here we log the general
        /// info as we render frames.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            _updateToDrawTime.Stop();
            if (_drawCount > 0) { _frameTime.Stop(); }

            _frameTime.Start();

            // Output some memory info every 5s.
            if (_frameTime.ElapsedMilliseconds >= 500 && _drawCount > 0)
            {
                double frameTimeMs = ((double)_frameTime.ElapsedMilliseconds) / (double)_drawCount;
                double updateToDrawTimeMs = ((double)_updateToDrawTime.ElapsedMilliseconds) / (double)_drawCount;
                _fpsLabel.Text = $"FPS:  {(1000.0 / frameTimeMs):#.00} fps / {frameTimeMs:#.00} ms \n" +
                                 $"Draw: {(1000.0 / updateToDrawTimeMs):#.00} fps / {updateToDrawTimeMs:#.00} ms";
                _fpsLabel.SizeToFit();
                _frameTime.Reset();
                _updateToDrawTime.Reset();
                _drawCount = 0;
            }

            ++_drawCount;

            if (_consoleOutputStopwatch.ElapsedMilliseconds >= 2500)
            {
                var memInfo = GC.GetGCMemoryInfo();
                // This helps us debug any memory leaks. If this number keeps going up
                // even past GC we have a problem. Use dotMemory or https://superluminal.eu/dotnet/
                // to analyze memory problems. Graphics memory is NOT reported here, use the platform
                // graphics debugger to diagnose those by capturing/debugging frame data.
                GD.LogInfo($" --- Draw : Heap {memInfo.HeapSizeBytes} bytes / " +
                     $"commit {memInfo.TotalCommittedBytes} bytes \n" +
                     $"{_fpsLabel.Text}");
                // GraphicsDebug.PrintTimers();
                _consoleOutputStopwatch = Stopwatch.StartNew();
            }
        }

        /// <summary>
        /// Add any UI stuff in the implemented Test class.
        /// </summary>
        protected virtual void InitializeGui()
        {
            _universe = new Universe(Content)
            {
                AutoHandleInput = true
            };
            Components.Add(new UniverseComponent(this, _universe));

            var exitButton = new Button
            {
                BackgroundColor = Color.Black,
                Content = new Label
                {
                    Font = _font,
                    Text = $"Exit",
                    TextColor = Color.White
                },
                Location = new Point(0, 0)
            };

            exitButton.Content.SizeToFit();
            exitButton.SizeToFit();
            exitButton.Tapped += (sender, e) =>
            {
                _universe.Stop();
                OnExiting(sender, new ExitingEventArgs());
            };

            _universe.Add(exitButton);
            _helpLabel = new Label
            {
                BackgroundColor = Color.Indigo,
                Font = _font,
                Location = new Point(100, 60),
                Text = $"(Running on: {PlatformInfo.MonoGamePlatform} {PlatformInfo.GraphicsBackend})\n",
                TextColor = Color.White
            };
            _universe.Add(_helpLabel);

            _fpsLabel = new Label
            {
                BackgroundColor = Color.Indigo,
                Font = _font,
                Location = new Point(100, 0),
                Text = "",
                TextColor = Color.White
            };
            _universe.Add(_fpsLabel);
        }
    }

    /// <summary>
    /// State returned by the `TestGame.RunTest` that helps the runner coordinate the test run.
    /// </summary>
    public enum TestState
    {
        Success,
        Fail
    };

    /// <summary>
    /// Holds the test result with additional information during `TestGame` runs.
    /// 
    /// `TestGame` implementations can fill in this result via `UpdateTestResult` which
    /// helps the overall controller to orchestrate the tests and jot down test results too.
    /// </summary>
    public record TestResult
    {
        public TestState State;
        public string Message;
    }
}
