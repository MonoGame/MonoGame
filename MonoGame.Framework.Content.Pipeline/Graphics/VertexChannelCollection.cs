// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for managing a list of vertex data channels.
    /// </summary>
    public sealed class VertexChannelCollection : IList<VertexChannel>, ICollection<VertexChannel>, IEnumerable<VertexChannel>, IEnumerable
    {
        List<VertexChannel> channels;
        VertexContent vertexContent;

        /// <summary>
        /// Gets the number of vertex channels in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return channels.Count;
            }
        }

        /// <summary>
        /// Gets or sets the vertex channel at the specified index position.
        /// </summary>
        public VertexChannel this[int index]
        {
            get
            {
                return channels[index];
            }
            set
            {
                channels[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertex channel with the specified name.
        /// </summary>
        public VertexChannel this[string name]
        {
            get
            {
                var index = IndexOf(name);
                if (index < 0)
                    throw new ArgumentException("name");
                return channels[index];
            }
            set
            {
                var index = IndexOf(name);
                if (index < 0)
                    throw new ArgumentException("name");
                channels[index] = value;
            }
        }

        /// <summary>
        /// Determines whether the collection is read-only.
        /// </summary>
        bool ICollection<VertexChannel>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Creates an instance of VertexChannelCollection.
        /// </summary>
        /// <param name="vertexContent">The VertexContent object that owns this collection.</param>
        internal VertexChannelCollection(VertexContent vertexContent)
        {
            this.vertexContent = vertexContent;
            channels = new List<VertexChannel>();
             _insertOverload = GetType().GetMethods().First(m => m.Name == "Insert" && m.IsGenericMethodDefinition);
        }

        /// <summary>
        /// Adds a new vertex channel to the end of the collection.
        /// </summary>
        /// <typeparam name="ElementType">Type of the channel.</typeparam>
        /// <param name="name">Name of the new channel.</param>
        /// <param name="channelData">Initial data for the new channel. If null, the channel is filled with the default value for that type.</param>
        /// <returns>The newly added vertex channel.</returns>
        public VertexChannel<ElementType> Add<ElementType>(string name, IEnumerable<ElementType> channelData)
        {
            return Insert(channels.Count, name, channelData);
        }

        /// <summary>
        /// Adds a new vertex channel to the end of the collection.
        /// </summary>
        /// <param name="name">Name of the new channel.</param>
        /// <param name="elementType">Type of data to be contained in the new channel.</param>
        /// <param name="channelData">Initial data for the new channel. If null, the channel is filled with the default value for that type.</param>
        /// <returns>The newly added vertex channel.</returns>
        public VertexChannel Add(string name, Type elementType, IEnumerable channelData)
        {
            return Insert(channels.Count, name, elementType, channelData);
        }

        /// <summary>
        /// Removes all vertex channels from the collection.
        /// </summary>
        public void Clear()
        {
            channels.Clear();
        }

        /// <summary>
        /// Determines whether the collection contains the specified vertex channel.
        /// </summary>
        /// <param name="name">Name of the channel being searched for.</param>
        /// <returns>true if the channel was found; false otherwise.</returns>
        public bool Contains(string name)
        {
            return channels.Exists(c => { return c.Name == name; });
        }

        /// <summary>
        /// Determines whether the collection contains the specified vertex channel.
        /// </summary>
        /// <param name="item">The channel being searched for.</param>
        /// <returns>true if the channel was found; false otherwise.</returns>
        public bool Contains(VertexChannel item)
        {
            return channels.Contains(item);
        }

        /// <summary>
        /// Converts the channel, at the specified index, to another vector format.
        /// </summary>
        /// <typeparam name="TargetType">Type of the target format. Can be one of the following: Single, Vector2, Vector3, Vector4, IPackedVector</typeparam>
        /// <param name="index">Index of the channel to be converted.</param>
        /// <returns>New channel in the specified format.</returns>
        public VertexChannel<TargetType> ConvertChannelContent<TargetType>(int index)
        {
            if (index < 0 || index >= channels.Count)
                throw new ArgumentOutOfRangeException("index");

            // Get the channel at that index
            var channel = this[index];
            // Remove it because we cannot add a new channel with the same name
            RemoveAt(index);
            VertexChannel<TargetType> result = null;
            try
            {
                // Insert a new converted channel at the same index
                result = Insert(index, channel.Name, channel.ReadConvertedContent<TargetType>());
            }
            catch
            {
                // If anything went wrong, put the old channel back...
                channels.Insert(index, channel);
                // ...before throwing the exception again
                throw;
            }
            // Return the new converted channel
            return result;
        }

        /// <summary>
        /// Converts the channel, specified by name to another vector format.
        /// </summary>
        /// <typeparam name="TargetType">Type of the target format. Can be one of the following: Single, Vector2, Vector3, Vector4, IPackedVector</typeparam>
        /// <param name="name">Name of the channel to be converted.</param>
        /// <returns>New channel in the specified format.</returns>
        public VertexChannel<TargetType> ConvertChannelContent<TargetType>(string name)
        {
            var index = IndexOf(name);
            if (index < 0)
                throw new ArgumentException("name");
            return ConvertChannelContent<TargetType>(index);
        }

        /// <summary>
        /// Gets the vertex channel with the specified index and content type.
        /// </summary>
        /// <typeparam name="T">Type of a vertex channel.</typeparam>
        /// <param name="index">Index of a vertex channel.</param>
        /// <returns>The vertex channel.</returns>
        public VertexChannel<T> Get<T>(int index)
        {
            if (index < 0 || index >= channels.Count)
                throw new ArgumentOutOfRangeException("index");
            var channel = this[index];
            // Make sure the channel type is as expected
            if (channel.ElementType != typeof(T))
                throw new InvalidOperationException("Mismatched channel type");
            return (VertexChannel<T>)channel;
        }

        /// <summary>
        /// Gets the vertex channel with the specified name and content type.
        /// </summary>
        /// <typeparam name="T">Type of the vertex channel.</typeparam>
        /// <param name="name">Name of a vertex channel.</param>
        /// <returns>The vertex channel.</returns>
        public VertexChannel<T> Get<T>(string name)
        {
            var index = IndexOf(name);
            if (index < 0)
                throw new ArgumentException("name");
            return Get<T>(index);
        }

        /// <summary>
        /// Gets an enumerator that iterates through the vertex channels of a collection.
        /// </summary>
        /// <returns>Enumerator for the collection.</returns>
        public IEnumerator<VertexChannel> GetEnumerator()
        {
            return channels.GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a vertex channel with the specified name.
        /// </summary>
        /// <param name="name">Name of the vertex channel being searched for.</param>
        /// <returns>Index of the vertex channel.</returns>
        public int IndexOf(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            return channels.FindIndex((v) => v.Name == name);
        }

        /// <summary>
        /// Determines the index of the specified vertex channel.
        /// </summary>
        /// <param name="item">Vertex channel being searched for.</param>
        /// <returns>Index of the vertex channel.</returns>
        public int IndexOf(VertexChannel item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return channels.IndexOf(item);
        }

        /// <summary>
        /// Inserts a new vertex channel at the specified position.
        /// </summary>
        /// <typeparam name="ElementType">Type of the new channel.</typeparam>
        /// <param name="index">Index for channel insertion.</param>
        /// <param name="name">Name of the new channel.</param>
        /// <param name="channelData">The new channel.</param>
        /// <returns>The inserted vertex channel.</returns>
        public VertexChannel<ElementType> Insert<ElementType>(int index, string name, IEnumerable<ElementType> channelData)
        {
            if ((index < 0) || (index > channels.Count))
                throw new ArgumentOutOfRangeException("index");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            // Don't insert a channel with the same name
            if (IndexOf(name) >= 0)
                throw new ArgumentException("Vertex channel with name " + name + " already exists");
            var channel = new VertexChannel<ElementType>(name);
            if (channelData != null)
            {
                // Insert the values from the enumerable into the channel
                channel.InsertRange(0, channelData);
                // Make sure we have the right number of vertices
                if (channel.Count != vertexContent.VertexCount)
                    throw new ArgumentOutOfRangeException("channelData");
            }
            else
            {
                // Insert enough default values to fill the channel
                channel.InsertRange(0, new ElementType[vertexContent.VertexCount]);
            }
            channels.Insert(index, channel);
            return channel;
        }

        // this reference the above Insert method and is initialized in the constructor
        private readonly MethodInfo _insertOverload;

        /// <summary>
        /// Inserts a new vertex channel at the specified position.
        /// </summary>
        /// <param name="index">Index for channel insertion.</param>
        /// <param name="name">Name of the new channel.</param>
        /// <param name="elementType">Type of the new channel.</param>
        /// <param name="channelData">Initial data for the new channel. If null, it is filled with the default value.</param>
        /// <returns>The inserted vertex channel.</returns>
        public VertexChannel Insert(int index, string name, Type elementType, IEnumerable channelData)
        {
            // Call the generic version of this method
            return (VertexChannel) _insertOverload.MakeGenericMethod(elementType).Invoke(this, new object[] { index, name, channelData });
        }

        /// <summary>
        /// Removes the specified vertex channel from the collection.
        /// </summary>
        /// <param name="name">Name of the vertex channel being removed.</param>
        /// <returns>true if the channel was removed; false otherwise.</returns>
        public bool Remove(string name)
        {
            var index = IndexOf(name);
            if (index >= 0)
            {
                channels.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the specified vertex channel from the collection.
        /// </summary>
        /// <param name="item">The vertex channel being removed.</param>
        /// <returns>true if the channel was removed; false otherwise.</returns>
        public bool Remove(VertexChannel item)
        {
            return channels.Remove(item);
        }

        /// <summary>
        /// Removes the vertex channel at the specified index position.
        /// </summary>
        /// <param name="index">Index of the vertex channel being removed.</param>
        public void RemoveAt(int index)
        {
            channels.RemoveAt(index);
        }

        /// <summary>
        /// Adds a new vertex channel to the collection.
        /// </summary>
        /// <param name="item">Vertex channel to be added.</param>
        void ICollection<VertexChannel>.Add(VertexChannel item)
        {
            channels.Add(item);
        }

        /// <summary>
        /// Copies the elements of the collection to an array, starting at the specified index.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The index at which to begin copying elements.</param>
        void ICollection<VertexChannel>.CopyTo(VertexChannel[] array, int arrayIndex)
        {
            channels.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The item to insert.</param>
        void IList<VertexChannel>.Insert(int index, VertexChannel item)
        {
            channels.Insert(index, item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return channels.GetEnumerator();
        }
    }
}
