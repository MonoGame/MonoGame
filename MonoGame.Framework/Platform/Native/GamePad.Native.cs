// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Input;

static partial class GamePad
{
    static int MaxSupported = MGP.GamePad_GetMaxSupported();

    static internal unsafe MGP_Platform* Handle;

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

    internal static unsafe void Add(int identifier)
    {
        var state = new State();
        state.Identifier = identifier;

        // Setup the caps for this device.
        MGP_ControllerCaps caps;
        MGP.GamePad_GetCaps(Handle, identifier, & caps);
        {
            state.Caps.IsConnected = true;
            state.Caps.Identifier = Marshal.PtrToStringUTF8(caps.Identifier);
            state.Caps.DisplayName = Marshal.PtrToStringUTF8(caps.DisplayName);
            state.Caps.GamePadType = caps.GamePadType;
            state.Caps.HasDPadUpButton = (caps.InputFlags & (1 << (int)ControllerInput.DpadUp)) != 0;
            state.Caps.HasDPadDownButton = (caps.InputFlags & (1 << (int)ControllerInput.DpadDown)) != 0;
            state.Caps.HasDPadLeftButton = (caps.InputFlags & (1 << (int)ControllerInput.DpadLeft)) != 0;
            state.Caps.HasDPadRightButton = (caps.InputFlags & (1 << (int)ControllerInput.DpadRight)) != 0;
            state.Caps.HasAButton = (caps.InputFlags & (1 << (int)ControllerInput.A)) != 0;
            state.Caps.HasBButton = (caps.InputFlags & (1 << (int)ControllerInput.B)) != 0;
            state.Caps.HasXButton = (caps.InputFlags & (1 << (int)ControllerInput.X)) != 0;
            state.Caps.HasYButton = (caps.InputFlags & (1 << (int)ControllerInput.Y)) != 0;
            state.Caps.HasLeftShoulderButton = (caps.InputFlags & (1 << (int)ControllerInput.LeftShoulder)) != 0;
            state.Caps.HasRightShoulderButton = (caps.InputFlags & (1 << (int)ControllerInput.RightShoulder)) != 0;
            state.Caps.HasLeftTrigger = (caps.InputFlags & (1 << (int)ControllerInput.LeftTrigger)) != 0;
            state.Caps.HasRightTrigger = (caps.InputFlags & (1 << (int)ControllerInput.RightTrigger)) != 0;
            state.Caps.HasBackButton = (caps.InputFlags & (1 << (int)ControllerInput.Back)) != 0;
            state.Caps.HasStartButton = (caps.InputFlags & (1 << (int)ControllerInput.Start)) != 0;
            state.Caps.HasLeftStickButton = (caps.InputFlags & (1 << (int)ControllerInput.LeftStick)) != 0;
            state.Caps.HasLeftXThumbStick = (caps.InputFlags & (1 << (int)ControllerInput.LeftStickX)) != 0;
            state.Caps.HasLeftYThumbStick = (caps.InputFlags & (1 << (int)ControllerInput.LeftStickY)) != 0;
            state.Caps.HasRightStickButton = (caps.InputFlags & (1 << (int)ControllerInput.RightStick)) != 0;
            state.Caps.HasRightXThumbStick = (caps.InputFlags & (1 << (int)ControllerInput.RightStickX)) != 0;
            state.Caps.HasRightYThumbStick = (caps.InputFlags & (1 << (int)ControllerInput.RightStickY)) != 0;
            state.Caps.HasBigButton = (caps.InputFlags & (1 << (int)ControllerInput.Guide)) != 0;
            state.Caps.HasVoiceSupport = caps.HasVoiceSupport;
            state.Caps.HasLeftVibrationMotor = caps.HasLeftVibrationMotor;
            state.Caps.HasRightVibrationMotor = caps.HasRightVibrationMotor;
        }

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

    private static unsafe bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
    {
        if (Handle == null)
            return false;

        if (!_stateByIndex.TryGetValue(index, out var state))
            return false;

        return MGP.GamePad_SetVibration(Handle, state.Identifier, leftMotor, rightMotor, leftTrigger, rightTrigger);
    }
}
