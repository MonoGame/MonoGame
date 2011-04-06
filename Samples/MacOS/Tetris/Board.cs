using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetris
{
	class Board : Microsoft.Xna.Framework.DrawableGameComponent
	{
		SpriteBatch sBatch;
		Texture2D textures;
		Rectangle[] rectangles;
		enum FieldState
		{
			Free,
			Static,
			Dynamic
		};
		FieldState[,] boardFields;
		Vector2[, ,] Figures;
		readonly Vector2 StartPositionForNewFigure = new Vector2 (3, 0);
		Vector2 PositionForDynamicFigure;
		Vector2[] DynamicFigure = new Vector2[BlocksCountInFigure];
		Random random = new Random ();
		int[,] BoardColor;
		const int height = 20;
		const int width = 10;
		const int BlocksCountInFigure = 4;
		int DynamicFigureNumber;
		int DynamicFigureModificationNumber;
		int DynamicFigureColor;
		bool BlockLine;
		bool showNewBlock;
		float movement;
		float speed;
		Queue<int> nextFigures = new Queue<int> ();
		Queue<int> nextFiguresModification = new Queue<int> ();

		public float Movement {
			set { movement = value; }
			get { return movement; }
		}

		public float Speed {
			set { speed = value; }
			get { return speed; }
		}

		public Board (Game game, ref Texture2D textures, Rectangle[] rectangles) 
		: base(game)		{
			sBatch = (SpriteBatch)Game.Services.GetService (typeof(SpriteBatch));

			// Load textures for blocks
			this.textures = textures;

			// Rectangles to draw figures
			this.rectangles = rectangles;

			// Create tetris board
			boardFields = new FieldState[width, height];
			BoardColor = new int[width, height];

		#region Creating figures
			// Figures[figure's number, figure's modification, figure's block number] = Vector2
			// At all figures is 7, every has 4 modifications (for cube all modifications the same)
			// and every figure consists from 4 blocks
			Figures = new Vector2[7, 4, 4];
			// O-figure
			for (int i = 0; i < 4; i++) {
				Figures [0, i, 0] = new Vector2 (1, 0);
				Figures [0, i, 1] = new Vector2 (2, 0);
				Figures [0, i, 2] = new Vector2 (1, 1);
				Figures [0, i, 3] = new Vector2 (2, 1);
			}
			// I-figures
			for (int i = 0; i < 4; i += 2) {
				Figures [1, i, 0] = new Vector2 (0, 0);
				Figures [1, i, 1] = new Vector2 (1, 0);
				Figures [1, i, 2] = new Vector2 (2, 0);
				Figures [1, i, 3] = new Vector2 (3, 0);
				Figures [1, i + 1, 0] = new Vector2 (1, 0);
				Figures [1, i + 1, 1] = new Vector2 (1, 1);
				Figures [1, i + 1, 2] = new Vector2 (1, 2);
				Figures [1, i + 1, 3] = new Vector2 (1, 3);
			}
			// J-figures
			Figures [2, 0, 0] = new Vector2 (0, 0);
			Figures [2, 0, 1] = new Vector2 (1, 0);
			Figures [2, 0, 2] = new Vector2 (2, 0);
			Figures [2, 0, 3] = new Vector2 (2, 1);
			Figures [2, 1, 0] = new Vector2 (2, 0);
			Figures [2, 1, 1] = new Vector2 (2, 1);
			Figures [2, 1, 2] = new Vector2 (1, 2);
			Figures [2, 1, 3] = new Vector2 (2, 2);
			Figures [2, 2, 0] = new Vector2 (0, 0);
			Figures [2, 2, 1] = new Vector2 (0, 1);
			Figures [2, 2, 2] = new Vector2 (1, 1);
			Figures [2, 2, 3] = new Vector2 (2, 1);
			Figures [2, 3, 0] = new Vector2 (1, 0);
			Figures [2, 3, 1] = new Vector2 (2, 0);
			Figures [2, 3, 2] = new Vector2 (1, 1);
			Figures [2, 3, 3] = new Vector2 (1, 2);
			// L-figures
			Figures [3, 0, 0] = new Vector2 (0, 0);
			Figures [3, 0, 1] = new Vector2 (1, 0);
			Figures [3, 0, 2] = new Vector2 (2, 0);
			Figures [3, 0, 3] = new Vector2 (0, 1);
			Figures [3, 1, 0] = new Vector2 (2, 0);
			Figures [3, 1, 1] = new Vector2 (2, 1);
			Figures [3, 1, 2] = new Vector2 (1, 0);
			Figures [3, 1, 3] = new Vector2 (2, 2);
			Figures [3, 2, 0] = new Vector2 (0, 1);
			Figures [3, 2, 1] = new Vector2 (1, 1);
			Figures [3, 2, 2] = new Vector2 (2, 1);
			Figures [3, 2, 3] = new Vector2 (2, 0);
			Figures [3, 3, 0] = new Vector2 (1, 0);
			Figures [3, 3, 1] = new Vector2 (2, 2);
			Figures [3, 3, 2] = new Vector2 (1, 1);
			Figures [3, 3, 3] = new Vector2 (1, 2);
			// S-figures
			for (int i = 0; i < 4; i += 2) {
				Figures [4, i, 0] = new Vector2 (0, 1);
				Figures [4, i, 1] = new Vector2 (1, 1);
				Figures [4, i, 2] = new Vector2 (1, 0);
				Figures [4, i, 3] = new Vector2 (2, 0);
				Figures [4, i + 1, 0] = new Vector2 (1, 0);
				Figures [4, i + 1, 1] = new Vector2 (1, 1);
				Figures [4, i + 1, 2] = new Vector2 (2, 1);
				Figures [4, i + 1, 3] = new Vector2 (2, 2);
			}
			// Z-figures
			for (int i = 0; i < 4; i += 2) {
				Figures [5, i, 0] = new Vector2 (0, 0);
				Figures [5, i, 1] = new Vector2 (1, 0);
				Figures [5, i, 2] = new Vector2 (1, 1);
				Figures [5, i, 3] = new Vector2 (2, 1);
				Figures [5, i + 1, 0] = new Vector2 (2, 0);
				Figures [5, i + 1, 1] = new Vector2 (1, 1);
				Figures [5, i + 1, 2] = new Vector2 (2, 1);
				Figures [5, i + 1, 3] = new Vector2 (1, 2);
			}
			// T-figures
			Figures [6, 0, 0] = new Vector2 (0, 1);
			Figures [6, 0, 1] = new Vector2 (1, 1);
			Figures [6, 0, 2] = new Vector2 (2, 1);
			Figures [6, 0, 3] = new Vector2 (1, 0);
			Figures [6, 1, 0] = new Vector2 (1, 0);
			Figures [6, 1, 1] = new Vector2 (1, 1);
			Figures [6, 1, 2] = new Vector2 (1, 2);
			Figures [6, 1, 3] = new Vector2 (2, 1);
			Figures [6, 2, 0] = new Vector2 (0, 0);
			Figures [6, 2, 1] = new Vector2 (1, 0);
			Figures [6, 2, 2] = new Vector2 (2, 0);
			Figures [6, 2, 3] = new Vector2 (1, 1);
			Figures [6, 3, 0] = new Vector2 (2, 0);
			Figures [6, 3, 1] = new Vector2 (2, 1);
			Figures [6, 3, 2] = new Vector2 (2, 2);
			Figures [6, 3, 3] = new Vector2 (1, 1);
		#endregion

			nextFigures.Enqueue (random.Next (7));
			nextFigures.Enqueue (random.Next (7));
			nextFigures.Enqueue (random.Next (7));
			nextFigures.Enqueue (random.Next (7));

			nextFiguresModification.Enqueue (random.Next (4));
			nextFiguresModification.Enqueue (random.Next (4));
			nextFiguresModification.Enqueue (random.Next (4));
			nextFiguresModification.Enqueue (random.Next (4));
		}

		public override void Initialize ()
		{
			showNewBlock = true;
			movement = 0;
			speed = 0.1f;

			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					ClearBoardField (i, j);

			base.Initialize ();
		}

		public void FindDynamicFigure ()
		{
			int BlockNumberInDynamicFigure = 0;
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					if (boardFields [i, j] == FieldState.Dynamic)
						DynamicFigure [BlockNumberInDynamicFigure++] = new Vector2 (i, j);
		}

		/// <summary>
		/// Find, destroy and save lines's count
		/// </summary>
		/// <returns>Number of destoyed lines</returns>
		public int DestroyLines ()
		{
			// Find total lines
			int BlockLineCount = 0;
			for (int j = 0; j < height; j++) {
				for (int i = 0; i < width; i++)
					if (boardFields [i, j] == FieldState.Static)
						BlockLine = true;
					else {
						BlockLine = false;
						break;
					}
				//Destroy total lines
				if (BlockLine) {
					// Save number of total lines
					BlockLineCount++;
					for (int l = j; l > 0; l--)
						for (int k = 0; k < width; k++) {
							boardFields [k, l] = boardFields [k, l - 1];
							BoardColor [k, l] = BoardColor [k, l - 1];
						}
					for (int l = 0; l < width; l++) {
						boardFields [l, 0] = FieldState.Free;
						BoardColor [l, 0] = -1;
					}
				}
			}
			return BlockLineCount;
		}

		/// <summary>
		/// Create new shape in the game, if need it
		/// </summary>
		public bool CreateNewFigure ()
		{
			if (showNewBlock) {
				// Generate new figure's shape
				DynamicFigureNumber = nextFigures.Dequeue ();
				nextFigures.Enqueue (random.Next (7));

				DynamicFigureModificationNumber = nextFiguresModification.Dequeue ();
				nextFiguresModification.Enqueue (random.Next (4));

				DynamicFigureColor = DynamicFigureNumber;

				// Position and coordinates for new dynamic figure
				PositionForDynamicFigure = StartPositionForNewFigure;
				for (int i = 0; i < BlocksCountInFigure; i++)
					DynamicFigure [i] = Figures [DynamicFigureNumber, DynamicFigureModificationNumber, i] + 
					PositionForDynamicFigure;

				if (!DrawFigureOnBoard (DynamicFigure, DynamicFigureColor))
					return false;

				showNewBlock = false;
			}
			return true;
		}

		bool DrawFigureOnBoard (Vector2[] vector, int color)
		{
			if (!TryPlaceFigureOnBoard (vector))
				return false;
			for (int i = 0; i <= vector.GetUpperBound(0); i++) {
				boardFields [(int)vector [i].X, (int)vector [i].Y] = FieldState.Dynamic;
				BoardColor [(int)vector [i].X, (int)vector [i].Y] = color;
			}
			return true;
		}

		bool TryPlaceFigureOnBoard (Vector2[] vector)
		{
			for (int i = 0; i <= vector.GetUpperBound(0); i++)
				if ((vector [i].X < 0) || (vector [i].X >= width) || 
			(vector [i].Y >= height))
					return false;
			for (int i = 0; i <= vector.GetUpperBound(0); i++)
				if (boardFields [(int)vector [i].X, (int)vector [i].Y] == FieldState.Static)
					return false;
			return true;
		}

		public void MoveFigureLeft ()
		{
			// Sorting blocks fro dynamic figure to correct moving
			SortingVector2 (ref DynamicFigure, true, DynamicFigure.GetLowerBound (0), DynamicFigure.GetUpperBound (0));
			// Check colisions
			for (int i = 0; i < BlocksCountInFigure; i++) {
				if ((DynamicFigure [i].X == 0))
					return;
				if (boardFields [(int)DynamicFigure [i].X - 1, (int)DynamicFigure [i].Y] == FieldState.Static)
					return;
			}
			// Move figure on board
			for (int i = 0; i < BlocksCountInFigure; i++) {
				boardFields [(int)DynamicFigure [i].X - 1, (int)DynamicFigure [i].Y] = 
			boardFields [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y];
				BoardColor [(int)DynamicFigure [i].X - 1, (int)DynamicFigure [i].Y] = 
			BoardColor [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y];
				ClearBoardField ((int)DynamicFigure [i].X, (int)DynamicFigure [i].Y);
				// Change position for blocks in DynamicFigure
				DynamicFigure [i].X = DynamicFigure [i].X - 1;
			}
			// Change position vector
			//if (PositionForDynamicFigure.X > 0)
			PositionForDynamicFigure.X--;
		}

		public void MoveFigureRight ()
		{
			// Sorting blocks fro dynamic figure to correct moving
			SortingVector2 (ref DynamicFigure, true, DynamicFigure.GetLowerBound (0), DynamicFigure.GetUpperBound (0));
			// Check colisions
			for (int i = 0; i < BlocksCountInFigure; i++) {
				if ((DynamicFigure [i].X == width - 1))
					return;
				if (boardFields [(int)DynamicFigure [i].X + 1, (int)DynamicFigure [i].Y] == FieldState.Static)
					return;
			}
			// Move figure on board
			for (int i = BlocksCountInFigure - 1; i >=0; i--) {
				boardFields [(int)DynamicFigure [i].X + 1, (int)DynamicFigure [i].Y] = 
			boardFields [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y];
				BoardColor [(int)DynamicFigure [i].X + 1, (int)DynamicFigure [i].Y] = 
			BoardColor [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y];
				ClearBoardField ((int)DynamicFigure [i].X, (int)DynamicFigure [i].Y);
				// Change position for blocks in DynamicFigure
				DynamicFigure [i].X = DynamicFigure [i].X + 1;
			}
			// Change position vector
			//if (PositionForDynamicFigure.X < width - 1)
			PositionForDynamicFigure.X++;
		}

		public void MoveFigureDown ()
		{
			// Sorting blocks fro dynamic figure to correct moving
			SortingVector2 (ref DynamicFigure, false, DynamicFigure.GetLowerBound (0), DynamicFigure.GetUpperBound (0));
			// Check colisions
			for (int i = 0; i < BlocksCountInFigure; i++) {
				if ((DynamicFigure [i].Y == height - 1)) {
					for (int k = 0; k < BlocksCountInFigure; k++)
						boardFields [(int)DynamicFigure [k].X, (int)DynamicFigure [k].Y] = FieldState.Static;
					showNewBlock = true;
					return;
				}
				if (boardFields [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y + 1] == FieldState.Static) {
					for (int k = 0; k < BlocksCountInFigure; k++)
						boardFields [(int)DynamicFigure [k].X, (int)DynamicFigure [k].Y] = FieldState.Static;
					showNewBlock = true;
					return;
				}
			}
			// Move figure on board
			for (int i = BlocksCountInFigure - 1; i >= 0; i--) {
				boardFields [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y + 1] = 
			boardFields [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y];
				BoardColor [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y + 1] = 
			BoardColor [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y];
				ClearBoardField ((int)DynamicFigure [i].X, (int)DynamicFigure [i].Y);
				// Change position for blocks in DynamicFigure
				DynamicFigure [i].Y = DynamicFigure [i].Y + 1;
			}
			// Change position vector
			//if (PositionForDynamicFigure.Y < height - 1)
			PositionForDynamicFigure.Y++;
		}

		public void RotateFigure ()
		{
			// Check colisions for next modification
			Vector2[] TestDynamicFigure = new Vector2[DynamicFigure.GetUpperBound (0) + 1];
			for (int i = 0; i < BlocksCountInFigure; i++)
				TestDynamicFigure [i] = Figures [DynamicFigureNumber, (DynamicFigureModificationNumber + 1) % 4, i] + PositionForDynamicFigure;

			// Make sure that figure can rotate if she stand near left and right borders
			SortingVector2 (ref TestDynamicFigure, true, TestDynamicFigure.GetLowerBound (0), TestDynamicFigure.GetUpperBound (0));
			int leftFigureBound;
			int rightFigureBound;
			if ((leftFigureBound = (int)TestDynamicFigure [0].X) < 0) {
				//int leftFigureBound = (int)TestDynamicFigure[0].X;
				for (int i = 0; i < BlocksCountInFigure; i++) {
					TestDynamicFigure [i] += new Vector2 (0 - leftFigureBound, 0);
				}
				if (TryPlaceFigureOnBoard (TestDynamicFigure))
					PositionForDynamicFigure += 
			new Vector2 (0 - leftFigureBound, 0);
			}
			if ((rightFigureBound = (int)TestDynamicFigure [BlocksCountInFigure - 1].X) >= width) {
				//int rightFigureBound = (int)TestDynamicFigure[BlocksCountInFigure - 1].X;
				for (int i = 0; i < BlocksCountInFigure; i++) {
					TestDynamicFigure [i] -= new Vector2 (rightFigureBound - width + 1, 0);
				}
				if (TryPlaceFigureOnBoard (TestDynamicFigure))
					PositionForDynamicFigure -= 
			new Vector2 (rightFigureBound - width + 1, 0);
			}

			if (TryPlaceFigureOnBoard (TestDynamicFigure)) {
				DynamicFigureModificationNumber = (DynamicFigureModificationNumber + 1) % 4;
				// Clear dynamic fields
				for (int i = 0; i <= DynamicFigure.GetUpperBound(0); i++)
					ClearBoardField ((int)DynamicFigure [i].X, (int)DynamicFigure [i].Y);
				DynamicFigure = TestDynamicFigure;
				for (int i = 0; i <= DynamicFigure.GetUpperBound(0); i++) {
					boardFields [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y] = FieldState.Dynamic;
					BoardColor [(int)DynamicFigure [i].X, (int)DynamicFigure [i].Y] = DynamicFigureColor;
				}
			}
		}

		public void SortingVector2 (ref Vector2[] vector, bool sortByX, int a, int b)
		{
			if (a >= b)
				return;
			int i = a;
			for (int j = a; j <= b; j++) {
				if (sortByX) {
					if (vector [j].X <= vector [b].X) {
						Vector2 tempVector = vector [i];
						vector [i] = vector [j];
						vector [j] = tempVector;
						i++;
					}
				} else {
					if (vector [j].Y <= vector [b].Y) {
						Vector2 tempVector = vector [i];
						vector [i] = vector [j];
						vector [j] = tempVector;
						i++;
					}
				}
			}
			int c = i - 1;
			SortingVector2 (ref vector, sortByX, a, c - 1);
			SortingVector2 (ref vector, sortByX, c + 1, b);
		}

		void ClearBoardField (int i, int j)
		{
			boardFields [i, j] = FieldState.Free;
			BoardColor [i, j] = -1;
		}

		public override void Draw (GameTime gameTime)
		{
			Vector2 startPosition;
			// Draw the blocks
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					if (boardFields [i, j] != FieldState.Free) {
						startPosition = new Vector2 ((10 + i) * rectangles [0].Width,
				(2 + j) * rectangles [0].Height);
						sBatch.Draw (textures, startPosition, rectangles [BoardColor [i, j]], Color.White);
					}

			// Draw next figures
			Queue<int>.Enumerator figure = nextFigures.GetEnumerator ();
			Queue<int>.Enumerator modification = nextFiguresModification.GetEnumerator ();
			for (int i = 0; i < nextFigures.Count; i++) {
				figure.MoveNext ();
				modification.MoveNext ();
				for (int j = 0; j < BlocksCountInFigure; j++) {
					startPosition = rectangles [0].Height * (new Vector2 (24, 3 + 5 * i) + 
			Figures [figure.Current, modification.Current, j]);
					sBatch.Draw (textures, startPosition, 
			rectangles [figure.Current], Color.White);
				}
			}

			base.Draw (gameTime);
		}
	}
}