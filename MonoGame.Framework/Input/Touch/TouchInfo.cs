using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoGame.Framework.Touch
{
    public struct TouchInfo
    {
        private const int _maxPosHistorySize = 10;

        internal TouchInfo(int id, Vector2 startingPos)
        {
            _touchLocId = id;
            _startingPosition = startingPos;
            _timeTouchStarted = DateTime.Now;
            _totalDistanceMoved = 0;

            _prevPositions = new List<Tuple<Vector2, DateTime>>();
        }

        private int _touchLocId;
        internal int TouchLocationID { get { return _touchLocId; } }

        private Vector2 _startingPosition;
        internal Vector2 StartingPosition { get { return _startingPosition; } }

        private float _totalDistanceMoved;
        internal float TotalDistanceMoved { get { return _totalDistanceMoved; } }

        private List<Tuple<Vector2, DateTime>> _prevPositions;
        public List<Tuple<Vector2, DateTime>> PreviousPositions { get { return _prevPositions; } }

        private DateTime _timeTouchStarted;
        internal TimeSpan Lifetime {  get { return DateTime.Now - _timeTouchStarted; } }

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

                _totalDistanceMoved += Vector2.Distance(positionToAdd, lastLoggedPosition);
            }

            PreviousPositions.Add(new Tuple<Vector2, DateTime>(positionToAdd, DateTime.Now));
        }
    }
}

