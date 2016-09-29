// DeflateStream.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2010 Dino Chiesa.
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
// Time-stamp: <2011-July-31 14:48:11>
//
// ------------------------------------------------------------------
//
// This module defines the DeflateStream class, which can be used as a replacement for
// the System.IO.Compression.DeflateStream class in the .NET BCL.
//
// ------------------------------------------------------------------


using System;

namespace MonoGame.Utilities.Deflate
{
    /// <summary>
    /// A class for compressing and decompressing streams using the Deflate algorithm.
    /// </summary>
    ///
    /// <remarks>
    ///
    /// <para>
    ///   The DeflateStream is a <see
    ///   href="http://en.wikipedia.org/wiki/Decorator_pattern">Decorator</see> on a <see
    ///   cref="System.IO.Stream"/>.  It adds DEFLATE compression or decompression to any
    ///   stream.
    /// </para>
    ///
    /// <para>
    ///   Using this stream, applications can compress or decompress data via stream
    ///   <c>Read</c> and <c>Write</c> operations.  Either compresssion or decompression
    ///   can occur through either reading or writing. The compression format used is
    ///   DEFLATE, which is documented in <see
    ///   href="http://www.ietf.org/rfc/rfc1951.txt">IETF RFC 1951</see>, "DEFLATE
    ///   Compressed Data Format Specification version 1.3.".
    /// </para>
    ///
    /// <para>
    ///   This class is similar to <see cref="ZlibStream"/>, except that
    ///   <c>ZlibStream</c> adds the <see href="http://www.ietf.org/rfc/rfc1950.txt">RFC
    ///   1950 - ZLIB</see> framing bytes to a compressed stream when compressing, or
    ///   expects the RFC1950 framing bytes when decompressing. The <c>DeflateStream</c>
    ///   does not.
    /// </para>
    ///
    /// </remarks>
    ///
    /// <seealso cref="ZlibStream" />
    /// <seealso cref="GZipStream" />
    public class DeflateStream : System.IO.Stream
    {
        internal ZlibBaseStream _baseStream;
        internal System.IO.Stream _innerStream;
        bool _disposed;

        /// <summary>
        ///   Create a DeflateStream using the specified CompressionMode.
        /// </summary>
        ///
        /// <remarks>
        ///   When mode is <c>CompressionMode.Compress</c>, the DeflateStream will use
        ///   the default compression level. The "captive" stream will be closed when
        ///   the DeflateStream is closed.
        /// </remarks>
        ///
        /// <example>
        /// This example uses a DeflateStream to compress data from a file, and writes
        /// the compressed data to another file.
        /// <code>
        /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        /// {
        ///     using (var raw = System.IO.File.Create(fileToCompress + ".deflated"))
        ///     {
        ///         using (Stream compressor = new DeflateStream(raw, CompressionMode.Compress))
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
        ///     Using raw As FileStream = File.Create(fileToCompress &amp; ".deflated")
        ///         Using compressor As Stream = New DeflateStream(raw, CompressionMode.Compress)
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
        /// <param name="stream">The stream which will be read or written.</param>
        /// <param name="mode">Indicates whether the DeflateStream will compress or decompress.</param>
        public DeflateStream(System.IO.Stream stream, CompressionMode mode)
            : this(stream, mode, CompressionLevel.Default, false)
        {
        }

        /// <summary>
        /// Create a DeflateStream using the specified CompressionMode and the specified CompressionLevel.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   When mode is <c>CompressionMode.Decompress</c>, the level parameter is
        ///   ignored.  The "captive" stream will be closed when the DeflateStream is
        ///   closed.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///
        ///   This example uses a DeflateStream to compress data from a file, and writes
        ///   the compressed data to another file.
        ///
        /// <code>
        /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        /// {
        ///     using (var raw = System.IO.File.Create(fileToCompress + ".deflated"))
        ///     {
        ///         using (Stream compressor = new DeflateStream(raw,
        ///                                                      CompressionMode.Compress,
        ///                                                      CompressionLevel.BestCompression))
        ///         {
        ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        ///             int n= -1;
        ///             while (n != 0)
        ///             {
        ///                 if (n &gt; 0)
        ///                     compressor.Write(buffer, 0, n);
        ///                 n= input.Read(buffer, 0, buffer.Length);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Using input As Stream = File.OpenRead(fileToCompress)
        ///     Using raw As FileStream = File.Create(fileToCompress &amp; ".deflated")
        ///         Using compressor As Stream = New DeflateStream(raw, CompressionMode.Compress, CompressionLevel.BestCompression)
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
        /// <param name="mode">Indicates whether the <c>DeflateStream</c> will compress or decompress.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        public DeflateStream(System.IO.Stream stream, CompressionMode mode, CompressionLevel level)
            : this(stream, mode, level, false)
        {
        }

