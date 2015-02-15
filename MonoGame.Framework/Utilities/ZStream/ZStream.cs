// Copyright (c) 2006, ComponentAce
// http://www.componentace.com
// All rights reserved.

// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
// Neither the name of ComponentAce nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

/*
Copyright (c) 2000,2001,2002,2003 ymnk, JCraft,Inc. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright 
notice, this list of conditions and the following disclaimer in 
the documentation and/or other materials provided with the distribution.

3. The names of the authors may not be used to endorse or promote products
derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
/*
* This program is based on zlib-1.1.3, so all credit should go authors
* Jean-loup Gailly(jloup@gzip.org) and Mark Adler(madler@alumni.caltech.edu)
* and contributors of zlib.
*/
using System;
using System.IO;

namespace MonoGame.Utilities.ZLib
{

    public static class ZStreamUtilities
    {
        public static void CompressStream(Stream decompressedStream, Stream compressedStream)
        {
            using (var outputZStream = new ZOutputStream(compressedStream, zlibConst.Z_DEFAULT_COMPRESSION))
            {
                try
                {
                    CopyStream(decompressedStream, outputZStream);
                }
                finally
                {
                    decompressedStream.Close();
                }
            }
        }
        
        public static void DecompressStream(Stream compressedStream, Stream decompressedStream)
        {
            using (var outputZStream = new ZOutputStream(decompressedStream))
            {
                try
                {
                    CopyStream(compressedStream, outputZStream);
                }
                finally
                {
                    compressedStream.Close();
                }
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[2048];
            int length;

            while ((length = input.Read(buffer, 0, 2048)) > 0)
            {
                output.Write(buffer, 0, length);
            }

            output.Flush();
        }
    }
    
    sealed public class ZStream
    {

        private const int MAX_WBITS = 15; // 32K LZ77 window		
        private static readonly int DEF_WBITS = MAX_WBITS;

        private const int Z_NO_FLUSH = 0;
        private const int Z_PARTIAL_FLUSH = 1;
        private const int Z_SYNC_FLUSH = 2;
        private const int Z_FULL_FLUSH = 3;
        private const int Z_FINISH = 4;

        private const int MAX_MEM_LEVEL = 9;

        private const int Z_OK = 0;
        private const int Z_STREAM_END = 1;
        private const int Z_NEED_DICT = 2;
        private const int Z_ERRNO = -1;
        private const int Z_STREAM_ERROR = -2;
        private const int Z_DATA_ERROR = -3;
        private const int Z_MEM_ERROR = -4;
        private const int Z_BUF_ERROR = -5;
        private const int Z_VERSION_ERROR = -6;

        public byte[] next_in; // next input byte
        public int next_in_index;
        public int avail_in; // number of bytes available at next_in
        public long total_in; // total nb of input bytes read so far

        public byte[] next_out; // next output byte should be put there
        public int next_out_index;
        public int avail_out; // remaining free space at next_out
        public long total_out; // total nb of bytes output so far

        public string msg;

        internal Deflate dstate;
        internal Inflate istate;

        internal int data_type; // best guess about the data type: ascii or binary

        public long adler;
        internal Adler32 _adler = new Adler32();

        public int inflateInit()
        {
            return inflateInit(DEF_WBITS);
        }
        public int inflateInit(int w)
        {
            istate = new Inflate();
            return istate.inflateInit(this, w);
        }

        public int inflate(int f)
        {
            if (istate == null)
                return Z_STREAM_ERROR;
            return istate.inflate(this, f);
        }
        public int inflateEnd()
        {
            if (istate == null)
                return Z_STREAM_ERROR;
            int ret = istate.inflateEnd(this);
            istate = null;
            return ret;
        }
        public int inflateSync()
        {
            if (istate == null)
                return Z_STREAM_ERROR;
            return istate.inflateSync(this);
        }
        public int inflateSetDictionary(byte[] dictionary, int dictLength)
        {
            if (istate == null)
                return Z_STREAM_ERROR;
            return istate.inflateSetDictionary(this, dictionary, dictLength);
        }

        public int deflateInit(int level)
        {
            return deflateInit(level, MAX_WBITS);
        }
        public int deflateInit(int level, int bits)
        {
            dstate = new Deflate();
            return dstate.deflateInit(this, level, bits);
        }
        public int deflate(int flush)
        {
            if (dstate == null)
            {
                return Z_STREAM_ERROR;
            }
            return dstate.deflate(this, flush);
        }
        public int deflateEnd()
        {
            if (dstate == null)
                return Z_STREAM_ERROR;
            int ret = dstate.deflateEnd();
            dstate = null;
            return ret;
        }
        public int deflateParams(int level, int strategy)
        {
            if (dstate == null)
                return Z_STREAM_ERROR;
            return dstate.deflateParams(this, level, strategy);
        }
        public int deflateSetDictionary(byte[] dictionary, int dictLength)
        {
            if (dstate == null)
                return Z_STREAM_ERROR;
            return dstate.deflateSetDictionary(this, dictionary, dictLength);
        }

        // Flush as much pending output as possible. All deflate() output goes
        // through this function so some applications may wish to modify it
        // to avoid allocating a large strm->next_out buffer and copying into it.
        // (See also read_buf()).
        internal void flush_pending()
        {
            int len = dstate.pending;

            if (len > avail_out)
                len = avail_out;
            if (len == 0)
                return;

            if (dstate.pending_buf.Length <= dstate.pending_out || next_out.Length <= next_out_index || dstate.pending_buf.Length < (dstate.pending_out + len) || next_out.Length < (next_out_index + len))
            {
                //System.Console.Out.WriteLine(dstate.pending_buf.Length + ", " + dstate.pending_out + ", " + next_out.Length + ", " + next_out_index + ", " + len);
                //System.Console.Out.WriteLine("avail_out=" + avail_out);
            }

            Array.Copy(dstate.pending_buf, dstate.pending_out, next_out, next_out_index, len);

            next_out_index += len;
            dstate.pending_out += len;
            total_out += len;
            avail_out -= len;
            dstate.pending -= len;
            if (dstate.pending == 0)
            {
                dstate.pending_out = 0;
            }
        }

        // Read a new buffer from the current input stream, update the adler32
        // and total number of bytes read.  All deflate() input goes through
        // this function so some applications may wish to modify it to avoid
        // allocating a large strm->next_in buffer and copying from it.
        // (See also flush_pending()).
        internal int read_buf(byte[] buf, int start, int size)
        {
            int len = avail_in;

            if (len > size)
                len = size;
            if (len == 0)
                return 0;

            avail_in -= len;

            if (dstate.noheader == 0)
            {
                adler = _adler.adler32(adler, next_in, next_in_index, len);
            }
            Array.Copy(next_in, next_in_index, buf, start, len);
            next_in_index += len;
            total_in += len;
            return len;
        }

        public void free()
        {
            next_in = null;
            next_out = null;
            msg = null;
            _adler = null;
        }
    }

    sealed class Adler32
    {

        // largest prime smaller than 65536
        private const int BASE = 65521;
        // NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
        private const int NMAX = 5552;

        internal long adler32(long adler, byte[] buf, int index, int len)
        {
            if (buf == null)
            {
                return 1L;
            }

            long s1 = adler & 0xffff;
            long s2 = (adler >> 16) & 0xffff;
            int k;

            while (len > 0)
            {
                k = len < NMAX ? len : NMAX;
                len -= k;
                while (k >= 16)
                {
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += (buf[index++] & 0xff); s2 += s1;
                    k -= 16;
                }
                if (k != 0)
                {
                    do
                    {
                        s1 += (buf[index++] & 0xff); s2 += s1;
                    }
                    while (--k != 0);
                }
                s1 %= BASE;
                s2 %= BASE;
            }
            return (s2 << 16) | s1;
        }

    }

    public sealed class Deflate
    {

        private const int MAX_MEM_LEVEL = 9;

        private const int Z_DEFAULT_COMPRESSION = -1;

        private const int MAX_WBITS = 15; // 32K LZ77 window
        private const int DEF_MEM_LEVEL = 8;

        internal class Config
        {
            internal int good_length; // reduce lazy search above this match length
            internal int max_lazy; // do not perform lazy search above this match length
            internal int nice_length; // quit search above this match length
            internal int max_chain;
            internal int func;
            internal Config(int good_length, int max_lazy, int nice_length, int max_chain, int func)
            {
                this.good_length = good_length;
                this.max_lazy = max_lazy;
                this.nice_length = nice_length;
                this.max_chain = max_chain;
                this.func = func;
            }
        }

        private const int STORED = 0;
        private const int FAST = 1;
        private const int SLOW = 2;
        private static Config[] config_table;

        private static readonly string[] z_errmsg = new string[] { "need dictionary", "stream end", "", "file error", "stream error", "data error", "insufficient memory", "buffer error", "incompatible version", "" };

        // block not completed, need more input or more output
        private const int NeedMore = 0;

        // block flush performed
        private const int BlockDone = 1;

        // finish started, need only more output at next deflate
        private const int FinishStarted = 2;

        // finish done, accept no more input or output
        private const int FinishDone = 3;

        // preset dictionary flag in zlib header
        private const int PRESET_DICT = 0x20;

        private const int Z_FILTERED = 1;
        private const int Z_HUFFMAN_ONLY = 2;
        private const int Z_DEFAULT_STRATEGY = 0;

        private const int Z_NO_FLUSH = 0;
        private const int Z_PARTIAL_FLUSH = 1;
        private const int Z_SYNC_FLUSH = 2;
        private const int Z_FULL_FLUSH = 3;
        private const int Z_FINISH = 4;

        private const int Z_OK = 0;
        private const int Z_STREAM_END = 1;
        private const int Z_NEED_DICT = 2;
        private const int Z_ERRNO = -1;
        private const int Z_STREAM_ERROR = -2;
        private const int Z_DATA_ERROR = -3;
        private const int Z_MEM_ERROR = -4;
        private const int Z_BUF_ERROR = -5;
        private const int Z_VERSION_ERROR = -6;

        private const int INIT_STATE = 42;
        private const int BUSY_STATE = 113;
        private const int FINISH_STATE = 666;

        // The deflate compression method
        private const int Z_DEFLATED = 8;

        private const int STORED_BLOCK = 0;
        private const int STATIC_TREES = 1;
        private const int DYN_TREES = 2;

        // The three kinds of block type
        private const int Z_BINARY = 0;
        private const int Z_ASCII = 1;
        private const int Z_UNKNOWN = 2;

        private const int Buf_size = 8 * 2;

        // repeat previous bit length 3-6 times (2 bits of repeat count)
        private const int REP_3_6 = 16;

        // repeat a zero length 3-10 times  (3 bits of repeat count)
        private const int REPZ_3_10 = 17;

        // repeat a zero length 11-138 times  (7 bits of repeat count)
        private const int REPZ_11_138 = 18;

        private const int MIN_MATCH = 3;
        private const int MAX_MATCH = 258;
        private static readonly int MIN_LOOKAHEAD = (MAX_MATCH + MIN_MATCH + 1);

        private const int MAX_BITS = 15;
        private const int D_CODES = 30;
        private const int BL_CODES = 19;
        private const int LENGTH_CODES = 29;
        private const int LITERALS = 256;
        private static readonly int L_CODES = (LITERALS + 1 + LENGTH_CODES);
        private static readonly int HEAP_SIZE = (2 * L_CODES + 1);

        private const int END_BLOCK = 256;

        internal ZStream strm; // pointer back to this zlib stream
        internal int status; // as the name implies
        internal byte[] pending_buf; // output still pending
        internal int pending_buf_size; // size of pending_buf
        internal int pending_out; // next pending byte to output to the stream
        internal int pending; // nb of bytes in the pending buffer
        internal int noheader; // suppress zlib header and adler32
        internal byte data_type; // UNKNOWN, BINARY or ASCII
        internal byte method; // STORED (for zip only) or DEFLATED
        internal int last_flush; // value of flush param for previous deflate call

        internal int w_size; // LZ77 window size (32K by default)
        internal int w_bits; // log2(w_size)  (8..16)
        internal int w_mask; // w_size - 1

        internal byte[] window;
        // Sliding window. Input bytes are read into the second half of the window,
        // and move to the first half later to keep a dictionary of at least wSize
        // bytes. With this organization, matches are limited to a distance of
        // wSize-MAX_MATCH bytes, but this ensures that IO is always
        // performed with a length multiple of the block size. Also, it limits
        // the window size to 64K, which is quite useful on MSDOS.
        // To do: use the user input buffer as sliding window.

        internal int window_size;
        // Actual size of window: 2*wSize, except when the user input buffer
        // is directly used as sliding window.

        internal short[] prev;
        // Link to older string with same hash index. To limit the size of this
        // array to 64K, this link is maintained only for the last 32K strings.
        // An index in this array is thus a window index modulo 32K.

        internal short[] head; // Heads of the hash chains or NIL.

        internal int ins_h; // hash index of string to be inserted
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

        internal int match_length; // length of best match
        internal int prev_match; // previous match
        internal int match_available; // set if previous match exists
        internal int strstart; // start of string to insert
        internal int match_start; // start of matching string
        internal int lookahead; // number of valid bytes ahead in window

        // Length of the best match at previous step. Matches not greater than this
        // are discarded. This is used in the lazy match evaluation.
        internal int prev_length;

        // To speed up deflation, hash chains are never searched beyond this
        // length.  A higher limit improves compression ratio but degrades the speed.
        internal int max_chain_length;

        // Attempt to find a better match only when the current match is strictly
        // smaller than this value. This mechanism is used only for compression
        // levels >= 4.
        internal int max_lazy_match;

        // Insert new strings in the hash table only if the match length is not
        // greater than this length. This saves time but degrades compression.
        // max_insert_length is used only for compression levels <= 3.

        internal int level; // compression level (1..9)
        internal int strategy; // favor or force Huffman coding

        // Use a faster search when the previous match is longer than this
        internal int good_match;

        // Stop searching when current match exceeds this
        internal int nice_match;

        internal short[] dyn_ltree; // literal and length tree
        internal short[] dyn_dtree; // distance tree
        internal short[] bl_tree; // Huffman tree for bit lengths

        internal Tree l_desc = new Tree(); // desc for literal tree
        internal Tree d_desc = new Tree(); // desc for distance tree
        internal Tree bl_desc = new Tree(); // desc for bit length tree

        // number of codes at each bit length for an optimal tree
        internal short[] bl_count = new short[MAX_BITS + 1];

        // heap used to build the Huffman trees
        internal int[] heap = new int[2 * L_CODES + 1];

        internal int heap_len; // number of elements in the heap
        internal int heap_max; // element of largest frequency
        // The sons of heap[n] are heap[2*n] and heap[2*n+1]. heap[0] is not used.
        // The same heap array is used to build all trees.

        // Depth of each subtree used as tie breaker for trees of equal frequency
        internal byte[] depth = new byte[2 * L_CODES + 1];

        internal int l_buf; // index for literals or lengths */

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
        //   - I can't count above 4
        internal int lit_bufsize;

        internal int last_lit; // running index in l_buf

        // Buffer for distances. To simplify the code, d_buf and l_buf have
        // the same number of elements. To use different lengths, an extra flag
        // array would be necessary.

        internal int d_buf; // index of pendig_buf

        internal int opt_len; // bit length of current block with optimal trees
        internal int static_len; // bit length of current block with static trees
        internal int matches; // number of string matches in current block
        internal int last_eob_len; // bit length of EOB code for last block

        // Output buffer. bits are inserted starting at the bottom (least
        // significant bits).
        internal short bi_buf;

        // Number of valid bits in bi_buf.  All bits above the last valid bit
        // are always zero.
        internal int bi_valid;

        internal Deflate()
        {
            dyn_ltree = new short[HEAP_SIZE * 2];
            dyn_dtree = new short[(2 * D_CODES + 1) * 2]; // distance tree
            bl_tree = new short[(2 * BL_CODES + 1) * 2]; // Huffman tree for bit lengths
        }

