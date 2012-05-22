#region License
// /*
// Microsoft Public License (Ms-PL)
// XnaTouch - Copyright © 2009-2010 The XnaTouch Team
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion Using clause

namespace Microsoft.Xna.Framework.Input.Touch
{
    public static class TouchPanel
    {
        /// <summary>
        /// The currently touch state.
        /// </summary>
        private static Dictionary<int, TouchLocation> _touchLocations = new Dictionary<int, TouchLocation>();

        /// <summary>
        /// The touch events to be processed and added to the current state.
        /// </summary>
        private static List<TouchLocation> _events = new List<TouchLocation>();

        /// <summary>
        /// Scratch list used to remove touch state.
        /// </summary>
        private static List<int> _removeId = new List<int>();

        /// <summary>
        /// The current touch state.
        /// </summary>
        private static TouchCollection _state = new TouchCollection();

        /// <summary>
        /// If true an update to the touch state should occur
        /// on the next call to GetState.
        /// </summary>
        private static bool _updateState = true;

        internal static Queue<GestureSample> GestureList = new Queue<GestureSample>();
		internal static event EventHandler EnabledGesturesChanged;
        internal static TouchPanelCapabilities Capabilities = new TouchPanelCapabilities();

        public static TouchPanelCapabilities GetCapabilities()
        {
            Capabilities.Initialize();
            return Capabilities;
        }

        public static TouchCollection GetState()
        {
            // If the state isn't dirty then just
            // return the current state.
            if (!_updateState)
                return _state;

            // Remove the previously released touch locations.
            foreach (var keyLoc in _touchLocations)
            {
                if (keyLoc.Value.State == TouchLocationState.Released)
                    _removeId.Add(keyLoc.Key);
            }
            foreach (var id in _removeId)
                _touchLocations.Remove(id);

            // Update the existing touch locations.
            for (var i = 0; i < _events.Count; )
            {
                var loc = _events[i];

                TouchLocation prev;
                if (_touchLocations.TryGetValue(loc.Id, out prev))
                {
                    // Remove this event.
                    _events.RemoveAt(i);

                    // Remove any pending events of this type.
                    for (var j = i; j < _events.Count; )
                    {
                        if (_events[j].Id == loc.Id)
                        {
                            loc = _events[j];
                            _events.RemoveAt(j);
                            continue;
                        }

                        j++;
                    }

                    // Set the new touch location state.
                    _touchLocations[loc.Id] = new TouchLocation(loc.Id,
                                                                    loc.State, loc.Position, loc.Pressure,
                                                                    prev.State, prev.Position, prev.Pressure);
                    continue;
                }

                i++;
            }

            // Add any new pressed events.
            for (var i = 0; i < _events.Count; )
            {
                var loc = _events[i];

                if (loc.State == TouchLocationState.Pressed)
                {
                    _touchLocations.Add(loc.Id, loc);
                    _events.RemoveAt(i);
                    continue;
                }

                i++;
            }

            // Set the new state.
            _state = new TouchCollection(_touchLocations.Values.ToArray());

            // Don't update again till the next frame.
            _updateState = false;
            
            // Return the state.
            return _state;
        }
        
        internal static void AddEvent(TouchLocation location)
        {
            _events.Add(location);
        }

        internal static void UpdateState()
        {
            // Just tell the next call to GetState() that
            // it is time for it to update.
            _updateState = true;
        }

		public static GestureSample ReadGesture()
        {
			return GestureList.Dequeue();			
        }

        public static int DisplayHeight
        {
            get
            {
#if ANDROID				
				return (int)Game.Activity.Resources.DisplayMetrics.HeightPixels;
#else
                return Game.Instance.Window.ClientBounds.Height;
#endif
            }
            set
            {
            }
        }

        public static DisplayOrientation DisplayOrientation
        {
            get;
            set;
        }

        public static int DisplayWidth
        {
            get
            {
#if ANDROID				
				return (int)Game.Activity.Resources.DisplayMetrics.WidthPixels;
#else
                return Game.Instance.Window.ClientBounds.Width;
#endif				
            }
            set
            {
            }
        }
		
		private static GestureType _enabledGestures = GestureType.None;
        public static GestureType EnabledGestures
        {
            get
			{ 
				return _enabledGestures;
			}
            set
			{
				var prev=_enabledGestures;
				_enabledGestures = value;
				if (_enabledGestures!=prev && EnabledGesturesChanged!=null)
					EnabledGesturesChanged(null, null);
			}
        }

        public static bool IsGestureAvailable
        {
            get
            {
				return ( GestureList.Count > 0 );				
            }
        }
    }
}