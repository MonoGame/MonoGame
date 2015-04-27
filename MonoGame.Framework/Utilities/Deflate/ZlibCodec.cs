// ZlibCodec.cs
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
// Time-stamp: <2009-November-03 15:40:51>
//
// ------------------------------------------------------------------
//
// This module defines a Codec for ZLIB compression and
// decompression. This code extends code that was based the jzlib
// implementation of zlib, but this code is completely novel.  The codec
// class is new, and encapsulates some behaviors that are new, and some
// that were present in other classes in the jzlib code base.  In
// keeping with the license for jzlib, the copyright to the jzlib code
// is included below.
//
// ------------------------------------------------------------------
// 
// Copyright (c) 2000,2001,2002,2003 ymnk, JCraft,Inc. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright 
// notice, this list of conditions and the following disclaimer in 
// the documentation and/or other materials provided with the distribution.
// 
// 3. The names of the authors may not be used to endorse or promote products
// derived from this software without specific prior written permission.
// 
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
// 
// -----------------------------------------------------------------------
//
// This program is based on zlib-1.1.3; credit to authors
// Jean-loup Gailly(jloup@gzip.org) and Mark Adler(madler@alumni.caltech.edu)
// and contributors of zlib.
//
// -----------------------------------------------------------------------


using System;

namespace MonoGame.Utilities.Deflate
{
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

    sealed public class ZlibCodec
    {
        /// <summary>
        /// The buffer from which data is taken.
        /// </summary>
        public byte[] InputBuffer;

        /// <summary>
        /// An index into the InputBuffer array, indicating where to start reading. 
        /// </summary>
        public int NextIn;

        /// <summary>
        /// The number of bytes available in the InputBuffer, starting at NextIn. 
        /// </summary>
        /// <remarks>
        /// Generally you should set this to InputBuffer.Length before the first Inflate() or Deflate() call. 
        /// The class will update this number as calls to Inflate/Deflate are made.
        /// </remarks>
        public int AvailableBytesIn;

        /// <summary>
        /// Total number of bytes read so far, through all calls to Inflate()/Deflate().
        /// </summary>
        public long TotalBytesIn;

        /// <summary>
        /// Buffer to store output data.
        /// </summary>
        public byte[] OutputBuffer;

        /// <summary>
        /// An index into the OutputBuffer array, indicating where to start writing. 
        /// </summary>
        public int NextOut;

        /// <summary>
        /// The number of bytes available in the OutputBuffer, starting at NextOut. 
        /// </summary>
        /// <remarks>
        /// Generally you should set this to OutputBuffer.Length before the first Inflate() or Deflate() call. 
        /// The class will update this number as calls to Inflate/Deflate are made.
        /// </remarks>
        public int AvailableBytesOut;

        /// <summary>
        /// Total number of bytes written to the output so far, through all calls to Inflate()/Deflate().
        /// </summary>
        public long TotalBytesOut;

        /// <summary>
        /// used for diagnostics, when something goes wrong!
        /// </summary>
        public System.String Message;

        internal DeflateManager dstate;
        internal InflateManager istate;

        internal uint _Adler32;

        /// <summary>
        /// The compression level to use in this codec.  Useful only in compression mode.
        /// </summary>
        public CompressionLevel CompressLevel = CompressionLevel.Default;

        /// <summary>
        /// The number of Window Bits to use.  
        /// </summary>
        /// <remarks>
        /// This gauges the size of the sliding window, and hence the 
        /// compression effectiveness as well as memory consumption. It's best to just leave this 
        /// setting alone if you don't know what it is.  The maximum value is 15 bits, which implies
        /// a 32k window.  
        /// </remarks>
        public int WindowBits = ZlibConstants.WindowBitsDefault;

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
        public CompressionStrategy Strategy = CompressionStrategy.Default;


        /// <summary>
        /// The Adler32 checksum on the data transferred through the codec so far. You probably don't need to look at this.
        /// </summary>
        public int Adler32 { get { return (int)_Adler32; } }


        /// <summary>
        /// Create a ZlibCodec.
        /// </summary>
        /// <remarks>
        /// If you use this default constructor, you will later have to explicitly call 
        /// InitializeInflate() or InitializeDeflate() before using the ZlibCodec to compress 
        /// or decompress. 
        /// </remarks>
        public ZlibCodec() { }

        /// <summary>
        /// Create a ZlibCodec that either compresses or decompresses.
        /// </summary>
        /// <param name="mode">
        /// Indicates whether the codec should compress (deflate) or decompress (inflate).
        /// </param>
        public ZlibCodec(CompressionMode mode)
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
        public int InitializeInflate()
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
        public int InitializeInflate(bool expectRfc1950Header)
        {
            return InitializeInflate(this.WindowBits, expectRfc1950Header);
        }

        /// <summary>
        /// Initialize the ZlibCodec for inflation, with the specified number of window bits. 
        /// </summary>
        /// <param name="windowBits">The number of window bits to use. If you need to ask what that is, 
        /// then you shouldn't be calling this initializer.</param>
        /// <returns>Z_OK if all goes well.</returns>
        public int InitializeInflate(int windowBits)
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
        public int InitializeInflate(int windowBits, bool expectRfc1950Header)
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
        public int Inflate(FlushType flush)
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
        public int EndInflate()
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
        public int SyncInflate()
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
        public int InitializeDeflate()
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
        public int InitializeDeflate(CompressionLevel level)
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
        public int InitializeDeflate(CompressionLevel level, bool wantRfc1950Header)
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
        public int InitializeDeflate(CompressionLevel level, int bits)
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
        public int InitializeDeflate(CompressionLevel level, int bits, bool wantRfc1950Header)
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
        public int Deflate(FlushType flush)
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
        public int EndDeflate()
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
        public void ResetDeflate()
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
        public int SetDeflateParams(CompressionLevel level, CompressionStrategy strategy)
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
        public int SetDictionary(byte[] dictionary)
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

            NextOut             += len;
            dstate.nextPending  += len;
            TotalBytesOut       += len;
            AvailableBytesOut   -= len;
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
}