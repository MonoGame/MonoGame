// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

#if WINRT
using System.Reflection;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Internal helper for accessing the bytecode for stock effects.
    /// </summary>
    internal partial class EffectResources
    {
        private static byte[] LoadResource(string name)
        {
#if WINRT
            var assembly = typeof(EffectResources).GetTypeInfo().Assembly;
#else
            var assembly = typeof(EffectResources).Assembly;
#endif
            var stream = assembly.GetManifestResourceStream(name);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
