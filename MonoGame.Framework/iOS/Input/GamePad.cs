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
	
	using Microsoft.Xna.Framework;
	using System;
	using Microsoft.Xna.Framework.Graphics;
	using MonoTouch.UIKit;
	using System.Collections.Generic;
	using MonoTouch.AudioToolbox;
	
	﻿namespace Microsoft.Xna.Framework.Input
	{
	    public class GamePad
	    {
			private static GamePad _instance;
			private float _thumbStickRadius = 20*20;	
			private bool _visible;
			private List<ButtonDefinition> _buttonsDefinitions;
			private ThumbStickDefinition _leftThumbDefinition,_rightThumbDefinition;
			private Color _alphaColor = Color.Gray;		
			private int _buttons;
			private Vector2 _leftStick, _rightStick;
			internal Dictionary<IntPtr,object> touchState = new Dictionary<IntPtr, object>();
			
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
				if(touchState.Count == 0)
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
			return new GamePadState(
					new GamePadThumbSticks(Instance._leftStick, Instance._rightStick), 
		                        new GamePadTriggers(0f, 0f), 
					new GamePadButtons((Buttons)Instance._buttons), 
					new GamePadDPad(0,0,0,0)
				);
	        }
	
	        public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
	        {					
				SystemSound.Vibrate.PlaySystemSound();
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
				return  buttonRect.Contains((int)location.X, (int)location.Y); 
			}
			
			private bool CheckThumbStickHit(ThumbStickDefinition theStick, Vector2 location)
			{
				Vector2 stickPosition = theStick.Position + theStick.Offset;
				Rectangle thumbRect = new Rectangle((int) stickPosition.X,(int)stickPosition.Y,theStick.TextureRect.Width, theStick.TextureRect.Height);
				return  thumbRect.Contains((int)location.X, (int)location.Y); 
			}
			
			internal void TouchesBegan( MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e, iOSGameView view)
			{
				// Reset State		
				//Reset();
				
				// Check where is the touch
				UITouch []touchesArray = touches.ToArray<UITouch>();
				foreach(UITouch touch in touchesArray)
				{
					var point = touch.LocationInView(touch.View);
					Vector2 location = new Vector2(point.X, point.Y);
					location = view.GetOffsetPosition(location, true);

					// Check where is the touch
					bool hitInButton = false;
					
					if (Visible) 
					{
						foreach(ButtonDefinition button in _buttonsDefinitions)
						{
							hitInButton |= UpdateButton (button, location);	
							UpdateTouch(touch,button);
						}
					}
					if (!hitInButton)
					{
						// check the left thumbstick
						if (Visible &&  (_leftThumbDefinition != null) && (CheckThumbStickHit(_leftThumbDefinition,location)))
						{
							_leftThumbDefinition.InitialHit = location;	
							UpdateTouch(touch,_leftThumbDefinition);
						}
						else 
						{
							// check the right thumbstick
							if (Visible && (_rightThumbDefinition != null) && (CheckThumbStickHit(_rightThumbDefinition,location)))
							{
								_rightThumbDefinition.InitialHit = location;
								UpdateTouch(touch,_rightThumbDefinition);
							}
							else // Handle mouse 
							{
								Mouse.State.X = (int) location.X;
								Mouse.State.Y = (int) location.Y;
							}
						}
						
					}
				}
			}
			
			internal void TouchesCancelled( MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e)
			{
				// do nothing
			}
			
			internal void TouchesMoved( MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e, iOSGameView view)
			{
				UITouch []touchesArray = touches.ToArray<UITouch>();
				foreach(UITouch touch in touchesArray)
				{
					var point = touch.LocationInView(touch.View);
					Vector2 location = new Vector2(point.X, point.Y);
					location = view.GetOffsetPosition(location, true);
					
					var oldItem = GetTouchesObject(touch);
					// Check if touch any button
					bool hitInButton = false;
					if (Visible)
					{
						if(oldItem != null && oldItem is ButtonDefinition)
						{
							hitInButton |= UpdateButton((ButtonDefinition)oldItem,location);
						}
						if(!hitInButton)
							foreach(ButtonDefinition button in _buttonsDefinitions)
							{
								hitInButton |= UpdateButton (button, location);
								if(hitInButton)
								{
									UpdateTouch(touch,button);
									continue;
								}
							}
					}
					
					if (!hitInButton) 
					{
						if(oldItem != null && oldItem == _leftThumbDefinition)
						{						
							Vector2 movement = location - LeftThumbStickDefinition.InitialHit;
							if(movement.X > 20)
								movement.X = 20;
							else if(movement.X < -20)
								movement.X = -20;
							
							if(movement.Y > 20)
								movement.Y = 20;
							else if(movement.Y < -20)
								movement.Y = -20;
							_leftThumbDefinition.Offset = movement;
							_leftStick = new Vector2(movement.X / 20,movement.Y / -20);
						}										
						else if (Visible && (_leftThumbDefinition != null) && (CheckThumbStickHit(_leftThumbDefinition,location)))
						{
							Vector2 movement = location - LeftThumbStickDefinition.InitialHit;
								
							UpdateTouch(touch,_leftThumbDefinition);
							LeftThumbStickDefinition.InitialHit = location;
							
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
							
											
							if(oldItem != null && oldItem == _rightThumbDefinition)
							{
								Vector2 movement = location - _rightThumbDefinition.InitialHit;
								_rightThumbDefinition.Offset = movement;
								_rightStick = new Vector2(movement.X / 20,movement.Y / -20);
							}										
							else if (Visible && (_rightThumbDefinition != null) && (CheckThumbStickHit(_rightThumbDefinition,location)))
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
								Mouse.State.X = (int) location.X;
								Mouse.State.Y = (int) location.Y;
							}
						}
					}
				}	
			}
			
			internal void TouchesEnded( MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e, iOSGameView view)
			{						
				UITouch []touchesArray = touches.ToArray<UITouch>();
				foreach(UITouch touch in touchesArray)
				{
					var point = touch.LocationInView(touch.View);
					Vector2 location = new Vector2(point.X, point.Y);
					location = view.GetOffsetPosition(location, true);

					// Check where is the touch
					if (Visible)
					{
						var oldItem = GetTouchesObject(touch);
						if(oldItem == null)
							continue;
						if(oldItem is ButtonDefinition)
						{
							ButtonDefinition button = (ButtonDefinition)oldItem;
							if  (CheckButtonHit(button, location)) 
							{
								_buttons &= ~(int)button.Type;
							}
						}
						else if(oldItem  == _leftThumbDefinition)
						{
							LeftThumbStickDefinition.Offset = Vector2.Zero;
							_leftStick = Vector2.Zero;
						}
						else if(oldItem == _rightThumbDefinition)
						{
							_rightThumbDefinition.Offset = Vector2.Zero;
							_rightStick = Vector2.Zero;
						}
						RemoveTouch(touch);
					}
				}	
				Reset();
				
			}	
			
			internal object GetTouchesObject(UITouch touch)
			{
				if(touchState.ContainsKey(touch.Handle))
					return touchState[touch.Handle];			
				return null;	
			}
			internal void UpdateTouch(UITouch touch, object theObject)
			{
				if(touchState.ContainsKey(touch.Handle))
					touchState[touch.Handle] = theObject;
				else
					touchState.Add(touch.Handle,theObject);
			}
			internal void RemoveTouch(UITouch touch)
			{
				if(touchState.ContainsKey(touch.Handle))
					touchState.Remove(touch.Handle);
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