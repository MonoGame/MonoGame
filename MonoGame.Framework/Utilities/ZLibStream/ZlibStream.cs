// ZlibStream.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa and Microsoft Corporation.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs):
// Time-stamp: <2011-July-31 14:53:33>
//
// ------------------------------------------------------------------
//
// This module defines the ZlibStream class, which is similar in idea to
// the System.IO.Compression.DeflateStream and
// System.IO.Compression.GZipStream classes in the .NET BCL.
//
// ------------------------------------------------------------------

// The following notice applies to jzlib:
// -----------------------------------------------------------------------------

// Copyright (c) 2000,2001,2002,2003 ymnk, JCraft,Inc. All rights reserved.

// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:

// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.

// 2. Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in
// the documentation and/or other materials provided with the distribution.

// 3. The names of the authors may not be used to endorse or promote products
// derived from this software without specific prior written permission.

// THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
// INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// -----------------------------------------------------------------------------

// jzlib is based on zlib-1.1.3.

// The following notice applies to zlib:

// -----------------------------------------------------------------------------

// Copyright (C) 1995-2004 Jean-loup Gailly and Mark Adler

// The ZLIB software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.

// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:

// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

// Jean-loup Gailly jloup@gzip.org
// Mark Adler madler@alumni.caltech.edu


