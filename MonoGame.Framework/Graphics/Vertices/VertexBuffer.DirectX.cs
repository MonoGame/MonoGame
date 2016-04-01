// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer
    {
        private SharpDX.Direct3D11.Buffer _buffer;

        internal SharpDX.Direct3D11.Buffer Buffer
        {
            get
            {
                GenerateIfRequired();
                return _buffer;
            }
        }

        private void PlatformConstruct()
        {
            GenerateIfRequired();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _buffer);
        }

        void GenerateIfRequired()
        {
            if (_buffer != null)
                return;

            // TODO: To use Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            var accessflags = SharpDX.Direct3D11.CpuAccessFlags.None;
            var usage = SharpDX.Direct3D11.ResourceUsage.Default;

            if (_isDynamic)
            {
                accessflags |= SharpDX.Direct3D11.CpuAccessFlags.Write;
                usage = SharpDX.Direct3D11.ResourceUsage.Dynamic;
            }

            _buffer = new SharpDX.Direct3D11.Buffer(GraphicsDevice._d3dDevice,
                                                        VertexDeclaration.VertexStride * VertexCount,
                                                        usage,
                                                        SharpDX.Direct3D11.BindFlags.VertexBuffer,
                                                        accessflags,
                                                        SharpDX.Direct3D11.ResourceOptionFlags.None,
                                                        0  // StructureSizeInBytes
                                                        );
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            GenerateIfRequired();

            if (_isDynamic)
            {
                throw new NotImplementedException();
            }
            else
            {
                var deviceContext = GraphicsDevice._d3dContext;

                // Copy the buffer to a staging resource
                var stagingDesc = _buffer.Description;
                stagingDesc.BindFlags = SharpDX.Direct3D11.BindFlags.None;
                stagingDesc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read | SharpDX.Direct3D11.CpuAccessFlags.Write;
                stagingDesc.Usage = SharpDX.Direct3D11.ResourceUsage.Staging;
                stagingDesc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;
                using (var stagingBuffer = new SharpDX.Direct3D11.Buffer(GraphicsDevice._d3dDevice, stagingDesc))
                {
                    lock (GraphicsDevice._d3dContext)
                        deviceContext.CopyResource(_buffer, stagingBuffer);

                    int TsizeInBytes = SharpDX.Utilities.SizeOf<T>();
                    var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    try
                    {
                        var startBytes = startIndex * vertexStride;
                        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                        lock (GraphicsDevice._d3dContext)
                        {
                            // Map the staging resource to a CPU accessible memory
                            var box = deviceContext.MapSubresource(stagingBuffer, 0, SharpDX.Direct3D11.MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                            if (vertexStride == TsizeInBytes)
                            {
                                SharpDX.Utilities.CopyMemory(dataPtr, box.DataPointer + offsetInBytes, vertexStride * elementCount);
                            }
                            else
                            {
                                for (int i = 0; i < elementCount; i++)
                                    SharpDX.Utilities.CopyMemory(dataPtr + i * TsizeInBytes, box.DataPointer + i * vertexStride + offsetInBytes, TsizeInBytes);
                            }

                            // Make sure that we unmap the resource in case of an exception
                            deviceContext.UnmapSubresource(stagingBuffer, 0);
                        }
                    }
                    finally
                    {
                        dataHandle.Free();
                    }
                }
            }
        }

        private void PlatformSetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes) where T : struct
        {
            GenerateIfRequired();

            if (_isDynamic)
            {
                // We assume discard by default.
                var mode = SharpDX.Direct3D11.MapMode.WriteDiscard;
                if ((options & SetDataOptions.NoOverwrite) == SetDataOptions.NoOverwrite)
                    mode = SharpDX.Direct3D11.MapMode.WriteNoOverwrite;

                var d3dContext = GraphicsDevice._d3dContext;
                lock (d3dContext)
                {
                    var dataBox = d3dContext.MapSubresource(_buffer, 0, mode, SharpDX.Direct3D11.MapFlags.None);
                    if (vertexStride == elementSizeInBytes)
					{
                        SharpDX.Utilities.Write(dataBox.DataPointer + offsetInBytes, data, startIndex, elementCount);
                    }
                    else
                    {
                        for (int i = 0; i < elementCount; i++)
                            SharpDX.Utilities.Write(dataBox.DataPointer + offsetInBytes + i * vertexStride, data, startIndex + i, 1);
                    }

                    d3dContext.UnmapSubresource(_buffer, 0);
                }
            }
            else
            {
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    var startBytes = startIndex * elementSizeInBytes;
                    var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                    var d3dContext = GraphicsDevice._d3dContext;

                    if (vertexStride == elementSizeInBytes)
                    {
                        var box = new SharpDX.DataBox(dataPtr, 1, 0);

                        var region = new SharpDX.Direct3D11.ResourceRegion();
                        region.Top = 0;
                        region.Front = 0;
                        region.Back = 1;
                        region.Bottom = 1;
                        region.Left = offsetInBytes;
                        region.Right = offsetInBytes + (elementCount * elementSizeInBytes);

                        lock (d3dContext)
                            d3dContext.UpdateSubresource(box, _buffer, 0, region);
                    }
                    else
                    {
                        // Copy the buffer to a staging resource, so that any elements we don't write to will still be correct.
                        var stagingDesc = _buffer.Description;
                        stagingDesc.BindFlags = SharpDX.Direct3D11.BindFlags.None;
                        stagingDesc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read | SharpDX.Direct3D11.CpuAccessFlags.Write;
                        stagingDesc.Usage = SharpDX.Direct3D11.ResourceUsage.Staging;
                        stagingDesc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;
                        using (var stagingBuffer = new SharpDX.Direct3D11.Buffer(GraphicsDevice._d3dDevice, stagingDesc))
                        {
                            lock (d3dContext)
                            {
                                d3dContext.CopyResource(_buffer, stagingBuffer);

                                // Map the staging resource to a CPU accessible memory
                                var box = d3dContext.MapSubresource(stagingBuffer, 0, SharpDX.Direct3D11.MapMode.Read,
                                    SharpDX.Direct3D11.MapFlags.None);

                                for (int i = 0; i < elementCount; i++)
                                    SharpDX.Utilities.CopyMemory(
                                        box.DataPointer + i * vertexStride + offsetInBytes,
                                        dataPtr + i * elementSizeInBytes, elementSizeInBytes);

                                // Make sure that we unmap the resource in case of an exception
                                d3dContext.UnmapSubresource(stagingBuffer, 0);

                                // Copy back from staging resource to real buffer.
                                d3dContext.CopyResource(stagingBuffer, _buffer);
                            }
                        }
                    }
                }
                finally
                {
                    dataHandle.Free();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                SharpDX.Utilities.Dispose(ref _buffer);

            base.Dispose(disposing);
        }
    }
}
