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

using System;
using System.IO;

namespace MonoGame.Utilities.Deflate
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
    /// This class is similar to <see cref="DeflateStream"/>, except that it adds the
    /// RFC1950 header and trailer bytes to a compressed stream when compressing, or expects
    /// the RFC1950 header and trailer bytes when decompressing.  It is also similar to the
    /// <see cref="GZipStream"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="DeflateStream" />
    /// <seealso cref="GZipStream" />
    public class ZlibStream : System.IO.Stream
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
                        this._baseStream.Close();
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
                if (this._baseStream._streamMode == MonoGame.Utilities.Deflate.ZlibBaseStream.StreamMode.Writer)
                    return this._baseStream._z.TotalBytesOut;
                if (this._baseStream._streamMode == MonoGame.Utilities.Deflate.ZlibBaseStream.StreamMode.Reader)
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
                    new ZlibStream( ms, CompressionMode.Compress, CompressionLevel.BestCompression );

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
                    new ZlibStream( input, CompressionMode.Decompress );

                return ZlibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }

    }


}