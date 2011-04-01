#region File Description
//-----------------------------------------------------------------------------
// AnimatedSprite.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;

#if IPHONE
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
#else
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

#endif

#endregion

namespace TiledSprites
{
	public class AnimatedSprite
	{
	#region Fields
		private SpriteSheet sheet;
		private Vector2 positionValue;
		private Vector2 originValue;
		private Vector2 scaleValue;
		private float rotationValue;
		private int currentFrameValue;
		private int numFrames;

	#endregion

	#region Public Properties
		public Vector2 Position {
			set {
				positionValue = value;
			}
			get {
				return positionValue;
			}
		}

		public Vector2 Origin {
			set {
				originValue = value;
			}
			get {
				return originValue;
			}
		}

		public float Rotation {
			set {
				rotationValue = value;
			}
			get {
				return rotationValue;
			}
		}

		public Vector2 ScaleValue {
			set {
				scaleValue = value;
			}
			get {
				return scaleValue;
			}
		}

		public int CurrentFrame {
			set {
				if (value > (numFrames - 1)) {
					string message = 
			string.Format ("{0} is an invalid value for CurrentFrame.  " + 
			"Valid values are from 0 to numFrames - 1 ({1})", 
			value, numFrames - 1);
					throw new ArgumentOutOfRangeException ("value", message);
				}
				currentFrameValue = value;
			}
			get {
				return currentFrameValue;
			}

		}

	#endregion

	#region Contructors
		public AnimatedSprite (SpriteSheet spriteSheet, int frameWidth, 
		int frameHeight,int padding,int rows,int columns,
		Point startFrame,int frames)		{
			if (spriteSheet == null) {
				throw new ArgumentNullException ("spriteSheet");
			}
			int spriteAreaHeight = (frameHeight + padding) * rows - padding;
			int spriteAreaWidth = (frameWidth + padding) * columns - padding;

			//first, make sure the sheet is possible
			if ((spriteAreaWidth > spriteSheet.Texture.Width) || 
		(spriteAreaHeight > spriteSheet.Texture.Height)) {
				throw new ArgumentException (
			"The layout specified is too large for the SpriteSheet."
			);
			}

			sheet = spriteSheet;
			numFrames = frames;

			int startFrameIndex = startFrame.Y * columns + startFrame.X;


			//now auto-generate the animation data,
			//left to right, top to bottom.
			int frameIndex = 0;
			for (int i = startFrameIndex; i < (numFrames+startFrameIndex); i++) {
				int x = (i % columns);
				int y = (i / columns);
				int left = (x * (frameWidth + padding));
				int top = (y * (frameHeight + padding));

				top = top % spriteAreaHeight;

				sheet.AddSourceSprite (frameIndex, 
			new Rectangle (left, top, frameWidth, frameHeight));
				frameIndex++;
			}
		}

	#endregion

	#region Methods
		public void IncrementAnimationFrame ()
		{
			currentFrameValue = (currentFrameValue + 1) % numFrames;
		}

		public void Draw (SpriteBatch batch, Color color, BlendState blendMode)
		{
			batch.Begin (SpriteSortMode.Immediate, blendMode);
			batch.Draw (sheet.Texture, positionValue, sheet [currentFrameValue], 
		color, rotationValue, originValue, scaleValue, 
		SpriteEffects.None, 0f);
			batch.End ();
		}
	#endregion
	}
}
