#region File Description
//-----------------------------------------------------------------------------
// Marble.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Marblets
{
	/// <summary>
	/// Marble represents a single marble
	/// </summary>
	public class Marble
	{
		/// <summary>
		/// Width of a marble in HD resolution screen coordinates
		/// </summary>
		public const int Width = 45;

		/// <summary>
		/// Height of a marble in HD resolution screen coordinates
		/// </summary>
		public const int Height = 45;

		/// <summary>
		/// How long it takes a marble to break
		/// </summary>
		public const double BreakTime = 0.5;

		/// <summary>
		/// Animation fields
		/// </summary>
		private const int breakFrameCount = 12;
		private static float pulseFactor;
		private double animationStart;
		private int frame;
		private Animation animation = Animation.None;

		/// <summary>
		/// Initialize the random number generator
		/// </summary>
		private static Random random = new Random();

		/// <summary>
		/// Marble textures
		/// </summary>
		private static Texture2D breakTexture;
		private static Texture2D glowRing1Texture;
		private static Texture2D glowRing2Texture;
		private static Texture2D marble;
		private Color color;

		/// <summary>
		/// Marble position
		/// </summary>
		private Vector2 position2D;




		/// <summary>
		/// Current state of the animation
		/// </summary>
		public Animation Animation
		{
			get
			{
				return animation;
			}
		}

		/// <summary>
		/// Color of the marble
		/// </summary>
		public Color Color
		{
			get
			{
				return color;
			}
		}

		/// <summary>
		/// Is this marble currently selected
		/// </summary>
		public bool Selected;

		/// <summary>
		/// The HD screen based coordinates of the marble
		/// </summary>
		public Vector2 Position
		{
			get
			{
				return position2D;
			}

			set
			{
				position2D = value;
			}
		}

		/// <summary>
		/// Constructs a new marble
		/// </summary>
		public Marble()
		{
			//Set a random color
			color = MarbletsGame.Settings.MarbleColors[
						random.Next(MarbletsGame.Settings.MarbleColors.GetLength(0))];
		}

		/// <summary>
		/// Initializes the marble
		/// </summary>
		/// <param name="game">The game object</param>
		public static void Initialize()
		{
		}

		/// <summary>
		/// Updates the animation for marbles
		/// </summary>
		/// <param name="time">Total game time</param>
		public void Update(GameTime time)
		{
			if(time != null)
			{
				if(Animation == Animation.Breaking)
				{
					if((time.TotalGameTime.TotalSeconds - animationStart) > BreakTime)
					{
						animation = Animation.Gone;
						frame = breakFrameCount;
					}
					else
					{
						frame = (int)((time.TotalGameTime.TotalSeconds - animationStart)
                                     / BreakTime * breakFrameCount);
					}
				}
			}
		}

		/// <summary>
		/// Updates the static parts of the class - only needs to be done once rather 
		/// than for 144 marbles each frame
		/// </summary>
		/// <param name="time">Total game time</param>
		public static void UpdateStatic(GameTime time)
		{
			if(time != null)
				pulseFactor = (float)Math.Sin(time.TotalGameTime.TotalSeconds * 6.0);
		}

		/// <summary>
		/// Draws the specified marble
		/// </summary>
		/// <param name="spriteBatch">The sprite batch to use</param>
		public void Draw(RelativeSpriteBatch spriteBatch)
		{
			Draw2DMarble(spriteBatch);
		}

		private void Draw2DMarble(RelativeSpriteBatch spriteBatch)
		{
			//Draw the 2d marble
			if(Animation == Animation.Breaking || Animation == Animation.Gone)
			{
				//Draw the correct frame.
				spriteBatch.Draw(breakTexture, Position, new Rectangle(frame * Width, 0,
								 Width, Height), Color);
			}
			else
			{
				spriteBatch.Draw(marble, Position, Color);

				//Highlight selected marbles
				if(Selected)
				{
					if(pulseFactor < 0.0)
					{
						//Make a new color with the correct transparency.
						Color fade = Color.White * -pulseFactor;

						//pulse the single ring
						spriteBatch.Draw(glowRing1Texture, Position, fade);
					}
					else
					{
						//pulse the double ring

						//Make a new color with the correct transparency.
						Color fade = Color.White * pulseFactor;

						spriteBatch.Draw(glowRing2Texture, Position, fade);
					}
				}
			}
		}

		/// <summary>
		/// Signal a marble to start breaking
		/// </summary>
		/// <param name="time">Start time for the animation</param>
		public void Break(GameTime time)
		{
			if(time != null)
			{
				animationStart = time.TotalGameTime.TotalSeconds;
				animation = Animation.Breaking;
			}
		}

		/// <summary>
		/// Load graphics content.
		/// </summary>
		public static void LoadContent()
		{
			breakTexture = 
                MarbletsGame.Content.Load<Texture2D>("Textures/marble_burst");
			glowRing1Texture =
                MarbletsGame.Content.Load<Texture2D>("Textures/marble_glow_1ring");
			glowRing2Texture =
                MarbletsGame.Content.Load<Texture2D>("Textures/marble_glow_2rings");
			marble = MarbletsGame.Content.Load<Texture2D>("Textures/marble");
		}
	}
}
