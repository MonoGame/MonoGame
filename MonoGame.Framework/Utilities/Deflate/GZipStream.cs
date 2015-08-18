// GZipStream.cs
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
// Time-stamp: <2011-August-08 18:14:39>
//
// ------------------------------------------------------------------
//
// This module defines the GZipStream class, which can be used as a replacement for
// the System.IO.Compression.GZipStream class in the .NET BCL.  NB: The design is not
// completely OO clean: there is some intelligence in the ZlibBaseStream that reads the
// GZip header.
//
// ------------------------------------------------------------------


using System;
using System.IO;

namespace MonoGame.Utilities.Deflate
{
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
    ///   <c>MonoGame.Utilities.Deflate.GZipStream</c> can compress while writing, or decompress while
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
    /// <para>
    ///   This class is similar to <see cref="ZlibStream"/> and <see cref="DeflateStream"/>.
    ///   <c>ZlibStream</c> handles RFC1950-compliant streams.  <see cref="DeflateStream"/>
    ///   handles RFC1951-compliant streams. This class handles RFC1952-compliant streams.
    /// </para>
    ///
    /// </remarks>
    ///
    /// <seealso cref="DeflateStream" />
    /// <seealso cref="ZlibStream" />
    public class GZipStream : System.IO.Stream
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
        public String Comment
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
        public String FileName
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
        public DateTime? LastModified;

        /// <summary>
        /// The CRC on the GZIP stream.
        /// </summary>
        /// <remarks>
        /// This is used for internal error checking. You probably don't need to look at this property.
        /// </remarks>
        public int Crc32 { get { return _Crc32; } }

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
        ///         using (Stream decompressor= new MonoGame.Utilities.Deflate.GZipStream(input, CompressionMode.Decompress, true))
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
        ///         Using decompressor As Stream = new MonoGame.Utilities.Deflate.GZipStream(input, CompressionMode.Decompress, True)
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
        public GZipStream(Stream stream, CompressionMode mode)
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
        public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level)
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
        public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen)
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
        public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            _baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.GZIP, leaveOpen);
        }

        #region Zlib properties

        /// <summary>
        /// This property sets the flush behavior on the stream.
        /// </summary>
        virtual public FlushType FlushMode
        {
            get { return (this._baseStream._flushMode); }
            set {
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
        public int BufferSize
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
                    {
                        this._baseStream.Close();
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
                if (this._baseStream._streamMode == MonoGame.Utilities.Deflate.ZlibBaseStream.StreamMode.Writer)
                    return this._baseStream._z.TotalBytesOut + _headerByteCount;
                if (this._baseStream._streamMode == MonoGame.Utilities.Deflate.ZlibBaseStream.StreamMode.Reader)
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
        ///     using (Stream decompressor= new MonoGame.Utilities.Deflate.GZipStream(input, CompressionMode.Decompress, true))
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
            if (_baseStream._streamMode == MonoGame.Utilities.Deflate.ZlibBaseStream.StreamMode.Undefined)
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
#if SILVERLIGHT || NETCF
        internal static readonly System.Text.Encoding iso8859dash1 = new Ionic.Encoding.Iso8859Dash1Encoding();
#else
        internal static readonly System.Text.Encoding iso8859dash1 = System.Text.Encoding.GetEncoding("iso-8859-1");
#endif


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
        public static byte[] CompressString(String s)
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
        public static byte[] CompressBuffer(byte[] b)
        {
            using (var ms = new MemoryStream())
            {
                System.IO.Stream compressor =
                    new GZipStream( ms, CompressionMode.Compress, CompressionLevel.BestCompression );

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
        public static String UncompressString(byte[] compressed)
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
        public static byte[] UncompressBuffer(byte[] compressed)
        {
            using (var input = new System.IO.MemoryStream(compressed))
            {
                System.IO.Stream decompressor =
                    new GZipStream( input, CompressionMode.Decompress );

                return ZlibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }


    }
}
