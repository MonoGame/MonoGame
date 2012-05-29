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
#endregion Using clause

using MonoGame.Framework.Touch;

namespace Microsoft.Xna.Framework.Input.Touch
{
    public struct TouchLocation : IEquatable<TouchLocation>
    {
		/// <summary>
		///Attributes 
		/// </summary>
		private int id;
		private Vector2 position;
		private Vector2 previousPosition;
		private TouchLocationState state;
		private TouchLocationState previousState;
		
        // TODO: Lets try to remove these.
		// internals required for gesture recognition.

        internal TouchInfo _touchHistory;
				
		// Only used in Android, for now
		private float pressure;
		private float previousPressure;
		
		#region Properties

        internal TouchInfo TouchHistory
        {
            get { return _touchHistory; }
            set { _touchHistory = value; }
        }

		public int Id 
		{ 
			get
	        {
	            return id;
	        }
		}

        public Vector2 Position 
		{ 
			get
	        {
	            return position;
	        }
		}
		
		public float Pressure 
		{ 
			get
        	{
            	return pressure;
        	}
		}
								
        public TouchLocationState State 
		{ 
			get
	        {
	            return state;
	        } 
		}
		
		#endregion
		
		#region Constructors

		public TouchLocation(int aId, TouchLocationState aState, Vector2 aPosition,
		                     TouchLocationState aPreviousState, Vector2 aPreviousPosition)
        {
            id = aId;
			position = aPosition;
			previousPosition = aPreviousPosition;
			state = aState;
			previousState = aPreviousState;	
			pressure = 0.0f;
			previousPressure = 0.0f;
            _touchHistory = new TouchInfo(id, position);
        }

        public TouchLocation(int aId, TouchLocationState aState, Vector2 aPosition)
        {
            id = aId;
			position = aPosition;
			previousPosition = Vector2.Zero;		
			state = aState;
			previousState = TouchLocationState.Invalid;	
			pressure = 0.0f;
			previousPressure = 0.0f;
            _touchHistory = new TouchInfo(id, position);
        }
		
		// Only for Android
		public TouchLocation(int aId, TouchLocationState aState, Vector2 aPosition, float aPressure,
		                     TouchLocationState aPreviousState, Vector2 aPreviousPosition, float aPreviousPressure)
        {
            id = aId;
			position = aPosition;
			previousPosition = aPreviousPosition;
			state = aState;
			previousState = aPreviousState;	
			pressure = aPressure;
			previousPressure = aPreviousPressure;
            _touchHistory = new TouchInfo(id, position);
        }
		
        // We are "updating" a touch location by creating a new one and filling in it's appropriate states.
		public TouchLocation(int aId, TouchLocationState aState, Vector2 aPosition, float aPressure,
		                     TouchLocationState aPreviousState, Vector2 aPreviousPosition, float aPreviousPressure, TouchInfo aPrevTouchInfo)
        {
            id = aId;
			position = aPosition;
			previousPosition = aPreviousPosition;
			state = aState;
			previousState = aPreviousState;	
			pressure = aPressure;
			previousPressure = aPreviousPressure;

            // Update the history object.
            _touchHistory = aPrevTouchInfo;
            _touchHistory.LogPosition(aPosition);
        }
		
		public TouchLocation(int aId, TouchLocationState aState, Vector2 aPosition, float aPressure)
        {
            id = aId;
			position = aPosition;
			previousPosition = Vector2.Zero;		
			state = aState;
			previousState = TouchLocationState.Invalid;
			pressure = aPressure;
			previousPressure = 0.0f;
            _touchHistory = new TouchInfo(id, position);
        }
		#endregion
		
        public override bool Equals(object obj)
        {
			bool result = false;
			if (obj is TouchLocation)
			{
				result = Equals((TouchLocation) obj);
			}
			return result;
		}

        public bool Equals(TouchLocation other)
        {
            return ( ( this.Id.Equals( other.Id ) ) && 
			        ( this.Position.Equals( other.Position ) ) && 
			        ( this.previousPosition.Equals ( other.previousPosition)));
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            return "Touch id:"+id+" state:"+state + " position:" + position + " pressure:" + pressure +" prevState:"+previousState+" prevPosition:"+ previousPosition + " previousPressure:" + previousPressure;
        }

        public bool TryGetPreviousLocation(out TouchLocation aPreviousLocation)
        {
			if ( previousState == TouchLocationState.Invalid )
			{
				aPreviousLocation.id = -1;
				aPreviousLocation.state = TouchLocationState.Invalid;
                aPreviousLocation.position = Vector2.Zero;
				aPreviousLocation.previousState = TouchLocationState.Invalid;
				aPreviousLocation.previousPosition = Vector2.Zero; 
				aPreviousLocation.pressure = 0.0f;
				aPreviousLocation.previousPressure = 0.0f;
                aPreviousLocation._touchHistory = new TouchInfo(-1, Vector2.Zero);
				return false;
			}
			else
			{
				aPreviousLocation.id = this.id;
				aPreviousLocation.state = this.previousState;
				aPreviousLocation.position = this.previousPosition;
				aPreviousLocation.previousState = TouchLocationState.Invalid;
				aPreviousLocation.previousPosition = Vector2.Zero;
				aPreviousLocation.pressure = this.previousPressure;
				aPreviousLocation.previousPressure = 0.0f;
                aPreviousLocation._touchHistory = this.TouchHistory;
				return true;
			}
        }

        public static bool operator !=(TouchLocation value1, TouchLocation value2)
        {
			return ! (((value1.id == value2.id) && 
			        (value1.state == value2.state) &&
			        (value1.position == value2.position) &&
			        (value1.previousState == value2.previousState) &&
			        (value1.previousPosition == value2.previousPosition)));
        }

        public static bool operator ==(TouchLocation value1, TouchLocation value2)
        {
            return ((value1.id == value2.id) && 
			        (value1.state == value2.state) &&
			        (value1.position == value2.position) &&
			        (value1.previousState == value2.previousState) &&
			        (value1.previousPosition == value2.previousPosition));
        }

       
    }
}