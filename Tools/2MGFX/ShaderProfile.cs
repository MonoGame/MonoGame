// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TwoMGFX
{
    [TypeConverter(typeof(StringTypeConverter))]
    public abstract partial class ShaderProfile
    {
        private static readonly List<ShaderProfile> _profiles = new List<ShaderProfile>();

        private readonly string _name;

        private readonly int _value;

        private ShaderProfile(string name, int value)
        {
            _name = name;
            _value = value;
            _profiles.Add(this);
        }

        public static explicit operator int(ShaderProfile profile)
        {
            return profile._value;
        }

        internal abstract void AddMacros(Dictionary<string, string> macros);

        internal abstract void ValidateShaderModels(PassInfo pass);

        internal abstract byte[] CompileShader(ShaderInfo shaderInfo, string shaderFunction, string shaderProfile, ref string errorsAndWarnings);

        internal abstract ShaderData CreateShader(byte[] byteCode, bool isVertexShader, List<ConstantBufferData> cbuffers, int sharedIndex, Dictionary<string, SamplerStateInfo> samplerStates, bool debug);

        internal abstract bool Supports(string platform);

        public static List<string> GetProfileNames()
        {
            var names = new List<string>();

            foreach (var profile in _profiles)
                names.Add(profile.ToString());

            return names;
        }

        public static ShaderProfile FindProfile(string platform)
        {
            foreach (var profile in _profiles)
            {
                if (profile.Supports(platform))
                    return profile;
            }

            return null;
        }

        public override string ToString()
        {
            return _name;
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


        private class StringTypeConverter : TypeConverter
        {
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    foreach (var profile in _profiles)
                    {
                        if (profile._name == (value as string))
                            return profile;
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}