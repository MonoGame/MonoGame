// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public enum StructuredBufferType
    {
        Basic, // no internal counter
        Append, // internal counter to allow Append() and Consume() from compute shader
        Counter, // internal counter to allow IncrementCounter() and DecrementCounter() from compute shader
    }

    public partial class StructuredBuffer : BufferResource
    {
        public new int ElementCount { get { return base.ElementCount; } }

        public new StructuredBufferType StructuredBufferType { get { return base.StructuredBufferType;  } }

        public int CounterResetValue { get { return CounterBufferResetValue; } set { CounterBufferResetValue = value; } }

        public StructuredBuffer(GraphicsDevice graphicsDevice, Type structureType, int elementCount, BufferUsage bufferUsage, ShaderAccess shaderAccess, StructuredBufferType bufferType = StructuredBufferType.Basic, int counterResetValue = -1) :
            base(graphicsDevice,
                elementCount,
                ReflectionHelpers.ManagedSizeOf(structureType),
                bufferUsage,
                false,
                BufferType.StructuredBuffer,
                shaderAccess,
                bufferType,
                counterResetValue)
        {
        }

        /// <summary>
        /// Copy this buffers counter value to another buffer.
        /// </summary>
        public void CopyCounterValue(BufferResource destinationBuffer, int byteOffset)
        {
            GraphicsDevice.CopyStructuredBufferCounterValue(this, destinationBuffer, byteOffset);
        }

        /// <summary>
        /// Get the data from this StructuredBuffer.
        /// </summary>
        /// <typeparam name="T">The struct you want to fill.</typeparam>
        /// <param name="offsetInBytes">The offset to the first element in the structured buffer in bytes.</param>
        /// <param name="data">An array of T's to be filled.</param>
        /// <param name="startIndex">The index to start filling the data array.</param>
        /// <param name="elementCount">The number of T's to get.</param>
        /// <param name="structureStride">The size of the structure.</param>
        ///
        /// <remarks>
        /// Note that this pulls data from VRAM into main memory and because of that is a very expensive operation.
        /// It is often a better idea to keep a copy of the data in main memory.
        /// </remarks>
        public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int structureStride = 0) where T : struct
        {
            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
            if (structureStride == 0)
                structureStride = elementSizeInBytes;

            var vertexByteSize = ElementCount * ElementStride;
            if (structureStride > vertexByteSize)
                throw new ArgumentOutOfRangeException("structureStride", "Structure stride can not be larger than the buffer size.");

            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new ArgumentOutOfRangeException("elementCount", "This parameter must be a valid index within the array.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
            if (elementCount > 1 && elementCount * structureStride > vertexByteSize)
                throw new InvalidOperationException("The array is not the correct size for the amount of data requested.");

            PlatformGetData<T>(offsetInBytes, data, startIndex, elementCount, structureStride);
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            this.GetData<T>(0, data, startIndex, elementCount, 0);
        }

        public void GetData<T>(T[] data) where T : struct
        {
            var elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
            this.GetData<T>(0, data, 0, data.Length, elementSizeInByte);
        }

        /// <summary>
        /// Sets the structured buffer data, specifying the index at which to start copying from the source data array,
        /// the number of elements to copy from the source data array, 
        /// and how far apart elements from the source data array should be when they are copied into the structured buffer.
        /// </summary>
        /// <typeparam name="T">Type of elements in the data array.</typeparam>
        /// <param name="offsetInBytes">Offset in bytes from the beginning of the structured buffer to the start of the copied data.</param>
        /// <param name="data">Data array.</param>
        /// <param name="startIndex">Index at which to start copying from <paramref name="data"/>.
        /// Must be within the <paramref name="data"/> array bounds.</param>
        /// <param name="elementCount">Number of elements to copy from <paramref name="data"/>.
        /// The combination of <paramref name="startIndex"/> and <paramref name="elementCount"/> 
        /// must be within the <paramref name="data"/> array bounds.</param>
        /// <param name="structureStride">Specifies how far apart, in bytes, elements from <paramref name="data"/> should be when 
        /// they are copied into the structured buffer.
        /// In almost all cases this should be <c>sizeof(T)</c>, to create a tightly-packed vertex buffer.
        /// If you specify <c>sizeof(T)</c>, elements from <paramref name="data"/> will be copied into the 
        /// vertex buffer with no padding between each element.
        /// If you specify a value greater than <c>sizeof(T)</c>, elements from <paramref name="data"/> will be copied 
        /// into the structured buffer with padding between each element.
        /// If you specify <c>0</c> for this parameter, it will be treated as if you had specified <c>sizeof(T)</c>.
        /// With the exception of <c>0</c>, you must specify a value greater than or equal to <c>sizeof(T)</c>.</param>
        /// <remarks>
        /// If you provide a <c>byte[]</c> in the <paramref name="data"/> parameter, then you should almost certainly
        /// set <paramref name="structureStride"/> to <c>1</c>, to avoid leaving any padding between the <c>byte</c> values
        /// when they are copied into the structured buffer.
        /// </remarks>
        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int structureStride) where T : struct
        {
            SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, structureStride, SetDataOptions.None);
        }

        /// <summary>
        /// Sets the structured buffer data, specifying the index at which to start copying from the source data array,
        /// and the number of elements to copy from the source data array. This is the same as calling 
        /// <see cref="SetData{T}(int, T[], int, int, int)"/>  with <c>offsetInBytes</c> equal to <c>0</c>,
        /// and <c>structureStride</c> equal to <c>sizeof(T)</c>.
        /// </summary>
        /// <typeparam name="T">Type of elements in the data array.</typeparam>
        /// <param name="data">Data array.</param>
        /// <param name="startIndex">Index at which to start copying from <paramref name="data"/>.
        /// Must be within the <paramref name="data"/> array bounds.</param>
        /// <param name="elementCount">Number of elements to copy from <paramref name="data"/>.
        /// The combination of <paramref name="startIndex"/> and <paramref name="elementCount"/> 
        /// must be within the <paramref name="data"/> array bounds.</param>
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
            SetDataInternal<T>(0, data, startIndex, elementCount, elementSizeInBytes, SetDataOptions.None);
        }

        /// <summary>
        /// Sets the structured buffer data. This is the same as calling <see cref="SetData{T}(int, T[], int, int, int)"/> 
        /// with <c>offsetInBytes</c> and <c>startIndex</c> equal to <c>0</c>, <c>elementCount</c> equal to <c>data.Length</c>, 
        /// and <c>structureStride</c> equal to <c>sizeof(T)</c>.
        /// </summary>
        /// <typeparam name="T">Type of elements in the data array.</typeparam>
        /// <param name="data">Data array.</param>
        public void SetData<T>(T[] data) where T : struct
        {
            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
            SetDataInternal<T>(0, data, 0, data.Length, elementSizeInBytes, SetDataOptions.None);
        }

        protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int structureStride, SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
            var bufferSize = ElementCount * ElementStride;

            if (structureStride == 0)
                structureStride = elementSizeInBytes;

            var byteSize = ElementCount * ElementStride;
            if (structureStride > byteSize)
                throw new ArgumentOutOfRangeException("structureStride", "Structure stride can not be larger than the buffer size.");

            if (startIndex + elementCount > data.Length || elementCount <= 0)
                throw new ArgumentOutOfRangeException("data", "The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (elementCount > 1 && (elementCount * structureStride > bufferSize))
                throw new InvalidOperationException("The structure stride is larger than the vertex buffer.");
            if (structureStride < elementSizeInBytes)
                throw new ArgumentOutOfRangeException("The structure stride must be greater than or equal to the size of the specified data (" + elementSizeInBytes + ").");

            PlatformSetData<T>(offsetInBytes, data, startIndex, elementCount, structureStride, options, bufferSize, elementSizeInBytes);
        }
    }
}
