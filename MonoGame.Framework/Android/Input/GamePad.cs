#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

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

using Android.Content;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


﻿namespace Microsoft.Xna.Framework.Input
{
    public class GamePad
    {
		private static GamePad _instance;
		private float _thumbStickRadius = 20*20;	
		private bool _visible;
		private List<ButtonDefinition> _buttonsDefinitions;
		private ThumbStickDefinition _leftThumbDefinition,_rightThumbDefinition;
		private Color _alphaColor = Color.DarkGray;		
		private int _buttons;
		private Vector2 _leftStick, _rightStick;
		
		protected GamePad()
		{
			_visible = true;
			_buttonsDefinitions = new List<ButtonDefinition>();
			
			// Set the transparency Level
			_alphaColor.A = 100;
	
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

        internal void SetBack()
        {
            _buttons |= (int)Buttons.Back;
        }

        internal void Update(MotionEvent e)
        {
            Vector2 location = new Vector2(e.GetX(), e.GetY());
            // Check where is the touch
            bool hitInButton = false;

            if (e.Action == MotionEventActions.Down) {

                Reset();

                if (Visible) {
                    foreach (ButtonDefinition button in _buttonsDefinitions) {
                        hitInButton |= UpdateButton(button, location);
                    }

                    if (!hitInButton) {
                        
                        if (_leftThumbDefinition != null && (CheckThumbStickHit(_leftThumbDefinition, location))) {
                            _leftThumbDefinition.InitialHit = location;
                        }
                        else if (Visible && (_rightThumbDefinition != null) && (CheckThumbStickHit(_rightThumbDefinition, location))) {
                            _rightThumbDefinition.InitialHit = location;
                        }
                    }
                }
            } 
            else if (e.Action == MotionEventActions.Move) {

                if (Visible) {
                    foreach (ButtonDefinition button in _buttonsDefinitions) {
                        hitInButton |= UpdateButton(button, location);
                    }
                }

                if (!hitInButton) {
                    if (Visible && (_leftThumbDefinition != null) &&
                        (CheckThumbStickHit(_leftThumbDefinition, location))) {
                        Vector2 movement = location - LeftThumbStickDefinition.InitialHit;

                        // Keep the stick in the "hole" 
                        float radius = (movement.X*movement.X) + (movement.Y*movement.Y);

                        if (radius <= _thumbStickRadius) {
                            _leftThumbDefinition.Offset = movement;
                            _leftStick = new Vector2(movement.X/20, movement.Y/-20);
                        }
                    }
                    else {
                        // reset left thumbstick
                        if (_leftThumbDefinition != null) {
                            _leftThumbDefinition.Offset = Vector2.Zero;
                            _leftStick = Vector2.Zero;
                        }

                        if (Visible && (_rightThumbDefinition != null) &&
                            (CheckThumbStickHit(_rightThumbDefinition, location))) {
                            Vector2 movement = location - _rightThumbDefinition.InitialHit;

                            // Keep the stick in the "hole" 
                            float radius = (movement.X*movement.X) + (movement.Y*movement.Y);

                            if (radius <= _thumbStickRadius) {
                                _rightThumbDefinition.Offset = movement;
                                _rightStick = new Vector2(movement.X/20, movement.Y/-20);
                            }
                        }
                        else {
                            // reset right thumbstick
                            if (_rightThumbDefinition != null) {
                                _rightThumbDefinition.Offset = Vector2.Zero;
                                _rightStick = Vector2.Zero;
                            }
                        }
                    }
                }
            }
            else if (e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Cancel) {
                if (Visible) {
                    foreach (ButtonDefinition button in _buttonsDefinitions) {
                        if (CheckButtonHit(button, location)) {
                            _buttons &= ~(int) button.Type;
                        }
                    }
                    if ((_leftThumbDefinition != null) && (CheckThumbStickHit(_leftThumbDefinition, location))) {
                        LeftThumbStickDefinition.Offset = Vector2.Zero;
                        _leftStick = Vector2.Zero;
                    }
                    if ((_rightThumbDefinition != null) && (CheckThumbStickHit(_rightThumbDefinition, location))) {
                        _rightThumbDefinition.Offset = Vector2.Zero;
                        _rightStick = Vector2.Zero;
                    }
                }
            }
        }

        private bool CheckButtonHit(ButtonDefinition theButton, Vector2 location)
        {
            Rectangle buttonRect = new Rectangle((int)theButton.Position.X, (int)theButton.Position.Y, theButton.TextureRect.Width, theButton.TextureRect.Height);
            return buttonRect.Contains(location);
        }

        private bool CheckThumbStickHit(ThumbStickDefinition theStick, Vector2 location)
        {
            Vector2 stickPosition = theStick.Position + theStick.Offset;
            Rectangle thumbRect = new Rectangle((int)stickPosition.X, (int)stickPosition.Y, theStick.TextureRect.Width, theStick.TextureRect.Height);
            return thumbRect.Contains(location);
        }

        private bool UpdateButton(ButtonDefinition button, Vector2 location)
        {
            bool hitInButton = CheckButtonHit(button, location);

            if (hitInButton)
            {
                _buttons |= (int)button.Type;
            }
            return hitInButton;
        }

        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            var instance = GamePad.Instance;
            var state = new GamePadState(new GamePadThumbSticks(), new GamePadTriggers(), new GamePadButtons((Buttons)instance._buttons), new GamePadDPad());
            instance.Reset();
            return state;
        }

        public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {	
			try
			{
	            Vibrator vibrator = (Vibrator)Game.Activity.GetSystemService(Context.VibratorService);
				vibrator.Vibrate(500);
	            return true;
			}
			catch
			{
				return false;
			}
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
	
		 
		#region render virtual gamepad
		
		public static List<ButtonDefinition> ButtonsDefinitions
		{
			get 
			{
				return Instance._buttonsDefinitions;
			}
		}
		
		public static void Draw(GameTime gameTime, SpriteBatch batch )
		{		
			Instance.Render(gameTime,batch);		
		}
		
		internal void Render(GameTime gameTime, SpriteBatch batch)
		{
			// render buttons
			foreach (ButtonDefinition button in _buttonsDefinitions)
			{
				RenderButton(button, batch);
			}			
			
			// Render the thumbsticks
			if (_leftThumbDefinition != null)
			{
				RenderThumbStick(_leftThumbDefinition, batch);
			}
			if (_rightThumbDefinition != null)
			{
				RenderThumbStick(_rightThumbDefinition, batch);
			}
		}
		
		private void RenderButton(ButtonDefinition theButton, SpriteBatch batch)
		{
			if (batch == null)
			{
				throw new InvalidOperationException("SpriteBatch not set.");
			}
			batch.Draw(theButton.Texture,theButton.Position,theButton.TextureRect,_alphaColor);
		}
		
		private void RenderThumbStick(ThumbStickDefinition theStick, SpriteBatch batch)
		{
			if (batch == null)
			{
				throw new InvalidOperationException("SpriteBatch not set.");
			}
			batch.Draw(theStick.Texture,theStick.Position + theStick.Offset,theStick.TextureRect,_alphaColor);
		}
		
		#endregion
	}
	
}
