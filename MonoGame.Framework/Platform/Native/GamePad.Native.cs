// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input;

static partial class GamePad
{
    static int MaxSupported = MGP.GamePad_GetMaxSupported();

    class State
    {
        public int Identifier;
        public int Index;

        public ulong Timestamp;

        public Buttons Buttons;
        public Vector2 ThumbstickL;
        public Vector2 ThumbstickR;
        public float TriggerL;
        public float TriggerR;

        public GamePadCapabilities Caps;
    }

    private static readonly Dictionary<int, State> _stateByIndex = new Dictionary<int, State>();
    private static readonly Dictionary<int, State> _stateById = new Dictionary<int, State>();

    internal static void Add(int identifier)
    {
        var state = new State();
        state.Identifier = identifier;
        state.Caps.IsConnected = true;

        // TODO: Maybe the platform should supply
        // the correct player index as some platforms
        // could define this.

        for (int i = 0; i < MaxSupported; i++)
        {
            // Add it to the first empty player index.
            if (!_stateByIndex.ContainsKey(i))
            {
                state.Index = i;
                _stateByIndex.Add(i, state);
                _stateById.Add(identifier, state);
                return;
            }
        }
    }

    internal static void Remove(int identifier)
    {
        if (_stateById.TryGetValue(identifier, out var state))
        {
            _stateByIndex.Remove(state.Index);
            _stateById.Remove(identifier);
        }
    }

    private static Buttons InputToButton(ControllerInput input)
    {
        switch (input)
        {
            case ControllerInput.A:
                return Buttons.A;
            case ControllerInput.B:
                return Buttons.B;
            case ControllerInput.X:
                return Buttons.X;
            case ControllerInput.Y:
                return Buttons.Y;
            case ControllerInput.Back:
                return Buttons.Back;
            case ControllerInput.Guide:
                return Buttons.BigButton;
            case ControllerInput.Start:
                return Buttons.Start;
            case ControllerInput.LeftStick:
                return Buttons.LeftStick;
            case ControllerInput.RightStick:
                return Buttons.RightStick;
            case ControllerInput.LeftShoulder:
                return Buttons.LeftShoulder;
            case ControllerInput.RightShoulder:
                return Buttons.RightShoulder;
            case ControllerInput.DpadUp:
                return Buttons.DPadUp;
            case ControllerInput.DpadDown:
                return Buttons.DPadDown;
            case ControllerInput.DpadLeft:
                return Buttons.DPadLeft;
            case ControllerInput.DpadRight:
                return Buttons.DPadRight;
        }

        // Unsupported button input.
        return Buttons.None;
    }

    private static float FromAxisValue(short axis)
    {
        if (axis < 0)
            return axis / 32768f;
        return axis / 32767f;
    }

    internal static void ChangeState(int identifier, ulong timestamp, ControllerInput input, short value)
    {
        if (!_stateById.TryGetValue(identifier, out var state))
            return;

        state.Timestamp = timestamp;

        if (input > ControllerInput.LAST_BUTTON)
        {
            switch (input)
            {
                case ControllerInput.LeftStickX:
                    state.ThumbstickL.X = FromAxisValue(value);
                    break;
                case ControllerInput.LeftStickY:
                    state.ThumbstickL.Y = FromAxisValue(value);
                    break;
                case ControllerInput.RightStickX:
                    state.ThumbstickR.X = FromAxisValue(value);
                    break;
                case ControllerInput.RightStickY:
                    state.ThumbstickR.Y = FromAxisValue(value);
                    break;
                case ControllerInput.LeftTrigger:
                    state.TriggerL = FromAxisValue(value);
                    break;
                case ControllerInput.RightTrigger:
                    state.TriggerR = FromAxisValue(value);
                    break;
            }
        }
        else
        {
            var button = InputToButton(input);
            if (value == 0)
                state.Buttons &= ~button;
            else
                state.Buttons |= button;
        }
    }

    private static int PlatformGetMaxNumberOfGamePads()
    {
        return MaxSupported;
    }

    private static GamePadCapabilities PlatformGetCapabilities(int index)
    {
        if (_stateByIndex.TryGetValue(index, out var state))
            return state.Caps;

        return new GamePadCapabilities();
    }

    private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
    {
        if (_stateByIndex.TryGetValue(index, out var state))
        {
            return new GamePadState(
                new GamePadThumbSticks(state.ThumbstickL, state.ThumbstickR, leftDeadZoneMode, rightDeadZoneMode),
                new GamePadTriggers(state.TriggerL, state.TriggerR),
                new GamePadButtons(state.Buttons),
                new GamePadDPad(state.Buttons));
        }

        return new GamePadState();
    }

    private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
    {
        return false;
    }
}
