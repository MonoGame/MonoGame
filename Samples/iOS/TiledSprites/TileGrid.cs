#region File Description
//-----------------------------------------------------------------------------
// TileGrid.cs
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
	/// <summary>
	/// EDUCATIONAL: Class used to align tiles to a regular grid.
	/// This represents a tiling "layer" in this sample
	/// </summary>
	public class TileGrid
	{
	#region Fields
		private int[][] grid;
		private GraphicsDeviceManager graphics;
		private SpriteSheet sheet;
		private int width;
		private int height;
		private int cellWidth;
		private int cellHeight;
		private Vector2 worldOffset;
		private bool visibilityChanged;
		private Rectangle visibleTiles;

		//drawing parameters
		private Vector2 cameraPostionValue;
		private float zoomValue;
		private Vector2 scaleValue;
		private float rotationValue;
		private Matrix rotationMatrix;
		private Vector2 displaySize;
		private Color layerColor = Color.White;

	#endregion

	#region Initialization
		public TileGrid (int tileWidth,int tileHeight,int numXTiles,int numYTiles,
		Vector2 offset,SpriteSheet tileSheet,
		GraphicsDeviceManager graphicsComponent)
			{
			if (graphicsComponent == null) {
				throw new ArgumentNullException ("graphicsComponent");
			}
			graphics = graphicsComponent;
			sheet = tileSheet;
			width = numXTiles;
			height = numYTiles;
			cellWidth = tileWidth;
			cellHeight = tileHeight;
			worldOffset = offset;

			visibleTiles = new Rectangle (0, 0, width, height);

			grid = new int[width][];
			for (int i = 0; i < width; i++) {
				grid [i] = new int[height];
				for (int j = 0; j < height; j++) {
					grid [i] [j] = 0;
				}
			}

			scaleValue = Vector2.One;
			zoomValue = 1.0f;
			CameraPosition = Vector2.Zero;

			graphicsComponent.DeviceReset += 
		new EventHandler (OnGraphicsComponentDeviceReset);

			OnGraphicsComponentDeviceReset (this, new EventArgs ());
		}

		void OnGraphicsComponentDeviceReset (object sender, EventArgs e)
		{
			displaySize.X = 320;
			// TODO graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;

			displaySize.Y = 480;
			// TODO graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;

			visibilityChanged = true;
		}

	#endregion

	#region Public Accessors

		public Vector2 CameraPosition {
			set {
				cameraPostionValue = value;
				visibilityChanged = true;
			}
			get {
				return cameraPostionValue;
			}
		}

		public float CameraRotation {
			set {
				rotationValue = value;
				rotationMatrix = Matrix.CreateRotationZ (rotationValue);
				visibilityChanged = true;
			}
			get {
				return rotationValue;
			}
		}

		public float CameraZoom {
			set {               
				zoomValue = value;
				visibilityChanged = true;
			}
			get {
				return zoomValue;
			}
		}

		public Color Color {
			set {
				layerColor = value;                
			}
			get {
				return layerColor ;
			}
		}

		public Vector2 TileScale {
			set {
				scaleValue = value;
				visibilityChanged = true;
			}
			get {
				return scaleValue;
			}
		}

		public Vector2 Position {
			set {
				worldOffset = value;
				visibilityChanged = true;
			}
			get {
				return worldOffset;
			}
		}

	#endregion

	#region Methods
		public void SetTile (int xIndex, int yIndex, int tile)
		{
			grid [xIndex] [yIndex] = tile;
		}


		/// <summary>
		/// This function determines which tiles are visible on the screen,
		/// given the current camera position, rotation, zoom, and tile scale
		/// </summary>
		private void DetermineVisibility ()
		{
			//create the view rectangle
			Vector2 upperLeft = Vector2.Zero;
			Vector2 upperRight = Vector2.Zero;
			Vector2 lowerLeft = Vector2.Zero;
			Vector2 lowerRight = Vector2.Zero;
			lowerRight.X = ((displaySize.X / 2) / zoomValue);
			lowerRight.Y = ((displaySize.Y / 2) / zoomValue);
			upperRight.X = lowerRight.X;
			upperRight.Y = -lowerRight.Y;
			lowerLeft.X = -lowerRight.X;
			lowerLeft.Y = lowerRight.Y;
			upperLeft.X = -lowerRight.X;
			upperLeft.Y = -lowerRight.Y;


			//rotate the view rectangle appropriately
			Vector2.Transform (ref upperLeft, ref rotationMatrix, out upperLeft);
			Vector2.Transform (ref lowerRight, ref rotationMatrix, out lowerRight);
			Vector2.Transform (ref upperRight, ref rotationMatrix, out upperRight);
			Vector2.Transform (ref lowerLeft, ref rotationMatrix, out lowerLeft);

			lowerLeft += (cameraPostionValue);
			lowerRight += (cameraPostionValue);
			upperRight += (cameraPostionValue);
			upperLeft += (cameraPostionValue);



			//the idea here is to figure out the smallest square
			//(in tile space) that contains tiles
			//the offset is calculated before scaling
			float top = MathHelper.Min (
		MathHelper.Min (upperLeft.Y, lowerRight.Y), 
		MathHelper.Min (upperRight.Y, lowerLeft.Y)) - 
		worldOffset.Y;

			float bottom = MathHelper.Max (
		MathHelper.Max (upperLeft.Y, lowerRight.Y), 
		MathHelper.Max (upperRight.Y, lowerLeft.Y)) - 
		worldOffset.Y;
			float right = MathHelper.Max (
		MathHelper.Max (upperLeft.X, lowerRight.X), 
		MathHelper.Max (upperRight.X, lowerLeft.X)) - 
		worldOffset.X;
			float left = MathHelper.Min (
		MathHelper.Min (upperLeft.X, lowerRight.X), 
		MathHelper.Min (upperRight.X, lowerLeft.X)) - 
		worldOffset.X;


			//now figure out where we are in the tile sheet
			float scaledTileWidth = (float)cellWidth * scaleValue.X;
			float scaledTileHeight = (float)cellHeight * scaleValue.Y;

			//get the visible tiles
			visibleTiles.X = (int)(left / (scaledTileWidth));
			visibleTiles.Y = (int)(top / (scaledTileWidth));

			//get the number of visible tiles
			visibleTiles.Height = 
		(int)((bottom) / (scaledTileHeight)) - visibleTiles.Y + 1;
			visibleTiles.Width = 
		(int)((right) / (scaledTileWidth)) - visibleTiles.X + 1;

			//clamp the "upper left" values to 0
			if (visibleTiles.X < 0)
				visibleTiles.X = 0;
			if (visibleTiles.X > (width - 1))
				visibleTiles.X = width;
			if (visibleTiles.Y < 0)
				visibleTiles.Y = 0;
			if (visibleTiles.Y > (height - 1))
				visibleTiles.Y = height;


			//clamp the "lower right" values to the gameboard size
			if (visibleTiles.Right > (width - 1))
				visibleTiles.Width = (width - visibleTiles.X);

			if (visibleTiles.Right < 0)
				visibleTiles.Width = 0;

			if (visibleTiles.Bottom > (height - 1))
				visibleTiles.Height = (height - visibleTiles.Y);

			if (visibleTiles.Bottom < 0)
				visibleTiles.Height = 0;

			visibilityChanged = false;
		}

		public void Draw (SpriteBatch batch)
		{
			if (visibilityChanged)
				DetermineVisibility ();


			float scaledTileWidth = (float)cellWidth * scaleValue.X;
			float scaledTileHeight = (float)cellHeight * scaleValue.Y;
			Vector2 screenCenter = new Vector2 (
		(displaySize.X / 2),
		(displaySize.Y / 2));

			//begin a batch of sprites to be drawn all at once
			batch.Begin (SpriteSortMode.Deferred, BlendState.AlphaBlend);

			Rectangle sourceRect = new Rectangle ();
			Vector2 scale = Vector2.One;

			for (int x = visibleTiles.Left; x < visibleTiles.Right; x++) {
				for (int y = visibleTiles.Top; y < visibleTiles.Bottom; y++) {
					if (grid [x] [y] != 0) {
						//Get the tile's position from the grid
						//in this section we're using reference methods
						//for the high frequency math functions
						Vector2 position = Vector2.Zero;
						position.X = (float)x * scaledTileWidth;
						position.Y = (float)y * scaledTileHeight;


						//offset the positions by the word position of the tile grid
						//this is the actual position of the tile in world coordinates
						Vector2.Add (ref position, ref worldOffset, out position);


						//Now, we get the camera position relative to the tile's position
						Vector2.Subtract (ref cameraPostionValue, ref position, 
				out position);


						//get the tile's final size (note that scaling is done after 
						//determining the position)
						Vector2.Multiply (ref scaleValue, zoomValue, out scale);

						//get the source rectnagle that defines the tile
						sheet.GetRectangle (ref grid [x] [y], out sourceRect);

						//Draw the tile.  Notice that position is used as the offset and
						//the screen center is used as a position.  This is required to
						//enable scaling and rotation about the center of the screen by
						//drawing tiles as an offset from the center coordinate
						batch.Draw (sheet.Texture, screenCenter, sourceRect, layerColor, 
				rotationValue, position, scale, SpriteEffects.None, 0.0f);
					}
				}
			}
			batch.End ();
		}
	#endregion
	}
}

