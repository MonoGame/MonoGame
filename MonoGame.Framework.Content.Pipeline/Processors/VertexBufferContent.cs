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

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Provides methods and properties for managing a design-time vertex buffer that holds packed vertex data.
    /// </summary>
    /// <remarks>This type directly corresponds to the runtime VertexBuffer class, and when a VertexBufferContent object is passed to the content compiler, the vertex data deserializes directly into a VertexBuffer at runtime. VertexBufferContent objects are not directly created by importers. The preferred method is to store vertex data in the more flexible VertexContent class.</remarks>
    public class VertexBufferContent : ContentItem
    {
        byte[] vertexData;
        VertexDeclarationContent vertexDeclarationContent;

        /// <summary>
        /// Gets the array containing the raw bytes of the packed vertex data. Use this method to get and set the contents of the vertex buffer.
        /// </summary>
        /// <value>Raw data of the packed vertex data.</value>
        public byte[] VertexData { get { return vertexData; } }

        /// <summary>
        /// Gets the associated VertexDeclarationContent object.
        /// </summary>
        /// <value>The associated VertexDeclarationContent object.</value>
        public VertexDeclarationContent VertexDeclaration { get { return vertexDeclarationContent; } set { vertexDeclarationContent = value; } }

        /// <summary>
        /// Initializes a new instance of VertexBufferContent.
        /// </summary>
        public VertexBufferContent()
        {

        }

        /// <summary>
        /// Initializes a new instance of VertexBufferContent of the specified size.
        /// </summary>
        /// <param name="size">The size of the vertex buffer content, in bytes.</param>
        public VertexBufferContent(int size)
            : base()
        {

        }

        /// <summary>
        /// Gets the size of the specified type, in bytes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The size of the specified type, in bytes.</returns>
        /// <remarks>Call this method to compute offset parameters for the Write method. If the specified data type cannot be packed into a vertex buffer—for example, if type is not a valid value type—a NotSupportedException is thrown.</remarks>
        /// <exception cref="NotSupportedException">type is not a valid value type</exception>
        public static int SizeOf(Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes additional data into the vertex buffer. Writing begins at the specified byte offset, and each value is spaced according to the specified stride value (in bytes).
        /// </summary>
        /// <typeparam name="T">Type being written.</typeparam>
        /// <param name="offset">Offset to begin writing at.</param>
        /// <param name="stride">Stride of the data being written, in bytes.</param>
        /// <param name="data">Enumerated collection of data.</param>
        /// <exception cref="NotSupportedException">The specified data type cannot be packed into a vertex buffer.</exception>
        public void Write<T>(int offset, int stride, IEnumerable<T> data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes additional data into the vertex buffer. Writing begins at the specified byte offset, and each value is spaced according to the specified stride value (in bytes).
        /// </summary>
        /// <param name="offset">Offset at which to begin writing.</param>
        /// <param name="stride">Stride of the data being written, in bytes.</param>
        /// <param name="dataType">The type of data to be written.</param>
        /// <param name="data">The data to write.</param>
        /// <exception cref="NotSupportedException">The specified data type cannot be packed into a vertex buffer.</exception>
        public void Write(int offset, int stride, Type dataType, IEnumerable data)
        {
            throw new NotSupportedException();
        }
    }
}
