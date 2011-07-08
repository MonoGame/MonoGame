#region File Description
//-----------------------------------------------------------------------------
// NetworkBusyScreen.cs
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

#endregion

namespace NetworkStateManagement
{
	/// <summary>
	/// When an asynchronous network operation (for instance searching for or joining a
	/// session) is in progress, we want to display some sort of busy indicator to let
	/// the user know the game hasn't just locked up. We also want to make sure they
	/// can't pick some other menu option before the current operation has finished.
	/// This screen takes care of both requirements in a single stroke. It monitors
	/// the IAsyncResult returned by an asynchronous network call, displaying a busy
	/// indicator for as long as the call is still in progress. When it notices the
	/// IAsyncResult has completed, it raises an event to let the game know it should
	/// proceed to the next step, after which the busy screen automatically goes away.
	/// Because this screen is on top of all others for as long as the asynchronous
	/// operation is in progress, it automatically takes over all user input,
	/// preventing any other menu entries being selected until the operation completes.
	/// </summary>
	class NetworkBusyScreen : GameScreen
	{
	#region Fields

		IAsyncResult asyncResult;
		Texture2D gradientTexture;
		Texture2D catTexture;

	#endregion

	#region Events

		public event EventHandler<OperationCompletedEventArgs> OperationCompleted;

	#endregion

	#region Initialization


		/// <summary>
		/// Constructs a network busy screen for the specified asynchronous operation.
		/// </summary>
		public NetworkBusyScreen (IAsyncResult asyncResult)
			{
			this.asyncResult = asyncResult;

			IsPopup = true;

			TransitionOnTime = TimeSpan.FromSeconds (0.1);
			TransitionOffTime = TimeSpan.FromSeconds (0.2);
		}


		/// <summary>
		/// Loads graphics content for this screen. This uses the shared ContentManager
		/// provided by the Game class, so the content will remain loaded forever.
		/// Whenever a subsequent NetworkBusyScreen tries to load this same content,
		/// it will just get back another reference to the already loaded data.
		/// </summary>
		public override void LoadContent ()
		{
			ContentManager content = ScreenManager.Game.Content;

			gradientTexture = content.Load<Texture2D> ("gradient");
			catTexture = content.Load<Texture2D> ("cat");
		}


	#endregion

	#region Update and Draw


		/// <summary>
		/// Updates the NetworkBusyScreen.
		/// </summary>
		public override void Update (GameTime gameTime, bool otherScreenHasFocus, 
							bool coveredByOtherScreen)
		{
			base.Update (gameTime, otherScreenHasFocus, coveredByOtherScreen);

			// Has our asynchronous operation completed?
			if ((asyncResult != null) && asyncResult.IsCompleted) {
				// If so, raise the OperationCompleted event.
				if (OperationCompleted != null) {
					OperationCompleted (this, 
					new OperationCompletedEventArgs (asyncResult));
				}

				ExitScreen ();

				asyncResult = null;
			}
		}


		/// <summary>
		/// Draws the NetworkBusyScreen.
		/// </summary>
		public override void Draw (GameTime gameTime)
		{
			SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
			SpriteFont font = ScreenManager.Font;

			string message = Resources.NetworkBusy;

			const int hPad = 32 ; 
			const int vPad = 16 ; 

			// Center the message text in the viewport.
			Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
			Vector2 viewportSize = new Vector2 (viewport.Width, viewport.Height);
			Vector2 textSize = font.MeasureString (message);

			// Add enough room to spin a cat.
			Vector2 catSize = new Vector2 (catTexture.Width);

			textSize.X = Math.Max (textSize.X, catSize.X);
			textSize.Y += catSize.Y + vPad;

			Vector2 textPosition = (viewportSize - textSize) / 2;

			// The background includes a border somewhat larger than the text itself.
			Rectangle backgroundRectangle = new Rectangle ((int)textPosition.X - hPad,
							(int)textPosition.Y - vPad,
							(int)textSize.X + hPad * 2,
							(int)textSize.Y + vPad * 2);

			// Fade the popup alpha during transitions.
			Color color = Color.White * TransitionAlpha;

			spriteBatch.Begin ();

			// Draw the background rectangle.
			spriteBatch.Draw (gradientTexture, backgroundRectangle, color);

			// Draw the message box text.
			spriteBatch.DrawString (font, message, textPosition, color);

			// Draw the spinning cat progress indicator.
			float catRotation = (float)gameTime.TotalGameTime.TotalSeconds * 3;

			Vector2 catPosition = new Vector2 (textPosition.X + textSize.X / 2,
						textPosition.Y + textSize.Y - 
								catSize.Y / 2);

			spriteBatch.Draw (catTexture, catPosition, null, color, catRotation, 
				catSize / 2, 1, SpriteEffects.None, 0);

			spriteBatch.End ();
		}


	#endregion
	}
}
