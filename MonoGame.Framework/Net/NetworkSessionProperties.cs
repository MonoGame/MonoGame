using System;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkSessionProperties : IList<Nullable<int>>, ICollection<Nullable<int>>, IEnumerable<Nullable<int>>, IEnumerable
    {
        private const int Size = 8;
        private IList<Nullable<int>> list = new List<Nullable<int>>(Size);

        public NetworkSessionProperties()
        {
            for (int i = 0; i < Size; i++)
            {
                list.Add(null);
            }
        }

        internal void Send(NetBuffer buffer)
        {
            buffer.Write(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                bool isSet = list[i] != null;
                int value = isSet ? (int)list[i] : -1;

                buffer.Write(isSet);
                buffer.Write(value);
            }
        }

        internal void Receive(NetBuffer buffer)
        {
            int remoteCount = buffer.ReadInt32();
            if (remoteCount != list.Count)
            {
                throw new NetworkException("NetworkSessionProperties size mismatch, different builds?");
            }

            for (int i = 0; i < list.Count; i++)
            {
                bool isSet = buffer.ReadBoolean();
                int value = buffer.ReadInt32();
                this[i] = isSet ? value : (Nullable<int>)null;
            }
        }

        internal bool SearchMatch(NetworkSessionProperties remote)
        {
            if (list.Count != remote.list.Count)
            {
                return false;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i] != remote.list[i])
                {
                    return false;
                }
            }

            return true;
        }

        public Nullable<int> this[int index]
        {
            get
            {
                if (index < 0 || index >= list.Count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return list[index];
            }

            set
            {
                if (index < 0 || index >= list.Count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                list[index] = value;
            }
        }

        public int Count
        {
            get
            {
                int count = 0;

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public bool IsReadOnly { get { return false; } }

        public void Add(Nullable<int> item)
        {
            throw new InvalidOperationException("Use []-operator instead");
        }

        public void Clear()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = null;
            }
        }

        public bool Contains(int? item)
        {
            return false;
        }

        public void CopyTo(int?[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<int?> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(int? item)
        {
            return -1;
        }

        public void Insert(int index, int? item)
        {
            this[index] = item;
        }

        public bool Remove(int? item)
        {
            throw new InvalidOperationException("Use []-operator instead");
        }

        public void RemoveAt(int index)
        {
            this[index] = null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}