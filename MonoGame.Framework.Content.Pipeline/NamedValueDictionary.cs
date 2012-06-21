#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
 All rights reserved.
 
 This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
 accept the license, do not use the software.
 
 1. Definitions
 The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
 U.S. copyright law.
 
 A "contribution" is the original software, or any additions or changes to the software.
 A "contributor" is any person that distributes its contribution under this license.
 "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 
 2. Grant of Rights
 (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 
 3. Conditions and Limitations
 (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
 your patent license from such contributor to the software ends automatically.
 (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
 notices that are present in the software.
 (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
 a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
 code form, you may only do so under a license that complies with this license.
 (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
 or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
 permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
 purpose and non-infringement.
 */
#endregion License

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    public class NamedValueDictionary<T> : IDictionary<string, T>, ICollection<KeyValuePair<string, T>>, IEnumerable<KeyValuePair<string, T>>, IEnumerable
    {
        Dictionary<string, T> dict = new Dictionary<string,T>();

        /// <summary>
        /// Initializes an instance of NamedValueDictionary.
        /// </summary>
        public NamedValueDictionary()
        {
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">Identity of the key of the new data pair.</param>
        /// <param name="value">The value of the new data pair.</param>
        public void Add(string key, T value)
        {
            dict.Add(key, value);
        }

        /// <summary>
        /// Determines whether the specified key is present in the dictionary.
        /// </summary>
        /// <param name="key">Identity of a key.</param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }

        /// <summary>
        /// Gets all keys contained in the dictionary.
        /// </summary>
        public ICollection<string> Keys
        {
            get { return dict.Keys; }
        }

        /// <summary>
        /// Removes the specified key and value from the dictionary.
        /// </summary>
        /// <param name="key">Identity of the key to be removed.</param>
        /// <returns>true if the value is present; false otherwise.</returns>
        public bool Remove(string key)
        {
            return dict.Remove(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">Identity of the key of the element whose value is to be retrieved.</param>
        /// <param name="value">The current value of the element.</param>
        /// <returns>true if the value is present; false otherwise.</returns>
        public bool TryGetValue(string key, out T value)
        {
            return TryGetValue(key, out value);
        }

        /// <summary>
        /// Specifies the type hint for the intermediate serializer. Values of this type do not store an explicit type attribute in the related XML source.
        /// </summary>
        protected internal virtual Type DefaultSerializerType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Gets all values contained in the dictionary.
        /// </summary>
        public ICollection<T> Values
        {
            get { return dict.Values; }
        }

        /// <summary>
        /// Gets or sets the specified item.
        /// </summary>
        /// <param name="key">Identity of a key.</param>
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

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        private void Add(KeyValuePair<string, T> item)
        {
            ((ICollection<KeyValuePair<string, T>>)dict).Add(item);
        }

        /// <summary>
        /// Removes all keys and values from the dictionary.
        /// </summary>
        public void Clear()
        {
            dict.Clear();
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>true if the collection contains the object; false otherwise.</returns>
        private bool Contains(KeyValuePair<string, T> item)
        {
            return ((ICollection<KeyValuePair<string, T>>)dict).Contains(item);
        }

        /// <summary>
        /// Copies the elements of the collection to an array, starting at a specified index.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The index at which to begin the copy.</param>
        private void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, T>>)dict).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of items in the dictionary.
        /// </summary>
        public int Count
        {
            get { return dict.Count; }
        }

        /// <summary>
        /// Gets a value indicating if this object is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of the specified object from the collection.
        /// </summary>
        /// <param name="item">The item to remove from the collection.</param>
        /// <returns>true if the item was successfully removed from the collection; false otherwise.</returns>
        private bool Remove(KeyValuePair<string, T> item)
        {
            return Remove(item);
        }

        /// <summary>
        /// Gets an enumerator that iterates through items in a dictionary.
        /// </summary>
        /// <returns>Enumerator for iterating through the dictionary.</returns>
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that can iterate through this collection.
        /// </summary>
        /// <returns>An enumerator that can iterate through this collection</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        /// <summary>
        /// Adds an element to the dictionary.
        /// </summary>
        /// <param name="key">Identity of the key of the new element.</param>
        /// <param name="value">The value of the new element.</param>
        protected virtual void AddItem(string key, T value)
        {
            dict.Add(key, value);
        }

        /// <summary>
        /// Removes all elements from the dictionary.
        /// </summary>
        protected virtual void ClearItems()
        {
            dict.Clear();
        }

        /// <summary>
        /// Removes the specified element from the dictionary.
        /// </summary>
        /// <param name="key">Identity of the key of the data pair to be removed.</param>
        /// <returns>true if the value is present; false otherwise.</returns>
        protected virtual bool RemoveItem(string key)
        {
            return dict.Remove(key);
        }

        /// <summary>
        /// Modifies the value of an existing element.
        /// </summary>
        /// <param name="key">Identity of the element to be modified.</param>
        /// <param name="value">The value to be set.</param>
        protected virtual void SetItem(string key, T value)
        {
            dict[key] = value;
        }
    }
}
