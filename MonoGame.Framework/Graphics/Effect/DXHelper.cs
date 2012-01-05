using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXHelper
	{
		
		public static T[] UnmarshalArray<T>(IntPtr ptr, int count) {
			Type type = typeof(T);
			T[] ret = new T[count];
			for (int i=0; i<count; i++) {
				ret[i] = (T)Marshal.PtrToStructure (ptr, type);
				ptr = IntPtr.Add (ptr, Marshal.SizeOf (type));
			}
			return ret;
		}
		
	}
}

