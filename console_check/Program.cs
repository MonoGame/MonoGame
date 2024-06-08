using System.IO;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;

var decompilerSettings = new DecompilerSettings(LanguageVersion.CSharp5);

var decompiler = new CSharpDecompiler("Artifacts/MonoGame.Framework/ConsoleCheck/Release/net8.0/MonoGame.Framework.dll", decompilerSettings);

var buildDir = "console_check/Build/source";
if (!Directory.Exists(buildDir))
    Directory.CreateDirectory(buildDir);

var decompiled_source = decompiler.DecompileWholeModuleAsString();

var r = new Regex(@"(^[^\/\n]+)<(?!<)([a-zA-Z_0-9]+)>([a-zA-Z_0-9]+)([^\/\n]+)$", RegexOptions.Compiled | RegexOptions.Multiline);
decompiled_source = r.Replace(decompiled_source, "$1$2__$3$4");

r = new Regex(@"^\[.*: .*?\]", RegexOptions.Compiled | RegexOptions.Multiline);
decompiled_source = r.Replace(decompiled_source, string.Empty);

decompiled_source = decompiled_source.Replace("nint", "IntPtr");
decompiled_source = decompiled_source.Replace("object?", "object");

using (var fs = File.CreateText($"{buildDir}/ConsoleCheck.cs"))
    await fs.WriteAsync(decompiled_source);

