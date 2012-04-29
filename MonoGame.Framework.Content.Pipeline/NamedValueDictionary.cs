using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    public class NamedValueDictionary<T> : IDictionary<string, T>, ICollection<KeyValuePair<string, T>>, IEnumerable<KeyValuePair<string, T>>, IEnumerable
    {
        Dictionary<string, T> dict = new Dictionary<string,T>();

        public NamedValueDictionary()
        {
        }

        public void Add(string key, T value)
        {
            dict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return dict.Keys; }
        }

        public bool Remove(string key)
        {
            return dict.Remove(key);
        }

        public bool TryGetValue(string key, out T value)
        {
            return TryGetValue(key, out value);
        }

        public ICollection<T> Values
        {
            get { return dict.Values; }
        }

        public T this[string key]
        {
            get
            {
                return dict[key];
            }
            set
            {
                dict[key] = value;
            }
        }

        public void Add(KeyValuePair<string, T> item)
        {
            ((ICollection<KeyValuePair<string, T>>)dict).Add(item);
        }

        public void Clear()
        {
            dict.Clear();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return ((ICollection<KeyValuePair<string, T>>)dict).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, T>>)dict).CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return Remove(item);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }
    }
}
