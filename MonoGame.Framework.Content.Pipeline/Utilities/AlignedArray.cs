// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// AlignedArray found at http://meekmaak.blogspot.com.au/2010/06/c-memory-aligned-array-wrapper-for-fast.html

using System;
//using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content.Pipeline.Utilities
{
    public class AlignedArray : IDisposable
	{
		private byte[] buffer;
		private GCHandle bufferHandle;

		private IntPtr bufferPointer;
		private readonly int length;

        public IntPtr Buffer
        {
            get
            {
                return bufferPointer;
            }
        }

		public AlignedArray(int length, int byteAlignment)
		{
			this.length = length;
			buffer = new byte[length * sizeof(byte) + byteAlignment];
			bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			long ptr = bufferHandle.AddrOfPinnedObject().ToInt64();
			// round up ptr to nearest 'byteAlignment' boundary
			ptr = (ptr + byteAlignment - 1) & ~(byteAlignment - 1);
			bufferPointer = new IntPtr(ptr);
		}

		public AlignedArray(byte[] src, int byteAlignment)
			: this(src.Length, byteAlignment)
		{
			Write(0, src, 0, src.Length);
		}

		~AlignedArray()
		{
			Dispose(false);
		}

		protected void Dispose(bool disposing)
		{
			if (bufferHandle.IsAllocated)
			{
				bufferHandle.Free();
				buffer = null;
			}
		}

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
		}
		#endregion

		public byte this[int index]
		{
			get
			{
				unsafe
				{
					return GetPointer()[index];
				}
			}
			set
			{
				unsafe
				{
					GetPointer()[index] = value;
				}
			}
		}

		public int Length
		{
			get
			{
				return length;
			}
		}

		public void Write(int index, byte[] src, int srcIndex, int count)
		{
			if (index < 0 || index >= length)
				throw new IndexOutOfRangeException();

			if ((index + count) > length)
				count = Math.Max(0, length - index);

			System.Runtime.InteropServices.Marshal.Copy(src, srcIndex,
				new IntPtr(bufferPointer.ToInt64() + index * sizeof(byte)), count);

		}

		public void Read(int index, byte[] dest, int dstIndex, int count)
		{
			if (index < 0 || index >= length)
				throw new IndexOutOfRangeException();
			if ((index + count) > length)
				count = Math.Max(0, length - index);

			System.Runtime.InteropServices.Marshal.Copy(
				new IntPtr(bufferPointer.ToInt64() + index * sizeof(byte)),
				dest, dstIndex, count);
		}

		public byte[] GetManagedArray()
		{
			return GetManagedArray(0, length);
		}

		public byte[] GetManagedArray(int index, int count)
		{
			byte[] result = new byte[count];
			Read(index, result, 0, count);
			return result;
		}

		public unsafe byte* GetPointer(int index)
		{
			return GetPointer() + index;
		}

		public unsafe byte* GetPointer()
		{
			return (byte*)bufferPointer.ToPointer();
		}
	}
}
