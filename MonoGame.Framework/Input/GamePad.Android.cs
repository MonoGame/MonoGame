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

        // Workaround for the OnKeyUp and OnKeyDown events for KeyCode.Menu 
        // both being sent in a single frame. This can be removed if the
        // OUYA firmware is updated to send these events in different frames. 
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
            //TODO: There is probably a better way to do this. Maybe device.GetMotionRange and device.GetKeyCharacterMap?
            //Or not http://stackoverflow.com/questions/11686703/android-enumerating-the-buttons-on-a-gamepad

            var capabilities = new GamePadCapabilities();
            capabilities.IsConnected = true;
            capabilities.GamePadType = GamePadType.GamePad;
            capabilities.HasLeftVibrationMotor = capabilities.HasRightVibrationMotor = device.Vibrator.HasVibrator;

            switch (device.Name)
            {
                default:
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
                    break;
            }
            return capabilities;
        }
    }


    static partial class GamePad
    {
        private const int maxGamepad = 1;

#if OUYA
        private const maxGamepad = OuyaController.MaxControllers;
#endif
        private static readonly AndroidGamePad[] GamePads = new AndroidGamePad[maxGamepad];
        internal static bool Back = false;

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
            if (index == 0 && Back)
            {
                // Consume state
                Back = false;
                return new GamePadState(new GamePadThumbSticks(), new GamePadTriggers(), new GamePadButtons(Buttons.Back), new GamePadDPad());
            }

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

            // The recommended way to map devices to players numbers is to use OuyaController.GetPlayerNumByDeviceId(), 
            // however as of ODK 0.0.6 there is a bug where disconnected and reconnected controllers get mapped
            // to new player numbers. Also, the player number returned does not match the LED on the controller.
            // Once this is fixed, we could consider using OuyaController.GetPlayerNumByDeviceId()
            // http://forums.ouya.tv/discussion/819/getplayernumbydeviceid-can-return-1-after-controllers-disconnect-and-reconnect

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
                if (e.KeyCode == Keycode.Back)
                {
                    GamePads[0]._buttons |= Buttons.Back;
                    return true;
                }

                return false;
            }

            gamePad._buttons |= ButtonForKeyCode(keyCode);

            if (keyCode == Keycode.Menu)
            {
                gamePad._startButtonPressed = true;
            }
            return true;
        }

        internal static bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            var gamePad = GetGamePad(e.Device);
            if (gamePad == null)
            {
                if (e.KeyCode == Keycode.Back)
                {
                    GamePads[0]._buttons &= ~Buttons.Back;
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
                case Keycode.ButtonA: //O
                    return Buttons.A;
                case Keycode.ButtonX: //U
                    return Buttons.X;
                case Keycode.ButtonY: //Y
                    return Buttons.Y;
                case Keycode.ButtonB: //A
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

                    // Ouya system button sends Keycode.Menu after a delay if no
                    // double tap or hold is detected. It also sends Keycode.Home just
                    // before the system menu is opened (but not on dev kit controllers)
                    // http://forums.ouya.tv/discussion/comment/6076/#Comment_6076
                case Keycode.Menu:
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
