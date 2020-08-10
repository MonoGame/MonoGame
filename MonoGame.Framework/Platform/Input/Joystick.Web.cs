// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using JSIL;

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        private const bool PlatformIsSupported = true;

        internal static bool TrackEvents = false;

        private static JoystickCapabilities PlatformGetCapabilities(int index)
        {
            bool connected = false;
            string id = "";
            int axiscount = 0;
            int buttoncount = 0;

            var navigator = Builtins.Global["navigator"];
            var gamepads = navigator.getGamepads ? navigator.getGamepads() : navigator.webkitGetGamepads();

            if (gamepads.length > index)
            {
                if (gamepads[index])
                {
                    connected = true;
                    id = gamepads[index].id;
                    axiscount = gamepads[index].axes.length;
                    buttoncount = gamepads[index].buttons.length;
                }
            }

            return new JoystickCapabilities()
            {
                IsConnected = connected,
                DisplayName = string.Empty,
                Identifier = id,
                AxisCount = axiscount,
                ButtonCount = buttoncount,
                HatCount = 0
            };
        }

        private static int PlatformLastConnectedIndex
        {
            get
            {
                //Stubbed so it compiles
                return -1;
            }
        }

        private static JoystickState PlatformGetState(int index)
        {
            var connected = false;
            var axes = _defaultJoystickState.Axes;
            var buttons = _defaultJoystickState.Buttons;

            var navigator = Builtins.Global["navigator"];
            var gamepads = navigator.getGamepads ? navigator.getGamepads() : navigator.webkitGetGamepads();

            if (gamepads.length > index)
            {
                if (gamepads[index])
                {
                    connected = true;

                    var axescount = gamepads[index].axes.length;
                    axes = new int[gamepads[index].axes.length];

                    for (int i = 0; i < axescount; i++)
                        axes[i] = gamepads[index].axes[i];

                    var buttoncount = gamepads[index].buttons.length;
                    buttons = new ButtonState[buttoncount];

                    for (int i = 0; i < buttoncount; i++)
                    {
                        if (gamepads[index].buttons[i].pressed)
                            buttons[i] = ButtonState.Pressed;
                        else
                            buttons[i] = ButtonState.Released;
                    }
                }
            }

            return new JoystickState()
            {
                IsConnected = connected,
                Axes = axes,
                Buttons = buttons,
                Hats = _defaultJoystickState.Hats
            };
        }

        private static void PlatformGetState(ref JoystickState joystickState, int index)
        {
            joystickState.IsConnected = false;

            var navigator = Builtins.Global["navigator"];
            var gamepads = navigator.getGamepads ? navigator.getGamepads() : navigator.webkitGetGamepads();

            if (gamepads.length > index)
            {
                if (gamepads[index])
                {
                    joystickState.IsConnected = true;

                    var axescount = gamepads[index].axes.length;
                    if (joystickState.Axes.Length < axescount)
                    {
                        joystickState.Axes = new int[axescount];
                    }

                    for (int i = 0; i < axescount; i++)
                        joystickState.Axes[i] = gamepads[index].axes[i];

                    var buttoncount = gamepads[index].buttons.length;
                    if (joystickState.Buttons.Length < buttoncount)
                    {
                        joystickState.Buttons = new ButtonState[buttoncount];
                    }
                    
                    for (int i = 0; i < buttoncount; i++)
                    {
                        if (gamepads[index].buttons[i].pressed)
                            joystickState.Buttons[i] = ButtonState.Pressed;
                        else
                            joystickState.Buttons[i] = ButtonState.Released;
                    }
                }
            }

            if (joystickState.Hats == null)
            {
                joystickState.Hats = _defaultJoystickState.Hats;
            }
        }
    }
}
