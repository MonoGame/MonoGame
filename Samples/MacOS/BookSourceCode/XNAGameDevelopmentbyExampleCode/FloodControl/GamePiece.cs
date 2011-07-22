using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Flood_Control
{
    class GamePiece
    {
        public static string[] PieceTypes = 
        { 
            "Left,Right", 
            "Top,Bottom", 
            "Left,Top", 
            "Top,Right",
            "Right,Bottom", 
            "Bottom,Left",
            "Empty"
         };

        public const int PieceHeight = 40;
        public const int PieceWidth = 40;

        public const int MaxPlayablePieceIndex = 5;
        public const int EmptyPieceIndex = 6;

        private const int textureOffsetX = 1;
        private const int textureOffsetY = 1;
        private const int texturePaddingX = 1;
        private const int texturePaddingY = 1;

        private string pieceType = "";
        private string pieceSuffix = "";

        public string PieceType
        {
            get { return pieceType; }
        }

        public string Suffix
        {
            get { return pieceSuffix; }
        }

        public GamePiece(string type, string suffix)
        {
            pieceType = type;
            pieceSuffix = suffix;
        }

        public GamePiece(string type)
        {
            pieceType = type;
            pieceSuffix = "";
        }

        public void SetPiece(string type, string suffix)
        {
            pieceType = type;
            pieceSuffix = suffix;
        }

        public void SetPiece(string type)
        {
            SetPiece(type, "");
        }

        public void AddSuffix(string suffix)
        {
            if (!pieceSuffix.Contains(suffix))
                pieceSuffix += suffix;
        }

        public void RemoveSuffix(string suffix)
        {
            pieceSuffix = pieceSuffix.Replace(suffix, "");
        }

        public void RotatePiece(bool Clockwise)
        {
            switch (pieceType)
            {
                case "Left,Right":
                    pieceType = "Top,Bottom";
                    break;
                case "Top,Bottom":
                    pieceType = "Left,Right";
                    break;
                case "Left,Top":
                    if (Clockwise)
                        pieceType = "Top,Right";
                    else
                        pieceType = "Bottom,Left";
                    break;
                case "Top,Right":
                    if (Clockwise)
                        pieceType = "Right,Bottom";
                    else
                        pieceType = "Left,Top";
                    break;
                case "Right,Bottom":
                    if (Clockwise)
                        pieceType = "Bottom,Left";
                    else
                        pieceType = "Top,Right";
                    break;
                case "Bottom,Left":
                    if (Clockwise)
                        pieceType = "Left,Top";
                    else
                        pieceType = "Right,Bottom";
                    break;
                case "Empty":
                    break;
            }
        }

        public string[] GetOtherEnds(string startingEnd)
        {
            List<string> opposites = new List<string>();

            foreach (string end in pieceType.Split(','))
            {
                if (end != startingEnd)
                    opposites.Add(end);
            }
            return opposites.ToArray();
        }

        public bool HasConnector(string direction)
        {
            return pieceType.Contains(direction);
        }

        public Rectangle GetSourceRect()
        {
            int x = textureOffsetX;
            int y = textureOffsetY;

            if (pieceSuffix.Contains("W"))
                x += PieceWidth + texturePaddingX;

            y += (Array.IndexOf(PieceTypes, pieceType) *
                 (PieceHeight + texturePaddingY));


            return new Rectangle(x, y, PieceWidth, PieceHeight);
        }

    }
}
