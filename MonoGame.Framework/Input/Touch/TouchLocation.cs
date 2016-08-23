#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright � 2009-2010 The MonoGame Team
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

        /// <summary>
        /// Helper for assigning an invalid touch location.
        /// </summary>
        internal static readonly TouchLocation Invalid = new TouchLocation();

		#region Properties
        
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
            : this(id, state, position, 0.0f, TouchLocationState.Invalid, Vector2.Zero, 0.0f)
        {            
        }

        internal TouchLocation(int id, TouchLocationState state, Vector2 position, float pressure,
                                TouchLocationState previousState, Vector2 previousPosition, float previousPressure)
        {
            _id = id;
            _state = state;
            _position = position;
            _pressure = pressure;

            _previousState = previousState;
            _previousPosition = previousPosition;
            _previousPressure = previousPressure;
        }


        #endregion

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
                return false;
			}

			aPreviousLocation._id = _id;
			aPreviousLocation._state = _previousState;
			aPreviousLocation._position = _previousPosition;
			aPreviousLocation._previousState = TouchLocationState.Invalid;
			aPreviousLocation._previousPosition = Vector2.Zero;
			aPreviousLocation._pressure = _previousPressure;
			aPreviousLocation._previousPressure = 0.0f;
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
        
    }
}