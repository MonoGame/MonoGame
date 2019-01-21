// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using GameController;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using System;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        internal static bool MenuPressed = false;

        private static int PlatformGetMaxNumberOfGamePads ()
        {
            return 4;
        }

        static bool IndexIsUsed (GCControllerPlayerIndex index)
        {
            foreach (var ctrl in GCController.Controllers)
                if (ctrl.PlayerIndex==(int)index) return true;

            return false;
        }

        static void AssingIndex(GCControllerPlayerIndex index) {
            if (IndexIsUsed(index))
                return;
            foreach (var controller in GCController.Controllers)
            {
                if (controller.PlayerIndex == (int)index)
                    break;
                if (controller.PlayerIndex == (int)GCControllerPlayerIndex.Unset)
                {
                    controller.PlayerIndex = (int)index;
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
                if (controller.PlayerIndex == (int)ind)
                    return GetCapabilities(controller);
            }
            return new GamePadCapabilities { IsConnected = false };
        }
               
        private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            var ind = (GCControllerPlayerIndex)index;


            var buttons = new List<Buttons>();
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

            foreach  (var controller in GCController.Controllers) {

                if (controller == null)
                    continue;

                if (controller.PlayerIndex != (int)ind)
                    continue;

                connected = true;
                if (MenuPressed)
                {
                    buttons.Add(Buttons.Back);
                    MenuPressed = false;
                }

                if (controller.ExtendedGamepad != null)
                {
                    if (controller.ExtendedGamepad.ButtonA.IsPressed == true && !buttons.Contains (Buttons.A))
                        buttons.Add(Buttons.A);
                    if (controller.ExtendedGamepad.ButtonB.IsPressed == true && !buttons.Contains (Buttons.B))
                        buttons.Add(Buttons.B);
                    if (controller.ExtendedGamepad.ButtonX.IsPressed == true && !buttons.Contains (Buttons.X))
                        buttons.Add(Buttons.X);
                    if (controller.ExtendedGamepad.ButtonY.IsPressed == true && !buttons.Contains (Buttons.Y))
                        buttons.Add(Buttons.Y);

                    if (controller.ExtendedGamepad.LeftShoulder.IsPressed == true && !buttons.Contains (Buttons.LeftShoulder))
                        buttons.Add (Buttons.LeftShoulder);
                    if (controller.ExtendedGamepad.RightShoulder.IsPressed == true && !buttons.Contains (Buttons.RightShoulder))
                        buttons.Add (Buttons.RightShoulder);

                    Up = controller.ExtendedGamepad.DPad.Up.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Down = controller.ExtendedGamepad.DPad.Down.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Left = controller.ExtendedGamepad.DPad.Left.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Right = controller.ExtendedGamepad.DPad.Right.IsPressed ? ButtonState.Pressed : ButtonState.Released;

                    leftThumbStickPosition.X = controller.ExtendedGamepad.LeftThumbstick.XAxis.Value;
                    leftThumbStickPosition.Y = controller.ExtendedGamepad.LeftThumbstick.YAxis.Value;

                    rightThumbStickPosition.X = controller.ExtendedGamepad.RightThumbstick.XAxis.Value;
                    rightThumbStickPosition.Y = controller.ExtendedGamepad.RightThumbstick.YAxis.Value;

                    leftTriggerValue = controller.ExtendedGamepad.LeftTrigger.Value;
                    rightTriggerValue = controller.ExtendedGamepad.RightTrigger.Value;
                }
                else if (controller.Gamepad != null)
                {
                    if (controller.Gamepad.ButtonA.IsPressed == true && !buttons.Contains (Buttons.A))
                        buttons.Add(Buttons.A);
                    if (controller.Gamepad.ButtonB.IsPressed == true && !buttons.Contains (Buttons.B))
                        buttons.Add(Buttons.B);
                    if (controller.Gamepad.ButtonX.IsPressed == true && !buttons.Contains (Buttons.X))
                        buttons.Add(Buttons.X);
                    if (controller.Gamepad.ButtonY.IsPressed == true && !buttons.Contains (Buttons.Y))
                        buttons.Add(Buttons.Y);
                    Up = controller.Gamepad.DPad.Up.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Down = controller.Gamepad.DPad.Down.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Left = controller.Gamepad.DPad.Left.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Right = controller.Gamepad.DPad.Right.IsPressed ? ButtonState.Pressed : ButtonState.Released;

                }
                else if (controller.MicroGamepad != null)
                {
                    if (controller.MicroGamepad.ButtonA.IsPressed == true && !buttons.Contains (Buttons.A))
                        buttons.Add(Buttons.A);
                    if (controller.MicroGamepad.ButtonX.IsPressed == true && !buttons.Contains (Buttons.X))
                        buttons.Add(Buttons.X);
                    Up = controller.MicroGamepad.Dpad.Up.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Down = controller.MicroGamepad.Dpad.Down.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Left = controller.MicroGamepad.Dpad.Left.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                    Right = controller.MicroGamepad.Dpad.Right.IsPressed ? ButtonState.Pressed : ButtonState.Released;
                }
            }
            var state = new GamePadState(
                new GamePadThumbSticks(leftThumbStickPosition, rightThumbStickPosition, leftDeadZoneMode, rightDeadZoneMode),
                new GamePadTriggers(leftTriggerValue, rightTriggerValue),
                new GamePadButtons(buttons.ToArray()),
                new GamePadDPad (Up, Down, Left, Right));
            state.IsConnected = connected;
            return state;
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            return false;
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
                capabilities.HasBackButton = true;
                capabilities.HasDPadUpButton = true;
                capabilities.HasDPadDownButton = true;
                capabilities.HasDPadLeftButton = true;
                capabilities.HasDPadRightButton = true;
                capabilities.HasLeftShoulderButton = true;
                capabilities.HasRightShoulderButton = true;
            }
            else if (controller.MicroGamepad != null)
            {
                capabilities.HasAButton = true;
                capabilities.HasXButton = true;
                capabilities.HasBackButton = true;
                capabilities.HasDPadUpButton = true;
                capabilities.HasDPadDownButton = true;
                capabilities.HasDPadLeftButton = true;
                capabilities.HasDPadRightButton = true;
            }
            return capabilities;
        }


    }
}
