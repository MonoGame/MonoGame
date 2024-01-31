// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        internal static Dictionary<int, IntPtr> Joysticks = new Dictionary<int, IntPtr>();
        private static int _lastConnectedIndex = -1;

        internal static void AddDevices()
        {
            int numJoysticks = Sdl.Joystick.NumJoysticks();
            for (int i = 0; i < numJoysticks; i++)
                AddDevice(i);
        }

        internal static void AddDevice(int deviceId)
        {
            var jdevice = Sdl.Joystick.Open(deviceId);
            if (Joysticks.ContainsValue(jdevice)) return;

            var id = 0;

            while (Joysticks.ContainsKey(id))
                id++;

            if (id > _lastConnectedIndex)
                _lastConnectedIndex = id;

            Joysticks.Add(id, jdevice);

            if (Sdl.GameController.IsGameController(deviceId) == 1)
                GamePad.AddDevice(deviceId);
        }

        internal static void RemoveDevice(int instanceid)
        {
            foreach (KeyValuePair<int, IntPtr> entry in Joysticks)
            {
                if (Sdl.Joystick.InstanceID(entry.Value) == instanceid)
                {
                    int key = entry.Key;

                    Sdl.Joystick.Close(Joysticks[entry.Key]);
                    Joysticks.Remove(entry.Key);

                    if (key == _lastConnectedIndex)
                        RecalculateLastConnectedIndex();

                    break;
                }
            }
        }

        internal static void CloseDevices()
        {
            GamePad.CloseDevices();

            foreach (var entry in Joysticks)
                Sdl.Joystick.Close(entry.Value);

            Joysticks.Clear ();
        }

        private static void RecalculateLastConnectedIndex()
        {
            _lastConnectedIndex = -1;
            foreach (var entry in Joysticks)
            {
                if (entry.Key > _lastConnectedIndex)
                    _lastConnectedIndex = entry.Key;
            }
        }

        private static int PlatformLastConnectedIndex
        {
            get { return _lastConnectedIndex; }
        }

        private const bool PlatformIsSupported = true;

        private static JoystickCapabilities PlatformGetCapabilities(int index)
        {
            IntPtr joystickPtr = IntPtr.Zero;
            if (!Joysticks.TryGetValue(index, out joystickPtr))
                return new JoystickCapabilities
                {
                    IsConnected = false,
                    DisplayName = string.Empty,
                    Identifier = "",
                    IsGamepad = false,
                    AxisCount = 0,
                    ButtonCount = 0,
                    HatCount = 0
                };

            var jdevice = joystickPtr;
            return new JoystickCapabilities
            {
                IsConnected = true,
                DisplayName = Sdl.Joystick.GetJoystickName(jdevice),
                Identifier = Sdl.Joystick.GetGUID(jdevice).ToString(),
                IsGamepad = (Sdl.GameController.IsGameController(index) == 1),
                AxisCount = Sdl.Joystick.NumAxes(jdevice),
                ButtonCount = Sdl.Joystick.NumButtons(jdevice), 
                HatCount = Sdl.Joystick.NumHats(jdevice)
            };
        }

        private static JoystickState PlatformGetState(int index)
        {
            IntPtr joystickPtr = IntPtr.Zero;
            if (!Joysticks.TryGetValue(index, out joystickPtr))
                return _defaultJoystickState;

            var jcap = PlatformGetCapabilities(index);
            var jdevice = joystickPtr;

            var axes = new int[jcap.AxisCount];
            for (var i = 0; i < axes.Length; i++)
                axes[i] = Sdl.Joystick.GetAxis(jdevice, i);

            var buttons = new ButtonState[jcap.ButtonCount];
            for (var i = 0; i < buttons.Length; i++)
                buttons[i] = (Sdl.Joystick.GetButton(jdevice, i) == 0) ? ButtonState.Released : ButtonState.Pressed;

            var hats = new JoystickHat[jcap.HatCount];
            for (var i = 0; i < hats.Length; i++)
            {
                var hatstate = Sdl.Joystick.GetHat(jdevice, i);

                hats[i] = new JoystickHat
                {
                    Up = ((hatstate & Sdl.Joystick.Hat.Up) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Down = ((hatstate & Sdl.Joystick.Hat.Down) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Left = ((hatstate & Sdl.Joystick.Hat.Left) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Right = ((hatstate & Sdl.Joystick.Hat.Right) != 0) ? ButtonState.Pressed : ButtonState.Released
                };
            }

            return new JoystickState
            {
                IsConnected = true,
                Axes = axes,
                Buttons = buttons,
                Hats = hats
            };
        }

        private static void PlatformGetState(ref JoystickState joystickState, int index)
        {
            IntPtr joystickPtr = IntPtr.Zero;
            if (!Joysticks.TryGetValue(index, out joystickPtr))
            {
                joystickState.IsConnected = false;
                return;
            }

            var jcap = PlatformGetCapabilities(index);
            var jdevice = joystickPtr;

            //Resize each array if the length is less than the count returned by the capabilities
            if (joystickState.Axes.Length < jcap.AxisCount)
            {
                joystickState.Axes = new int[jcap.AxisCount];
            }

            if (joystickState.Buttons.Length < jcap.ButtonCount)
            {
                joystickState.Buttons = new ButtonState[jcap.ButtonCount];
            }

            if (joystickState.Hats.Length < jcap.HatCount)
            {
                joystickState.Hats = new JoystickHat[jcap.HatCount];
            }

            for (var i = 0; i < jcap.AxisCount; i++)
                joystickState.Axes[i] = Sdl.Joystick.GetAxis(jdevice, i);

            for (var i = 0; i < jcap.ButtonCount; i++)
                joystickState.Buttons[i] = (Sdl.Joystick.GetButton(jdevice, i) == 0) ? ButtonState.Released : ButtonState.Pressed;

            for (var i = 0; i < jcap.HatCount; i++)
            {
                var hatstate = Sdl.Joystick.GetHat(jdevice, i);

                joystickState.Hats[i] = new JoystickHat
                {
                    Up = ((hatstate & Sdl.Joystick.Hat.Up) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Down = ((hatstate & Sdl.Joystick.Hat.Down) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Left = ((hatstate & Sdl.Joystick.Hat.Left) != 0) ? ButtonState.Pressed : ButtonState.Released,
                    Right = ((hatstate & Sdl.Joystick.Hat.Right) != 0) ? ButtonState.Pressed : ButtonState.Released
                };
            }

            joystickState.IsConnected = true;
        }
    }
}
