// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    partial class VertexInputLayout
    {
        public InputElement[] GetInputElements()
        {
            var list = new List<InputElement>();
            for (int i = 0; i < Count; i++)
            {
                foreach (var vertexElement in VertexDeclarations[i].InternalVertexElements)
                {
                    var inputElement = vertexElement.GetInputElement(i, InstanceFrequencies[i]);
                    list.Add(inputElement);
                }
            }

            var inputElements = list.ToArray();

            // Fix semantics indices. (If there are more vertex declarations with, for example, 
            // POSITION0, the indices are changed to POSITION1/2/3/...)
            for (int i = 1; i < inputElements.Length; i++)
            {
                string semanticName = inputElements[i].SemanticName;
                int semanticIndex = inputElements[i].SemanticIndex;
                for (int j = 0; j < i; j++)
                {
                    if (inputElements[j].SemanticName == semanticName && inputElements[j].SemanticIndex == semanticIndex)
                    {
                        // Semantic index already used.
                        semanticIndex++;
                    }
                }

                inputElements[i].SemanticIndex = semanticIndex;
            }

            return inputElements;
        }
    }
}