        internal void lm_init()
        {
            window_size = 2 * w_size;

            head[hash_size - 1] = 0;
            for (int i = 0; i < hash_size - 1; i++)
            {
                head[i] = 0;
            }

            // Set the default configuration parameters:
            max_lazy_match = Deflate.config_table[level].max_lazy;
            good_match = Deflate.config_table[level].good_length;
            nice_match = Deflate.config_table[level].nice_length;
            max_chain_length = Deflate.config_table[level].max_chain;

            strstart = 0;
            block_start = 0;
            lookahead = 0;
            match_length = prev_length = MIN_MATCH - 1;
            match_available = 0;
            ins_h = 0;
        }

        // Initialize the tree data structures for a new zlib stream.
        internal void tr_init()
        {

            l_desc.dyn_tree = dyn_ltree;
            l_desc.stat_desc = StaticTree.static_l_desc;

            d_desc.dyn_tree = dyn_dtree;
            d_desc.stat_desc = StaticTree.static_d_desc;

            bl_desc.dyn_tree = bl_tree;
            bl_desc.stat_desc = StaticTree.static_bl_desc;

            bi_buf = 0;
            bi_valid = 0;
            last_eob_len = 8; // enough lookahead for inflate

            // Initialize the first block of the first file:
            init_block();
        }

        internal void init_block()
        {
            // Initialize the trees.
            for (int i = 0; i < L_CODES; i++)
                dyn_ltree[i * 2] = 0;
            for (int i = 0; i < D_CODES; i++)
                dyn_dtree[i * 2] = 0;
            for (int i = 0; i < BL_CODES; i++)
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
                if (j < heap_len && smaller(tree, heap[j + 1], heap[j], depth))
                {
                    j++;
                }
                // Exit if v is smaller than both sons
                if (smaller(tree, v, heap[j], depth))
                    break;

                // Exchange v with the smallest son
                heap[k] = heap[j]; k = j;
                // And continue down the tree, setting j to the left son of k
                j <<= 1;
            }
            heap[k] = v;
        }

        internal static bool smaller(short[] tree, int n, int m, byte[] depth)
        {
            return (tree[n * 2] < tree[m * 2] || (tree[n * 2] == tree[m * 2] && depth[n] <= depth[m]));
        }

