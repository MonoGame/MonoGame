// Deflate.cs
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
// Time-stamp: <2011-August-03 19:52:15>
//
// ------------------------------------------------------------------
//
// This module defines logic for handling the Deflate or compression.
//
// This code is based on multiple sources:
// - the original zlib v1.2.3 source, which is Copyright (C) 1995-2005 Jean-loup Gailly.
// - the original jzlib, which is Copyright (c) 2000-2003 ymnk, JCraft,Inc.
//
// However, this code is significantly different from both.
// The object model is not the same, and many of the behaviors are different.
//
// In keeping with the license for these other works, the copyrights for
// jzlib and zlib are here.
//
// -----------------------------------------------------------------------
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

            public static Config Lookup(CompressionLevel level)
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
            int prevlen   = -1;              // last emitted length
            int curlen;                      // length of current code
            int nextlen   = tree[0 * 2 + 1]; // length of next code
            int count     = 0;               // repeat count of the current code
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
            pending[_distanceOffset + last_lit * 2] = unchecked((byte) ( (uint)dist >> 8 ) );
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
            int scan         = strstart;              // current string
            int match;                                // matched string
            int len;                                  // length of current match
            int best_len     = prev_length;           // best match length so far
            int limit        = strstart > (w_size - MIN_LOOKAHEAD) ? strstart - (w_size - MIN_LOOKAHEAD) : 0;

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
            int length = dictionary.Length;
            int index = 0;

            if (dictionary == null || status != INIT_STATE)
                throw new ZlibException("Stream error.");

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
}