using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXHelper
	{
		
		public static T[] UnmarshalArray<T>(IntPtr ptr, int count) 
        {
			var type = typeof(T);
            var size = Marshal.SizeOf(type);            
            var ret = new T[count];

            for (int i = 0; i < count; i++)
            {
                var offset = i * size;
                ret[i] = (T)Marshal.PtrToStructure(ptr + offset, type);
            }

			return ret;
		}
		
	}
}