        // Scan a literal or distance tree to determine the frequencies of the codes
        // in the bit length tree.
        internal void scan_tree(short[] tree, int max_code)
        {
            int n; // iterates over all tree elements
            int prevlen = -1; // last emitted length
            int curlen; // length of current code
            int nextlen = tree[0 * 2 + 1]; // length of next code
            int count = 0; // repeat count of the current code
            int max_count = 7; // max repeat count
            int min_count = 4; // min repeat count

            if (nextlen == 0)
            {
                max_count = 138; min_count = 3;
            }
            tree[(max_code + 1) * 2 + 1] = (short)SupportClass.Identity(0xffff); // guard

            for (n = 0; n <= max_code; n++)
            {
                curlen = nextlen; nextlen = tree[(n + 1) * 2 + 1];
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
                    bl_tree[REP_3_6 * 2]++;
                }
                else if (count <= 10)
                {
                    bl_tree[REPZ_3_10 * 2]++;
                }
                else
                {
                    bl_tree[REPZ_11_138 * 2]++;
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
            scan_tree(dyn_ltree, l_desc.max_code);
            scan_tree(dyn_dtree, d_desc.max_code);

            // Build the bit length tree:
            bl_desc.build_tree(this);
            // opt_len now includes the length of the tree representations, except
            // the lengths of the bit lengths codes and the 5+5+4 bits for the counts.

            // Determine the number of bit length codes to send. The pkzip format
            // requires that at least 4 bit length codes be sent. (appnote.txt says
            // 3 but the actual value used is 4.)
            for (max_blindex = BL_CODES - 1; max_blindex >= 3; max_blindex--)
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
            int n; // iterates over all tree elements
            int prevlen = -1; // last emitted length
            int curlen; // length of current code
            int nextlen = tree[0 * 2 + 1]; // length of next code
            int count = 0; // repeat count of the current code
            int max_count = 7; // max repeat count
            int min_count = 4; // min repeat count

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
                    send_code(REP_3_6, bl_tree);
                    send_bits(count - 3, 2);
                }
                else if (count <= 10)
                {
                    send_code(REPZ_3_10, bl_tree);
                    send_bits(count - 3, 3);
                }
                else
                {
                    send_code(REPZ_11_138, bl_tree);
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

        // Output a byte on the stream.
        // IN assertion: there is enough room in pending_buf.
        internal void put_byte(byte[] p, int start, int len)
        {
            Array.Copy(p, start, pending_buf, pending, len);
            pending += len;
        }

        internal void put_byte(byte c)
        {
            pending_buf[pending++] = c;
        }
        internal void put_short(int w)
        {
            put_byte((byte)(w));
            put_byte((byte)(SupportClass.URShift(w, 8)));
        }
        internal void putShortMSB(int b)
        {
            put_byte((byte)(b >> 8));
            put_byte((byte)(b));
        }

        internal void send_code(int c, short[] tree)
        {
            send_bits((tree[c * 2] & 0xffff), (tree[c * 2 + 1] & 0xffff));
        }

        internal void send_bits(int value_Renamed, int length)
        {
            int len = length;
            if (bi_valid > (int)Buf_size - len)
            {
                int val = value_Renamed;
                //      bi_buf |= (val << bi_valid);
                bi_buf = (short)((ushort)bi_buf | (ushort)(((val << bi_valid) & 0xffff)));
                put_short(bi_buf);
                bi_buf = (short)(SupportClass.URShift(val, (Buf_size - bi_valid)));
                bi_valid += len - Buf_size;
            }
            else
            {
                //      bi_buf |= (value) << bi_valid;
                bi_buf = (short)((ushort)bi_buf | (ushort)((((value_Renamed) << bi_valid) & 0xffff)));
                bi_valid += len;
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
            send_code(END_BLOCK, StaticTree.static_ltree);

            bi_flush();

            // Of the 10 bits for the empty block, we have already sent
            // (10 - bi_valid) bits. The lookahead for the last real code (before
            // the EOB of the previous block) was thus at least one plus the length
            // of the EOB plus what we have just sent of the empty static block.
            if (1 + last_eob_len + 10 - bi_valid < 9)
            {
                send_bits(STATIC_TREES << 1, 3);
                send_code(END_BLOCK, StaticTree.static_ltree);
                bi_flush();
            }
            last_eob_len = 7;
        }


        // Save the match info and tally the frequency counts. Return true if
        // the current block must be flushed.
        internal bool _tr_tally(int dist, int lc)
        {

            pending_buf[d_buf + last_lit * 2] = (byte)(SupportClass.URShift(dist, 8));
            pending_buf[d_buf + last_lit * 2 + 1] = (byte)dist;

            pending_buf[l_buf + last_lit] = (byte)lc; last_lit++;

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
                dyn_ltree[(Tree._length_code[lc] + LITERALS + 1) * 2]++;
                dyn_dtree[Tree.d_code(dist) * 2]++;
            }

            if ((last_lit & 0x1fff) == 0 && level > 2)
            {
                // Compute an upper bound for the compressed length
                int out_length = last_lit * 8;
                int in_length = strstart - block_start;
                int dcode;
                for (dcode = 0; dcode < D_CODES; dcode++)
                {
                    out_length = (int)(out_length + (int)dyn_dtree[dcode * 2] * (5L + Tree.extra_dbits[dcode]));
                }
                out_length = SupportClass.URShift(out_length, 3);
                if ((matches < (last_lit / 2)) && out_length < in_length / 2)
                    return true;
            }

            return (last_lit == lit_bufsize - 1);
            // We avoid equality with lit_bufsize because of wraparound at 64K
            // on 16 bit machines and because stored blocks are restricted to
            // 64K-1 bytes.
        }

        // Send the block data compressed using the given Huffman trees
        internal void compress_block(short[] ltree, short[] dtree)
        {
            int dist; // distance of matched string
            int lc; // match length or unmatched char (if dist == 0)
            int lx = 0; // running index in l_buf
            int code; // the code to send
            int extra; // number of extra bits to send

            if (last_lit != 0)
            {
                do
                {
                    dist = ((pending_buf[d_buf + lx * 2] << 8) & 0xff00) | (pending_buf[d_buf + lx * 2 + 1] & 0xff);
                    lc = (pending_buf[l_buf + lx]) & 0xff; lx++;

                    if (dist == 0)
                    {
                        send_code(lc, ltree); // send a literal byte
                    }
                    else
                    {
                        // Here, lc is the match length - MIN_MATCH
                        code = Tree._length_code[lc];

                        send_code(code + LITERALS + 1, ltree); // send the length code
                        extra = Tree.extra_lbits[code];
                        if (extra != 0)
                        {
                            lc -= Tree.base_length[code];
                            send_bits(lc, extra); // send the extra length bits
                        }
                        dist--; // dist is now the match distance - 1
                        code = Tree.d_code(dist);

                        send_code(code, dtree); // send the distance code
                        extra = Tree.extra_dbits[code];
                        if (extra != 0)
                        {
                            dist -= Tree.base_dist[code];
                            send_bits(dist, extra); // send the extra distance bits
                        }
                    } // literal or match pair ?

                    // Check that the overlay between pending_buf and d_buf+l_buf is ok:
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
            while (n < LITERALS)
            {
                bin_freq += dyn_ltree[n * 2]; n++;
            }
            data_type = (byte)(bin_freq > (SupportClass.URShift(ascii_freq, 2)) ? Z_BINARY : Z_ASCII);
        }

        // Flush the bit buffer, keeping at most 7 bits in it.
        internal void bi_flush()
        {
            if (bi_valid == 16)
            {
                put_short(bi_buf);
                bi_buf = 0;
                bi_valid = 0;
            }
            else if (bi_valid >= 8)
            {
                put_byte((byte)bi_buf);
                bi_buf = (short)(SupportClass.URShift(bi_buf, 8));
                bi_valid -= 8;
            }
        }

        // Flush the bit buffer and align the output on a byte boundary
        internal void bi_windup()
        {
            if (bi_valid > 8)
            {
                put_short(bi_buf);
            }
            else if (bi_valid > 0)
            {
                put_byte((byte)bi_buf);
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
            {
                put_short((short)len);
                put_short((short)~len);
            }

            //  while(len--!=0) {
            //    put_byte(window[buf+index]);
            //    index++;
            //  }
            put_byte(window, buf, len);
        }

        internal void flush_block_only(bool eof)
        {
            _tr_flush_block(block_start >= 0 ? block_start : -1, strstart - block_start, eof);
            block_start = strstart;
            strm.flush_pending();
        }

        // Copy without compression as much as possible from the input stream, return
        // the current block state.
        // This function does not insert new strings in the dictionary since
        // uncompressible data is probably not useful. This function is used
        // only for the level=0 compression option.
        // NOTE: this function should be optimized to avoid extra copying from
        // window to pending_buf.
        internal int deflate_stored(int flush)
        {
            // Stored blocks are limited to 0xffff bytes, pending_buf is limited
            // to pending_buf_size, and each stored block has a 5 byte header:

            int max_block_size = 0xffff;
            int max_start;

            if (max_block_size > pending_buf_size - 5)
            {
                max_block_size = pending_buf_size - 5;
            }

            // Copy as much as possible from input to output:
            while (true)
            {
                // Fill the window as much as possible:
                if (lookahead <= 1)
                {
                    fill_window();
                    if (lookahead == 0 && flush == Z_NO_FLUSH)
                        return NeedMore;
                    if (lookahead == 0)
                        break; // flush the current block
                }

                strstart += lookahead;
                lookahead = 0;

                // Emit a stored block if pending_buf will be full:
                max_start = block_start + max_block_size;
                if (strstart == 0 || strstart >= max_start)
                {
                    // strstart == 0 is possible when wraparound on 16-bit machine
                    lookahead = (int)(strstart - max_start);
                    strstart = (int)max_start;

                    flush_block_only(false);
                    if (strm.avail_out == 0)
                        return NeedMore;
                }

                // Flush if we may have to slide, otherwise block_start may become
                // negative and the data will be gone:
                if (strstart - block_start >= w_size - MIN_LOOKAHEAD)
                {
                    flush_block_only(false);
                    if (strm.avail_out == 0)
                        return NeedMore;
                }
            }

            flush_block_only(flush == Z_FINISH);
            if (strm.avail_out == 0)
                return (flush == Z_FINISH) ? FinishStarted : NeedMore;

            return flush == Z_FINISH ? FinishDone : BlockDone;
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
            if (level > 0)
            {
                // Check if the file is ascii or binary
                if (data_type == Z_UNKNOWN)
                    set_data_type();

                // Construct the literal and distance trees
                l_desc.build_tree(this);

                d_desc.build_tree(this);

                // At this point, opt_len and static_len are the total bit lengths of
                // the compressed block data, excluding the tree representations.

                // Build the bit length tree for the above two trees, and get the index
                // in bl_order of the last bit length code to send.
                max_blindex = build_bl_tree();

                // Determine the best encoding. Compute first the block length in bytes
                opt_lenb = SupportClass.URShift((opt_len + 3 + 7), 3);
                static_lenb = SupportClass.URShift((static_len + 3 + 7), 3);

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
                compress_block(StaticTree.static_ltree, StaticTree.static_dtree);
            }
            else
            {
                send_bits((DYN_TREES << 1) + (eof ? 1 : 0), 3);
                send_all_trees(l_desc.max_code + 1, d_desc.max_code + 1, max_blindex + 1);
                compress_block(dyn_ltree, dyn_dtree);
            }

            // The above check is made mod 2^32, for files larger than 512 MB
            // and uLong implemented on 32 bits.

            init_block();

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
        internal void fill_window()
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
                        head[p] = (short)(m >= w_size ? (m - w_size) : 0);
                        //head[p] = (m >= w_size?(short) (m - w_size):0);
                    }
                    while (--n != 0);

                    n = w_size;
                    p = n;
                    do
                    {
                        m = (prev[--p] & 0xffff);
                        prev[p] = (short)(m >= w_size ? (m - w_size) : 0);
                        //prev[p] = (m >= w_size?(short) (m - w_size):0);
                        // If n is not on any hash chain, prev[n] is garbage but
                        // its value will never be used.
                    }
                    while (--n != 0);
                    more += w_size;
                }

                if (strm.avail_in == 0)
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

                n = strm.read_buf(window, strstart + lookahead, more);
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
            while (lookahead < MIN_LOOKAHEAD && strm.avail_in != 0);
        }

        // Compress as much as possible from the input stream, return the current
        // block state.
        // This function does not perform lazy evaluation of matches and inserts
        // new strings in the dictionary only for unmatched strings or for short
        // matches. It is used only for the fast compression options.
        internal int deflate_fast(int flush)
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
                    fill_window();
                    if (lookahead < MIN_LOOKAHEAD && flush == Z_NO_FLUSH)
                    {
                        return NeedMore;
                    }
                    if (lookahead == 0)
                        break; // flush the current block
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:
                if (lookahead >= MIN_MATCH)
                {
                    ins_h = (((ins_h) << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;

                    //	prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = (head[ins_h] & 0xffff);
                    prev[strstart & w_mask] = head[ins_h];
                    head[ins_h] = (short)strstart;
                }

                // Find the longest match, discarding those <= prev_length.
                // At this point we have always match_length < MIN_MATCH

                if (hash_head != 0L && ((strstart - hash_head) & 0xffff) <= w_size - MIN_LOOKAHEAD)
                {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).
                    if (strategy != Z_HUFFMAN_ONLY)
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
                    if (match_length <= max_lazy_match && lookahead >= MIN_MATCH)
                    {
                        match_length--; // string at strstart already in hash table
                        do
                        {
                            strstart++;

                            ins_h = ((ins_h << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;
                            //	    prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = (head[ins_h] & 0xffff);
                            prev[strstart & w_mask] = head[ins_h];
                            head[ins_h] = (short)strstart;

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
                    if (strm.avail_out == 0)
                        return NeedMore;
                }
            }

            flush_block_only(flush == Z_FINISH);
            if (strm.avail_out == 0)
            {
                if (flush == Z_FINISH)
                    return FinishStarted;
                else
                    return NeedMore;
            }
            return flush == Z_FINISH ? FinishDone : BlockDone;
        }

        // Same as above, but achieves better compression. We use a lazy
        // evaluation for matches: a match is finally adopted only if there is
        // no better match at the next window position.
        internal int deflate_slow(int flush)
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
                    fill_window();
                    if (lookahead < MIN_LOOKAHEAD && flush == Z_NO_FLUSH)
                    {
                        return NeedMore;
                    }
                    if (lookahead == 0)
                        break; // flush the current block
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:

                if (lookahead >= MIN_MATCH)
                {
                    ins_h = (((ins_h) << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;
                    //	prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = (head[ins_h] & 0xffff);
                    prev[strstart & w_mask] = head[ins_h];
                    head[ins_h] = (short)strstart;
                }

                // Find the longest match, discarding those <= prev_length.
                prev_length = match_length; prev_match = match_start;
                match_length = MIN_MATCH - 1;

                if (hash_head != 0 && prev_length < max_lazy_match && ((strstart - hash_head) & 0xffff) <= w_size - MIN_LOOKAHEAD)
                {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).

                    if (strategy != Z_HUFFMAN_ONLY)
                    {
                        match_length = longest_match(hash_head);
                    }
                    // longest_match() sets match_start

                    if (match_length <= 5 && (strategy == Z_FILTERED || (match_length == MIN_MATCH && strstart - match_start > 4096)))
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
                            head[ins_h] = (short)strstart;
                        }
                    }
                    while (--prev_length != 0);
                    match_available = 0;
                    match_length = MIN_MATCH - 1;
                    strstart++;

                    if (bflush)
                    {
                        flush_block_only(false);
                        if (strm.avail_out == 0)
                            return NeedMore;
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
                    if (strm.avail_out == 0)
                        return NeedMore;
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
            flush_block_only(flush == Z_FINISH);

            if (strm.avail_out == 0)
            {
                if (flush == Z_FINISH)
                    return FinishStarted;
                else
                    return NeedMore;
            }

            return flush == Z_FINISH ? FinishDone : BlockDone;
        }

        internal int longest_match(int cur_match)
        {
            int chain_length = max_chain_length; // max hash chain length
            int scan = strstart; // current string
            int match; // matched string
            int len; // length of current match
            int best_len = prev_length; // best match length so far
            int limit = strstart > (w_size - MIN_LOOKAHEAD) ? strstart - (w_size - MIN_LOOKAHEAD) : 0;
            int nice_match = this.nice_match;

            // Stop when cur_match becomes <= limit. To simplify the code,
            // we prevent matches with the string of window index 0.

            int wmask = w_mask;

            int strend = strstart + MAX_MATCH;
            byte scan_end1 = window[scan + best_len - 1];
            byte scan_end = window[scan + best_len];

            // The code is optimized for HASH_BITS >= 8 and MAX_MATCH-2 multiple of 16.
            // It is easy to get rid of this optimization if necessary.

            // Do not waste too much time if we already have a good match:
            if (prev_length >= good_match)
            {
                chain_length >>= 2;
            }

            // Do not look for matches beyond the end of the input. This is necessary
            // to make deflate deterministic.
            if (nice_match > lookahead)
                nice_match = lookahead;

            do
            {
                match = cur_match;

                // Skip to next match if the match length cannot increase
                // or if the match length is less than 2:
                if (window[match + best_len] != scan_end || window[match + best_len - 1] != scan_end1 || window[match] != window[scan] || window[++match] != window[scan + 1])
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
                while (window[++scan] == window[++match] && window[++scan] == window[++match] && window[++scan] == window[++match] && window[++scan] == window[++match] && window[++scan] == window[++match] && window[++scan] == window[++match] && window[++scan] == window[++match] && window[++scan] == window[++match] && scan < strend);

                len = MAX_MATCH - (int)(strend - scan);
                scan = strend - MAX_MATCH;

                if (len > best_len)
                {
                    match_start = cur_match;
                    best_len = len;
                    if (len >= nice_match)
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

        internal int deflateInit(ZStream strm, int level, int bits)
        {
            return deflateInit2(strm, level, Z_DEFLATED, bits, DEF_MEM_LEVEL, Z_DEFAULT_STRATEGY);
        }
        internal int deflateInit(ZStream strm, int level)
        {
            return deflateInit(strm, level, MAX_WBITS);
        }
        internal int deflateInit2(ZStream strm, int level, int method, int windowBits, int memLevel, int strategy)
        {
            int noheader = 0;
            //    byte[] my_version=ZLIB_VERSION;

            //
            //  if (version == null || version[0] != my_version[0]
            //  || stream_size != sizeof(z_stream)) {
            //  return Z_VERSION_ERROR;
            //  }

            strm.msg = null;

            if (level == Z_DEFAULT_COMPRESSION)
                level = 6;

            if (windowBits < 0)
            {
                // undocumented feature: suppress zlib header
                noheader = 1;
                windowBits = -windowBits;
            }

            if (memLevel < 1 || memLevel > MAX_MEM_LEVEL || method != Z_DEFLATED || windowBits < 9 || windowBits > 15 || level < 0 || level > 9 || strategy < 0 || strategy > Z_HUFFMAN_ONLY)
            {
                return Z_STREAM_ERROR;
            }

            strm.dstate = (Deflate)this;

            this.noheader = noheader;
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

            lit_bufsize = 1 << (memLevel + 6); // 16K elements by default

            // We overlay pending_buf and d_buf+l_buf. This works since the average
            // output size for (length,distance) codes is <= 24 bits.
            pending_buf = new byte[lit_bufsize * 4];
            pending_buf_size = lit_bufsize * 4;

            d_buf = lit_bufsize;
            l_buf = (1 + 2) * lit_bufsize;

            this.level = level;

            //System.out.println("level="+level);

            this.strategy = strategy;
            this.method = (byte)method;

            return deflateReset(strm);
        }

        internal int deflateReset(ZStream strm)
        {
            strm.total_in = strm.total_out = 0;
            strm.msg = null; //
            strm.data_type = Z_UNKNOWN;

            pending = 0;
            pending_out = 0;

            if (noheader < 0)
            {
                noheader = 0; // was set to -1 by deflate(..., Z_FINISH);
            }
            status = (noheader != 0) ? BUSY_STATE : INIT_STATE;
            strm.adler = strm._adler.adler32(0, null, 0, 0);

            last_flush = Z_NO_FLUSH;

            tr_init();
            lm_init();
            return Z_OK;
        }

        internal int deflateEnd()
        {
            if (status != INIT_STATE && status != BUSY_STATE && status != FINISH_STATE)
            {
                return Z_STREAM_ERROR;
            }
            // Deallocate in reverse order of allocations:
            pending_buf = null;
            head = null;
            prev = null;
            window = null;
            // free
            // dstate=null;
            return status == BUSY_STATE ? Z_DATA_ERROR : Z_OK;
        }

        internal int deflateParams(ZStream strm, int _level, int _strategy)
        {
            int err = Z_OK;

            if (_level == Z_DEFAULT_COMPRESSION)
            {
                _level = 6;
            }
            if (_level < 0 || _level > 9 || _strategy < 0 || _strategy > Z_HUFFMAN_ONLY)
            {
                return Z_STREAM_ERROR;
            }

            if (config_table[level].func != config_table[_level].func && strm.total_in != 0)
            {
                // Flush the last buffer:
                err = strm.deflate(Z_PARTIAL_FLUSH);
            }

            if (level != _level)
            {
                level = _level;
                max_lazy_match = config_table[level].max_lazy;
                good_match = config_table[level].good_length;
                nice_match = config_table[level].nice_length;
                max_chain_length = config_table[level].max_chain;
            }
            strategy = _strategy;
            return err;
        }

        internal int deflateSetDictionary(ZStream strm, byte[] dictionary, int dictLength)
        {
            int length = dictLength;
            int index = 0;

            if (dictionary == null || status != INIT_STATE)
                return Z_STREAM_ERROR;

            strm.adler = strm._adler.adler32(strm.adler, dictionary, 0, dictLength);

            if (length < MIN_MATCH)
                return Z_OK;
            if (length > w_size - MIN_LOOKAHEAD)
            {
                length = w_size - MIN_LOOKAHEAD;
                index = dictLength - length; // use the tail of the dictionary
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
            return Z_OK;
        }

        internal int deflate(ZStream strm, int flush)
        {
            int old_flush;

            if (flush > Z_FINISH || flush < 0)
            {
                return Z_STREAM_ERROR;
            }

            if (strm.next_out == null || (strm.next_in == null && strm.avail_in != 0) || (status == FINISH_STATE && flush != Z_FINISH))
            {
                strm.msg = z_errmsg[Z_NEED_DICT - (Z_STREAM_ERROR)];
                return Z_STREAM_ERROR;
            }
            if (strm.avail_out == 0)
            {
                strm.msg = z_errmsg[Z_NEED_DICT - (Z_BUF_ERROR)];
                return Z_BUF_ERROR;
            }

            this.strm = strm; // just in case
            old_flush = last_flush;
            last_flush = flush;

            // Write the zlib header
            if (status == INIT_STATE)
            {
                int header = (Z_DEFLATED + ((w_bits - 8) << 4)) << 8;
                int level_flags = ((level - 1) & 0xff) >> 1;

                if (level_flags > 3)
                    level_flags = 3;
                header |= (level_flags << 6);
                if (strstart != 0)
                    header |= PRESET_DICT;
                header += 31 - (header % 31);

                status = BUSY_STATE;
                putShortMSB(header);


                // Save the adler32 of the preset dictionary:
                if (strstart != 0)
                {
                    putShortMSB((int)(SupportClass.URShift(strm.adler, 16)));
                    putShortMSB((int)(strm.adler & 0xffff));
                }
                strm.adler = strm._adler.adler32(0, null, 0, 0);
            }

            // Flush as much pending output as possible
            if (pending != 0)
            {
                strm.flush_pending();
                if (strm.avail_out == 0)
                {
                    //System.out.println("  avail_out==0");
                    // Since avail_out is 0, deflate will be called again with
                    // more output space, but possibly with both pending and
                    // avail_in equal to zero. There won't be anything to do,
                    // but this is not an error situation so make sure we
                    // return OK instead of BUF_ERROR at next call of deflate:
                    last_flush = -1;
                    return Z_OK;
                }

                // Make sure there is something to do and avoid duplicate consecutive
                // flushes. For repeated and useless calls with Z_FINISH, we keep
                // returning Z_STREAM_END instead of Z_BUFF_ERROR.
            }
            else if (strm.avail_in == 0 && flush <= old_flush && flush != Z_FINISH)
            {
                strm.msg = z_errmsg[Z_NEED_DICT - (Z_BUF_ERROR)];
                return Z_BUF_ERROR;
            }

            // User must not provide more input after the first FINISH:
            if (status == FINISH_STATE && strm.avail_in != 0)
            {
                strm.msg = z_errmsg[Z_NEED_DICT - (Z_BUF_ERROR)];
                return Z_BUF_ERROR;
            }

            // Start a new block or continue the current one.
            if (strm.avail_in != 0 || lookahead != 0 || (flush != Z_NO_FLUSH && status != FINISH_STATE))
            {
                int bstate = -1;
                switch (config_table[level].func)
                {

                    case STORED:
                        bstate = deflate_stored(flush);
                        break;

                    case FAST:
                        bstate = deflate_fast(flush);
                        break;

                    case SLOW:
                        bstate = deflate_slow(flush);
                        break;

                    default:
                        break;

                }

                if (bstate == FinishStarted || bstate == FinishDone)
                {
                    status = FINISH_STATE;
                }
                if (bstate == NeedMore || bstate == FinishStarted)
                {
                    if (strm.avail_out == 0)
                    {
                        last_flush = -1; // avoid BUF_ERROR next call, see above
                    }
                    return Z_OK;
                    // If flush != Z_NO_FLUSH && avail_out == 0, the next call
                    // of deflate should use the same flush parameter to make sure
                    // that the flush is complete. So we don't have to output an
                    // empty block here, this will be done at next call. This also
                    // ensures that for a very small output buffer, we emit at most
                    // one empty block.
                }

                if (bstate == BlockDone)
                {
                    if (flush == Z_PARTIAL_FLUSH)
                    {
                        _tr_align();
                    }
                    else
                    {
                        // FULL_FLUSH or SYNC_FLUSH
                        _tr_stored_block(0, 0, false);
                        // For a full flush, this empty block will be recognized
                        // as a special marker by inflate_sync().
                        if (flush == Z_FULL_FLUSH)
                        {
                            //state.head[s.hash_size-1]=0;
                            for (int i = 0; i < hash_size; i++)
                                // forget history
                                head[i] = 0;
                        }
                    }
                    strm.flush_pending();
                    if (strm.avail_out == 0)
                    {
                        last_flush = -1; // avoid BUF_ERROR at next call, see above
                        return Z_OK;
                    }
                }
            }

            if (flush != Z_FINISH)
                return Z_OK;
            if (noheader != 0)
                return Z_STREAM_END;

            // Write the zlib trailer (adler32)
            putShortMSB((int)(SupportClass.URShift(strm.adler, 16)));
            putShortMSB((int)(strm.adler & 0xffff));
            strm.flush_pending();

            // If avail_out is zero, the application will call deflate again
            // to flush the rest.
            noheader = -1; // write the trailer only once!
            return pending != 0 ? Z_OK : Z_STREAM_END;
        }
        static Deflate()
        {
            {
                config_table = new Config[10];
                //                         good  lazy  nice  chain
                config_table[0] = new Config(0, 0, 0, 0, STORED);
                config_table[1] = new Config(4, 4, 8, 4, FAST);
                config_table[2] = new Config(4, 5, 16, 8, FAST);
                config_table[3] = new Config(4, 6, 32, 32, FAST);

                config_table[4] = new Config(4, 4, 16, 16, SLOW);
                config_table[5] = new Config(8, 16, 32, 32, SLOW);
                config_table[6] = new Config(8, 16, 128, 128, SLOW);
                config_table[7] = new Config(8, 32, 128, 256, SLOW);
                config_table[8] = new Config(32, 128, 258, 1024, SLOW);
                config_table[9] = new Config(32, 258, 258, 4096, SLOW);
            }
        }
    }

    sealed class Inflate
    {

        private const int MAX_WBITS = 15; // 32K LZ77 window

        // preset dictionary flag in zlib header
        private const int PRESET_DICT = 0x20;

        internal const int Z_NO_FLUSH = 0;
        internal const int Z_PARTIAL_FLUSH = 1;
        internal const int Z_SYNC_FLUSH = 2;
        internal const int Z_FULL_FLUSH = 3;
        internal const int Z_FINISH = 4;

        private const int Z_DEFLATED = 8;

        private const int Z_OK = 0;
        private const int Z_STREAM_END = 1;
        private const int Z_NEED_DICT = 2;
        private const int Z_ERRNO = -1;
        private const int Z_STREAM_ERROR = -2;
        private const int Z_DATA_ERROR = -3;
        private const int Z_MEM_ERROR = -4;
        private const int Z_BUF_ERROR = -5;
        private const int Z_VERSION_ERROR = -6;

        private const int METHOD = 0; // waiting for method byte
        private const int FLAG = 1; // waiting for flag byte
        private const int DICT4 = 2; // four dictionary check bytes to go
        private const int DICT3 = 3; // three dictionary check bytes to go
        private const int DICT2 = 4; // two dictionary check bytes to go
        private const int DICT1 = 5; // one dictionary check byte to go
        private const int DICT0 = 6; // waiting for inflateSetDictionary
        private const int BLOCKS = 7; // decompressing blocks
        private const int CHECK4 = 8; // four check bytes to go
        private const int CHECK3 = 9; // three check bytes to go
        private const int CHECK2 = 10; // two check bytes to go
        private const int CHECK1 = 11; // one check byte to go
        private const int DONE = 12; // finished check, done
        private const int BAD = 13; // got an error--stay here

        internal int mode; // current inflate mode

        // mode dependent information
        internal int method; // if FLAGS, method byte

        // if CHECK, check values to compare
        internal long[] was = new long[1]; // computed check value
        internal long need; // stream check value

        // if BAD, inflateSync's marker bytes count
        internal int marker;

        // mode independent information
        internal int nowrap; // flag for no wrapper
        internal int wbits; // log2(window size)  (8..15, defaults to 15)

        internal InfBlocks blocks; // current inflate_blocks state

        internal int inflateReset(ZStream z)
        {
            if (z == null || z.istate == null)
                return Z_STREAM_ERROR;

            z.total_in = z.total_out = 0;
            z.msg = null;
            z.istate.mode = z.istate.nowrap != 0 ? BLOCKS : METHOD;
            z.istate.blocks.reset(z, null);
            return Z_OK;
        }

        internal int inflateEnd(ZStream z)
        {
            if (blocks != null)
                blocks.free(z);
            blocks = null;
            //    ZFREE(z, z->state);
            return Z_OK;
        }

        internal int inflateInit(ZStream z, int w)
        {
            z.msg = null;
            blocks = null;

            // handle undocumented nowrap option (no zlib header or check)
            nowrap = 0;
            if (w < 0)
            {
                w = -w;
                nowrap = 1;
            }

            // set window size
            if (w < 8 || w > 15)
            {
                inflateEnd(z);
                return Z_STREAM_ERROR;
            }
            wbits = w;

            z.istate.blocks = new InfBlocks(z, z.istate.nowrap != 0 ? null : this, 1 << w);

            // reset state
            inflateReset(z);
            return Z_OK;
        }

        internal int inflate(ZStream z, int f)
        {
            int r;
            int b;

            if (z == null || z.istate == null || z.next_in == null)
                return Z_STREAM_ERROR;
            f = f == Z_FINISH ? Z_BUF_ERROR : Z_OK;
            r = Z_BUF_ERROR;
            while (true)
            {
                //System.out.println("mode: "+z.istate.mode);
                switch (z.istate.mode)
                {

                    case METHOD:

                        if (z.avail_in == 0)
                            return r; r = f;

                        z.avail_in--; z.total_in++;
                        if (((z.istate.method = z.next_in[z.next_in_index++]) & 0xf) != Z_DEFLATED)
                        {
                            z.istate.mode = BAD;
                            z.msg = "unknown compression method";
                            z.istate.marker = 5; // can't try inflateSync
                            break;
                        }
                        if ((z.istate.method >> 4) + 8 > z.istate.wbits)
                        {
                            z.istate.mode = BAD;
                            z.msg = "invalid window size";
                            z.istate.marker = 5; // can't try inflateSync
                            break;
                        }
                        z.istate.mode = FLAG;
                        goto case FLAG;

                    case FLAG:

                        if (z.avail_in == 0)
                            return r; r = f;

                        z.avail_in--; z.total_in++;
                        b = (z.next_in[z.next_in_index++]) & 0xff;

                        if ((((z.istate.method << 8) + b) % 31) != 0)
                        {
                            z.istate.mode = BAD;
                            z.msg = "incorrect header check";
                            z.istate.marker = 5; // can't try inflateSync
                            break;
                        }

                        if ((b & PRESET_DICT) == 0)
                        {
                            z.istate.mode = BLOCKS;
                            break;
                        }
                        z.istate.mode = DICT4;
                        goto case DICT4;

                    case DICT4:

                        if (z.avail_in == 0)
                            return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need = ((z.next_in[z.next_in_index++] & 0xff) << 24) & unchecked((int)0xff000000L);
                        z.istate.mode = DICT3;
                        goto case DICT3;

                    case DICT3:

                        if (z.avail_in == 0)
                            return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += (((z.next_in[z.next_in_index++] & 0xff) << 16) & 0xff0000L);
                        z.istate.mode = DICT2;
                        goto case DICT2;

                    case DICT2:

                        if (z.avail_in == 0)
                            return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += (((z.next_in[z.next_in_index++] & 0xff) << 8) & 0xff00L);
                        z.istate.mode = DICT1;
                        goto case DICT1;

                    case DICT1:

                        if (z.avail_in == 0)
                            return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += (z.next_in[z.next_in_index++] & 0xffL);
                        z.adler = z.istate.need;
                        z.istate.mode = DICT0;
                        return Z_NEED_DICT;

                    case DICT0:
                        z.istate.mode = BAD;
                        z.msg = "need dictionary";
                        z.istate.marker = 0; // can try inflateSync
                        return Z_STREAM_ERROR;

                    case BLOCKS:

                        r = z.istate.blocks.proc(z, r);
                        if (r == Z_DATA_ERROR)
                        {
                            z.istate.mode = BAD;
                            z.istate.marker = 0; // can try inflateSync
                            break;
                        }
                        if (r == Z_OK)
                        {
                            r = f;
                        }
                        if (r != Z_STREAM_END)
                        {
                            return r;
                        }
                        r = f;
                        z.istate.blocks.reset(z, z.istate.was);
                        if (z.istate.nowrap != 0)
                        {
                            z.istate.mode = DONE;
                            break;
                        }
                        z.istate.mode = CHECK4;
                        goto case CHECK4;

                    case CHECK4:

                        if (z.avail_in == 0)
                            return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need = ((z.next_in[z.next_in_index++] & 0xff) << 24) & unchecked((int)0xff000000L);
                        z.istate.mode = CHECK3;
                        goto case CHECK3;

                    case CHECK3:

                        if (z.avail_in == 0)
                            return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += (((z.next_in[z.next_in_index++] & 0xff) << 16) & 0xff0000L);
                        z.istate.mode = CHECK2;
                        goto case CHECK2;

                    case CHECK2:

                        if (z.avail_in == 0)
                            return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += (((z.next_in[z.next_in_index++] & 0xff) << 8) & 0xff00L);
                        z.istate.mode = CHECK1;
                        goto case CHECK1;

                    case CHECK1:

                        if (z.avail_in == 0)
                            return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += (z.next_in[z.next_in_index++] & 0xffL);

                        if (((int)(z.istate.was[0])) != ((int)(z.istate.need)))
                        {
                            z.istate.mode = BAD;
                            z.msg = "incorrect data check";
                            z.istate.marker = 5; // can't try inflateSync
                            break;
                        }

                        z.istate.mode = DONE;
                        goto case DONE;

                    case DONE:
                        return Z_STREAM_END;

                    case BAD:
                        return Z_DATA_ERROR;

                    default:
                        return Z_STREAM_ERROR;

                }
            }
        }


        internal int inflateSetDictionary(ZStream z, byte[] dictionary, int dictLength)
        {
            int index = 0;
            int length = dictLength;
            if (z == null || z.istate == null || z.istate.mode != DICT0)
                return Z_STREAM_ERROR;

            if (z._adler.adler32(1L, dictionary, 0, dictLength) != z.adler)
            {
                return Z_DATA_ERROR;
            }

            z.adler = z._adler.adler32(0, null, 0, 0);

            if (length >= (1 << z.istate.wbits))
            {
                length = (1 << z.istate.wbits) - 1;
                index = dictLength - length;
            }
            z.istate.blocks.set_dictionary(dictionary, index, length);
            z.istate.mode = BLOCKS;
            return Z_OK;
        }

        private static byte[] mark = new byte[] { (byte)0, (byte)0, (byte)SupportClass.Identity(0xff), (byte)SupportClass.Identity(0xff) };

        internal int inflateSync(ZStream z)
        {
            int n; // number of bytes to look at
            int p; // pointer to bytes
            int m; // number of marker bytes found in a row
            long r, w; // temporaries to save total_in and total_out

            // set up
            if (z == null || z.istate == null)
                return Z_STREAM_ERROR;
            if (z.istate.mode != BAD)
            {
                z.istate.mode = BAD;
                z.istate.marker = 0;
            }
            if ((n = z.avail_in) == 0)
                return Z_BUF_ERROR;
            p = z.next_in_index;
            m = z.istate.marker;

            // search
            while (n != 0 && m < 4)
            {
                if (z.next_in[p] == mark[m])
                {
                    m++;
                }
                else if (z.next_in[p] != 0)
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
            z.total_in += p - z.next_in_index;
            z.next_in_index = p;
            z.avail_in = n;
            z.istate.marker = m;

            // return no joy or set up to restart on a new block
            if (m != 4)
            {
                return Z_DATA_ERROR;
            }
            r = z.total_in; w = z.total_out;
            inflateReset(z);
            z.total_in = r; z.total_out = w;
            z.istate.mode = BLOCKS;
            return Z_OK;
        }

        // Returns true if inflate is currently at the end of a block generated
        // by Z_SYNC_FLUSH or Z_FULL_FLUSH. This function is used by one PPP
        // implementation to provide an additional safety check. PPP uses Z_SYNC_FLUSH
        // but removes the length bytes of the resulting empty stored block. When
        // decompressing, PPP checks that at the end of input packet, inflate is
        // waiting for these length bytes.
        internal int inflateSyncPoint(ZStream z)
        {
            if (z == null || z.istate == null || z.istate.blocks == null)
                return Z_STREAM_ERROR;
            return z.istate.blocks.sync_point();
        }
    }

    sealed class InfBlocks
    {
        private const int MANY = 1440;

        // And'ing with mask[n] masks the lower n bits		
        private static readonly int[] inflate_mask = new int[] { 0x00000000, 0x00000001, 0x00000003, 0x00000007, 0x0000000f, 0x0000001f, 0x0000003f, 0x0000007f, 0x000000ff, 0x000001ff, 0x000003ff, 0x000007ff, 0x00000fff, 0x00001fff, 0x00003fff, 0x00007fff, 0x0000ffff };

        // Table for deflate from PKZIP's appnote.txt.		
        internal static readonly int[] border = new int[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

        private const int Z_OK = 0;
        private const int Z_STREAM_END = 1;
        private const int Z_NEED_DICT = 2;
        private const int Z_ERRNO = -1;
        private const int Z_STREAM_ERROR = -2;
        private const int Z_DATA_ERROR = -3;
        private const int Z_MEM_ERROR = -4;
        private const int Z_BUF_ERROR = -5;
        private const int Z_VERSION_ERROR = -6;

        private const int TYPE = 0; // get type bits (3, including end bit)
        private const int LENS = 1; // get lengths for stored
        private const int STORED = 2; // processing stored block
        private const int TABLE = 3; // get table lengths
        private const int BTREE = 4; // get bit lengths tree for a dynamic block
        private const int DTREE = 5; // get length, distance trees for a dynamic block
        private const int CODES = 6; // processing fixed or dynamic block
        private const int DRY = 7; // output remaining window bytes
        private const int DONE = 8; // finished last block, done
        private const int BAD = 9; // ot a data error--stuck here

        internal int mode; // current inflate_block mode 

        internal int left; // if STORED, bytes left to copy 

        internal int table; // table lengths (14 bits) 
        internal int index; // index into blens (or border) 
        internal int[] blens; // bit lengths of codes 
        internal int[] bb = new int[1]; // bit length tree depth 
        internal int[] tb = new int[1]; // bit length decoding tree 

        internal InfCodes codes; // if CODES, current state 

        internal int last; // true if this block is the last block 

        // mode independent information 
        internal int bitk; // bits in bit buffer 
        internal int bitb; // bit buffer 
        internal int[] hufts; // single malloc for tree space 
        internal byte[] window; // sliding window 
        internal int end; // one byte after sliding window 
        internal int read; // window read pointer 
        internal int write; // window write pointer 
        internal System.Object checkfn; // check function 
        internal long check; // check on output 

        internal InfBlocks(ZStream z, System.Object checkfn, int w)
        {
            hufts = new int[MANY * 3];
            window = new byte[w];
            end = w;
            this.checkfn = checkfn;
            mode = TYPE;
            reset(z, null);
        }

        internal void reset(ZStream z, long[] c)
        {
            if (c != null)
                c[0] = check;
            if (mode == BTREE || mode == DTREE)
            {
                blens = null;
            }
            if (mode == CODES)
            {
                codes.free(z);
            }
            mode = TYPE;
            bitk = 0;
            bitb = 0;
            read = write = 0;

            if (checkfn != null)
                z.adler = check = z._adler.adler32(0L, null, 0, 0);
        }

        internal int proc(ZStream z, int r)
        {
            int t; // temporary storage
            int b; // bit buffer
            int k; // bits in bit buffer
            int p; // input data pointer
            int n; // bytes available there
            int q; // output window write pointer
            int m; // bytes to end of window or read pointer

            // copy input/output information to locals (UPDATE macro restores)
            {
                p = z.next_in_index; n = z.avail_in; b = bitb; k = bitk;
            }
            {
                q = write; m = (int)(q < read ? read - q - 1 : end - q);
            }

            // process input based on current state
            while (true)
            {
                switch (mode)
                {

                    case TYPE:

                        while (k < (3))
                        {
                            if (n != 0)
                            {
                                r = Z_OK;
                            }
                            else
                            {
                                bitb = b; bitk = k;
                                z.avail_in = n;
                                z.total_in += p - z.next_in_index; z.next_in_index = p;
                                write = q;
                                return inflate_flush(z, r);
                            }
                            ;
                            n--;
                            b |= (z.next_in[p++] & 0xff) << k;
                            k += 8;
                        }
                        t = (int)(b & 7);
                        last = t & 1;

                        switch (SupportClass.URShift(t, 1))
                        {

                            case 0:  // stored 
                                {
                                    b = SupportClass.URShift(b, (3)); k -= (3);
                                }
                                t = k & 7; // go to byte boundary
                                {
                                    b = SupportClass.URShift(b, (t)); k -= (t);
                                }
                                mode = LENS; // get length of stored block
                                break;

                            case 1:  // fixed
                                {
                                    int[] bl = new int[1];
                                    int[] bd = new int[1];
                                    int[][] tl = new int[1][];
                                    int[][] td = new int[1][];

                                    InfTree.inflate_trees_fixed(bl, bd, tl, td, z);
                                    codes = new InfCodes(bl[0], bd[0], tl[0], td[0], z);
                                }
                                {
                                    b = SupportClass.URShift(b, (3)); k -= (3);
                                }

                                mode = CODES;
                                break;

                            case 2:  // dynamic
                                {
                                    b = SupportClass.URShift(b, (3)); k -= (3);
                                }

                                mode = TABLE;
                                break;

                            case 3:  // illegal
                                {
                                    b = SupportClass.URShift(b, (3)); k -= (3);
                                }
                                mode = BAD;
                                z.msg = "invalid block type";
                                r = Z_DATA_ERROR;

                                bitb = b; bitk = k;
                                z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                write = q;
                                return inflate_flush(z, r);
                        }
                        break;

                    case LENS:

                        while (k < (32))
                        {
                            if (n != 0)
                            {
                                r = Z_OK;
                            }
                            else
                            {
                                bitb = b; bitk = k;
                                z.avail_in = n;
                                z.total_in += p - z.next_in_index; z.next_in_index = p;
                                write = q;
                                return inflate_flush(z, r);
                            }
                            ;
                            n--;
                            b |= (z.next_in[p++] & 0xff) << k;
                            k += 8;
                        }

                        if (((SupportClass.URShift((~b), 16)) & 0xffff) != (b & 0xffff))
                        {
                            mode = BAD;
                            z.msg = "invalid stored block lengths";
                            r = Z_DATA_ERROR;

                            bitb = b; bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            write = q;
                            return inflate_flush(z, r);
                        }
                        left = (b & 0xffff);
                        b = k = 0; // dump bits
                        mode = left != 0 ? STORED : (last != 0 ? DRY : TYPE);
                        break;

                    case STORED:
                        if (n == 0)
                        {
                            bitb = b; bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            write = q;
                            return inflate_flush(z, r);
                        }

                        if (m == 0)
                        {
                            if (q == end && read != 0)
                            {
                                q = 0; m = (int)(q < read ? read - q - 1 : end - q);
                            }
                            if (m == 0)
                            {
                                write = q;
                                r = inflate_flush(z, r);
                                q = write; m = (int)(q < read ? read - q - 1 : end - q);
                                if (q == end && read != 0)
                                {
                                    q = 0; m = (int)(q < read ? read - q - 1 : end - q);
                                }
                                if (m == 0)
                                {
                                    bitb = b; bitk = k;
                                    z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                    write = q;
                                    return inflate_flush(z, r);
                                }
                            }
                        }
                        r = Z_OK;

                        t = left;
                        if (t > n)
                            t = n;
                        if (t > m)
                            t = m;
                        Array.Copy(z.next_in, p, window, q, t);
                        p += t; n -= t;
                        q += t; m -= t;
                        if ((left -= t) != 0)
                            break;
                        mode = last != 0 ? DRY : TYPE;
                        break;

                    case TABLE:

                        while (k < (14))
                        {
                            if (n != 0)
                            {
                                r = Z_OK;
                            }
                            else
                            {
                                bitb = b; bitk = k;
                                z.avail_in = n;
                                z.total_in += p - z.next_in_index; z.next_in_index = p;
                                write = q;
                                return inflate_flush(z, r);
                            }
                            ;
                            n--;
                            b |= (z.next_in[p++] & 0xff) << k;
                            k += 8;
                        }

                        table = t = (b & 0x3fff);
                        if ((t & 0x1f) > 29 || ((t >> 5) & 0x1f) > 29)
                        {
                            mode = BAD;
                            z.msg = "too many length or distance symbols";
                            r = Z_DATA_ERROR;

                            bitb = b; bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            write = q;
                            return inflate_flush(z, r);
                        }
                        t = 258 + (t & 0x1f) + ((t >> 5) & 0x1f);
                        blens = new int[t];
                        {
                            b = SupportClass.URShift(b, (14)); k -= (14);
                        }

                        index = 0;
                        mode = BTREE;
                        goto case BTREE;

                    case BTREE:
                        while (index < 4 + (SupportClass.URShift(table, 10)))
                        {
                            while (k < (3))
                            {
                                if (n != 0)
                                {
                                    r = Z_OK;
                                }
                                else
                                {
                                    bitb = b; bitk = k;
                                    z.avail_in = n;
                                    z.total_in += p - z.next_in_index; z.next_in_index = p;
                                    write = q;
                                    return inflate_flush(z, r);
                                }
                                ;
                                n--;
                                b |= (z.next_in[p++] & 0xff) << k;
                                k += 8;
                            }

                            blens[border[index++]] = b & 7;

                            {
                                b = SupportClass.URShift(b, (3)); k -= (3);
                            }
                        }

                        while (index < 19)
                        {
                            blens[border[index++]] = 0;
                        }

                        bb[0] = 7;
                        t = InfTree.inflate_trees_bits(blens, bb, tb, hufts, z);
                        if (t != Z_OK)
                        {
                            r = t;
                            if (r == Z_DATA_ERROR)
                            {
                                blens = null;
                                mode = BAD;
                            }

                            bitb = b; bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            write = q;
                            return inflate_flush(z, r);
                        }

                        index = 0;
                        mode = DTREE;
                        goto case DTREE;

                    case DTREE:
                        while (true)
                        {
                            t = table;
                            if (!(index < 258 + (t & 0x1f) + ((t >> 5) & 0x1f)))
                            {
                                break;
                            }


                            int i, j, c;

                            t = bb[0];

                            while (k < (t))
                            {
                                if (n != 0)
                                {
                                    r = Z_OK;
                                }
                                else
                                {
                                    bitb = b; bitk = k;
                                    z.avail_in = n;
                                    z.total_in += p - z.next_in_index; z.next_in_index = p;
                                    write = q;
                                    return inflate_flush(z, r);
                                }
                                ;
                                n--;
                                b |= (z.next_in[p++] & 0xff) << k;
                                k += 8;
                            }

                            if (tb[0] == -1)
                            {
                                //System.err.println("null...");
                            }

                            t = hufts[(tb[0] + (b & inflate_mask[t])) * 3 + 1];
                            c = hufts[(tb[0] + (b & inflate_mask[t])) * 3 + 2];

                            if (c < 16)
                            {
                                b = SupportClass.URShift(b, (t)); k -= (t);
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
                                        r = Z_OK;
                                    }
                                    else
                                    {
                                        bitb = b; bitk = k;
                                        z.avail_in = n;
                                        z.total_in += p - z.next_in_index; z.next_in_index = p;
                                        write = q;
                                        return inflate_flush(z, r);
                                    }
                                    ;
                                    n--;
                                    b |= (z.next_in[p++] & 0xff) << k;
                                    k += 8;
                                }

                                b = SupportClass.URShift(b, (t)); k -= (t);

                                j += (b & inflate_mask[i]);

                                b = SupportClass.URShift(b, (i)); k -= (i);

                                i = index;
                                t = table;
                                if (i + j > 258 + (t & 0x1f) + ((t >> 5) & 0x1f) || (c == 16 && i < 1))
                                {
                                    blens = null;
                                    mode = BAD;
                                    z.msg = "invalid bit length repeat";
                                    r = Z_DATA_ERROR;

                                    bitb = b; bitk = k;
                                    z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                    write = q;
                                    return inflate_flush(z, r);
                                }

                                c = c == 16 ? blens[i - 1] : 0;
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
                            int[] bl = new int[1];
                            int[] bd = new int[1];
                            int[] tl = new int[1];
                            int[] td = new int[1];


                            bl[0] = 9; // must be <= 9 for lookahead assumptions
                            bd[0] = 6; // must be <= 9 for lookahead assumptions
                            t = table;
                            t = InfTree.inflate_trees_dynamic(257 + (t & 0x1f), 1 + ((t >> 5) & 0x1f), blens, bl, bd, tl, td, hufts, z);
                            if (t != Z_OK)
                            {
                                if (t == Z_DATA_ERROR)
                                {
                                    blens = null;
                                    mode = BAD;
                                }
                                r = t;

                                bitb = b; bitk = k;
                                z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                write = q;
                                return inflate_flush(z, r);
                            }

                            codes = new InfCodes(bl[0], bd[0], hufts, tl[0], hufts, td[0], z);
                        }
                        blens = null;
                        mode = CODES;
                        goto case CODES;

                    case CODES:
                        bitb = b; bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        write = q;

                        if ((r = codes.proc(this, z, r)) != Z_STREAM_END)
                        {
                            return inflate_flush(z, r);
                        }
                        r = Z_OK;
                        codes.free(z);

                        p = z.next_in_index; n = z.avail_in; b = bitb; k = bitk;
                        q = write; m = (int)(q < read ? read - q - 1 : end - q);

                        if (last == 0)
                        {
                            mode = TYPE;
                            break;
                        }
                        mode = DRY;
                        goto case DRY;

                    case DRY:
                        write = q;
                        r = inflate_flush(z, r);
                        q = write; m = (int)(q < read ? read - q - 1 : end - q);
                        if (read != write)
                        {
                            bitb = b; bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            write = q;
                            return inflate_flush(z, r);
                        }
                        mode = DONE;
                        goto case DONE;

                    case DONE:
                        r = Z_STREAM_END;

                        bitb = b; bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        write = q;
                        return inflate_flush(z, r);

                    case BAD:
                        r = Z_DATA_ERROR;

                        bitb = b; bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        write = q;
                        return inflate_flush(z, r);


                    default:
                        r = Z_STREAM_ERROR;

                        bitb = b; bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        write = q;
                        return inflate_flush(z, r);

                }
            }
        }

        internal void free(ZStream z)
        {
            reset(z, null);
            window = null;
            hufts = null;
            //ZFREE(z, s);
        }

        internal void set_dictionary(byte[] d, int start, int n)
        {
            Array.Copy(d, start, window, 0, n);
            read = write = n;
        }

        // Returns true if inflate is currently at the end of a block generated
        // by Z_SYNC_FLUSH or Z_FULL_FLUSH. 
        internal int sync_point()
        {
            return mode == LENS ? 1 : 0;
        }

        // copy as much as possible from the sliding window to the output area
        internal int inflate_flush(ZStream z, int r)
        {
            int n;
            int p;
            int q;

            // local copies of source and destination pointers
            p = z.next_out_index;
            q = read;

            // compute number of bytes to copy as far as end of window
            n = (int)((q <= write ? write : end) - q);
            if (n > z.avail_out)
                n = z.avail_out;
            if (n != 0 && r == Z_BUF_ERROR)
                r = Z_OK;

            // update counters
            z.avail_out -= n;
            z.total_out += n;

            // update check information
            if (checkfn != null)
                z.adler = check = z._adler.adler32(check, window, q, n);

            // copy as far as end of window
            Array.Copy(window, q, z.next_out, p, n);
            p += n;
            q += n;

            // see if more to copy at beginning of window
            if (q == end)
            {
                // wrap pointers
                q = 0;
                if (write == end)
                    write = 0;

                // compute bytes to copy
                n = write - q;
                if (n > z.avail_out)
                    n = z.avail_out;
                if (n != 0 && r == Z_BUF_ERROR)
                    r = Z_OK;

                // update counters
                z.avail_out -= n;
                z.total_out += n;

                // update check information
                if (checkfn != null)
                    z.adler = check = z._adler.adler32(check, window, q, n);

                // copy
                Array.Copy(window, q, z.next_out, p, n);
                p += n;
                q += n;
            }

            // update pointers
            z.next_out_index = p;
            read = q;

            // done
            return r;
        }
    }

    sealed class InfCodes
    {

        private static readonly int[] inflate_mask = new int[] { 0x00000000, 0x00000001, 0x00000003, 0x00000007, 0x0000000f, 0x0000001f, 0x0000003f, 0x0000007f, 0x000000ff, 0x000001ff, 0x000003ff, 0x000007ff, 0x00000fff, 0x00001fff, 0x00003fff, 0x00007fff, 0x0000ffff };

        private const int Z_OK = 0;
        private const int Z_STREAM_END = 1;
        private const int Z_NEED_DICT = 2;
        private const int Z_ERRNO = -1;
        private const int Z_STREAM_ERROR = -2;
        private const int Z_DATA_ERROR = -3;
        private const int Z_MEM_ERROR = -4;
        private const int Z_BUF_ERROR = -5;
        private const int Z_VERSION_ERROR = -6;

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

        internal int mode; // current inflate_codes mode

        // mode dependent information
        internal int len;

        internal int[] tree; // pointer into tree
        internal int tree_index = 0;
        internal int need; // bits needed

        internal int lit;

        // if EXT or COPY, where and how much
        internal int get_Renamed; // bits to get for extra
        internal int dist; // distance back to copy from

        internal byte lbits; // ltree bits decoded per branch
        internal byte dbits; // dtree bits decoder per branch
        internal int[] ltree; // literal/length/eob tree
        internal int ltree_index; // literal/length/eob tree
        internal int[] dtree; // distance tree
        internal int dtree_index; // distance tree

        internal InfCodes(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, ZStream z)
        {
            mode = START;
            lbits = (byte)bl;
            dbits = (byte)bd;
            ltree = tl;
            ltree_index = tl_index;
            dtree = td;
            dtree_index = td_index;
        }

        internal InfCodes(int bl, int bd, int[] tl, int[] td, ZStream z)
        {
            mode = START;
            lbits = (byte)bl;
            dbits = (byte)bd;
            ltree = tl;
            ltree_index = 0;
            dtree = td;
            dtree_index = 0;
        }

        internal int proc(InfBlocks s, ZStream z, int r)
        {
            int j; // temporary storage
            //int[] t; // temporary pointer
            int tindex; // temporary pointer
            int e; // extra bits or operation
            int b = 0; // bit buffer
            int k = 0; // bits in bit buffer
            int p = 0; // input data pointer
            int n; // bytes available there
            int q; // output window write pointer
            int m; // bytes to end of window or read pointer
            int f; // pointer to copy strings from

            // copy input/output information to locals (UPDATE macro restores)
            p = z.next_in_index; n = z.avail_in; b = s.bitb; k = s.bitk;
            q = s.write; m = q < s.read ? s.read - q - 1 : s.end - q;

            // process input and output based on current state
            while (true)
            {
                switch (mode)
                {

                    // waiting for "i:"=input, "o:"=output, "x:"=nothing
                    case START:  // x: set up for LEN
                        if (m >= 258 && n >= 10)
                        {

                            s.bitb = b; s.bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            s.write = q;
                            r = inflate_fast(lbits, dbits, ltree, ltree_index, dtree, dtree_index, s, z);

                            p = z.next_in_index; n = z.avail_in; b = s.bitb; k = s.bitk;
                            q = s.write; m = q < s.read ? s.read - q - 1 : s.end - q;

                            if (r != Z_OK)
                            {
                                mode = r == Z_STREAM_END ? WASH : BADCODE;
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

                        while (k < (j))
                        {
                            if (n != 0)
                                r = Z_OK;
                            else
                            {

                                s.bitb = b; s.bitk = k;
                                z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                s.write = q;
                                return s.inflate_flush(z, r);
                            }
                            n--;
                            b |= (z.next_in[p++] & 0xff) << k;
                            k += 8;
                        }

                        tindex = (tree_index + (b & inflate_mask[j])) * 3;

                        b = SupportClass.URShift(b, (tree[tindex + 1]));
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
                            get_Renamed = e & 15;
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
                        z.msg = "invalid literal/length code";
                        r = Z_DATA_ERROR;

                        s.bitb = b; s.bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        s.write = q;
                        return s.inflate_flush(z, r);


                    case LENEXT:  // i: getting length extra (have base)
                        j = get_Renamed;

                        while (k < (j))
                        {
                            if (n != 0)
                                r = Z_OK;
                            else
                            {

                                s.bitb = b; s.bitk = k;
                                z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                s.write = q;
                                return s.inflate_flush(z, r);
                            }
                            n--; b |= (z.next_in[p++] & 0xff) << k;
                            k += 8;
                        }

                        len += (b & inflate_mask[j]);

                        b >>= j;
                        k -= j;

                        need = dbits;
                        tree = dtree;
                        tree_index = dtree_index;
                        mode = DIST;
                        goto case DIST;

                    case DIST:  // i: get distance next
                        j = need;

                        while (k < (j))
                        {
                            if (n != 0)
                                r = Z_OK;
                            else
                            {

                                s.bitb = b; s.bitk = k;
                                z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                s.write = q;
                                return s.inflate_flush(z, r);
                            }
                            n--; b |= (z.next_in[p++] & 0xff) << k;
                            k += 8;
                        }

                        tindex = (tree_index + (b & inflate_mask[j])) * 3;

                        b >>= tree[tindex + 1];
                        k -= tree[tindex + 1];

                        e = (tree[tindex]);
                        if ((e & 16) != 0)
                        {
                            // distance
                            get_Renamed = e & 15;
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
                        z.msg = "invalid distance code";
                        r = Z_DATA_ERROR;

                        s.bitb = b; s.bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        s.write = q;
                        return s.inflate_flush(z, r);


                    case DISTEXT:  // i: getting distance extra
                        j = get_Renamed;

                        while (k < (j))
                        {
                            if (n != 0)
                                r = Z_OK;
                            else
                            {

                                s.bitb = b; s.bitk = k;
                                z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                s.write = q;
                                return s.inflate_flush(z, r);
                            }
                            n--; b |= (z.next_in[p++] & 0xff) << k;
                            k += 8;
                        }

                        dist += (b & inflate_mask[j]);

                        b >>= j;
                        k -= j;

                        mode = COPY;
                        goto case COPY;

                    case COPY:  // o: copying bytes in window, waiting for space
                        f = q - dist;
                        while (f < 0)
                        {
                            // modulo window size-"while" instead
                            f += s.end; // of "if" handles invalid distances
                        }
                        while (len != 0)
                        {

                            if (m == 0)
                            {
                                if (q == s.end && s.read != 0)
                                {
                                    q = 0; m = q < s.read ? s.read - q - 1 : s.end - q;
                                }
                                if (m == 0)
                                {
                                    s.write = q; r = s.inflate_flush(z, r);
                                    q = s.write; m = q < s.read ? s.read - q - 1 : s.end - q;

                                    if (q == s.end && s.read != 0)
                                    {
                                        q = 0; m = q < s.read ? s.read - q - 1 : s.end - q;
                                    }

                                    if (m == 0)
                                    {
                                        s.bitb = b; s.bitk = k;
                                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                        s.write = q;
                                        return s.inflate_flush(z, r);
                                    }
                                }
                            }

                            s.window[q++] = s.window[f++]; m--;

                            if (f == s.end)
                                f = 0;
                            len--;
                        }
                        mode = START;
                        break;

                    case LIT:  // o: got literal, waiting for output space
                        if (m == 0)
                        {
                            if (q == s.end && s.read != 0)
                            {
                                q = 0; m = q < s.read ? s.read - q - 1 : s.end - q;
                            }
                            if (m == 0)
                            {
                                s.write = q; r = s.inflate_flush(z, r);
                                q = s.write; m = q < s.read ? s.read - q - 1 : s.end - q;

                                if (q == s.end && s.read != 0)
                                {
                                    q = 0; m = q < s.read ? s.read - q - 1 : s.end - q;
                                }
                                if (m == 0)
                                {
                                    s.bitb = b; s.bitk = k;
                                    z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                    s.write = q;
                                    return s.inflate_flush(z, r);
                                }
                            }
                        }
                        r = Z_OK;

                        s.window[q++] = (byte)lit; m--;

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

                        s.write = q; r = s.inflate_flush(z, r);
                        q = s.write; m = q < s.read ? s.read - q - 1 : s.end - q;

                        if (s.read != s.write)
                        {
                            s.bitb = b; s.bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            s.write = q;
                            return s.inflate_flush(z, r);
                        }
                        mode = END;
                        goto case END;

                    case END:
                        r = Z_STREAM_END;
                        s.bitb = b; s.bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        s.write = q;
                        return s.inflate_flush(z, r);


                    case BADCODE:  // x: got error

                        r = Z_DATA_ERROR;

                        s.bitb = b; s.bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        s.write = q;
                        return s.inflate_flush(z, r);


                    default:
                        r = Z_STREAM_ERROR;

                        s.bitb = b; s.bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        s.write = q;
                        return s.inflate_flush(z, r);

                }
            }
        }

        internal void free(ZStream z)
        {
            //  ZFREE(z, c);
        }

        // Called with number of bytes left to write in window at least 258
        // (the maximum string length) and number of input bytes available
        // at least ten.  The ten bytes are six bytes for the longest length/
        // distance pair plus four bytes for overloading the bit buffer.

        internal int inflate_fast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InfBlocks s, ZStream z)
        {
            int t; // temporary pointer
            int[] tp; // temporary pointer
            int tp_index; // temporary pointer
            int e; // extra bits or operation
            int b; // bit buffer
            int k; // bits in bit buffer
            int p; // input data pointer
            int n; // bytes available there
            int q; // output window write pointer
            int m; // bytes to end of window or read pointer
            int ml; // mask for literal/length tree
            int md; // mask for distance tree
            int c; // bytes to copy
            int d; // distance back to copy from
            int r; // copy source pointer

            // load input, output, bit values
            p = z.next_in_index; n = z.avail_in; b = s.bitb; k = s.bitk;
            q = s.write; m = q < s.read ? s.read - q - 1 : s.end - q;

            // initialize masks
            ml = inflate_mask[bl];
            md = inflate_mask[bd];

            // do until not enough input or output space for fast loop
            do
            {
                // assume called with m >= 258 && n >= 10
                // get literal/length code
                while (k < (20))
                {
                    // max bits for literal/length code
                    n--;
                    b |= (z.next_in[p++] & 0xff) << k; k += 8;
                }

                t = b & ml;
                tp = tl;
                tp_index = tl_index;
                if ((e = tp[(tp_index + t) * 3]) == 0)
                {
                    b >>= (tp[(tp_index + t) * 3 + 1]); k -= (tp[(tp_index + t) * 3 + 1]);

                    s.window[q++] = (byte)tp[(tp_index + t) * 3 + 2];
                    m--;
                    continue;
                }
                do
                {

                    b >>= (tp[(tp_index + t) * 3 + 1]); k -= (tp[(tp_index + t) * 3 + 1]);

                    if ((e & 16) != 0)
                    {
                        e &= 15;
                        c = tp[(tp_index + t) * 3 + 2] + ((int)b & inflate_mask[e]);

                        b >>= e; k -= e;

                        // decode distance base of block to copy
                        while (k < (15))
                        {
                            // max bits for distance code
                            n--;
                            b |= (z.next_in[p++] & 0xff) << k; k += 8;
                        }

                        t = b & md;
                        tp = td;
                        tp_index = td_index;
                        e = tp[(tp_index + t) * 3];

                        do
                        {

                            b >>= (tp[(tp_index + t) * 3 + 1]); k -= (tp[(tp_index + t) * 3 + 1]);

                            if ((e & 16) != 0)
                            {
                                // get extra bits to add to distance base
                                e &= 15;
                                while (k < (e))
                                {
                                    // get extra bits (up to 13)
                                    n--;
                                    b |= (z.next_in[p++] & 0xff) << k; k += 8;
                                }

                                d = tp[(tp_index + t) * 3 + 2] + (b & inflate_mask[e]);

                                b >>= (e); k -= (e);

                                // do the copy
                                m -= c;
                                if (q >= d)
                                {
                                    // offset before dest
                                    //  just copy
                                    r = q - d;
                                    if (q - r > 0 && 2 > (q - r))
                                    {
                                        s.window[q++] = s.window[r++]; c--; // minimum count is three,
                                        s.window[q++] = s.window[r++]; c--; // so unroll loop a little
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
                                t += tp[(tp_index + t) * 3 + 2];
                                t += (b & inflate_mask[e]);
                                e = tp[(tp_index + t) * 3];
                            }
                            else
                            {
                                z.msg = "invalid distance code";

                                c = z.avail_in - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

                                s.bitb = b; s.bitk = k;
                                z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                s.write = q;

                                return Z_DATA_ERROR;
                            }
                        }
                        while (true);
                        break;
                    }

                    if ((e & 64) == 0)
                    {
                        t += tp[(tp_index + t) * 3 + 2];
                        t += (b & inflate_mask[e]);
                        if ((e = tp[(tp_index + t) * 3]) == 0)
                        {

                            b >>= (tp[(tp_index + t) * 3 + 1]); k -= (tp[(tp_index + t) * 3 + 1]);

                            s.window[q++] = (byte)tp[(tp_index + t) * 3 + 2];
                            m--;
                            break;
                        }
                    }
                    else if ((e & 32) != 0)
                    {

                        c = z.avail_in - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

                        s.bitb = b; s.bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        s.write = q;

                        return Z_STREAM_END;
                    }
                    else
                    {
                        z.msg = "invalid literal/length code";

                        c = z.avail_in - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

                        s.bitb = b; s.bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        s.write = q;

                        return Z_DATA_ERROR;
                    }
                }
                while (true);
            }
            while (m >= 258 && n >= 10);

            // not enough input or output--restore pointers and return
            c = z.avail_in - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

            s.bitb = b; s.bitk = k;
            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
            s.write = q;

            return Z_OK;
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


        internal static readonly int[] fixed_tl = new int[]{96, 7, 256, 0, 8, 80, 0, 8, 16, 84, 8, 115, 82, 7, 31, 0, 8, 112, 0, 8, 48, 0, 9, 192, 80, 7, 10, 0, 8, 96, 0, 8, 32, 0, 9, 160, 0, 8, 0, 0, 8, 128, 0, 8, 64, 0, 9, 224, 80, 7, 6, 0, 8, 88, 0, 8, 24, 0, 9, 144, 83, 7, 59, 0, 8, 120, 0, 8, 56, 0, 9, 208, 81, 7, 17, 0, 8, 104, 0, 8, 40, 0, 9, 176, 0, 8, 8, 0, 8, 136, 0, 8, 72, 0, 9, 240, 80, 7, 4, 0, 8, 84, 0, 8, 20, 85, 8, 227, 83, 7, 43, 0, 8, 116, 0, 8, 52, 0, 9, 200, 81, 7, 13, 0, 8, 100, 0, 8, 36, 0, 9, 168, 0, 8, 4, 0, 8, 132, 0, 8, 68, 0, 9, 232, 80, 7, 8, 0, 8, 92, 0, 8, 28, 0, 9, 152, 84, 7, 83, 0, 8, 124, 0, 8, 60, 0, 9, 216, 82, 7, 23, 0, 8, 108, 0, 8, 44, 0, 9, 184, 0, 8, 12, 0, 8, 140, 0, 8, 76, 0, 9, 248, 80, 7, 3, 0, 8, 82, 0, 8, 18, 85, 8, 163, 83, 7, 35, 0, 8, 114, 0, 8, 50, 0, 9, 196, 81, 7, 11, 0, 8, 98, 0, 8, 34, 0, 9, 164, 0, 8, 2, 0, 8, 130, 0, 8, 66, 0, 9, 228, 80, 7, 7, 0, 8, 90, 0, 8, 26, 0, 9, 148, 84, 7, 67, 0, 8, 122, 0, 8, 58, 0, 9, 212, 82, 7, 19, 0, 8, 106, 0, 8, 42, 0, 9, 180, 0, 8, 10, 0, 8, 138, 0, 8, 74, 0, 9, 244, 80, 7, 5, 0, 8, 86, 0, 8, 22, 192, 8, 0, 83, 7, 51, 0, 8, 118, 0, 8, 54, 0, 9, 204, 81, 7, 15, 0, 8, 102, 0, 8, 38, 0, 9, 172, 0, 8, 6, 0, 8, 134, 0, 8, 70, 0, 9, 236, 80, 7, 9, 0, 8, 94, 0, 8, 30, 0, 9, 156, 84, 7, 99, 0, 8, 126, 0, 8, 62, 0, 9, 220, 82, 7, 27, 0, 8, 110, 0, 8, 46, 0, 9, 188, 0, 8, 14, 0, 8, 142, 0, 8, 78, 0, 9, 252, 96, 7, 256, 0, 8, 81, 0, 8, 17, 85, 8, 131, 82, 7, 31, 0, 8, 113, 0, 8, 49, 0, 9, 194, 80, 7, 10, 0, 8, 97, 0, 8, 33, 0, 9, 162, 0, 8, 1, 0, 8, 129, 0, 8, 65, 0, 9, 226, 80, 7, 6, 0, 8, 89, 0, 8, 25, 0, 9, 146, 83, 7, 59, 0, 8, 121, 0, 8, 57, 0, 9, 210, 81, 7, 17, 0, 8, 105, 0, 8, 41, 0, 9, 178, 0, 8, 9, 0, 8, 137, 0, 8, 73, 0, 9, 242, 80, 7, 4, 0, 8, 85, 0, 8, 21, 80, 8, 258, 83, 7, 43, 0, 8, 117, 0, 8, 53, 0, 9, 202, 81, 7, 13, 0, 8, 101, 0, 8, 37, 0, 9, 170, 0, 8, 5, 0, 8, 133, 0, 8, 69, 0, 9, 234, 80, 7, 8, 0, 8, 93, 0, 8, 29, 0, 9, 154, 84, 7, 83, 0, 8, 125, 0, 8, 61, 0, 9, 218, 82, 7, 23, 0, 8, 109, 0, 8, 45, 0, 9, 186, 
			0, 8, 13, 0, 8, 141, 0, 8, 77, 0, 9, 250, 80, 7, 3, 0, 8, 83, 0, 8, 19, 85, 8, 195, 83, 7, 35, 0, 8, 115, 0, 8, 51, 0, 9, 198, 81, 7, 11, 0, 8, 99, 0, 8, 35, 0, 9, 166, 0, 8, 3, 0, 8, 131, 0, 8, 67, 0, 9, 230, 80, 7, 7, 0, 8, 91, 0, 8, 27, 0, 9, 150, 84, 7, 67, 0, 8, 123, 0, 8, 59, 0, 9, 214, 82, 7, 19, 0, 8, 107, 0, 8, 43, 0, 9, 182, 0, 8, 11, 0, 8, 139, 0, 8, 75, 0, 9, 246, 80, 7, 5, 0, 8, 87, 0, 8, 23, 192, 8, 0, 83, 7, 51, 0, 8, 119, 0, 8, 55, 0, 9, 206, 81, 7, 15, 0, 8, 103, 0, 8, 39, 0, 9, 174, 0, 8, 7, 0, 8, 135, 0, 8, 71, 0, 9, 238, 80, 7, 9, 0, 8, 95, 0, 8, 31, 0, 9, 158, 84, 7, 99, 0, 8, 127, 0, 8, 63, 0, 9, 222, 82, 7, 27, 0, 8, 111, 0, 8, 47, 0, 9, 190, 0, 8, 15, 0, 8, 143, 0, 8, 79, 0, 9, 254, 96, 7, 256, 0, 8, 80, 0, 8, 16, 84, 8, 115, 82, 7, 31, 0, 8, 112, 0, 8, 48, 0, 9, 193, 80, 7, 10, 0, 8, 96, 0, 8, 32, 0, 9, 161, 0, 8, 0, 0, 8, 128, 0, 8, 64, 0, 9, 225, 80, 7, 6, 0, 8, 88, 0, 8, 24, 0, 9, 145, 83, 7, 59, 0, 8, 120, 0, 8, 56, 0, 9, 209, 81, 7, 17, 0, 8, 104, 0, 8, 40, 0, 9, 177, 0, 8, 8, 0, 8, 136, 0, 8, 72, 0, 9, 241, 80, 7, 4, 0, 8, 84, 0, 8, 20, 85, 8, 227, 83, 7, 43, 0, 8, 116, 0, 8, 52, 0, 9, 201, 81, 7, 13, 0, 8, 100, 0, 8, 36, 0, 9, 169, 0, 8, 4, 0, 8, 132, 0, 8, 68, 0, 9, 233, 80, 7, 8, 0, 8, 92, 0, 8, 28, 0, 9, 153, 84, 7, 83, 0, 8, 124, 0, 8, 60, 0, 9, 217, 82, 7, 23, 0, 8, 108, 0, 8, 44, 0, 9, 185, 0, 8, 12, 0, 8, 140, 0, 8, 76, 0, 9, 249, 80, 7, 3, 0, 8, 82, 0, 8, 18, 85, 8, 163, 83, 7, 35, 0, 8, 114, 0, 8, 50, 0, 9, 197, 81, 7, 11, 0, 8, 98, 0, 8, 34, 0, 9, 165, 0, 8, 2, 0, 8, 130, 0, 8, 66, 0, 9, 229, 80, 7, 7, 0, 8, 90, 0, 8, 26, 0, 9, 149, 84, 7, 67, 0, 8, 122, 0, 8, 58, 0, 9, 213, 82, 7, 19, 0, 8, 106, 0, 8, 42, 0, 9, 181, 0, 8, 10, 0, 8, 138, 0, 8, 74, 0, 9, 245, 80, 7, 5, 0, 8, 86, 0, 8, 22, 192, 8, 0, 83, 7, 51, 0, 8, 118, 0, 8, 54, 0, 9, 205, 81, 7, 15, 0, 8, 102, 0, 8, 38, 0, 9, 173, 0, 8, 6, 0, 8, 134, 0, 8, 70, 0, 9, 237, 80, 7, 9, 0, 8, 94, 0, 8, 30, 0, 9, 157, 84, 7, 99, 0, 8, 126, 0, 8, 62, 0, 9, 221, 82, 7, 27, 0, 8, 110, 0, 8, 46, 0, 9, 189, 0, 8, 
			14, 0, 8, 142, 0, 8, 78, 0, 9, 253, 96, 7, 256, 0, 8, 81, 0, 8, 17, 85, 8, 131, 82, 7, 31, 0, 8, 113, 0, 8, 49, 0, 9, 195, 80, 7, 10, 0, 8, 97, 0, 8, 33, 0, 9, 163, 0, 8, 1, 0, 8, 129, 0, 8, 65, 0, 9, 227, 80, 7, 6, 0, 8, 89, 0, 8, 25, 0, 9, 147, 83, 7, 59, 0, 8, 121, 0, 8, 57, 0, 9, 211, 81, 7, 17, 0, 8, 105, 0, 8, 41, 0, 9, 179, 0, 8, 9, 0, 8, 137, 0, 8, 73, 0, 9, 243, 80, 7, 4, 0, 8, 85, 0, 8, 21, 80, 8, 258, 83, 7, 43, 0, 8, 117, 0, 8, 53, 0, 9, 203, 81, 7, 13, 0, 8, 101, 0, 8, 37, 0, 9, 171, 0, 8, 5, 0, 8, 133, 0, 8, 69, 0, 9, 235, 80, 7, 8, 0, 8, 93, 0, 8, 29, 0, 9, 155, 84, 7, 83, 0, 8, 125, 0, 8, 61, 0, 9, 219, 82, 7, 23, 0, 8, 109, 0, 8, 45, 0, 9, 187, 0, 8, 13, 0, 8, 141, 0, 8, 77, 0, 9, 251, 80, 7, 3, 0, 8, 83, 0, 8, 19, 85, 8, 195, 83, 7, 35, 0, 8, 115, 0, 8, 51, 0, 9, 199, 81, 7, 11, 0, 8, 99, 0, 8, 35, 0, 9, 167, 0, 8, 3, 0, 8, 131, 0, 8, 67, 0, 9, 231, 80, 7, 7, 0, 8, 91, 0, 8, 27, 0, 9, 151, 84, 7, 67, 0, 8, 123, 0, 8, 59, 0, 9, 215, 82, 7, 19, 0, 8, 107, 0, 8, 43, 0, 9, 183, 0, 8, 11, 0, 8, 139, 0, 8, 75, 0, 9, 247, 80, 7, 5, 0, 8, 87, 0, 8, 23, 192, 8, 0, 83, 7, 51, 0, 8, 119, 0, 8, 55, 0, 9, 207, 81, 7, 15, 0, 8, 103, 0, 8, 39, 0, 9, 175, 0, 8, 7, 0, 8, 135, 0, 8, 71, 0, 9, 239, 80, 7, 9, 0, 8, 95, 0, 8, 31, 0, 9, 159, 84, 7, 99, 0, 8, 127, 0, 8, 63, 0, 9, 223, 82, 7, 27, 0, 8, 111, 0, 8, 47, 0, 9, 191, 0, 8, 15, 0, 8, 143, 0, 8, 79, 0, 9, 255};

        internal static readonly int[] fixed_td = new int[] { 80, 5, 1, 87, 5, 257, 83, 5, 17, 91, 5, 4097, 81, 5, 5, 89, 5, 1025, 85, 5, 65, 93, 5, 16385, 80, 5, 3, 88, 5, 513, 84, 5, 33, 92, 5, 8193, 82, 5, 9, 90, 5, 2049, 86, 5, 129, 192, 5, 24577, 80, 5, 2, 87, 5, 385, 83, 5, 25, 91, 5, 6145, 81, 5, 7, 89, 5, 1537, 85, 5, 97, 93, 5, 24577, 80, 5, 4, 88, 5, 769, 84, 5, 49, 92, 5, 12289, 82, 5, 13, 90, 5, 3073, 86, 5, 193, 192, 5, 24577 };

        // Tables for deflate from PKZIP's appnote.txt.		
        internal static readonly int[] cplens = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258, 0, 0 };

        internal static readonly int[] cplext = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0, 112, 112 };

        internal static readonly int[] cpdist = new int[] { 1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577 };

        internal static readonly int[] cpdext = new int[] { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13 };

        // If BMAX needs to be larger than 16, then h and x[] should be uLong.
        internal const int BMAX = 15; // maximum bit length of any code

        internal static int huft_build(int[] b, int bindex, int n, int s, int[] d, int[] e, int[] t, int[] m, int[] hp, int[] hn, int[] v)
        {
            // Given a list of code lengths and a maximum table size, make a set of
            // tables to decode that set of codes.  Return Z_OK on success, Z_BUF_ERROR
            // if the given code set is incomplete (the tables are still built in this
            // case), Z_DATA_ERROR if the input is invalid (an over-subscribed set of
            // lengths), or Z_MEM_ERROR if not enough memory.

            int a; // counter for codes of length k
            int[] c = new int[BMAX + 1]; // bit length count table
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
            int[] r = new int[3]; // table entry for structure assignment
            int[] u = new int[BMAX]; // table stack
            int w; // bits before this table == (l * h)
            int[] x = new int[BMAX + 1]; // bit offsets, then code stack
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
                            // (note: doesn't matter for fixed)
                            return Z_DATA_ERROR; // overflow of MANY
                        u[h] = q = hn[0]; // DEBUG
                        hn[0] += z;

                        // connect to last table, if there is one
                        if (h != 0)
                        {
                            x[h] = i; // save pattern for backing up
                            r[0] = (byte)j; // bits in this table
                            r[1] = (byte)l; // bits to dump before this table
                            j = SupportClass.URShift(i, (w - l));
                            r[2] = (int)(q - u[h - 1] - j); // offset to this table
                            Array.Copy(r, 0, hp, (u[h - 1] + j) * 3, 3); // connect to last table
                        }
                        else
                        {
                            t[0] = q; // first table is returned result
                        }
                    }

                    // set up table entry in r
                    r[1] = (byte)(k - w);
                    if (p >= n)
                    {
                        r[0] = 128 + 64; // out of values--invalid code
                    }
                    else if (v[p] < s)
                    {
                        r[0] = (byte)(v[p] < 256 ? 0 : 32 + 64); // 256 is end-of-block
                        r[2] = v[p++]; // simple code is just the value
                    }
                    else
                    {
                        r[0] = (byte)(e[v[p] - s] + 16 + 64); // non-simple--look up in lists
                        r[2] = d[v[p++] - s];
                    }

                    // fill code-like entries with r
                    f = 1 << (k - w);
                    for (j = SupportClass.URShift(i, w); j < z; j += f)
                    {
                        Array.Copy(r, 0, hp, (q + j) * 3, 3);
                    }

                    // backwards increment the k-bit code i
                    for (j = 1 << (k - 1); (i & j) != 0; j = SupportClass.URShift(j, 1))
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

        internal static int inflate_trees_bits(int[] c, int[] bb, int[] tb, int[] hp, ZStream z)
        {
            int r;
            int[] hn = new int[1]; // hufts used in space
            int[] v = new int[19]; // work area for huft_build 

            r = huft_build(c, 0, 19, 19, null, null, tb, bb, hp, hn, v);

            if (r == Z_DATA_ERROR)
            {
                z.msg = "oversubscribed dynamic bit lengths tree";
            }
            else if (r == Z_BUF_ERROR || bb[0] == 0)
            {
                z.msg = "incomplete dynamic bit lengths tree";
                r = Z_DATA_ERROR;
            }
            return r;
        }

        internal static int inflate_trees_dynamic(int nl, int nd, int[] c, int[] bl, int[] bd, int[] tl, int[] td, int[] hp, ZStream z)
        {
            int r;
            int[] hn = new int[1]; // hufts used in space
            int[] v = new int[288]; // work area for huft_build

            // build literal/length tree
            r = huft_build(c, 0, nl, 257, cplens, cplext, tl, bl, hp, hn, v);
            if (r != Z_OK || bl[0] == 0)
            {
                if (r == Z_DATA_ERROR)
                {
                    z.msg = "oversubscribed literal/length tree";
                }
                else if (r != Z_MEM_ERROR)
                {
                    z.msg = "incomplete literal/length tree";
                    r = Z_DATA_ERROR;
                }
                return r;
            }

            // build distance tree
            r = huft_build(c, nl, nd, 0, cpdist, cpdext, td, bd, hp, hn, v);

            if (r != Z_OK || (bd[0] == 0 && nl > 257))
            {
                if (r == Z_DATA_ERROR)
                {
                    z.msg = "oversubscribed distance tree";
                }
                else if (r == Z_BUF_ERROR)
                {
                    z.msg = "incomplete distance tree";
                    r = Z_DATA_ERROR;
                }
                else if (r != Z_MEM_ERROR)
                {
                    z.msg = "empty distance tree with lengths";
                    r = Z_DATA_ERROR;
                }
                return r;
            }

            return Z_OK;
        }

        internal static int inflate_trees_fixed(int[] bl, int[] bd, int[][] tl, int[][] td, ZStream z)
        {
            bl[0] = fixed_bl;
            bd[0] = fixed_bd;
            tl[0] = fixed_tl;
            td[0] = fixed_td;
            return Z_OK;
        }
    }

    sealed class StaticTree
    {
        private const int MAX_BITS = 15;

        private const int BL_CODES = 19;
        private const int D_CODES = 30;
        private const int LITERALS = 256;
        private const int LENGTH_CODES = 29;
        private static readonly int L_CODES = (LITERALS + 1 + LENGTH_CODES);

        // Bit length codes must not exceed MAX_BL_BITS bits
        internal const int MAX_BL_BITS = 7;

        internal static readonly short[] static_ltree = new short[]{12, 8, 140, 8, 76, 8, 204, 8, 44, 8, 172, 8, 108, 8, 236, 8, 28, 8, 156, 8, 92, 8, 220, 8, 60, 8, 188, 8, 124, 8, 252, 8, 2, 8, 130, 8, 66, 8, 194, 8, 34, 8, 162, 8, 98, 8, 226, 8, 18, 8, 146, 8, 82, 8, 210, 8, 50, 8, 178, 8, 114, 8, 242, 8, 10, 8, 138, 8, 74, 8, 202, 8, 42, 8, 170, 8, 106, 8, 234, 8, 26, 8, 154, 8, 90, 8, 218, 8, 58, 8, 186, 8, 122, 8, 250, 8, 6, 8, 134, 8, 70, 8, 198, 8, 38, 8, 166, 8, 102, 8, 230, 8, 22, 8, 150, 8, 86, 8, 214, 8, 54, 8, 182, 8, 118, 8, 246, 8, 14, 8, 142, 8, 78, 8, 206, 8, 46, 8, 174, 8, 110, 8, 238, 8, 30, 8, 158, 8, 94, 8, 222, 8, 62, 8, 190, 8, 126, 8, 254, 8, 1, 8, 129, 8, 65, 8, 193, 8, 33, 8, 161, 8, 97, 8, 225, 8, 17, 8, 145, 8, 81, 8, 209, 8, 49, 8, 177, 8, 113, 8, 241, 8, 9, 8, 137, 8, 73, 8, 201, 8, 41, 8, 169, 8, 105, 8, 233, 8, 25, 8, 153, 8, 89, 8, 217, 8, 57, 8, 185, 8, 121, 8, 249, 8, 5, 8, 133, 8, 69, 8, 197, 8, 37, 8, 165, 8, 101, 8, 229, 8, 21, 8, 149, 8, 85, 8, 213, 8, 53, 8, 181, 8, 117, 8, 245, 8, 13, 8, 141, 8, 77, 8, 205, 8, 45, 8, 173, 8, 109, 8, 237, 8, 29, 8, 157, 8, 93, 8, 221, 8, 61, 8, 189, 8, 125, 8, 253, 8, 19, 9, 275, 9, 147, 9, 403, 9, 83, 9, 339, 9, 211, 9, 467, 9, 51, 9, 307, 9, 179, 9, 435, 9, 115, 9, 371, 9, 243, 9, 499, 9, 11, 9, 267, 9, 139, 9, 395, 9, 75, 9, 331, 9, 203, 9, 459, 9, 43, 9, 299, 9, 171, 9, 427, 9, 107, 9, 363, 9, 235, 9, 491, 9, 27, 9, 283, 9, 155, 9, 411, 9, 91, 9, 347, 9, 219, 9, 475, 9, 59, 9, 315, 9, 187, 9, 443, 9, 123, 9, 379, 9, 251, 9, 507, 9, 7, 9, 263, 9, 135, 9, 391, 9, 71, 9, 327, 9, 199, 9, 455, 9, 39, 9, 295, 9, 167, 9, 423, 9, 103, 9, 359, 9, 231, 9, 487, 9, 23, 9, 279, 9, 151, 9, 407, 9, 87, 9, 343, 9, 215, 9, 471, 9, 55, 9, 311, 9, 183, 9, 439, 9, 119, 9, 375, 9, 247, 9, 503, 9, 15, 9, 271, 9, 143, 9, 399, 9, 79, 9, 335, 9, 207, 9, 463, 9, 47, 9, 303, 9, 175, 9, 431, 9, 111, 9, 367, 9, 239, 9, 495, 9, 31, 9, 287, 9, 159, 9, 415, 9, 95, 9, 351, 9, 223, 9, 479, 9, 63, 9, 319, 9, 191, 9, 447, 9, 127, 9, 383, 9, 255, 9, 511, 9, 0, 7, 64, 7
			, 32, 7, 96, 7, 16, 7, 80, 7, 48, 7, 112, 7, 8, 7, 72, 7, 40, 7, 104, 7, 24, 7, 88, 7, 56, 7, 120, 7, 4, 7, 68, 7, 36, 7, 100, 7, 20, 7, 84, 7, 52, 7, 116, 7, 3, 8, 131, 8, 67, 8, 195, 8, 35, 8, 163, 8, 99, 8, 227, 8};

        internal static readonly short[] static_dtree = new short[] { 0, 5, 16, 5, 8, 5, 24, 5, 4, 5, 20, 5, 12, 5, 28, 5, 2, 5, 18, 5, 10, 5, 26, 5, 6, 5, 22, 5, 14, 5, 30, 5, 1, 5, 17, 5, 9, 5, 25, 5, 5, 5, 21, 5, 13, 5, 29, 5, 3, 5, 19, 5, 11, 5, 27, 5, 7, 5, 23, 5 };

        internal static StaticTree static_l_desc;

        internal static StaticTree static_d_desc;

        internal static StaticTree static_bl_desc;

        internal short[] static_tree; // static tree or null
        internal int[] extra_bits; // extra bits for each code or null
        internal int extra_base; // base index for extra_bits
        internal int elems; // max number of elements in the tree
        internal int max_length; // max bit length for the codes

        internal StaticTree(short[] static_tree, int[] extra_bits, int extra_base, int elems, int max_length)
        {
            this.static_tree = static_tree;
            this.extra_bits = extra_bits;
            this.extra_base = extra_base;
            this.elems = elems;
            this.max_length = max_length;
        }
        static StaticTree()
        {
            static_l_desc = new StaticTree(static_ltree, Tree.extra_lbits, LITERALS + 1, L_CODES, MAX_BITS);
            static_d_desc = new StaticTree(static_dtree, Tree.extra_dbits, 0, D_CODES, MAX_BITS);
            static_bl_desc = new StaticTree(null, Tree.extra_blbits, 0, BL_CODES, MAX_BL_BITS);
        }
    }

    public class SupportClass
    {
        /// <summary>
        /// This method returns the literal value received
        /// </summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static long Identity(long literal)
        {
            return literal;
        }

        /// <summary>
        /// This method returns the literal value received
        /// </summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static ulong Identity(ulong literal)
        {
            return literal;
        }

        /// <summary>
        /// This method returns the literal value received
        /// </summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static float Identity(float literal)
        {
            return literal;
        }

        /// <summary>
        /// This method returns the literal value received
        /// </summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static double Identity(double literal)
        {
            return literal;
        }

        /*******************************/
        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static int URShift(int number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            else
                return (number >> bits) + (2 << ~bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static int URShift(int number, long bits)
        {
            return URShift(number, (int)bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static long URShift(long number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            else
                return (number >> bits) + (2L << ~bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static long URShift(long number, long bits)
        {
            return URShift(number, (int)bits);
        }

        /*******************************/
        /// <summary>Reads a number of characters from the current source Stream and writes the data to the target array at the specified index.</summary>
        /// <param name="sourceStream">The source Stream to read from.</param>
        /// <param name="target">Contains the array of characteres read from the source Stream.</param>
        /// <param name="start">The starting index of the target array.</param>
        /// <param name="count">The maximum number of characters to read from the source Stream.</param>
        /// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source Stream. Returns -1 if the end of the stream is reached.</returns>
        public static System.Int32 ReadInput(System.IO.Stream sourceStream, byte[] target, int start, int count)
        {
            // Returns 0 bytes if not enough space in target
            if (target.Length == 0)
                return 0;

            byte[] receiver = new byte[target.Length];
            int bytesRead = sourceStream.Read(receiver, start, count);

            // Returns -1 if EOF
            if (bytesRead == 0)
                return -1;

            for (int i = start; i < start + bytesRead; i++)
                target[i] = (byte)receiver[i];

            return bytesRead;
        }

        /// <summary>Reads a number of characters from the current source TextReader and writes the data to the target array at the specified index.</summary>
        /// <param name="sourceTextReader">The source TextReader to read from</param>
        /// <param name="target">Contains the array of characteres read from the source TextReader.</param>
        /// <param name="start">The starting index of the target array.</param>
        /// <param name="count">The maximum number of characters to read from the source TextReader.</param>
        /// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source TextReader. Returns -1 if the end of the stream is reached.</returns>
        public static System.Int32 ReadInput(System.IO.TextReader sourceTextReader, byte[] target, int start, int count)
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

        /// <summary>
        /// Converts a string to an array of bytes
        /// </summary>
        /// <param name="sourceString">The string to be converted</param>
        /// <returns>The new array of bytes</returns>
        public static byte[] ToByteArray(string sourceString)
        {
            return System.Text.UTF8Encoding.UTF8.GetBytes(sourceString);
        }

        /// <summary>
        /// Converts an array of bytes to an array of chars
        /// </summary>
        /// <param name="byteArray">The array of bytes to convert</param>
        /// <returns>The new array of chars</returns>
        public static char[] ToCharArray(byte[] byteArray)
        {
            return System.Text.UTF8Encoding.UTF8.GetChars(byteArray);
        }


    }

    sealed class Tree
    {
        private const int MAX_BITS = 15;
        private const int BL_CODES = 19;
        private const int D_CODES = 30;
        private const int LITERALS = 256;
        private const int LENGTH_CODES = 29;
        private static readonly int L_CODES = (LITERALS + 1 + LENGTH_CODES);
        private static readonly int HEAP_SIZE = (2 * L_CODES + 1);

        // Bit length codes must not exceed MAX_BL_BITS bits
        internal const int MAX_BL_BITS = 7;

        // end of block literal code
        internal const int END_BLOCK = 256;

        // repeat previous bit length 3-6 times (2 bits of repeat count)
        internal const int REP_3_6 = 16;

        // repeat a zero length 3-10 times  (3 bits of repeat count)
        internal const int REPZ_3_10 = 17;

        // repeat a zero length 11-138 times  (7 bits of repeat count)
        internal const int REPZ_11_138 = 18;

        // extra bits for each length code		
        internal static readonly int[] extra_lbits = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0 };

        // extra bits for each distance code		
        internal static readonly int[] extra_dbits = new int[] { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13 };

        // extra bits for each bit length code		
        internal static readonly int[] extra_blbits = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 7 };

        internal static readonly byte[] bl_order = new byte[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };


        // The lengths of the bit length codes are sent in order of decreasing
        // probability, to avoid transmitting the lengths for unused bit
        // length codes.

        internal const int Buf_size = 8 * 2;

        // see definition of array dist_code below
        internal const int DIST_CODE_LEN = 512;

        internal static readonly byte[] _dist_code = new byte[]{0, 1, 2, 3, 4, 4, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 0, 0, 16, 17, 18, 18, 19, 19, 20, 20, 20, 20, 21, 21, 21, 21, 22, 22, 22, 22, 22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23, 23, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 
			29, 29, 29, 29, 29, 29, 29, 29, 29};

        internal static readonly byte[] _length_code = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 12, 12, 13, 13, 13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16, 16, 16, 16, 16, 16, 17, 17, 17, 17, 17, 17, 17, 17, 18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19, 19, 19, 19, 19, 19, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28 };

        internal static readonly int[] base_length = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 12, 14, 16, 20, 24, 28, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 0 };

        internal static readonly int[] base_dist = new int[] { 0, 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 128, 192, 256, 384, 512, 768, 1024, 1536, 2048, 3072, 4096, 6144, 8192, 12288, 16384, 24576 };

        // Mapping from a distance to a distance code. dist is the distance - 1 and
        // must not have side effects. _dist_code[256] and _dist_code[257] are never
        // used.
        internal static int d_code(int dist)
        {
            return ((dist) < 256 ? _dist_code[dist] : _dist_code[256 + (SupportClass.URShift((dist), 7))]);
        }

        internal short[] dyn_tree; // the dynamic tree
        internal int max_code; // largest code with non zero frequency
        internal StaticTree stat_desc; // the corresponding static tree

        // Compute the optimal bit lengths for a tree and update the total bit length
        // for the current block.
        // IN assertion: the fields freq and dad are set, heap[heap_max] and
        //    above are the tree nodes sorted by increasing frequency.
        // OUT assertions: the field len is set to the optimal bit length, the
        //     array bl_count contains the frequencies for each bit length.
        //     The length opt_len is updated; static_len is also updated if stree is
        //     not null.
        internal void gen_bitlen(Deflate s)
        {
            short[] tree = dyn_tree;
            short[] stree = stat_desc.static_tree;
            int[] extra = stat_desc.extra_bits;
            int base_Renamed = stat_desc.extra_base;
            int max_length = stat_desc.max_length;
            int h; // heap index
            int n, m; // iterate over the tree elements
            int bits; // bit length
            int xbits; // extra bits
            short f; // frequency
            int overflow = 0; // number of elements with bit length too large

            for (bits = 0; bits <= MAX_BITS; bits++)
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
        internal void build_tree(Deflate s)
        {
            short[] tree = dyn_tree;
            short[] stree = stat_desc.static_tree;
            int elems = stat_desc.elems;
            int n, m; // iterate over heap elements
            int max_code = -1; // largest code with non zero frequency
            int node; // new node being created

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
                tree[node * 2] = (short)(tree[n * 2] + tree[m * 2]);
                s.depth[node] = (byte)(System.Math.Max((byte)s.depth[n], (byte)s.depth[m]) + 1);
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
            short[] next_code = new short[MAX_BITS + 1]; // next code value for each bit length
            short code = 0; // running code value
            int bits; // bit index
            int n; // code index

            // The distribution counts are first used to generate the code values
            // without bit reversal.
            for (bits = 1; bits <= MAX_BITS; bits++)
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
                tree[n * 2] = (short)(bi_reverse(next_code[len]++, len));
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
                code = SupportClass.URShift(code, 1);
                res <<= 1;
            }
            while (--len > 0);
            return SupportClass.URShift(res, 1);
        }
    }

    public class ZInputStream : BinaryReader
    {
        internal void InitBlock()
        {
            flush = zlibConst.Z_NO_FLUSH;
            buf = new byte[bufsize];
        }
        virtual public int FlushMode
        {
            get
            {
                return (flush);
            }

            set
            {
                this.flush = value;
            }

        }
        /// <summary> Returns the total number of bytes input so far.</summary>
        virtual public long TotalIn
        {
            get
            {
                return z.total_in;
            }

        }
        /// <summary> Returns the total number of bytes output so far.</summary>
        virtual public long TotalOut
        {
            get
            {
                return z.total_out;
            }

        }

        protected ZStream z = new ZStream();
        protected int bufsize = 512;
        protected int flush;
        protected byte[] buf, buf1 = new byte[1];
        protected bool compress;

        internal System.IO.Stream in_Renamed = null;

        public ZInputStream(System.IO.Stream in_Renamed)
            : base(in_Renamed)
        {
            InitBlock();
            this.in_Renamed = in_Renamed;
            z.inflateInit();
            compress = false;
            z.next_in = buf;
            z.next_in_index = 0;
            z.avail_in = 0;
        }

        public ZInputStream(System.IO.Stream in_Renamed, int level)
            : base(in_Renamed)
        {
            InitBlock();
            this.in_Renamed = in_Renamed;
            z.deflateInit(level);
            compress = true;
            z.next_in = buf;
            z.next_in_index = 0;
            z.avail_in = 0;
        }

        /*public int available() throws IOException {
        return inf.finished() ? 0 : 1;
        }*/

        public override int Read()
        {
            if (read(buf1, 0, 1) == -1)
                return (-1);
            return (buf1[0] & 0xFF);
        }

        internal bool nomoreinput = false;

        public int read(byte[] b, int off, int len)
        {
            if (len == 0)
                return (0);
            int err;
            z.next_out = b;
            z.next_out_index = off;
            z.avail_out = len;
            do
            {
                if ((z.avail_in == 0) && (!nomoreinput))
                {
                    // if buffer is empty and more input is avaiable, refill it
                    z.next_in_index = 0;
                    z.avail_in = SupportClass.ReadInput(in_Renamed, buf, 0, bufsize); //(bufsize<z.avail_out ? bufsize : z.avail_out));
                    if (z.avail_in == -1)
                    {
                        z.avail_in = 0;
                        nomoreinput = true;
                    }
                }
                if (compress)
                    err = z.deflate(flush);
                else
                    err = z.inflate(flush);
                if (nomoreinput && (err == zlibConst.Z_BUF_ERROR))
                    return (-1);
                if (err != zlibConst.Z_OK && err != zlibConst.Z_STREAM_END)
                    throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                if (nomoreinput && (z.avail_out == len))
                    return (-1);
            }
            while (z.avail_out == len && err == zlibConst.Z_OK);
            //System.err.print("("+(len-z.avail_out)+")");
            return (len - z.avail_out);
        }

        public long skip(long n)
        {
            int len = 512;
            if (n < len)
                len = (int)n;
            byte[] tmp = new byte[len];
            return ((long)SupportClass.ReadInput(BaseStream, tmp, 0, tmp.Length));
        }

        protected override void Dispose(bool disposing)
        {
            in_Renamed.Dispose();
            
            base.Dispose(disposing);
        }
    }

    sealed public class zlibConst
    {
        private const string version_Renamed_Field = "1.0.2";
        public static string version()
        {
            return version_Renamed_Field;
        }

        // compression levels
        public const int Z_NO_COMPRESSION = 0;
        public const int Z_BEST_SPEED = 1;
        public const int Z_BEST_COMPRESSION = 9;
        public const int Z_DEFAULT_COMPRESSION = (-1);

        // compression strategy
        public const int Z_FILTERED = 1;
        public const int Z_HUFFMAN_ONLY = 2;
        public const int Z_DEFAULT_STRATEGY = 0;

        public const int Z_NO_FLUSH = 0;
        public const int Z_PARTIAL_FLUSH = 1;
        public const int Z_SYNC_FLUSH = 2;
        public const int Z_FULL_FLUSH = 3;
        public const int Z_FINISH = 4;

        public const int Z_OK = 0;
        public const int Z_STREAM_END = 1;
        public const int Z_NEED_DICT = 2;
        public const int Z_ERRNO = -1;
        public const int Z_STREAM_ERROR = -2;
        public const int Z_DATA_ERROR = -3;
        public const int Z_MEM_ERROR = -4;
        public const int Z_BUF_ERROR = -5;
        public const int Z_VERSION_ERROR = -6;
    }

    public class ZOutputStream : Stream
    {
        private void InitBlock()
        {
            flush_Renamed_Field = zlibConst.Z_NO_FLUSH;
            buf = new byte[bufsize];
        }
        virtual public int FlushMode
        {
            get
            {
                return (flush_Renamed_Field);
            }

            set
            {
                this.flush_Renamed_Field = value;
            }

        }
        /// <summary> Returns the total number of bytes input so far.</summary>
        virtual public long TotalIn
        {
            get
            {
                return z.total_in;
            }

        }
        /// <summary> Returns the total number of bytes output so far.</summary>
        virtual public long TotalOut
        {
            get
            {
                return z.total_out;
            }

        }

        protected internal ZStream z = new ZStream();
        protected internal int bufsize = 4096;
        protected internal int flush_Renamed_Field;
        protected internal byte[] buf, buf1 = new byte[1];
        protected internal bool compress;

        private System.IO.Stream out_Renamed;

        public ZOutputStream(System.IO.Stream out_Renamed)
            : base()
        {
            InitBlock();
            this.out_Renamed = out_Renamed;
            z.inflateInit();
            compress = false;
        }

        public ZOutputStream(System.IO.Stream out_Renamed, int level)
            : base()
        {
            InitBlock();
            this.out_Renamed = out_Renamed;
            z.deflateInit(level);
            compress = true;
        }

        public void WriteByte(int b)
        {
            buf1[0] = (byte)b;
            Write(buf1, 0, 1);
        }
        //UPGRADE_TODO: The differences in the Expected value  of parameters for method 'WriteByte'  may cause compilation errors.  'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1092_3"'
        public override void WriteByte(byte b)
        {
            WriteByte((int)b);
        }

        public override void Write(System.Byte[] b1, int off, int len)
        {
            if (len == 0)
                return;
            int err;
            byte[] b = new byte[b1.Length];
            System.Array.Copy(b1, 0, b, 0, b1.Length);
            z.next_in = b;
            z.next_in_index = off;
            z.avail_in = len;
            do
            {
                z.next_out = buf;
                z.next_out_index = 0;
                z.avail_out = bufsize;
                if (compress)
                    err = z.deflate(flush_Renamed_Field);
                else
                    err = z.inflate(flush_Renamed_Field);
                if (err != zlibConst.Z_OK && err != zlibConst.Z_STREAM_END)
                    throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                out_Renamed.Write(buf, 0, bufsize - z.avail_out);
            }
            while (z.avail_in > 0 || z.avail_out == 0);
        }

        public virtual void finish()
        {
            int err;
            do
            {
                z.next_out = buf;
                z.next_out_index = 0;
                z.avail_out = bufsize;
                if (compress)
                {
                    err = z.deflate(zlibConst.Z_FINISH);
                }
                else
                {
                    err = z.inflate(zlibConst.Z_FINISH);
                }
                if (err != zlibConst.Z_STREAM_END && err != zlibConst.Z_OK)
                    throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                if (bufsize - z.avail_out > 0)
                {
                    out_Renamed.Write(buf, 0, bufsize - z.avail_out);
                }
            }
            while (z.avail_in > 0 || z.avail_out == 0);
            try
            {
                Flush();
            }
            catch
            {
            }
        }
        public virtual void end()
        {
            if (compress)
            {
                z.deflateEnd();
            }
            else
            {
                z.inflateEnd();
            }
            z.free();
            z = null;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                try
                {
                    finish();
                }
                catch
                {
                }
            }
            finally
            {
                end();
                out_Renamed.Close();
                out_Renamed = null;
            }
            
            base.Dispose(disposing);
        }
        
        public override void Flush()
        {
            out_Renamed.Flush();
        }
        //UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override System.Int32 Read(System.Byte[] buffer, System.Int32 offset, System.Int32 count)
        {
            return 0;
        }
        //UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override void SetLength(Int64 value)
        {
        }
        //UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override Int64 Seek(Int64 offset, System.IO.SeekOrigin origin)
        {
            return 0;
        }
        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override bool CanRead
        {
            get
            {
                return false;
            }

        }
        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override bool CanSeek
        {
            get
            {
                return false;
            }

        }
        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override bool CanWrite
        {
            get
            {
                return false;
            }

        }
        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override Int64 Length
        {
            get
            {
                return 0;
            }

        }
        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override Int64 Position
        {
            get
            {
                return 0;
            }

            set
            {
            }

        }
    }

    public class ZStreamException : IOException
    {
        public ZStreamException()
            : base()
        {
        }
        public ZStreamException(string s)
            : base(s)
        {
        }
    }
}