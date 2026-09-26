// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Framework.Utilities;
using MonoGame.InteractiveTests.TestUI;

namespace MonoGame.InteractiveTests;

/// <summary>
/// Tests mouse input forwarded to <see cref="TouchPanel"/> when mouse touch points are enabled.
/// </summary>
[InteractiveTest("Mouse Touch Point Test", Categories.General,
    platforms: new[] { MonoGamePlatform.DesktopGL })]
public class MouseTouchPointTestGame : TestGame
{
    private int _numTouchEvents;
    private TouchLocationState _lastTouchState;
    private Vector2 _lastTouchPosition;
    private Label _labelTouchInfo;

    protected override void Initialize()
    {
        base.Initialize();

        TouchPanel.EnableMouseTouchPoint = true;
        TouchPanelState touchPanelState = TouchPanel.GetState(Window);
        if (touchPanelState is not null)
        {
            touchPanelState.OnTouchEvent += OnTouchEvent;
        }
    }

    private void OnTouchEvent(object sender, TouchLocation touchLocation)
    {
        ++_numTouchEvents;
        _lastTouchState = touchLocation.State;
        _lastTouchPosition = touchLocation.Position;
    }

    protected override void InitializeGui()
    {
        base.InitializeGui();

        _labelTouchInfo = new Label
        {
            Frame = new Rectangle(20, 150, 420, 60),
            Font = _font,
            TextColor = Color.White
        };

        _universe.Add(_labelTouchInfo);

        _helpLabel.Text += "Click and drag to validate mouse touch points.";
        _helpLabel.SizeToFit();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Indigo);
        _labelTouchInfo.Text =
            $"Mouse touch events: {_numTouchEvents}\n" +
            $"Last mouse touch: {_lastTouchState} at {_lastTouchPosition}";
        base.Draw(gameTime);
    }
}
