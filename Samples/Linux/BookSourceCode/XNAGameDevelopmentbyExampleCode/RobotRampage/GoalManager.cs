using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Robot_Rampage
{
    static class GoalManager
    {
        #region Declarations
        private static List<ComputerTerminal> computerTerminals =
            new List<ComputerTerminal>();
        private static int activeCount = 0;

        private static int minDistanceFromPlayer = 250;

        private static Random rand = new Random();

        private static Texture2D texture;
        private static Rectangle initialActiveFrame;
        private static Rectangle initialDisabledFrame;
        private static int activeFrameCount;
        private static int disabledFrameCount;
        #endregion

        #region Properties
        public static int ActiveTerminals
        {
            get { return activeCount; }
        }
        #endregion

        #region Initialization
        public static void Initialize(
            Texture2D textureSheet,
            Rectangle initialActiveRectangle,
            Rectangle initialDisabledRectangle,
            int activeFrames,
            int disabledFrames)
        {
            texture = textureSheet;
            initialActiveFrame = initialActiveRectangle;
            initialDisabledFrame = initialDisabledRectangle;
            activeFrameCount = activeFrames;
            disabledFrameCount = disabledFrames;
        }
        #endregion

        #region Terminal Management
        public static ComputerTerminal TerminalInSquare(
            Vector2 mapLocation)
        {
            foreach (ComputerTerminal terminal in computerTerminals)
            {
                if (terminal.MapLocation == mapLocation)
                {
                    return terminal;
                }
            }

            return null;
        }

        public static void DetectShutdowns()
        {
            foreach (ComputerTerminal terminal in computerTerminals)
            {
                if (terminal.Active)
                {
                    if (terminal.IsCircleColliding(
                        Player.BaseSprite.WorldCenter,
                        Player.BaseSprite.CollisionRadius))
                    {
                        terminal.Deactivate();
                        activeCount--;
                        GameManager.Score += 100;
                    }
                }
            }
        }

        public static void AddComputerTerminal()
        {
            int startX = rand.Next(2, TileMap.MapWidth - 2);
            int startY = rand.Next(0, TileMap.MapHeight - 2);

            Vector2 tileLocation = new Vector2(startX, startY);

            if ((TerminalInSquare(tileLocation) != null) ||
                (TileMap.IsWallTile(tileLocation)))
            {
                return;
            }

            if (Vector2.Distance(
                TileMap.GetSquareCenter(startX, startY),
                Player.BaseSprite.WorldCenter) < minDistanceFromPlayer)
            {
                return;
            }

            List<Vector2> path =
                PathFinder.FindPath(
                    new Vector2(startX, startY),
                    TileMap.GetSquareAtPixel(
                        Player.BaseSprite.WorldCenter));

            if (path != null)
            {
                Rectangle squareRect =
                    TileMap.SquareWorldRectangle(startX, startY);

                Sprite activeSprite = new Sprite(
                    new Vector2(squareRect.X, squareRect.Y),
                    texture,
                    initialActiveFrame,
                    Vector2.Zero);

                for (int x = 1; x < 3; x++)
                {
                    activeSprite.AddFrame(
                        new Rectangle(
                        initialActiveFrame.X + (x *
                            initialActiveFrame.Width),
                        initialActiveFrame.Y,
                        initialActiveFrame.Width,
                        initialActiveFrame.Height));
                }
                activeSprite.CollisionRadius = 15;

                Sprite disabledSprite = new Sprite(
                    new Vector2(squareRect.X, squareRect.Y),
                        texture,
                        initialDisabledFrame,
                        Vector2.Zero);

                ComputerTerminal terminal = new ComputerTerminal(
                        activeSprite,
                        disabledSprite,
                        new Vector2(startX, startY));

                float timerOffset = (float)rand.Next(1, 100);
                terminal.LastSpawnCounter = timerOffset / 100f;

                computerTerminals.Add(terminal);

                activeCount++;
            }
        }
        #endregion

        #region Public Methods
        public static void GenerateComputers(int computerCount)
        {
            computerTerminals.Clear();
            activeCount = 0;

            while (activeCount < computerCount)
            {
                AddComputerTerminal();
            }
        }

        public static void Update(GameTime gameTime)
        {
            DetectShutdowns();
            foreach (ComputerTerminal terminal in
                computerTerminals)
            {
                terminal.Update(gameTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (ComputerTerminal terminal in
                computerTerminals)
            {
                terminal.Draw(spriteBatch);
            }
        }
        #endregion

    }
}
