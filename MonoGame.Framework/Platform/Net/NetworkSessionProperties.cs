#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
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
using System.Collections.Generic;

using Microsoft.Xna.Framework.GamerServices;

#endregion Using clause

namespace Microsoft.Xna.Framework.Net
{
	public class NetworkSessionProperties : List<Nullable<int>>
	{

		// The NetworkSessionProperies can contain up to eight interger values
		//  from all the documentation I can find as well as tests that have been done
		//  to confirm this.
		public NetworkSessionProperties () : base(8)
		{
			this.Add (null);
			this.Add (null);
			this.Add (null);
			this.Add (null);
			this.Add (null);
			this.Add (null);
			this.Add (null);
			this.Add (null);

		}
		
		public static void WriteProperties (NetworkSessionProperties properties, int[] propertyData) 
		{
			
			for (int x = 0; x < 8; x++) {
				if ((properties != null) && properties[x].HasValue) {
					// flag it as having a value
					propertyData[x*2] = 1;
					propertyData[x*2+1] = properties[x].Value;
					
				}
				else {
					// flag it as not having a value
					propertyData[x*2] = 0;
					propertyData[x*2+1] = 0;
					
				}
			}
		}
		
		public static void ReadProperties (NetworkSessionProperties properties, int[] propertyData) 
		{
			for (int x = 0; x < 8; x++) {
				// set it to null to start
				properties[x] = null;
				// and only if the flag is turned on do we have a value.
				if (propertyData[x*2] > 0)
					properties[x] = propertyData[x*2+1];
			}
		}		
	}
}