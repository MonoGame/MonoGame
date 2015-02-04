// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace MonoGame.Utilities
{
    public class MemoryPool: IComparer<byte[]>
    {
        private static MemoryPool _instance;
        public int MinimumBufferSize = 2097152;
        private bool _enable;        
        private SortedSet<byte[]> bufferSet;
        
        public bool Enable
        {
            get { return _enable; }
            set { if (!value) Clear(); _enable = value; }
        }
        
        public static MemoryPool Current
        {
            get 
            {
                if (_instance == null) _instance = new MemoryPool();
                return _instance;
            }
        }
        
        private MemoryPool()
        {
            bufferSet = new SortedSet<byte[]>(this);
            Enable = true;
        }

        public byte[] GetPooledBuffer(int size)
        {   
            byte[] buffer = null;
            lock (bufferSet)
            {
                foreach (var tmpbuffer in bufferSet)
                {
                    if (tmpbuffer.Length >= size)
                    {
                        bufferSet.Remove(tmpbuffer);
                        buffer = tmpbuffer;
                        break;
                    }
                }
            }
            if (buffer == null)
            {
                int dataSize = Math.Max(MinimumBufferSize, size);
                buffer = new byte[dataSize];
            }
            return buffer;
        }
        
        internal void PoolBuffer(byte[] buffer)
        {
            if (!_enable) return;
            lock (bufferSet)
                bufferSet.Add(buffer);
        }
        
        private void Clear()
        {
             foreach (var tmpbuffer in bufferSet)
                bufferSet.Remove(tmpbuffer);
        }

        public int Compare(byte[] x, byte[] y)
        {
                return (x.Length - y.Length);
        }
    }
}
