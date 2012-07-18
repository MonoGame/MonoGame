using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GBF = SharpDX.XInput.GamepadButtonFlags;

namespace Microsoft.Xna.Framework.Input
{
    public static class GamePad
    {
        public static Microsoft.Xna.Framework.Input.GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
        {
            var controller = GetController(playerIndex);
            var capabilities = controller.GetCapabilities(SharpDX.XInput.DeviceQueryType.Any);
            var ret = new Microsoft.Xna.Framework.Input.GamePadCapabilities();
            switch (capabilities.SubType)
            {
                case SharpDX.XInput.DeviceSubType.ArcadePad:
                    Debug.WriteLine("XInput's DeviceSubType.ArcadePad is not supported in XNA");
                    ret.GamePadType = Input.GamePadType.Unknown; // TODO: Should this be BigButtonPad?
                    break;
                case SharpDX.XInput.DeviceSubType.ArcadeStick:
                    ret.GamePadType = Input.GamePadType.ArcadeStick;
                    break;
                case SharpDX.XInput.DeviceSubType.DancePad:
                    ret.GamePadType = Input.GamePadType.DancePad;
                    break;
                case SharpDX.XInput.DeviceSubType.DrumKit:
                    ret.GamePadType = Input.GamePadType.DrumKit;
                    break;
                case SharpDX.XInput.DeviceSubType.FlightStick:
                    ret.GamePadType = Input.GamePadType.FlightStick;
                    break;
                case SharpDX.XInput.DeviceSubType.Gamepad:
                    ret.GamePadType = Input.GamePadType.GamePad;
                    break;
                case SharpDX.XInput.DeviceSubType.Guitar:
                    ret.GamePadType = Input.GamePadType.Guitar;
                    break;
                case SharpDX.XInput.DeviceSubType.GuitarAlternate:
                    ret.GamePadType = Input.GamePadType.AlternateGuitar;
                    break;
                case SharpDX.XInput.DeviceSubType.GuitarBass:
                    // Note: XNA doesn't distinguish between Guitar and GuitarBass, but 
                    // GuitarBass is identical to Guitar in XInput, distinguished only
                    // to help setup for those controllers. 
                    ret.GamePadType = Input.GamePadType.Guitar;
                    break;
                case SharpDX.XInput.DeviceSubType.Unknown:
                    ret.GamePadType = Input.GamePadType.Unknown;
                    break;
                case SharpDX.XInput.DeviceSubType.Wheel:
                    ret.GamePadType = Input.GamePadType.Wheel;
                    break;
                default:
                    Debug.WriteLine("unexpected XInput DeviceSubType: {0}", capabilities.SubType.ToString());
                    ret.GamePadType = Input.GamePadType.Unknown;
                    break;
            }

            var gamepad = capabilities.Gamepad;

            // digital buttons
            var buttons = gamepad.Buttons;
            ret.HasAButton = buttons.HasFlag(GBF.A);
            ret.HasBackButton = buttons.HasFlag(GBF.Back);
            ret.HasBButton = buttons.HasFlag(GBF.B);
            ret.HasBigButton = false; // TODO: what IS this? Is it related to GamePadType.BigGamePad?
            ret.HasDPadDownButton = buttons.HasFlag(GBF.DPadDown);
            ret.HasDPadLeftButton = buttons.HasFlag(GBF.DPadLeft);
            ret.HasDPadRightButton = buttons.HasFlag(GBF.DPadLeft);
            ret.HasDPadUpButton = buttons.HasFlag(GBF.DPadUp);
            ret.HasLeftShoulderButton = buttons.HasFlag(GBF.LeftShoulder);
            ret.HasLeftStickButton = buttons.HasFlag(GBF.LeftThumb);
            ret.HasRightShoulderButton = buttons.HasFlag(GBF.RightShoulder);
            ret.HasRightStickButton = buttons.HasFlag(GBF.RightThumb);
            ret.HasStartButton = buttons.HasFlag(GBF.Start);
            ret.HasXButton = buttons.HasFlag(GBF.X);
            ret.HasYButton = buttons.HasFlag(GBF.Y);

            // analog controls
            ret.HasRightTrigger = gamepad.LeftTrigger > 0;
            ret.HasRightXThumbStick = gamepad.RightThumbX != 0;
            ret.HasRightYThumbStick = gamepad.RightThumbY != 0;
            ret.HasLeftTrigger = gamepad.LeftTrigger > 0;
            ret.HasLeftXThumbStick = gamepad.LeftThumbX != 0;
            ret.HasLeftYThumbStick = gamepad.LeftThumbY != 0;
            
            // vibration
            bool hasForceFeedback = capabilities.Flags.HasFlag(SharpDX.XInput.CapabilityFlags.FfbSupported);
            ret.HasLeftVibrationMotor = hasForceFeedback && capabilities.Vibration.LeftMotorSpeed > 0;
            ret.HasRightVibrationMotor = hasForceFeedback && capabilities.Vibration.RightMotorSpeed > 0;

            // other
            ret.IsConnected = controller.IsConnected;
            ret.HasVoiceSupport = capabilities.Flags.HasFlag(SharpDX.XInput.CapabilityFlags.VoiceSupported);

            return ret;
        }

