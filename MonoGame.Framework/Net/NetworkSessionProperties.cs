using System;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkSessionProperties : IList<Nullable<int>>, ICollection<Nullable<int>>, IEnumerable<Nullable<int>>, IEnumerable
    {
        private const int Size = 8;
        private List<int?> list = new List<int?>(Size);
        private readonly bool isReadOnly;

        public NetworkSessionProperties()
        {
            for (int i = 0; i < Size; i++)
            {
                list.Add(null);
            }
        }

        internal NetworkSessionProperties(bool isReadOnly) : this()
        {
            this.isReadOnly = isReadOnly;
        }

        public bool IsReadOnly { get { return isReadOnly; } }
        public int Count { get { return list.Count; } }

        internal void CopyValuesFrom(NetworkSessionProperties other)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = other.list[i];
            }
        }

        internal void Pack(NetOutgoingMessage msg)
        {
            msg.Write(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                bool isSet = list[i] != null;
                int value = isSet ? (int)list[i] : -1;

                msg.Write(isSet);
                msg.Write(value);
            }
        }

        internal bool Unpack(NetBuffer msg)
        {
            int remoteCount;
            try
            {
                remoteCount = msg.ReadInt32();
            }
            catch
            {
                return false;
            }
            if (remoteCount != list.Count)
            {
                return false;
            }

            var isSet = new bool[list.Count];
            var value = new int[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    isSet[i] = msg.ReadBoolean();
                    value[i] = msg.ReadInt32();
                }
                catch
                {
                    return false;
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = isSet[i] ? value[i] : (int?)null;
            }
            return true;
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

        public void Clear()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("NetworkSessionProperties is read-only");
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = null;
            }
        }

        public int? this[int index]
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
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("NetworkSessionProperties is read-only");
                }

                if (index < 0 || index >= list.Count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                list[index] = value;
            }
        }

        public void Insert(int index, int? item)
        {
            try { this[index] = item; }
            catch { throw; }
        }

        public void RemoveAt(int index)
        {
            try { this[index] = null; }
            catch { throw; }
        }

        public void Add(int? item)
        {
            throw new InvalidOperationException("Use []-operator instead");
        }

        public bool Remove(int? item)
        {
            throw new InvalidOperationException("Use []-operator instead");
        }

        public bool Contains(int? item)
        {
            return false;
        }

        public void CopyTo(int?[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int IndexOf(int? item)
        {
            return -1;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<int?> GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
