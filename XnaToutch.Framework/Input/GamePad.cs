#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

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

using XnaTouch.Framework;
using System;
using XnaTouch.Framework.Graphics;
using OpenTK.Graphics.ES11;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.AudioToolbox;

﻿namespace XnaTouch.Framework.Input
{
    public class GamePad
    {
		private static GamePad _instance;
		private float _transparency = 0.6f;
		private float _thumbStickRadius = 20*20;	
		private bool _visible;
		private List<ButtonDefinition> _buttonsDefinitions;
		private ThumbStickDefinition _leftThumbDefinition,_rightThumbDefinition;
		private bool _useAccelerometer = false;
		
		private int _buttons;
		private Vector2 _leftStick, _rightStick;
		
		protected GamePad()
		{
			_visible = true;
			_buttonsDefinitions = new List<ButtonDefinition>();
			Reset();
		}
		
		internal static GamePad Instance 
		{
			get 
			{
				if (_instance == null) 
				{
					_instance = new GamePad();
				}
				return _instance;
			}
		}
		
		public static bool UseAccelerometer {
			get 
			{
				return Instance._useAccelerometer;
			}
			set 
			{
				Instance._useAccelerometer = value;
				if (value)
				{
					UIAccelerometer.SharedAccelerometer.UpdateInterval = 1/30;
					UIAccelerometer.SharedAccelerometer.Acceleration += delegate(object sender, UIAccelerometerEventArgs e) {						
						Instance._leftStick = new Vector2((float)e.Acceleration.X,(float)e.Acceleration.Y);
					};
				}
				else 
				{
					UIAccelerometer.SharedAccelerometer.Delegate = null;
				}
			}
		}
		public void Reset()
		{
			_buttons = 0;
			_leftStick = Vector2.Zero;
			_rightStick = Vector2.Zero;
			
			// reset thumbsticks
			if (_leftThumbDefinition != null) 
			{
				_leftThumbDefinition.Offset = Vector2.Zero;
			}
			if (_rightThumbDefinition != null) 
			{
				_rightThumbDefinition.Offset = Vector2.Zero;
			}
		}
		
		public static bool Visible 
		{
			get 
			{
				return GamePad.Instance._visible;
			}
			set 
			{
				GamePad.Instance.Reset();
				GamePad.Instance._visible = value;
			}
		}
		
        public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
        {
            GamePadCapabilities capabilities = new GamePadCapabilities();
			capabilities.IsConnected = (playerIndex == PlayerIndex.One);
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
            if (playerIndex != PlayerIndex.One) 
			{
				throw new NotSupportedException("Only one player!");
			}
			
			return new GamePadState((Buttons)GamePad.Instance._buttons,GamePad.Instance._leftStick,GamePad.Instance._rightStick);
        }

        public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {		
			//SystemSound.PlaySystemSound ( SystemSound.Vibrate();
            return true;
        }
		
		public static ThumbStickDefinition LeftThumbStickDefinition
		{
			get 
			{
				return Instance._leftThumbDefinition;
			}
			set
			{
				Instance._leftThumbDefinition = value;
			}
		}
		
		public static ThumbStickDefinition RightThumbStickDefinition
		{
			get 
			{
				return Instance._rightThumbDefinition;
			}
			set
			{
				Instance._rightThumbDefinition = value;
			}
		}
		
		private bool CheckButtonHit(ButtonDefinition theButton, Vector2 location)
		{
			Rectangle buttonRect = new Rectangle((int) theButton.Position.X,(int)theButton.Position.Y,theButton.TextureRect.Width, theButton.TextureRect.Height);
			return  buttonRect.Contains(location); 
		}
		
		private bool CheckThumbStickHit(ThumbStickDefinition theStick, Vector2 location)
		{
			Vector2 stickPosition = theStick.Position + theStick.Offset;
			Rectangle thumbRect = new Rectangle((int) stickPosition.X,(int)stickPosition.Y,theStick.TextureRect.Width, theStick.TextureRect.Height);
			return  thumbRect.Contains(location); 
		}
		
		internal void TouchesBegan( MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e)
		{
			// Reset State		
			Reset();
			
			// Check where is the touch
			UITouch []touchesArray = touches.ToArray<UITouch>();
			foreach(UITouch touch in touchesArray)
			{
				Vector2 location = new Vector2(touch.LocationInView(touch.View));
				
				// Check where is the touch
				bool hitInButton = false;
				
				if (Visible) 
				{
					foreach(ButtonDefinition button in _buttonsDefinitions)
					{
						hitInButton |= UpdateButton (button, location);													
					}
				}
				if (!hitInButton)
				{
					// check the left thumbstick
					if (Visible &&  (_leftThumbDefinition != null) && (CheckThumbStickHit(_leftThumbDefinition,location)))
					{
						_leftThumbDefinition.InitialHit = location;						
					}
					else 
					{
						// check the right thumbstick
						if (Visible && (_rightThumbDefinition != null) && (CheckThumbStickHit(_rightThumbDefinition,location)))
						{
							_rightThumbDefinition.InitialHit = location;						
						}
						else // Handle mouse 
						{
							Mouse.SetPosition((int) location.X, (int) location.Y);
						}
					}
					
				}
			}
		}
		
		internal void TouchesCancelled( MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e)
		{
			// do nothing
		}
		
		internal void TouchesMoved( MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e)
		{
			UITouch []touchesArray = touches.ToArray<UITouch>();
			foreach(UITouch touch in touchesArray)
			{
				Vector2 location = new Vector2(touch.LocationInView(touch.View));
				// Check if touch any button
				bool hitInButton = false;
				if (Visible)
				{
					foreach(ButtonDefinition button in _buttonsDefinitions)
					{
						hitInButton |= UpdateButton (button, location);								
					}
				}
				
				if (!hitInButton) 
				{
					if (Visible && (_leftThumbDefinition != null) && (CheckThumbStickHit(_leftThumbDefinition,location)))
					{
						Vector2 movement = location - LeftThumbStickDefinition.InitialHit;
						
						// Keep the stick in the "hole" 
						float radius = (movement.X*movement.X) + (movement.Y*movement.Y);
										
						if (radius <= _thumbStickRadius) 
						{
							_leftThumbDefinition.Offset = movement;
							_leftStick = new Vector2(movement.X / 20,movement.Y / -20);
						}												
					}
					else 
					{
						// reset left thumbstick
						if (_leftThumbDefinition != null) 
						{
							_leftThumbDefinition.Offset = Vector2.Zero;
							_leftStick = Vector2.Zero;
						}
						
						if (Visible && (_rightThumbDefinition != null) && (CheckThumbStickHit(_rightThumbDefinition,location)))
						{
							Vector2 movement = location - _rightThumbDefinition.InitialHit;
							
							// Keep the stick in the "hole" 
							float radius = (movement.X*movement.X) + (movement.Y*movement.Y);
											
							if (radius <= _thumbStickRadius) 
							{
								_rightThumbDefinition.Offset = movement;
								_rightStick = new Vector2(movement.X / 20,movement.Y / -20);
							}												
						}
						else 
						{
							// reset right thumbstick
							if (_rightThumbDefinition != null) 
							{
								_rightThumbDefinition.Offset = Vector2.Zero;
								_rightStick = Vector2.Zero;
							}
							
							// Handle the mouse
							Mouse.SetPosition( (int) location.X, (int) location.Y);
						}
					}
				}
			}		
		}
		
		internal void TouchesEnded( MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e)
		{						
			UITouch []touchesArray = touches.ToArray<UITouch>();
			foreach(UITouch touch in touchesArray)
			{
				Vector2 location = new Vector2(touch.LocationInView(touch.View).X, touch.LocationInView(touch.View).Y);
				
				// Check where is the touch
				if (Visible)
				{
					foreach(ButtonDefinition button in _buttonsDefinitions)
					{
						if  (CheckButtonHit(button, location)) 
						{
							_buttons &= ~(int)button.Type;
						}
					}
					if ((_leftThumbDefinition != null) && (CheckThumbStickHit(_leftThumbDefinition,location)))
					{
						LeftThumbStickDefinition.Offset = Vector2.Zero;
						_leftStick = Vector2.Zero;
					}
					if ((_rightThumbDefinition != null) && (CheckThumbStickHit(_rightThumbDefinition,location)))
					{
						_rightThumbDefinition.Offset = Vector2.Zero;
						_rightStick = Vector2.Zero;
					}
				}
			}		
		}		
		
		private bool UpdateButton (ButtonDefinition button, Vector2 location)
		{
			bool hitInButton = CheckButtonHit (button, location);
			
			if (hitInButton) 
			{
				_buttons |= (int)button.Type;
			}
			return hitInButton;
		}
		 
		#region render virtual gamepad
		
		public static float Transparency
		{
			get
			{
				return Instance._transparency;
			}
			set
			{
				Instance._transparency = value;
			}
		}		

		public static List<ButtonDefinition> ButtonsDefinitions
		{
			get 
			{
				return Instance._buttonsDefinitions;
			}
		}
		
		public static void Draw(GameTime gameTime )
		{		
			Instance.Render(gameTime);		
		}
		
		internal void Render(GameTime gameTime)
		{
			GL.Enable(All.Blend);
			
			// render buttons
			foreach (ButtonDefinition button in _buttonsDefinitions)
			{
				RenderButton(button);
			}			
			
			// Render the thumbsticks
			if (_leftThumbDefinition != null)
			{
				RenderThumbStick(_leftThumbDefinition);
			}
			if (_rightThumbDefinition != null)
			{
				RenderThumbStick(_rightThumbDefinition);
			}
	
			GL.Disable(All.Blend);
		}
		
		private void RenderButton(ButtonDefinition theButton)
		{
			Vector2 position = theButton.Position;
			position.Y = (480 - position.Y)-theButton.TextureRect.Height;
			
			theButton.Texture.Image.FilterColor = Color.DarkGray.ToEAGLColor();
			theButton.Texture.Image.SetAlpha(_transparency);			
			GraphicsDevice.RenderSubImageAtPoint(theButton.Texture.Image, position,new Vector2(theButton.TextureRect.Left, theButton.TextureRect.Top),
			                                              theButton.TextureRect.Width, theButton.TextureRect.Height);
		}
		
		private void RenderThumbStick(ThumbStickDefinition theStick)
		{
			Vector2 position = theStick.Position + theStick.Offset;
			position.Y = (480 - position.Y)-theStick.TextureRect.Height;
			
			theStick.Texture.Image.FilterColor = Color.DarkGray.ToEAGLColor();
			theStick.Texture.Image.SetAlpha(_transparency);			
			GraphicsDevice.RenderSubImageAtPoint(theStick.Texture.Image, position,new Vector2(theStick.TextureRect.Left, theStick.TextureRect.Top),
			                                              theStick.TextureRect.Width, theStick.TextureRect.Height);
		}
		
		#endregion
	}
	
}
