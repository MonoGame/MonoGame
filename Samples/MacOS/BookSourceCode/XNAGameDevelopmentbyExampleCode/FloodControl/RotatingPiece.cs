using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Flood_Control
{
    class RotatingPiece : GamePiece
    {
        public bool clockwise;

        public static float rotationRate = (MathHelper.PiOver2 / 10);
        private float rotationAmount = 0;
        public int rotationTicksRemaining = 10;

        public float RotationAmount
        {
            get
            {
                if (clockwise)
                    return rotationAmount;
                else
                    return (MathHelper.Pi * 2) - rotationAmount;
            }
        }

        public RotatingPiece(string pieceType, bool clockwise)
            : base(pieceType)
        {
            this.clockwise = clockwise;
        }

        public void UpdatePiece()
        {
            rotationAmount += rotationRate;
            rotationTicksRemaining = (int)MathHelper.Max(
                0,
                rotationTicksRemaining - 1);
        }

    }
}
