    // MonoGame - Copyright (C) The MonoGame Team
    // This file is subject to the terms and conditions defined in
    // file 'LICENSE.txt', which is part of this source code package.
	
	using Microsoft.Xna.Framework;
	using System;
	using Microsoft.Xna.Framework.Graphics;
	using MonoTouch.UIKit;
	using System.Collections.Generic;
	using MonoTouch.AudioToolbox;
	
	﻿namespace Microsoft.Xna.Framework.Input
	{
	    internal class IOSGamePad
	    {
			private float _thumbStickRadius = 20*20;
            internal bool _visible;
            internal List<ButtonDefinition> _buttonsDefinitions;
            private ThumbStickDefinition _leftThumbDefinition, _rightThumbDefinition;
			private Color _alphaColor = Color.Gray;
            internal int _buttons;
            internal Vector2 _leftStick, _rightStick;
            public Dictionary<IntPtr, object> touchState = new Dictionary<IntPtr, object>();

            public IOSGamePad()
			{
				_visible = true;
				_buttonsDefinitions = new List<ButtonDefinition>();
				
				// Set the transparency Level
				_alphaColor.A = 100;
		
				Reset();
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

            public void TouchesBegan(MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e, iOSGameView view)
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

                    if (_visible) 
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
                        if (_visible && (_leftThumbDefinition != null) && (CheckThumbStickHit(_leftThumbDefinition, location)))
						{
							_leftThumbDefinition.InitialHit = location;	
							UpdateTouch(touch,_leftThumbDefinition);
						}
						else 
						{
							// check the right thumbstick
                            if (_visible && (_rightThumbDefinition != null) && (CheckThumbStickHit(_rightThumbDefinition, location)))
							{
								_rightThumbDefinition.InitialHit = location;
								UpdateTouch(touch,_rightThumbDefinition);
							}
							else // Handle mouse 
							{
                                Mouse.PrimaryWindow.MouseState.X = (int)location.X;
                                Mouse.PrimaryWindow.MouseState.Y = (int)location.Y;
							}
						}
						
					}
				}
			}

            public void TouchesCancelled(MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e)
			{
				// do nothing
			}

            public void TouchesMoved(MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e, iOSGameView view)
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
                    if (_visible)
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
                            Vector2 movement = location - _leftThumbDefinition.InitialHit;
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
                        else if (_visible && (_leftThumbDefinition != null) && (CheckThumbStickHit(_leftThumbDefinition, location)))
						{
                            Vector2 movement = location - _leftThumbDefinition.InitialHit;
								
							UpdateTouch(touch,_leftThumbDefinition);
                            _leftThumbDefinition.InitialHit = location;
							
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
                            else if (_visible && (_rightThumbDefinition != null) && (CheckThumbStickHit(_rightThumbDefinition, location)))
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
                                Mouse.PrimaryWindow.MouseState.X = (int)location.X;
                                Mouse.PrimaryWindow.MouseState.Y = (int)location.Y;
							}
						}
					}
				}	
			}

            public void TouchesEnded(MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent e, iOSGameView view)
			{						
				UITouch []touchesArray = touches.ToArray<UITouch>();
				foreach(UITouch touch in touchesArray)
				{
					var point = touch.LocationInView(touch.View);
					Vector2 location = new Vector2(point.X, point.Y);
					location = view.GetOffsetPosition(location, true);

					// Check where is the touch
                    if (_visible)
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
                            _leftThumbDefinition.Offset = Vector2.Zero;
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

            public object GetTouchesObject(UITouch touch)
			{
				if(touchState.ContainsKey(touch.Handle))
					return touchState[touch.Handle];			
				return null;	
			}

            public void UpdateTouch(UITouch touch, object theObject)
			{
				if(touchState.ContainsKey(touch.Handle))
					touchState[touch.Handle] = theObject;
				else
					touchState.Add(touch.Handle,theObject);
			}

			public void RemoveTouch(UITouch touch)
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
								
			public void Render(GameTime gameTime, SpriteBatch batch)
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


        static partial class GamePad
        {
            private static IOSGamePad _instance;

            internal static IOSGamePad Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new IOSGamePad();
                    }
                    return _instance;
                }
            }

            public static bool Visible
            {
                get
                {
                    return Instance._visible;
                }
                set
                {
                    Instance.Reset();
                    Instance._visible = value;
                }
            }

            private static GamePadCapabilities PlatformGetCapabilities(int index)
            {
                GamePadCapabilities capabilities = new GamePadCapabilities();
                capabilities.IsConnected = (index == 0);
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
                return new GamePadState(
                        new GamePadThumbSticks(Instance._leftStick, Instance._rightStick),
                                    new GamePadTriggers(0f, 0f),
                        new GamePadButtons((Buttons)Instance._buttons),
                        new GamePadDPad(0, 0, 0, 0)
                    );
            }

            private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
            {
                SystemSound.Vibrate.PlaySystemSound();
                return true;
            }

            #region render virtual gamepad

            public static List<ButtonDefinition> ButtonsDefinitions
            {
                get
                {
                    return Instance._buttonsDefinitions;
                }
            }

            public static void Draw(GameTime gameTime, SpriteBatch batch)
            {
                Instance.Render(gameTime, batch);
            }

            #endregion
        }
	}