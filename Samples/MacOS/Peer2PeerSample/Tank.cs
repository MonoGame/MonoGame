#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace PeerToPeer
{
	/// <summary>
	/// Each player controls a tank, which they can drive around the screen.
	/// This class implements the logic for moving and drawing the tank, and
	/// responds to input that is passed in from outside. The Tank class does
	/// not implement any networking functionality, however: that is all
	/// handled by the main game class.
	/// </summary>
	class Tank
	{
	#region Constants

		// Constants control how fast the tank moves and turns.
		const float TankTurnRate = 0.01f;
		const float TurretTurnRate = 0.03f;
		const float TankSpeed = 0.3f;
		const float TankFriction = 0.9f;

	#endregion

	#region Fields

		// The current position and rotation of the tank.
		public Vector2 Position;
		public Vector2 Velocity;
		public float TankRotation;
		public float TurretRotation;

		// Input controls can be read from keyboard, gamepad, or the network.
		public Vector2 TankInput;
		public Vector2 TurretInput;

		// Textures used to draw the tank.
		Texture2D tankTexture;
		Texture2D turretTexture;
		Vector2 screenSize;

	#endregion


		/// <summary>
		/// Constructs a new Tank instance.
		/// </summary>
		public Tank (int gamerIndex,ContentManager content, 
			int screenWidth,int screenHeight)
			{
			// Use the gamer index to compute a starting position, so each player
			// starts in a different place as opposed to all on top of each other.
			Position.X = screenWidth / 4 + (gamerIndex % 5) * screenWidth / 8;
			Position.Y = screenHeight / 4 + (gamerIndex / 5) * screenHeight / 5;

			TankRotation = -MathHelper.PiOver2;
			TurretRotation = -MathHelper.PiOver2;

			tankTexture = content.Load<Texture2D> ("Tank");
			turretTexture = content.Load<Texture2D> ("Turret");

			screenSize = new Vector2 (screenWidth, screenHeight);
		}


		/// <summary>
		/// Moves the tank in response to the current input settings.
		/// </summary>
		public void Update ()
		{
			// Gradually turn the tank and turret to face the requested direction.
			TankRotation = TurnToFace (TankRotation, TankInput, TankTurnRate);
			TurretRotation = TurnToFace (TurretRotation, TurretInput, TurretTurnRate);

			// How close the desired direction is the tank facing?
			Vector2 tankForward = new Vector2 ((float)Math.Cos (TankRotation),
						(float)Math.Sin (TankRotation));

			Vector2 targetForward = new Vector2 (TankInput.X, -TankInput.Y);

			float facingForward = Vector2.Dot (tankForward, targetForward);

			// If we have finished turning, also start moving forward.
			if (facingForward > 0)
				Velocity += tankForward * facingForward * facingForward * TankSpeed;

			// Update the position and velocity.
			Position += Velocity;
			Velocity *= TankFriction;

			// Clamp so the tank cannot drive off the edge of the screen.
			Position = Vector2.Clamp (Position, Vector2.Zero, screenSize);
		}


		/// <summary>
		/// Gradually rotates the tank to face the specified direction.
		/// </summary>
		static float TurnToFace (float rotation, Vector2 target, float turnRate)
		{
			if (target == Vector2.Zero)
				return rotation;

			float angle = (float)Math.Atan2 (-target.Y, target.X);

			float difference = rotation - angle;

			while (difference > MathHelper.Pi)
				difference -= MathHelper.TwoPi;

			while (difference < -MathHelper.Pi)
				difference += MathHelper.TwoPi;

			turnRate *= Math.Abs (difference);

			if (difference < 0)
				return rotation + Math.Min (turnRate, -difference);
			else
				return rotation - Math.Min (turnRate, difference);
		}


		/// <summary>
		/// Draws the tank and turret.
		/// </summary>
		public void Draw (SpriteBatch spriteBatch)
		{
			Vector2 origin = new Vector2 (tankTexture.Width / 2, tankTexture.Height / 2);

			spriteBatch.Draw (tankTexture, Position, null, Color.White, 
				TankRotation, origin, 1, SpriteEffects.None, 0);

			spriteBatch.Draw (turretTexture, Position, null, Color.White, 
				TurretRotation, origin, 1, SpriteEffects.None, 0);
		}
	}
}
