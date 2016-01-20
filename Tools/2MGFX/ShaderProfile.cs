// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace TwoMGFX
{
    public abstract partial class ShaderProfile : Enumeration<ShaderProfile>
    {
        private ShaderProfile(string name, int value)
            : base(name, value)
        {
        }

        internal abstract void AddMacros(Dictionary<string, string> macros);

        internal abstract void ValidateShaderModels(PassInfo pass);

        internal abstract ShaderData CreateShader(ShaderInfo shaderInfo, string shaderFunction, string shaderProfile, bool isVertexShader, EffectObject effect, ref string errorsAndWarnings);

        internal abstract bool Supports(string platform);

        public static ShaderProfile ForPlatform(string platform)
        {
            return All.FirstOrDefault(p => p.Supports(platform));
        }

        private static void ParseShaderModel(string text, Regex regex, out int major, out int minor)
        {
            var match = regex.Match(text);
            if (!match.Success)
            {
                major = 0;
                minor = 0;
                return;
            }

            major = int.Parse(match.Groups["major"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            minor = int.Parse(match.Groups["minor"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
    }
}