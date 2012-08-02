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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for managing a list of vertex data channels.
    /// </summary>
    public sealed class VertexChannelCollection : IList<VertexChannel>, ICollection<VertexChannel>, IEnumerable<VertexChannel>, IEnumerable
    {
        List<VertexChannel> channels;

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
                int index = IndexOf(name);
                if (index < 0)
                    throw new ArgumentException("name");
                return channels[index];
            }
            set
            {
                int index = IndexOf(name);
                if (index < 0)
                    throw new ArgumentException("name");
                channels[index] = value;
            }
        }

        /// <summary>
        /// Determines whether the collection is read-only.
        /// </summary>
        private bool ICollection<VertexChannel>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Creates an instance of VertexChannelCollection.
        /// </summary>
        internal VertexChannelCollection()
        {
            channels = new List<VertexChannel>();
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
            // Instantiate an instance of VertexChannel<> with the given elementType
            // The backtick represents the type parameter in the generic class
            var d1 = Type.GetType(typeof(VertexChannel).FullName + "`1");
            Type[] typeArgs = { typeof(ElementType) };
            var makeme = d1.MakeGenericType(typeArgs);
            VertexChannel<ElementType> channel = (VertexChannel<ElementType>)Activator.CreateInstance(makeme);
            if (channelData == null)
            {
                ((ICollection<ElementType>)channel).Add(default(ElementType));
            }
            else
            {
                foreach (ElementType element in channelData)
                    ((ICollection<ElementType>)channel).Add(element);
            }
            return channel;
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
            // Call the generic version of this method
            return (VertexChannel)GetType().GetMethod("Add").MakeGenericMethod(elementType).Invoke(this, new object[] { name, channelData });
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
        /// Determines the index of a vertex channel with the specified name.
        /// </summary>
        /// <param name="name">Name of the vertex channel being searched for.</param>
        /// <returns>Index of the vertex channel.</returns>
        public int IndexOf(string name)
        {
            for (int i = 0; i < channels.Count; ++i)
            {
                if (channels[i].Name == name)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Determines the index of the specified vertex channel.
        /// </summary>
        /// <param name="item">Vertex channel being searched for.</param>
        /// <returns>Index of the vertex channel.</returns>
        public int IndexOf(VertexChannel item)
        {
            return channels.IndexOf(item);
        }
    }
}
