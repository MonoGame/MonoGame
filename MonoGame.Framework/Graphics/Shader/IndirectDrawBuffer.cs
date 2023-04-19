// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public struct DrawInstancedArguments
    {
        public uint VertexCountPerInstance;
        public uint InstanceCount;
        public uint StartVertexLocation;
        public uint StartInstanceLocation;

        public const int ByteOffsetVertexCountPerInstance = 0;
        public const int ByteOffsetInstanceCount          = 4;
        public const int ByteOffsetStartVertexLocation    = 8;
        public const int ByteOffsetStartInstanceLocation  = 12;

        public const int Count = 4;
        public const int ByteSize = Count * 4;
        public int ArgumentCount { get { return Count; } }
        public int TotalByteSize { get { return ByteSize; } }

        public int WriteToArray(uint[] array, int offset)
        {
            array[offset + 0] = VertexCountPerInstance;
            array[offset + 1] = InstanceCount;
            array[offset + 2] = StartVertexLocation;
            array[offset + 3] = StartInstanceLocation;
            return Count;
        }
    }

    public struct DrawIndexedInstancedArguments
    {
        public uint IndexCountPerInstance;
        public uint InstanceCount;
        public uint StartIndexLocation;
        public  int BaseVertexLocation;
        public uint StartInstanceLocation;

        public const int ByteOffsetIndexCountPerInstance = 0;
        public const int ByteOffsetInstanceCount         = 4;
        public const int ByteOffsetStartIndexLocation    = 8;
        public const int ByteOffsetBaseVertexLocation    = 12;
        public const int ByteOffsetStartInstanceLocation = 16;

        public const int Count = 5;
        public const int ByteSize = Count * 4;
        public int ArgumentCount { get { return Count; } }
        public int TotalByteSize { get { return ByteSize; } }

        public int WriteToArray(uint[] array, int offset)
        {
            array[offset + 0] = IndexCountPerInstance;
            array[offset + 1] = InstanceCount;
            array[offset + 2] = StartIndexLocation;
            array[offset + 3] = (uint)BaseVertexLocation;
            array[offset + 4] = StartInstanceLocation;
            return Count;
        }
    }

    public struct DispatchComputeArguments
    {
        public uint GroupCountX;
        public uint GroupCountY;
        public uint GroupCountZ;

        public const int ByteOffsetGroupCountX = 0;
        public const int ByteOffsetGroupCountY = 4;
        public const int ByteOffsetGroupCountZ = 8;

        public const int Count = 3;
        public const int ByteSize = Count * 4;
        public int ArgumentCount { get { return Count; } }
        public int TotalByteSize { get { return ByteSize; } }

        public int WriteToArray(uint[] array, int offset)
        {
            array[offset + 0] = GroupCountX;
            array[offset + 1] = GroupCountY;
            array[offset + 2] = GroupCountZ;
            return Count;
        }
    }

    public partial class IndirectDrawBuffer : BufferResource
    {
        private uint[] data;

        private uint[] Data
        {
            get
            {
                if (data == null)
                    data = new uint[ElementCount];

                return data;
            }
        }

        public int ElementCount { get { return base.ElementCount; } }
        
        public IndirectDrawBuffer(GraphicsDevice graphicsDevice, BufferUsage bufferUsage, ShaderAccess shaderAccess, int elementCount = 5) :
            base(graphicsDevice, elementCount, sizeof(uint), bufferUsage, false, BufferType.IndirectDrawBuffer, shaderAccess)
        {
        }

        public void SetData(DrawInstancedArguments args)
        {
            args.WriteToArray(Data, 0);
            SetData(data, 0, 4);
        }

        public void SetData(DrawIndexedInstancedArguments args)
        {
            args.WriteToArray(Data, 0);
            SetData(data, 0, 4);
        }

        public void SetData(DispatchComputeArguments args)
        {
            args.WriteToArray(Data, 0);
            SetData(data, 0, 4);
        }

        /// <summary>
        /// Get the data from this indirect draw buffer.
        /// </summary>
        /// <param name="offsetInBytes">The offset to the first element in the indirect draw buffer in bytes.</param>
        /// <param name="data">An array of uint's to be filled.</param>
        /// <param name="startIndex">The index to start filling the data array.</param>
        /// <param name="elementCount">The number of uint's to get.</param>
        public void GetData(int offsetInBytes, uint[] data, int startIndex, int elementCount) 
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new ArgumentOutOfRangeException("elementCount", "This parameter must be a valid index within the array.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");

            PlatformGetData(offsetInBytes, data, startIndex, elementCount, ElementStride);
        }

        public void GetData(uint[] data, int startIndex, int elementCount)
        {
            this.GetData(0, data, startIndex, elementCount);
        }

        public void GetData(uint[] data)
        {
            this.GetData(0, data, 0, data.Length);
        }

        /// <summary>
        /// Sets the indirect buffer data, specifying the index at which to start copying from the source data array,
        /// and the number of elements to copy from the source data array.
        /// </summary>
        /// <param name="offsetInBytes">Offset in bytes from the beginning of the indirect draw buffer to the start of the copied data.</param>
        /// <param name="data">Data array.</param>
        /// <param name="startIndex">Index at which to start copying from <paramref name="data"/>.
        /// Must be within the <paramref name="data"/> array bounds.</param>
        /// <param name="elementCount">Number of elements to copy from <paramref name="data"/>.
        /// The combination of <paramref name="startIndex"/> and <paramref name="elementCount"/> 
        /// must be within the <paramref name="data"/> array bounds.</param>
        public void SetData(int offsetInBytes, uint[] data, int startIndex, int elementCount) 
        {
            SetDataInternal(offsetInBytes, data, startIndex, elementCount, SetDataOptions.None);
        }

        /// <summary>
        /// Sets the indirect draw buffer data, specifying the index at which to start copying from the source data array,
        /// and the number of elements to copy from the source data array. This is the same as calling 
        /// <see cref="SetData(int, uint[], int, int)"/>  with <c>offsetInBytes</c> equal to <c>0</c>.
        /// </summary>
        /// <param name="data">Data array.</param>
        /// <param name="startIndex">Index at which to start copying from <paramref name="data"/>.
        /// Must be within the <paramref name="data"/> array bounds.</param>
        /// <param name="elementCount">Number of elements to copy from <paramref name="data"/>.
        /// The combination of <paramref name="startIndex"/> and <paramref name="elementCount"/> 
        /// must be within the <paramref name="data"/> array bounds.</param>
		public void SetData(uint[] data, int startIndex, int elementCount)
        { 
            SetDataInternal(0, data, startIndex, elementCount, SetDataOptions.None);
        }

        /// <summary>
        /// Sets the indirect draw buffer data. This is the same as calling <see cref="SetData(int, uint[], int, int)"/> 
        /// with <c>offsetInBytes</c> and <c>startIndex</c> equal to <c>0</c> and <c>elementCount</c> equal to <c>data.Length</c>.
        /// </summary>
        /// <param name="data">Data array.</param>
        public void SetData(uint[] data)
        {
            SetDataInternal(0, data, 0, data.Length, SetDataOptions.None);
        }

        protected void SetDataInternal(int offsetInBytes, uint[] data, int startIndex, int elementCount, SetDataOptions options) 
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (startIndex + elementCount > data.Length || elementCount <= 0)
                throw new ArgumentOutOfRangeException("data", "The array specified in the data parameter is not the correct size for the amount of data requested.");

            PlatformSetData(offsetInBytes, data, startIndex, elementCount, ElementStride, options, ElementCount * ElementStride, ElementStride);
        }
    }
}
