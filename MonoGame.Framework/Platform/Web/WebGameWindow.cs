// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using JSIL;
using Microsoft.Xna.Framework.Input;
using MonoGame.Web;

namespace Microsoft.Xna.Framework
{
    class WebGameWindow : GameWindow
    {
        private WebGamePlatform _platform;
        private List<Keys> _keys;

        public dynamic document, glcanvas, gl, window;

        private Action<dynamic> _onmousemove, _onmousedown, _onmouseup, _onkeydown, _onkeyup, _onwheel;

        public WebGameWindow(WebGamePlatform platform)
        {
            _platform = platform;
            _keys = new List<Keys>();

            _onmousemove = (Action<dynamic>)OnMouseMove;
            _onmousedown = (Action<dynamic>)OnMouseDown;
            _onmouseup = (Action<dynamic>)OnMouseUp;
            _onkeydown = (Action<dynamic>)OnKeyDown;
            _onkeyup = (Action<dynamic>)OnKeyUp;
            _onwheel = (Action<dynamic>)OnMouseWheel;
            
            document = Builtins.Global["document"];
            window = Builtins.Global["window"];
            glcanvas = document.getElementById("mgcanvas");
            gl = glcanvas.getContext("webgl");

            WebGL.gl = gl;

            if (glcanvas.mozRequestPointerLock)
                glcanvas.requestPointerLock = glcanvas.mozRequestPointerLock;
            else if(glcanvas.webkitRequestPointerLock)
                glcanvas.requestPointerLock = glcanvas.webkitRequestPointerLock;

            document.addEventListener("pointerlockchange", (Action)OnCursorLockChange, false);
            document.addEventListener("mozpointerlockchange", (Action)OnCursorLockChange, false);
            document.addEventListener("webkitpointerlockchange", (Action)OnCursorLockChange, false);

            glcanvas.addEventListener("contextmenu", (Action<dynamic>)((e) => e.preventDefault()), false);

            glcanvas.onclick = (Action<dynamic>)OnMouseClick;

            Mouse.PrimaryWindow = this;
        }

        private void OnCursorLockChange()
        {
            if (document.pointerLockElement == glcanvas || document.mozPointerLockElement == glcanvas || document.webkitPointerLockElement == glcanvas)
            {
                document.addEventListener("mousemove", _onmousemove, false);
                document.addEventListener("mousedown", _onmousedown, false);
                document.addEventListener("mouseup", _onmouseup, false);
                glcanvas.addEventListener("wheel", _onwheel, false);
                document.addEventListener("keydown", _onkeydown, false);
                document.addEventListener("keyup", _onkeyup, false);

                Joystick.TrackEvents = true;
            }
            else
            {
                document.removeEventListener("mousemove", _onmousemove, false);
                document.removeEventListener("mousedown", _onmousedown, false);
                document.removeEventListener("mouseup", _onmouseup, false);
                glcanvas.removeEventListener("wheel", _onwheel, false);
                document.removeEventListener("keydown", _onkeydown, false);
                document.removeEventListener("keyup", _onkeyup, false);

                Joystick.TrackEvents = false;
            }
        }

        private void OnMouseClick(dynamic e)
        {
            glcanvas.requestPointerLock();
        }

        private void OnMouseMove(dynamic e)
        {
            var movementX = e.movementX || e.mozMovementX || e.webkitMovementX || 0;
            var movementY = e.movementY || e.mozMovementY || e.webkitMovementY || 0;

            this.MouseState.X = Math.Min(Math.Max(this.MouseState.X + movementX, 0), glcanvas.clientWidth);
            this.MouseState.Y = Math.Min(Math.Max(this.MouseState.Y + movementY, 0), glcanvas.clientHeight);
        }

        private void OnMouseDown(dynamic e)
        {
            if (e.button == 0)
                this.MouseState.LeftButton = ButtonState.Pressed;
            else if (e.button == 1)
                this.MouseState.MiddleButton = ButtonState.Pressed;
            else if (e.button == 2)
                this.MouseState.RightButton = ButtonState.Pressed;
        }

        private void OnMouseUp(dynamic e)
        {
            if (e.button == 0)
                this.MouseState.LeftButton = ButtonState.Released;
            else if (e.button == 1)
                this.MouseState.MiddleButton = ButtonState.Released;
            else if (e.button == 2)
                this.MouseState.RightButton = ButtonState.Released;
        }

        private void OnMouseWheel(dynamic e)
        {
            if (e.deltaY < 0)
                this.MouseState.ScrollWheelValue += 120;
            else
                this.MouseState.ScrollWheelValue -= 120;
        }

        private void OnKeyDown(dynamic e)
        {
            Keys xnaKey = KeyboardUtil.ToXna(e.keyCode, e.location);

            if (!_keys.Contains(xnaKey))
                _keys.Add(xnaKey);
        }

        private void OnKeyUp(dynamic e)
        {
            Keys xnaKey = KeyboardUtil.ToXna(e.keyCode, e.location);

            if (_keys.Contains(xnaKey))
                _keys.Remove(xnaKey);
        }

        internal void ProcessEvents()
        {
            Keyboard.SetKeys(_keys);
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
        }

        protected override void SetTitle(string title)
        {
            Builtins.Eval("window.title = '" + title + "';");
        }

        public override bool AllowUserResizing
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                int width = glcanvas.clientWidth;
                int height = glcanvas.clientHeight;

                return new Rectangle(0, 0, width, height);
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                return DisplayOrientation.Default;
            }
        }

        public override IntPtr Handle
        {
            get
            {
                return IntPtr.Zero;
            }
        }

        public override string ScreenDeviceName
        {
            get
            {
                return string.Empty;
            }
        }
    }
}

