// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using System;

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Represents data from a multi-touch gesture over a span of time.
    /// </summary>
    public struct GestureSample
    {
        // attributes
		private GestureType _gestureType;
		private TimeSpan _timestamp;
		private Vector2 _position;
		private Vector2 _position2;
		private Vector2 _delta;
		private Vector2 _delta2;
		
		#region Properties

        /// <summary>
        /// Gets the type of the gesture.
        /// </summary>
        public GestureType GestureType
        {
            get
            {
				return this._gestureType;
            }
        }

        /// <summary>
        /// Gets the starting time for this multi-touch gesture sample.
        /// </summary>
        public TimeSpan Timestamp
        {
            get
            {
				return this._timestamp;
            }
        }

        /// <summary>
        /// Gets the position of the first touch-point in the gesture sample.
        /// </summary>
        public Vector2 Position
        {
            get
            {
				return this._position;
            }
        }

        /// <summary>
        /// Gets the position of the second touch-point in the gesture sample.
        /// </summary>
        public Vector2 Position2
        {
            get
            {
				return this._position2;
            }
        }

        /// <summary>
        /// Gets the delta information for the first touch-point in the gesture sample.
        /// </summary>
        public Vector2 Delta
        {
            get
            {
				return this._delta;
            }
        }

        /// <summary>
        /// Gets the delta information for the second touch-point in the gesture sample.
        /// </summary>
        public Vector2 Delta2
        {
            get
            {
				return this._delta2;
            }
        }
		#endregion
		
        /// <summary>
        /// Initializes a new <see cref="GestureSample"/>.
        /// </summary>
        /// <param name="gestureType"><see cref="GestureType"/></param>
        /// <param name="timestamp"></param>
        /// <param name="position"></param>
        /// <param name="position2"></param>
        /// <param name="delta"></param>
        /// <param name="delta2"></param>
        public GestureSample(GestureType gestureType, TimeSpan timestamp, Vector2 position, Vector2 position2, Vector2 delta, Vector2 delta2)
        {
			this._gestureType = gestureType;
			this._timestamp = timestamp;
			this._position = position;
			this._position2 = position2;
			this._delta = delta;
			this._delta2 = delta2;
        }
    }
}

