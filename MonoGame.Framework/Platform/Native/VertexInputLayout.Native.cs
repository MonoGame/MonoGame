// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using MonoGame.Interop;


namespace Microsoft.Xna.Framework.Graphics;

partial class VertexInputLayout
{
    public void GenerateInputElements(VertexAttribute[] inputs, out MGG_InputElement[] elements, out int[] strides)
    {
        elements = new MGG_InputElement[inputs.Length];
        strides = new int[Count];

        var missingShaderInputs = false;

        // So we are looking for element matches between the REQUIRED vertex
        // shader inputs and the provided vertex declarations for this draw.
        //
        // We don't concern ourselves with the same vertex elements bound
        // to multiple shader inputs because that is useful behavior.
        //
        // We also don't worry about unused vertex declaration elements
        // as we expect the shader is what drives the rendering.  For example
        // you may use the same model data for rendering shadows which just
        // need a position and rendering lighting which need position and a normal.

        for (int i = 0; i < inputs.Length; i++)
        {
            var attr = inputs[i];

            bool found = false;

            for (int j = 0; j < Count; j++)
            {
                var declaration = VertexDeclarations[j];
                var vertexElements = declaration.InternalVertexElements;
                var instanceFrequencies = InstanceFrequencies[j];
                
                foreach (var vertexElement in vertexElements)
                {
                    if (vertexElement.VertexElementUsage == attr.usage &&
                        vertexElement.UsageIndex == attr.index)
                    {
                        found = true;
                        elements[i] = vertexElement.AsInputElement(j, instanceFrequencies);
                        strides[j] = declaration.VertexStride;
                        break;
                    }
                }

                if (found)
                    break;
            }

            if (!found)
                missingShaderInputs = true;
        }

        if (missingShaderInputs)
        {
            // TODO: This should reference the documentation for more information on this issue.

            var message =   "An error occurred while preparing to draw.\n\n" +
                            "The set VertexDeclaration does not provide all the elements " +
                            "required by the current vertex shader:\n\n\t" +
                            string.Join(",  ", inputs.Select((x) => x.ToShaderSemantic())) + "\n\n" +
                            "To fix the error change your VertexDeclaration or your Effect.";

            throw new InvalidOperationException(message);
        }
    }
}
