// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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

        public static readonly ShaderProfile Vulkan = FromName("Vulkan");

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

        protected static int RunTool(string exe, string args, out string stdout, out string stderr)
        {
            stdout = string.Empty;
            stderr = string.Empty;
            var processInfo = new ProcessStartInfo
            {
                Arguments = args,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = false,
                FileName = exe,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                process.Start();
                string stderrOutput = string.Empty;
                string stdoutOutput = string.Empty;
                var stdoutThread = new Thread(new ThreadStart(() =>
                {
                    var memory = new MemoryStream();
                    process.StandardOutput.BaseStream.CopyTo(memory);
                    var bytes = new byte[memory.Position];
                    memory.Seek(0, SeekOrigin.Begin);
                    memory.Read(bytes, 0, bytes.Length);
                    stdoutOutput = System.Text.Encoding.ASCII.GetString(bytes);
                }));
                stdoutThread.Start();

                var stderrThread = new Thread(new ThreadStart(() =>
                {
                    var memory = new MemoryStream();
                    process.StandardError.BaseStream.CopyTo(memory);
                    var bytes = new byte[memory.Position];
                    memory.Seek(0, SeekOrigin.Begin);
                    memory.Read(bytes, 0, bytes.Length);
                    stderrOutput = System.Text.Encoding.ASCII.GetString(bytes);
                }));
                stderrThread.Start();

                process.WaitForExit();

                stdoutThread.Join();
                stderrThread.Join();

                stderr = stderrOutput;
                stdout = stdoutOutput;

                return process.ExitCode;
            }
        }

        /// <summary>
        /// Safely deletes the file if it exists.
        /// </summary>
        /// <param name="filePath">The path to the file to delete.</param>
        protected static void DeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception)
            {
            }
        }
    }
}
