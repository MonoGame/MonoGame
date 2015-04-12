// Inflate.cs
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
// Time-stamp: <2010-January-08 18:32:12>
//
// ------------------------------------------------------------------
//
// This module defines classes for decompression. This code is derived
// from the jzlib implementation of zlib, but significantly modified.
// The object model is not the same, and many of the behaviors are
// different.  Nonetheless, in keeping with the license for jzlib, I am
// reproducing the copyright to that code here.
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
    sealed class InflateBlocks
    {
        private const int MANY = 1440;

        // Table for deflate from PKZIP's appnote.txt.
        internal static readonly int[] border = new int[]
        { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

        private enum InflateBlockMode
        {
            TYPE   = 0,                     // get type bits (3, including end bit)
            LENS   = 1,                     // get lengths for stored
            STORED = 2,                     // processing stored block
            TABLE  = 3,                     // get table lengths
            BTREE  = 4,                     // get bit lengths tree for a dynamic block
            DTREE  = 5,                     // get length, distance trees for a dynamic block
            CODES  = 6,                     // processing fixed or dynamic block
            DRY    = 7,                     // output remaining window bytes
            DONE   = 8,                     // finished last block, done
            BAD    = 9,                     // ot a data error--stuck here
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

                        if ( ( ((~b)>>16) & 0xffff) != (b & 0xffff))
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

                                c = (c == 16) ? blens[i-1] : 0;
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

            for (int pass=0; pass < 2; pass++)
            {
                if (pass==0)
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
        private const int START   = 0; // x: set up for LEN
        private const int LEN     = 1; // i: get length/literal/eob next
        private const int LENEXT  = 2; // i: getting length extra (have base)
        private const int DIST    = 3; // i: get distance next
        private const int DISTEXT = 4; // i: getting distance extra
        private const int COPY    = 5; // o: copying bytes in window, waiting for space
        private const int LIT     = 6; // o: got literal, waiting for output space
        private const int WASH    = 7; // o: got eob, possibly still output waiting
        private const int END     = 8; // x: got eob and all data flushed
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
            FLAG   = 1,  // waiting for flag byte
            DICT4  = 2,  // four dictionary check bytes to go
            DICT3  = 3,  // three dictionary check bytes to go
            DICT2  = 4,  // two dictionary check bytes to go
            DICT1  = 5,  // one dictionary check byte to go
            DICT0  = 6,  // waiting for inflateSetDictionary
            BLOCKS = 7,  // decompressing blocks
            CHECK4 = 8,  // four check bytes to go
            CHECK3 = 9,  // three check bytes to go
            CHECK2 = 10, // two check bytes to go
            CHECK1 = 11, // one check byte to go
            DONE   = 12, // finished check, done
            BAD    = 13, // got an error--stay here
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

        public InflateManager() { }

        public InflateManager(bool expectRfc1950HeaderBytes)
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
}