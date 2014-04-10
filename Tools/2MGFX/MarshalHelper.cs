using System;
using System.Runtime.InteropServices;

namespace TwoMGFX
{
	internal class MarshalHelper
	{
        public static T Unmarshal<T>(IntPtr ptr)
        {
            var type = typeof(T);
            var result = (T)Marshal.PtrToStructure(ptr, type);
            return result;
        }

		public static T[] UnmarshalArray<T>(IntPtr ptr, int count) 
        {
			var type = typeof(T);
            var size = Marshal.SizeOf(type);            
            var ret = new T[count];

            for (int i = 0; i < count; i++)
            {
                var offset = i * size;
				var structPtr = new IntPtr(ptr.ToInt64() + offset);
                ret[i] = (T)Marshal.PtrToStructure(structPtr, type);
            }

			return ret;
		}

        public static byte[] UnmarshalArray(IntPtr ptr, int count)
        {
            var result = new byte[count];
            Marshal.Copy(ptr, result, 0, count);
            return result;
        }	
	}
}

