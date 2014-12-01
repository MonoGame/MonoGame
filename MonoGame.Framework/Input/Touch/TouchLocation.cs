#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009-2010 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

#region Using clause
using System;
using System.Diagnostics;
#endregion Using clause


namespace Microsoft.Xna.Framework.Input.Touch
{
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

		public int Id 
		{ 
			get
	        {
	            return _id;
	        }
		}

        public Vector2 Position 
		{ 
			get
	        {
	            return _position;
	        }
		}
		
		public float Pressure 
		{ 
			get
        	{
            	return _pressure;
        	}
		}
								
        public TouchLocationState State 
		{ 
			get
	        {
	            return _state;
	        } 
		}
		
		#endregion
		
		#region Constructors

        public TouchLocation(int id, TouchLocationState state, Vector2 position)
            : this(id, state, position, TouchLocationState.Invalid, Vector2.Zero)
        {
        }

        public TouchLocation(   int id, TouchLocationState state, Vector2 position, 
                                TouchLocationState previousState, Vector2 previousPosition)
            : this(id, state, position, previousState, previousPosition, TimeSpan.Zero)
        {
        }

        internal TouchLocation(int id, TouchLocationState state, Vector2 position, TimeSpan timestamp)
            : this(id, state, position, TouchLocationState.Invalid, Vector2.Zero, timestamp)
        {
        }

        internal TouchLocation(int id, TouchLocationState state, Vector2 position,
            TouchLocationState previousState, Vector2 previousPosition, TimeSpan timestamp)
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

        public override bool Equals(object obj)
        {
			if (obj is TouchLocation)
				return Equals((TouchLocation)obj);

			return false;
		}

        public bool Equals(TouchLocation other)
        {
            return  _id.Equals(other._id) &&
                    _position.Equals(other._position) &&
                    _previousPosition.Equals(other._previousPosition);
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public override string ToString()
        {
            return "Touch id:"+_id+" state:"+_state + " position:" + _position + " pressure:" + _pressure +" prevState:"+_previousState+" prevPosition:"+ _previousPosition + " previousPressure:" + _previousPressure;
        }

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
            return true;
        }

        public static bool operator !=(TouchLocation value1, TouchLocation value2)
        {
			return  value1._id != value2._id || 
			        value1._state != value2._state ||
			        value1._position != value2._position ||
			        value1._previousState != value2._previousState ||
			        value1._previousPosition != value2._previousPosition;
        }

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