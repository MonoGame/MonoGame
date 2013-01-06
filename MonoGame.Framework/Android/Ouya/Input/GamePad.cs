#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright � 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using Android.Views;
using System;


namespace Microsoft.Xna.Framework.Input
{
	public class GamePad
	{
		private readonly InputDevice _device;
		private readonly int _deviceId;

		private Buttons _buttons;
		private float _leftTrigger, _rightTrigger;
		private Vector2 _leftStick, _rightStick;

		private readonly GamePadCapabilities _capabilities;

		private static readonly GamePad[] GamePads = new GamePad[4];

		protected GamePad(InputDevice device)
		{
			_device = device;
			_deviceId = device.Id;

			_capabilities = CapabilitiesOfDevice(device);
		}

		public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
		{
			var gamePad = GamePads[(int) playerIndex];
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

		public static GamePadState GetState(PlayerIndex playerIndex)
		{
			var gamePad = GamePads[(int) playerIndex];

			GamePadState state = GamePadState.InitializedState;

			if (gamePad != null)
			{
				state = new GamePadState(
					new GamePadThumbSticks(gamePad._leftStick, gamePad._rightStick), 
					new GamePadTriggers(gamePad._leftTrigger, gamePad._rightTrigger), 
					new GamePadButtons(gamePad._buttons), 
					new GamePadDPad(gamePad._buttons));
			}

			return state;
		}

		public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
		{
			var gamePad = GamePads[(int)playerIndex];
			if (gamePad == null)
				return false;

			var vibrator = gamePad._device.Vibrator;
			if (!vibrator.HasVibrator)
				return false;
			vibrator.Vibrate(500);
			return true;
		}

		internal static GamePad GetGamePad(InputDevice device)
		{
			if ((device.Sources & InputSourceType.Gamepad) != InputSourceType.Gamepad)
				return null;

			for (int i = 0; i < GamePads.Length; i++)
			{
				if (GamePads[i] == null) //Have looked at all the gamepads, must be a new one
				{
					Console.WriteLine("Found new controller [" + i + "] " + device.Name);
					GamePads[i] = new GamePad(device);
					return GamePads[i];
				}

				if (GamePads[i]._deviceId == device.Id)
				{
					return GamePads[i];
				}
			}

			return null;
		}

		internal static bool OnKeyDown(Keycode keyCode, KeyEvent e)
		{
			var gamePad = GetGamePad(e.Device);
			if (gamePad == null)
				return false;

			gamePad._buttons |= ButtonForKeyCode(keyCode);
			return true;
		}

		internal static bool OnKeyUp(Keycode keyCode, KeyEvent e)
		{
			var gamePad = GetGamePad(e.Device);
			if (gamePad == null)
				return false;

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

			gamePad._leftStick = new Vector2(e.GetAxisValue(Axis.X), e.GetAxisValue(Axis.Y));
			gamePad._rightStick = new Vector2(e.GetAxisValue(Axis.Z), e.GetAxisValue(Axis.Rz));
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

				case Keycode.ButtonStart:
					return Buttons.Start;
				case Keycode.Back:
					return Buttons.Back;
			}

			return 0;
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
				case "OUYA Game Controller":

					capabilities.HasAButton = true;
					capabilities.HasBButton = true;
					capabilities.HasXButton = true;
					capabilities.HasYButton = true;

					capabilities.HasLeftXThumbStick = true;
					capabilities.HasLeftYThumbStick = true;
					capabilities.HasRightXThumbStick = true;
					capabilities.HasRightYThumbStick = true;

					capabilities.HasLeftShoulderButton = true;
					capabilities.HasRightShoulderButton = true;
					capabilities.HasLeftTrigger = true;
					capabilities.HasRightTrigger = true;

					capabilities.HasDPadDownButton = true;
					capabilities.HasDPadLeftButton = true;
					capabilities.HasDPadRightButton = true;
					capabilities.HasDPadUpButton = true;
					break;

				case "Microsoft X-Box 360 pad":
					capabilities.HasAButton = true;
					capabilities.HasBButton = true;
					capabilities.HasXButton = true;
					capabilities.HasYButton = true;

					capabilities.HasLeftXThumbStick = true;
					capabilities.HasLeftYThumbStick = true;
					capabilities.HasRightXThumbStick = true;
					capabilities.HasRightYThumbStick = true;

					capabilities.HasLeftShoulderButton = true;
					capabilities.HasRightShoulderButton = true;
					capabilities.HasLeftTrigger = true;
					capabilities.HasRightTrigger = true;

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
