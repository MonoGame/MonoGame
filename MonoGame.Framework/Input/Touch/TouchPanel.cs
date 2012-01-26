#region License
// /*
// Microsoft Public License (Ms-PL)
// XnaTouch - Copyright � 2009-2010 The XnaTouch Team
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
#endregion Using clause

namespace Microsoft.Xna.Framework.Input.Touch
{
    public static class TouchPanel
    {
		internal static TouchCollection Collection = new TouchCollection();
		internal static Queue<GestureSample> GestureList = new Queue<GestureSample>();
		internal static event EventHandler EnabledGesturesChanged;
		
        public static TouchPanelCapabilities GetCapabilities()
        {
			// Go off and create an updated TouchPanelCapabilities with the latest state			
            return new TouchPanelCapabilities(false,true,8);;
        }

        public static TouchCollection GetState()
        {
			TouchCollection result = new TouchCollection(Collection);		
			Collection.Update();
			return result;
        }       
		
		public static void Reset()
		{
			Collection.Clear();
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