//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MonoGame.Utilities
{

    /// <summary>
    /// Represents a Zlib stream for compression or decompression.
    /// </summary>
    /// <remarks>
    ///
    /// <para>
    /// The ZlibStream is a <see
    /// href="http://en.wikipedia.org/wiki/Decorator_pattern">Decorator</see> on a <see
    /// cref="System.IO.Stream"/>.  It adds ZLIB compression or decompression to any
    /// stream.
    /// </para>
    ///
    /// <para> Using this stream, applications can compress or decompress data via
    /// stream <c>Read()</c> and <c>Write()</c> operations.  Either compression or
    /// decompression can occur through either reading or writing. The compression
    /// format used is ZLIB, which is documented in <see
    /// href="http://www.ietf.org/rfc/rfc1950.txt">IETF RFC 1950</see>, "ZLIB Compressed
    /// Data Format Specification version 3.3". This implementation of ZLIB always uses
    /// DEFLATE as the compression method.  (see <see
    /// href="http://www.ietf.org/rfc/rfc1951.txt">IETF RFC 1951</see>, "DEFLATE
    /// Compressed Data Format Specification version 1.3.") </para>
    ///
    /// <para>
    /// The ZLIB format allows for varying compression methods, window sizes, and dictionaries.
    /// This implementation always uses the DEFLATE compression method, a preset dictionary,
    /// and 15 window bits by default.
    /// </para>
    ///
    /// <para>
    /// This class is similar to DeflateStream, except that it adds the
    /// RFC1950 header and trailer bytes to a compressed stream when compressing, or expects
    /// the RFC1950 header and trailer bytes when decompressing.  It is also similar to the
    /// <see cref="GZipStream"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GZipStream" />
    public class ZlibStream : Stream
    {
        internal ZlibBaseStream _baseStream;
        bool _disposed;

        /// <summary>
        /// Create a <c>ZlibStream</c> using the specified <c>CompressionMode</c>.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   When mode is <c>CompressionMode.Compress</c>, the <c>ZlibStream</c>
        ///   will use the default compression level. The "captive" stream will be
        ///   closed when the <c>ZlibStream</c> is closed.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        /// This example uses a <c>ZlibStream</c> to compress a file, and writes the
        /// compressed data to another file.
        /// <code>
        /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        /// {
        ///     using (var raw = System.IO.File.Create(fileToCompress + ".zlib"))
        ///     {
        ///         using (Stream compressor = new ZlibStream(raw, CompressionMode.Compress))
        ///         {
        ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        ///             int n;
        ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
        ///             {
        ///                 compressor.Write(buffer, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// <code lang="VB">
        /// Using input As Stream = File.OpenRead(fileToCompress)
        ///     Using raw As FileStream = File.Create(fileToCompress &amp; ".zlib")
        ///     Using compressor As Stream = New ZlibStream(raw, CompressionMode.Compress)
        ///         Dim buffer As Byte() = New Byte(4096) {}
        ///         Dim n As Integer = -1
        ///         Do While (n &lt;&gt; 0)
        ///             If (n &gt; 0) Then
        ///                 compressor.Write(buffer, 0, n)
        ///             End If
        ///             n = input.Read(buffer, 0, buffer.Length)
        ///         Loop
        ///     End Using
        ///     End Using
        /// End Using
        /// </code>
        /// </example>
        ///
        /// <param name="stream">The stream which will be read or written.</param>
        /// <param name="mode">Indicates whether the ZlibStream will compress or decompress.</param>
        public ZlibStream(System.IO.Stream stream, CompressionMode mode)
            : this(stream, mode, CompressionLevel.Default, false)
        {
        }

        /// <summary>
        ///   Create a <c>ZlibStream</c> using the specified <c>CompressionMode</c> and
        ///   the specified <c>CompressionLevel</c>.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   When mode is <c>CompressionMode.Decompress</c>, the level parameter is ignored.
        ///   The "captive" stream will be closed when the <c>ZlibStream</c> is closed.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///   This example uses a <c>ZlibStream</c> to compress data from a file, and writes the
        ///   compressed data to another file.
        ///
        /// <code>
        /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        /// {
        ///     using (var raw = System.IO.File.Create(fileToCompress + ".zlib"))
        ///     {
        ///         using (Stream compressor = new ZlibStream(raw,
        ///                                                   CompressionMode.Compress,
        ///                                                   CompressionLevel.BestCompression))
        ///         {
        ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        ///             int n;
        ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
        ///             {
        ///                 compressor.Write(buffer, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Using input As Stream = File.OpenRead(fileToCompress)
        ///     Using raw As FileStream = File.Create(fileToCompress &amp; ".zlib")
        ///         Using compressor As Stream = New ZlibStream(raw, CompressionMode.Compress, CompressionLevel.BestCompression)
        ///             Dim buffer As Byte() = New Byte(4096) {}
        ///             Dim n As Integer = -1
        ///             Do While (n &lt;&gt; 0)
        ///                 If (n &gt; 0) Then
        ///                     compressor.Write(buffer, 0, n)
        ///                 End If
        ///                 n = input.Read(buffer, 0, buffer.Length)
        ///             Loop
        ///         End Using
        ///     End Using
        /// End Using
        /// </code>
        /// </example>
        ///
        /// <param name="stream">The stream to be read or written while deflating or inflating.</param>
        /// <param name="mode">Indicates whether the ZlibStream will compress or decompress.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        public ZlibStream(System.IO.Stream stream, CompressionMode mode, CompressionLevel level)
            : this(stream, mode, level, false)
        {
        }

        /// <summary>
        ///   Create a <c>ZlibStream</c> using the specified <c>CompressionMode</c>, and
        ///   explicitly specify whether the captive stream should be left open after
        ///   Deflation or Inflation.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   When mode is <c>CompressionMode.Compress</c>, the <c>ZlibStream</c> will use
        ///   the default compression level.
        /// </para>
        ///
        /// <para>
        ///   This constructor allows the application to request that the captive stream
        ///   remain open after the deflation or inflation occurs.  By default, after
        ///   <c>Close()</c> is called on the stream, the captive stream is also
        ///   closed. In some cases this is not desired, for example if the stream is a
        ///   <see cref="System.IO.MemoryStream"/> that will be re-read after
        ///   compression.  Specify true for the <paramref name="leaveOpen"/> parameter to leave the stream
        ///   open.
        /// </para>
        ///
        /// <para>
        /// See the other overloads of this constructor for example code.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="stream">The stream which will be read or written. This is called the
        /// "captive" stream in other places in this documentation.</param>
        /// <param name="mode">Indicates whether the ZlibStream will compress or decompress.</param>
        /// <param name="leaveOpen">true if the application would like the stream to remain
        /// open after inflation/deflation.</param>
        public ZlibStream(System.IO.Stream stream, CompressionMode mode, bool leaveOpen)
            : this(stream, mode, CompressionLevel.Default, leaveOpen)
        {
        }

        /// <summary>
        ///   Create a <c>ZlibStream</c> using the specified <c>CompressionMode</c>
        ///   and the specified <c>CompressionLevel</c>, and explicitly specify
        ///   whether the stream should be left open after Deflation or Inflation.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   This constructor allows the application to request that the captive
        ///   stream remain open after the deflation or inflation occurs.  By
        ///   default, after <c>Close()</c> is called on the stream, the captive
        ///   stream is also closed. In some cases this is not desired, for example
        ///   if the stream is a <see cref="System.IO.MemoryStream"/> that will be
        ///   re-read after compression.  Specify true for the <paramref
        ///   name="leaveOpen"/> parameter to leave the stream open.
        /// </para>
        ///
        /// <para>
        ///   When mode is <c>CompressionMode.Decompress</c>, the level parameter is
        ///   ignored.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///
        /// This example shows how to use a ZlibStream to compress the data from a file,
        /// and store the result into another file. The filestream remains open to allow
        /// additional data to be written to it.
        ///
        /// <code>
        /// using (var output = System.IO.File.Create(fileToCompress + ".zlib"))
        /// {
        ///     using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        ///     {
        ///         using (Stream compressor = new ZlibStream(output, CompressionMode.Compress, CompressionLevel.BestCompression, true))
        ///         {
        ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        ///             int n;
        ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
        ///             {
        ///                 compressor.Write(buffer, 0, n);
        ///             }
        ///         }
        ///     }
        ///     // can write additional data to the output stream here
        /// }
        /// </code>
        /// <code lang="VB">
        /// Using output As FileStream = File.Create(fileToCompress &amp; ".zlib")
        ///     Using input As Stream = File.OpenRead(fileToCompress)
        ///         Using compressor As Stream = New ZlibStream(output, CompressionMode.Compress, CompressionLevel.BestCompression, True)
        ///             Dim buffer As Byte() = New Byte(4096) {}
        ///             Dim n As Integer = -1
        ///             Do While (n &lt;&gt; 0)
        ///                 If (n &gt; 0) Then
        ///                     compressor.Write(buffer, 0, n)
        ///                 End If
        ///                 n = input.Read(buffer, 0, buffer.Length)
        ///             Loop
        ///         End Using
        ///     End Using
        ///     ' can write additional data to the output stream here.
        /// End Using
        /// </code>
        /// </example>
        ///
        /// <param name="stream">The stream which will be read or written.</param>
        ///
        /// <param name="mode">Indicates whether the ZlibStream will compress or decompress.</param>
        ///
        /// <param name="leaveOpen">
        /// true if the application would like the stream to remain open after
        /// inflation/deflation.
        /// </param>
        ///
        /// <param name="level">
        /// A tuning knob to trade speed for effectiveness. This parameter is
        /// effective only when mode is <c>CompressionMode.Compress</c>.
        /// </param>
        public ZlibStream(System.IO.Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            _baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.ZLIB, leaveOpen);
        }

        #region Zlib properties

        /// <summary>
        /// This property sets the flush behavior on the stream.
        /// Sorry, though, not sure exactly how to describe all the various settings.
        /// </summary>
        virtual public FlushType FlushMode
        {
            get { return (this._baseStream._flushMode); }
            set
            {
                if (_disposed) throw new ObjectDisposedException("ZlibStream");
                this._baseStream._flushMode = value;
            }
        }

        /// <summary>
        ///   The size of the working buffer for the compression codec.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The working buffer is used for all stream operations.  The default size is
        ///   1024 bytes. The minimum size is 128 bytes. You may get better performance
        ///   with a larger buffer.  Then again, you might not.  You would have to test
        ///   it.
        /// </para>
        ///
        /// <para>
        ///   Set this before the first call to <c>Read()</c> or <c>Write()</c> on the
        ///   stream. If you try to set it afterwards, it will throw.
        /// </para>
        /// </remarks>
        public int BufferSize
        {
            get
            {
                return this._baseStream._bufferSize;
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("ZlibStream");
                if (this._baseStream._workingBuffer != null)
                    throw new ZlibException("The working buffer is already set.");
                if (value < ZlibConstants.WorkingBufferSizeMin)
                    throw new ZlibException(String.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, ZlibConstants.WorkingBufferSizeMin));
                this._baseStream._bufferSize = value;
            }
        }

        /// <summary> Returns the total number of bytes input so far.</summary>
        virtual public long TotalIn
        {
            get { return this._baseStream._z.TotalBytesIn; }
        }

        /// <summary> Returns the total number of bytes output so far.</summary>
        virtual public long TotalOut
        {
            get { return this._baseStream._z.TotalBytesOut; }
        }

        #endregion

        #region System.IO.Stream methods

        /// <summary>
        ///   Dispose the stream.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This may or may not result in a <c>Close()</c> call on the captive
        ///     stream.  See the constructors that have a <c>leaveOpen</c> parameter
        ///     for more information.
        ///   </para>
        ///   <para>
        ///     This method may be invoked in two distinct scenarios.  If disposing
        ///     == true, the method has been called directly or indirectly by a
        ///     user's code, for example via the public Dispose() method. In this
        ///     case, both managed and unmanaged resources can be referenced and
        ///     disposed.  If disposing == false, the method has been called by the
        ///     runtime from inside the object finalizer and this method should not
        ///     reference other objects; in that case only unmanaged resources must
        ///     be referenced or disposed.
        ///   </para>
        /// </remarks>
        /// <param name="disposing">
        ///   indicates whether the Dispose method was invoked by user code.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!_disposed)
                {
                    if (disposing && (this._baseStream != null))
                        this._baseStream.Dispose();
                    _disposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }


        /// <summary>
        /// Indicates whether the stream can be read.
        /// </summary>
        /// <remarks>
        /// The return value depends on whether the captive stream supports reading.
        /// </remarks>
        public override bool CanRead
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("ZlibStream");
                return _baseStream._stream.CanRead;
            }
        }

        /// <summary>
        /// Indicates whether the stream supports Seek operations.
        /// </summary>
        /// <remarks>
        /// Always returns false.
        /// </remarks>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Indicates whether the stream can be written.
        /// </summary>
        /// <remarks>
        /// The return value depends on whether the captive stream supports writing.
        /// </remarks>
        public override bool CanWrite
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("ZlibStream");
                return _baseStream._stream.CanWrite;
            }
        }

        /// <summary>
        /// Flush the stream.
        /// </summary>
        public override void Flush()
        {
            if (_disposed) throw new ObjectDisposedException("ZlibStream");
            _baseStream.Flush();
        }

        /// <summary>
        /// Finish and flush.
        /// TODO: shouldn't Flush just do this all the time?
        /// </summary>
        public void Finish()
        {
            _baseStream.Finish();
            Flush();
        }

        /// <summary>
        /// Reading this property always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        ///   The position of the stream pointer.
        /// </summary>
        ///
        /// <remarks>
        ///   Setting this property always throws a <see
        ///   cref="NotSupportedException"/>. Reading will return the total bytes
        ///   written out, if used in writing, or the total bytes read in, if used in
        ///   reading.  The count may refer to compressed bytes or uncompressed bytes,
        ///   depending on how you've used the stream.
        /// </remarks>
        public override long Position
        {
            get
            {
                if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
                    return this._baseStream._z.TotalBytesOut;
                if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
                    return this._baseStream._z.TotalBytesIn;
                return 0;
            }

            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Read data from the stream.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   If you wish to use the <c>ZlibStream</c> to compress data while reading,
        ///   you can create a <c>ZlibStream</c> with <c>CompressionMode.Compress</c>,
        ///   providing an uncompressed data stream.  Then call <c>Read()</c> on that
        ///   <c>ZlibStream</c>, and the data read will be compressed.  If you wish to
        ///   use the <c>ZlibStream</c> to decompress data while reading, you can create
        ///   a <c>ZlibStream</c> with <c>CompressionMode.Decompress</c>, providing a
        ///   readable compressed data stream.  Then call <c>Read()</c> on that
        ///   <c>ZlibStream</c>, and the data will be decompressed as it is read.
        /// </para>
        ///
        /// <para>
        ///   A <c>ZlibStream</c> can be used for <c>Read()</c> or <c>Write()</c>, but
        ///   not both.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="buffer">
        /// The buffer into which the read data should be placed.</param>
        ///
        /// <param name="offset">
        /// the offset within that data array to put the first byte read.</param>
        ///
        /// <param name="count">the number of bytes to read.</param>
        ///
        /// <returns>the number of bytes read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException("ZlibStream");
            return _baseStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Calling this method always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="offset">
        ///   The offset to seek to....
        ///   IF THIS METHOD ACTUALLY DID ANYTHING.
        /// </param>
        /// <param name="origin">
        ///   The reference specifying how to apply the offset....  IF
        ///   THIS METHOD ACTUALLY DID ANYTHING.
        /// </param>
        ///
        /// <returns>nothing. This method always throws.</returns>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Calling this method always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="value">
        ///   The new value for the stream length....  IF
        ///   THIS METHOD ACTUALLY DID ANYTHING.
        /// </param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Write data to the stream.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   If you wish to use the <c>ZlibStream</c> to compress data while writing,
        ///   you can create a <c>ZlibStream</c> with <c>CompressionMode.Compress</c>,
        ///   and a writable output stream.  Then call <c>Write()</c> on that
        ///   <c>ZlibStream</c>, providing uncompressed data as input.  The data sent to
        ///   the output stream will be the compressed form of the data written.  If you
        ///   wish to use the <c>ZlibStream</c> to decompress data while writing, you
        ///   can create a <c>ZlibStream</c> with <c>CompressionMode.Decompress</c>, and a
        ///   writable output stream.  Then call <c>Write()</c> on that stream,
        ///   providing previously compressed data. The data sent to the output stream
        ///   will be the decompressed form of the data written.
        /// </para>
        ///
        /// <para>
        ///   A <c>ZlibStream</c> can be used for <c>Read()</c> or <c>Write()</c>, but not both.
        /// </para>
        /// </remarks>
        /// <param name="buffer">The buffer holding data to write to the stream.</param>
        /// <param name="offset">the offset within that data array to find the first byte to write.</param>
        /// <param name="count">the number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException("ZlibStream");
            _baseStream.Write(buffer, offset, count);
        }
        #endregion


        /// <summary>
        ///   Compress a string into a byte array using ZLIB.
        /// </summary>
        ///
        /// <remarks>
        ///   Uncompress it with <see cref="ZlibStream.UncompressString(byte[])"/>.
        /// </remarks>
        ///
        /// <seealso cref="ZlibStream.UncompressString(byte[])"/>
        /// <seealso cref="ZlibStream.CompressBuffer(byte[])"/>
        /// <seealso cref="GZipStream.CompressString(string)"/>
        ///
        /// <param name="s">
        ///   A string to compress.  The string will first be encoded
        ///   using UTF8, then compressed.
        /// </param>
        ///
        /// <returns>The string in compressed form</returns>
        public static byte[] CompressString(String s)
        {
            using (var ms = new MemoryStream())
            {
                Stream compressor =
                    new ZlibStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZlibBaseStream.CompressString(s, compressor);
                return ms.ToArray();
            }
        }


        /// <summary>
        ///   Compress a byte array into a new byte array using ZLIB.
        /// </summary>
        ///
        /// <remarks>
        ///   Uncompress it with <see cref="ZlibStream.UncompressBuffer(byte[])"/>.
        /// </remarks>
        ///
        /// <seealso cref="ZlibStream.CompressString(string)"/>
        /// <seealso cref="ZlibStream.UncompressBuffer(byte[])"/>
        ///
        /// <param name="b">
        /// A buffer to compress.
        /// </param>
        ///
        /// <returns>The data in compressed form</returns>
        public static byte[] CompressBuffer(byte[] b)
        {
            using (var ms = new MemoryStream())
            {
                Stream compressor =
                    new ZlibStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);

                ZlibBaseStream.CompressBuffer(b, compressor);
                return ms.ToArray();
            }
        }


        /// <summary>
        ///   Uncompress a ZLIB-compressed byte array into a single string.
        /// </summary>
        ///
        /// <seealso cref="ZlibStream.CompressString(String)"/>
        /// <seealso cref="ZlibStream.UncompressBuffer(byte[])"/>
        ///
        /// <param name="compressed">
        ///   A buffer containing ZLIB-compressed data.
        /// </param>
        ///
        /// <returns>The uncompressed string</returns>
        public static String UncompressString(byte[] compressed)
        {
            using (var input = new MemoryStream(compressed))
            {
                Stream decompressor =
                    new ZlibStream(input, CompressionMode.Decompress);

                return ZlibBaseStream.UncompressString(compressed, decompressor);
            }
        }


        /// <summary>
        ///   Uncompress a ZLIB-compressed byte array into a byte array.
        /// </summary>
        ///
        /// <seealso cref="ZlibStream.CompressBuffer(byte[])"/>
        /// <seealso cref="ZlibStream.UncompressString(byte[])"/>
        ///
        /// <param name="compressed">
        ///   A buffer containing ZLIB-compressed data.
        /// </param>
        ///
        /// <returns>The data in uncompressed form</returns>
        public static byte[] UncompressBuffer(byte[] compressed)
        {
            using (var input = new MemoryStream(compressed))
            {
                Stream decompressor =
                    new ZlibStream(input, CompressionMode.Decompress);

                return ZlibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }

    }

    /// <summary>
    /// A bunch of constants used in the Zlib interface.
    /// </summary>
    internal static class ZlibConstants
    {
        /// <summary>
        /// The maximum number of window bits for the Deflate algorithm.
        /// </summary>
        internal const int WindowBitsMax = 15; // 32K LZ77 window

        /// <summary>
        /// The default number of window bits for the Deflate algorithm.
        /// </summary>
        internal const int WindowBitsDefault = WindowBitsMax;

        /// <summary>
        /// indicates everything is A-OK
        /// </summary>
        internal const int Z_OK = 0;

        /// <summary>
        /// Indicates that the last operation reached the end of the stream.
        /// </summary>
        internal const int Z_STREAM_END = 1;

        /// <summary>
        /// The operation ended in need of a dictionary. 
        /// </summary>
        internal const int Z_NEED_DICT = 2;

        /// <summary>
        /// There was an error with the stream - not enough data, not open and readable, etc.
        /// </summary>
        internal const int Z_STREAM_ERROR = -2;

        /// <summary>
        /// There was an error with the data - not enough data, bad data, etc.
        /// </summary>
        internal const int Z_DATA_ERROR = -3;

        /// <summary>
        /// There was an error with the working buffer.
        /// </summary>
        internal const int Z_BUF_ERROR = -5;

        /// <summary>
        /// The size of the working buffer used in the ZlibCodec class. Defaults to 8192 bytes.
        /// </summary>
#if NETCF        
        internal const int WorkingBufferSizeDefault = 8192;
#else
        internal const int WorkingBufferSizeDefault = 16384;
#endif
        /// <summary>
        /// The minimum size of the working buffer used in the ZlibCodec class.  Currently it is 128 bytes.
        /// </summary>
        internal const int WorkingBufferSizeMin = 1024;
    }

    /// <summary>
    /// Encoder and Decoder for ZLIB and DEFLATE (IETF RFC1950 and RFC1951).
    /// </summary>
    ///
    /// <remarks>
    /// This class compresses and decompresses data according to the Deflate algorithm
    /// and optionally, the ZLIB format, as documented in <see
    /// href="http://www.ietf.org/rfc/rfc1950.txt">RFC 1950 - ZLIB</see> and <see
    /// href="http://www.ietf.org/rfc/rfc1951.txt">RFC 1951 - DEFLATE</see>.
    /// </remarks>

    sealed internal class ZlibCodec
    {
        /// <summary>
        /// The buffer from which data is taken.
        /// </summary>
        internal byte[] InputBuffer;

        /// <summary>
        /// An index into the InputBuffer array, indicating where to start reading. 
        /// </summary>
        internal int NextIn;

        /// <summary>
        /// The number of bytes available in the InputBuffer, starting at NextIn. 
        /// </summary>
        /// <remarks>
        /// Generally you should set this to InputBuffer.Length before the first Inflate() or Deflate() call. 
        /// The class will update this number as calls to Inflate/Deflate are made.
        /// </remarks>
        internal int AvailableBytesIn;

        /// <summary>
        /// Total number of bytes read so far, through all calls to Inflate()/Deflate().
        /// </summary>
        internal long TotalBytesIn;

        /// <summary>
        /// Buffer to store output data.
        /// </summary>
        internal byte[] OutputBuffer;

        /// <summary>
        /// An index into the OutputBuffer array, indicating where to start writing. 
        /// </summary>
        internal int NextOut;

        /// <summary>
        /// The number of bytes available in the OutputBuffer, starting at NextOut. 
        /// </summary>
        /// <remarks>
        /// Generally you should set this to OutputBuffer.Length before the first Inflate() or Deflate() call. 
        /// The class will update this number as calls to Inflate/Deflate are made.
        /// </remarks>
        internal int AvailableBytesOut;

        /// <summary>
        /// Total number of bytes written to the output so far, through all calls to Inflate()/Deflate().
        /// </summary>
        internal long TotalBytesOut;

        /// <summary>
        /// used for diagnostics, when something goes wrong!
        /// </summary>
        internal System.String Message;

        internal DeflateManager dstate;
        internal InflateManager istate;

        internal uint _Adler32;

        /// <summary>
        /// The compression level to use in this codec.  Useful only in compression mode.
        /// </summary>
        internal CompressionLevel CompressLevel = CompressionLevel.Default;

        /// <summary>
        /// The number of Window Bits to use.  
        /// </summary>
        /// <remarks>
        /// This gauges the size of the sliding window, and hence the 
        /// compression effectiveness as well as memory consumption. It's best to just leave this 
        /// setting alone if you don't know what it is.  The maximum value is 15 bits, which implies
        /// a 32k window.  
        /// </remarks>
        internal int WindowBits = ZlibConstants.WindowBitsDefault;

        /// <summary>
        /// The compression strategy to use.
        /// </summary>
        /// <remarks>
        /// This is only effective in compression.  The theory offered by ZLIB is that different
        /// strategies could potentially produce significant differences in compression behavior
        /// for different data sets.  Unfortunately I don't have any good recommendations for how
        /// to set it differently.  When I tested changing the strategy I got minimally different
        /// compression performance. It's best to leave this property alone if you don't have a
        /// good feel for it.  Or, you may want to produce a test harness that runs through the
        /// different strategy options and evaluates them on different file types. If you do that,
        /// let me know your results.
        /// </remarks>
        internal CompressionStrategy Strategy = CompressionStrategy.Default;


        /// <summary>
        /// The Adler32 checksum on the data transferred through the codec so far. You probably don't need to look at this.
        /// </summary>
        internal int Adler32 { get { return (int)_Adler32; } }


        /// <summary>
        /// Create a ZlibCodec.
        /// </summary>
        /// <remarks>
        /// If you use this default constructor, you will later have to explicitly call 
        /// InitializeInflate() or InitializeDeflate() before using the ZlibCodec to compress 
        /// or decompress. 
        /// </remarks>
        internal ZlibCodec() { }

        /// <summary>
        /// Create a ZlibCodec that either compresses or decompresses.
        /// </summary>
        /// <param name="mode">
        /// Indicates whether the codec should compress (deflate) or decompress (inflate).
        /// </param>
        internal ZlibCodec(CompressionMode mode)
        {
            if (mode == CompressionMode.Compress)
            {
                int rc = InitializeDeflate();
                if (rc != ZlibConstants.Z_OK) throw new ZlibException("Cannot initialize for deflate.");
            }
            else if (mode == CompressionMode.Decompress)
            {
                int rc = InitializeInflate();
                if (rc != ZlibConstants.Z_OK) throw new ZlibException("Cannot initialize for inflate.");
            }
            else throw new ZlibException("Invalid ZlibStreamFlavor.");
        }

        /// <summary>
        /// Initialize the inflation state. 
        /// </summary>
        /// <remarks>
        /// It is not necessary to call this before using the ZlibCodec to inflate data; 
        /// It is implicitly called when you call the constructor.
        /// </remarks>
        /// <returns>Z_OK if everything goes well.</returns>
        internal int InitializeInflate()
        {
            return InitializeInflate(this.WindowBits);
        }

        /// <summary>
        /// Initialize the inflation state with an explicit flag to
        /// govern the handling of RFC1950 header bytes.
        /// </summary>
        ///
        /// <remarks>
        /// By default, the ZLIB header defined in <see
        /// href="http://www.ietf.org/rfc/rfc1950.txt">RFC 1950</see> is expected.  If
        /// you want to read a zlib stream you should specify true for
        /// expectRfc1950Header.  If you have a deflate stream, you will want to specify
        /// false. It is only necessary to invoke this initializer explicitly if you
        /// want to specify false.
        /// </remarks>
        ///
        /// <param name="expectRfc1950Header">whether to expect an RFC1950 header byte
        /// pair when reading the stream of data to be inflated.</param>
        ///
        /// <returns>Z_OK if everything goes well.</returns>
        internal int InitializeInflate(bool expectRfc1950Header)
        {
            return InitializeInflate(this.WindowBits, expectRfc1950Header);
        }

        /// <summary>
        /// Initialize the ZlibCodec for inflation, with the specified number of window bits. 
        /// </summary>
        /// <param name="windowBits">The number of window bits to use. If you need to ask what that is, 
        /// then you shouldn't be calling this initializer.</param>
        /// <returns>Z_OK if all goes well.</returns>
        internal int InitializeInflate(int windowBits)
        {
            this.WindowBits = windowBits;
            return InitializeInflate(windowBits, true);
        }

        /// <summary>
        /// Initialize the inflation state with an explicit flag to govern the handling of
        /// RFC1950 header bytes. 
        /// </summary>
        ///
        /// <remarks>
        /// If you want to read a zlib stream you should specify true for
        /// expectRfc1950Header. In this case, the library will expect to find a ZLIB
        /// header, as defined in <see href="http://www.ietf.org/rfc/rfc1950.txt">RFC
        /// 1950</see>, in the compressed stream.  If you will be reading a DEFLATE or
        /// GZIP stream, which does not have such a header, you will want to specify
        /// false.
        /// </remarks>
        ///
        /// <param name="expectRfc1950Header">whether to expect an RFC1950 header byte pair when reading 
        /// the stream of data to be inflated.</param>
        /// <param name="windowBits">The number of window bits to use. If you need to ask what that is, 
        /// then you shouldn't be calling this initializer.</param>
        /// <returns>Z_OK if everything goes well.</returns>
        internal int InitializeInflate(int windowBits, bool expectRfc1950Header)
        {
            this.WindowBits = windowBits;
            if (dstate != null) throw new ZlibException("You may not call InitializeInflate() after calling InitializeDeflate().");
            istate = new InflateManager(expectRfc1950Header);
            return istate.Initialize(this, windowBits);
        }

        /// <summary>
        /// Inflate the data in the InputBuffer, placing the result in the OutputBuffer.
        /// </summary>
        /// <remarks>
        /// You must have set InputBuffer and OutputBuffer, NextIn and NextOut, and AvailableBytesIn and 
        /// AvailableBytesOut  before calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// private void InflateBuffer()
        /// {
        ///     int bufferSize = 1024;
        ///     byte[] buffer = new byte[bufferSize];
        ///     ZlibCodec decompressor = new ZlibCodec();
        /// 
        ///     Console.WriteLine("\n============================================");
        ///     Console.WriteLine("Size of Buffer to Inflate: {0} bytes.", CompressedBytes.Length);
        ///     MemoryStream ms = new MemoryStream(DecompressedBytes);
        /// 
        ///     int rc = decompressor.InitializeInflate();
        /// 
        ///     decompressor.InputBuffer = CompressedBytes;
        ///     decompressor.NextIn = 0;
        ///     decompressor.AvailableBytesIn = CompressedBytes.Length;
        /// 
        ///     decompressor.OutputBuffer = buffer;
        /// 
        ///     // pass 1: inflate 
        ///     do
        ///     {
        ///         decompressor.NextOut = 0;
        ///         decompressor.AvailableBytesOut = buffer.Length;
        ///         rc = decompressor.Inflate(FlushType.None);
        /// 
        ///         if (rc != ZlibConstants.Z_OK &amp;&amp; rc != ZlibConstants.Z_STREAM_END)
        ///             throw new Exception("inflating: " + decompressor.Message);
        /// 
        ///         ms.Write(decompressor.OutputBuffer, 0, buffer.Length - decompressor.AvailableBytesOut);
        ///     }
        ///     while (decompressor.AvailableBytesIn &gt; 0 || decompressor.AvailableBytesOut == 0);
        /// 
        ///     // pass 2: finish and flush
        ///     do
        ///     {
        ///         decompressor.NextOut = 0;
        ///         decompressor.AvailableBytesOut = buffer.Length;
        ///         rc = decompressor.Inflate(FlushType.Finish);
        /// 
        ///         if (rc != ZlibConstants.Z_STREAM_END &amp;&amp; rc != ZlibConstants.Z_OK)
        ///             throw new Exception("inflating: " + decompressor.Message);
        /// 
        ///         if (buffer.Length - decompressor.AvailableBytesOut &gt; 0)
        ///             ms.Write(buffer, 0, buffer.Length - decompressor.AvailableBytesOut);
        ///     }
        ///     while (decompressor.AvailableBytesIn &gt; 0 || decompressor.AvailableBytesOut == 0);
        /// 
        ///     decompressor.EndInflate();
        /// }
        ///
        /// </code>
        /// </example>
        /// <param name="flush">The flush to use when inflating.</param>
        /// <returns>Z_OK if everything goes well.</returns>
        internal int Inflate(FlushType flush)
        {
            if (istate == null)
                throw new ZlibException("No Inflate State!");
            return istate.Inflate(flush);
        }


        /// <summary>
        /// Ends an inflation session. 
        /// </summary>
        /// <remarks>
        /// Call this after successively calling Inflate().  This will cause all buffers to be flushed. 
        /// After calling this you cannot call Inflate() without a intervening call to one of the
        /// InitializeInflate() overloads.
        /// </remarks>
        /// <returns>Z_OK if everything goes well.</returns>
        internal int EndInflate()
        {
            if (istate == null)
                throw new ZlibException("No Inflate State!");
            int ret = istate.End();
            istate = null;
            return ret;
        }

        /// <summary>
        /// I don't know what this does!
        /// </summary>
        /// <returns>Z_OK if everything goes well.</returns>
        internal int SyncInflate()
        {
            if (istate == null)
                throw new ZlibException("No Inflate State!");
            return istate.Sync();
        }

        /// <summary>
        /// Initialize the ZlibCodec for deflation operation.
        /// </summary>
        /// <remarks>
        /// The codec will use the MAX window bits and the default level of compression.
        /// </remarks>
        /// <example>
        /// <code>
        ///  int bufferSize = 40000;
        ///  byte[] CompressedBytes = new byte[bufferSize];
        ///  byte[] DecompressedBytes = new byte[bufferSize];
        ///  
        ///  ZlibCodec compressor = new ZlibCodec();
        ///  
        ///  compressor.InitializeDeflate(CompressionLevel.Default);
        ///  
        ///  compressor.InputBuffer = System.Text.ASCIIEncoding.ASCII.GetBytes(TextToCompress);
        ///  compressor.NextIn = 0;
        ///  compressor.AvailableBytesIn = compressor.InputBuffer.Length;
        ///  
        ///  compressor.OutputBuffer = CompressedBytes;
        ///  compressor.NextOut = 0;
        ///  compressor.AvailableBytesOut = CompressedBytes.Length;
        ///  
        ///  while (compressor.TotalBytesIn != TextToCompress.Length &amp;&amp; compressor.TotalBytesOut &lt; bufferSize)
        ///  {
        ///    compressor.Deflate(FlushType.None);
        ///  }
        ///  
        ///  while (true)
        ///  {
        ///    int rc= compressor.Deflate(FlushType.Finish);
        ///    if (rc == ZlibConstants.Z_STREAM_END) break;
        ///  }
        ///  
        ///  compressor.EndDeflate();
        ///   
        /// </code>
        /// </example>
        /// <returns>Z_OK if all goes well. You generally don't need to check the return code.</returns>
        internal int InitializeDeflate()
        {
            return _InternalInitializeDeflate(true);
        }

        /// <summary>
        /// Initialize the ZlibCodec for deflation operation, using the specified CompressionLevel.
        /// </summary>
        /// <remarks>
        /// The codec will use the maximum window bits (15) and the specified
        /// CompressionLevel.  It will emit a ZLIB stream as it compresses.
        /// </remarks>
        /// <param name="level">The compression level for the codec.</param>
        /// <returns>Z_OK if all goes well.</returns>
        internal int InitializeDeflate(CompressionLevel level)
        {
            this.CompressLevel = level;
            return _InternalInitializeDeflate(true);
        }


        /// <summary>
        /// Initialize the ZlibCodec for deflation operation, using the specified CompressionLevel, 
        /// and the explicit flag governing whether to emit an RFC1950 header byte pair.
        /// </summary>
        /// <remarks>
        /// The codec will use the maximum window bits (15) and the specified CompressionLevel.
        /// If you want to generate a zlib stream, you should specify true for
        /// wantRfc1950Header. In this case, the library will emit a ZLIB
        /// header, as defined in <see href="http://www.ietf.org/rfc/rfc1950.txt">RFC
        /// 1950</see>, in the compressed stream.  
        /// </remarks>
        /// <param name="level">The compression level for the codec.</param>
        /// <param name="wantRfc1950Header">whether to emit an initial RFC1950 byte pair in the compressed stream.</param>
        /// <returns>Z_OK if all goes well.</returns>
        internal int InitializeDeflate(CompressionLevel level, bool wantRfc1950Header)
        {
            this.CompressLevel = level;
            return _InternalInitializeDeflate(wantRfc1950Header);
        }


        /// <summary>
        /// Initialize the ZlibCodec for deflation operation, using the specified CompressionLevel, 
        /// and the specified number of window bits. 
        /// </summary>
        /// <remarks>
        /// The codec will use the specified number of window bits and the specified CompressionLevel.
        /// </remarks>
        /// <param name="level">The compression level for the codec.</param>
        /// <param name="bits">the number of window bits to use.  If you don't know what this means, don't use this method.</param>
        /// <returns>Z_OK if all goes well.</returns>
        internal int InitializeDeflate(CompressionLevel level, int bits)
        {
            this.CompressLevel = level;
            this.WindowBits = bits;
            return _InternalInitializeDeflate(true);
        }

        /// <summary>
        /// Initialize the ZlibCodec for deflation operation, using the specified
        /// CompressionLevel, the specified number of window bits, and the explicit flag
        /// governing whether to emit an RFC1950 header byte pair.
        /// </summary>
        ///
        /// <param name="level">The compression level for the codec.</param>
        /// <param name="wantRfc1950Header">whether to emit an initial RFC1950 byte pair in the compressed stream.</param>
        /// <param name="bits">the number of window bits to use.  If you don't know what this means, don't use this method.</param>
        /// <returns>Z_OK if all goes well.</returns>
        internal int InitializeDeflate(CompressionLevel level, int bits, bool wantRfc1950Header)
        {
            this.CompressLevel = level;
            this.WindowBits = bits;
            return _InternalInitializeDeflate(wantRfc1950Header);
        }

        private int _InternalInitializeDeflate(bool wantRfc1950Header)
        {
            if (istate != null) throw new ZlibException("You may not call InitializeDeflate() after calling InitializeInflate().");
            dstate = new DeflateManager();
            dstate.WantRfc1950HeaderBytes = wantRfc1950Header;

            return dstate.Initialize(this, this.CompressLevel, this.WindowBits, this.Strategy);
        }

        /// <summary>
        /// Deflate one batch of data.
        /// </summary>
        /// <remarks>
        /// You must have set InputBuffer and OutputBuffer before calling this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// private void DeflateBuffer(CompressionLevel level)
        /// {
        ///     int bufferSize = 1024;
        ///     byte[] buffer = new byte[bufferSize];
        ///     ZlibCodec compressor = new ZlibCodec();
        /// 
        ///     Console.WriteLine("\n============================================");
        ///     Console.WriteLine("Size of Buffer to Deflate: {0} bytes.", UncompressedBytes.Length);
        ///     MemoryStream ms = new MemoryStream();
        /// 
        ///     int rc = compressor.InitializeDeflate(level);
        /// 
        ///     compressor.InputBuffer = UncompressedBytes;
        ///     compressor.NextIn = 0;
        ///     compressor.AvailableBytesIn = UncompressedBytes.Length;
        /// 
        ///     compressor.OutputBuffer = buffer;
        /// 
        ///     // pass 1: deflate 
        ///     do
        ///     {
        ///         compressor.NextOut = 0;
        ///         compressor.AvailableBytesOut = buffer.Length;
        ///         rc = compressor.Deflate(FlushType.None);
        /// 
        ///         if (rc != ZlibConstants.Z_OK &amp;&amp; rc != ZlibConstants.Z_STREAM_END)
        ///             throw new Exception("deflating: " + compressor.Message);
        /// 
        ///         ms.Write(compressor.OutputBuffer, 0, buffer.Length - compressor.AvailableBytesOut);
        ///     }
        ///     while (compressor.AvailableBytesIn &gt; 0 || compressor.AvailableBytesOut == 0);
        /// 
        ///     // pass 2: finish and flush
        ///     do
        ///     {
        ///         compressor.NextOut = 0;
        ///         compressor.AvailableBytesOut = buffer.Length;
        ///         rc = compressor.Deflate(FlushType.Finish);
        /// 
        ///         if (rc != ZlibConstants.Z_STREAM_END &amp;&amp; rc != ZlibConstants.Z_OK)
        ///             throw new Exception("deflating: " + compressor.Message);
        /// 
        ///         if (buffer.Length - compressor.AvailableBytesOut &gt; 0)
        ///             ms.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);
        ///     }
        ///     while (compressor.AvailableBytesIn &gt; 0 || compressor.AvailableBytesOut == 0);
        /// 
        ///     compressor.EndDeflate();
        /// 
        ///     ms.Seek(0, SeekOrigin.Begin);
        ///     CompressedBytes = new byte[compressor.TotalBytesOut];
        ///     ms.Read(CompressedBytes, 0, CompressedBytes.Length);
        /// }
        /// </code>
        /// </example>
        /// <param name="flush">whether to flush all data as you deflate. Generally you will want to 
        /// use Z_NO_FLUSH here, in a series of calls to Deflate(), and then call EndDeflate() to 
        /// flush everything. 
        /// </param>
        /// <returns>Z_OK if all goes well.</returns>
        internal int Deflate(FlushType flush)
        {
            if (dstate == null)
                throw new ZlibException("No Deflate State!");
            return dstate.Deflate(flush);
        }

        /// <summary>
        /// End a deflation session.
        /// </summary>
        /// <remarks>
        /// Call this after making a series of one or more calls to Deflate(). All buffers are flushed.
        /// </remarks>
        /// <returns>Z_OK if all goes well.</returns>
        internal int EndDeflate()
        {
            if (dstate == null)
                throw new ZlibException("No Deflate State!");
            // TODO: dinoch Tue, 03 Nov 2009  15:39 (test this)
            //int ret = dstate.End();
            dstate = null;
            return ZlibConstants.Z_OK; //ret;
        }

        /// <summary>
        /// Reset a codec for another deflation session.
        /// </summary>
        /// <remarks>
        /// Call this to reset the deflation state.  For example if a thread is deflating
        /// non-consecutive blocks, you can call Reset() after the Deflate(Sync) of the first
        /// block and before the next Deflate(None) of the second block.
        /// </remarks>
        /// <returns>Z_OK if all goes well.</returns>
        internal void ResetDeflate()
        {
            if (dstate == null)
                throw new ZlibException("No Deflate State!");
            dstate.Reset();
        }


        /// <summary>
        /// Set the CompressionStrategy and CompressionLevel for a deflation session.
        /// </summary>
        /// <param name="level">the level of compression to use.</param>
        /// <param name="strategy">the strategy to use for compression.</param>
        /// <returns>Z_OK if all goes well.</returns>
        internal int SetDeflateParams(CompressionLevel level, CompressionStrategy strategy)
        {
            if (dstate == null)
                throw new ZlibException("No Deflate State!");
            return dstate.SetParams(level, strategy);
        }


        /// <summary>
        /// Set the dictionary to be used for either Inflation or Deflation.
        /// </summary>
        /// <param name="dictionary">The dictionary bytes to use.</param>
        /// <returns>Z_OK if all goes well.</returns>
        internal int SetDictionary(byte[] dictionary)
        {
            if (istate != null)
                return istate.SetDictionary(dictionary);

            if (dstate != null)
                return dstate.SetDictionary(dictionary);

            throw new ZlibException("No Inflate or Deflate state!");
        }

        // Flush as much pending output as possible. All deflate() output goes
        // through this function so some applications may wish to modify it
        // to avoid allocating a large strm->next_out buffer and copying into it.
        // (See also read_buf()).
        internal void flush_pending()
        {
            int len = dstate.pendingCount;

            if (len > AvailableBytesOut)
                len = AvailableBytesOut;
            if (len == 0)
                return;

            if (dstate.pending.Length <= dstate.nextPending ||
                OutputBuffer.Length <= NextOut ||
                dstate.pending.Length < (dstate.nextPending + len) ||
                OutputBuffer.Length < (NextOut + len))
            {
                throw new ZlibException(String.Format("Invalid State. (pending.Length={0}, pendingCount={1})",
                    dstate.pending.Length, dstate.pendingCount));
            }

            Array.Copy(dstate.pending, dstate.nextPending, OutputBuffer, NextOut, len);

            NextOut += len;
            dstate.nextPending += len;
            TotalBytesOut += len;
            AvailableBytesOut -= len;
            dstate.pendingCount -= len;
            if (dstate.pendingCount == 0)
            {
                dstate.nextPending = 0;
            }
        }

        // Read a new buffer from the current input stream, update the adler32
        // and total number of bytes read.  All deflate() input goes through
        // this function so some applications may wish to modify it to avoid
        // allocating a large strm->next_in buffer and copying from it.
        // (See also flush_pending()).
        internal int read_buf(byte[] buf, int start, int size)
        {
            int len = AvailableBytesIn;

            if (len > size)
                len = size;
            if (len == 0)
                return 0;

            AvailableBytesIn -= len;

            if (dstate.WantRfc1950HeaderBytes)
            {
                _Adler32 = Adler.Adler32(_Adler32, InputBuffer, NextIn, len);
            }
            Array.Copy(InputBuffer, NextIn, buf, start, len);
            NextIn += len;
            TotalBytesIn += len;
            return len;
        }

    }

    internal enum ZlibStreamFlavor { ZLIB = 1950, DEFLATE = 1951, GZIP = 1952 }

    internal class ZlibBaseStream : System.IO.Stream
    {
        protected internal ZlibCodec _z = null; // deferred init... new ZlibCodec();

        protected internal StreamMode _streamMode = StreamMode.Undefined;
        protected internal FlushType _flushMode;
        protected internal ZlibStreamFlavor _flavor;
        protected internal CompressionMode _compressionMode;
        protected internal CompressionLevel _level;
        protected internal bool _leaveOpen;
        protected internal byte[] _workingBuffer;
        protected internal int _bufferSize = ZlibConstants.WorkingBufferSizeDefault;
        protected internal byte[] _buf1 = new byte[1];

        protected internal System.IO.Stream _stream;
        protected internal CompressionStrategy Strategy = CompressionStrategy.Default;

        // workitem 7159
        CRC32 crc;
        protected internal string _GzipFileName;
        protected internal string _GzipComment;
        protected internal DateTime _GzipMtime;
        protected internal int _gzipHeaderByteCount;

        internal int Crc32 { get { if (crc == null) return 0; return crc.Crc32Result; } }

        internal ZlibBaseStream(System.IO.Stream stream,
                              CompressionMode compressionMode,
                              CompressionLevel level,
                              ZlibStreamFlavor flavor,
                              bool leaveOpen)
            : base()
        {
            this._flushMode = FlushType.None;
            //this._workingBuffer = new byte[WORKING_BUFFER_SIZE_DEFAULT];
            this._stream = stream;
            this._leaveOpen = leaveOpen;
            this._compressionMode = compressionMode;
            this._flavor = flavor;
            this._level = level;
            // workitem 7159
            if (flavor == ZlibStreamFlavor.GZIP)
            {
                this.crc = new CRC32();
            }
        }


        protected internal bool _wantCompress
        {
            get
            {
                return (this._compressionMode == CompressionMode.Compress);
            }
        }

        private ZlibCodec z
        {
            get
            {
                if (_z == null)
                {
                    bool wantRfc1950Header = (this._flavor == ZlibStreamFlavor.ZLIB);
                    _z = new ZlibCodec();
                    if (this._compressionMode == CompressionMode.Decompress)
                    {
                        _z.InitializeInflate(wantRfc1950Header);
                    }
                    else
                    {
                        _z.Strategy = Strategy;
                        _z.InitializeDeflate(this._level, wantRfc1950Header);
                    }
                }
                return _z;
            }
        }



        private byte[] workingBuffer
        {
            get
            {
                if (_workingBuffer == null)
                    _workingBuffer = new byte[_bufferSize];
                return _workingBuffer;
            }
        }

        public override void Write(System.Byte[] buffer, int offset, int count)
        {
            // workitem 7159
            // calculate the CRC on the unccompressed data  (before writing)
            if (crc != null)
                crc.SlurpBlock(buffer, offset, count);

            if (_streamMode == StreamMode.Undefined)
                _streamMode = StreamMode.Writer;
            else if (_streamMode != StreamMode.Writer)
                throw new ZlibException("Cannot Write after Reading.");

            if (count == 0)
                return;

            // first reference of z property will initialize the private var _z
            z.InputBuffer = buffer;
            _z.NextIn = offset;
            _z.AvailableBytesIn = count;
            bool done = false;
            do
            {
                _z.OutputBuffer = workingBuffer;
                _z.NextOut = 0;
                _z.AvailableBytesOut = _workingBuffer.Length;
                int rc = (_wantCompress)
                    ? _z.Deflate(_flushMode)
                    : _z.Inflate(_flushMode);
                if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
                    throw new ZlibException((_wantCompress ? "de" : "in") + "flating: " + _z.Message);
                
                //if (_workingBuffer.Length - _z.AvailableBytesOut > 0)
                _stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);

                done = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;

                // If GZIP and de-compress, we're done when 8 bytes remain.
                if (_flavor == ZlibStreamFlavor.GZIP && !_wantCompress)
                    done = (_z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0);

            }
            while (!done);
        }



        public void Finish()
        {
            if (_z == null) return;

            if (_streamMode == StreamMode.Writer)
            {
                bool done = false;
                do
                {
                    _z.OutputBuffer = workingBuffer;
                    _z.NextOut = 0;
                    _z.AvailableBytesOut = _workingBuffer.Length;
                    int rc = (_wantCompress)
                        ? _z.Deflate(FlushType.Finish)
                        : _z.Inflate(FlushType.Finish);

                    if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
                    {
                        string verb = (_wantCompress ? "de" : "in") + "flating";
                        if (_z.Message == null)
                            throw new ZlibException(String.Format("{0}: (rc = {1})", verb, rc));
                        else
                            throw new ZlibException(verb + ": " + _z.Message);
                    }

                    if (_workingBuffer.Length - _z.AvailableBytesOut > 0)
                    {
                        _stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);
                    }

                    done = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;
                    // If GZIP and de-compress, we're done when 8 bytes remain.
                    if (_flavor == ZlibStreamFlavor.GZIP && !_wantCompress)
                        done = (_z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0);

                }
                while (!done);

                Flush();

                // workitem 7159
                if (_flavor == ZlibStreamFlavor.GZIP)
                {
                    if (_wantCompress)
                    {
                        // Emit the GZIP trailer: CRC32 and  size mod 2^32
                        int c1 = crc.Crc32Result;
                        _stream.Write(BitConverter.GetBytes(c1), 0, 4);
                        int c2 = (Int32)(crc.TotalBytesRead & 0x00000000FFFFFFFF);
                        _stream.Write(BitConverter.GetBytes(c2), 0, 4);
                    }
                    else
                    {
                        throw new ZlibException("Writing with decompression is not supported.");
                    }
                }
            }
            // workitem 7159
            else if (_streamMode == StreamMode.Reader)
            {
                if (_flavor == ZlibStreamFlavor.GZIP)
                {
                    if (!_wantCompress)
                    {
                        // workitem 8501: handle edge case (decompress empty stream)
                        if (_z.TotalBytesOut == 0L)
                            return;

                        // Read and potentially verify the GZIP trailer:
                        // CRC32 and size mod 2^32
                        byte[] trailer = new byte[8];

                        // workitems 8679 & 12554
                        if (_z.AvailableBytesIn < 8)
                        {
                            // Make sure we have read to the end of the stream
                            Array.Copy(_z.InputBuffer, _z.NextIn, trailer, 0, _z.AvailableBytesIn);
                            int bytesNeeded = 8 - _z.AvailableBytesIn;
                            int bytesRead = _stream.Read(trailer,
                                                         _z.AvailableBytesIn,
                                                         bytesNeeded);
                            if (bytesNeeded != bytesRead)
                            {
                                throw new ZlibException(String.Format("Missing or incomplete GZIP trailer. Expected 8 bytes, got {0}.",
                                                                      _z.AvailableBytesIn + bytesRead));
                            }
                        }
                        else
                        {
                            Array.Copy(_z.InputBuffer, _z.NextIn, trailer, 0, trailer.Length);
                        }

                        Int32 crc32_expected = BitConverter.ToInt32(trailer, 0);
                        Int32 crc32_actual = crc.Crc32Result;
                        Int32 isize_expected = BitConverter.ToInt32(trailer, 4);
                        Int32 isize_actual = (Int32)(_z.TotalBytesOut & 0x00000000FFFFFFFF);

                        if (crc32_actual != crc32_expected)
                            throw new ZlibException(String.Format("Bad CRC32 in GZIP trailer. (actual({0:X8})!=expected({1:X8}))", crc32_actual, crc32_expected));

                        if (isize_actual != isize_expected)
                            throw new ZlibException(String.Format("Bad size in GZIP trailer. (actual({0})!=expected({1}))", isize_actual, isize_expected));

                    }
                    else
                    {
                        throw new ZlibException("Reading with compression is not supported.");
                    }
                }
            }
        }


        private void end()
        {
            if (z == null)
                return;
            if (_wantCompress)
            {
                _z.EndDeflate();
            }
            else
            {
                _z.EndInflate();
            }
            _z = null;
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override System.Int64 Seek(System.Int64 offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
            //_outStream.Seek(offset, origin);
        }
        public override void SetLength(System.Int64 value)
        {
            _stream.SetLength(value);
        }


#if NOT
        internal int Read()
        {
            if (Read(_buf1, 0, 1) == 0)
                return 0;
            // calculate CRC after reading
            if (crc!=null)
                crc.SlurpBlock(_buf1,0,1);
            return (_buf1[0] & 0xFF);
        }
#endif

        private bool nomoreinput = false;



        private string ReadZeroTerminatedString()
        {
            var list = new System.Collections.Generic.List<byte>();
            bool done = false;
            do
            {
                // workitem 7740
                int n = _stream.Read(_buf1, 0, 1);
                if (n != 1)
                    throw new ZlibException("Unexpected EOF reading GZIP header.");
                else
                {
                    if (_buf1[0] == 0)
                        done = true;
                    else
                        list.Add(_buf1[0]);
                }
            } while (!done);
            byte[] a = list.ToArray();
            return GZipStream.iso8859dash1.GetString(a, 0, a.Length);
        }


        private int _ReadAndValidateGzipHeader()
        {
            int totalBytesRead = 0;
            // read the header on the first read
            byte[] header = new byte[10];
            int n = _stream.Read(header, 0, header.Length);

            // workitem 8501: handle edge case (decompress empty stream)
            if (n == 0)
                return 0;

            if (n != 10)
                throw new ZlibException("Not a valid GZIP stream.");

            if (header[0] != 0x1F || header[1] != 0x8B || header[2] != 8)
                throw new ZlibException("Bad GZIP header.");

            Int32 timet = BitConverter.ToInt32(header, 4);
            _GzipMtime = GZipStream._unixEpoch.AddSeconds(timet);
            totalBytesRead += n;
            if ((header[3] & 0x04) == 0x04)
            {
                // read and discard extra field
                n = _stream.Read(header, 0, 2); // 2-byte length field
                totalBytesRead += n;

                Int16 extraLength = (Int16)(header[0] + header[1] * 256);
                byte[] extra = new byte[extraLength];
                n = _stream.Read(extra, 0, extra.Length);
                if (n != extraLength)
                    throw new ZlibException("Unexpected end-of-file reading GZIP header.");
                totalBytesRead += n;
            }
            if ((header[3] & 0x08) == 0x08)
                _GzipFileName = ReadZeroTerminatedString();
            if ((header[3] & 0x10) == 0x010)
                _GzipComment = ReadZeroTerminatedString();
            if ((header[3] & 0x02) == 0x02)
                Read(_buf1, 0, 1); // CRC16, ignore

            return totalBytesRead;
        }



        public override System.Int32 Read(System.Byte[] buffer, System.Int32 offset, System.Int32 count)
        {
            // According to MS documentation, any implementation of the IO.Stream.Read function must:
            // (a) throw an exception if offset & count reference an invalid part of the buffer,
            //     or if count < 0, or if buffer is null
            // (b) return 0 only upon EOF, or if count = 0
            // (c) if not EOF, then return at least 1 byte, up to <count> bytes

            if (_streamMode == StreamMode.Undefined)
            {
                if (!this._stream.CanRead) throw new ZlibException("The stream is not readable.");
                // for the first read, set up some controls.
                _streamMode = StreamMode.Reader;
                // (The first reference to _z goes through the private accessor which
                // may initialize it.)
                z.AvailableBytesIn = 0;
                if (_flavor == ZlibStreamFlavor.GZIP)
                {
                    _gzipHeaderByteCount = _ReadAndValidateGzipHeader();
                    // workitem 8501: handle edge case (decompress empty stream)
                    if (_gzipHeaderByteCount == 0)
                        return 0;
                }
            }

            if (_streamMode != StreamMode.Reader)
                throw new ZlibException("Cannot Read after Writing.");

            if (count == 0) return 0;
            if (nomoreinput && _wantCompress) return 0;  // workitem 8557
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (offset < buffer.GetLowerBound(0)) throw new ArgumentOutOfRangeException("offset");
            if ((offset + count) > buffer.GetLength(0)) throw new ArgumentOutOfRangeException("count");

            int rc = 0;

            // set up the output of the deflate/inflate codec:
            _z.OutputBuffer = buffer;
            _z.NextOut = offset;
            _z.AvailableBytesOut = count;

            // This is necessary in case _workingBuffer has been resized. (new byte[])
            // (The first reference to _workingBuffer goes through the private accessor which
            // may initialize it.)
            _z.InputBuffer = workingBuffer;

            do
            {
                // need data in _workingBuffer in order to deflate/inflate.  Here, we check if we have any.
                if ((_z.AvailableBytesIn == 0) && (!nomoreinput))
                {
                    // No data available, so try to Read data from the captive stream.
                    _z.NextIn = 0;
                    _z.AvailableBytesIn = _stream.Read(_workingBuffer, 0, _workingBuffer.Length);
                    if (_z.AvailableBytesIn == 0)
                        nomoreinput = true;

                }
                // we have data in InputBuffer; now compress or decompress as appropriate
                rc = (_wantCompress)
                    ? _z.Deflate(_flushMode)
                    : _z.Inflate(_flushMode);

                if (nomoreinput && (rc == ZlibConstants.Z_BUF_ERROR))
                    return 0;

                if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
                    throw new ZlibException(String.Format("{0}flating:  rc={1}  msg={2}", (_wantCompress ? "de" : "in"), rc, _z.Message));

                if ((nomoreinput || rc == ZlibConstants.Z_STREAM_END) && (_z.AvailableBytesOut == count))
                    break; // nothing more to read
            }
            //while (_z.AvailableBytesOut == count && rc == ZlibConstants.Z_OK);
            while (_z.AvailableBytesOut > 0 && !nomoreinput && rc == ZlibConstants.Z_OK);


            // workitem 8557
            // is there more room in output?
            if (_z.AvailableBytesOut > 0)
            {
                if (rc == ZlibConstants.Z_OK && _z.AvailableBytesIn == 0)
                {
                    // deferred
                }

                // are we completely done reading?
                if (nomoreinput)
                {
                    // and in compression?
                    if (_wantCompress)
                    {
                        // no more input data available; therefore we flush to
                        // try to complete the read
                        rc = _z.Deflate(FlushType.Finish);

                        if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
                            throw new ZlibException(String.Format("Deflating:  rc={0}  msg={1}", rc, _z.Message));
                    }
                }
            }


            rc = (count - _z.AvailableBytesOut);

            // calculate CRC after reading
            if (crc != null)
                crc.SlurpBlock(buffer, offset, rc);

            return rc;
        }



        public override System.Boolean CanRead
        {
            get { return this._stream.CanRead; }
        }

        public override System.Boolean CanSeek
        {
            get { return this._stream.CanSeek; }
        }

        public override System.Boolean CanWrite
        {
            get { return this._stream.CanWrite; }
        }

        public override System.Int64 Length
        {
            get { return _stream.Length; }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        internal enum StreamMode
        {
            Writer,
            Reader,
            Undefined,
        }


        internal static void CompressString(String s, Stream compressor)
        {
            byte[] uncompressed = System.Text.Encoding.UTF8.GetBytes(s);
            using (compressor)
            {
                compressor.Write(uncompressed, 0, uncompressed.Length);
            }
        }

        internal static void CompressBuffer(byte[] b, Stream compressor)
        {
            // workitem 8460
            using (compressor)
            {
                compressor.Write(b, 0, b.Length);
            }
        }

        internal static String UncompressString(byte[] compressed, Stream decompressor)
        {
            // workitem 8460
            byte[] working = new byte[1024];
            var encoding = System.Text.Encoding.UTF8;
            using (var output = new MemoryStream())
            {
                using (decompressor)
                {
                    int n;
                    while ((n = decompressor.Read(working, 0, working.Length)) != 0)
                    {
                        output.Write(working, 0, n);
                    }
                }

                // reset to allow read from start
                output.Seek(0, SeekOrigin.Begin);
                var sr = new StreamReader(output, encoding);
                return sr.ReadToEnd();
            }
        }

        internal static byte[] UncompressBuffer(byte[] compressed, Stream decompressor)
        {
            // workitem 8460
            byte[] working = new byte[1024];
            using (var output = new MemoryStream())
            {
                using (decompressor)
                {
                    int n;
                    while ((n = decompressor.Read(working, 0, working.Length)) != 0)
                    {
                        output.Write(working, 0, n);
                    }
                }
                return output.ToArray();
            }
        }

    }

    /// <summary>
    /// Describes how to flush the current deflate operation.
    /// </summary>
    /// <remarks>
    /// The different FlushType values are useful when using a Deflate in a streaming application.
    /// </remarks>
    public enum FlushType
    {
        /// <summary>No flush at all.</summary>
        None = 0,

        /// <summary>Closes the current block, but doesn't flush it to
        /// the output. Used internally only in hypothetical
        /// scenarios.  This was supposed to be removed by Zlib, but it is
        /// still in use in some edge cases.
        /// </summary>
        Partial,

        /// <summary>
        /// Use this during compression to specify that all pending output should be
        /// flushed to the output buffer and the output should be aligned on a byte
        /// boundary.  You might use this in a streaming communication scenario, so that
        /// the decompressor can get all input data available so far.  When using this
        /// with a ZlibCodec, <c>AvailableBytesIn</c> will be zero after the call if
        /// enough output space has been provided before the call.  Flushing will
        /// degrade compression and so it should be used only when necessary.
        /// </summary>
        Sync,

        /// <summary>
        /// Use this during compression to specify that all output should be flushed, as
        /// with <c>FlushType.Sync</c>, but also, the compression state should be reset
        /// so that decompression can restart from this point if previous compressed
        /// data has been damaged or if random access is desired.  Using
        /// <c>FlushType.Full</c> too often can significantly degrade the compression.
        /// </summary>
        Full,

        /// <summary>Signals the end of the compression/decompression stream.</summary>
        Finish,
    }


    /// <summary>
    /// The compression level to be used when using a DeflateStream or ZlibStream with CompressionMode.Compress.
    /// </summary>
    public enum CompressionLevel
    {
        /// <summary>
        /// None means that the data will be simply stored, with no change at all.
        /// If you are producing ZIPs for use on Mac OSX, be aware that archives produced with CompressionLevel.None
        /// cannot be opened with the default zip reader. Use a different CompressionLevel.
        /// </summary>
        None = 0,
        /// <summary>
        /// Same as None.
        /// </summary>
        Level0 = 0,

        /// <summary>
        /// The fastest but least effective compression.
        /// </summary>
        BestSpeed = 1,

        /// <summary>
        /// A synonym for BestSpeed.
        /// </summary>
        Level1 = 1,

        /// <summary>
        /// A little slower, but better, than level 1.
        /// </summary>
        Level2 = 2,

        /// <summary>
        /// A little slower, but better, than level 2.
        /// </summary>
        Level3 = 3,

        /// <summary>
        /// A little slower, but better, than level 3.
        /// </summary>
        Level4 = 4,

        /// <summary>
        /// A little slower than level 4, but with better compression.
        /// </summary>
        Level5 = 5,

        /// <summary>
        /// The default compression level, with a good balance of speed and compression efficiency.
        /// </summary>
        Default = 6,
        /// <summary>
        /// A synonym for Default.
        /// </summary>
        Level6 = 6,

        /// <summary>
        /// Pretty good compression!
        /// </summary>
        Level7 = 7,

        /// <summary>
        ///  Better compression than Level7!
        /// </summary>
        Level8 = 8,

        /// <summary>
        /// The "best" compression, where best means greatest reduction in size of the input data stream.
        /// This is also the slowest compression.
        /// </summary>
        BestCompression = 9,

        /// <summary>
        /// A synonym for BestCompression.
        /// </summary>
        Level9 = 9,
    }

    /// <summary>
    /// Describes options for how the compression algorithm is executed.  Different strategies
    /// work better on different sorts of data.  The strategy parameter can affect the compression
    /// ratio and the speed of compression but not the correctness of the compresssion.
    /// </summary>
    internal enum CompressionStrategy
    {
        /// <summary>
        /// The default strategy is probably the best for normal data.
        /// </summary>
        Default = 0,

        /// <summary>
        /// The <c>Filtered</c> strategy is intended to be used most effectively with data produced by a
        /// filter or predictor.  By this definition, filtered data consists mostly of small
        /// values with a somewhat random distribution.  In this case, the compression algorithm
        /// is tuned to compress them better.  The effect of <c>Filtered</c> is to force more Huffman
        /// coding and less string matching; it is a half-step between <c>Default</c> and <c>HuffmanOnly</c>.
        /// </summary>
        Filtered = 1,

        /// <summary>
        /// Using <c>HuffmanOnly</c> will force the compressor to do Huffman encoding only, with no
        /// string matching.
        /// </summary>
        HuffmanOnly = 2,
    }


    /// <summary>
    /// An enum to specify the direction of transcoding - whether to compress or decompress.
    /// </summary>
    public enum CompressionMode
    {
        /// <summary>
        /// Used to specify that the stream should compress the data.
        /// </summary>
        Compress = 0,
        /// <summary>
        /// Used to specify that the stream should decompress the data.
        /// </summary>
        Decompress = 1,
    }


    /// <summary>
    /// A general purpose exception class for exceptions in the Zlib library.
    /// </summary>
    internal class ZlibException : System.Exception
    {
        /// <summary>
        /// The ZlibException class captures exception information generated
        /// by the Zlib library.
        /// </summary>
        internal ZlibException()
            : base()
        {
        }

        /// <summary>
        /// This ctor collects a message attached to the exception.
        /// </summary>
        /// <param name="s">the message for the exception.</param>
        internal ZlibException(System.String s)
            : base(s)
        {
        }
    }


    internal class SharedUtils
    {
        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        internal static int URShift(int number, int bits)
        {
            return (int)((uint)number >> bits);
        }

#if NOT
        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        internal static long URShift(long number, int bits)
        {
            return (long) ((UInt64)number >> bits);
        }
#endif

        /// <summary>
        ///   Reads a number of characters from the current source TextReader and writes
        ///   the data to the target array at the specified index.
        /// </summary>
        ///
        /// <param name="sourceTextReader">The source TextReader to read from</param>
        /// <param name="target">Contains the array of characteres read from the source TextReader.</param>
        /// <param name="start">The starting index of the target array.</param>
        /// <param name="count">The maximum number of characters to read from the source TextReader.</param>
        ///
        /// <returns>
        ///   The number of characters read. The number will be less than or equal to
        ///   count depending on the data available in the source TextReader. Returns -1
        ///   if the end of the stream is reached.
        /// </returns>
        internal static System.Int32 ReadInput(System.IO.TextReader sourceTextReader, byte[] target, int start, int count)
        {
            // Returns 0 bytes if not enough space in target
            if (target.Length == 0) return 0;

            char[] charArray = new char[target.Length];
            int bytesRead = sourceTextReader.Read(charArray, start, count);

            // Returns -1 if EOF
            if (bytesRead == 0) return -1;

            for (int index = start; index < start + bytesRead; index++)
                target[index] = (byte)charArray[index];

            return bytesRead;
        }


        internal static byte[] ToByteArray(System.String sourceString)
        {
            return System.Text.UTF8Encoding.UTF8.GetBytes(sourceString);
        }


        internal static char[] ToCharArray(byte[] byteArray)
        {
            return System.Text.UTF8Encoding.UTF8.GetChars(byteArray);
        }
    }

    internal static class InternalConstants
    {
        internal static readonly int MAX_BITS = 15;
        internal static readonly int BL_CODES = 19;
        internal static readonly int D_CODES = 30;
        internal static readonly int LITERALS = 256;
        internal static readonly int LENGTH_CODES = 29;
        internal static readonly int L_CODES = (LITERALS + 1 + LENGTH_CODES);

        // Bit length codes must not exceed MAX_BL_BITS bits
        internal static readonly int MAX_BL_BITS = 7;

        // repeat previous bit length 3-6 times (2 bits of repeat count)
        internal static readonly int REP_3_6 = 16;

        // repeat a zero length 3-10 times  (3 bits of repeat count)
        internal static readonly int REPZ_3_10 = 17;

        // repeat a zero length 11-138 times  (7 bits of repeat count)
        internal static readonly int REPZ_11_138 = 18;

    }

    internal sealed class StaticTree
    {
        internal static readonly short[] lengthAndLiteralsTreeCodes = new short[] {
            12, 8, 140, 8, 76, 8, 204, 8, 44, 8, 172, 8, 108, 8, 236, 8,
            28, 8, 156, 8, 92, 8, 220, 8, 60, 8, 188, 8, 124, 8, 252, 8,
             2, 8, 130, 8, 66, 8, 194, 8, 34, 8, 162, 8, 98, 8, 226, 8,
            18, 8, 146, 8, 82, 8, 210, 8, 50, 8, 178, 8, 114, 8, 242, 8,
            10, 8, 138, 8, 74, 8, 202, 8, 42, 8, 170, 8, 106, 8, 234, 8,
            26, 8, 154, 8, 90, 8, 218, 8, 58, 8, 186, 8, 122, 8, 250, 8,
             6, 8, 134, 8, 70, 8, 198, 8, 38, 8, 166, 8, 102, 8, 230, 8,
            22, 8, 150, 8, 86, 8, 214, 8, 54, 8, 182, 8, 118, 8, 246, 8,
            14, 8, 142, 8, 78, 8, 206, 8, 46, 8, 174, 8, 110, 8, 238, 8,
            30, 8, 158, 8, 94, 8, 222, 8, 62, 8, 190, 8, 126, 8, 254, 8,
             1, 8, 129, 8, 65, 8, 193, 8, 33, 8, 161, 8, 97, 8, 225, 8,
            17, 8, 145, 8, 81, 8, 209, 8, 49, 8, 177, 8, 113, 8, 241, 8,
             9, 8, 137, 8, 73, 8, 201, 8, 41, 8, 169, 8, 105, 8, 233, 8,
            25, 8, 153, 8, 89, 8, 217, 8, 57, 8, 185, 8, 121, 8, 249, 8,
             5, 8, 133, 8, 69, 8, 197, 8, 37, 8, 165, 8, 101, 8, 229, 8,
            21, 8, 149, 8, 85, 8, 213, 8, 53, 8, 181, 8, 117, 8, 245, 8,
            13, 8, 141, 8, 77, 8, 205, 8, 45, 8, 173, 8, 109, 8, 237, 8,
            29, 8, 157, 8, 93, 8, 221, 8, 61, 8, 189, 8, 125, 8, 253, 8,
            19, 9, 275, 9, 147, 9, 403, 9, 83, 9, 339, 9, 211, 9, 467, 9,
            51, 9, 307, 9, 179, 9, 435, 9, 115, 9, 371, 9, 243, 9, 499, 9,
            11, 9, 267, 9, 139, 9, 395, 9, 75, 9, 331, 9, 203, 9, 459, 9,
            43, 9, 299, 9, 171, 9, 427, 9, 107, 9, 363, 9, 235, 9, 491, 9,
            27, 9, 283, 9, 155, 9, 411, 9, 91, 9, 347, 9, 219, 9, 475, 9,
            59, 9, 315, 9, 187, 9, 443, 9, 123, 9, 379, 9, 251, 9, 507, 9,
             7, 9, 263, 9, 135, 9, 391, 9, 71, 9, 327, 9, 199, 9, 455, 9,
            39, 9, 295, 9, 167, 9, 423, 9, 103, 9, 359, 9, 231, 9, 487, 9,
            23, 9, 279, 9, 151, 9, 407, 9, 87, 9, 343, 9, 215, 9, 471, 9,
            55, 9, 311, 9, 183, 9, 439, 9, 119, 9, 375, 9, 247, 9, 503, 9,
            15, 9, 271, 9, 143, 9, 399, 9, 79, 9, 335, 9, 207, 9, 463, 9,
            47, 9, 303, 9, 175, 9, 431, 9, 111, 9, 367, 9, 239, 9, 495, 9,
            31, 9, 287, 9, 159, 9, 415, 9, 95, 9, 351, 9, 223, 9, 479, 9,
            63, 9, 319, 9, 191, 9, 447, 9, 127, 9, 383, 9, 255, 9, 511, 9,
             0, 7, 64, 7, 32, 7, 96, 7, 16, 7, 80, 7, 48, 7, 112, 7,
             8, 7, 72, 7, 40, 7, 104, 7, 24, 7, 88, 7, 56, 7, 120, 7,
             4, 7, 68, 7, 36, 7, 100, 7, 20, 7, 84, 7, 52, 7, 116, 7,
             3, 8, 131, 8, 67, 8, 195, 8, 35, 8, 163, 8, 99, 8, 227, 8
        };

        internal static readonly short[] distTreeCodes = new short[] {
            0, 5, 16, 5, 8, 5, 24, 5, 4, 5, 20, 5, 12, 5, 28, 5,
            2, 5, 18, 5, 10, 5, 26, 5, 6, 5, 22, 5, 14, 5, 30, 5,
            1, 5, 17, 5, 9, 5, 25, 5, 5, 5, 21, 5, 13, 5, 29, 5,
            3, 5, 19, 5, 11, 5, 27, 5, 7, 5, 23, 5 };

        internal static readonly StaticTree Literals;
        internal static readonly StaticTree Distances;
        internal static readonly StaticTree BitLengths;

        internal short[] treeCodes; // static tree or null
        internal int[] extraBits;   // extra bits for each code or null
        internal int extraBase;     // base index for extra_bits
        internal int elems;         // max number of elements in the tree
        internal int maxLength;     // max bit length for the codes

        private StaticTree(short[] treeCodes, int[] extraBits, int extraBase, int elems, int maxLength)
        {
            this.treeCodes = treeCodes;
            this.extraBits = extraBits;
            this.extraBase = extraBase;
            this.elems = elems;
            this.maxLength = maxLength;
        }
        static StaticTree()
        {
            Literals = new StaticTree(lengthAndLiteralsTreeCodes, Tree.ExtraLengthBits, InternalConstants.LITERALS + 1, InternalConstants.L_CODES, InternalConstants.MAX_BITS);
            Distances = new StaticTree(distTreeCodes, Tree.ExtraDistanceBits, 0, InternalConstants.D_CODES, InternalConstants.MAX_BITS);
            BitLengths = new StaticTree(null, Tree.extra_blbits, 0, InternalConstants.BL_CODES, InternalConstants.MAX_BL_BITS);
        }
    }



    /// <summary>
    /// Computes an Adler-32 checksum.
    /// </summary>
    /// <remarks>
    /// The Adler checksum is similar to a CRC checksum, but faster to compute, though less
    /// reliable.  It is used in producing RFC1950 compressed streams.  The Adler checksum
    /// is a required part of the "ZLIB" standard.  Applications will almost never need to
    /// use this class directly.
    /// </remarks>
    ///
    /// <exclude/>
    internal sealed class Adler
    {
        // largest prime smaller than 65536
        private static readonly uint BASE = 65521;
        // NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
        private static readonly int NMAX = 5552;


#pragma warning disable 3001
#pragma warning disable 3002

        /// <summary>
        ///   Calculates the Adler32 checksum.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This is used within ZLIB.  You probably don't need to use this directly.
        ///   </para>
        /// </remarks>
        /// <example>
        ///    To compute an Adler32 checksum on a byte array:
        ///  <code>
        ///    var adler = Adler.Adler32(0, null, 0, 0);
        ///    adler = Adler.Adler32(adler, buffer, index, length);
        ///  </code>
        /// </example>
        internal static uint Adler32(uint adler, byte[] buf, int index, int len)
        {
            if (buf == null)
                return 1;

            uint s1 = (uint)(adler & 0xffff);
            uint s2 = (uint)((adler >> 16) & 0xffff);

            while (len > 0)
            {
                int k = len < NMAX ? len : NMAX;
                len -= k;
                while (k >= 16)
                {
                    //s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    s1 += buf[index++]; s2 += s1;
                    k -= 16;
                }
                if (k != 0)
                {
                    do
                    {
                        s1 += buf[index++];
                        s2 += s1;
                    }
                    while (--k != 0);
                }
                s1 %= BASE;
                s2 %= BASE;
            }
            return (uint)((s2 << 16) | s1);
        }
#pragma warning restore 3001
#pragma warning restore 3002

    }

    sealed class Tree
    {
        private static readonly int HEAP_SIZE = (2 * InternalConstants.L_CODES + 1);

        // extra bits for each length code
        internal static readonly int[] ExtraLengthBits = new int[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
            3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
        };

        // extra bits for each distance code
        internal static readonly int[] ExtraDistanceBits = new int[]
        {
            0, 0, 0, 0, 1, 1,  2,  2,  3,  3,  4,  4,  5,  5,  6,  6,
            7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13
        };

        // extra bits for each bit length code
        internal static readonly int[] extra_blbits = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 7 };

        internal static readonly sbyte[] bl_order = new sbyte[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };


        // The lengths of the bit length codes are sent in order of decreasing
        // probability, to avoid transmitting the lengths for unused bit
        // length codes.

        internal const int Buf_size = 8 * 2;

        // see definition of array dist_code below
        //internal const int DIST_CODE_LEN = 512;

        private static readonly sbyte[] _dist_code = new sbyte[]
        {
            0,  1,  2,  3,  4,  4,  5,  5,  6,  6,  6,  6,  7,  7,  7,  7, 
            8,  8,  8,  8,  8,  8,  8,  8,  9,  9,  9,  9,  9,  9,  9,  9,
            10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 
            11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 
            12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 
            12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 
            13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 
            13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 
            14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
            14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
            14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
            14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 
            0,   0, 16, 17, 18, 18, 19, 19, 20, 20, 20, 20, 21, 21, 21, 21, 
            22, 22, 22, 22, 22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23, 23, 
            24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 
            25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 
            26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 
            26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 
            27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 
            27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 
            28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 
            28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 
            28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 
            28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 
            29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 
            29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 
            29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 
            29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29
        };

        internal static readonly sbyte[] LengthCode = new sbyte[]
        {
            0,   1,  2,  3,  4,  5,  6,  7,  8,  8,  9,  9, 10, 10, 11, 11,
            12, 12, 12, 12, 13, 13, 13, 13, 14, 14, 14, 14, 15, 15, 15, 15,
            16, 16, 16, 16, 16, 16, 16, 16, 17, 17, 17, 17, 17, 17, 17, 17,
            18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19, 19, 19, 19, 19, 19,
            20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
            21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21,
            22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22,
            23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
            24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
            24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
            25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
            25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
            26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
            26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
            27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
            27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28
        };


        internal static readonly int[] LengthBase = new int[]
        {
            0,   1,  2,  3,  4,  5,  6,   7,   8,  10,  12,  14, 16, 20, 24, 28,
            32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 0
        };


        internal static readonly int[] DistanceBase = new int[]
        {
            0, 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 128, 192,
            256, 384, 512, 768, 1024, 1536, 2048, 3072, 4096, 6144, 8192, 12288, 16384, 24576
        };


        /// <summary>
        /// Map from a distance to a distance code.
        /// </summary>
        /// <remarks> 
        /// No side effects. _dist_code[256] and _dist_code[257] are never used.
        /// </remarks>
        internal static int DistanceCode(int dist)
        {
            return (dist < 256)
                ? _dist_code[dist]
                : _dist_code[256 + SharedUtils.URShift(dist, 7)];
        }

        internal short[] dyn_tree; // the dynamic tree
        internal int max_code; // largest code with non zero frequency
        internal StaticTree staticTree; // the corresponding static tree

        // Compute the optimal bit lengths for a tree and update the total bit length
        // for the current block.
        // IN assertion: the fields freq and dad are set, heap[heap_max] and
        //    above are the tree nodes sorted by increasing frequency.
        // OUT assertions: the field len is set to the optimal bit length, the
        //     array bl_count contains the frequencies for each bit length.
        //     The length opt_len is updated; static_len is also updated if stree is
        //     not null.
        internal void gen_bitlen(DeflateManager s)
        {
            short[] tree = dyn_tree;
            short[] stree = staticTree.treeCodes;
            int[] extra = staticTree.extraBits;
            int base_Renamed = staticTree.extraBase;
            int max_length = staticTree.maxLength;
            int h; // heap index
            int n, m; // iterate over the tree elements
            int bits; // bit length
            int xbits; // extra bits
            short f; // frequency
            int overflow = 0; // number of elements with bit length too large

            for (bits = 0; bits <= InternalConstants.MAX_BITS; bits++)
                s.bl_count[bits] = 0;

            // In a first pass, compute the optimal bit lengths (which may
            // overflow in the case of the bit length tree).
            tree[s.heap[s.heap_max] * 2 + 1] = 0; // root of the heap

            for (h = s.heap_max + 1; h < HEAP_SIZE; h++)
            {
                n = s.heap[h];
                bits = tree[tree[n * 2 + 1] * 2 + 1] + 1;
                if (bits > max_length)
                {
                    bits = max_length; overflow++;
                }
                tree[n * 2 + 1] = (short)bits;
                // We overwrite tree[n*2+1] which is no longer needed

                if (n > max_code)
                    continue; // not a leaf node

                s.bl_count[bits]++;
                xbits = 0;
                if (n >= base_Renamed)
                    xbits = extra[n - base_Renamed];
                f = tree[n * 2];
                s.opt_len += f * (bits + xbits);
                if (stree != null)
                    s.static_len += f * (stree[n * 2 + 1] + xbits);
            }
            if (overflow == 0)
                return;

            // This happens for example on obj2 and pic of the Calgary corpus
            // Find the first bit length which could increase:
            do
            {
                bits = max_length - 1;
                while (s.bl_count[bits] == 0)
                    bits--;
                s.bl_count[bits]--; // move one leaf down the tree
                s.bl_count[bits + 1] = (short)(s.bl_count[bits + 1] + 2); // move one overflow item as its brother
                s.bl_count[max_length]--;
                // The brother of the overflow item also moves one step up,
                // but this does not affect bl_count[max_length]
                overflow -= 2;
            }
            while (overflow > 0);

            for (bits = max_length; bits != 0; bits--)
            {
                n = s.bl_count[bits];
                while (n != 0)
                {
                    m = s.heap[--h];
                    if (m > max_code)
                        continue;
                    if (tree[m * 2 + 1] != bits)
                    {
                        s.opt_len = (int)(s.opt_len + ((long)bits - (long)tree[m * 2 + 1]) * (long)tree[m * 2]);
                        tree[m * 2 + 1] = (short)bits;
                    }
                    n--;
                }
            }
        }

        // Construct one Huffman tree and assigns the code bit strings and lengths.
        // Update the total bit length for the current block.
        // IN assertion: the field freq is set for all tree elements.
        // OUT assertions: the fields len and code are set to the optimal bit length
        //     and corresponding code. The length opt_len is updated; static_len is
        //     also updated if stree is not null. The field max_code is set.
        internal void build_tree(DeflateManager s)
        {
            short[] tree = dyn_tree;
            short[] stree = staticTree.treeCodes;
            int elems = staticTree.elems;
            int n, m;            // iterate over heap elements
            int max_code = -1;  // largest code with non zero frequency
            int node;            // new node being created

            // Construct the initial heap, with least frequent element in
            // heap[1]. The sons of heap[n] are heap[2*n] and heap[2*n+1].
            // heap[0] is not used.
            s.heap_len = 0;
            s.heap_max = HEAP_SIZE;

            for (n = 0; n < elems; n++)
            {
                if (tree[n * 2] != 0)
                {
                    s.heap[++s.heap_len] = max_code = n;
                    s.depth[n] = 0;
                }
                else
                {
                    tree[n * 2 + 1] = 0;
                }
            }

            // The pkzip format requires that at least one distance code exists,
            // and that at least one bit should be sent even if there is only one
            // possible code. So to avoid special checks later on we force at least
            // two codes of non zero frequency.
            while (s.heap_len < 2)
            {
                node = s.heap[++s.heap_len] = (max_code < 2 ? ++max_code : 0);
                tree[node * 2] = 1;
                s.depth[node] = 0;
                s.opt_len--;
                if (stree != null)
                    s.static_len -= stree[node * 2 + 1];
                // node is 0 or 1 so it does not have extra bits
            }
            this.max_code = max_code;

            // The elements heap[heap_len/2+1 .. heap_len] are leaves of the tree,
            // establish sub-heaps of increasing lengths:

            for (n = s.heap_len / 2; n >= 1; n--)
                s.pqdownheap(tree, n);

            // Construct the Huffman tree by repeatedly combining the least two
            // frequent nodes.

            node = elems; // next internal node of the tree
            do
            {
                // n = node of least frequency
                n = s.heap[1];
                s.heap[1] = s.heap[s.heap_len--];
                s.pqdownheap(tree, 1);
                m = s.heap[1]; // m = node of next least frequency

                s.heap[--s.heap_max] = n; // keep the nodes sorted by frequency
                s.heap[--s.heap_max] = m;

                // Create a new node father of n and m
                tree[node * 2] = unchecked((short)(tree[n * 2] + tree[m * 2]));
                s.depth[node] = (sbyte)(System.Math.Max((byte)s.depth[n], (byte)s.depth[m]) + 1);
                tree[n * 2 + 1] = tree[m * 2 + 1] = (short)node;

                // and insert the new node in the heap
                s.heap[1] = node++;
                s.pqdownheap(tree, 1);
            }
            while (s.heap_len >= 2);

            s.heap[--s.heap_max] = s.heap[1];

            // At this point, the fields freq and dad are set. We can now
            // generate the bit lengths.

            gen_bitlen(s);

            // The field len is now set, we can generate the bit codes
            gen_codes(tree, max_code, s.bl_count);
        }

        // Generate the codes for a given tree and bit counts (which need not be
        // optimal).
        // IN assertion: the array bl_count contains the bit length statistics for
        // the given tree and the field len is set for all tree elements.
        // OUT assertion: the field code is set for all tree elements of non
        //     zero code length.
        internal static void gen_codes(short[] tree, int max_code, short[] bl_count)
        {
            short[] next_code = new short[InternalConstants.MAX_BITS + 1]; // next code value for each bit length
            short code = 0; // running code value
            int bits; // bit index
            int n; // code index

            // The distribution counts are first used to generate the code values
            // without bit reversal.
            for (bits = 1; bits <= InternalConstants.MAX_BITS; bits++)
                unchecked
                {
                    next_code[bits] = code = (short)((code + bl_count[bits - 1]) << 1);
                }

            // Check that the bit counts in bl_count are consistent. The last code
            // must be all ones.
            //Assert (code + bl_count[MAX_BITS]-1 == (1<<MAX_BITS)-1,
            //        "inconsistent bit counts");
            //Tracev((stderr,"\ngen_codes: max_code %d ", max_code));

            for (n = 0; n <= max_code; n++)
            {
                int len = tree[n * 2 + 1];
                if (len == 0)
                    continue;
                // Now reverse the bits
                tree[n * 2] = unchecked((short)(bi_reverse(next_code[len]++, len)));
            }
        }

        // Reverse the first len bits of a code, using straightforward code (a faster
        // method would use a table)
        // IN assertion: 1 <= len <= 15
        internal static int bi_reverse(int code, int len)
        {
            int res = 0;
            do
            {
                res |= code & 1;
                code >>= 1; //SharedUtils.URShift(code, 1);
                res <<= 1;
            }
            while (--len > 0);
            return res >> 1;
        }
    }

    sealed class InfTree
    {

        private const int MANY = 1440;

        private const int Z_OK = 0;
        private const int Z_STREAM_END = 1;
        private const int Z_NEED_DICT = 2;
        private const int Z_ERRNO = -1;
        private const int Z_STREAM_ERROR = -2;
        private const int Z_DATA_ERROR = -3;
        private const int Z_MEM_ERROR = -4;
        private const int Z_BUF_ERROR = -5;
        private const int Z_VERSION_ERROR = -6;

        internal const int fixed_bl = 9;
        internal const int fixed_bd = 5;

        //UPGRADE_NOTE: Final was removed from the declaration of 'fixed_tl'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly int[] fixed_tl = new int[]{96, 7, 256, 0, 8, 80, 0, 8, 16, 84, 8, 115, 82, 7, 31, 0, 8, 112, 0, 8, 48, 0, 9, 192, 80, 7, 10, 0, 8, 96, 0, 8, 32, 0, 9, 160, 0, 8, 0, 0, 8, 128, 0, 8, 64, 0, 9, 224, 80, 7, 6, 0, 8, 88, 0, 8, 24, 0, 9, 144, 83, 7, 59, 0, 8, 120, 0, 8, 56, 0, 9, 208, 81, 7, 17, 0, 8, 104, 0, 8, 40, 0, 9, 176, 0, 8, 8, 0, 8, 136, 0, 8, 72, 0, 9, 240, 80, 7, 4, 0, 8, 84, 0, 8, 20, 85, 8, 227, 83, 7, 43, 0, 8, 116, 0, 8, 52, 0, 9, 200, 81, 7, 13, 0, 8, 100, 0, 8, 36, 0, 9, 168, 0, 8, 4, 0, 8, 132, 0, 8, 68, 0, 9, 232, 80, 7, 8, 0, 8, 92, 0, 8, 28, 0, 9, 152, 84, 7, 83, 0, 8, 124, 0, 8, 60, 0, 9, 216, 82, 7, 23, 0, 8, 108, 0, 8, 44, 0, 9, 184, 0, 8, 12, 0, 8, 140, 0, 8, 76, 0, 9, 248, 80, 7, 3, 0, 8, 82, 0, 8, 18, 85, 8, 163, 83, 7, 35, 0, 8, 114, 0, 8, 50, 0, 9, 196, 81, 7, 11, 0, 8, 98, 0, 8, 34, 0, 9, 164, 0, 8, 2, 0, 8, 130, 0, 8, 66, 0, 9, 228, 80, 7, 7, 0, 8, 90, 0, 8, 26, 0, 9, 148, 84, 7, 67, 0, 8, 122, 0, 8, 58, 0, 9, 212, 82, 7, 19, 0, 8, 106, 0, 8, 42, 0, 9, 180, 0, 8, 10, 0, 8, 138, 0, 8, 74, 0, 9, 244, 80, 7, 5, 0, 8, 86, 0, 8, 22, 192, 8, 0, 83, 7, 51, 0, 8, 118, 0, 8, 54, 0, 9, 204, 81, 7, 15, 0, 8, 102, 0, 8, 38, 0, 9, 172, 0, 8, 6, 0, 8, 134, 0, 8, 70, 0, 9, 236, 80, 7, 9, 0, 8, 94, 0, 8, 30, 0, 9, 156, 84, 7, 99, 0, 8, 126, 0, 8, 62, 0, 9, 220, 82, 7, 27, 0, 8, 110, 0, 8, 46, 0, 9, 188, 0, 8, 14, 0, 8, 142, 0, 8, 78, 0, 9, 252, 96, 7, 256, 0, 8, 81, 0, 8, 17, 85, 8, 131, 82, 7, 31, 0, 8, 113, 0, 8, 49, 0, 9, 194, 80, 7, 10, 0, 8, 97, 0, 8, 33, 0, 9, 162, 0, 8, 1, 0, 8, 129, 0, 8, 65, 0, 9, 226, 80, 7, 6, 0, 8, 89, 0, 8, 25, 0, 9, 146, 83, 7, 59, 0, 8, 121, 0, 8, 57, 0, 9, 210, 81, 7, 17, 0, 8, 105, 0, 8, 41, 0, 9, 178, 0, 8, 9, 0, 8, 137, 0, 8, 73, 0, 9, 242, 80, 7, 4, 0, 8, 85, 0, 8, 21, 80, 8, 258, 83, 7, 43, 0, 8, 117, 0, 8, 53, 0, 9, 202, 81, 7, 13, 0, 8, 101, 0, 8, 37, 0, 9, 170, 0, 8, 5, 0, 8, 133, 0, 8, 69, 0, 9, 234, 80, 7, 8, 0, 8, 93, 0, 8, 29, 0, 9, 154, 84, 7, 83, 0, 8, 125, 0, 8, 61, 0, 9, 218, 82, 7, 23, 0, 8, 109, 0, 8, 45, 0, 9, 186, 
                        0, 8, 13, 0, 8, 141, 0, 8, 77, 0, 9, 250, 80, 7, 3, 0, 8, 83, 0, 8, 19, 85, 8, 195, 83, 7, 35, 0, 8, 115, 0, 8, 51, 0, 9, 198, 81, 7, 11, 0, 8, 99, 0, 8, 35, 0, 9, 166, 0, 8, 3, 0, 8, 131, 0, 8, 67, 0, 9, 230, 80, 7, 7, 0, 8, 91, 0, 8, 27, 0, 9, 150, 84, 7, 67, 0, 8, 123, 0, 8, 59, 0, 9, 214, 82, 7, 19, 0, 8, 107, 0, 8, 43, 0, 9, 182, 0, 8, 11, 0, 8, 139, 0, 8, 75, 0, 9, 246, 80, 7, 5, 0, 8, 87, 0, 8, 23, 192, 8, 0, 83, 7, 51, 0, 8, 119, 0, 8, 55, 0, 9, 206, 81, 7, 15, 0, 8, 103, 0, 8, 39, 0, 9, 174, 0, 8, 7, 0, 8, 135, 0, 8, 71, 0, 9, 238, 80, 7, 9, 0, 8, 95, 0, 8, 31, 0, 9, 158, 84, 7, 99, 0, 8, 127, 0, 8, 63, 0, 9, 222, 82, 7, 27, 0, 8, 111, 0, 8, 47, 0, 9, 190, 0, 8, 15, 0, 8, 143, 0, 8, 79, 0, 9, 254, 96, 7, 256, 0, 8, 80, 0, 8, 16, 84, 8, 115, 82, 7, 31, 0, 8, 112, 0, 8, 48, 0, 9, 193, 80, 7, 10, 0, 8, 96, 0, 8, 32, 0, 9, 161, 0, 8, 0, 0, 8, 128, 0, 8, 64, 0, 9, 225, 80, 7, 6, 0, 8, 88, 0, 8, 24, 0, 9, 145, 83, 7, 59, 0, 8, 120, 0, 8, 56, 0, 9, 209, 81, 7, 17, 0, 8, 104, 0, 8, 40, 0, 9, 177, 0, 8, 8, 0, 8, 136, 0, 8, 72, 0, 9, 241, 80, 7, 4, 0, 8, 84, 0, 8, 20, 85, 8, 227, 83, 7, 43, 0, 8, 116, 0, 8, 52, 0, 9, 201, 81, 7, 13, 0, 8, 100, 0, 8, 36, 0, 9, 169, 0, 8, 4, 0, 8, 132, 0, 8, 68, 0, 9, 233, 80, 7, 8, 0, 8, 92, 0, 8, 28, 0, 9, 153, 84, 7, 83, 0, 8, 124, 0, 8, 60, 0, 9, 217, 82, 7, 23, 0, 8, 108, 0, 8, 44, 0, 9, 185, 0, 8, 12, 0, 8, 140, 0, 8, 76, 0, 9, 249, 80, 7, 3, 0, 8, 82, 0, 8, 18, 85, 8, 163, 83, 7, 35, 0, 8, 114, 0, 8, 50, 0, 9, 197, 81, 7, 11, 0, 8, 98, 0, 8, 34, 0, 9, 165, 0, 8, 2, 0, 8, 130, 0, 8, 66, 0, 9, 229, 80, 7, 7, 0, 8, 90, 0, 8, 26, 0, 9, 149, 84, 7, 67, 0, 8, 122, 0, 8, 58, 0, 9, 213, 82, 7, 19, 0, 8, 106, 0, 8, 42, 0, 9, 181, 0, 8, 10, 0, 8, 138, 0, 8, 74, 0, 9, 245, 80, 7, 5, 0, 8, 86, 0, 8, 22, 192, 8, 0, 83, 7, 51, 0, 8, 118, 0, 8, 54, 0, 9, 205, 81, 7, 15, 0, 8, 102, 0, 8, 38, 0, 9, 173, 0, 8, 6, 0, 8, 134, 0, 8, 70, 0, 9, 237, 80, 7, 9, 0, 8, 94, 0, 8, 30, 0, 9, 157, 84, 7, 99, 0, 8, 126, 0, 8, 62, 0, 9, 221, 82, 7, 27, 0, 8, 110, 0, 8, 46, 0, 9, 189, 0, 8, 
                        14, 0, 8, 142, 0, 8, 78, 0, 9, 253, 96, 7, 256, 0, 8, 81, 0, 8, 17, 85, 8, 131, 82, 7, 31, 0, 8, 113, 0, 8, 49, 0, 9, 195, 80, 7, 10, 0, 8, 97, 0, 8, 33, 0, 9, 163, 0, 8, 1, 0, 8, 129, 0, 8, 65, 0, 9, 227, 80, 7, 6, 0, 8, 89, 0, 8, 25, 0, 9, 147, 83, 7, 59, 0, 8, 121, 0, 8, 57, 0, 9, 211, 81, 7, 17, 0, 8, 105, 0, 8, 41, 0, 9, 179, 0, 8, 9, 0, 8, 137, 0, 8, 73, 0, 9, 243, 80, 7, 4, 0, 8, 85, 0, 8, 21, 80, 8, 258, 83, 7, 43, 0, 8, 117, 0, 8, 53, 0, 9, 203, 81, 7, 13, 0, 8, 101, 0, 8, 37, 0, 9, 171, 0, 8, 5, 0, 8, 133, 0, 8, 69, 0, 9, 235, 80, 7, 8, 0, 8, 93, 0, 8, 29, 0, 9, 155, 84, 7, 83, 0, 8, 125, 0, 8, 61, 0, 9, 219, 82, 7, 23, 0, 8, 109, 0, 8, 45, 0, 9, 187, 0, 8, 13, 0, 8, 141, 0, 8, 77, 0, 9, 251, 80, 7, 3, 0, 8, 83, 0, 8, 19, 85, 8, 195, 83, 7, 35, 0, 8, 115, 0, 8, 51, 0, 9, 199, 81, 7, 11, 0, 8, 99, 0, 8, 35, 0, 9, 167, 0, 8, 3, 0, 8, 131, 0, 8, 67, 0, 9, 231, 80, 7, 7, 0, 8, 91, 0, 8, 27, 0, 9, 151, 84, 7, 67, 0, 8, 123, 0, 8, 59, 0, 9, 215, 82, 7, 19, 0, 8, 107, 0, 8, 43, 0, 9, 183, 0, 8, 11, 0, 8, 139, 0, 8, 75, 0, 9, 247, 80, 7, 5, 0, 8, 87, 0, 8, 23, 192, 8, 0, 83, 7, 51, 0, 8, 119, 0, 8, 55, 0, 9, 207, 81, 7, 15, 0, 8, 103, 0, 8, 39, 0, 9, 175, 0, 8, 7, 0, 8, 135, 0, 8, 71, 0, 9, 239, 80, 7, 9, 0, 8, 95, 0, 8, 31, 0, 9, 159, 84, 7, 99, 0, 8, 127, 0, 8, 63, 0, 9, 223, 82, 7, 27, 0, 8, 111, 0, 8, 47, 0, 9, 191, 0, 8, 15, 0, 8, 143, 0, 8, 79, 0, 9, 255};
        //UPGRADE_NOTE: Final was removed from the declaration of 'fixed_td'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly int[] fixed_td = new int[] { 80, 5, 1, 87, 5, 257, 83, 5, 17, 91, 5, 4097, 81, 5, 5, 89, 5, 1025, 85, 5, 65, 93, 5, 16385, 80, 5, 3, 88, 5, 513, 84, 5, 33, 92, 5, 8193, 82, 5, 9, 90, 5, 2049, 86, 5, 129, 192, 5, 24577, 80, 5, 2, 87, 5, 385, 83, 5, 25, 91, 5, 6145, 81, 5, 7, 89, 5, 1537, 85, 5, 97, 93, 5, 24577, 80, 5, 4, 88, 5, 769, 84, 5, 49, 92, 5, 12289, 82, 5, 13, 90, 5, 3073, 86, 5, 193, 192, 5, 24577 };

        // Tables for deflate from PKZIP's appnote.txt.
        //UPGRADE_NOTE: Final was removed from the declaration of 'cplens'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly int[] cplens = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258, 0, 0 };

        // see note #13 above about 258
        //UPGRADE_NOTE: Final was removed from the declaration of 'cplext'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly int[] cplext = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0, 112, 112 };

        //UPGRADE_NOTE: Final was removed from the declaration of 'cpdist'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly int[] cpdist = new int[] { 1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577 };

        //UPGRADE_NOTE: Final was removed from the declaration of 'cpdext'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
        internal static readonly int[] cpdext = new int[] { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13 };

        // If BMAX needs to be larger than 16, then h and x[] should be uLong.
        internal const int BMAX = 15; // maximum bit length of any code

        internal int[] hn = null; // hufts used in space
        internal int[] v = null; // work area for huft_build 
        internal int[] c = null; // bit length count table
        internal int[] r = null; // table entry for structure assignment
        internal int[] u = null; // table stack
        internal int[] x = null; // bit offsets, then code stack

        private int huft_build(int[] b, int bindex, int n, int s, int[] d, int[] e, int[] t, int[] m, int[] hp, int[] hn, int[] v)
        {
            // Given a list of code lengths and a maximum table size, make a set of
            // tables to decode that set of codes.  Return Z_OK on success, Z_BUF_ERROR
            // if the given code set is incomplete (the tables are still built in this
            // case), Z_DATA_ERROR if the input is invalid (an over-subscribed set of
            // lengths), or Z_MEM_ERROR if not enough memory.

            int a; // counter for codes of length k
            int f; // i repeats in table every f entries
            int g; // maximum code length
            int h; // table level
            int i; // counter, current code
            int j; // counter
            int k; // number of bits in current code
            int l; // bits per table (returned in m)
            int mask; // (1 << w) - 1, to avoid cc -O bug on HP
            int p; // pointer into c[], b[], or v[]
            int q; // points to current table
            int w; // bits before this table == (l * h)
            int xp; // pointer into x
            int y; // number of dummy codes added
            int z; // number of entries in current table

            // Generate counts for each bit length

            p = 0; i = n;
            do
            {
                c[b[bindex + p]]++; p++; i--; // assume all entries <= BMAX
            }
            while (i != 0);

            if (c[0] == n)
            {
                // null input--all zero length codes
                t[0] = -1;
                m[0] = 0;
                return Z_OK;
            }

            // Find minimum and maximum length, bound *m by those
            l = m[0];
            for (j = 1; j <= BMAX; j++)
                if (c[j] != 0)
                    break;
            k = j; // minimum code length
            if (l < j)
            {
                l = j;
            }
            for (i = BMAX; i != 0; i--)
            {
                if (c[i] != 0)
                    break;
            }
            g = i; // maximum code length
            if (l > i)
            {
                l = i;
            }
            m[0] = l;

            // Adjust last length count to fill out codes, if needed
            for (y = 1 << j; j < i; j++, y <<= 1)
            {
                if ((y -= c[j]) < 0)
                {
                    return Z_DATA_ERROR;
                }
            }
            if ((y -= c[i]) < 0)
            {
                return Z_DATA_ERROR;
            }
            c[i] += y;

            // Generate starting offsets into the value table for each length
            x[1] = j = 0;
            p = 1; xp = 2;
            while (--i != 0)
            {
                // note that i == g from above
                x[xp] = (j += c[p]);
                xp++;
                p++;
            }

            // Make a table of values in order of bit lengths
            i = 0; p = 0;
            do
            {
                if ((j = b[bindex + p]) != 0)
                {
                    v[x[j]++] = i;
                }
                p++;
            }
            while (++i < n);
            n = x[g]; // set n to length of v

            // Generate the Huffman codes and for each, make the table entries
            x[0] = i = 0; // first Huffman code is zero
            p = 0; // grab values in bit order
            h = -1; // no tables yet--level -1
            w = -l; // bits decoded == (l * h)
            u[0] = 0; // just to keep compilers happy
            q = 0; // ditto
            z = 0; // ditto

            // go through the bit lengths (k already is bits in shortest code)
            for (; k <= g; k++)
            {
                a = c[k];
                while (a-- != 0)
                {
                    // here i is the Huffman code of length k bits for value *p
                    // make tables up to required level
                    while (k > w + l)
                    {
                        h++;
                        w += l; // previous table always l bits
                        // compute minimum size table less than or equal to l bits
                        z = g - w;
                        z = (z > l) ? l : z; // table size upper limit
                        if ((f = 1 << (j = k - w)) > a + 1)
                        {
                            // try a k-w bit table
                            // too few codes for k-w bit table
                            f -= (a + 1); // deduct codes from patterns left
                            xp = k;
                            if (j < z)
                            {
                                while (++j < z)
                                {
                                    // try smaller tables up to z bits
                                    if ((f <<= 1) <= c[++xp])
                                        break; // enough codes to use up j bits
                                    f -= c[xp]; // else deduct codes from patterns
                                }
                            }
                        }
                        z = 1 << j; // table entries for j-bit table

                        // allocate new table
                        if (hn[0] + z > MANY)
                        {
                            // (note: doesn't matter for fixed)
                            return Z_DATA_ERROR; // overflow of MANY
                        }
                        u[h] = q = hn[0]; // DEBUG
                        hn[0] += z;

                        // connect to last table, if there is one
                        if (h != 0)
                        {
                            x[h] = i; // save pattern for backing up
                            r[0] = (sbyte)j; // bits in this table
                            r[1] = (sbyte)l; // bits to dump before this table
                            j = SharedUtils.URShift(i, (w - l));
                            r[2] = (int)(q - u[h - 1] - j); // offset to this table
                            Array.Copy(r, 0, hp, (u[h - 1] + j) * 3, 3); // connect to last table
                        }
                        else
                        {
                            t[0] = q; // first table is returned result
                        }
                    }

                    // set up table entry in r
                    r[1] = (sbyte)(k - w);
                    if (p >= n)
                    {
                        r[0] = 128 + 64; // out of values--invalid code
                    }
                    else if (v[p] < s)
                    {
                        r[0] = (sbyte)(v[p] < 256 ? 0 : 32 + 64); // 256 is end-of-block
                        r[2] = v[p++]; // simple code is just the value
                    }
                    else
                    {
                        r[0] = (sbyte)(e[v[p] - s] + 16 + 64); // non-simple--look up in lists
                        r[2] = d[v[p++] - s];
                    }

                    // fill code-like entries with r
                    f = 1 << (k - w);
                    for (j = SharedUtils.URShift(i, w); j < z; j += f)
                    {
                        Array.Copy(r, 0, hp, (q + j) * 3, 3);
                    }

                    // backwards increment the k-bit code i
                    for (j = 1 << (k - 1); (i & j) != 0; j = SharedUtils.URShift(j, 1))
                    {
                        i ^= j;
                    }
                    i ^= j;

                    // backup over finished tables
                    mask = (1 << w) - 1; // needed on HP, cc -O bug
                    while ((i & mask) != x[h])
                    {
                        h--; // don't need to update q
                        w -= l;
                        mask = (1 << w) - 1;
                    }
                }
            }
            // Return Z_BUF_ERROR if we were given an incomplete table
            return y != 0 && g != 1 ? Z_BUF_ERROR : Z_OK;
        }

        internal int inflate_trees_bits(int[] c, int[] bb, int[] tb, int[] hp, ZlibCodec z)
        {
            int result;
            initWorkArea(19);
            hn[0] = 0;
            result = huft_build(c, 0, 19, 19, null, null, tb, bb, hp, hn, v);

            if (result == Z_DATA_ERROR)
            {
                z.Message = "oversubscribed dynamic bit lengths tree";
            }
            else if (result == Z_BUF_ERROR || bb[0] == 0)
            {
                z.Message = "incomplete dynamic bit lengths tree";
                result = Z_DATA_ERROR;
            }
            return result;
        }

        internal int inflate_trees_dynamic(int nl, int nd, int[] c, int[] bl, int[] bd, int[] tl, int[] td, int[] hp, ZlibCodec z)
        {
            int result;

            // build literal/length tree
            initWorkArea(288);
            hn[0] = 0;
            result = huft_build(c, 0, nl, 257, cplens, cplext, tl, bl, hp, hn, v);
            if (result != Z_OK || bl[0] == 0)
            {
                if (result == Z_DATA_ERROR)
                {
                    z.Message = "oversubscribed literal/length tree";
                }
                else if (result != Z_MEM_ERROR)
                {
                    z.Message = "incomplete literal/length tree";
                    result = Z_DATA_ERROR;
                }
                return result;
            }

            // build distance tree
            initWorkArea(288);
            result = huft_build(c, nl, nd, 0, cpdist, cpdext, td, bd, hp, hn, v);

            if (result != Z_OK || (bd[0] == 0 && nl > 257))
            {
                if (result == Z_DATA_ERROR)
                {
                    z.Message = "oversubscribed distance tree";
                }
                else if (result == Z_BUF_ERROR)
                {
                    z.Message = "incomplete distance tree";
                    result = Z_DATA_ERROR;
                }
                else if (result != Z_MEM_ERROR)
                {
                    z.Message = "empty distance tree with lengths";
                    result = Z_DATA_ERROR;
                }
                return result;
            }

            return Z_OK;
        }

        internal static int inflate_trees_fixed(int[] bl, int[] bd, int[][] tl, int[][] td, ZlibCodec z)
        {
            bl[0] = fixed_bl;
            bd[0] = fixed_bd;
            tl[0] = fixed_tl;
            td[0] = fixed_td;
            return Z_OK;
        }

        private void initWorkArea(int vsize)
        {
            if (hn == null)
            {
                hn = new int[1];
                v = new int[vsize];
                c = new int[BMAX + 1];
                r = new int[3];
                u = new int[BMAX];
                x = new int[BMAX + 1];
            }
            else
            {
                if (v.Length < vsize)
                {
                    v = new int[vsize];
                }
                Array.Clear(v, 0, vsize);
                Array.Clear(c, 0, BMAX + 1);
                r[0] = 0; r[1] = 0; r[2] = 0;
                //  for(int i=0; i<BMAX; i++){u[i]=0;}
                //Array.Copy(c, 0, u, 0, BMAX);
                Array.Clear(u, 0, BMAX);
                //  for(int i=0; i<BMAX+1; i++){x[i]=0;}
                //Array.Copy(c, 0, x, 0, BMAX + 1);
                Array.Clear(x, 0, BMAX + 1);
            }
        }
    }

    sealed class InflateBlocks
    {
        private const int MANY = 1440;

        // Table for deflate from PKZIP's appnote.txt.
        internal static readonly int[] border = new int[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

        private enum InflateBlockMode
        {
            TYPE = 0,                     // get type bits (3, including end bit)
            LENS = 1,                     // get lengths for stored
            STORED = 2,                     // processing stored block
            TABLE = 3,                     // get table lengths
            BTREE = 4,                     // get bit lengths tree for a dynamic block
            DTREE = 5,                     // get length, distance trees for a dynamic block
            CODES = 6,                     // processing fixed or dynamic block
            DRY = 7,                     // output remaining window bytes
            DONE = 8,                     // finished last block, done
            BAD = 9,                     // ot a data error--stuck here
        }

        private InflateBlockMode mode;                    // current inflate_block mode

        internal int left;                                // if STORED, bytes left to copy

        internal int table;                               // table lengths (14 bits)
        internal int index;                               // index into blens (or border)
        internal int[] blens;                             // bit lengths of codes
        internal int[] bb = new int[1];                   // bit length tree depth
        internal int[] tb = new int[1];                   // bit length decoding tree

        internal InflateCodes codes = new InflateCodes(); // if CODES, current state

        internal int last;                                // true if this block is the last block

        internal ZlibCodec _codec;                        // pointer back to this zlib stream

        // mode independent information
        internal int bitk;                                // bits in bit buffer
        internal int bitb;                                // bit buffer
        internal int[] hufts;                             // single malloc for tree space
        internal byte[] window;                           // sliding window
        internal int end;                                 // one byte after sliding window
        internal int readAt;                              // window read pointer
        internal int writeAt;                             // window write pointer
        internal System.Object checkfn;                   // check function
        internal uint check;                              // check on output

        internal InfTree inftree = new InfTree();

        internal InflateBlocks(ZlibCodec codec, System.Object checkfn, int w)
        {
            _codec = codec;
            hufts = new int[MANY * 3];
            window = new byte[w];
            end = w;
            this.checkfn = checkfn;
            mode = InflateBlockMode.TYPE;
            Reset();
        }

        internal uint Reset()
        {
            uint oldCheck = check;
            mode = InflateBlockMode.TYPE;
            bitk = 0;
            bitb = 0;
            readAt = writeAt = 0;

            if (checkfn != null)
                _codec._Adler32 = check = Adler.Adler32(0, null, 0, 0);
            return oldCheck;
        }


        internal int Process(int r)
        {
            int t; // temporary storage
            int b; // bit buffer
            int k; // bits in bit buffer
            int p; // input data pointer
            int n; // bytes available there
            int q; // output window write pointer
            int m; // bytes to end of window or read pointer

            // copy input/output information to locals (UPDATE macro restores)

            p = _codec.NextIn;
            n = _codec.AvailableBytesIn;
            b = bitb;
            k = bitk;

            q = writeAt;
            m = (int)(q < readAt ? readAt - q - 1 : end - q);


            // process input based on current state
            while (true)
            {
                switch (mode)
                {
                    case InflateBlockMode.TYPE:

                        while (k < (3))
                        {
                            if (n != 0)
                            {
                                r = ZlibConstants.Z_OK;
                            }
                            else
                            {
                                bitb = b; bitk = k;
                                _codec.AvailableBytesIn = n;
                                _codec.TotalBytesIn += p - _codec.NextIn;
                                _codec.NextIn = p;
                                writeAt = q;
                                return Flush(r);
                            }

                            n--;
                            b |= (_codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }
                        t = (int)(b & 7);
                        last = t & 1;

                        switch ((uint)t >> 1)
                        {
                            case 0:  // stored
                                b >>= 3; k -= (3);
                                t = k & 7; // go to byte boundary
                                b >>= t; k -= t;
                                mode = InflateBlockMode.LENS; // get length of stored block
                                break;

                            case 1:  // fixed
                                int[] bl = new int[1];
                                int[] bd = new int[1];
                                int[][] tl = new int[1][];
                                int[][] td = new int[1][];
                                InfTree.inflate_trees_fixed(bl, bd, tl, td, _codec);
                                codes.Init(bl[0], bd[0], tl[0], 0, td[0], 0);
                                b >>= 3; k -= 3;
                                mode = InflateBlockMode.CODES;
                                break;

                            case 2:  // dynamic
                                b >>= 3; k -= 3;
                                mode = InflateBlockMode.TABLE;
                                break;

                            case 3:  // illegal
                                b >>= 3; k -= 3;
                                mode = InflateBlockMode.BAD;
                                _codec.Message = "invalid block type";
                                r = ZlibConstants.Z_DATA_ERROR;
                                bitb = b; bitk = k;
                                _codec.AvailableBytesIn = n;
                                _codec.TotalBytesIn += p - _codec.NextIn;
                                _codec.NextIn = p;
                                writeAt = q;
                                return Flush(r);
                        }
                        break;

                    case InflateBlockMode.LENS:

                        while (k < (32))
                        {
                            if (n != 0)
                            {
                                r = ZlibConstants.Z_OK;
                            }
                            else
                            {
                                bitb = b; bitk = k;
                                _codec.AvailableBytesIn = n;
                                _codec.TotalBytesIn += p - _codec.NextIn;
                                _codec.NextIn = p;
                                writeAt = q;
                                return Flush(r);
                            }
                            ;
                            n--;
                            b |= (_codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        if ((((~b) >> 16) & 0xffff) != (b & 0xffff))
                        {
                            mode = InflateBlockMode.BAD;
                            _codec.Message = "invalid stored block lengths";
                            r = ZlibConstants.Z_DATA_ERROR;

                            bitb = b; bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }
                        left = (b & 0xffff);
                        b = k = 0; // dump bits
                        mode = left != 0 ? InflateBlockMode.STORED : (last != 0 ? InflateBlockMode.DRY : InflateBlockMode.TYPE);
                        break;

                    case InflateBlockMode.STORED:
                        if (n == 0)
                        {
                            bitb = b; bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }

                        if (m == 0)
                        {
                            if (q == end && readAt != 0)
                            {
                                q = 0; m = (int)(q < readAt ? readAt - q - 1 : end - q);
                            }
                            if (m == 0)
                            {
                                writeAt = q;
                                r = Flush(r);
                                q = writeAt; m = (int)(q < readAt ? readAt - q - 1 : end - q);
                                if (q == end && readAt != 0)
                                {
                                    q = 0; m = (int)(q < readAt ? readAt - q - 1 : end - q);
                                }
                                if (m == 0)
                                {
                                    bitb = b; bitk = k;
                                    _codec.AvailableBytesIn = n;
                                    _codec.TotalBytesIn += p - _codec.NextIn;
                                    _codec.NextIn = p;
                                    writeAt = q;
                                    return Flush(r);
                                }
                            }
                        }
                        r = ZlibConstants.Z_OK;

                        t = left;
                        if (t > n)
                            t = n;
                        if (t > m)
                            t = m;
                        Array.Copy(_codec.InputBuffer, p, window, q, t);
                        p += t; n -= t;
                        q += t; m -= t;
                        if ((left -= t) != 0)
                            break;
                        mode = last != 0 ? InflateBlockMode.DRY : InflateBlockMode.TYPE;
                        break;

                    case InflateBlockMode.TABLE:

                        while (k < (14))
                        {
                            if (n != 0)
                            {
                                r = ZlibConstants.Z_OK;
                            }
                            else
                            {
                                bitb = b; bitk = k;
                                _codec.AvailableBytesIn = n;
                                _codec.TotalBytesIn += p - _codec.NextIn;
                                _codec.NextIn = p;
                                writeAt = q;
                                return Flush(r);
                            }

                            n--;
                            b |= (_codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        table = t = (b & 0x3fff);
                        if ((t & 0x1f) > 29 || ((t >> 5) & 0x1f) > 29)
                        {
                            mode = InflateBlockMode.BAD;
                            _codec.Message = "too many length or distance symbols";
                            r = ZlibConstants.Z_DATA_ERROR;

                            bitb = b; bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }
                        t = 258 + (t & 0x1f) + ((t >> 5) & 0x1f);
                        if (blens == null || blens.Length < t)
                        {
                            blens = new int[t];
                        }
                        else
                        {
                            Array.Clear(blens, 0, t);
                            // for (int i = 0; i < t; i++)
                            // {
                            //     blens[i] = 0;
                            // }
                        }

                        b >>= 14;
                        k -= 14;


                        index = 0;
                        mode = InflateBlockMode.BTREE;
                        goto case InflateBlockMode.BTREE;

                    case InflateBlockMode.BTREE:
                        while (index < 4 + (table >> 10))
                        {
                            while (k < (3))
                            {
                                if (n != 0)
                                {
                                    r = ZlibConstants.Z_OK;
                                }
                                else
                                {
                                    bitb = b; bitk = k;
                                    _codec.AvailableBytesIn = n;
                                    _codec.TotalBytesIn += p - _codec.NextIn;
                                    _codec.NextIn = p;
                                    writeAt = q;
                                    return Flush(r);
                                }

                                n--;
                                b |= (_codec.InputBuffer[p++] & 0xff) << k;
                                k += 8;
                            }

                            blens[border[index++]] = b & 7;

                            b >>= 3; k -= 3;
                        }

                        while (index < 19)
                        {
                            blens[border[index++]] = 0;
                        }

                        bb[0] = 7;
                        t = inftree.inflate_trees_bits(blens, bb, tb, hufts, _codec);
                        if (t != ZlibConstants.Z_OK)
                        {
                            r = t;
                            if (r == ZlibConstants.Z_DATA_ERROR)
                            {
                                blens = null;
                                mode = InflateBlockMode.BAD;
                            }

                            bitb = b; bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }

                        index = 0;
                        mode = InflateBlockMode.DTREE;
                        goto case InflateBlockMode.DTREE;

                    case InflateBlockMode.DTREE:
                        while (true)
                        {
                            t = table;
                            if (!(index < 258 + (t & 0x1f) + ((t >> 5) & 0x1f)))
                            {
                                break;
                            }

                            int i, j, c;

                            t = bb[0];

                            while (k < t)
                            {
                                if (n != 0)
                                {
                                    r = ZlibConstants.Z_OK;
                                }
                                else
                                {
                                    bitb = b; bitk = k;
                                    _codec.AvailableBytesIn = n;
                                    _codec.TotalBytesIn += p - _codec.NextIn;
                                    _codec.NextIn = p;
                                    writeAt = q;
                                    return Flush(r);
                                }

                                n--;
                                b |= (_codec.InputBuffer[p++] & 0xff) << k;
                                k += 8;
                            }

                            t = hufts[(tb[0] + (b & InternalInflateConstants.InflateMask[t])) * 3 + 1];
                            c = hufts[(tb[0] + (b & InternalInflateConstants.InflateMask[t])) * 3 + 2];

                            if (c < 16)
                            {
                                b >>= t; k -= t;
                                blens[index++] = c;
                            }
                            else
                            {
                                // c == 16..18
                                i = c == 18 ? 7 : c - 14;
                                j = c == 18 ? 11 : 3;

                                while (k < (t + i))
                                {
                                    if (n != 0)
                                    {
                                        r = ZlibConstants.Z_OK;
                                    }
                                    else
                                    {
                                        bitb = b; bitk = k;
                                        _codec.AvailableBytesIn = n;
                                        _codec.TotalBytesIn += p - _codec.NextIn;
                                        _codec.NextIn = p;
                                        writeAt = q;
                                        return Flush(r);
                                    }

                                    n--;
                                    b |= (_codec.InputBuffer[p++] & 0xff) << k;
                                    k += 8;
                                }

                                b >>= t; k -= t;

                                j += (b & InternalInflateConstants.InflateMask[i]);

                                b >>= i; k -= i;

                                i = index;
                                t = table;
                                if (i + j > 258 + (t & 0x1f) + ((t >> 5) & 0x1f) || (c == 16 && i < 1))
                                {
                                    blens = null;
                                    mode = InflateBlockMode.BAD;
                                    _codec.Message = "invalid bit length repeat";
                                    r = ZlibConstants.Z_DATA_ERROR;

                                    bitb = b; bitk = k;
                                    _codec.AvailableBytesIn = n;
                                    _codec.TotalBytesIn += p - _codec.NextIn;
                                    _codec.NextIn = p;
                                    writeAt = q;
                                    return Flush(r);
                                }

                                c = (c == 16) ? blens[i - 1] : 0;
                                do
                                {
                                    blens[i++] = c;
                                }
                                while (--j != 0);
                                index = i;
                            }
                        }

                        tb[0] = -1;
                        {
                            int[] bl = new int[] { 9 };  // must be <= 9 for lookahead assumptions
                            int[] bd = new int[] { 6 }; // must be <= 9 for lookahead assumptions
                            int[] tl = new int[1];
                            int[] td = new int[1];

                            t = table;
                            t = inftree.inflate_trees_dynamic(257 + (t & 0x1f), 1 + ((t >> 5) & 0x1f), blens, bl, bd, tl, td, hufts, _codec);

                            if (t != ZlibConstants.Z_OK)
                            {
                                if (t == ZlibConstants.Z_DATA_ERROR)
                                {
                                    blens = null;
                                    mode = InflateBlockMode.BAD;
                                }
                                r = t;

                                bitb = b; bitk = k;
                                _codec.AvailableBytesIn = n;
                                _codec.TotalBytesIn += p - _codec.NextIn;
                                _codec.NextIn = p;
                                writeAt = q;
                                return Flush(r);
                            }
                            codes.Init(bl[0], bd[0], hufts, tl[0], hufts, td[0]);
                        }
                        mode = InflateBlockMode.CODES;
                        goto case InflateBlockMode.CODES;

                    case InflateBlockMode.CODES:
                        bitb = b; bitk = k;
                        _codec.AvailableBytesIn = n;
                        _codec.TotalBytesIn += p - _codec.NextIn;
                        _codec.NextIn = p;
                        writeAt = q;

                        r = codes.Process(this, r);
                        if (r != ZlibConstants.Z_STREAM_END)
                        {
                            return Flush(r);
                        }

                        r = ZlibConstants.Z_OK;
                        p = _codec.NextIn;
                        n = _codec.AvailableBytesIn;
                        b = bitb;
                        k = bitk;
                        q = writeAt;
                        m = (int)(q < readAt ? readAt - q - 1 : end - q);

                        if (last == 0)
                        {
                            mode = InflateBlockMode.TYPE;
                            break;
                        }
                        mode = InflateBlockMode.DRY;
                        goto case InflateBlockMode.DRY;

                    case InflateBlockMode.DRY:
                        writeAt = q;
                        r = Flush(r);
                        q = writeAt; m = (int)(q < readAt ? readAt - q - 1 : end - q);
                        if (readAt != writeAt)
                        {
                            bitb = b; bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }
                        mode = InflateBlockMode.DONE;
                        goto case InflateBlockMode.DONE;

                    case InflateBlockMode.DONE:
                        r = ZlibConstants.Z_STREAM_END;
                        bitb = b;
                        bitk = k;
                        _codec.AvailableBytesIn = n;
                        _codec.TotalBytesIn += p - _codec.NextIn;
                        _codec.NextIn = p;
                        writeAt = q;
                        return Flush(r);

                    case InflateBlockMode.BAD:
                        r = ZlibConstants.Z_DATA_ERROR;

                        bitb = b; bitk = k;
                        _codec.AvailableBytesIn = n;
                        _codec.TotalBytesIn += p - _codec.NextIn;
                        _codec.NextIn = p;
                        writeAt = q;
                        return Flush(r);


                    default:
                        r = ZlibConstants.Z_STREAM_ERROR;

                        bitb = b; bitk = k;
                        _codec.AvailableBytesIn = n;
                        _codec.TotalBytesIn += p - _codec.NextIn;
                        _codec.NextIn = p;
                        writeAt = q;
                        return Flush(r);
                }
            }
        }


        internal void Free()
        {
            Reset();
            window = null;
            hufts = null;
        }

        internal void SetDictionary(byte[] d, int start, int n)
        {
            Array.Copy(d, start, window, 0, n);
            readAt = writeAt = n;
        }

        // Returns true if inflate is currently at the end of a block generated
        // by Z_SYNC_FLUSH or Z_FULL_FLUSH.
        internal int SyncPoint()
        {
            return mode == InflateBlockMode.LENS ? 1 : 0;
        }

        // copy as much as possible from the sliding window to the output area
        internal int Flush(int r)
        {
            int nBytes;

            for (int pass = 0; pass < 2; pass++)
            {
                if (pass == 0)
                {
                    // compute number of bytes to copy as far as end of window
                    nBytes = (int)((readAt <= writeAt ? writeAt : end) - readAt);
                }
                else
                {
                    // compute bytes to copy
                    nBytes = writeAt - readAt;
                }

                // workitem 8870
                if (nBytes == 0)
                {
                    if (r == ZlibConstants.Z_BUF_ERROR)
                        r = ZlibConstants.Z_OK;
                    return r;
                }

                if (nBytes > _codec.AvailableBytesOut)
                    nBytes = _codec.AvailableBytesOut;

                if (nBytes != 0 && r == ZlibConstants.Z_BUF_ERROR)
                    r = ZlibConstants.Z_OK;

                // update counters
                _codec.AvailableBytesOut -= nBytes;
                _codec.TotalBytesOut += nBytes;

                // update check information
                if (checkfn != null)
                    _codec._Adler32 = check = Adler.Adler32(check, window, readAt, nBytes);

                // copy as far as end of window
                Array.Copy(window, readAt, _codec.OutputBuffer, _codec.NextOut, nBytes);
                _codec.NextOut += nBytes;
                readAt += nBytes;

                // see if more to copy at beginning of window
                if (readAt == end && pass == 0)
                {
                    // wrap pointers
                    readAt = 0;
                    if (writeAt == end)
                        writeAt = 0;
                }
                else pass++;
            }

            // done
            return r;
        }
    }


    internal static class InternalInflateConstants
    {
        // And'ing with mask[n] masks the lower n bits
        internal static readonly int[] InflateMask = new int[] {
            0x00000000, 0x00000001, 0x00000003, 0x00000007,
            0x0000000f, 0x0000001f, 0x0000003f, 0x0000007f,
            0x000000ff, 0x000001ff, 0x000003ff, 0x000007ff,
            0x00000fff, 0x00001fff, 0x00003fff, 0x00007fff, 0x0000ffff };
    }


    sealed class InflateCodes
    {
        // waiting for "i:"=input,
        //             "o:"=output,
        //             "x:"=nothing
        private const int START = 0; // x: set up for LEN
        private const int LEN = 1; // i: get length/literal/eob next
        private const int LENEXT = 2; // i: getting length extra (have base)
        private const int DIST = 3; // i: get distance next
        private const int DISTEXT = 4; // i: getting distance extra
        private const int COPY = 5; // o: copying bytes in window, waiting for space
        private const int LIT = 6; // o: got literal, waiting for output space
        private const int WASH = 7; // o: got eob, possibly still output waiting
        private const int END = 8; // x: got eob and all data flushed
        private const int BADCODE = 9; // x: got error

        internal int mode;        // current inflate_codes mode

        // mode dependent information
        internal int len;

        internal int[] tree;      // pointer into tree
        internal int tree_index = 0;
        internal int need;        // bits needed

        internal int lit;

        // if EXT or COPY, where and how much
        internal int bitsToGet;   // bits to get for extra
        internal int dist;        // distance back to copy from

        internal byte lbits;      // ltree bits decoded per branch
        internal byte dbits;      // dtree bits decoder per branch
        internal int[] ltree;     // literal/length/eob tree
        internal int ltree_index; // literal/length/eob tree
        internal int[] dtree;     // distance tree
        internal int dtree_index; // distance tree

        internal InflateCodes()
        {
        }

        internal void Init(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index)
        {
            mode = START;
            lbits = (byte)bl;
            dbits = (byte)bd;
            ltree = tl;
            ltree_index = tl_index;
            dtree = td;
            dtree_index = td_index;
            tree = null;
        }

        internal int Process(InflateBlocks blocks, int r)
        {
            int j;      // temporary storage
            int tindex; // temporary pointer
            int e;      // extra bits or operation
            int b = 0;  // bit buffer
            int k = 0;  // bits in bit buffer
            int p = 0;  // input data pointer
            int n;      // bytes available there
            int q;      // output window write pointer
            int m;      // bytes to end of window or read pointer
            int f;      // pointer to copy strings from

            ZlibCodec z = blocks._codec;

            // copy input/output information to locals (UPDATE macro restores)
            p = z.NextIn;
            n = z.AvailableBytesIn;
            b = blocks.bitb;
            k = blocks.bitk;
            q = blocks.writeAt; m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;

            // process input and output based on current state
            while (true)
            {
                switch (mode)
                {
                    // waiting for "i:"=input, "o:"=output, "x:"=nothing
                    case START:  // x: set up for LEN
                        if (m >= 258 && n >= 10)
                        {
                            blocks.bitb = b; blocks.bitk = k;
                            z.AvailableBytesIn = n;
                            z.TotalBytesIn += p - z.NextIn;
                            z.NextIn = p;
                            blocks.writeAt = q;
                            r = InflateFast(lbits, dbits, ltree, ltree_index, dtree, dtree_index, blocks, z);

                            p = z.NextIn;
                            n = z.AvailableBytesIn;
                            b = blocks.bitb;
                            k = blocks.bitk;
                            q = blocks.writeAt; m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;

                            if (r != ZlibConstants.Z_OK)
                            {
                                mode = (r == ZlibConstants.Z_STREAM_END) ? WASH : BADCODE;
                                break;
                            }
                        }
                        need = lbits;
                        tree = ltree;
                        tree_index = ltree_index;

                        mode = LEN;
                        goto case LEN;

                    case LEN:  // i: get length/literal/eob next
                        j = need;

                        while (k < j)
                        {
                            if (n != 0)
                                r = ZlibConstants.Z_OK;
                            else
                            {
                                blocks.bitb = b; blocks.bitk = k;
                                z.AvailableBytesIn = n;
                                z.TotalBytesIn += p - z.NextIn;
                                z.NextIn = p;
                                blocks.writeAt = q;
                                return blocks.Flush(r);
                            }
                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        tindex = (tree_index + (b & InternalInflateConstants.InflateMask[j])) * 3;

                        b >>= (tree[tindex + 1]);
                        k -= (tree[tindex + 1]);

                        e = tree[tindex];

                        if (e == 0)
                        {
                            // literal
                            lit = tree[tindex + 2];
                            mode = LIT;
                            break;
                        }
                        if ((e & 16) != 0)
                        {
                            // length
                            bitsToGet = e & 15;
                            len = tree[tindex + 2];
                            mode = LENEXT;
                            break;
                        }
                        if ((e & 64) == 0)
                        {
                            // next table
                            need = e;
                            tree_index = tindex / 3 + tree[tindex + 2];
                            break;
                        }
                        if ((e & 32) != 0)
                        {
                            // end of block
                            mode = WASH;
                            break;
                        }
                        mode = BADCODE; // invalid code
                        z.Message = "invalid literal/length code";
                        r = ZlibConstants.Z_DATA_ERROR;

                        blocks.bitb = b; blocks.bitk = k;
                        z.AvailableBytesIn = n;
                        z.TotalBytesIn += p - z.NextIn;
                        z.NextIn = p;
                        blocks.writeAt = q;
                        return blocks.Flush(r);


                    case LENEXT:  // i: getting length extra (have base)
                        j = bitsToGet;

                        while (k < j)
                        {
                            if (n != 0)
                                r = ZlibConstants.Z_OK;
                            else
                            {
                                blocks.bitb = b; blocks.bitk = k;
                                z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                                blocks.writeAt = q;
                                return blocks.Flush(r);
                            }
                            n--; b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        len += (b & InternalInflateConstants.InflateMask[j]);

                        b >>= j;
                        k -= j;

                        need = dbits;
                        tree = dtree;
                        tree_index = dtree_index;
                        mode = DIST;
                        goto case DIST;

                    case DIST:  // i: get distance next
                        j = need;

                        while (k < j)
                        {
                            if (n != 0)
                                r = ZlibConstants.Z_OK;
                            else
                            {
                                blocks.bitb = b; blocks.bitk = k;
                                z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                                blocks.writeAt = q;
                                return blocks.Flush(r);
                            }
                            n--; b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        tindex = (tree_index + (b & InternalInflateConstants.InflateMask[j])) * 3;

                        b >>= tree[tindex + 1];
                        k -= tree[tindex + 1];

                        e = (tree[tindex]);
                        if ((e & 0x10) != 0)
                        {
                            // distance
                            bitsToGet = e & 15;
                            dist = tree[tindex + 2];
                            mode = DISTEXT;
                            break;
                        }
                        if ((e & 64) == 0)
                        {
                            // next table
                            need = e;
                            tree_index = tindex / 3 + tree[tindex + 2];
                            break;
                        }
                        mode = BADCODE; // invalid code
                        z.Message = "invalid distance code";
                        r = ZlibConstants.Z_DATA_ERROR;

                        blocks.bitb = b; blocks.bitk = k;
                        z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                        blocks.writeAt = q;
                        return blocks.Flush(r);


                    case DISTEXT:  // i: getting distance extra
                        j = bitsToGet;

                        while (k < j)
                        {
                            if (n != 0)
                                r = ZlibConstants.Z_OK;
                            else
                            {
                                blocks.bitb = b; blocks.bitk = k;
                                z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                                blocks.writeAt = q;
                                return blocks.Flush(r);
                            }
                            n--; b |= (z.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        dist += (b & InternalInflateConstants.InflateMask[j]);

                        b >>= j;
                        k -= j;

                        mode = COPY;
                        goto case COPY;

                    case COPY:  // o: copying bytes in window, waiting for space
                        f = q - dist;
                        while (f < 0)
                        {
                            // modulo window size-"while" instead
                            f += blocks.end; // of "if" handles invalid distances
                        }
                        while (len != 0)
                        {
                            if (m == 0)
                            {
                                if (q == blocks.end && blocks.readAt != 0)
                                {
                                    q = 0; m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;
                                }
                                if (m == 0)
                                {
                                    blocks.writeAt = q; r = blocks.Flush(r);
                                    q = blocks.writeAt; m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;

                                    if (q == blocks.end && blocks.readAt != 0)
                                    {
                                        q = 0; m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;
                                    }

                                    if (m == 0)
                                    {
                                        blocks.bitb = b; blocks.bitk = k;
                                        z.AvailableBytesIn = n;
                                        z.TotalBytesIn += p - z.NextIn;
                                        z.NextIn = p;
                                        blocks.writeAt = q;
                                        return blocks.Flush(r);
                                    }
                                }
                            }

                            blocks.window[q++] = blocks.window[f++]; m--;

                            if (f == blocks.end)
                                f = 0;
                            len--;
                        }
                        mode = START;
                        break;

                    case LIT:  // o: got literal, waiting for output space
                        if (m == 0)
                        {
                            if (q == blocks.end && blocks.readAt != 0)
                            {
                                q = 0; m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;
                            }
                            if (m == 0)
                            {
                                blocks.writeAt = q; r = blocks.Flush(r);
                                q = blocks.writeAt; m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;

                                if (q == blocks.end && blocks.readAt != 0)
                                {
                                    q = 0; m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;
                                }
                                if (m == 0)
                                {
                                    blocks.bitb = b; blocks.bitk = k;
                                    z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                                    blocks.writeAt = q;
                                    return blocks.Flush(r);
                                }
                            }
                        }
                        r = ZlibConstants.Z_OK;

                        blocks.window[q++] = (byte)lit; m--;

                        mode = START;
                        break;

                    case WASH:  // o: got eob, possibly more output
                        if (k > 7)
                        {
                            // return unused byte, if any
                            k -= 8;
                            n++;
                            p--; // can always return one
                        }

                        blocks.writeAt = q; r = blocks.Flush(r);
                        q = blocks.writeAt; m = q < blocks.readAt ? blocks.readAt - q - 1 : blocks.end - q;

                        if (blocks.readAt != blocks.writeAt)
                        {
                            blocks.bitb = b; blocks.bitk = k;
                            z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                            blocks.writeAt = q;
                            return blocks.Flush(r);
                        }
                        mode = END;
                        goto case END;

                    case END:
                        r = ZlibConstants.Z_STREAM_END;
                        blocks.bitb = b; blocks.bitk = k;
                        z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                        blocks.writeAt = q;
                        return blocks.Flush(r);

                    case BADCODE:  // x: got error

                        r = ZlibConstants.Z_DATA_ERROR;

                        blocks.bitb = b; blocks.bitk = k;
                        z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                        blocks.writeAt = q;
                        return blocks.Flush(r);

                    default:
                        r = ZlibConstants.Z_STREAM_ERROR;

                        blocks.bitb = b; blocks.bitk = k;
                        z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                        blocks.writeAt = q;
                        return blocks.Flush(r);
                }
            }
        }


        // Called with number of bytes left to write in window at least 258
        // (the maximum string length) and number of input bytes available
        // at least ten.  The ten bytes are six bytes for the longest length/
        // distance pair plus four bytes for overloading the bit buffer.

        internal int InflateFast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InflateBlocks s, ZlibCodec z)
        {
            int t;        // temporary pointer
            int[] tp;     // temporary pointer
            int tp_index; // temporary pointer
            int e;        // extra bits or operation
            int b;        // bit buffer
            int k;        // bits in bit buffer
            int p;        // input data pointer
            int n;        // bytes available there
            int q;        // output window write pointer
            int m;        // bytes to end of window or read pointer
            int ml;       // mask for literal/length tree
            int md;       // mask for distance tree
            int c;        // bytes to copy
            int d;        // distance back to copy from
            int r;        // copy source pointer

            int tp_index_t_3; // (tp_index+t)*3

            // load input, output, bit values
            p = z.NextIn; n = z.AvailableBytesIn; b = s.bitb; k = s.bitk;
            q = s.writeAt; m = q < s.readAt ? s.readAt - q - 1 : s.end - q;

            // initialize masks
            ml = InternalInflateConstants.InflateMask[bl];
            md = InternalInflateConstants.InflateMask[bd];

            // do until not enough input or output space for fast loop
            do
            {
                // assume called with m >= 258 && n >= 10
                // get literal/length code
                while (k < (20))
                {
                    // max bits for literal/length code
                    n--;
                    b |= (z.InputBuffer[p++] & 0xff) << k; k += 8;
                }

                t = b & ml;
                tp = tl;
                tp_index = tl_index;
                tp_index_t_3 = (tp_index + t) * 3;
                if ((e = tp[tp_index_t_3]) == 0)
                {
                    b >>= (tp[tp_index_t_3 + 1]); k -= (tp[tp_index_t_3 + 1]);

                    s.window[q++] = (byte)tp[tp_index_t_3 + 2];
                    m--;
                    continue;
                }
                do
                {

                    b >>= (tp[tp_index_t_3 + 1]); k -= (tp[tp_index_t_3 + 1]);

                    if ((e & 16) != 0)
                    {
                        e &= 15;
                        c = tp[tp_index_t_3 + 2] + ((int)b & InternalInflateConstants.InflateMask[e]);

                        b >>= e; k -= e;

                        // decode distance base of block to copy
                        while (k < 15)
                        {
                            // max bits for distance code
                            n--;
                            b |= (z.InputBuffer[p++] & 0xff) << k; k += 8;
                        }

                        t = b & md;
                        tp = td;
                        tp_index = td_index;
                        tp_index_t_3 = (tp_index + t) * 3;
                        e = tp[tp_index_t_3];

                        do
                        {

                            b >>= (tp[tp_index_t_3 + 1]); k -= (tp[tp_index_t_3 + 1]);

                            if ((e & 16) != 0)
                            {
                                // get extra bits to add to distance base
                                e &= 15;
                                while (k < e)
                                {
                                    // get extra bits (up to 13)
                                    n--;
                                    b |= (z.InputBuffer[p++] & 0xff) << k; k += 8;
                                }

                                d = tp[tp_index_t_3 + 2] + (b & InternalInflateConstants.InflateMask[e]);

                                b >>= e; k -= e;

                                // do the copy
                                m -= c;
                                if (q >= d)
                                {
                                    // offset before dest
                                    //  just copy
                                    r = q - d;
                                    if (q - r > 0 && 2 > (q - r))
                                    {
                                        s.window[q++] = s.window[r++]; // minimum count is three,
                                        s.window[q++] = s.window[r++]; // so unroll loop a little
                                        c -= 2;
                                    }
                                    else
                                    {
                                        Array.Copy(s.window, r, s.window, q, 2);
                                        q += 2; r += 2; c -= 2;
                                    }
                                }
                                else
                                {
                                    // else offset after destination
                                    r = q - d;
                                    do
                                    {
                                        r += s.end; // force pointer in window
                                    }
                                    while (r < 0); // covers invalid distances
                                    e = s.end - r;
                                    if (c > e)
                                    {
                                        // if source crosses,
                                        c -= e; // wrapped copy
                                        if (q - r > 0 && e > (q - r))
                                        {
                                            do
                                            {
                                                s.window[q++] = s.window[r++];
                                            }
                                            while (--e != 0);
                                        }
                                        else
                                        {
                                            Array.Copy(s.window, r, s.window, q, e);
                                            q += e; r += e; e = 0;
                                        }
                                        r = 0; // copy rest from start of window
                                    }
                                }

                                // copy all or what's left
                                if (q - r > 0 && c > (q - r))
                                {
                                    do
                                    {
                                        s.window[q++] = s.window[r++];
                                    }
                                    while (--c != 0);
                                }
                                else
                                {
                                    Array.Copy(s.window, r, s.window, q, c);
                                    q += c; r += c; c = 0;
                                }
                                break;
                            }
                            else if ((e & 64) == 0)
                            {
                                t += tp[tp_index_t_3 + 2];
                                t += (b & InternalInflateConstants.InflateMask[e]);
                                tp_index_t_3 = (tp_index + t) * 3;
                                e = tp[tp_index_t_3];
                            }
                            else
                            {
                                z.Message = "invalid distance code";

                                c = z.AvailableBytesIn - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

                                s.bitb = b; s.bitk = k;
                                z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                                s.writeAt = q;

                                return ZlibConstants.Z_DATA_ERROR;
                            }
                        }
                        while (true);
                        break;
                    }

                    if ((e & 64) == 0)
                    {
                        t += tp[tp_index_t_3 + 2];
                        t += (b & InternalInflateConstants.InflateMask[e]);
                        tp_index_t_3 = (tp_index + t) * 3;
                        if ((e = tp[tp_index_t_3]) == 0)
                        {
                            b >>= (tp[tp_index_t_3 + 1]); k -= (tp[tp_index_t_3 + 1]);
                            s.window[q++] = (byte)tp[tp_index_t_3 + 2];
                            m--;
                            break;
                        }
                    }
                    else if ((e & 32) != 0)
                    {
                        c = z.AvailableBytesIn - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

                        s.bitb = b; s.bitk = k;
                        z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                        s.writeAt = q;

                        return ZlibConstants.Z_STREAM_END;
                    }
                    else
                    {
                        z.Message = "invalid literal/length code";

                        c = z.AvailableBytesIn - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

                        s.bitb = b; s.bitk = k;
                        z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
                        s.writeAt = q;

                        return ZlibConstants.Z_DATA_ERROR;
                    }
                }
                while (true);
            }
            while (m >= 258 && n >= 10);

            // not enough input or output--restore pointers and return
            c = z.AvailableBytesIn - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

            s.bitb = b; s.bitk = k;
            z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
            s.writeAt = q;

            return ZlibConstants.Z_OK;
        }
    }


    internal sealed class InflateManager
    {
        // preset dictionary flag in zlib header
        private const int PRESET_DICT = 0x20;

        private const int Z_DEFLATED = 8;

        private enum InflateManagerMode
        {
            METHOD = 0,  // waiting for method byte
            FLAG = 1,  // waiting for flag byte
            DICT4 = 2,  // four dictionary check bytes to go
            DICT3 = 3,  // three dictionary check bytes to go
            DICT2 = 4,  // two dictionary check bytes to go
            DICT1 = 5,  // one dictionary check byte to go
            DICT0 = 6,  // waiting for inflateSetDictionary
            BLOCKS = 7,  // decompressing blocks
            CHECK4 = 8,  // four check bytes to go
            CHECK3 = 9,  // three check bytes to go
            CHECK2 = 10, // two check bytes to go
            CHECK1 = 11, // one check byte to go
            DONE = 12, // finished check, done
            BAD = 13, // got an error--stay here
        }

        private InflateManagerMode mode; // current inflate mode
        internal ZlibCodec _codec; // pointer back to this zlib stream

        // mode dependent information
        internal int method; // if FLAGS, method byte

        // if CHECK, check values to compare
        internal uint computedCheck; // computed check value
        internal uint expectedCheck; // stream check value

        // if BAD, inflateSync's marker bytes count
        internal int marker;

        // mode independent information
        //internal int nowrap; // flag for no wrapper
        private bool _handleRfc1950HeaderBytes = true;
        internal bool HandleRfc1950HeaderBytes
        {
            get { return _handleRfc1950HeaderBytes; }
            set { _handleRfc1950HeaderBytes = value; }
        }
        internal int wbits; // log2(window size)  (8..15, defaults to 15)

        internal InflateBlocks blocks; // current inflate_blocks state

        internal InflateManager() { }

        internal InflateManager(bool expectRfc1950HeaderBytes)
        {
            _handleRfc1950HeaderBytes = expectRfc1950HeaderBytes;
        }

        internal int Reset()
        {
            _codec.TotalBytesIn = _codec.TotalBytesOut = 0;
            _codec.Message = null;
            mode = HandleRfc1950HeaderBytes ? InflateManagerMode.METHOD : InflateManagerMode.BLOCKS;
            blocks.Reset();
            return ZlibConstants.Z_OK;
        }

        internal int End()
        {
            if (blocks != null)
                blocks.Free();
            blocks = null;
            return ZlibConstants.Z_OK;
        }

        internal int Initialize(ZlibCodec codec, int w)
        {
            _codec = codec;
            _codec.Message = null;
            blocks = null;

            // handle undocumented nowrap option (no zlib header or check)
            //nowrap = 0;
            //if (w < 0)
            //{
            //    w = - w;
            //    nowrap = 1;
            //}

            // set window size
            if (w < 8 || w > 15)
            {
                End();
                throw new ZlibException("Bad window size.");

                //return ZlibConstants.Z_STREAM_ERROR;
            }
            wbits = w;

            blocks = new InflateBlocks(codec,
                HandleRfc1950HeaderBytes ? this : null,
                1 << w);

            // reset state
            Reset();
            return ZlibConstants.Z_OK;
        }


        internal int Inflate(FlushType flush)
        {
            int b;

            if (_codec.InputBuffer == null)
                throw new ZlibException("InputBuffer is null. ");

            //             int f = (flush == FlushType.Finish)
            //                 ? ZlibConstants.Z_BUF_ERROR
            //                 : ZlibConstants.Z_OK;

            // workitem 8870
            int f = ZlibConstants.Z_OK;
            int r = ZlibConstants.Z_BUF_ERROR;

            while (true)
            {
                switch (mode)
                {
                    case InflateManagerMode.METHOD:
                        if (_codec.AvailableBytesIn == 0) return r;
                        r = f;
                        _codec.AvailableBytesIn--;
                        _codec.TotalBytesIn++;
                        if (((method = _codec.InputBuffer[_codec.NextIn++]) & 0xf) != Z_DEFLATED)
                        {
                            mode = InflateManagerMode.BAD;
                            _codec.Message = String.Format("unknown compression method (0x{0:X2})", method);
                            marker = 5; // can't try inflateSync
                            break;
                        }
                        if ((method >> 4) + 8 > wbits)
                        {
                            mode = InflateManagerMode.BAD;
                            _codec.Message = String.Format("invalid window size ({0})", (method >> 4) + 8);
                            marker = 5; // can't try inflateSync
                            break;
                        }
                        mode = InflateManagerMode.FLAG;
                        break;


                    case InflateManagerMode.FLAG:
                        if (_codec.AvailableBytesIn == 0) return r;
                        r = f;
                        _codec.AvailableBytesIn--;
                        _codec.TotalBytesIn++;
                        b = (_codec.InputBuffer[_codec.NextIn++]) & 0xff;

                        if ((((method << 8) + b) % 31) != 0)
                        {
                            mode = InflateManagerMode.BAD;
                            _codec.Message = "incorrect header check";
                            marker = 5; // can't try inflateSync
                            break;
                        }

                        mode = ((b & PRESET_DICT) == 0)
                            ? InflateManagerMode.BLOCKS
                            : InflateManagerMode.DICT4;
                        break;

                    case InflateManagerMode.DICT4:
                        if (_codec.AvailableBytesIn == 0) return r;
                        r = f;
                        _codec.AvailableBytesIn--;
                        _codec.TotalBytesIn++;
                        expectedCheck = (uint)((_codec.InputBuffer[_codec.NextIn++] << 24) & 0xff000000);
                        mode = InflateManagerMode.DICT3;
                        break;

                    case InflateManagerMode.DICT3:
                        if (_codec.AvailableBytesIn == 0) return r;
                        r = f;
                        _codec.AvailableBytesIn--;
                        _codec.TotalBytesIn++;
                        expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 16) & 0x00ff0000);
                        mode = InflateManagerMode.DICT2;
                        break;

                    case InflateManagerMode.DICT2:

                        if (_codec.AvailableBytesIn == 0) return r;
                        r = f;
                        _codec.AvailableBytesIn--;
                        _codec.TotalBytesIn++;
                        expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 8) & 0x0000ff00);
                        mode = InflateManagerMode.DICT1;
                        break;


                    case InflateManagerMode.DICT1:
                        if (_codec.AvailableBytesIn == 0) return r;
                        r = f;
                        _codec.AvailableBytesIn--; _codec.TotalBytesIn++;
                        expectedCheck += (uint)(_codec.InputBuffer[_codec.NextIn++] & 0x000000ff);
                        _codec._Adler32 = expectedCheck;
                        mode = InflateManagerMode.DICT0;
                        return ZlibConstants.Z_NEED_DICT;


                    case InflateManagerMode.DICT0:
                        mode = InflateManagerMode.BAD;
                        _codec.Message = "need dictionary";
                        marker = 0; // can try inflateSync
                        return ZlibConstants.Z_STREAM_ERROR;


                    case InflateManagerMode.BLOCKS:
                        r = blocks.Process(r);
                        if (r == ZlibConstants.Z_DATA_ERROR)
                        {
                            mode = InflateManagerMode.BAD;
                            marker = 0; // can try inflateSync
                            break;
                        }

                        if (r == ZlibConstants.Z_OK) r = f;

                        if (r != ZlibConstants.Z_STREAM_END)
                            return r;

                        r = f;
                        computedCheck = blocks.Reset();
                        if (!HandleRfc1950HeaderBytes)
                        {
                            mode = InflateManagerMode.DONE;
                            return ZlibConstants.Z_STREAM_END;
                        }
                        mode = InflateManagerMode.CHECK4;
                        break;

                    case InflateManagerMode.CHECK4:
                        if (_codec.AvailableBytesIn == 0) return r;
                        r = f;
                        _codec.AvailableBytesIn--;
                        _codec.TotalBytesIn++;
                        expectedCheck = (uint)((_codec.InputBuffer[_codec.NextIn++] << 24) & 0xff000000);
                        mode = InflateManagerMode.CHECK3;
                        break;

                    case InflateManagerMode.CHECK3:
                        if (_codec.AvailableBytesIn == 0) return r;
                        r = f;
                        _codec.AvailableBytesIn--; _codec.TotalBytesIn++;
                        expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 16) & 0x00ff0000);
                        mode = InflateManagerMode.CHECK2;
                        break;

                    case InflateManagerMode.CHECK2:
                        if (_codec.AvailableBytesIn == 0) return r;
                        r = f;
                        _codec.AvailableBytesIn--;
                        _codec.TotalBytesIn++;
                        expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 8) & 0x0000ff00);
                        mode = InflateManagerMode.CHECK1;
                        break;

                    case InflateManagerMode.CHECK1:
                        if (_codec.AvailableBytesIn == 0) return r;
                        r = f;
                        _codec.AvailableBytesIn--; _codec.TotalBytesIn++;
                        expectedCheck += (uint)(_codec.InputBuffer[_codec.NextIn++] & 0x000000ff);
                        if (computedCheck != expectedCheck)
                        {
                            mode = InflateManagerMode.BAD;
                            _codec.Message = "incorrect data check";
                            marker = 5; // can't try inflateSync
                            break;
                        }
                        mode = InflateManagerMode.DONE;
                        return ZlibConstants.Z_STREAM_END;

                    case InflateManagerMode.DONE:
                        return ZlibConstants.Z_STREAM_END;

                    case InflateManagerMode.BAD:
                        throw new ZlibException(String.Format("Bad state ({0})", _codec.Message));

                    default:
                        throw new ZlibException("Stream error.");

                }
            }
        }



        internal int SetDictionary(byte[] dictionary)
        {
            int index = 0;
            int length = dictionary.Length;
            if (mode != InflateManagerMode.DICT0)
                throw new ZlibException("Stream error.");

            if (Adler.Adler32(1, dictionary, 0, dictionary.Length) != _codec._Adler32)
            {
                return ZlibConstants.Z_DATA_ERROR;
            }

            _codec._Adler32 = Adler.Adler32(0, null, 0, 0);

            if (length >= (1 << wbits))
            {
                length = (1 << wbits) - 1;
                index = dictionary.Length - length;
            }
            blocks.SetDictionary(dictionary, index, length);
            mode = InflateManagerMode.BLOCKS;
            return ZlibConstants.Z_OK;
        }


        private static readonly byte[] mark = new byte[] { 0, 0, 0xff, 0xff };

        internal int Sync()
        {
            int n; // number of bytes to look at
            int p; // pointer to bytes
            int m; // number of marker bytes found in a row
            long r, w; // temporaries to save total_in and total_out

            // set up
            if (mode != InflateManagerMode.BAD)
            {
                mode = InflateManagerMode.BAD;
                marker = 0;
            }
            if ((n = _codec.AvailableBytesIn) == 0)
                return ZlibConstants.Z_BUF_ERROR;
            p = _codec.NextIn;
            m = marker;

            // search
            while (n != 0 && m < 4)
            {
                if (_codec.InputBuffer[p] == mark[m])
                {
                    m++;
                }
                else if (_codec.InputBuffer[p] != 0)
                {
                    m = 0;
                }
                else
                {
                    m = 4 - m;
                }
                p++; n--;
            }

            // restore
            _codec.TotalBytesIn += p - _codec.NextIn;
            _codec.NextIn = p;
            _codec.AvailableBytesIn = n;
            marker = m;

            // return no joy or set up to restart on a new block
            if (m != 4)
            {
                return ZlibConstants.Z_DATA_ERROR;
            }
            r = _codec.TotalBytesIn;
            w = _codec.TotalBytesOut;
            Reset();
            _codec.TotalBytesIn = r;
            _codec.TotalBytesOut = w;
            mode = InflateManagerMode.BLOCKS;
            return ZlibConstants.Z_OK;
        }


        // Returns true if inflate is currently at the end of a block generated
        // by Z_SYNC_FLUSH or Z_FULL_FLUSH. This function is used by one PPP
        // implementation to provide an additional safety check. PPP uses Z_SYNC_FLUSH
        // but removes the length bytes of the resulting empty stored block. When
        // decompressing, PPP checks that at the end of input packet, inflate is
        // waiting for these length bytes.
        internal int SyncPoint(ZlibCodec z)
        {
            return blocks.SyncPoint();
        }
    }

    /// <summary>
    ///   A class for compressing and decompressing GZIP streams.
    /// </summary>
    /// <remarks>
    ///
    /// <para>
    ///   The <c>GZipStream</c> is a <see
    ///   href="http://en.wikipedia.org/wiki/Decorator_pattern">Decorator</see> on a
    ///   <see cref="Stream"/>. It adds GZIP compression or decompression to any
    ///   stream.
    /// </para>
    ///
    /// <para>
    ///   Like the <c>System.IO.Compression.GZipStream</c> in the .NET Base Class Library, the
    ///   <c>Ionic.Zlib.GZipStream</c> can compress while writing, or decompress while
    ///   reading, but not vice versa.  The compression method used is GZIP, which is
    ///   documented in <see href="http://www.ietf.org/rfc/rfc1952.txt">IETF RFC
    ///   1952</see>, "GZIP file format specification version 4.3".</para>
    ///
    /// <para>
    ///   A <c>GZipStream</c> can be used to decompress data (through <c>Read()</c>) or
    ///   to compress data (through <c>Write()</c>), but not both.
    /// </para>
    ///
    /// <para>
    ///   If you wish to use the <c>GZipStream</c> to compress data, you must wrap it
    ///   around a write-able stream. As you call <c>Write()</c> on the <c>GZipStream</c>, the
    ///   data will be compressed into the GZIP format.  If you want to decompress data,
    ///   you must wrap the <c>GZipStream</c> around a readable stream that contains an
    ///   IETF RFC 1952-compliant stream.  The data will be decompressed as you call
    ///   <c>Read()</c> on the <c>GZipStream</c>.
    /// </para>
    ///
    /// <para>
    ///   Though the GZIP format allows data from multiple files to be concatenated
    ///   together, this stream handles only a single segment of GZIP format, typically
    ///   representing a single file.
    /// </para>
    ///
    ///
    /// </remarks>
    internal class GZipStream : Stream
    {
        // GZip format
        // source: http://tools.ietf.org/html/rfc1952
        //
        //  header id:           2 bytes    1F 8B
        //  compress method      1 byte     8= DEFLATE (none other supported)
        //  flag                 1 byte     bitfield (See below)
        //  mtime                4 bytes    time_t (seconds since jan 1, 1970 UTC of the file.
        //  xflg                 1 byte     2 = max compress used , 4 = max speed (can be ignored)
        //  OS                   1 byte     OS for originating archive. set to 0xFF in compression.
        //  extra field length   2 bytes    optional - only if FEXTRA is set.
        //  extra field          varies
        //  filename             varies     optional - if FNAME is set.  zero terminated. ISO-8859-1.
        //  file comment         varies     optional - if FCOMMENT is set. zero terminated. ISO-8859-1.
        //  crc16                1 byte     optional - present only if FHCRC bit is set
        //  compressed data      varies
        //  CRC32                4 bytes
        //  isize                4 bytes    data size modulo 2^32
        //
        //     FLG (FLaGs)
        //                bit 0   FTEXT - indicates file is ASCII text (can be safely ignored)
        //                bit 1   FHCRC - there is a CRC16 for the header immediately following the header
        //                bit 2   FEXTRA - extra fields are present
        //                bit 3   FNAME - the zero-terminated filename is present. encoding; ISO-8859-1.
        //                bit 4   FCOMMENT  - a zero-terminated file comment is present. encoding: ISO-8859-1
        //                bit 5   reserved
        //                bit 6   reserved
        //                bit 7   reserved
        //
        // On consumption:
        // Extra field is a bunch of nonsense and can be safely ignored.
        // Header CRC and OS, likewise.
        //
        // on generation:
        // all optional fields get 0, except for the OS, which gets 255.
        //



        /// <summary>
        ///   The comment on the GZIP stream.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The GZIP format allows for each file to optionally have an associated
        ///   comment stored with the file.  The comment is encoded with the ISO-8859-1
        ///   code page.  To include a comment in a GZIP stream you create, set this
        ///   property before calling <c>Write()</c> for the first time on the
        ///   <c>GZipStream</c>.
        /// </para>
        ///
        /// <para>
        ///   When using <c>GZipStream</c> to decompress, you can retrieve this property
        ///   after the first call to <c>Read()</c>.  If no comment has been set in the
        ///   GZIP bytestream, the Comment property will return <c>null</c>
        ///   (<c>Nothing</c> in VB).
        /// </para>
        /// </remarks>
        internal String Comment
        {
            get
            {
                return _Comment;
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("GZipStream");
                _Comment = value;
            }
        }

        /// <summary>
        ///   The FileName for the GZIP stream.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   The GZIP format optionally allows each file to have an associated
        ///   filename.  When compressing data (through <c>Write()</c>), set this
        ///   FileName before calling <c>Write()</c> the first time on the <c>GZipStream</c>.
        ///   The actual filename is encoded into the GZIP bytestream with the
        ///   ISO-8859-1 code page, according to RFC 1952. It is the application's
        ///   responsibility to insure that the FileName can be encoded and decoded
        ///   correctly with this code page.
        /// </para>
        ///
        /// <para>
        ///   When decompressing (through <c>Read()</c>), you can retrieve this value
        ///   any time after the first <c>Read()</c>.  In the case where there was no filename
        ///   encoded into the GZIP bytestream, the property will return <c>null</c> (<c>Nothing</c>
        ///   in VB).
        /// </para>
        /// </remarks>
        internal String FileName
        {
            get { return _FileName; }
            set
            {
                if (_disposed) throw new ObjectDisposedException("GZipStream");
                _FileName = value;
                if (_FileName == null) return;
                if (_FileName.IndexOf("/") != -1)
                {
                    _FileName = _FileName.Replace("/", "\\");
                }
                if (_FileName.EndsWith("\\"))
                    throw new Exception("Illegal filename");
                if (_FileName.IndexOf("\\") != -1)
                {
                    // trim any leading path
                    _FileName = Path.GetFileName(_FileName);
                }
            }
        }

        /// <summary>
        ///   The last modified time for the GZIP stream.
        /// </summary>
        ///
        /// <remarks>
        ///   GZIP allows the storage of a last modified time with each GZIP entry.
        ///   When compressing data, you can set this before the first call to
        ///   <c>Write()</c>.  When decompressing, you can retrieve this value any time
        ///   after the first call to <c>Read()</c>.
        /// </remarks>
        internal DateTime? LastModified;

        /// <summary>
        /// The CRC on the GZIP stream.
        /// </summary>
        /// <remarks>
        /// This is used for internal error checking. You probably don't need to look at this property.
        /// </remarks>
        internal int Crc32 { get { return _Crc32; } }

        private int _headerByteCount;
        internal ZlibBaseStream _baseStream;
        bool _disposed;
        bool _firstReadDone;
        string _FileName;
        string _Comment;
        int _Crc32;


        /// <summary>
        ///   Create a <c>GZipStream</c> using the specified <c>CompressionMode</c>.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   When mode is <c>CompressionMode.Compress</c>, the <c>GZipStream</c> will use the
        ///   default compression level.
        /// </para>
        ///
        /// <para>
        ///   As noted in the class documentation, the <c>CompressionMode</c> (Compress
        ///   or Decompress) also establishes the "direction" of the stream.  A
        ///   <c>GZipStream</c> with <c>CompressionMode.Compress</c> works only through
        ///   <c>Write()</c>.  A <c>GZipStream</c> with
        ///   <c>CompressionMode.Decompress</c> works only through <c>Read()</c>.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///   This example shows how to use a GZipStream to compress data.
        /// <code>
        /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        /// {
        ///     using (var raw = System.IO.File.Create(outputFile))
        ///     {
        ///         using (Stream compressor = new GZipStream(raw, CompressionMode.Compress))
        ///         {
        ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        ///             int n;
        ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
        ///             {
        ///                 compressor.Write(buffer, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// <code lang="VB">
        /// Dim outputFile As String = (fileToCompress &amp; ".compressed")
        /// Using input As Stream = File.OpenRead(fileToCompress)
        ///     Using raw As FileStream = File.Create(outputFile)
        ///     Using compressor As Stream = New GZipStream(raw, CompressionMode.Compress)
        ///         Dim buffer As Byte() = New Byte(4096) {}
        ///         Dim n As Integer = -1
        ///         Do While (n &lt;&gt; 0)
        ///             If (n &gt; 0) Then
        ///                 compressor.Write(buffer, 0, n)
        ///             End If
        ///             n = input.Read(buffer, 0, buffer.Length)
        ///         Loop
        ///     End Using
        ///     End Using
        /// End Using
        /// </code>
        /// </example>
        ///
        /// <example>
        /// This example shows how to use a GZipStream to uncompress a file.
        /// <code>
        /// private void GunZipFile(string filename)
        /// {
        ///     if (!filename.EndsWith(".gz))
        ///         throw new ArgumentException("filename");
        ///     var DecompressedFile = filename.Substring(0,filename.Length-3);
        ///     byte[] working = new byte[WORKING_BUFFER_SIZE];
        ///     int n= 1;
        ///     using (System.IO.Stream input = System.IO.File.OpenRead(filename))
        ///     {
        ///         using (Stream decompressor= new Ionic.Zlib.GZipStream(input, CompressionMode.Decompress, true))
        ///         {
        ///             using (var output = System.IO.File.Create(DecompressedFile))
        ///             {
        ///                 while (n !=0)
        ///                 {
        ///                     n= decompressor.Read(working, 0, working.Length);
        ///                     if (n > 0)
        ///                     {
        ///                         output.Write(working, 0, n);
        ///                     }
        ///                 }
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Private Sub GunZipFile(ByVal filename as String)
        ///     If Not (filename.EndsWith(".gz)) Then
        ///         Throw New ArgumentException("filename")
        ///     End If
        ///     Dim DecompressedFile as String = filename.Substring(0,filename.Length-3)
        ///     Dim working(WORKING_BUFFER_SIZE) as Byte
        ///     Dim n As Integer = 1
        ///     Using input As Stream = File.OpenRead(filename)
        ///         Using decompressor As Stream = new Ionic.Zlib.GZipStream(input, CompressionMode.Decompress, True)
        ///             Using output As Stream = File.Create(UncompressedFile)
        ///                 Do
        ///                     n= decompressor.Read(working, 0, working.Length)
        ///                     If n > 0 Then
        ///                         output.Write(working, 0, n)
        ///                     End IF
        ///                 Loop While (n  > 0)
        ///             End Using
        ///         End Using
        ///     End Using
        /// End Sub
        /// </code>
        /// </example>
        ///
        /// <param name="stream">The stream which will be read or written.</param>
        /// <param name="mode">Indicates whether the GZipStream will compress or decompress.</param>
        internal GZipStream(Stream stream, CompressionMode mode)
            : this(stream, mode, CompressionLevel.Default, false)
        {
        }

        /// <summary>
        ///   Create a <c>GZipStream</c> using the specified <c>CompressionMode</c> and
        ///   the specified <c>CompressionLevel</c>.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   The <c>CompressionMode</c> (Compress or Decompress) also establishes the
        ///   "direction" of the stream.  A <c>GZipStream</c> with
        ///   <c>CompressionMode.Compress</c> works only through <c>Write()</c>.  A
        ///   <c>GZipStream</c> with <c>CompressionMode.Decompress</c> works only
        ///   through <c>Read()</c>.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///
        /// This example shows how to use a <c>GZipStream</c> to compress a file into a .gz file.
        ///
        /// <code>
        /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        /// {
        ///     using (var raw = System.IO.File.Create(fileToCompress + ".gz"))
        ///     {
        ///         using (Stream compressor = new GZipStream(raw,
        ///                                                   CompressionMode.Compress,
        ///                                                   CompressionLevel.BestCompression))
        ///         {
        ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        ///             int n;
        ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
        ///             {
        ///                 compressor.Write(buffer, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Using input As Stream = File.OpenRead(fileToCompress)
        ///     Using raw As FileStream = File.Create(fileToCompress &amp; ".gz")
        ///         Using compressor As Stream = New GZipStream(raw, CompressionMode.Compress, CompressionLevel.BestCompression)
        ///             Dim buffer As Byte() = New Byte(4096) {}
        ///             Dim n As Integer = -1
        ///             Do While (n &lt;&gt; 0)
        ///                 If (n &gt; 0) Then
        ///                     compressor.Write(buffer, 0, n)
        ///                 End If
        ///                 n = input.Read(buffer, 0, buffer.Length)
        ///             Loop
        ///         End Using
        ///     End Using
        /// End Using
        /// </code>
        /// </example>
        /// <param name="stream">The stream to be read or written while deflating or inflating.</param>
        /// <param name="mode">Indicates whether the <c>GZipStream</c> will compress or decompress.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        internal GZipStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : this(stream, mode, level, false)
        {
        }

        /// <summary>
        ///   Create a <c>GZipStream</c> using the specified <c>CompressionMode</c>, and
        ///   explicitly specify whether the stream should be left open after Deflation
        ///   or Inflation.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This constructor allows the application to request that the captive stream
        ///   remain open after the deflation or inflation occurs.  By default, after
        ///   <c>Close()</c> is called on the stream, the captive stream is also
        ///   closed. In some cases this is not desired, for example if the stream is a
        ///   memory stream that will be re-read after compressed data has been written
        ///   to it.  Specify true for the <paramref name="leaveOpen"/> parameter to leave
        ///   the stream open.
        /// </para>
        ///
        /// <para>
        ///   The <see cref="CompressionMode"/> (Compress or Decompress) also
        ///   establishes the "direction" of the stream.  A <c>GZipStream</c> with
        ///   <c>CompressionMode.Compress</c> works only through <c>Write()</c>.  A <c>GZipStream</c>
        ///   with <c>CompressionMode.Decompress</c> works only through <c>Read()</c>.
        /// </para>
        ///
        /// <para>
        ///   The <c>GZipStream</c> will use the default compression level. If you want
        ///   to specify the compression level, see <see cref="GZipStream(Stream,
        ///   CompressionMode, CompressionLevel, bool)"/>.
        /// </para>
        ///
        /// <para>
        ///   See the other overloads of this constructor for example code.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="stream">
        ///   The stream which will be read or written. This is called the "captive"
        ///   stream in other places in this documentation.
        /// </param>
        ///
        /// <param name="mode">Indicates whether the GZipStream will compress or decompress.
        /// </param>
        ///
        /// <param name="leaveOpen">
        ///   true if the application would like the base stream to remain open after
        ///   inflation/deflation.
        /// </param>
        internal GZipStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : this(stream, mode, CompressionLevel.Default, leaveOpen)
        {
        }

        /// <summary>
        ///   Create a <c>GZipStream</c> using the specified <c>CompressionMode</c> and the
        ///   specified <c>CompressionLevel</c>, and explicitly specify whether the
        ///   stream should be left open after Deflation or Inflation.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   This constructor allows the application to request that the captive stream
        ///   remain open after the deflation or inflation occurs.  By default, after
        ///   <c>Close()</c> is called on the stream, the captive stream is also
        ///   closed. In some cases this is not desired, for example if the stream is a
        ///   memory stream that will be re-read after compressed data has been written
        ///   to it.  Specify true for the <paramref name="leaveOpen"/> parameter to
        ///   leave the stream open.
        /// </para>
        ///
        /// <para>
        ///   As noted in the class documentation, the <c>CompressionMode</c> (Compress
        ///   or Decompress) also establishes the "direction" of the stream.  A
        ///   <c>GZipStream</c> with <c>CompressionMode.Compress</c> works only through
        ///   <c>Write()</c>.  A <c>GZipStream</c> with <c>CompressionMode.Decompress</c> works only
        ///   through <c>Read()</c>.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///   This example shows how to use a <c>GZipStream</c> to compress data.
        /// <code>
        /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        /// {
        ///     using (var raw = System.IO.File.Create(outputFile))
        ///     {
        ///         using (Stream compressor = new GZipStream(raw, CompressionMode.Compress, CompressionLevel.BestCompression, true))
        ///         {
        ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        ///             int n;
        ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
        ///             {
        ///                 compressor.Write(buffer, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// <code lang="VB">
        /// Dim outputFile As String = (fileToCompress &amp; ".compressed")
        /// Using input As Stream = File.OpenRead(fileToCompress)
        ///     Using raw As FileStream = File.Create(outputFile)
        ///     Using compressor As Stream = New GZipStream(raw, CompressionMode.Compress, CompressionLevel.BestCompression, True)
        ///         Dim buffer As Byte() = New Byte(4096) {}
        ///         Dim n As Integer = -1
        ///         Do While (n &lt;&gt; 0)
        ///             If (n &gt; 0) Then
        ///                 compressor.Write(buffer, 0, n)
        ///             End If
        ///             n = input.Read(buffer, 0, buffer.Length)
        ///         Loop
        ///     End Using
        ///     End Using
        /// End Using
        /// </code>
        /// </example>
        /// <param name="stream">The stream which will be read or written.</param>
        /// <param name="mode">Indicates whether the GZipStream will compress or decompress.</param>
        /// <param name="leaveOpen">true if the application would like the stream to remain open after inflation/deflation.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        internal GZipStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            _baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.GZIP, leaveOpen);
        }

        #region Zlib properties

        /// <summary>
        /// This property sets the flush behavior on the stream.
        /// </summary>
        virtual internal FlushType FlushMode
        {
            get { return (this._baseStream._flushMode); }
            set
            {
                if (_disposed) throw new ObjectDisposedException("GZipStream");
                this._baseStream._flushMode = value;
            }
        }

        /// <summary>
        ///   The size of the working buffer for the compression codec.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The working buffer is used for all stream operations.  The default size is
        ///   1024 bytes.  The minimum size is 128 bytes. You may get better performance
        ///   with a larger buffer.  Then again, you might not.  You would have to test
        ///   it.
        /// </para>
        ///
        /// <para>
        ///   Set this before the first call to <c>Read()</c> or <c>Write()</c> on the
        ///   stream. If you try to set it afterwards, it will throw.
        /// </para>
        /// </remarks>
        internal int BufferSize
        {
            get
            {
                return this._baseStream._bufferSize;
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("GZipStream");
                if (this._baseStream._workingBuffer != null)
                    throw new ZlibException("The working buffer is already set.");
                if (value < ZlibConstants.WorkingBufferSizeMin)
                    throw new ZlibException(String.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, ZlibConstants.WorkingBufferSizeMin));
                this._baseStream._bufferSize = value;
            }
        }


        /// <summary> Returns the total number of bytes input so far.</summary>
        virtual internal long TotalIn
        {
            get
            {
                return this._baseStream._z.TotalBytesIn;
            }
        }

        /// <summary> Returns the total number of bytes output so far.</summary>
        virtual internal long TotalOut
        {
            get
            {
                return this._baseStream._z.TotalBytesOut;
            }
        }

        #endregion

        #region Stream methods

        /// <summary>
        ///   Dispose the stream.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This may or may not result in a <c>Close()</c> call on the captive
        ///     stream.  See the constructors that have a <c>leaveOpen</c> parameter
        ///     for more information.
        ///   </para>
        ///   <para>
        ///     This method may be invoked in two distinct scenarios.  If disposing
        ///     == true, the method has been called directly or indirectly by a
        ///     user's code, for example via the internal Dispose() method. In this
        ///     case, both managed and unmanaged resources can be referenced and
        ///     disposed.  If disposing == false, the method has been called by the
        ///     runtime from inside the object finalizer and this method should not
        ///     reference other objects; in that case only unmanaged resources must
        ///     be referenced or disposed.
        ///   </para>
        /// </remarks>
        /// <param name="disposing">
        ///   indicates whether the Dispose method was invoked by user code.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!_disposed)
                {
                    if (disposing && (this._baseStream != null))
                    {
                        this._baseStream.Dispose();
                        this._Crc32 = _baseStream.Crc32;
                    }
                    _disposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }


        /// <summary>
        /// Indicates whether the stream can be read.
        /// </summary>
        /// <remarks>
        /// The return value depends on whether the captive stream supports reading.
        /// </remarks>
        public override bool CanRead
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("GZipStream");
                return _baseStream._stream.CanRead;
            }
        }

        /// <summary>
        /// Indicates whether the stream supports Seek operations.
        /// </summary>
        /// <remarks>
        /// Always returns false.
        /// </remarks>
        public override bool CanSeek
        {
            get { return false; }
        }


        /// <summary>
        /// Indicates whether the stream can be written.
        /// </summary>
        /// <remarks>
        /// The return value depends on whether the captive stream supports writing.
        /// </remarks>
        public override bool CanWrite
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("GZipStream");
                return _baseStream._stream.CanWrite;
            }
        }

        /// <summary>
        /// Flush the stream.
        /// </summary>
        public override void Flush()
        {
            if (_disposed) throw new ObjectDisposedException("GZipStream");
            _baseStream.Flush();
        }

        /// <summary>
        /// Reading this property always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        ///   The position of the stream pointer.
        /// </summary>
        ///
        /// <remarks>
        ///   Setting this property always throws a <see
        ///   cref="NotImplementedException"/>. Reading will return the total bytes
        ///   written out, if used in writing, or the total bytes read in, if used in
        ///   reading.  The count may refer to compressed bytes or uncompressed bytes,
        ///   depending on how you've used the stream.
        /// </remarks>
        public override long Position
        {
            get
            {
                if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
                    return this._baseStream._z.TotalBytesOut + _headerByteCount;
                if (this._baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
                    return this._baseStream._z.TotalBytesIn + this._baseStream._gzipHeaderByteCount;
                return 0;
            }

            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///   Read and decompress data from the source stream.
        /// </summary>
        ///
        /// <remarks>
        ///   With a <c>GZipStream</c>, decompression is done through reading.
        /// </remarks>
        ///
        /// <example>
        /// <code>
        /// byte[] working = new byte[WORKING_BUFFER_SIZE];
        /// using (System.IO.Stream input = System.IO.File.OpenRead(_CompressedFile))
        /// {
        ///     using (Stream decompressor= new Ionic.Zlib.GZipStream(input, CompressionMode.Decompress, true))
        ///     {
        ///         using (var output = System.IO.File.Create(_DecompressedFile))
        ///         {
        ///             int n;
        ///             while ((n= decompressor.Read(working, 0, working.Length)) !=0)
        ///             {
        ///                 output.Write(working, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="buffer">The buffer into which the decompressed data should be placed.</param>
        /// <param name="offset">the offset within that data array to put the first byte read.</param>
        /// <param name="count">the number of bytes to read.</param>
        /// <returns>the number of bytes actually read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException("GZipStream");
            int n = _baseStream.Read(buffer, offset, count);

            // Console.WriteLine("GZipStream::Read(buffer, off({0}), c({1}) = {2}", offset, count, n);
            // Console.WriteLine( Util.FormatByteArray(buffer, offset, n) );

            if (!_firstReadDone)
            {
                _firstReadDone = true;
                FileName = _baseStream._GzipFileName;
                Comment = _baseStream._GzipComment;
            }
            return n;
        }



        /// <summary>
        ///   Calling this method always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="offset">irrelevant; it will always throw!</param>
        /// <param name="origin">irrelevant; it will always throw!</param>
        /// <returns>irrelevant!</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Calling this method always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="value">irrelevant; this method will always throw!</param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Write data to the stream.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   If you wish to use the <c>GZipStream</c> to compress data while writing,
        ///   you can create a <c>GZipStream</c> with <c>CompressionMode.Compress</c>, and a
        ///   writable output stream.  Then call <c>Write()</c> on that <c>GZipStream</c>,
        ///   providing uncompressed data as input.  The data sent to the output stream
        ///   will be the compressed form of the data written.
        /// </para>
        ///
        /// <para>
        ///   A <c>GZipStream</c> can be used for <c>Read()</c> or <c>Write()</c>, but not
        ///   both. Writing implies compression.  Reading implies decompression.
        /// </para>
        ///
        /// </remarks>
        /// <param name="buffer">The buffer holding data to write to the stream.</param>
        /// <param name="offset">the offset within that data array to find the first byte to write.</param>
        /// <param name="count">the number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException("GZipStream");
            if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Undefined)
            {
                //Console.WriteLine("GZipStream: First write");
                if (_baseStream._wantCompress)
                {
                    // first write in compression, therefore, emit the GZIP header
                    _headerByteCount = EmitHeader();
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            _baseStream.Write(buffer, offset, count);
        }
        #endregion


        internal static readonly System.DateTime _unixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static readonly System.Text.Encoding iso8859dash1 = new Iso88591Encoding();

        private int EmitHeader()
        {
            byte[] commentBytes = (Comment == null) ? null : iso8859dash1.GetBytes(Comment);
            byte[] filenameBytes = (FileName == null) ? null : iso8859dash1.GetBytes(FileName);

            int cbLength = (Comment == null) ? 0 : commentBytes.Length + 1;
            int fnLength = (FileName == null) ? 0 : filenameBytes.Length + 1;

            int bufferLength = 10 + cbLength + fnLength;
            byte[] header = new byte[bufferLength];
            int i = 0;
            // ID
            header[i++] = 0x1F;
            header[i++] = 0x8B;

            // compression method
            header[i++] = 8;
            byte flag = 0;
            if (Comment != null)
                flag ^= 0x10;
            if (FileName != null)
                flag ^= 0x8;

            // flag
            header[i++] = flag;

            // mtime
            if (!LastModified.HasValue) LastModified = DateTime.Now;
            System.TimeSpan delta = LastModified.Value - _unixEpoch;
            Int32 timet = (Int32)delta.TotalSeconds;
            Array.Copy(BitConverter.GetBytes(timet), 0, header, i, 4);
            i += 4;

            // xflg
            header[i++] = 0;    // this field is totally useless
            // OS
            header[i++] = 0xFF; // 0xFF == unspecified

            // extra field length - only if FEXTRA is set, which it is not.
            //header[i++]= 0;
            //header[i++]= 0;

            // filename
            if (fnLength != 0)
            {
                Array.Copy(filenameBytes, 0, header, i, fnLength - 1);
                i += fnLength - 1;
                header[i++] = 0; // terminate
            }

            // comment
            if (cbLength != 0)
            {
                Array.Copy(commentBytes, 0, header, i, cbLength - 1);
                i += cbLength - 1;
                header[i++] = 0; // terminate
            }

            _baseStream._stream.Write(header, 0, header.Length);

            return header.Length; // bytes written
        }



        /// <summary>
        ///   Compress a string into a byte array using GZip.
        /// </summary>
        ///
        /// <remarks>
        ///   Uncompress it with <see cref="GZipStream.UncompressString(byte[])"/>.
        /// </remarks>
        ///
        /// <seealso cref="GZipStream.UncompressString(byte[])"/>
        /// <seealso cref="GZipStream.CompressBuffer(byte[])"/>
        ///
        /// <param name="s">
        ///   A string to compress. The string will first be encoded
        ///   using UTF8, then compressed.
        /// </param>
        ///
        /// <returns>The string in compressed form</returns>
        internal static byte[] CompressString(String s)
        {
            using (var ms = new MemoryStream())
            {
                System.IO.Stream compressor =
                    new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZlibBaseStream.CompressString(s, compressor);
                return ms.ToArray();
            }
        }


        /// <summary>
        ///   Compress a byte array into a new byte array using GZip.
        /// </summary>
        ///
        /// <remarks>
        ///   Uncompress it with <see cref="GZipStream.UncompressBuffer(byte[])"/>.
        /// </remarks>
        ///
        /// <seealso cref="GZipStream.CompressString(string)"/>
        /// <seealso cref="GZipStream.UncompressBuffer(byte[])"/>
        ///
        /// <param name="b">
        ///   A buffer to compress.
        /// </param>
        ///
        /// <returns>The data in compressed form</returns>
        internal static byte[] CompressBuffer(byte[] b)
        {
            using (var ms = new MemoryStream())
            {
                System.IO.Stream compressor =
                    new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);

                ZlibBaseStream.CompressBuffer(b, compressor);
                return ms.ToArray();
            }
        }


        /// <summary>
        ///   Uncompress a GZip'ed byte array into a single string.
        /// </summary>
        ///
        /// <seealso cref="GZipStream.CompressString(String)"/>
        /// <seealso cref="GZipStream.UncompressBuffer(byte[])"/>
        ///
        /// <param name="compressed">
        ///   A buffer containing GZIP-compressed data.
        /// </param>
        ///
        /// <returns>The uncompressed string</returns>
        internal static String UncompressString(byte[] compressed)
        {
            using (var input = new MemoryStream(compressed))
            {
                Stream decompressor = new GZipStream(input, CompressionMode.Decompress);
                return ZlibBaseStream.UncompressString(compressed, decompressor);
            }
        }


        /// <summary>
        ///   Uncompress a GZip'ed byte array into a byte array.
        /// </summary>
        ///
        /// <seealso cref="GZipStream.CompressBuffer(byte[])"/>
        /// <seealso cref="GZipStream.UncompressString(byte[])"/>
        ///
        /// <param name="compressed">
        ///   A buffer containing data that has been compressed with GZip.
        /// </param>
        ///
        /// <returns>The data in uncompressed form</returns>
        internal static byte[] UncompressBuffer(byte[] compressed)
        {
            using (var input = new System.IO.MemoryStream(compressed))
            {
                System.IO.Stream decompressor =
                    new GZipStream(input, CompressionMode.Decompress);

                return ZlibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }


    }

    internal enum BlockState
    {
        NeedMore = 0,       // block not completed, need more input or more output
        BlockDone,          // block flush performed
        FinishStarted,              // finish started, need only more output at next deflate
        FinishDone          // finish done, accept no more input or output
    }

    internal enum DeflateFlavor
    {
        Store,
        Fast,
        Slow
    }

    internal sealed class DeflateManager
    {
        private static readonly int MEM_LEVEL_MAX = 9;
        private static readonly int MEM_LEVEL_DEFAULT = 8;

        internal delegate BlockState CompressFunc(FlushType flush);

        internal class Config
        {
            // Use a faster search when the previous match is longer than this
            internal int GoodLength; // reduce lazy search above this match length

            // Attempt to find a better match only when the current match is
            // strictly smaller than this value. This mechanism is used only for
            // compression levels >= 4.  For levels 1,2,3: MaxLazy is actually
            // MaxInsertLength. (See DeflateFast)

            internal int MaxLazy;    // do not perform lazy search above this match length

            internal int NiceLength; // quit search above this match length

            // To speed up deflation, hash chains are never searched beyond this
            // length.  A higher limit improves compression ratio but degrades the speed.

            internal int MaxChainLength;

            internal DeflateFlavor Flavor;

            private Config(int goodLength, int maxLazy, int niceLength, int maxChainLength, DeflateFlavor flavor)
            {
                this.GoodLength = goodLength;
                this.MaxLazy = maxLazy;
                this.NiceLength = niceLength;
                this.MaxChainLength = maxChainLength;
                this.Flavor = flavor;
            }

            internal static Config Lookup(CompressionLevel level)
            {
                return Table[(int)level];
            }


            static Config()
            {
                Table = new Config[] {
                    new Config(0, 0, 0, 0, DeflateFlavor.Store),
                    new Config(4, 4, 8, 4, DeflateFlavor.Fast),
                    new Config(4, 5, 16, 8, DeflateFlavor.Fast),
                    new Config(4, 6, 32, 32, DeflateFlavor.Fast),

                    new Config(4, 4, 16, 16, DeflateFlavor.Slow),
                    new Config(8, 16, 32, 32, DeflateFlavor.Slow),
                    new Config(8, 16, 128, 128, DeflateFlavor.Slow),
                    new Config(8, 32, 128, 256, DeflateFlavor.Slow),
                    new Config(32, 128, 258, 1024, DeflateFlavor.Slow),
                    new Config(32, 258, 258, 4096, DeflateFlavor.Slow),
                };
            }

            private static readonly Config[] Table;
        }


        private CompressFunc DeflateFunction;

        private static readonly System.String[] _ErrorMessage = new System.String[]
        {
            "need dictionary",
            "stream end",
            "",
            "file error",
            "stream error",
            "data error",
            "insufficient memory",
            "buffer error",
            "incompatible version",
            ""
        };

        // preset dictionary flag in zlib header
        private static readonly int PRESET_DICT = 0x20;

        private static readonly int INIT_STATE = 42;
        private static readonly int BUSY_STATE = 113;
        private static readonly int FINISH_STATE = 666;

        // The deflate compression method
        private static readonly int Z_DEFLATED = 8;

        private static readonly int STORED_BLOCK = 0;
        private static readonly int STATIC_TREES = 1;
        private static readonly int DYN_TREES = 2;

        // The three kinds of block type
        private static readonly int Z_BINARY = 0;
        private static readonly int Z_ASCII = 1;
        private static readonly int Z_UNKNOWN = 2;

        private static readonly int Buf_size = 8 * 2;

        private static readonly int MIN_MATCH = 3;
        private static readonly int MAX_MATCH = 258;

        private static readonly int MIN_LOOKAHEAD = (MAX_MATCH + MIN_MATCH + 1);

        private static readonly int HEAP_SIZE = (2 * InternalConstants.L_CODES + 1);

        private static readonly int END_BLOCK = 256;

        internal ZlibCodec _codec; // the zlib encoder/decoder
        internal int status;       // as the name implies
        internal byte[] pending;   // output still pending - waiting to be compressed
        internal int nextPending;  // index of next pending byte to output to the stream
        internal int pendingCount; // number of bytes in the pending buffer

        internal sbyte data_type;  // UNKNOWN, BINARY or ASCII
        internal int last_flush;   // value of flush param for previous deflate call

        internal int w_size;       // LZ77 window size (32K by default)
        internal int w_bits;       // log2(w_size)  (8..16)
        internal int w_mask;       // w_size - 1

        //internal byte[] dictionary;
        internal byte[] window;

        // Sliding window. Input bytes are read into the second half of the window,
        // and move to the first half later to keep a dictionary of at least wSize
        // bytes. With this organization, matches are limited to a distance of
        // wSize-MAX_MATCH bytes, but this ensures that IO is always
        // performed with a length multiple of the block size.
        //
        // To do: use the user input buffer as sliding window.

        internal int window_size;
        // Actual size of window: 2*wSize, except when the user input buffer
        // is directly used as sliding window.

        internal short[] prev;
        // Link to older string with same hash index. To limit the size of this
        // array to 64K, this link is maintained only for the last 32K strings.
        // An index in this array is thus a window index modulo 32K.

        internal short[] head;  // Heads of the hash chains or NIL.

        internal int ins_h;     // hash index of string to be inserted
        internal int hash_size; // number of elements in hash table
        internal int hash_bits; // log2(hash_size)
        internal int hash_mask; // hash_size-1

        // Number of bits by which ins_h must be shifted at each input
        // step. It must be such that after MIN_MATCH steps, the oldest
        // byte no longer takes part in the hash key, that is:
        // hash_shift * MIN_MATCH >= hash_bits
        internal int hash_shift;

        // Window position at the beginning of the current output block. Gets
        // negative when the window is moved backwards.

        internal int block_start;

        Config config;
        internal int match_length;    // length of best match
        internal int prev_match;      // previous match
        internal int match_available; // set if previous match exists
        internal int strstart;        // start of string to insert into.....????
        internal int match_start;     // start of matching string
        internal int lookahead;       // number of valid bytes ahead in window

        // Length of the best match at previous step. Matches not greater than this
        // are discarded. This is used in the lazy match evaluation.
        internal int prev_length;

        // Insert new strings in the hash table only if the match length is not
        // greater than this length. This saves time but degrades compression.
        // max_insert_length is used only for compression levels <= 3.

        internal CompressionLevel compressionLevel; // compression level (1..9)
        internal CompressionStrategy compressionStrategy; // favor or force Huffman coding


        internal short[] dyn_ltree;         // literal and length tree
        internal short[] dyn_dtree;         // distance tree
        internal short[] bl_tree;           // Huffman tree for bit lengths

        internal Tree treeLiterals = new Tree();  // desc for literal tree
        internal Tree treeDistances = new Tree();  // desc for distance tree
        internal Tree treeBitLengths = new Tree(); // desc for bit length tree

        // number of codes at each bit length for an optimal tree
        internal short[] bl_count = new short[InternalConstants.MAX_BITS + 1];

        // heap used to build the Huffman trees
        internal int[] heap = new int[2 * InternalConstants.L_CODES + 1];

        internal int heap_len;              // number of elements in the heap
        internal int heap_max;              // element of largest frequency

        // The sons of heap[n] are heap[2*n] and heap[2*n+1]. heap[0] is not used.
        // The same heap array is used to build all trees.

        // Depth of each subtree used as tie breaker for trees of equal frequency
        internal sbyte[] depth = new sbyte[2 * InternalConstants.L_CODES + 1];

        internal int _lengthOffset;                 // index for literals or lengths


        // Size of match buffer for literals/lengths.  There are 4 reasons for
        // limiting lit_bufsize to 64K:
        //   - frequencies can be kept in 16 bit counters
        //   - if compression is not successful for the first block, all input
        //     data is still in the window so we can still emit a stored block even
        //     when input comes from standard input.  (This can also be done for
        //     all blocks if lit_bufsize is not greater than 32K.)
        //   - if compression is not successful for a file smaller than 64K, we can
        //     even emit a stored file instead of a stored block (saving 5 bytes).
        //     This is applicable only for zip (not gzip or zlib).
        //   - creating new Huffman trees less frequently may not provide fast
        //     adaptation to changes in the input data statistics. (Take for
        //     example a binary file with poorly compressible code followed by
        //     a highly compressible string table.) Smaller buffer sizes give
        //     fast adaptation but have of course the overhead of transmitting
        //     trees more frequently.

        internal int lit_bufsize;

        internal int last_lit;     // running index in l_buf

        // Buffer for distances. To simplify the code, d_buf and l_buf have
        // the same number of elements. To use different lengths, an extra flag
        // array would be necessary.

        internal int _distanceOffset;        // index into pending; points to distance data??

        internal int opt_len;      // bit length of current block with optimal trees
        internal int static_len;   // bit length of current block with static trees
        internal int matches;      // number of string matches in current block
        internal int last_eob_len; // bit length of EOB code for last block

        // Output buffer. bits are inserted starting at the bottom (least
        // significant bits).
        internal short bi_buf;

        // Number of valid bits in bi_buf.  All bits above the last valid bit
        // are always zero.
        internal int bi_valid;


        internal DeflateManager()
        {
            dyn_ltree = new short[HEAP_SIZE * 2];
            dyn_dtree = new short[(2 * InternalConstants.D_CODES + 1) * 2]; // distance tree
            bl_tree = new short[(2 * InternalConstants.BL_CODES + 1) * 2]; // Huffman tree for bit lengths
        }


        // lm_init
        private void _InitializeLazyMatch()
        {
            window_size = 2 * w_size;

            // clear the hash - workitem 9063
            Array.Clear(head, 0, hash_size);
            //for (int i = 0; i < hash_size; i++) head[i] = 0;

            config = Config.Lookup(compressionLevel);
            SetDeflater();

            strstart = 0;
            block_start = 0;
            lookahead = 0;
            match_length = prev_length = MIN_MATCH - 1;
            match_available = 0;
            ins_h = 0;
        }

        // Initialize the tree data structures for a new zlib stream.
        private void _InitializeTreeData()
        {
            treeLiterals.dyn_tree = dyn_ltree;
            treeLiterals.staticTree = StaticTree.Literals;

            treeDistances.dyn_tree = dyn_dtree;
            treeDistances.staticTree = StaticTree.Distances;

            treeBitLengths.dyn_tree = bl_tree;
            treeBitLengths.staticTree = StaticTree.BitLengths;

            bi_buf = 0;
            bi_valid = 0;
            last_eob_len = 8; // enough lookahead for inflate

            // Initialize the first block of the first file:
            _InitializeBlocks();
        }

        internal void _InitializeBlocks()
        {
            // Initialize the trees.
            for (int i = 0; i < InternalConstants.L_CODES; i++)
                dyn_ltree[i * 2] = 0;
            for (int i = 0; i < InternalConstants.D_CODES; i++)
                dyn_dtree[i * 2] = 0;
            for (int i = 0; i < InternalConstants.BL_CODES; i++)
                bl_tree[i * 2] = 0;

            dyn_ltree[END_BLOCK * 2] = 1;
            opt_len = static_len = 0;
            last_lit = matches = 0;
        }

        // Restore the heap property by moving down the tree starting at node k,
        // exchanging a node with the smallest of its two sons if necessary, stopping
        // when the heap property is re-established (each father smaller than its
        // two sons).
        internal void pqdownheap(short[] tree, int k)
        {
            int v = heap[k];
            int j = k << 1; // left son of k
            while (j <= heap_len)
            {
                // Set j to the smallest of the two sons:
                if (j < heap_len && _IsSmaller(tree, heap[j + 1], heap[j], depth))
                {
                    j++;
                }
                // Exit if v is smaller than both sons
                if (_IsSmaller(tree, v, heap[j], depth))
                    break;

                // Exchange v with the smallest son
                heap[k] = heap[j]; k = j;
                // And continue down the tree, setting j to the left son of k
                j <<= 1;
            }
            heap[k] = v;
        }

        internal static bool _IsSmaller(short[] tree, int n, int m, sbyte[] depth)
        {
            short tn2 = tree[n * 2];
            short tm2 = tree[m * 2];
            return (tn2 < tm2 || (tn2 == tm2 && depth[n] <= depth[m]));
        }


        // Scan a literal or distance tree to determine the frequencies of the codes
        // in the bit length tree.
        internal void scan_tree(short[] tree, int max_code)
        {
            int n; // iterates over all tree elements
            int prevlen = -1; // last emitted length
            int curlen; // length of current code
            int nextlen = (int)tree[0 * 2 + 1]; // length of next code
            int count = 0; // repeat count of the current code
            int max_count = 7; // max repeat count
            int min_count = 4; // min repeat count

            if (nextlen == 0)
            {
                max_count = 138; min_count = 3;
            }
            tree[(max_code + 1) * 2 + 1] = (short)0x7fff; // guard //??

            for (n = 0; n <= max_code; n++)
            {
                curlen = nextlen; nextlen = (int)tree[(n + 1) * 2 + 1];
                if (++count < max_count && curlen == nextlen)
                {
                    continue;
                }
                else if (count < min_count)
                {
                    bl_tree[curlen * 2] = (short)(bl_tree[curlen * 2] + count);
                }
                else if (curlen != 0)
                {
                    if (curlen != prevlen)
                        bl_tree[curlen * 2]++;
                    bl_tree[InternalConstants.REP_3_6 * 2]++;
                }
                else if (count <= 10)
                {
                    bl_tree[InternalConstants.REPZ_3_10 * 2]++;
                }
                else
                {
                    bl_tree[InternalConstants.REPZ_11_138 * 2]++;
                }
                count = 0; prevlen = curlen;
                if (nextlen == 0)
                {
                    max_count = 138; min_count = 3;
                }
                else if (curlen == nextlen)
                {
                    max_count = 6; min_count = 3;
                }
                else
                {
                    max_count = 7; min_count = 4;
                }
            }
        }

        // Construct the Huffman tree for the bit lengths and return the index in
        // bl_order of the last bit length code to send.
        internal int build_bl_tree()
        {
            int max_blindex; // index of last bit length code of non zero freq

            // Determine the bit length frequencies for literal and distance trees
            scan_tree(dyn_ltree, treeLiterals.max_code);
            scan_tree(dyn_dtree, treeDistances.max_code);

            // Build the bit length tree:
            treeBitLengths.build_tree(this);
            // opt_len now includes the length of the tree representations, except
            // the lengths of the bit lengths codes and the 5+5+4 bits for the counts.

            // Determine the number of bit length codes to send. The pkzip format
            // requires that at least 4 bit length codes be sent. (appnote.txt says
            // 3 but the actual value used is 4.)
            for (max_blindex = InternalConstants.BL_CODES - 1; max_blindex >= 3; max_blindex--)
            {
                if (bl_tree[Tree.bl_order[max_blindex] * 2 + 1] != 0)
                    break;
            }
            // Update opt_len to include the bit length tree and counts
            opt_len += 3 * (max_blindex + 1) + 5 + 5 + 4;

            return max_blindex;
        }


        // Send the header for a block using dynamic Huffman trees: the counts, the
        // lengths of the bit length codes, the literal tree and the distance tree.
        // IN assertion: lcodes >= 257, dcodes >= 1, blcodes >= 4.
        internal void send_all_trees(int lcodes, int dcodes, int blcodes)
        {
            int rank; // index in bl_order

            send_bits(lcodes - 257, 5); // not +255 as stated in appnote.txt
            send_bits(dcodes - 1, 5);
            send_bits(blcodes - 4, 4); // not -3 as stated in appnote.txt
            for (rank = 0; rank < blcodes; rank++)
            {
                send_bits(bl_tree[Tree.bl_order[rank] * 2 + 1], 3);
            }
            send_tree(dyn_ltree, lcodes - 1); // literal tree
            send_tree(dyn_dtree, dcodes - 1); // distance tree
        }

        // Send a literal or distance tree in compressed form, using the codes in
        // bl_tree.
        internal void send_tree(short[] tree, int max_code)
        {
            int n;                           // iterates over all tree elements
            int prevlen = -1;              // last emitted length
            int curlen;                      // length of current code
            int nextlen = tree[0 * 2 + 1]; // length of next code
            int count = 0;               // repeat count of the current code
            int max_count = 7;               // max repeat count
            int min_count = 4;               // min repeat count

            if (nextlen == 0)
            {
                max_count = 138; min_count = 3;
            }

            for (n = 0; n <= max_code; n++)
            {
                curlen = nextlen; nextlen = tree[(n + 1) * 2 + 1];
                if (++count < max_count && curlen == nextlen)
                {
                    continue;
                }
                else if (count < min_count)
                {
                    do
                    {
                        send_code(curlen, bl_tree);
                    }
                    while (--count != 0);
                }
                else if (curlen != 0)
                {
                    if (curlen != prevlen)
                    {
                        send_code(curlen, bl_tree); count--;
                    }
                    send_code(InternalConstants.REP_3_6, bl_tree);
                    send_bits(count - 3, 2);
                }
                else if (count <= 10)
                {
                    send_code(InternalConstants.REPZ_3_10, bl_tree);
                    send_bits(count - 3, 3);
                }
                else
                {
                    send_code(InternalConstants.REPZ_11_138, bl_tree);
                    send_bits(count - 11, 7);
                }
                count = 0; prevlen = curlen;
                if (nextlen == 0)
                {
                    max_count = 138; min_count = 3;
                }
                else if (curlen == nextlen)
                {
                    max_count = 6; min_count = 3;
                }
                else
                {
                    max_count = 7; min_count = 4;
                }
            }
        }

        // Output a block of bytes on the stream.
        // IN assertion: there is enough room in pending_buf.
        private void put_bytes(byte[] p, int start, int len)
        {
            Array.Copy(p, start, pending, pendingCount, len);
            pendingCount += len;
        }

#if NOTNEEDED
        private void put_byte(byte c)
        {
            pending[pendingCount++] = c;
        }
        internal void put_short(int b)
        {
            unchecked
            {
                pending[pendingCount++] = (byte)b;
                pending[pendingCount++] = (byte)(b >> 8);
            }
        }
        internal void putShortMSB(int b)
        {
            unchecked
            {
                pending[pendingCount++] = (byte)(b >> 8);
                pending[pendingCount++] = (byte)b;
            }
        }
#endif

        internal void send_code(int c, short[] tree)
        {
            int c2 = c * 2;
            send_bits((tree[c2] & 0xffff), (tree[c2 + 1] & 0xffff));
        }

        internal void send_bits(int value, int length)
        {
            int len = length;
            unchecked
            {
                if (bi_valid > (int)Buf_size - len)
                {
                    //int val = value;
                    //      bi_buf |= (val << bi_valid);

                    bi_buf |= (short)((value << bi_valid) & 0xffff);
                    //put_short(bi_buf);
                    pending[pendingCount++] = (byte)bi_buf;
                    pending[pendingCount++] = (byte)(bi_buf >> 8);


                    bi_buf = (short)((uint)value >> (Buf_size - bi_valid));
                    bi_valid += len - Buf_size;
                }
                else
                {
                    //      bi_buf |= (value) << bi_valid;
                    bi_buf |= (short)((value << bi_valid) & 0xffff);
                    bi_valid += len;
                }
            }
        }

        // Send one empty static block to give enough lookahead for inflate.
        // This takes 10 bits, of which 7 may remain in the bit buffer.
        // The current inflate code requires 9 bits of lookahead. If the
        // last two codes for the previous block (real code plus EOB) were coded
        // on 5 bits or less, inflate may have only 5+3 bits of lookahead to decode
        // the last real code. In this case we send two empty static blocks instead
        // of one. (There are no problems if the previous block is stored or fixed.)
        // To simplify the code, we assume the worst case of last real code encoded
        // on one bit only.
        internal void _tr_align()
        {
            send_bits(STATIC_TREES << 1, 3);
            send_code(END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);

            bi_flush();

            // Of the 10 bits for the empty block, we have already sent
            // (10 - bi_valid) bits. The lookahead for the last real code (before
            // the EOB of the previous block) was thus at least one plus the length
            // of the EOB plus what we have just sent of the empty static block.
            if (1 + last_eob_len + 10 - bi_valid < 9)
            {
                send_bits(STATIC_TREES << 1, 3);
                send_code(END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
                bi_flush();
            }
            last_eob_len = 7;
        }


        // Save the match info and tally the frequency counts. Return true if
        // the current block must be flushed.
        internal bool _tr_tally(int dist, int lc)
        {
            pending[_distanceOffset + last_lit * 2] = unchecked((byte)((uint)dist >> 8));
            pending[_distanceOffset + last_lit * 2 + 1] = unchecked((byte)dist);
            pending[_lengthOffset + last_lit] = unchecked((byte)lc);
            last_lit++;

            if (dist == 0)
            {
                // lc is the unmatched char
                dyn_ltree[lc * 2]++;
            }
            else
            {
                matches++;
                // Here, lc is the match length - MIN_MATCH
                dist--; // dist = match distance - 1
                dyn_ltree[(Tree.LengthCode[lc] + InternalConstants.LITERALS + 1) * 2]++;
                dyn_dtree[Tree.DistanceCode(dist) * 2]++;
            }

            if ((last_lit & 0x1fff) == 0 && (int)compressionLevel > 2)
            {
                // Compute an upper bound for the compressed length
                int out_length = last_lit << 3;
                int in_length = strstart - block_start;
                int dcode;
                for (dcode = 0; dcode < InternalConstants.D_CODES; dcode++)
                {
                    out_length = (int)(out_length + (int)dyn_dtree[dcode * 2] * (5L + Tree.ExtraDistanceBits[dcode]));
                }
                out_length >>= 3;
                if ((matches < (last_lit / 2)) && out_length < in_length / 2)
                    return true;
            }

            return (last_lit == lit_bufsize - 1) || (last_lit == lit_bufsize);
            // dinoch - wraparound?
            // We avoid equality with lit_bufsize because of wraparound at 64K
            // on 16 bit machines and because stored blocks are restricted to
            // 64K-1 bytes.
        }



        // Send the block data compressed using the given Huffman trees
        internal void send_compressed_block(short[] ltree, short[] dtree)
        {
            int distance; // distance of matched string
            int lc;       // match length or unmatched char (if dist == 0)
            int lx = 0;   // running index in l_buf
            int code;     // the code to send
            int extra;    // number of extra bits to send

            if (last_lit != 0)
            {
                do
                {
                    int ix = _distanceOffset + lx * 2;
                    distance = ((pending[ix] << 8) & 0xff00) |
                        (pending[ix + 1] & 0xff);
                    lc = (pending[_lengthOffset + lx]) & 0xff;
                    lx++;

                    if (distance == 0)
                    {
                        send_code(lc, ltree); // send a literal byte
                    }
                    else
                    {
                        // literal or match pair
                        // Here, lc is the match length - MIN_MATCH
                        code = Tree.LengthCode[lc];

                        // send the length code
                        send_code(code + InternalConstants.LITERALS + 1, ltree);
                        extra = Tree.ExtraLengthBits[code];
                        if (extra != 0)
                        {
                            // send the extra length bits
                            lc -= Tree.LengthBase[code];
                            send_bits(lc, extra);
                        }
                        distance--; // dist is now the match distance - 1
                        code = Tree.DistanceCode(distance);

                        // send the distance code
                        send_code(code, dtree);

                        extra = Tree.ExtraDistanceBits[code];
                        if (extra != 0)
                        {
                            // send the extra distance bits
                            distance -= Tree.DistanceBase[code];
                            send_bits(distance, extra);
                        }
                    }

                    // Check that the overlay between pending and d_buf+l_buf is ok:
                }
                while (lx < last_lit);
            }

            send_code(END_BLOCK, ltree);
            last_eob_len = ltree[END_BLOCK * 2 + 1];
        }



        // Set the data type to ASCII or BINARY, using a crude approximation:
        // binary if more than 20% of the bytes are <= 6 or >= 128, ascii otherwise.
        // IN assertion: the fields freq of dyn_ltree are set and the total of all
        // frequencies does not exceed 64K (to fit in an int on 16 bit machines).
        internal void set_data_type()
        {
            int n = 0;
            int ascii_freq = 0;
            int bin_freq = 0;
            while (n < 7)
            {
                bin_freq += dyn_ltree[n * 2]; n++;
            }
            while (n < 128)
            {
                ascii_freq += dyn_ltree[n * 2]; n++;
            }
            while (n < InternalConstants.LITERALS)
            {
                bin_freq += dyn_ltree[n * 2]; n++;
            }
            data_type = (sbyte)(bin_freq > (ascii_freq >> 2) ? Z_BINARY : Z_ASCII);
        }



        // Flush the bit buffer, keeping at most 7 bits in it.
        internal void bi_flush()
        {
            if (bi_valid == 16)
            {
                pending[pendingCount++] = (byte)bi_buf;
                pending[pendingCount++] = (byte)(bi_buf >> 8);
                bi_buf = 0;
                bi_valid = 0;
            }
            else if (bi_valid >= 8)
            {
                //put_byte((byte)bi_buf);
                pending[pendingCount++] = (byte)bi_buf;
                bi_buf >>= 8;
                bi_valid -= 8;
            }
        }

        // Flush the bit buffer and align the output on a byte boundary
        internal void bi_windup()
        {
            if (bi_valid > 8)
            {
                pending[pendingCount++] = (byte)bi_buf;
                pending[pendingCount++] = (byte)(bi_buf >> 8);
            }
            else if (bi_valid > 0)
            {
                //put_byte((byte)bi_buf);
                pending[pendingCount++] = (byte)bi_buf;
            }
            bi_buf = 0;
            bi_valid = 0;
        }

        // Copy a stored block, storing first the length and its
        // one's complement if requested.
        internal void copy_block(int buf, int len, bool header)
        {
            bi_windup(); // align on byte boundary
            last_eob_len = 8; // enough lookahead for inflate

            if (header)
                unchecked
                {
                    //put_short((short)len);
                    pending[pendingCount++] = (byte)len;
                    pending[pendingCount++] = (byte)(len >> 8);
                    //put_short((short)~len);
                    pending[pendingCount++] = (byte)~len;
                    pending[pendingCount++] = (byte)(~len >> 8);
                }

            put_bytes(window, buf, len);
        }

        internal void flush_block_only(bool eof)
        {
            _tr_flush_block(block_start >= 0 ? block_start : -1, strstart - block_start, eof);
            block_start = strstart;
            _codec.flush_pending();
        }

        // Copy without compression as much as possible from the input stream, return
        // the current block state.
        // This function does not insert new strings in the dictionary since
        // uncompressible data is probably not useful. This function is used
        // only for the level=0 compression option.
        // NOTE: this function should be optimized to avoid extra copying from
        // window to pending_buf.
        internal BlockState DeflateNone(FlushType flush)
        {
            // Stored blocks are limited to 0xffff bytes, pending is limited
            // to pending_buf_size, and each stored block has a 5 byte header:

            int max_block_size = 0xffff;
            int max_start;

            if (max_block_size > pending.Length - 5)
            {
                max_block_size = pending.Length - 5;
            }

            // Copy as much as possible from input to output:
            while (true)
            {
                // Fill the window as much as possible:
                if (lookahead <= 1)
                {
                    _fillWindow();
                    if (lookahead == 0 && flush == FlushType.None)
                        return BlockState.NeedMore;
                    if (lookahead == 0)
                        break; // flush the current block
                }

                strstart += lookahead;
                lookahead = 0;

                // Emit a stored block if pending will be full:
                max_start = block_start + max_block_size;
                if (strstart == 0 || strstart >= max_start)
                {
                    // strstart == 0 is possible when wraparound on 16-bit machine
                    lookahead = (int)(strstart - max_start);
                    strstart = (int)max_start;

                    flush_block_only(false);
                    if (_codec.AvailableBytesOut == 0)
                        return BlockState.NeedMore;
                }

                // Flush if we may have to slide, otherwise block_start may become
                // negative and the data will be gone:
                if (strstart - block_start >= w_size - MIN_LOOKAHEAD)
                {
                    flush_block_only(false);
                    if (_codec.AvailableBytesOut == 0)
                        return BlockState.NeedMore;
                }
            }

            flush_block_only(flush == FlushType.Finish);
            if (_codec.AvailableBytesOut == 0)
                return (flush == FlushType.Finish) ? BlockState.FinishStarted : BlockState.NeedMore;

            return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
        }


        // Send a stored block
        internal void _tr_stored_block(int buf, int stored_len, bool eof)
        {
            send_bits((STORED_BLOCK << 1) + (eof ? 1 : 0), 3); // send block type
            copy_block(buf, stored_len, true); // with header
        }

        // Determine the best encoding for the current block: dynamic trees, static
        // trees or store, and output the encoded block to the zip file.
        internal void _tr_flush_block(int buf, int stored_len, bool eof)
        {
            int opt_lenb, static_lenb; // opt_len and static_len in bytes
            int max_blindex = 0; // index of last bit length code of non zero freq

            // Build the Huffman trees unless a stored block is forced
            if (compressionLevel > 0)
            {
                // Check if the file is ascii or binary
                if (data_type == Z_UNKNOWN)
                    set_data_type();

                // Construct the literal and distance trees
                treeLiterals.build_tree(this);

                treeDistances.build_tree(this);

                // At this point, opt_len and static_len are the total bit lengths of
                // the compressed block data, excluding the tree representations.

                // Build the bit length tree for the above two trees, and get the index
                // in bl_order of the last bit length code to send.
                max_blindex = build_bl_tree();

                // Determine the best encoding. Compute first the block length in bytes
                opt_lenb = (opt_len + 3 + 7) >> 3;
                static_lenb = (static_len + 3 + 7) >> 3;

                if (static_lenb <= opt_lenb)
                    opt_lenb = static_lenb;
            }
            else
            {
                opt_lenb = static_lenb = stored_len + 5; // force a stored block
            }

            if (stored_len + 4 <= opt_lenb && buf != -1)
            {
                // 4: two words for the lengths
                // The test buf != NULL is only necessary if LIT_BUFSIZE > WSIZE.
                // Otherwise we can't have processed more than WSIZE input bytes since
                // the last block flush, because compression would have been
                // successful. If LIT_BUFSIZE <= WSIZE, it is never too late to
                // transform a block into a stored block.
                _tr_stored_block(buf, stored_len, eof);
            }
            else if (static_lenb == opt_lenb)
            {
                send_bits((STATIC_TREES << 1) + (eof ? 1 : 0), 3);
                send_compressed_block(StaticTree.lengthAndLiteralsTreeCodes, StaticTree.distTreeCodes);
            }
            else
            {
                send_bits((DYN_TREES << 1) + (eof ? 1 : 0), 3);
                send_all_trees(treeLiterals.max_code + 1, treeDistances.max_code + 1, max_blindex + 1);
                send_compressed_block(dyn_ltree, dyn_dtree);
            }

            // The above check is made mod 2^32, for files larger than 512 MB
            // and uLong implemented on 32 bits.

            _InitializeBlocks();

            if (eof)
            {
                bi_windup();
            }
        }

        // Fill the window when the lookahead becomes insufficient.
        // Updates strstart and lookahead.
        //
        // IN assertion: lookahead < MIN_LOOKAHEAD
        // OUT assertions: strstart <= window_size-MIN_LOOKAHEAD
        //    At least one byte has been read, or avail_in == 0; reads are
        //    performed for at least two bytes (required for the zip translate_eol
        //    option -- not supported here).
        private void _fillWindow()
        {
            int n, m;
            int p;
            int more; // Amount of free space at the end of the window.

            do
            {
                more = (window_size - lookahead - strstart);

                // Deal with !@#$% 64K limit:
                if (more == 0 && strstart == 0 && lookahead == 0)
                {
                    more = w_size;
                }
                else if (more == -1)
                {
                    // Very unlikely, but possible on 16 bit machine if strstart == 0
                    // and lookahead == 1 (input done one byte at time)
                    more--;

                    // If the window is almost full and there is insufficient lookahead,
                    // move the upper half to the lower one to make room in the upper half.
                }
                else if (strstart >= w_size + w_size - MIN_LOOKAHEAD)
                {
                    Array.Copy(window, w_size, window, 0, w_size);
                    match_start -= w_size;
                    strstart -= w_size; // we now have strstart >= MAX_DIST
                    block_start -= w_size;

                    // Slide the hash table (could be avoided with 32 bit values
                    // at the expense of memory usage). We slide even when level == 0
                    // to keep the hash table consistent if we switch back to level > 0
                    // later. (Using level 0 permanently is not an optimal usage of
                    // zlib, so we don't care about this pathological case.)

                    n = hash_size;
                    p = n;
                    do
                    {
                        m = (head[--p] & 0xffff);
                        head[p] = (short)((m >= w_size) ? (m - w_size) : 0);
                    }
                    while (--n != 0);

                    n = w_size;
                    p = n;
                    do
                    {
                        m = (prev[--p] & 0xffff);
                        prev[p] = (short)((m >= w_size) ? (m - w_size) : 0);
                        // If n is not on any hash chain, prev[n] is garbage but
                        // its value will never be used.
                    }
                    while (--n != 0);
                    more += w_size;
                }

                if (_codec.AvailableBytesIn == 0)
                    return;

                // If there was no sliding:
                //    strstart <= WSIZE+MAX_DIST-1 && lookahead <= MIN_LOOKAHEAD - 1 &&
                //    more == window_size - lookahead - strstart
                // => more >= window_size - (MIN_LOOKAHEAD-1 + WSIZE + MAX_DIST-1)
                // => more >= window_size - 2*WSIZE + 2
                // In the BIG_MEM or MMAP case (not yet supported),
                //   window_size == input_size + MIN_LOOKAHEAD  &&
                //   strstart + s->lookahead <= input_size => more >= MIN_LOOKAHEAD.
                // Otherwise, window_size == 2*WSIZE so more >= 2.
                // If there was sliding, more >= WSIZE. So in all cases, more >= 2.

                n = _codec.read_buf(window, strstart + lookahead, more);
                lookahead += n;

                // Initialize the hash value now that we have some input:
                if (lookahead >= MIN_MATCH)
                {
                    ins_h = window[strstart] & 0xff;
                    ins_h = (((ins_h) << hash_shift) ^ (window[strstart + 1] & 0xff)) & hash_mask;
                }
                // If the whole input has less than MIN_MATCH bytes, ins_h is garbage,
                // but this is not important since only literal bytes will be emitted.
            }
            while (lookahead < MIN_LOOKAHEAD && _codec.AvailableBytesIn != 0);
        }

        // Compress as much as possible from the input stream, return the current
        // block state.
        // This function does not perform lazy evaluation of matches and inserts
        // new strings in the dictionary only for unmatched strings or for short
        // matches. It is used only for the fast compression options.
        internal BlockState DeflateFast(FlushType flush)
        {
            //    short hash_head = 0; // head of the hash chain
            int hash_head = 0; // head of the hash chain
            bool bflush; // set if current block must be flushed

            while (true)
            {
                // Make sure that we always have enough lookahead, except
                // at the end of the input file. We need MAX_MATCH bytes
                // for the next match, plus MIN_MATCH bytes to insert the
                // string following the next match.
                if (lookahead < MIN_LOOKAHEAD)
                {
                    _fillWindow();
                    if (lookahead < MIN_LOOKAHEAD && flush == FlushType.None)
                    {
                        return BlockState.NeedMore;
                    }
                    if (lookahead == 0)
                        break; // flush the current block
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:
                if (lookahead >= MIN_MATCH)
                {
                    ins_h = (((ins_h) << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;

                    //  prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = (head[ins_h] & 0xffff);
                    prev[strstart & w_mask] = head[ins_h];
                    head[ins_h] = unchecked((short)strstart);
                }

                // Find the longest match, discarding those <= prev_length.
                // At this point we have always match_length < MIN_MATCH

                if (hash_head != 0L && ((strstart - hash_head) & 0xffff) <= w_size - MIN_LOOKAHEAD)
                {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).
                    if (compressionStrategy != CompressionStrategy.HuffmanOnly)
                    {
                        match_length = longest_match(hash_head);
                    }
                    // longest_match() sets match_start
                }
                if (match_length >= MIN_MATCH)
                {
                    //        check_match(strstart, match_start, match_length);

                    bflush = _tr_tally(strstart - match_start, match_length - MIN_MATCH);

                    lookahead -= match_length;

                    // Insert new strings in the hash table only if the match length
                    // is not too large. This saves time but degrades compression.
                    if (match_length <= config.MaxLazy && lookahead >= MIN_MATCH)
                    {
                        match_length--; // string at strstart already in hash table
                        do
                        {
                            strstart++;

                            ins_h = ((ins_h << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;
                            //      prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = (head[ins_h] & 0xffff);
                            prev[strstart & w_mask] = head[ins_h];
                            head[ins_h] = unchecked((short)strstart);

                            // strstart never exceeds WSIZE-MAX_MATCH, so there are
                            // always MIN_MATCH bytes ahead.
                        }
                        while (--match_length != 0);
                        strstart++;
                    }
                    else
                    {
                        strstart += match_length;
                        match_length = 0;
                        ins_h = window[strstart] & 0xff;

                        ins_h = (((ins_h) << hash_shift) ^ (window[strstart + 1] & 0xff)) & hash_mask;
                        // If lookahead < MIN_MATCH, ins_h is garbage, but it does not
                        // matter since it will be recomputed at next deflate call.
                    }
                }
                else
                {
                    // No match, output a literal byte

                    bflush = _tr_tally(0, window[strstart] & 0xff);
                    lookahead--;
                    strstart++;
                }
                if (bflush)
                {
                    flush_block_only(false);
                    if (_codec.AvailableBytesOut == 0)
                        return BlockState.NeedMore;
                }
            }

            flush_block_only(flush == FlushType.Finish);
            if (_codec.AvailableBytesOut == 0)
            {
                if (flush == FlushType.Finish)
                    return BlockState.FinishStarted;
                else
                    return BlockState.NeedMore;
            }
            return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
        }

        // Same as above, but achieves better compression. We use a lazy
        // evaluation for matches: a match is finally adopted only if there is
        // no better match at the next window position.
        internal BlockState DeflateSlow(FlushType flush)
        {
            //    short hash_head = 0;    // head of hash chain
            int hash_head = 0; // head of hash chain
            bool bflush; // set if current block must be flushed

            // Process the input block.
            while (true)
            {
                // Make sure that we always have enough lookahead, except
                // at the end of the input file. We need MAX_MATCH bytes
                // for the next match, plus MIN_MATCH bytes to insert the
                // string following the next match.

                if (lookahead < MIN_LOOKAHEAD)
                {
                    _fillWindow();
                    if (lookahead < MIN_LOOKAHEAD && flush == FlushType.None)
                        return BlockState.NeedMore;

                    if (lookahead == 0)
                        break; // flush the current block
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:

                if (lookahead >= MIN_MATCH)
                {
                    ins_h = (((ins_h) << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;
                    //  prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = (head[ins_h] & 0xffff);
                    prev[strstart & w_mask] = head[ins_h];
                    head[ins_h] = unchecked((short)strstart);
                }

                // Find the longest match, discarding those <= prev_length.
                prev_length = match_length;
                prev_match = match_start;
                match_length = MIN_MATCH - 1;

                if (hash_head != 0 && prev_length < config.MaxLazy &&
                    ((strstart - hash_head) & 0xffff) <= w_size - MIN_LOOKAHEAD)
                {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).

                    if (compressionStrategy != CompressionStrategy.HuffmanOnly)
                    {
                        match_length = longest_match(hash_head);
                    }
                    // longest_match() sets match_start

                    if (match_length <= 5 && (compressionStrategy == CompressionStrategy.Filtered ||
                                              (match_length == MIN_MATCH && strstart - match_start > 4096)))
                    {

                        // If prev_match is also MIN_MATCH, match_start is garbage
                        // but we will ignore the current match anyway.
                        match_length = MIN_MATCH - 1;
                    }
                }

                // If there was a match at the previous step and the current
                // match is not better, output the previous match:
                if (prev_length >= MIN_MATCH && match_length <= prev_length)
                {
                    int max_insert = strstart + lookahead - MIN_MATCH;
                    // Do not insert strings in hash table beyond this.

                    //          check_match(strstart-1, prev_match, prev_length);

                    bflush = _tr_tally(strstart - 1 - prev_match, prev_length - MIN_MATCH);

                    // Insert in hash table all strings up to the end of the match.
                    // strstart-1 and strstart are already inserted. If there is not
                    // enough lookahead, the last two strings are not inserted in
                    // the hash table.
                    lookahead -= (prev_length - 1);
                    prev_length -= 2;
                    do
                    {
                        if (++strstart <= max_insert)
                        {
                            ins_h = (((ins_h) << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;
                            //prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = (head[ins_h] & 0xffff);
                            prev[strstart & w_mask] = head[ins_h];
                            head[ins_h] = unchecked((short)strstart);
                        }
                    }
                    while (--prev_length != 0);
                    match_available = 0;
                    match_length = MIN_MATCH - 1;
                    strstart++;

                    if (bflush)
                    {
                        flush_block_only(false);
                        if (_codec.AvailableBytesOut == 0)
                            return BlockState.NeedMore;
                    }
                }
                else if (match_available != 0)
                {

                    // If there was no match at the previous position, output a
                    // single literal. If there was a match but the current match
                    // is longer, truncate the previous match to a single literal.

                    bflush = _tr_tally(0, window[strstart - 1] & 0xff);

                    if (bflush)
                    {
                        flush_block_only(false);
                    }
                    strstart++;
                    lookahead--;
                    if (_codec.AvailableBytesOut == 0)
                        return BlockState.NeedMore;
                }
                else
                {
                    // There is no previous match to compare with, wait for
                    // the next step to decide.

                    match_available = 1;
                    strstart++;
                    lookahead--;
                }
            }

            if (match_available != 0)
            {
                bflush = _tr_tally(0, window[strstart - 1] & 0xff);
                match_available = 0;
            }
            flush_block_only(flush == FlushType.Finish);

            if (_codec.AvailableBytesOut == 0)
            {
                if (flush == FlushType.Finish)
                    return BlockState.FinishStarted;
                else
                    return BlockState.NeedMore;
            }

            return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
        }


        internal int longest_match(int cur_match)
        {
            int chain_length = config.MaxChainLength; // max hash chain length
            int scan = strstart;              // current string
            int match;                                // matched string
            int len;                                  // length of current match
            int best_len = prev_length;           // best match length so far
            int limit = strstart > (w_size - MIN_LOOKAHEAD) ? strstart - (w_size - MIN_LOOKAHEAD) : 0;

            int niceLength = config.NiceLength;

            // Stop when cur_match becomes <= limit. To simplify the code,
            // we prevent matches with the string of window index 0.

            int wmask = w_mask;

            int strend = strstart + MAX_MATCH;
            byte scan_end1 = window[scan + best_len - 1];
            byte scan_end = window[scan + best_len];

            // The code is optimized for HASH_BITS >= 8 and MAX_MATCH-2 multiple of 16.
            // It is easy to get rid of this optimization if necessary.

            // Do not waste too much time if we already have a good match:
            if (prev_length >= config.GoodLength)
            {
                chain_length >>= 2;
            }

            // Do not look for matches beyond the end of the input. This is necessary
            // to make deflate deterministic.
            if (niceLength > lookahead)
                niceLength = lookahead;

            do
            {
                match = cur_match;

                // Skip to next match if the match length cannot increase
                // or if the match length is less than 2:
                if (window[match + best_len] != scan_end ||
                    window[match + best_len - 1] != scan_end1 ||
                    window[match] != window[scan] ||
                    window[++match] != window[scan + 1])
                    continue;

                // The check at best_len-1 can be removed because it will be made
                // again later. (This heuristic is not always a win.)
                // It is not necessary to compare scan[2] and match[2] since they
                // are always equal when the other bytes match, given that
                // the hash keys are equal and that HASH_BITS >= 8.
                scan += 2; match++;

                // We check for insufficient lookahead only every 8th comparison;
                // the 256th check will be made at strstart+258.
                do
                {
                }
                while (window[++scan] == window[++match] &&
                       window[++scan] == window[++match] &&
                       window[++scan] == window[++match] &&
                       window[++scan] == window[++match] &&
                       window[++scan] == window[++match] &&
                       window[++scan] == window[++match] &&
                       window[++scan] == window[++match] &&
                       window[++scan] == window[++match] && scan < strend);

                len = MAX_MATCH - (int)(strend - scan);
                scan = strend - MAX_MATCH;

                if (len > best_len)
                {
                    match_start = cur_match;
                    best_len = len;
                    if (len >= niceLength)
                        break;
                    scan_end1 = window[scan + best_len - 1];
                    scan_end = window[scan + best_len];
                }
            }
            while ((cur_match = (prev[cur_match & wmask] & 0xffff)) > limit && --chain_length != 0);

            if (best_len <= lookahead)
                return best_len;
            return lookahead;
        }


        private bool Rfc1950BytesEmitted = false;
        private bool _WantRfc1950HeaderBytes = true;
        internal bool WantRfc1950HeaderBytes
        {
            get { return _WantRfc1950HeaderBytes; }
            set { _WantRfc1950HeaderBytes = value; }
        }


        internal int Initialize(ZlibCodec codec, CompressionLevel level)
        {
            return Initialize(codec, level, ZlibConstants.WindowBitsMax);
        }

        internal int Initialize(ZlibCodec codec, CompressionLevel level, int bits)
        {
            return Initialize(codec, level, bits, MEM_LEVEL_DEFAULT, CompressionStrategy.Default);
        }

        internal int Initialize(ZlibCodec codec, CompressionLevel level, int bits, CompressionStrategy compressionStrategy)
        {
            return Initialize(codec, level, bits, MEM_LEVEL_DEFAULT, compressionStrategy);
        }

        internal int Initialize(ZlibCodec codec, CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy)
        {
            _codec = codec;
            _codec.Message = null;

            // validation
            if (windowBits < 9 || windowBits > 15)
                throw new ZlibException("windowBits must be in the range 9..15.");

            if (memLevel < 1 || memLevel > MEM_LEVEL_MAX)
                throw new ZlibException(String.Format("memLevel must be in the range 1.. {0}", MEM_LEVEL_MAX));

            _codec.dstate = this;

            w_bits = windowBits;
            w_size = 1 << w_bits;
            w_mask = w_size - 1;

            hash_bits = memLevel + 7;
            hash_size = 1 << hash_bits;
            hash_mask = hash_size - 1;
            hash_shift = ((hash_bits + MIN_MATCH - 1) / MIN_MATCH);

            window = new byte[w_size * 2];
            prev = new short[w_size];
            head = new short[hash_size];

            // for memLevel==8, this will be 16384, 16k
            lit_bufsize = 1 << (memLevel + 6);

            // Use a single array as the buffer for data pending compression,
            // the output distance codes, and the output length codes (aka tree).
            // orig comment: This works just fine since the average
            // output size for (length,distance) codes is <= 24 bits.
            pending = new byte[lit_bufsize * 4];
            _distanceOffset = lit_bufsize;
            _lengthOffset = (1 + 2) * lit_bufsize;

            // So, for memLevel 8, the length of the pending buffer is 65536. 64k.
            // The first 16k are pending bytes.
            // The middle slice, of 32k, is used for distance codes.
            // The final 16k are length codes.

            this.compressionLevel = level;
            this.compressionStrategy = strategy;

            Reset();
            return ZlibConstants.Z_OK;
        }


        internal void Reset()
        {
            _codec.TotalBytesIn = _codec.TotalBytesOut = 0;
            _codec.Message = null;
            //strm.data_type = Z_UNKNOWN;

            pendingCount = 0;
            nextPending = 0;

            Rfc1950BytesEmitted = false;

            status = (WantRfc1950HeaderBytes) ? INIT_STATE : BUSY_STATE;
            _codec._Adler32 = Adler.Adler32(0, null, 0, 0);

            last_flush = (int)FlushType.None;

            _InitializeTreeData();
            _InitializeLazyMatch();
        }


        internal int End()
        {
            if (status != INIT_STATE && status != BUSY_STATE && status != FINISH_STATE)
            {
                return ZlibConstants.Z_STREAM_ERROR;
            }
            // Deallocate in reverse order of allocations:
            pending = null;
            head = null;
            prev = null;
            window = null;
            // free
            // dstate=null;
            return status == BUSY_STATE ? ZlibConstants.Z_DATA_ERROR : ZlibConstants.Z_OK;
        }


        private void SetDeflater()
        {
            switch (config.Flavor)
            {
                case DeflateFlavor.Store:
                    DeflateFunction = DeflateNone;
                    break;
                case DeflateFlavor.Fast:
                    DeflateFunction = DeflateFast;
                    break;
                case DeflateFlavor.Slow:
                    DeflateFunction = DeflateSlow;
                    break;
            }
        }


        internal int SetParams(CompressionLevel level, CompressionStrategy strategy)
        {
            int result = ZlibConstants.Z_OK;

            if (compressionLevel != level)
            {
                Config newConfig = Config.Lookup(level);

                // change in the deflate flavor (Fast vs slow vs none)?
                if (newConfig.Flavor != config.Flavor && _codec.TotalBytesIn != 0)
                {
                    // Flush the last buffer:
                    result = _codec.Deflate(FlushType.Partial);
                }

                compressionLevel = level;
                config = newConfig;
                SetDeflater();
            }

            // no need to flush with change in strategy?  Really?
            compressionStrategy = strategy;

            return result;
        }


        internal int SetDictionary(byte[] dictionary)
        {
            if (dictionary == null || status != INIT_STATE)
                throw new ZlibException("Stream error.");

            int length = dictionary.Length;
            int index = 0;

            _codec._Adler32 = Adler.Adler32(_codec._Adler32, dictionary, 0, dictionary.Length);

            if (length < MIN_MATCH)
                return ZlibConstants.Z_OK;
            if (length > w_size - MIN_LOOKAHEAD)
            {
                length = w_size - MIN_LOOKAHEAD;
                index = dictionary.Length - length; // use the tail of the dictionary
            }
            Array.Copy(dictionary, index, window, 0, length);
            strstart = length;
            block_start = length;

            // Insert all strings in the hash table (except for the last two bytes).
            // s->lookahead stays null, so s->ins_h will be recomputed at the next
            // call of fill_window.

            ins_h = window[0] & 0xff;
            ins_h = (((ins_h) << hash_shift) ^ (window[1] & 0xff)) & hash_mask;

            for (int n = 0; n <= length - MIN_MATCH; n++)
            {
                ins_h = (((ins_h) << hash_shift) ^ (window[(n) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;
                prev[n & w_mask] = head[ins_h];
                head[ins_h] = (short)n;
            }
            return ZlibConstants.Z_OK;
        }



        internal int Deflate(FlushType flush)
        {
            int old_flush;

            if (_codec.OutputBuffer == null ||
                (_codec.InputBuffer == null && _codec.AvailableBytesIn != 0) ||
                (status == FINISH_STATE && flush != FlushType.Finish))
            {
                _codec.Message = _ErrorMessage[ZlibConstants.Z_NEED_DICT - (ZlibConstants.Z_STREAM_ERROR)];
                throw new ZlibException(String.Format("Something is fishy. [{0}]", _codec.Message));
            }
            if (_codec.AvailableBytesOut == 0)
            {
                _codec.Message = _ErrorMessage[ZlibConstants.Z_NEED_DICT - (ZlibConstants.Z_BUF_ERROR)];
                throw new ZlibException("OutputBuffer is full (AvailableBytesOut == 0)");
            }

            old_flush = last_flush;
            last_flush = (int)flush;

            // Write the zlib (rfc1950) header bytes
            if (status == INIT_STATE)
            {
                int header = (Z_DEFLATED + ((w_bits - 8) << 4)) << 8;
                int level_flags = (((int)compressionLevel - 1) & 0xff) >> 1;

                if (level_flags > 3)
                    level_flags = 3;
                header |= (level_flags << 6);
                if (strstart != 0)
                    header |= PRESET_DICT;
                header += 31 - (header % 31);

                status = BUSY_STATE;
                //putShortMSB(header);
                unchecked
                {
                    pending[pendingCount++] = (byte)(header >> 8);
                    pending[pendingCount++] = (byte)header;
                }
                // Save the adler32 of the preset dictionary:
                if (strstart != 0)
                {
                    pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF000000) >> 24);
                    pending[pendingCount++] = (byte)((_codec._Adler32 & 0x00FF0000) >> 16);
                    pending[pendingCount++] = (byte)((_codec._Adler32 & 0x0000FF00) >> 8);
                    pending[pendingCount++] = (byte)(_codec._Adler32 & 0x000000FF);
                }
                _codec._Adler32 = Adler.Adler32(0, null, 0, 0);
            }

            // Flush as much pending output as possible
            if (pendingCount != 0)
            {
                _codec.flush_pending();
                if (_codec.AvailableBytesOut == 0)
                {
                    //System.out.println("  avail_out==0");
                    // Since avail_out is 0, deflate will be called again with
                    // more output space, but possibly with both pending and
                    // avail_in equal to zero. There won't be anything to do,
                    // but this is not an error situation so make sure we
                    // return OK instead of BUF_ERROR at next call of deflate:
                    last_flush = -1;
                    return ZlibConstants.Z_OK;
                }

                // Make sure there is something to do and avoid duplicate consecutive
                // flushes. For repeated and useless calls with Z_FINISH, we keep
                // returning Z_STREAM_END instead of Z_BUFF_ERROR.
            }
            else if (_codec.AvailableBytesIn == 0 &&
                     (int)flush <= old_flush &&
                     flush != FlushType.Finish)
            {
                // workitem 8557
                //
                // Not sure why this needs to be an error.  pendingCount == 0, which
                // means there's nothing to deflate.  And the caller has not asked
                // for a FlushType.Finish, but...  that seems very non-fatal.  We
                // can just say "OK" and do nothing.

                // _codec.Message = z_errmsg[ZlibConstants.Z_NEED_DICT - (ZlibConstants.Z_BUF_ERROR)];
                // throw new ZlibException("AvailableBytesIn == 0 && flush<=old_flush && flush != FlushType.Finish");

                return ZlibConstants.Z_OK;
            }

            // User must not provide more input after the first FINISH:
            if (status == FINISH_STATE && _codec.AvailableBytesIn != 0)
            {
                _codec.Message = _ErrorMessage[ZlibConstants.Z_NEED_DICT - (ZlibConstants.Z_BUF_ERROR)];
                throw new ZlibException("status == FINISH_STATE && _codec.AvailableBytesIn != 0");
            }

            // Start a new block or continue the current one.
            if (_codec.AvailableBytesIn != 0 || lookahead != 0 || (flush != FlushType.None && status != FINISH_STATE))
            {
                BlockState bstate = DeflateFunction(flush);

                if (bstate == BlockState.FinishStarted || bstate == BlockState.FinishDone)
                {
                    status = FINISH_STATE;
                }
                if (bstate == BlockState.NeedMore || bstate == BlockState.FinishStarted)
                {
                    if (_codec.AvailableBytesOut == 0)
                    {
                        last_flush = -1; // avoid BUF_ERROR next call, see above
                    }
                    return ZlibConstants.Z_OK;
                    // If flush != Z_NO_FLUSH && avail_out == 0, the next call
                    // of deflate should use the same flush parameter to make sure
                    // that the flush is complete. So we don't have to output an
                    // empty block here, this will be done at next call. This also
                    // ensures that for a very small output buffer, we emit at most
                    // one empty block.
                }

                if (bstate == BlockState.BlockDone)
                {
                    if (flush == FlushType.Partial)
                    {
                        _tr_align();
                    }
                    else
                    {
                        // FlushType.Full or FlushType.Sync
                        _tr_stored_block(0, 0, false);
                        // For a full flush, this empty block will be recognized
                        // as a special marker by inflate_sync().
                        if (flush == FlushType.Full)
                        {
                            // clear hash (forget the history)
                            for (int i = 0; i < hash_size; i++)
                                head[i] = 0;
                        }
                    }
                    _codec.flush_pending();
                    if (_codec.AvailableBytesOut == 0)
                    {
                        last_flush = -1; // avoid BUF_ERROR at next call, see above
                        return ZlibConstants.Z_OK;
                    }
                }
            }

            if (flush != FlushType.Finish)
                return ZlibConstants.Z_OK;

            if (!WantRfc1950HeaderBytes || Rfc1950BytesEmitted)
                return ZlibConstants.Z_STREAM_END;

            // Write the zlib trailer (adler32)
            pending[pendingCount++] = (byte)((_codec._Adler32 & 0xFF000000) >> 24);
            pending[pendingCount++] = (byte)((_codec._Adler32 & 0x00FF0000) >> 16);
            pending[pendingCount++] = (byte)((_codec._Adler32 & 0x0000FF00) >> 8);
            pending[pendingCount++] = (byte)(_codec._Adler32 & 0x000000FF);
            //putShortMSB((int)(SharedUtils.URShift(_codec._Adler32, 16)));
            //putShortMSB((int)(_codec._Adler32 & 0xffff));

            _codec.flush_pending();

            // If avail_out is zero, the application will call deflate again
            // to flush the rest.

            Rfc1950BytesEmitted = true; // write the trailer only once!

            return pendingCount != 0 ? ZlibConstants.Z_OK : ZlibConstants.Z_STREAM_END;
        }

    }

    /// <summary>
    ///   Computes a CRC-32. The CRC-32 algorithm is parameterized - you
    ///   can set the polynomial and enable or disable bit
    ///   reversal. This can be used for GZIP, BZip2, or ZIP.
    /// </summary>
    /// <remarks>
    ///   This type is used internally by DotNetZip; it is generally not used
    ///   directly by applications wishing to create, read, or manipulate zip
    ///   archive files.
    /// </remarks>

    internal class CRC32
    {
        /// <summary>
        ///   Indicates the total number of bytes applied to the CRC.
        /// </summary>
        internal Int64 TotalBytesRead
        {
            get
            {
                return _TotalBytesRead;
            }
        }

        /// <summary>
        /// Indicates the current CRC for all blocks slurped in.
        /// </summary>
        internal Int32 Crc32Result
        {
            get
            {
                return unchecked((Int32)(~_register));
            }
        }

        /// <summary>
        /// Returns the CRC32 for the specified stream.
        /// </summary>
        /// <param name="input">The stream over which to calculate the CRC32</param>
        /// <returns>the CRC32 calculation</returns>
        internal Int32 GetCrc32(System.IO.Stream input)
        {
            return GetCrc32AndCopy(input, null);
        }

        /// <summary>
        /// Returns the CRC32 for the specified stream, and writes the input into the
        /// output stream.
        /// </summary>
        /// <param name="input">The stream over which to calculate the CRC32</param>
        /// <param name="output">The stream into which to deflate the input</param>
        /// <returns>the CRC32 calculation</returns>
        internal Int32 GetCrc32AndCopy(System.IO.Stream input, System.IO.Stream output)
        {
            if (input == null)
                throw new Exception("The input stream must not be null.");

            unchecked
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                int readSize = BUFFER_SIZE;

                _TotalBytesRead = 0;
                int count = input.Read(buffer, 0, readSize);
                if (output != null) output.Write(buffer, 0, count);
                _TotalBytesRead += count;
                while (count > 0)
                {
                    SlurpBlock(buffer, 0, count);
                    count = input.Read(buffer, 0, readSize);
                    if (output != null) output.Write(buffer, 0, count);
                    _TotalBytesRead += count;
                }

                return (Int32)(~_register);
            }
        }


        /// <summary>
        ///   Get the CRC32 for the given (word,byte) combo.  This is a
        ///   computation defined by PKzip for PKZIP 2.0 (weak) encryption.
        /// </summary>
        /// <param name="W">The word to start with.</param>
        /// <param name="B">The byte to combine it with.</param>
        /// <returns>The CRC-ized result.</returns>
        internal Int32 ComputeCrc32(Int32 W, byte B)
        {
            return _InternalComputeCrc32((UInt32)W, B);
        }

        internal Int32 _InternalComputeCrc32(UInt32 W, byte B)
        {
            return (Int32)(crc32Table[(W ^ B) & 0xFF] ^ (W >> 8));
        }


        /// <summary>
        /// Update the value for the running CRC32 using the given block of bytes.
        /// This is useful when using the CRC32() class in a Stream.
        /// </summary>
        /// <param name="block">block of bytes to slurp</param>
        /// <param name="offset">starting point in the block</param>
        /// <param name="count">how many bytes within the block to slurp</param>
        internal void SlurpBlock(byte[] block, int offset, int count)
        {
            if (block == null)
                throw new Exception("The data buffer must not be null.");

            // bzip algorithm
            for (int i = 0; i < count; i++)
            {
                int x = offset + i;
                byte b = block[x];
                if (this.reverseBits)
                {
                    UInt32 temp = (_register >> 24) ^ b;
                    _register = (_register << 8) ^ crc32Table[temp];
                }
                else
                {
                    UInt32 temp = (_register & 0x000000FF) ^ b;
                    _register = (_register >> 8) ^ crc32Table[temp];
                }
            }
            _TotalBytesRead += count;
        }


        /// <summary>
        ///   Process one byte in the CRC.
        /// </summary>
        /// <param name = "b">the byte to include into the CRC .  </param>
        internal void UpdateCRC(byte b)
        {
            if (this.reverseBits)
            {
                UInt32 temp = (_register >> 24) ^ b;
                _register = (_register << 8) ^ crc32Table[temp];
            }
            else
            {
                UInt32 temp = (_register & 0x000000FF) ^ b;
                _register = (_register >> 8) ^ crc32Table[temp];
            }
        }

        /// <summary>
        ///   Process a run of N identical bytes into the CRC.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This method serves as an optimization for updating the CRC when a
        ///     run of identical bytes is found. Rather than passing in a buffer of
        ///     length n, containing all identical bytes b, this method accepts the
        ///     byte value and the length of the (virtual) buffer - the length of
        ///     the run.
        ///   </para>
        /// </remarks>
        /// <param name = "b">the byte to include into the CRC.  </param>
        /// <param name = "n">the number of times that byte should be repeated. </param>
        internal void UpdateCRC(byte b, int n)
        {
            while (n-- > 0)
            {
                if (this.reverseBits)
                {
                    uint temp = (_register >> 24) ^ b;
                    _register = (_register << 8) ^ crc32Table[(temp >= 0)
                                                              ? temp
                                                              : (temp + 256)];
                }
                else
                {
                    UInt32 temp = (_register & 0x000000FF) ^ b;
                    _register = (_register >> 8) ^ crc32Table[(temp >= 0)
                                                              ? temp
                                                              : (temp + 256)];

                }
            }
        }



        private static uint ReverseBits(uint data)
        {
            unchecked
            {
                uint ret = data;
                ret = (ret & 0x55555555) << 1 | (ret >> 1) & 0x55555555;
                ret = (ret & 0x33333333) << 2 | (ret >> 2) & 0x33333333;
                ret = (ret & 0x0F0F0F0F) << 4 | (ret >> 4) & 0x0F0F0F0F;
                ret = (ret << 24) | ((ret & 0xFF00) << 8) | ((ret >> 8) & 0xFF00) | (ret >> 24);
                return ret;
            }
        }

        private static byte ReverseBits(byte data)
        {
            unchecked
            {
                uint u = (uint)data * 0x00020202;
                uint m = 0x01044010;
                uint s = u & m;
                uint t = (u << 2) & (m << 1);
                return (byte)((0x01001001 * (s + t)) >> 24);
            }
        }



        private void GenerateLookupTable()
        {
            crc32Table = new UInt32[256];
            unchecked
            {
                UInt32 dwCrc;
                byte i = 0;
                do
                {
                    dwCrc = i;
                    for (byte j = 8; j > 0; j--)
                    {
                        if ((dwCrc & 1) == 1)
                        {
                            dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                        }
                        else
                        {
                            dwCrc >>= 1;
                        }
                    }
                    if (reverseBits)
                    {
                        crc32Table[ReverseBits(i)] = ReverseBits(dwCrc);
                    }
                    else
                    {
                        crc32Table[i] = dwCrc;
                    }
                    i++;
                } while (i != 0);
            }

#if VERBOSE
            Console.WriteLine();
            Console.WriteLine("private static readonly UInt32[] crc32Table = {");
            for (int i = 0; i < crc32Table.Length; i+=4)
            {
                Console.Write("   ");
                for (int j=0; j < 4; j++)
                {
                    Console.Write(" 0x{0:X8}U,", crc32Table[i+j]);
                }
                Console.WriteLine();
            }
            Console.WriteLine("};");
            Console.WriteLine();
#endif
        }


        private uint gf2_matrix_times(uint[] matrix, uint vec)
        {
            uint sum = 0;
            int i = 0;
            while (vec != 0)
            {
                if ((vec & 0x01) == 0x01)
                    sum ^= matrix[i];
                vec >>= 1;
                i++;
            }
            return sum;
        }

        private void gf2_matrix_square(uint[] square, uint[] mat)
        {
            for (int i = 0; i < 32; i++)
                square[i] = gf2_matrix_times(mat, mat[i]);
        }



        /// <summary>
        ///   Combines the given CRC32 value with the current running total.
        /// </summary>
        /// <remarks>
        ///   This is useful when using a divide-and-conquer approach to
        ///   calculating a CRC.  Multiple threads can each calculate a
        ///   CRC32 on a segment of the data, and then combine the
        ///   individual CRC32 values at the end.
        /// </remarks>
        /// <param name="crc">the crc value to be combined with this one</param>
        /// <param name="length">the length of data the CRC value was calculated on</param>
        internal void Combine(int crc, int length)
        {
            uint[] even = new uint[32];     // even-power-of-two zeros operator
            uint[] odd = new uint[32];      // odd-power-of-two zeros operator

            if (length == 0)
                return;

            uint crc1 = ~_register;
            uint crc2 = (uint)crc;

            // put operator for one zero bit in odd
            odd[0] = this.dwPolynomial;  // the CRC-32 polynomial
            uint row = 1;
            for (int i = 1; i < 32; i++)
            {
                odd[i] = row;
                row <<= 1;
            }

            // put operator for two zero bits in even
            gf2_matrix_square(even, odd);

            // put operator for four zero bits in odd
            gf2_matrix_square(odd, even);

            uint len2 = (uint)length;

            // apply len2 zeros to crc1 (first square will put the operator for one
            // zero byte, eight zero bits, in even)
            do
            {
                // apply zeros operator for this bit of len2
                gf2_matrix_square(even, odd);

                if ((len2 & 1) == 1)
                    crc1 = gf2_matrix_times(even, crc1);
                len2 >>= 1;

                if (len2 == 0)
                    break;

                // another iteration of the loop with odd and even swapped
                gf2_matrix_square(odd, even);
                if ((len2 & 1) == 1)
                    crc1 = gf2_matrix_times(odd, crc1);
                len2 >>= 1;


            } while (len2 != 0);

            crc1 ^= crc2;

            _register = ~crc1;

            //return (int) crc1;
            return;
        }


        /// <summary>
        ///   Create an instance of the CRC32 class using the default settings: no
        ///   bit reversal, and a polynomial of 0xEDB88320.
        /// </summary>
        internal CRC32()
            : this(false)
        {
        }

        /// <summary>
        ///   Create an instance of the CRC32 class, specifying whether to reverse
        ///   data bits or not.
        /// </summary>
        /// <param name='reverseBits'>
        ///   specify true if the instance should reverse data bits.
        /// </param>
        /// <remarks>
        ///   <para>
        ///     In the CRC-32 used by BZip2, the bits are reversed. Therefore if you
        ///     want a CRC32 with compatibility with BZip2, you should pass true
        ///     here. In the CRC-32 used by GZIP and PKZIP, the bits are not
        ///     reversed; Therefore if you want a CRC32 with compatibility with
        ///     those, you should pass false.
        ///   </para>
        /// </remarks>
        internal CRC32(bool reverseBits) :
            this(unchecked((int)0xEDB88320), reverseBits)
        {
        }


        /// <summary>
        ///   Create an instance of the CRC32 class, specifying the polynomial and
        ///   whether to reverse data bits or not.
        /// </summary>
        /// <param name='polynomial'>
        ///   The polynomial to use for the CRC, expressed in the reversed (LSB)
        ///   format: the highest ordered bit in the polynomial value is the
        ///   coefficient of the 0th power; the second-highest order bit is the
        ///   coefficient of the 1 power, and so on. Expressed this way, the
        ///   polynomial for the CRC-32C used in IEEE 802.3, is 0xEDB88320.
        /// </param>
        /// <param name='reverseBits'>
        ///   specify true if the instance should reverse data bits.
        /// </param>
        ///
        /// <remarks>
        ///   <para>
        ///     In the CRC-32 used by BZip2, the bits are reversed. Therefore if you
        ///     want a CRC32 with compatibility with BZip2, you should pass true
        ///     here for the <c>reverseBits</c> parameter. In the CRC-32 used by
        ///     GZIP and PKZIP, the bits are not reversed; Therefore if you want a
        ///     CRC32 with compatibility with those, you should pass false for the
        ///     <c>reverseBits</c> parameter.
        ///   </para>
        /// </remarks>
        internal CRC32(int polynomial, bool reverseBits)
        {
            this.reverseBits = reverseBits;
            this.dwPolynomial = (uint)polynomial;
            this.GenerateLookupTable();
        }

        /// <summary>
        ///   Reset the CRC-32 class - clear the CRC "remainder register."
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Use this when employing a single instance of this class to compute
        ///     multiple, distinct CRCs on multiple, distinct data blocks.
        ///   </para>
        /// </remarks>
        internal void Reset()
        {
            _register = 0xFFFFFFFFU;
        }

        // private member vars
        private UInt32 dwPolynomial;
        private Int64 _TotalBytesRead;
        private bool reverseBits;
        private UInt32[] crc32Table;
        private const int BUFFER_SIZE = 8192;
        private UInt32 _register = 0xFFFFFFFFU;
    }


    /// <summary>
    /// A Stream that calculates a CRC32 (a checksum) on all bytes read,
    /// or on all bytes written.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    /// This class can be used to verify the CRC of a ZipEntry when
    /// reading from a stream, or to calculate a CRC when writing to a
    /// stream.  The stream should be used to either read, or write, but
    /// not both.  If you intermix reads and writes, the results are not
    /// defined.
    /// </para>
    ///
    /// <para>
    /// This class is intended primarily for use internally by the
    /// DotNetZip library.
    /// </para>
    /// </remarks>
    internal class CrcCalculatorStream : System.IO.Stream, System.IDisposable
    {
        private static readonly Int64 UnsetLengthLimit = -99;

        internal System.IO.Stream _innerStream;
        private CRC32 _Crc32;
        private Int64 _lengthLimit = -99;
        private bool _leaveOpen;

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Instances returned from this constructor will leave the underlying
        ///     stream open upon Close().  The stream uses the default CRC32
        ///     algorithm, which implies a polynomial of 0xEDB88320.
        ///   </para>
        /// </remarks>
        /// <param name="stream">The underlying stream</param>
        internal CrcCalculatorStream(System.IO.Stream stream)
            : this(true, CrcCalculatorStream.UnsetLengthLimit, stream, null)
        {
        }

        /// <summary>
        ///   The constructor allows the caller to specify how to handle the
        ///   underlying stream at close.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The stream uses the default CRC32 algorithm, which implies a
        ///     polynomial of 0xEDB88320.
        ///   </para>
        /// </remarks>
        /// <param name="stream">The underlying stream</param>
        /// <param name="leaveOpen">true to leave the underlying stream
        /// open upon close of the <c>CrcCalculatorStream</c>; false otherwise.</param>
        internal CrcCalculatorStream(System.IO.Stream stream, bool leaveOpen)
            : this(leaveOpen, CrcCalculatorStream.UnsetLengthLimit, stream, null)
        {
        }

        /// <summary>
        ///   A constructor allowing the specification of the length of the stream
        ///   to read.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The stream uses the default CRC32 algorithm, which implies a
        ///     polynomial of 0xEDB88320.
        ///   </para>
        ///   <para>
        ///     Instances returned from this constructor will leave the underlying
        ///     stream open upon Close().
        ///   </para>
        /// </remarks>
        /// <param name="stream">The underlying stream</param>
        /// <param name="length">The length of the stream to slurp</param>
        internal CrcCalculatorStream(System.IO.Stream stream, Int64 length)
            : this(true, length, stream, null)
        {
            if (length < 0)
                throw new ArgumentException("length");
        }

        /// <summary>
        ///   A constructor allowing the specification of the length of the stream
        ///   to read, as well as whether to keep the underlying stream open upon
        ///   Close().
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The stream uses the default CRC32 algorithm, which implies a
        ///     polynomial of 0xEDB88320.
        ///   </para>
        /// </remarks>
        /// <param name="stream">The underlying stream</param>
        /// <param name="length">The length of the stream to slurp</param>
        /// <param name="leaveOpen">true to leave the underlying stream
        /// open upon close of the <c>CrcCalculatorStream</c>; false otherwise.</param>
        internal CrcCalculatorStream(System.IO.Stream stream, Int64 length, bool leaveOpen)
            : this(leaveOpen, length, stream, null)
        {
            if (length < 0)
                throw new ArgumentException("length");
        }

        /// <summary>
        ///   A constructor allowing the specification of the length of the stream
        ///   to read, as well as whether to keep the underlying stream open upon
        ///   Close(), and the CRC32 instance to use.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The stream uses the specified CRC32 instance, which allows the
        ///     application to specify how the CRC gets calculated.
        ///   </para>
        /// </remarks>
        /// <param name="stream">The underlying stream</param>
        /// <param name="length">The length of the stream to slurp</param>
        /// <param name="leaveOpen">true to leave the underlying stream
        /// open upon close of the <c>CrcCalculatorStream</c>; false otherwise.</param>
        /// <param name="crc32">the CRC32 instance to use to calculate the CRC32</param>
        internal CrcCalculatorStream(System.IO.Stream stream, Int64 length, bool leaveOpen,
                                   CRC32 crc32)
            : this(leaveOpen, length, stream, crc32)
        {
            if (length < 0)
                throw new ArgumentException("length");
        }


        // This ctor is private - no validation is done here.  This is to allow the use
        // of a (specific) negative value for the _lengthLimit, to indicate that there
        // is no length set.  So we validate the length limit in those ctors that use an
        // explicit param, otherwise we don't validate, because it could be our special
        // value.
        private CrcCalculatorStream
            (bool leaveOpen, Int64 length, System.IO.Stream stream, CRC32 crc32)
            : base()
        {
            _innerStream = stream;
            _Crc32 = crc32 ?? new CRC32();
            _lengthLimit = length;
            _leaveOpen = leaveOpen;
        }


        /// <summary>
        ///   Gets the total number of bytes run through the CRC32 calculator.
        /// </summary>
        ///
        /// <remarks>
        ///   This is either the total number of bytes read, or the total number of
        ///   bytes written, depending on the direction of this stream.
        /// </remarks>
        internal Int64 TotalBytesSlurped
        {
            get { return _Crc32.TotalBytesRead; }
        }

        /// <summary>
        ///   Provides the current CRC for all blocks slurped in.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The running total of the CRC is kept as data is written or read
        ///     through the stream.  read this property after all reads or writes to
        ///     get an accurate CRC for the entire stream.
        ///   </para>
        /// </remarks>
        internal Int32 Crc
        {
            get { return _Crc32.Crc32Result; }
        }

        /// <summary>
        ///   Indicates whether the underlying stream will be left open when the
        ///   <c>CrcCalculatorStream</c> is Closed.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Set this at any point before calling <see cref="Stream.Close"/>.
        ///   </para>
        /// </remarks>
        internal bool LeaveOpen
        {
            get { return _leaveOpen; }
            set { _leaveOpen = value; }
        }

        /// <summary>
        /// Read from the stream
        /// </summary>
        /// <param name="buffer">the buffer to read</param>
        /// <param name="offset">the offset at which to start</param>
        /// <param name="count">the number of bytes to read</param>
        /// <returns>the number of bytes actually read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesToRead = count;

            // Need to limit the # of bytes returned, if the stream is intended to have
            // a definite length.  This is especially useful when returning a stream for
            // the uncompressed data directly to the application.  The app won't
            // necessarily read only the UncompressedSize number of bytes.  For example
            // wrapping the stream returned from OpenReader() into a StreadReader() and
            // calling ReadToEnd() on it, We can "over-read" the zip data and get a
            // corrupt string.  The length limits that, prevents that problem.

            if (_lengthLimit != CrcCalculatorStream.UnsetLengthLimit)
            {
                if (_Crc32.TotalBytesRead >= _lengthLimit) return 0; // EOF
                Int64 bytesRemaining = _lengthLimit - _Crc32.TotalBytesRead;
                if (bytesRemaining < count) bytesToRead = (int)bytesRemaining;
            }
            int n = _innerStream.Read(buffer, offset, bytesToRead);
            if (n > 0) _Crc32.SlurpBlock(buffer, offset, n);
            return n;
        }

        /// <summary>
        /// Write to the stream.
        /// </summary>
        /// <param name="buffer">the buffer from which to write</param>
        /// <param name="offset">the offset at which to start writing</param>
        /// <param name="count">the number of bytes to write</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > 0) _Crc32.SlurpBlock(buffer, offset, count);
            _innerStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Indicates whether the stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return _innerStream.CanRead; }
        }

        /// <summary>
        ///   Indicates whether the stream supports seeking.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Always returns false.
        ///   </para>
        /// </remarks>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Indicates whether the stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return _innerStream.CanWrite; }
        }

        /// <summary>
        /// Flush the stream.
        /// </summary>
        public override void Flush()
        {
            _innerStream.Flush();
        }

        /// <summary>
        ///   Returns the length of the underlying stream.
        /// </summary>
        public override long Length
        {
            get
            {
                if (_lengthLimit == CrcCalculatorStream.UnsetLengthLimit)
                    return _innerStream.Length;
                else return _lengthLimit;
            }
        }

        /// <summary>
        ///   The getter for this property returns the total bytes read.
        ///   If you use the setter, it will throw
        /// <see cref="NotSupportedException"/>.
        /// </summary>
        public override long Position
        {
            get { return _Crc32.TotalBytesRead; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Seeking is not supported on this stream. This method always throws
        /// <see cref="NotSupportedException"/>
        /// </summary>
        /// <param name="offset">N/A</param>
        /// <param name="origin">N/A</param>
        /// <returns>N/A</returns>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method always throws
        /// <see cref="NotSupportedException"/>
        /// </summary>
        /// <param name="value">N/A</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }


        void IDisposable.Dispose()
        {
            base.Dispose();
        }
    }

    /// <summary>
    /// A custom encoding class that provides encoding capabilities for the
    /// 'Western European (ISO)' encoding under Silverlight.<br/>
    /// This class was generated by a tool. For more information, visit
    /// http://www.hardcodet.net/2010/03/silverlight-text-encoding-class-generator
    /// </summary>
    internal class Iso88591Encoding : Encoding
    {
        /// <summary>
        /// Gets the name registered with the
        /// Internet Assigned Numbers Authority (IANA) for the current encoding.
        /// </summary>
        /// <returns>
        /// The IANA name for the current <see cref="System.Text.Encoding"/>.
        /// </returns>
        public override string WebName
        {
            get
            {
                return "iso-8859-1";
            }
        }


        private char? fallbackCharacter;

        /// <summary>
        /// A character that can be set in order to make the encoding class
        /// more fault tolerant. If this property is set, the encoding class will
        /// use this property instead of throwing an exception if an unsupported
        /// byte value is being passed for decoding.
        /// </summary>
        public char? FallbackCharacter
        {
            get { return fallbackCharacter; }
            set
            {
                fallbackCharacter = value;

                if (value.HasValue && !charToByte.ContainsKey(value.Value))
                {
                    string msg = "Cannot use the character [{0}] (int value {1}) as fallback value "
                    + "- the fallback character itself is not supported by the encoding.";
                    msg = String.Format(msg, value.Value, (int)value.Value);
                    throw new EncoderFallbackException(msg);
                }

                FallbackByte = value.HasValue ? charToByte[value.Value] : (byte?)null;
            }
        }

        /// <summary>
        /// A byte value that corresponds to the <see cref="FallbackCharacter"/>.
        /// It is used in encoding scenarios in case an unsupported character is
        /// being passed for encoding.
        /// </summary>
        public byte? FallbackByte { get; private set; }


        public Iso88591Encoding()
        {
            FallbackCharacter = '?';
        }

        /// <summary>
        /// Encodes a set of characters from the specified character array into the specified byte array.
        /// </summary>
        /// <returns>
        /// The actual number of bytes written into <paramref name="bytes"/>.
        /// </returns>
        /// <param name="chars">The character array containing the set of characters to encode. 
        /// </param><param name="charIndex">The index of the first character to encode. 
        /// </param><param name="charCount">The number of characters to encode. 
        /// </param><param name="bytes">The byte array to contain the resulting sequence of bytes.
        /// </param><param name="byteIndex">The index at which to start writing the resulting sequence of bytes. 
        /// </param>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            return FallbackByte.HasValue
                     ? GetBytesWithFallBack(chars, charIndex, charCount, bytes, byteIndex)
                     : GetBytesWithoutFallback(chars, charIndex, charCount, bytes, byteIndex);
        }


        private int GetBytesWithFallBack(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (int i = 0; i < charCount; i++)
            {
                var character = chars[i + charIndex];
                byte byteValue;
                bool status = charToByte.TryGetValue(character, out byteValue);

                bytes[byteIndex + i] = status ? byteValue : FallbackByte.Value;
            }

            return charCount;
        }

        private int GetBytesWithoutFallback(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (int i = 0; i < charCount; i++)
            {
                var character = chars[i + charIndex];
                byte byteValue;
                bool status = charToByte.TryGetValue(character, out byteValue);

                if (!status)
                {
                    //throw exception
                    string msg =
                      "The encoding [{0}] cannot encode the character [{1}] (int value {2}). Set the FallbackCharacter property in order to suppress this exception and encode a default character instead.";
                    msg = String.Format(msg, WebName, character, (int)character);
                    throw new EncoderFallbackException(msg);
                }

                bytes[byteIndex + i] = byteValue;
            }

            return charCount;
        }



        /// <summary>
        /// Decodes a sequence of bytes from the specified byte array into the specified character array.
        /// </summary>
        /// <returns>
        /// The actual number of characters written into <paramref name="chars"/>.
        /// </returns>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode. 
        /// </param><param name="byteIndex">The index of the first byte to decode. 
        /// </param><param name="byteCount">The number of bytes to decode. 
        /// </param><param name="chars">The character array to contain the resulting set of characters. 
        /// </param><param name="charIndex">The index at which to start writing the resulting set of characters. 
        /// </param>
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return FallbackCharacter.HasValue
                     ? GetCharsWithFallback(bytes, byteIndex, byteCount, chars, charIndex)
                     : GetCharsWithoutFallback(bytes, byteIndex, byteCount, chars, charIndex);
        }


        private int GetCharsWithFallback(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (int i = 0; i < byteCount; i++)
            {
                byte lookupIndex = bytes[i + byteIndex];

                //if the byte value is not in our lookup array, fall back to default character
                char result = lookupIndex >= byteToChar.Length
                                ? FallbackCharacter.Value
                                : byteToChar[lookupIndex];

                chars[charIndex + i] = result;
            }

            return byteCount;
        }



        private int GetCharsWithoutFallback(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (int i = 0; i < byteCount; i++)
            {
                byte lookupIndex = bytes[i + byteIndex];
                if (lookupIndex >= byteToChar.Length)
                {
                    //throw exception
                    string msg = "The encoding [{0}] cannot decode byte value [{1}]. Set the FallbackCharacter property in order to suppress this exception and decode the value as a default character instead.";
                    msg = String.Format(msg, WebName, lookupIndex);
                    throw new EncoderFallbackException(msg);
                }


                chars[charIndex + i] = byteToChar[lookupIndex];
            }

            return byteCount;
        }



        /// <summary>
        /// Calculates the number of bytes produced by encoding a set of characters
        /// from the specified character array.
        /// </summary>
        /// <returns>
        /// The number of bytes produced by encoding the specified characters. This class
        /// always returns the value of <paramref name="count"/>.
        /// </returns>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }


        /// <summary>
        /// Calculates the number of characters produced by decoding a sequence
        /// of bytes from the specified byte array.
        /// </summary>
        /// <returns>
        /// The number of characters produced by decoding the specified sequence of bytes. This class
        /// always returns the value of <paramref name="count"/>. 
        /// </returns>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }


        /// <summary>
        /// Calculates the maximum number of bytes produced by encoding the specified number of characters.
        /// </summary>
        /// <returns>
        /// The maximum number of bytes produced by encoding the specified number of characters. This
        /// class always returns the value of <paramref name="charCount"/>.
        /// </returns>
        /// <param name="charCount">The number of characters to encode. 
        /// </param>
        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        /// <summary>
        /// Calculates the maximum number of characters produced by decoding the specified number of bytes.
        /// </summary>
        /// <returns>
        /// The maximum number of characters produced by decoding the specified number of bytes. This class
        /// always returns the value of <paramref name="byteCount"/>.
        /// </returns>
        /// <param name="byteCount">The number of bytes to decode.</param> 
        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }


        /// <summary>
        /// Gets the number of characters that are supported by this encoding.
        /// This property returns a maximum value of 256, as the encoding class
        /// only supports single byte encodings (1 byte == 256 possible values).
        /// </summary>
        public static int CharacterCount
        {
            get { return byteToChar.Length; }
        }

        #region Character Table

        /// <summary>
        /// This table contains characters in an array. The index within the
        /// array corresponds to the encoding's mapping of bytes to characters
        /// (e.g. if a byte value of 5 is used to encode the character 'x', this
        /// character will be stored at the array index 5.
        /// </summary>
        private static char[] byteToChar = new char[]
        {
          (char)0 /* byte 0 */  ,
          (char)1 /* byte 1 */  ,
          (char)2 /* byte 2 */  ,
          (char)3 /* byte 3 */  ,
          (char)4 /* byte 4 */  ,
          (char)5 /* byte 5 */  ,
          (char)6 /* byte 6 */  ,
          (char)7 /* byte 7 */  ,
          (char)8 /* byte 8 */  ,
          (char)9 /* byte 9 */  ,
          (char)10 /* byte 10 */  ,
          (char)11 /* byte 11 */  ,
          (char)12 /* byte 12 */  ,
          (char)13 /* byte 13 */  ,
          (char)14 /* byte 14 */  ,
          (char)15 /* byte 15 */  ,
          (char)16 /* byte 16 */  ,
          (char)17 /* byte 17 */  ,
          (char)18 /* byte 18 */  ,
          (char)19 /* byte 19 */  ,
          (char)20 /* byte 20 */  ,
          (char)21 /* byte 21 */  ,
          (char)22 /* byte 22 */  ,
          (char)23 /* byte 23 */  ,
          (char)24 /* byte 24 */  ,
          (char)25 /* byte 25 */  ,
          (char)26 /* byte 26 */  ,
          (char)27 /* byte 27 */  ,
          (char)28 /* byte 28 */  ,
          (char)29 /* byte 29 */  ,
          (char)30 /* byte 30 */  ,
          (char)31 /* byte 31 */  ,
          (char)32 /* byte 32 */  ,
          (char)33 /* byte 33 */  ,
          (char)34 /* byte 34 */  ,
          (char)35 /* byte 35 */  ,
          (char)36 /* byte 36 */  ,
          (char)37 /* byte 37 */  ,
          (char)38 /* byte 38 */  ,
          (char)39 /* byte 39 */  ,
          (char)40 /* byte 40 */  ,
          (char)41 /* byte 41 */  ,
          (char)42 /* byte 42 */  ,
          (char)43 /* byte 43 */  ,
          (char)44 /* byte 44 */  ,
          (char)45 /* byte 45 */  ,
          (char)46 /* byte 46 */  ,
          (char)47 /* byte 47 */  ,
          (char)48 /* byte 48 */  ,
          (char)49 /* byte 49 */  ,
          (char)50 /* byte 50 */  ,
          (char)51 /* byte 51 */  ,
          (char)52 /* byte 52 */  ,
          (char)53 /* byte 53 */  ,
          (char)54 /* byte 54 */  ,
          (char)55 /* byte 55 */  ,
          (char)56 /* byte 56 */  ,
          (char)57 /* byte 57 */  ,
          (char)58 /* byte 58 */  ,
          (char)59 /* byte 59 */  ,
          (char)60 /* byte 60 */  ,
          (char)61 /* byte 61 */  ,
          (char)62 /* byte 62 */  ,
          (char)63 /* byte 63 */  ,
          (char)64 /* byte 64 */  ,
          (char)65 /* byte 65 */  ,
          (char)66 /* byte 66 */  ,
          (char)67 /* byte 67 */  ,
          (char)68 /* byte 68 */  ,
          (char)69 /* byte 69 */  ,
          (char)70 /* byte 70 */  ,
          (char)71 /* byte 71 */  ,
          (char)72 /* byte 72 */  ,
          (char)73 /* byte 73 */  ,
          (char)74 /* byte 74 */  ,
          (char)75 /* byte 75 */  ,
          (char)76 /* byte 76 */  ,
          (char)77 /* byte 77 */  ,
          (char)78 /* byte 78 */  ,
          (char)79 /* byte 79 */  ,
          (char)80 /* byte 80 */  ,
          (char)81 /* byte 81 */  ,
          (char)82 /* byte 82 */  ,
          (char)83 /* byte 83 */  ,
          (char)84 /* byte 84 */  ,
          (char)85 /* byte 85 */  ,
          (char)86 /* byte 86 */  ,
          (char)87 /* byte 87 */  ,
          (char)88 /* byte 88 */  ,
          (char)89 /* byte 89 */  ,
          (char)90 /* byte 90 */  ,
          (char)91 /* byte 91 */  ,
          (char)92 /* byte 92 */  ,
          (char)93 /* byte 93 */  ,
          (char)94 /* byte 94 */  ,
          (char)95 /* byte 95 */  ,
          (char)96 /* byte 96 */  ,
          (char)97 /* byte 97 */  ,
          (char)98 /* byte 98 */  ,
          (char)99 /* byte 99 */  ,
          (char)100 /* byte 100 */  ,
          (char)101 /* byte 101 */  ,
          (char)102 /* byte 102 */  ,
          (char)103 /* byte 103 */  ,
          (char)104 /* byte 104 */  ,
          (char)105 /* byte 105 */  ,
          (char)106 /* byte 106 */  ,
          (char)107 /* byte 107 */  ,
          (char)108 /* byte 108 */  ,
          (char)109 /* byte 109 */  ,
          (char)110 /* byte 110 */  ,
          (char)111 /* byte 111 */  ,
          (char)112 /* byte 112 */  ,
          (char)113 /* byte 113 */  ,
          (char)114 /* byte 114 */  ,
          (char)115 /* byte 115 */  ,
          (char)116 /* byte 116 */  ,
          (char)117 /* byte 117 */  ,
          (char)118 /* byte 118 */  ,
          (char)119 /* byte 119 */  ,
          (char)120 /* byte 120 */  ,
          (char)121 /* byte 121 */  ,
          (char)122 /* byte 122 */  ,
          (char)123 /* byte 123 */  ,
          (char)124 /* byte 124 */  ,
          (char)125 /* byte 125 */  ,
          (char)126 /* byte 126 */  ,
          (char)127 /* byte 127 */  ,
          (char)128 /* byte 128 */  ,
          (char)129 /* byte 129 */  ,
          (char)130 /* byte 130 */  ,
          (char)131 /* byte 131 */  ,
          (char)132 /* byte 132 */  ,
          (char)133 /* byte 133 */  ,
          (char)134 /* byte 134 */  ,
          (char)135 /* byte 135 */  ,
          (char)136 /* byte 136 */  ,
          (char)137 /* byte 137 */  ,
          (char)138 /* byte 138 */  ,
          (char)139 /* byte 139 */  ,
          (char)140 /* byte 140 */  ,
          (char)141 /* byte 141 */  ,
          (char)142 /* byte 142 */  ,
          (char)143 /* byte 143 */  ,
          (char)144 /* byte 144 */  ,
          (char)145 /* byte 145 */  ,
          (char)146 /* byte 146 */  ,
          (char)147 /* byte 147 */  ,
          (char)148 /* byte 148 */  ,
          (char)149 /* byte 149 */  ,
          (char)150 /* byte 150 */  ,
          (char)151 /* byte 151 */  ,
          (char)152 /* byte 152 */  ,
          (char)153 /* byte 153 */  ,
          (char)154 /* byte 154 */  ,
          (char)155 /* byte 155 */  ,
          (char)156 /* byte 156 */  ,
          (char)157 /* byte 157 */  ,
          (char)158 /* byte 158 */  ,
          (char)159 /* byte 159 */  ,
          (char)160 /* byte 160 */  ,
          (char)161 /* byte 161 */  ,
          (char)162 /* byte 162 */  ,
          (char)163 /* byte 163 */  ,
          (char)164 /* byte 164 */  ,
          (char)165 /* byte 165 */  ,
          (char)166 /* byte 166 */  ,
          (char)167 /* byte 167 */  ,
          (char)168 /* byte 168 */  ,
          (char)169 /* byte 169 */  ,
          (char)170 /* byte 170 */  ,
          (char)171 /* byte 171 */  ,
          (char)172 /* byte 172 */  ,
          (char)173 /* byte 173 */  ,
          (char)174 /* byte 174 */  ,
          (char)175 /* byte 175 */  ,
          (char)176 /* byte 176 */  ,
          (char)177 /* byte 177 */  ,
          (char)178 /* byte 178 */  ,
          (char)179 /* byte 179 */  ,
          (char)180 /* byte 180 */  ,
          (char)181 /* byte 181 */  ,
          (char)182 /* byte 182 */  ,
          (char)183 /* byte 183 */  ,
          (char)184 /* byte 184 */  ,
          (char)185 /* byte 185 */  ,
          (char)186 /* byte 186 */  ,
          (char)187 /* byte 187 */  ,
          (char)188 /* byte 188 */  ,
          (char)189 /* byte 189 */  ,
          (char)190 /* byte 190 */  ,
          (char)191 /* byte 191 */  ,
          (char)192 /* byte 192 */  ,
          (char)193 /* byte 193 */  ,
          (char)194 /* byte 194 */  ,
          (char)195 /* byte 195 */  ,
          (char)196 /* byte 196 */  ,
          (char)197 /* byte 197 */  ,
          (char)198 /* byte 198 */  ,
          (char)199 /* byte 199 */  ,
          (char)200 /* byte 200 */  ,
          (char)201 /* byte 201 */  ,
          (char)202 /* byte 202 */  ,
          (char)203 /* byte 203 */  ,
          (char)204 /* byte 204 */  ,
          (char)205 /* byte 205 */  ,
          (char)206 /* byte 206 */  ,
          (char)207 /* byte 207 */  ,
          (char)208 /* byte 208 */  ,
          (char)209 /* byte 209 */  ,
          (char)210 /* byte 210 */  ,
          (char)211 /* byte 211 */  ,
          (char)212 /* byte 212 */  ,
          (char)213 /* byte 213 */  ,
          (char)214 /* byte 214 */  ,
          (char)215 /* byte 215 */  ,
          (char)216 /* byte 216 */  ,
          (char)217 /* byte 217 */  ,
          (char)218 /* byte 218 */  ,
          (char)219 /* byte 219 */  ,
          (char)220 /* byte 220 */  ,
          (char)221 /* byte 221 */  ,
          (char)222 /* byte 222 */  ,
          (char)223 /* byte 223 */  ,
          (char)224 /* byte 224 */  ,
          (char)225 /* byte 225 */  ,
          (char)226 /* byte 226 */  ,
          (char)227 /* byte 227 */  ,
          (char)228 /* byte 228 */  ,
          (char)229 /* byte 229 */  ,
          (char)230 /* byte 230 */  ,
          (char)231 /* byte 231 */  ,
          (char)232 /* byte 232 */  ,
          (char)233 /* byte 233 */  ,
          (char)234 /* byte 234 */  ,
          (char)235 /* byte 235 */  ,
          (char)236 /* byte 236 */  ,
          (char)237 /* byte 237 */  ,
          (char)238 /* byte 238 */  ,
          (char)239 /* byte 239 */  ,
          (char)240 /* byte 240 */  ,
          (char)241 /* byte 241 */  ,
          (char)242 /* byte 242 */  ,
          (char)243 /* byte 243 */  ,
          (char)244 /* byte 244 */  ,
          (char)245 /* byte 245 */  ,
          (char)246 /* byte 246 */  ,
          (char)247 /* byte 247 */  ,
          (char)248 /* byte 248 */  ,
          (char)249 /* byte 249 */  ,
          (char)250 /* byte 250 */  ,
          (char)251 /* byte 251 */  ,
          (char)252 /* byte 252 */  ,
          (char)253 /* byte 253 */  ,
          (char)254 /* byte 254 */  ,
          (char)255 /* byte 255 */  
        };

        #endregion

        #region Byte Lookup Dictionary

        /// <summary>
        /// This dictionary is used to resolve byte values for a given character.
        /// </summary>
        private static Dictionary<char, byte> charToByte = new Dictionary<char, byte>
        {
          { (char)0, 0 },
          { (char)1, 1 },
          { (char)2, 2 },
          { (char)3, 3 },
          { (char)4, 4 },
          { (char)5, 5 },
          { (char)6, 6 },
          { (char)7, 7 },
          { (char)8, 8 },
          { (char)9, 9 },
          { (char)10, 10 },
          { (char)11, 11 },
          { (char)12, 12 },
          { (char)13, 13 },
          { (char)14, 14 },
          { (char)15, 15 },
          { (char)16, 16 },
          { (char)17, 17 },
          { (char)18, 18 },
          { (char)19, 19 },
          { (char)20, 20 },
          { (char)21, 21 },
          { (char)22, 22 },
          { (char)23, 23 },
          { (char)24, 24 },
          { (char)25, 25 },
          { (char)26, 26 },
          { (char)27, 27 },
          { (char)28, 28 },
          { (char)29, 29 },
          { (char)30, 30 },
          { (char)31, 31 },
          { (char)32, 32 },
          { (char)33, 33 },
          { (char)34, 34 },
          { (char)35, 35 },
          { (char)36, 36 },
          { (char)37, 37 },
          { (char)38, 38 },
          { (char)39, 39 },
          { (char)40, 40 },
          { (char)41, 41 },
          { (char)42, 42 },
          { (char)43, 43 },
          { (char)44, 44 },
          { (char)45, 45 },
          { (char)46, 46 },
          { (char)47, 47 },
          { (char)48, 48 },
          { (char)49, 49 },
          { (char)50, 50 },
          { (char)51, 51 },
          { (char)52, 52 },
          { (char)53, 53 },
          { (char)54, 54 },
          { (char)55, 55 },
          { (char)56, 56 },
          { (char)57, 57 },
          { (char)58, 58 },
          { (char)59, 59 },
          { (char)60, 60 },
          { (char)61, 61 },
          { (char)62, 62 },
          { (char)63, 63 },
          { (char)64, 64 },
          { (char)65, 65 },
          { (char)66, 66 },
          { (char)67, 67 },
          { (char)68, 68 },
          { (char)69, 69 },
          { (char)70, 70 },
          { (char)71, 71 },
          { (char)72, 72 },
          { (char)73, 73 },
          { (char)74, 74 },
          { (char)75, 75 },
          { (char)76, 76 },
          { (char)77, 77 },
          { (char)78, 78 },
          { (char)79, 79 },
          { (char)80, 80 },
          { (char)81, 81 },
          { (char)82, 82 },
          { (char)83, 83 },
          { (char)84, 84 },
          { (char)85, 85 },
          { (char)86, 86 },
          { (char)87, 87 },
          { (char)88, 88 },
          { (char)89, 89 },
          { (char)90, 90 },
          { (char)91, 91 },
          { (char)92, 92 },
          { (char)93, 93 },
          { (char)94, 94 },
          { (char)95, 95 },
          { (char)96, 96 },
          { (char)97, 97 },
          { (char)98, 98 },
          { (char)99, 99 },
          { (char)100, 100 },
          { (char)101, 101 },
          { (char)102, 102 },
          { (char)103, 103 },
          { (char)104, 104 },
          { (char)105, 105 },
          { (char)106, 106 },
          { (char)107, 107 },
          { (char)108, 108 },
          { (char)109, 109 },
          { (char)110, 110 },
          { (char)111, 111 },
          { (char)112, 112 },
          { (char)113, 113 },
          { (char)114, 114 },
          { (char)115, 115 },
          { (char)116, 116 },
          { (char)117, 117 },
          { (char)118, 118 },
          { (char)119, 119 },
          { (char)120, 120 },
          { (char)121, 121 },
          { (char)122, 122 },
          { (char)123, 123 },
          { (char)124, 124 },
          { (char)125, 125 },
          { (char)126, 126 },
          { (char)127, 127 },
          { (char)128, 128 },
          { (char)129, 129 },
          { (char)130, 130 },
          { (char)131, 131 },
          { (char)132, 132 },
          { (char)133, 133 },
          { (char)134, 134 },
          { (char)135, 135 },
          { (char)136, 136 },
          { (char)137, 137 },
          { (char)138, 138 },
          { (char)139, 139 },
          { (char)140, 140 },
          { (char)141, 141 },
          { (char)142, 142 },
          { (char)143, 143 },
          { (char)144, 144 },
          { (char)145, 145 },
          { (char)146, 146 },
          { (char)147, 147 },
          { (char)148, 148 },
          { (char)149, 149 },
          { (char)150, 150 },
          { (char)151, 151 },
          { (char)152, 152 },
          { (char)153, 153 },
          { (char)154, 154 },
          { (char)155, 155 },
          { (char)156, 156 },
          { (char)157, 157 },
          { (char)158, 158 },
          { (char)159, 159 },
          { (char)160, 160 },
          { (char)161, 161 },
          { (char)162, 162 },
          { (char)163, 163 },
          { (char)164, 164 },
          { (char)165, 165 },
          { (char)166, 166 },
          { (char)167, 167 },
          { (char)168, 168 },
          { (char)169, 169 },
          { (char)170, 170 },
          { (char)171, 171 },
          { (char)172, 172 },
          { (char)173, 173 },
          { (char)174, 174 },
          { (char)175, 175 },
          { (char)176, 176 },
          { (char)177, 177 },
          { (char)178, 178 },
          { (char)179, 179 },
          { (char)180, 180 },
          { (char)181, 181 },
          { (char)182, 182 },
          { (char)183, 183 },
          { (char)184, 184 },
          { (char)185, 185 },
          { (char)186, 186 },
          { (char)187, 187 },
          { (char)188, 188 },
          { (char)189, 189 },
          { (char)190, 190 },
          { (char)191, 191 },
          { (char)192, 192 },
          { (char)193, 193 },
          { (char)194, 194 },
          { (char)195, 195 },
          { (char)196, 196 },
          { (char)197, 197 },
          { (char)198, 198 },
          { (char)199, 199 },
          { (char)200, 200 },
          { (char)201, 201 },
          { (char)202, 202 },
          { (char)203, 203 },
          { (char)204, 204 },
          { (char)205, 205 },
          { (char)206, 206 },
          { (char)207, 207 },
          { (char)208, 208 },
          { (char)209, 209 },
          { (char)210, 210 },
          { (char)211, 211 },
          { (char)212, 212 },
          { (char)213, 213 },
          { (char)214, 214 },
          { (char)215, 215 },
          { (char)216, 216 },
          { (char)217, 217 },
          { (char)218, 218 },
          { (char)219, 219 },
          { (char)220, 220 },
          { (char)221, 221 },
          { (char)222, 222 },
          { (char)223, 223 },
          { (char)224, 224 },
          { (char)225, 225 },
          { (char)226, 226 },
          { (char)227, 227 },
          { (char)228, 228 },
          { (char)229, 229 },
          { (char)230, 230 },
          { (char)231, 231 },
          { (char)232, 232 },
          { (char)233, 233 },
          { (char)234, 234 },
          { (char)235, 235 },
          { (char)236, 236 },
          { (char)237, 237 },
          { (char)238, 238 },
          { (char)239, 239 },
          { (char)240, 240 },
          { (char)241, 241 },
          { (char)242, 242 },
          { (char)243, 243 },
          { (char)244, 244 },
          { (char)245, 245 },
          { (char)246, 246 },
          { (char)247, 247 },
          { (char)248, 248 },
          { (char)249, 249 },
          { (char)250, 250 },
          { (char)251, 251 },
          { (char)252, 252 },
          { (char)253, 253 },
          { (char)254, 254 },
          { (char)255, 255 }
        };

            #endregion
        }
}