        /// <summary>
        ///   Create a <c>DeflateStream</c> using the specified
        ///   <c>CompressionMode</c>, and explicitly specify whether the
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
        ///   memory stream that will be re-read after compression.  Specify true for
        ///   the <paramref name="leaveOpen"/> parameter to leave the stream open.
        /// </para>
        ///
        /// <para>
        ///   The <c>DeflateStream</c> will use the default compression level.
        /// </para>
        ///
        /// <para>
        ///   See the other overloads of this constructor for example code.
        /// </para>
        /// </remarks>
        ///
        /// <param name="stream">
        ///   The stream which will be read or written. This is called the
        ///   "captive" stream in other places in this documentation.
        /// </param>
        ///
        /// <param name="mode">
        ///   Indicates whether the <c>DeflateStream</c> will compress or decompress.
        /// </param>
        ///
        /// <param name="leaveOpen">true if the application would like the stream to
        /// remain open after inflation/deflation.</param>
        public DeflateStream(System.IO.Stream stream, CompressionMode mode, bool leaveOpen)
            : this(stream, mode, CompressionLevel.Default, leaveOpen)
        {
        }

        /// <summary>
        ///   Create a <c>DeflateStream</c> using the specified <c>CompressionMode</c>
        ///   and the specified <c>CompressionLevel</c>, and explicitly specify whether
        ///   the stream should be left open after Deflation or Inflation.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   When mode is <c>CompressionMode.Decompress</c>, the level parameter is ignored.
        /// </para>
        ///
        /// <para>
        ///   This constructor allows the application to request that the captive stream
        ///   remain open after the deflation or inflation occurs.  By default, after
        ///   <c>Close()</c> is called on the stream, the captive stream is also
        ///   closed. In some cases this is not desired, for example if the stream is a
        ///   <see cref="System.IO.MemoryStream"/> that will be re-read after
        ///   compression.  Specify true for the <paramref name="leaveOpen"/> parameter
        ///   to leave the stream open.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///
        ///   This example shows how to use a <c>DeflateStream</c> to compress data from
        ///   a file, and store the compressed data into another file.
        ///
        /// <code>
        /// using (var output = System.IO.File.Create(fileToCompress + ".deflated"))
        /// {
        ///     using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        ///     {
        ///         using (Stream compressor = new DeflateStream(output, CompressionMode.Compress, CompressionLevel.BestCompression, true))
        ///         {
        ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        ///             int n= -1;
        ///             while (n != 0)
        ///             {
        ///                 if (n &gt; 0)
        ///                     compressor.Write(buffer, 0, n);
        ///                 n= input.Read(buffer, 0, buffer.Length);
        ///             }
        ///         }
        ///     }
        ///     // can write additional data to the output stream here
        /// }
        /// </code>
        ///
        /// <code lang="VB">
        /// Using output As FileStream = File.Create(fileToCompress &amp; ".deflated")
        ///     Using input As Stream = File.OpenRead(fileToCompress)
        ///         Using compressor As Stream = New DeflateStream(output, CompressionMode.Compress, CompressionLevel.BestCompression, True)
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
        /// <param name="stream">The stream which will be read or written.</param>
        /// <param name="mode">Indicates whether the DeflateStream will compress or decompress.</param>
        /// <param name="leaveOpen">true if the application would like the stream to remain open after inflation/deflation.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        public DeflateStream(System.IO.Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            _innerStream = stream;
            _baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.DEFLATE, leaveOpen);
        }

        #region Zlib properties

        /// <summary>
        /// This property sets the flush behavior on the stream.
        /// </summary>
        /// <remarks> See the ZLIB documentation for the meaning of the flush behavior.
        /// </remarks>
        virtual public FlushType FlushMode
        {
            get { return (this._baseStream._flushMode); }
            set
            {
                if (_disposed) throw new ObjectDisposedException("DeflateStream");
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
        public int BufferSize
        {
            get
            {
                return this._baseStream._bufferSize;
            }
            set
            {
                if (_disposed) throw new ObjectDisposedException("DeflateStream");
                if (this._baseStream._workingBuffer != null)
                    throw new ZlibException("The working buffer is already set.");
                if (value < ZlibConstants.WorkingBufferSizeMin)
                    throw new ZlibException(String.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, ZlibConstants.WorkingBufferSizeMin));
                this._baseStream._bufferSize = value;
            }
        }

        /// <summary>
        ///   The ZLIB strategy to be used during compression.
        /// </summary>
        ///
        /// <remarks>
        ///   By tweaking this parameter, you may be able to optimize the compression for
        ///   data with particular characteristics.
        /// </remarks>
        public CompressionStrategy Strategy
        {
            get
            {
                return this._baseStream.Strategy;
            }
            set
            {
            if (_disposed) throw new ObjectDisposedException("DeflateStream");
                this._baseStream.Strategy = value;
            }
        }

        /// <summary> Returns the total number of bytes input so far.</summary>
        virtual public long TotalIn
        {
            get
            {
                return this._baseStream._z.TotalBytesIn;
            }
        }

        /// <summary> Returns the total number of bytes output so far.</summary>
        virtual public long TotalOut
        {
            get
            {
                return this._baseStream._z.TotalBytesOut;
            }
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
        ///     Application code won't call this code directly.  This method may be
        ///     invoked in two distinct scenarios.  If disposing == true, the method
        ///     has been called directly or indirectly by a user's code, for example
        ///     via the public Dispose() method. In this case, both managed and
        ///     unmanaged resources can be referenced and disposed.  If disposing ==
        ///     false, the method has been called by the runtime from inside the
        ///     object finalizer and this method should not reference other objects;
        ///     in that case only unmanaged resources must be referenced or
        ///     disposed.
        ///   </para>
        /// </remarks>
        /// <param name="disposing">
        ///   true if the Dispose method was invoked by user code.
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
                if (_disposed) throw new ObjectDisposedException("DeflateStream");
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
                if (_disposed) throw new ObjectDisposedException("DeflateStream");
                return _baseStream._stream.CanWrite;
            }
        }

        /// <summary>
        /// Flush the stream.
        /// </summary>
        public override void Flush()
        {
            if (_disposed) throw new ObjectDisposedException("DeflateStream");
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
        /// The position of the stream pointer.
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
                if (this._baseStream._streamMode == MonoGame.Utilities.Deflate.ZlibBaseStream.StreamMode.Writer)
                    return this._baseStream._z.TotalBytesOut;
                if (this._baseStream._streamMode == MonoGame.Utilities.Deflate.ZlibBaseStream.StreamMode.Reader)
                    return this._baseStream._z.TotalBytesIn;
                return 0;
            }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Read data from the stream.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   If you wish to use the <c>DeflateStream</c> to compress data while
        ///   reading, you can create a <c>DeflateStream</c> with
        ///   <c>CompressionMode.Compress</c>, providing an uncompressed data stream.
        ///   Then call Read() on that <c>DeflateStream</c>, and the data read will be
        ///   compressed as you read.  If you wish to use the <c>DeflateStream</c> to
        ///   decompress data while reading, you can create a <c>DeflateStream</c> with
        ///   <c>CompressionMode.Decompress</c>, providing a readable compressed data
        ///   stream.  Then call Read() on that <c>DeflateStream</c>, and the data read
        ///   will be decompressed as you read.
        /// </para>
        ///
        /// <para>
        ///   A <c>DeflateStream</c> can be used for <c>Read()</c> or <c>Write()</c>, but not both.
        /// </para>
        ///
        /// </remarks>
        /// <param name="buffer">The buffer into which the read data should be placed.</param>
        /// <param name="offset">the offset within that data array to put the first byte read.</param>
        /// <param name="count">the number of bytes to read.</param>
        /// <returns>the number of bytes actually read</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException("DeflateStream");
            return _baseStream.Read(buffer, offset, count);
        }


        /// <summary>
        /// Calling this method always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="offset">this is irrelevant, since it will always throw!</param>
        /// <param name="origin">this is irrelevant, since it will always throw!</param>
        /// <returns>irrelevant!</returns>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calling this method always throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="value">this is irrelevant, since it will always throw!</param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Write data to the stream.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   If you wish to use the <c>DeflateStream</c> to compress data while
        ///   writing, you can create a <c>DeflateStream</c> with
        ///   <c>CompressionMode.Compress</c>, and a writable output stream.  Then call
        ///   <c>Write()</c> on that <c>DeflateStream</c>, providing uncompressed data
        ///   as input.  The data sent to the output stream will be the compressed form
        ///   of the data written.  If you wish to use the <c>DeflateStream</c> to
        ///   decompress data while writing, you can create a <c>DeflateStream</c> with
        ///   <c>CompressionMode.Decompress</c>, and a writable output stream.  Then
        ///   call <c>Write()</c> on that stream, providing previously compressed
        ///   data. The data sent to the output stream will be the decompressed form of
        ///   the data written.
        /// </para>
        ///
        /// <para>
        ///   A <c>DeflateStream</c> can be used for <c>Read()</c> or <c>Write()</c>,
        ///   but not both.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="buffer">The buffer holding data to write to the stream.</param>
        /// <param name="offset">the offset within that data array to find the first byte to write.</param>
        /// <param name="count">the number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException("DeflateStream");
            _baseStream.Write(buffer, offset, count);
        }
        #endregion




        /// <summary>
        ///   Compress a string into a byte array using DEFLATE (RFC 1951).
        /// </summary>
        ///
        /// <remarks>
        ///   Uncompress it with <see cref="DeflateStream.UncompressString(byte[])"/>.
        /// </remarks>
        ///
        /// <seealso cref="DeflateStream.UncompressString(byte[])">DeflateStream.UncompressString(byte[])</seealso>
        /// <seealso cref="DeflateStream.CompressBuffer(byte[])">DeflateStream.CompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.CompressString(string)">GZipStream.CompressString(string)</seealso>
        /// <seealso cref="ZlibStream.CompressString(string)">ZlibStream.CompressString(string)</seealso>
        ///
        /// <param name="s">
        ///   A string to compress. The string will first be encoded
        ///   using UTF8, then compressed.
        /// </param>
        ///
        /// <returns>The string in compressed form</returns>
        public static byte[] CompressString(String s)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                System.IO.Stream compressor =
                    new DeflateStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZlibBaseStream.CompressString(s, compressor);
                return ms.ToArray();
            }
        }


        /// <summary>
        ///   Compress a byte array into a new byte array using DEFLATE.
        /// </summary>
        ///
        /// <remarks>
        ///   Uncompress it with <see cref="DeflateStream.UncompressBuffer(byte[])"/>.
        /// </remarks>
        ///
        /// <seealso cref="DeflateStream.CompressString(string)">DeflateStream.CompressString(string)</seealso>
        /// <seealso cref="DeflateStream.UncompressBuffer(byte[])">DeflateStream.UncompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.CompressBuffer(byte[])">GZipStream.CompressBuffer(byte[])</seealso>
        /// <seealso cref="ZlibStream.CompressBuffer(byte[])">ZlibStream.CompressBuffer(byte[])</seealso>
        ///
        /// <param name="b">
        ///   A buffer to compress.
        /// </param>
        ///
        /// <returns>The data in compressed form</returns>
        public static byte[] CompressBuffer(byte[] b)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                System.IO.Stream compressor =
                    new DeflateStream( ms, CompressionMode.Compress, CompressionLevel.BestCompression );

                ZlibBaseStream.CompressBuffer(b, compressor);
                return ms.ToArray();
            }
        }


        /// <summary>
        ///   Uncompress a DEFLATE'd byte array into a single string.
        /// </summary>
        ///
        /// <seealso cref="DeflateStream.CompressString(String)">DeflateStream.CompressString(String)</seealso>
        /// <seealso cref="DeflateStream.UncompressBuffer(byte[])">DeflateStream.UncompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.UncompressString(byte[])">GZipStream.UncompressString(byte[])</seealso>
        /// <seealso cref="ZlibStream.UncompressString(byte[])">ZlibStream.UncompressString(byte[])</seealso>
        ///
        /// <param name="compressed">
        ///   A buffer containing DEFLATE-compressed data.
        /// </param>
        ///
        /// <returns>The uncompressed string</returns>
        public static String UncompressString(byte[] compressed)
        {
            using (var input = new System.IO.MemoryStream(compressed))
            {
                System.IO.Stream decompressor =
                    new DeflateStream(input, CompressionMode.Decompress);

                return ZlibBaseStream.UncompressString(compressed, decompressor);
            }
        }


        /// <summary>
        ///   Uncompress a DEFLATE'd byte array into a byte array.
        /// </summary>
        ///
        /// <seealso cref="DeflateStream.CompressBuffer(byte[])">DeflateStream.CompressBuffer(byte[])</seealso>
        /// <seealso cref="DeflateStream.UncompressString(byte[])">DeflateStream.UncompressString(byte[])</seealso>
        /// <seealso cref="GZipStream.UncompressBuffer(byte[])">GZipStream.UncompressBuffer(byte[])</seealso>
        /// <seealso cref="ZlibStream.UncompressBuffer(byte[])">ZlibStream.UncompressBuffer(byte[])</seealso>
        ///
        /// <param name="compressed">
        ///   A buffer containing data that has been compressed with DEFLATE.
        /// </param>
        ///
        /// <returns>The data in uncompressed form</returns>
        public static byte[] UncompressBuffer(byte[] compressed)
        {
            using (var input = new System.IO.MemoryStream(compressed))
            {
                System.IO.Stream decompressor =
                    new DeflateStream( input, CompressionMode.Decompress );

                return ZlibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }

    }

}

