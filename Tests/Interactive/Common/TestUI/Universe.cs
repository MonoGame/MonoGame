// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoGame.InteractiveTests.TestUI
{
    /// <summary>
    /// Manages and holds on to UI elements such as <see cref="Button"/>s, <see cref="Label"/> etc.
    /// </summary>
    public class Universe
    {
        private bool _isActive = true;
        private readonly ViewCollection _views;

        public Universe(ContentManager content)
        {
            if (content == null)
                throw new ArgumentNullException("content");
            _content = content;
            _views = new ViewCollection(null);
            _views.Universe = this;
        }

        private readonly ContentManager _content;
        private bool _wasPressed;
        public ContentManager Content { get { return _content; } }

        public bool AutoHandleInput { get; set; }

        public void Add(View view)
        {
            _views.Add(view);
        }

        public bool HandleGestureSample(Point position, GameTime gameTime)
        {
            if (!_isActive) return false;
            bool handled = false;
            foreach (var view in _views.HitTest(position))
            {
                if (view.HandleGestureSample(position, gameTime))
                {
                    handled = true;
                    break;
                }
            }

            return handled;
        }

        public void Update(GameTime gameTime)
        {
            if (!_isActive) return;
            if (AutoHandleInput)
            {
#if IOS || ANDROID
                if (TouchPanel.GetCapabilities().IsConnected)
                {
                    while (_isActive && TouchPanel.IsGestureAvailable)
                    {
                        var gestureSample = TouchPanel.ReadGesture();
                        HandleGestureSample(new((int)gestureSample.Position.X, (int)gestureSample.Position.Y),
                            gameTime);
                    }
                }
#else
                    var state = Mouse.GetState();
                    var position = state.Position;
                    if (state.LeftButton == ButtonState.Released && _wasPressed)
                    {
                        HandleGestureSample(new(position.X, position.Y), gameTime);
                    }
                    _wasPressed = state.LeftButton == ButtonState.Pressed;
#endif
            }
        }

        public void Draw(DrawContext context, GameTime gameTime)
        {
            if (!_isActive) return;
            _views.Draw(context, gameTime);
        }

        public void Stop()
        {
            _isActive = false;
            AutoHandleInput = false;
        }
    }
}
