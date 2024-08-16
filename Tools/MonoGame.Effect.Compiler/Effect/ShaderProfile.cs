// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Effect.TPGParser;

namespace MonoGame.Effect
{
    [TypeConverter(typeof(StringConverter))]
    public abstract class ShaderProfile
    {
        private static readonly LoadedTypeCollection<ShaderProfile> _profiles = new LoadedTypeCollection<ShaderProfile>();

        protected ShaderProfile(string name, byte formatId)
        {
            Name = name;
            FormatId = formatId;
        }

        public static readonly ShaderProfile OpenGL = FromName("OpenGL");

        public static readonly ShaderProfile DirectX_11 = FromName("DirectX_11");

        /// <summary>
        /// Returns all the loaded shader profiles.
        /// </summary>
        public static IEnumerable<ShaderProfile> All
        {
            get { return _profiles; }
        }

        /// <summary>
        /// Returns the name of the shader profile.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the format identifier used in the MGFX file format.
        /// </summary>
        public byte FormatId { get; private set; }

        /// <summary>
        /// Returns the profile by name or null if no match is found.
        /// </summary>
        public static ShaderProfile FromName(string name)
        {
            return _profiles.FirstOrDefault(p => p.Name == name);
        }

        internal abstract void AddMacros(Dictionary<string, string> macros);

        internal abstract void ValidateShaderModels(PassInfo pass);

        internal abstract ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader, EffectObject effect, ref string errorsAndWarnings);

        protected static void ParseShaderModel(string text, Regex regex, out int major, out int minor)
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

        private class StringConverter : TypeConverter
        {
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    var name = value as string;

                    foreach (var e in All)
                    {
                        if (e.Name == name)
                            return e;
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}