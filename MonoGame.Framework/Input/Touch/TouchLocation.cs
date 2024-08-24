// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#region Using clause
using System;
using System.Diagnostics;
#endregion Using clause


namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Provides methods and properties for interacting with a touch location on a touch screen device.
    /// </summary>
    public struct TouchLocation : IEquatable<TouchLocation>
    {
		/// <summary>
		///Attributes 
		/// </summary>
		private int _id;
		private Vector2 _position;
		private Vector2 _previousPosition;
		private TouchLocationState _state;
		private TouchLocationState _previousState;

		// Only used in Android, for now
		private float _pressure;
		private float _previousPressure;

        // Used for gesture recognition.
        private Vector2 _velocity;
        // Use for high-frequency (optional) touch events processing
        private bool _isHighFrequency;
        private Vector2 _pressPosition;
        private TimeSpan _pressTimestamp;
        private TimeSpan _timestamp;

        /// <summary>
        /// True if this touch was pressed and released on the same frame.
        /// In this case we will keep it around for the user to get by GetState that frame.
        /// However if they do not call GetState that frame, this touch will be forgotten.
        /// </summary>
        internal bool SameFrameReleased;

        /// <summary>
        /// Helper for assigning an invalid touch location.
        /// </summary>
        internal static readonly TouchLocation Invalid = new TouchLocation();

		#region Properties

        internal Vector2 PressPosition
        {
            get { return _pressPosition; }
        }

        internal TimeSpan PressTimestamp
        {
            get { return _pressTimestamp; }
        }

        internal TimeSpan Timestamp
        {
            get { return _timestamp; }
        }

        internal Vector2 Velocity
        {
            get { return _velocity; }
        }

        /// <summary>
        /// Gets the ID of the touch location.
        /// </summary>
		public int Id 
		{ 
			get
	        {
	            return _id;
	        }
		}

        /// <summary>
        /// Gets the position of the touch location.
        /// </summary>
        public Vector2 Position 
		{ 
			get
	        {
	            return _position;
	        }
		}

        /// <summary>
        /// Gets the pressure of the touch location.
        /// </summary>
        /// <remarks>Only used in Android devices</remarks>
		public float Pressure 
		{ 
			get
        	{
            	return _pressure;
        	}
		}

        /// <summary>
        /// Gets the current state of the touch location.
        /// </summary>
        public TouchLocationState State 
		{ 
			get
	        {
	            return _state;
	        } 
		}

        #endregion

        #region Constructors

        /// <inheritdoc cref="TouchLocation(int, TouchLocationState, Vector2, TouchLocationState, Vector2)"/>
        public TouchLocation(int id, TouchLocationState state, Vector2 position)
            : this(id, state, position, TouchLocationState.Invalid, Vector2.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchLocation"/> structure.
        /// </summary>
        /// <param name="id">ID of the touch location.</param>
        /// <param name="state">Current state of the touch location.</param>
        /// <param name="position">Position of the touch location.</param>
        /// <param name="previousState">Previous state of this touch location.</param>
        /// <param name="previousPosition">Previous position of this touch location.</param>
        public TouchLocation(int id, TouchLocationState state, Vector2 position, 
                                TouchLocationState previousState, Vector2 previousPosition)
            : this(id, state, position, previousState, previousPosition, TimeSpan.Zero, false)
        {
        }

        internal TouchLocation(int id, TouchLocationState state, Vector2 position, TimeSpan timestamp)
            : this(id, state, position, TouchLocationState.Invalid, Vector2.Zero, timestamp, false)
        {
        }

        internal TouchLocation(int id, TouchLocationState state, Vector2 position, TimeSpan timestamp, bool isHighFrequency)
            : this(id, state, position, TouchLocationState.Invalid, Vector2.Zero, timestamp, isHighFrequency)
        {
        }

        internal TouchLocation(int id, TouchLocationState state, Vector2 position,
            TouchLocationState previousState, Vector2 previousPosition, TimeSpan timestamp, bool isHighFrequency)
        {
            _id = id;
            _state = state;
            _position = position;
            _pressure = 0.0f;

            _previousState = previousState;
            _previousPosition = previousPosition;
            _previousPressure = 0.0f;

            _timestamp = timestamp;
            _velocity = Vector2.Zero;

            _isHighFrequency = isHighFrequency;

            // If this is a pressed location then store the 
            // current position and timestamp as pressed.
            if (state == TouchLocationState.Pressed)
            {
                _pressPosition = _position;
                _pressTimestamp = _timestamp;
            }
            else
            {
                _pressPosition = Vector2.Zero;
                _pressTimestamp = TimeSpan.Zero;
            }

            SameFrameReleased = false;
        }

		#endregion

        /// <summary>
        /// Returns a copy of the touch with the state changed to moved.
        /// </summary>
        /// <returns>The new touch location.</returns>
        internal TouchLocation AsMovedState()
        {
            var touch = this;

            // Store the current state as the previous.
            touch._previousState = touch._state;
            touch._previousPosition = touch._position;
            touch._previousPressure = touch._pressure;

            // Set the new state.
            touch._state = TouchLocationState.Moved;
            
            return touch;
        }

        /// <summary>
        /// Updates the touch location using the new event.
        /// </summary>
        /// <param name="touchEvent">The next event for this touch location.</param>
        internal bool UpdateState(TouchLocation touchEvent)
        {
            Debug.Assert(Id == touchEvent.Id, "The touch event must have the same Id!");
            Debug.Assert(State != TouchLocationState.Released, "We shouldn't be changing state on a released location!");
            Debug.Assert(   touchEvent.State == TouchLocationState.Moved ||
                            touchEvent.State == TouchLocationState.Released, "The new touch event should be a move or a release!");
            Debug.Assert(touchEvent.Timestamp >= _timestamp, "The touch event is older than our timestamp!");

            // Store the current state as the previous one.
            _previousPosition = _position;
            _previousState = _state;
            _previousPressure = _pressure;

            // Set the new state.
            _position = touchEvent._position;
            if (touchEvent.State == TouchLocationState.Released)
                _state = touchEvent._state;
            _pressure = touchEvent._pressure;

            // If time has elapsed then update the velocity.
            var delta = _position - _previousPosition;
            var elapsed = touchEvent.Timestamp - _timestamp;
            if (elapsed > TimeSpan.Zero)
            {
                // Use a simple low pass filter to accumulate velocity.
                var velocity = delta / (float)elapsed.TotalSeconds;
                _velocity += (velocity - _velocity) * 0.45f;
            }

            //Going straight from pressed to released on the same frame
            if (_previousState == TouchLocationState.Pressed && _state == TouchLocationState.Released && elapsed == TimeSpan.Zero)
            {
                //Lie that we are pressed for now
                SameFrameReleased = true;
                _state = TouchLocationState.Pressed;
            }

            // Set the new timestamp.
            _timestamp = touchEvent.Timestamp;

            // Return true if the state actually changed.
            return _state != _previousState || delta.LengthSquared() > 0.001f;
        }

        /// <summary>
        /// Returns true if the touch panel is configured to process high frequence touch events.
        /// </summary>
        public bool IsHighFrequencyEvent()
        {
            return _isHighFrequency;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
			if (obj is TouchLocation)
				return Equals((TouchLocation)obj);

			return false;
		}

        /// <inheritdoc/>
        public bool Equals(TouchLocation other)
        {
            return  _id.Equals(other._id) &&
                    _position.Equals(other._position) &&
                    _previousPosition.Equals(other._previousPosition);
        }


        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _id;
        }

        /// <summary>
        /// Gets a string representation of the <see cref="TouchLocation"/>.
        /// </summary>
        public override string ToString()
        {
            return "Touch id:"+_id+" state:"+_state + " position:" + _position + " pressure:" + _pressure +" prevState:"+_previousState+" prevPosition:"+ _previousPosition + " previousPressure:" + _previousPressure;
        }

        /// <summary>
        /// Attempts to get the previous location of this touch location object.
        /// </summary>
        /// <param name="aPreviousLocation">Previous location data, as a <see cref="TouchLocation"/>.</param>
        public bool TryGetPreviousLocation(out TouchLocation aPreviousLocation)
        {
			if (_previousState == TouchLocationState.Invalid)
			{
				aPreviousLocation._id = -1;
				aPreviousLocation._state = TouchLocationState.Invalid;
                aPreviousLocation._position = Vector2.Zero;
				aPreviousLocation._previousState = TouchLocationState.Invalid;
				aPreviousLocation._previousPosition = Vector2.Zero; 
				aPreviousLocation._pressure = 0.0f;
				aPreviousLocation._previousPressure = 0.0f;
			    aPreviousLocation._timestamp = TimeSpan.Zero;
			    aPreviousLocation._pressPosition = Vector2.Zero;
			    aPreviousLocation._pressTimestamp = TimeSpan.Zero;
                aPreviousLocation._velocity = Vector2.Zero;
                aPreviousLocation.SameFrameReleased = false;
                aPreviousLocation._isHighFrequency = false;
                return false;
			}

			aPreviousLocation._id = _id;
			aPreviousLocation._state = _previousState;
			aPreviousLocation._position = _previousPosition;
			aPreviousLocation._previousState = TouchLocationState.Invalid;
			aPreviousLocation._previousPosition = Vector2.Zero;
			aPreviousLocation._pressure = _previousPressure;
			aPreviousLocation._previousPressure = 0.0f;
			aPreviousLocation._timestamp = _timestamp;
            aPreviousLocation._pressPosition = _pressPosition;
            aPreviousLocation._pressTimestamp = _pressTimestamp;
            aPreviousLocation._velocity = _velocity;
            aPreviousLocation.SameFrameReleased = SameFrameReleased;
            aPreviousLocation._isHighFrequency = _isHighFrequency;
            return true;
        }

        /// <summary>
        /// Returns a value that indicates whether the two values are not equal.
        /// </summary>
        /// <param name="value1">The value on the left of the inequality operator.</param>
        /// <param name="value2">The value on the right of the inequality operator.</param>
        /// <returns><see langword="true"/> if the two values are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(TouchLocation value1, TouchLocation value2)
        {
			return  value1._id != value2._id || 
			        value1._state != value2._state ||
			        value1._position != value2._position ||
			        value1._previousState != value2._previousState ||
			        value1._previousPosition != value2._previousPosition;
        }

        /// <summary>
        /// Returns a value that indicates whether the two values are equal.
        /// </summary>
        /// <param name="value1">The value on the left of the equality operator.</param>
        /// <param name="value2">The value on the right of the equality operator.</param>
        /// <returns><see langword="true"/> if the two values are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(TouchLocation value1, TouchLocation value2)
        {
            return  value1._id == value2._id && 
			        value1._state == value2._state &&
			        value1._position == value2._position &&
			        value1._previousState == value2._previousState &&
			        value1._previousPosition == value2._previousPosition;
        }


        internal void AgeState()
        {
            if (_state == TouchLocationState.Moved)
            {
                _previousState = _state;
                _previousPosition = _position;
                _previousPressure = _pressure;
            }
            else
            {
                Debug.Assert(_state == TouchLocationState.Pressed, "Can only age the state of touches that are in the Pressed State");

                _previousState = _state;
                _previousPosition = _position;
                _previousPressure = _pressure;

                if (SameFrameReleased)
                    _state = TouchLocationState.Released;
                else
                    _state = TouchLocationState.Moved;
            }
        }
    }
}
