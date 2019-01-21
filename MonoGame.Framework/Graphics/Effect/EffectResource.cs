// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Internal helper for accessing the bytecode for stock effects.
    /// </summary>
    internal partial class EffectResource
    {
        public static readonly EffectResource AlphaTestEffect = new EffectResource(AlphaTestEffectName);
        public static readonly EffectResource BasicEffect = new EffectResource(BasicEffectName);
        public static readonly EffectResource DualTextureEffect = new EffectResource(DualTextureEffectName);
        public static readonly EffectResource EnvironmentMapEffect = new EffectResource(EnvironmentMapEffectName);
        public static readonly EffectResource SkinnedEffect = new EffectResource(SkinnedEffectName);
        public static readonly EffectResource SpriteEffect = new EffectResource(SpriteEffectName);

        private readonly object _locker = new object();
        private readonly string _name;
        private volatile byte[] _bytecode;

        private EffectResource(string name)
        {
            _name = name;
        }

        public byte[] Bytecode
        {
            get
            {
                if (_bytecode == null)
                {
                    lock (_locker)
                    {
                        if (_bytecode != null)
                            return _bytecode;

                        var assembly = ReflectionHelpers.GetAssembly(typeof(EffectResource));

                        var stream = assembly.GetManifestResourceStream(_name);
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            _bytecode = ms.ToArray();
                        }
                    }
                }

                return _bytecode;
            }
        }
    }
}
