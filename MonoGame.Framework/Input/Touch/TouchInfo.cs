using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGame.Framework.Touch
{
    public struct TouchInfo
    {
        private const int _maxPosHistorySize = 10;

        internal TouchInfo(int id, Vector2 startingPos)
        {
            _prevPositions = new List<Tuple<Vector2, DateTime>>();
        }

        private List<Tuple<Vector2, DateTime>> _prevPositions;
        public List<Tuple<Vector2, DateTime>> PreviousPositions { get { return _prevPositions; } }

        internal void LogPosition(Vector2 positionToAdd)
        {
            if (_prevPositions.Count >= _maxPosHistorySize)
            {
                var numToRemove = Math.Abs(_maxPosHistorySize - _prevPositions.Count);

                _prevPositions.RemoveRange(0, numToRemove + 1);
            }

            if (PreviousPositions.Count > 0)
            {
                // Update total distance moved
                var lastLoggedPosition = _prevPositions[_prevPositions.Count - 1].Item1;
            }

            PreviousPositions.Add(new Tuple<Vector2, DateTime>(positionToAdd, DateTime.Now));
        }
    }
}

