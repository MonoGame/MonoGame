// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// A collection of <see cref="ModelMeshPartContent"/> objects.
    /// This class cannot be inherited.
    /// </summary>
    public sealed class ModelMeshContentCollection : ReadOnlyCollection<ModelMeshContent>
    {
        internal ModelMeshContentCollection(IList<ModelMeshContent> list)
            : base(list)
        {
        }
    }
}