        public static Microsoft.Xna.Framework.Input.GamePadState GetState(PlayerIndex playerIndex)
        {
            return GetState(playerIndex, Microsoft.Xna.Framework.Input.GamePadDeadZone.IndependentAxes);
        }

        private static SharpDX.XInput.Controller playerOne = new SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.One);
        private static SharpDX.XInput.Controller playerTwo = new SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.Two);
        private static SharpDX.XInput.Controller playerThree = new SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.Three);
        private static SharpDX.XInput.Controller playerFour = new SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.Four);
        private static SharpDX.XInput.Controller playerAny = new SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.Any);

        public static Microsoft.Xna.Framework.Input.GamePadState GetState(PlayerIndex playerIndex,
            Microsoft.Xna.Framework.Input.GamePadDeadZone deadZoneMode)
        {
            var controller = GetController(playerIndex);
            var gamepad = controller.GetState().Gamepad;

            Microsoft.Xna.Framework.Input.GamePadThumbSticks thumbSticks = new Microsoft.Xna.Framework.Input.GamePadThumbSticks(
                leftPosition: ConvertThumbStick(gamepad.LeftThumbX, gamepad.LeftThumbY,
                    SharpDX.XInput.Gamepad.LeftThumbDeadZone, deadZoneMode),
                rightPosition: ConvertThumbStick(gamepad.RightThumbX, gamepad.RightThumbY,
                    SharpDX.XInput.Gamepad.RightThumbDeadZone, deadZoneMode));

            Microsoft.Xna.Framework.Input.GamePadTriggers triggers = new Microsoft.Xna.Framework.Input.GamePadTriggers(
                    leftTrigger: gamepad.LeftTrigger / (float)byte.MaxValue,
                    rightTrigger: gamepad.RightTrigger / (float)byte.MaxValue);

            Microsoft.Xna.Framework.Input.GamePadState state = new Microsoft.Xna.Framework.Input.GamePadState(
                thumbSticks: thumbSticks,
                triggers: triggers,
                buttons: ConvertToButtons(
                    buttonFlags: gamepad.Buttons,
                    leftThumbX: gamepad.LeftThumbX,
                    leftThumbY: gamepad.LeftThumbY,
                    rightThumbX: gamepad.RightThumbX,
                    rightThumbY: gamepad.RightThumbY,
                    leftTrigger: gamepad.LeftTrigger,
                    rightTrigger: gamepad.RightTrigger),
                dPad: ConvertToGamePadDPad(gamepad.Buttons));

            return state;
        }

        private static SharpDX.XInput.Controller GetController(PlayerIndex playerIndex)
        {
            SharpDX.XInput.Controller controller = null;
            switch (playerIndex)
            {
                case PlayerIndex.One:
                    // TODO: need to research XInput vs. XNA behavior with regards to which player controllers
                    // are assigned to in XInput, and if they are reassigned as controllers are added/removed.
                    // for now, we won't use playerAny unless you pass (PlayerIndex)0
                    //controller = !playerOne.IsConnected && playerAny.IsConnected ? playerAny : playerOne;
                    controller = playerOne;
                    break;
                case PlayerIndex.Two:
                    controller = playerTwo;
                    break;
                case PlayerIndex.Three:
                    controller = playerThree;
                    break;
                case PlayerIndex.Four:
                    controller = playerFour;
                    break;
                default:
                    controller = playerAny;
                    break;
            }
            return controller;
        }

        private static Vector2 ConvertThumbStick(
            short x, short y, short deadZone, Microsoft.Xna.Framework.Input.GamePadDeadZone deadZoneMode)
        {
            if (deadZoneMode == Microsoft.Xna.Framework.Input.GamePadDeadZone.IndependentAxes)
            {
                // using int to prevent overrun
                int fx = x;
                int fy = y;
                int fdz = deadZone;
                if (fx * fx < fdz * fdz)
                    x = 0;
                if (fy * fy < fdz * fdz)
                    y = 0;
            }
            else if (deadZoneMode == Microsoft.Xna.Framework.Input.GamePadDeadZone.Circular)
            {
                // using int to prevent overrun
                int fx = x;
                int fy = y;
                int fdz = deadZone;
                if ((fx * fx) + (fy * fy) < fdz * fdz)
                {
                    x = 0;
                    y = 0;
                }
            }

            return new Vector2(x < 0 ? -((float)x / (float)short.MinValue) : (float)x / (float)short.MaxValue,
                               y < 0 ? -((float)y / (float)short.MinValue) : (float)y / (float)short.MaxValue);
        }

        private static Microsoft.Xna.Framework.Input.ButtonState ConvertToButtonState(
            SharpDX.XInput.GamepadButtonFlags buttonFlags,
            SharpDX.XInput.GamepadButtonFlags desiredButton)
        {
            return buttonFlags.HasFlag(desiredButton) ? 
                Microsoft.Xna.Framework.Input.ButtonState.Pressed : Microsoft.Xna.Framework.Input.ButtonState.Released;
        }

        private static Microsoft.Xna.Framework.Input.GamePadDPad ConvertToGamePadDPad(
            SharpDX.XInput.GamepadButtonFlags buttonFlags)
        {
            return new Microsoft.Xna.Framework.Input.GamePadDPad(
                upValue: ConvertToButtonState(buttonFlags, SharpDX.XInput.GamepadButtonFlags.DPadUp),
                downValue: ConvertToButtonState(buttonFlags, SharpDX.XInput.GamepadButtonFlags.DPadDown),
                leftValue: ConvertToButtonState(buttonFlags, SharpDX.XInput.GamepadButtonFlags.DPadLeft),
                rightValue: ConvertToButtonState(buttonFlags, SharpDX.XInput.GamepadButtonFlags.DPadRight));
        }

        private static Microsoft.Xna.Framework.Input.Buttons AddButtonIfPressed(Microsoft.Xna.Framework.Input.Buttons originalButtonState,
            SharpDX.XInput.GamepadButtonFlags buttonFlags,
            SharpDX.XInput.GamepadButtonFlags xInputButton,
            Microsoft.Xna.Framework.Input.Buttons xnaButton)
        {
            Microsoft.Xna.Framework.Input.ButtonState buttonState = ConvertToButtonState(buttonFlags, xInputButton);
            return buttonState == Microsoft.Xna.Framework.Input.ButtonState.Pressed ? originalButtonState | xnaButton : originalButtonState;
        }

        private static readonly List<Tuple<GBF, Microsoft.Xna.Framework.Input.Buttons>> buttonMap
            = new List<Tuple<GBF, Microsoft.Xna.Framework.Input.Buttons>>()
            {
                Tuple.Create(GBF.A, Microsoft.Xna.Framework.Input.Buttons.A),
                Tuple.Create(GBF.B, Microsoft.Xna.Framework.Input.Buttons.B),
                Tuple.Create(GBF.Back, Microsoft.Xna.Framework.Input.Buttons.Back),
                Tuple.Create(GBF.DPadDown, Microsoft.Xna.Framework.Input.Buttons.DPadDown),
                Tuple.Create(GBF.DPadLeft, Microsoft.Xna.Framework.Input.Buttons.DPadLeft),
                Tuple.Create(GBF.DPadRight, Microsoft.Xna.Framework.Input.Buttons.DPadRight),
                Tuple.Create(GBF.DPadUp, Microsoft.Xna.Framework.Input.Buttons.DPadUp),
                Tuple.Create(GBF.LeftShoulder, Microsoft.Xna.Framework.Input.Buttons.LeftShoulder),
                Tuple.Create(GBF.RightShoulder, Microsoft.Xna.Framework.Input.Buttons.RightShoulder),
                Tuple.Create(GBF.LeftThumb, Microsoft.Xna.Framework.Input.Buttons.LeftStick),
                Tuple.Create(GBF.RightThumb, Microsoft.Xna.Framework.Input.Buttons.RightStick),
                Tuple.Create(GBF.Start, Microsoft.Xna.Framework.Input.Buttons.Start),
                Tuple.Create(GBF.X, Microsoft.Xna.Framework.Input.Buttons.X),
                Tuple.Create(GBF.Y, Microsoft.Xna.Framework.Input.Buttons.Y),
            };

        private static Microsoft.Xna.Framework.Input.Buttons AddThumbstickButtons(
            short thumbX, short thumbY, short deadZone, 
            Microsoft.Xna.Framework.Input.Buttons bitFieldToAddTo,
            Microsoft.Xna.Framework.Input.Buttons thumbstickLeft, 
            Microsoft.Xna.Framework.Input.Buttons thumbStickRight, 
            Microsoft.Xna.Framework.Input.Buttons thumbStickUp, 
            Microsoft.Xna.Framework.Input.Buttons thumbStickDown)
        {
            // TODO: this needs adjustment. Very naive implementation. Doesn't match XNA yet
            if (thumbX < -deadZone)
                bitFieldToAddTo = bitFieldToAddTo | thumbstickLeft;
            if (thumbX > deadZone)
                bitFieldToAddTo = bitFieldToAddTo | thumbStickRight;
            if (thumbY < -deadZone)
                bitFieldToAddTo = bitFieldToAddTo | thumbStickUp;
            else if (thumbY > deadZone)
                bitFieldToAddTo = bitFieldToAddTo | thumbStickDown;
            return bitFieldToAddTo;
        }

        private static Microsoft.Xna.Framework.Input.GamePadButtons ConvertToButtons(SharpDX.XInput.GamepadButtonFlags buttonFlags,
            short leftThumbX, short leftThumbY,
            short rightThumbX, short rightThumbY,
            byte leftTrigger,
            byte rightTrigger)
        {
            Microsoft.Xna.Framework.Input.Buttons ret = new Microsoft.Xna.Framework.Input.Buttons();
            for (int i = 0; i < buttonMap.Count; i++)
            {
                var curMap = buttonMap[i];
                ret = AddButtonIfPressed(ret, buttonFlags, curMap.Item1, curMap.Item2);
            }

            ret = AddThumbstickButtons(leftThumbX, leftThumbY,
                SharpDX.XInput.Gamepad.LeftThumbDeadZone, ret,
                Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickLeft, 
                Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickRight, 
                Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickUp, 
                Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickDown);

            ret = AddThumbstickButtons(rightThumbX, rightThumbY,
                SharpDX.XInput.Gamepad.RightThumbDeadZone, ret,
                Microsoft.Xna.Framework.Input.Buttons.RightThumbstickLeft, 
                Microsoft.Xna.Framework.Input.Buttons.RightThumbstickRight, 
                Microsoft.Xna.Framework.Input.Buttons.RightThumbstickUp, 
                Microsoft.Xna.Framework.Input.Buttons.RightThumbstickDown);

            if (leftTrigger >= SharpDX.XInput.Gamepad.TriggerThreshold)
                ret = ret | Microsoft.Xna.Framework.Input.Buttons.LeftTrigger;

            if (rightTrigger >= SharpDX.XInput.Gamepad.TriggerThreshold)
                ret = ret | Microsoft.Xna.Framework.Input.Buttons.RightTrigger;

            var r = new Microsoft.Xna.Framework.Input.GamePadButtons(ret);
            return r;
        }
    }
}
