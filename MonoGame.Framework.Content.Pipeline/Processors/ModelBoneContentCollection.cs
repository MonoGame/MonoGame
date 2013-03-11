﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    public sealed class ModelBoneContentCollection : ReadOnlyCollection<ModelBoneContent>
    {
        internal ModelBoneContentCollection(IList<ModelBoneContent> list) : base(list)
        {
        }
    }
}
