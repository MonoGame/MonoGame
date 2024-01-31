// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    public sealed class ModelMeshPartContentCollection : ReadOnlyCollection<ModelMeshPartContent>
    {
        internal ModelMeshPartContentCollection(IList<ModelMeshPartContent> list)
            : base(list)
        {
        }
    }
}
