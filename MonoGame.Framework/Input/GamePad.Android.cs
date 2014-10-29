using Android.Views;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Input
{
    internal class AndroidGamePad
    {
        public InputDevice _device;
        public int _deviceId;
        public string _descriptor;
        public bool _isConnected;

        public Buttons _buttons;
        public float _leftTrigger, _rightTrigger;
        public Vector2 _leftStick, _rightStick;
        public bool _startButtonPressed;
        public readonly GamePadCapabilities _capabilities;

        public AndroidGamePad(InputDevice device)
        {
            _device = device;
            _deviceId = device.Id;
            _descriptor = device.Descriptor;
            _isConnected = true;
            _capabilities = CapabilitiesOfDevice(device);
        }

        private static GamePadCapabilities CapabilitiesOfDevice(InputDevice device)
        {
            // Because there are a lot of possible Gamepads on Android
            var capabilities = new GamePadCapabilities();
            capabilities.IsConnected = true;
            capabilities.GamePadType = GamePadType.GamePad;
            capabilities.HasLeftVibrationMotor = capabilities.HasRightVibrationMotor = device.Vibrator.HasVibrator;
            capabilities.HasAButton = true;
            capabilities.HasBButton = true;
            capabilities.HasXButton = true;
            capabilities.HasYButton = true;
            capabilities.HasLeftXThumbStick = true;
            capabilities.HasLeftYThumbStick = true;  
            capabilities.HasDPadDownButton = true;
            capabilities.HasDPadLeftButton = true;
            capabilities.HasDPadRightButton = true;
            capabilities.HasDPadUpButton = true;
            capabilities.HasStartButton = true;
            capabilities.HasBackButton = true;
            return capabilities;
        }
    }
        
    static partial class GamePad
    {
        private static readonly AndroidGamePad[] GamePads = new AndroidGamePad[4];
        internal static Buttons _noGamepadButtons;

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            var gamePad = GamePads[index];
            if (gamePad != null)
                return gamePad._capabilities;

            GamePadCapabilities capabilities = new GamePadCapabilities();
            capabilities.IsConnected = false;
            capabilities.HasAButton = true;
            capabilities.HasBButton = true;
            capabilities.HasXButton = true;
            capabilities.HasYButton = true;
            capabilities.HasBackButton = true;
            capabilities.HasLeftXThumbStick = true;
            capabilities.HasLeftYThumbStick = true;
            capabilities.HasRightXThumbStick = true;
            capabilities.HasRightYThumbStick = true;

            return capabilities;
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone deadZoneMode)
        {
            var gamePad = GamePads[index];
            GamePadState state = GamePadState.Default;

            if (gamePad != null && gamePad._isConnected)
            {
                // Check if the device was disconnected
                var dvc = InputDevice.GetDevice(gamePad._deviceId);
                if (dvc == null)
                {
                    Debug.WriteLine("Detected controller disconnect [" + index + "] ");
                    gamePad._isConnected = false;
                    return state;
                }

                GamePadThumbSticks thumbSticks = new GamePadThumbSticks(gamePad._leftStick, gamePad._rightStick);
                thumbSticks.ApplyDeadZone(deadZoneMode, 0.3f);

                if (gamePad._startButtonPressed)
                {
                    gamePad._buttons |= Buttons.Start;
                    gamePad._startButtonPressed = false;
                }
                else
                {
                    gamePad._buttons &= ~Buttons.Start;
                }

                state = new GamePadState(
                    thumbSticks,
                    new GamePadTriggers(gamePad._leftTrigger, gamePad._rightTrigger),
                    new GamePadButtons(gamePad._buttons),
                    new GamePadDPad(gamePad._buttons));
            }
            else
            {
                if (_noGamepadButtons != 0)
                {
                    state = new GamePadState(state.ThumbSticks, 
                        new GamePadTriggers(state.Triggers.Left, state.Triggers.Right), 
                        new GamePadButtons(_noGamepadButtons), 
                        new GamePadDPad(ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released));
                }
            }

            return state;
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            var gamePad = GamePads[index];
            if (gamePad == null)
                return false;

            var vibrator = gamePad._device.Vibrator;
            if (!vibrator.HasVibrator)
                return false;

            vibrator.Vibrate(500);
            return true;
        }

        internal static AndroidGamePad GetGamePad(InputDevice device)
        {
            if (device == null || (device.Sources & InputSourceType.Gamepad) != InputSourceType.Gamepad)
                return null;
                
            int firstDisconnectedPadId = -1;
            for (int i = 0; i < GamePads.Length; i++)
            {
                var pad = GamePads[i];
                if (pad != null && pad._isConnected && pad._deviceId == device.Id)
                {
                    return pad;
                }
                else if (pad != null && !pad._isConnected && pad._descriptor == device.Descriptor)
                {
                    Debug.WriteLine("Found previous controller [" + i + "] " + device.Name);
                    pad._deviceId = device.Id;
                    pad._isConnected = true;
                    return pad;
                }
                else if (pad == null)
                {
                    Debug.WriteLine("Found new controller [" + i + "] " + device.Name);
                    pad = new AndroidGamePad(device);
                    GamePads[i] = pad;
                    return pad;
                }
                else if (!pad._isConnected && firstDisconnectedPadId < 0)
                {
                    firstDisconnectedPadId = i;
                }
            }

            // If we get here, we failed to find a game pad or an empty slot to create one.
            // If we're holding onto a disconnected pad, overwrite it with this one
            if (firstDisconnectedPadId >= 0)
            {
                Debug.WriteLine("Found new controller in place of disconnected controller [" + firstDisconnectedPadId + "] " + device.Name);
                var pad = new AndroidGamePad(device);
                GamePads[firstDisconnectedPadId] = pad;
                return pad;
            }

            // All pad slots are taken so ignore further devices.
            return null;
        }

        internal static bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            var gamePad = GetGamePad(e.Device);
            if (gamePad == null)
            {
                if (keyCode == Keycode.Back)
                {
                    _noGamepadButtons |= ButtonForKeyCode(keyCode);
                    return true;
                }

                return false;
            }

            gamePad._buttons |= ButtonForKeyCode(keyCode);

            if (keyCode == Keycode.Menu)
                gamePad._startButtonPressed = true;

            return true;
        }

        internal static bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            var gamePad = GetGamePad(e.Device);
            if (gamePad == null)
            {
                if (keyCode == Keycode.Back)
                {
                    _noGamepadButtons &= ~ButtonForKeyCode(keyCode);
                    return true;
                }

                return false;
            }

            gamePad._buttons &= ~ButtonForKeyCode(keyCode);
            return true;
        }

        internal static bool OnGenericMotionEvent(MotionEvent e)
        {
            var gamePad = GetGamePad(e.Device);
            if (gamePad == null)
                return false;

            if (e.Action != MotionEventActions.Move)
                return false;

            gamePad._leftStick = new Vector2(e.GetAxisValue(Axis.X), -e.GetAxisValue(Axis.Y));
            gamePad._rightStick = new Vector2(e.GetAxisValue(Axis.Z), -e.GetAxisValue(Axis.Rz));
            gamePad._leftTrigger = e.GetAxisValue(Axis.Ltrigger);
            gamePad._rightTrigger = e.GetAxisValue(Axis.Rtrigger);

            return true;
        }

        private static Buttons ButtonForKeyCode(Keycode keyCode)
        {
            switch (keyCode)
            {
                case Keycode.ButtonA:
                    return Buttons.A;
                case Keycode.ButtonX:
                    return Buttons.X;
                case Keycode.ButtonY:
                    return Buttons.Y;
                case Keycode.ButtonB:
                    return Buttons.B;

                case Keycode.ButtonL1:
                    return Buttons.LeftShoulder;
                case Keycode.ButtonL2:
                    return Buttons.LeftTrigger;
                case Keycode.ButtonR1:
                    return Buttons.RightShoulder;
                case Keycode.ButtonR2:
                    return Buttons.RightTrigger;

                case Keycode.ButtonThumbl:
                    return Buttons.LeftStick;
                case Keycode.ButtonThumbr:
                    return Buttons.RightStick;

                case Keycode.DpadUp:
                    return Buttons.DPadUp;
                case Keycode.DpadDown:
                    return Buttons.DPadDown;
                case Keycode.DpadLeft:
                    return Buttons.DPadLeft;
                case Keycode.DpadRight:
                    return Buttons.DPadRight;
                case Keycode.Menu: 
                    // Home button ?
                case Keycode.ButtonStart:
                    return Buttons.Start;
                case Keycode.Home:
                    return Buttons.BigButton;
                case Keycode.Back:
                    return Buttons.Back;
            }

            return 0;
        }

        internal static void Initialize()
        {
            //Iterate and 'connect' any detected gamepads
            foreach (var deviceId in InputDevice.GetDeviceIds())
            {
                GetGamePad(InputDevice.GetDevice(deviceId));
            }
        }
    }
}
