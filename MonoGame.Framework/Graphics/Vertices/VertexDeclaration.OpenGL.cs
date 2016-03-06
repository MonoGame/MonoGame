// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

#if MONOMAC && PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#elif DESKTOPGL || (MONOMAC && !PLATFORM_MACOS_LEGACY)
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexDeclaration
    {
        Dictionary<int, VertexDeclarationAttributeInfo> shaderAttributeInfo = new Dictionary<int, VertexDeclarationAttributeInfo>();

		internal void Apply(Shader shader, IntPtr offset)
		{
            VertexDeclarationAttributeInfo attrInfo;
            int shaderHash = shader.GetHashCode();
            if (!shaderAttributeInfo.TryGetValue(shaderHash, out attrInfo))
            {
                // Get the vertex attribute info and cache it
                attrInfo = new VertexDeclarationAttributeInfo(GraphicsDevice.MaxVertexAttributes);

                foreach (var ve in InternalVertexElements)
                {
                    var attributeLocation = shader.GetAttribLocation(ve.VertexElementUsage, ve.UsageIndex);
                    // XNA appears to ignore usages it can't find a match for, so we will do the same
                    if (attributeLocation >= 0)
                    {
                        attrInfo.Elements.Add(new VertexDeclarationAttributeInfo.Element()
                        {
                            Offset = ve.Offset,
                            AttributeLocation = attributeLocation,
                            NumberOfElements = ve.VertexElementFormat.OpenGLNumberOfElements(),
                            VertexAttribPointerType = ve.VertexElementFormat.OpenGLVertexAttribPointerType(),
                            Normalized = ve.OpenGLVertexAttribNormalized(),
                        });
                        attrInfo.EnabledAttributes[attributeLocation] = true;
                    }
                }

                shaderAttributeInfo.Add(shaderHash, attrInfo);
            }

            // Apply the vertex attribute info
            foreach (var element in attrInfo.Elements)
            {
                GL.VertexAttribPointer(element.AttributeLocation,
                    element.NumberOfElements,
                    element.VertexAttribPointerType,
                    element.Normalized,
                    this.VertexStride,
                    (IntPtr)(offset.ToInt64() + element.Offset));
                GraphicsExtensions.CheckGLError();
            }
            GraphicsDevice.SetVertexAttributeArray(attrInfo.EnabledAttributes);
		}

        /// <summary>
        /// Vertex attribute information for a particular shader/vertex declaration combination.
        /// </summary>
        class VertexDeclarationAttributeInfo
        {
            internal bool[] EnabledAttributes;

            internal class Element
            {
                public int Offset;
                public int AttributeLocation;
                public int NumberOfElements;
                public VertexAttribPointerType VertexAttribPointerType;
                public bool Normalized;
            }

            internal List<Element> Elements;

            internal VertexDeclarationAttributeInfo(int maxVertexAttributes)
            {
                EnabledAttributes = new bool[maxVertexAttributes];
                Elements = new List<Element>();
            }
        }
    }
}
