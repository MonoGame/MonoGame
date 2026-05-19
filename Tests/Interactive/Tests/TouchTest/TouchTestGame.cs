// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.InteractiveTests.TestUI;
using MonoGame.Framework.Utilities;
using Color = Microsoft.Xna.Framework.Color;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Tests the <see cref="TouchPanel.AddHighResolutionTouchEvent"/> API.
    /// Run the test and touch the iOS or Android device's display.
    /// You should see a running live count of high-frequency touch events.
    ///
    /// Note: To test high-frequency events are NOT available on desktop GL, this
    /// test also runs on desktop GL.
    /// </summary>
    [InteractiveTest("Touch Test", Categories.General,
        platforms: new[] { MonoGamePlatform.iOS, MonoGamePlatform.Android, MonoGamePlatform.DesktopGL })]
    public class TouchTestGame : TestGame
    {
        private int _numTouchEvents;
        private int _numHighFreqTouchEvents;

        private Label _labelTouchInfo;

        protected override void Initialize()
        {
            base.Initialize();
            if (TouchPanel.GetState(Window) != null) { TouchPanel.GetState(Window).OnTouchEvent += OnTouchEvent; }
        }

        private void OnTouchEvent(object sender, TouchLocation e)
        {
            ++_numTouchEvents;
            if (e.IsHighFrequencyEvent()) { ++_numHighFreqTouchEvents; }
        }

        protected override void InitializeGui()
        {
            base.InitializeGui();
            _labelTouchInfo = new Label
            {
                Frame = new Rectangle(20, 150, 320, 20),
                Font = _font,
                TextColor = Color.White
            };

            _universe.Add(_labelTouchInfo);

            _helpLabel.Text += "Showcases high-frequency touch events on iOS/Android.";
            _helpLabel.SizeToFit();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Indigo);
            _labelTouchInfo.Text =
                $"Num. touch events: {_numTouchEvents} vs " +
                $"high-frequency events {_numHighFreqTouchEvents}";
            base.Draw(gameTime);
        }
    }
}
