// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using GameController;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        private static int PlatformGetMaxNumberOfGamePads()
        {
            return 4;
        }

        static bool IndexIsUsed(GCControllerPlayerIndex index)
        {
            foreach (var ctrl in GCController.Controllers)
                if (ctrl.PlayerIndex == index) return true;

            return false;
        }

        static void AssingIndex(GCControllerPlayerIndex index)
        {
            if (IndexIsUsed(index))
                return;
            foreach (var controller in GCController.Controllers)
            {
                if (controller.PlayerIndex == index)
                    break;
                if (controller.PlayerIndex == GCControllerPlayerIndex.Unset)
                {
                    controller.PlayerIndex = index;
                    break;
                }
            }
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            var ind = (GCControllerPlayerIndex)index;

            AssingIndex(ind);

            foreach (var controller in GCController.Controllers)
            {
                if (controller == null)
                    continue;
                if (controller.PlayerIndex == ind)
                    return GetCapabilities(controller);
            }
            return new GamePadCapabilities { IsConnected = false };
        }

        private static GamePadCapabilities GetCapabilities(GCController controller)
        {
            //All iOS controllers have these basics
            var capabilities = new GamePadCapabilities()
            {
                IsConnected = false,
                GamePadType = GamePadType.GamePad,
            };
            if (controller.ExtendedGamepad != null)
            {
                capabilities.HasAButton = true;
                capabilities.HasBButton = true;
                capabilities.HasXButton = true;
                capabilities.HasYButton = true;
                capabilities.HasBackButton = true;
                capabilities.HasStartButton = true;
                capabilities.HasDPadUpButton = true;
                capabilities.HasDPadDownButton = true;
                capabilities.HasDPadLeftButton = true;
                capabilities.HasDPadRightButton = true;
                capabilities.HasLeftShoulderButton = true;
                capabilities.HasRightShoulderButton = true;
                capabilities.HasLeftTrigger = true;
                capabilities.HasRightTrigger = true;
                capabilities.HasLeftXThumbStick = true;
                capabilities.HasLeftYThumbStick = true;
                capabilities.HasRightXThumbStick = true;
                capabilities.HasRightYThumbStick = true;
            }
            else if (controller.Gamepad != null)
            {
                capabilities.HasAButton = true;
                capabilities.HasBButton = true;
                capabilities.HasXButton = true;
                capabilities.HasYButton = true;
                capabilities.HasDPadUpButton = true;
                capabilities.HasDPadDownButton = true;
                capabilities.HasDPadLeftButton = true;
                capabilities.HasDPadRightButton = true;
                capabilities.HasLeftShoulderButton = true;
                capabilities.HasRightShoulderButton = true;
            }
            return capabilities;
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            var ind = (GCControllerPlayerIndex)index;


            Buttons buttons = 0;
            bool connected = false;
            ButtonState Up = ButtonState.Released;
            ButtonState Down = ButtonState.Released;
            ButtonState Left = ButtonState.Released;
            ButtonState Right = ButtonState.Released;

            Vector2 leftThumbStickPosition = Vector2.Zero;
            Vector2 rightThumbStickPosition = Vector2.Zero;

            float leftTriggerValue = 0;
            float rightTriggerValue = 0;

            AssingIndex(ind);

            foreach (var controller in GCController.Controllers)
            {

                if (controller == null)
                    continue;

                if (controller.PlayerIndex != ind)
                    continue;

                connected = true;

                if (controller.ExtendedGamepad != null)
                {
                    if (controller.ExtendedGamepad.ButtonA.IsPressed)
                        buttons |= Buttons.A;
                    if (controller.ExtendedGamepad.ButtonB.IsPressed)
                        buttons |= Buttons.B;
                    if (controller.ExtendedGamepad.ButtonX.IsPressed)
                        buttons |= Buttons.X;
                    if (controller.ExtendedGamepad.ButtonY.IsPressed)
                        buttons |= Buttons.Y;

                    if (controller.ExtendedGamepad.LeftShoulder.IsPressed)
                        buttons |= Buttons.LeftShoulder;
                    if (controller.ExtendedGamepad.RightShoulder.IsPressed)
                        buttons |= Buttons.RightShoulder;

                    if (controller.ExtendedGamepad.LeftTrigger.IsPressed)
                        buttons |= Buttons.LeftTrigger;
                    if (controller.ExtendedGamepad.RightTrigger.IsPressed)
                        buttons |= Buttons.RightTrigger;

                    if (controller.ExtendedGamepad.ButtonMenu.IsPressed)
                        buttons |= Buttons.Start;
                    if (controller.ExtendedGamepad.ButtonOptions?.IsPressed == true)
                        buttons |= Buttons.Back;

                    if (controller.ExtendedGamepad.DPad.Up.IsPressed)
                    {
                        Up = ButtonState.Pressed;
                        buttons |= Buttons.DPadUp;
                    }
                    if (controller.ExtendedGamepad.DPad.Down.IsPressed)
                    {
                        Down = ButtonState.Pressed;
                        buttons |= Buttons.DPadDown;
                    }
                    if (controller.ExtendedGamepad.DPad.Left.IsPressed)
                    {
                        Left = ButtonState.Pressed;
                        buttons |= Buttons.DPadLeft;
                    }
                    if (controller.ExtendedGamepad.DPad.Right.IsPressed)
                    {
                        Right = ButtonState.Pressed;
                        buttons |= Buttons.DPadRight;
                    }

                    leftThumbStickPosition.X = controller.ExtendedGamepad.LeftThumbstick.XAxis.Value;
                    leftThumbStickPosition.Y = controller.ExtendedGamepad.LeftThumbstick.YAxis.Value;
                    rightThumbStickPosition.X = controller.ExtendedGamepad.RightThumbstick.XAxis.Value;
                    rightThumbStickPosition.Y = controller.ExtendedGamepad.RightThumbstick.YAxis.Value;
                    leftTriggerValue = controller.ExtendedGamepad.LeftTrigger.Value;
                    rightTriggerValue = controller.ExtendedGamepad.RightTrigger.Value;
                }
                else if (controller.Gamepad != null)
                {
                    if (controller.Gamepad.ButtonA.IsPressed)
                        buttons |= Buttons.A;
                    if (controller.Gamepad.ButtonB.IsPressed)
                        buttons |= Buttons.B;
                    if (controller.Gamepad.ButtonX.IsPressed)
                        buttons |= Buttons.X;
                    if (controller.Gamepad.ButtonY.IsPressed)
                        buttons |= Buttons.Y;

                    if (controller.Gamepad.DPad.Up.IsPressed)
                    {
                        Up = ButtonState.Pressed;
                        buttons |= Buttons.DPadUp;
                    }
                    if (controller.Gamepad.DPad.Down.IsPressed)
                    {
                        Down = ButtonState.Pressed;
                        buttons |= Buttons.DPadDown;
                    }
                    if (controller.Gamepad.DPad.Left.IsPressed)
                    {
                        Left = ButtonState.Pressed;
                        buttons |= Buttons.DPadLeft;
                    }
                    if (controller.Gamepad.DPad.Right.IsPressed)
                    {
                        Right = ButtonState.Pressed;
                        buttons |= Buttons.DPadRight;
                    }
                }
            }
            var state = new GamePadState(
                new GamePadThumbSticks(leftThumbStickPosition, rightThumbStickPosition, leftDeadZoneMode, rightDeadZoneMode),
                new GamePadTriggers(leftTriggerValue, rightTriggerValue),
                new GamePadButtons(buttons),
                new GamePadDPad(Up, Down, Left, Right));
            state.IsConnected = connected;
            return state;
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            return false;
        }
    }
}
