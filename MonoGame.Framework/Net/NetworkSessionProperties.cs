using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Net.Backend;

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

        internal NetworkSession Session { get; set; }
        public bool IsReadOnly { get { return Session == null || !Session.IsHost; } }
        public int Count { get { return list.Count; } }

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
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Properties are read-only when not host");
                }

                if (index < 0 || index >= list.Count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                list[index] = value;
            }
        }

        internal void Pack(IOutgoingMessage msg)
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

        internal void Unpack(IIncomingMessage msg)
        {
            int remoteCount = msg.ReadInt();
            if (remoteCount != list.Count)
            {
                throw new NetworkException("NetworkSessionProperties size mismatch, different builds?");
            }

            for (int i = 0; i < list.Count; i++)
            {
                bool isSet = msg.ReadBoolean();
                int value = msg.ReadInt();
                list[i] = isSet ? value : (Nullable<int>)null;
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

        public void Clear()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Properties are read-only when not host");
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = null;
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

        public void Add(Nullable<int> item)
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