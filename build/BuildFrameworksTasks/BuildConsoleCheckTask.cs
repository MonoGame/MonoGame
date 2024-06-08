using System.Text.RegularExpressions;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;

namespace BuildScripts;

[TaskName("Build ConsoleCheck")]
public sealed class BuildConsoleCheckTask : FrostingTask<BuildContext>
{
    private const string MathF = """
    namespace System
    {
        internal static class MathF
        {
            public const float E = (float)Math.E;
            public const float PI = (float)Math.PI;
            public static float Sqrt(float f) { return (float)Math.Sqrt(f); }
            public static float Pow(float x, float y) { return (float)Math.Pow(x, y); }
            public static float Sin(float f) { return (float)Math.Sin(f); }
            public static float Cos(float f) { return (float)Math.Cos(f); }
            public static float Tan(float f) { return (float)Math.Tan(f); }
            public static float Asin(float f) { return (float)Math.Asin(f); }
            public static float Acos(float f) { return (float)Math.Acos(f); }
            public static float Atan(float f) { return (float)Math.Atan(f); }
            public static float Round(float f) { return (float)Math.Round(f); }
            public static float Ceiling(float f) { return (float)Math.Ceiling(f); }
            public static float Floor(float f) { return (float)Math.Floor(f); }
        }
    }
    """;

    private const string SystemNumericsVectors = """
    namespace System.Numerics
    {
        public struct Matrix4x4
        {
            public float M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44;
            public Matrix4x4(
                float m11, float m12, float m13, float m14,
                float m21, float m22, float m23, float m24,
                float m31, float m32, float m33, float m34,
                float m41, float m42, float m43, float m44)
            {
                M11 = m11; M12 = m12; M13 = m13; M14 = m14;
                M21 = m21; M22 = m22; M23 = m23; M24 = m24;
                M31 = m31; M32 = m32; M33 = m33; M34 = m34;
                M41 = m41; M42 = m42; M43 = m43; M44 = m44;
            }
        }
        public struct Plane
        {
            public Vector3 Normal;
            public float D;
            public Plane(float x, float y, float z, float d) : this(new Vector3(x, y, z), d) { }
            public Plane(Vector3 normal, float d) { Normal = normal; D = d; }
        }
        public struct Quaternion
        {
            public float X, Y, Z, W;
            public Quaternion(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
        }
        public struct Vector2
        {
            public float X, Y;
            public Vector2(float x, float y) { X = x; Y = y; }
        }
        public struct Vector3
        {
            public float X, Y, Z;
            public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
        }
        public struct Vector4
        {
            public float X, Y, Z, W;
            public Vector4(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
        }
    }
    """;

    private const string ConsoleCheckProject = """
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net452</TargetFramework>
        <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
        <LangVersion>5</LangVersion>
        <WarningLevel>1</WarningLevel>
      </PropertyGroup>
    </Project>
    """;

    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        var buildSettings = new DotNetBuildSettings
        {
            MSBuildSettings = context.DotNetBuildSettings.MSBuildSettings,
            Verbosity = context.DotNetBuildSettings.Verbosity,
            Configuration = "Release"
        };

        context.DotNetBuild(context.GetProjectPath(ProjectType.Framework, "ConsoleCheck"), buildSettings);

        var decompilerSettings = new DecompilerSettings(LanguageVersion.CSharp5)
        {
            ShowXmlDocumentation = false,
        };

        var decompiler = new CSharpDecompiler($"{context.BuildOutput}/MonoGame.Framework/ConsoleCheck/Release/net8.0/MonoGame.Framework.dll", decompilerSettings);

        var buildDir = $"{context.BuildOutput}/MonoGame.Framework/ConsoleCheck/Recompiled";
        if (!Directory.Exists(buildDir))
            Directory.CreateDirectory(buildDir);

        var decompiled_source = decompiler.DecompileWholeModuleAsString();

        decompiled_source = decompiled_source.Replace("\r\n", "\n");

        var r = new Regex(@"(.*?)<([A-Za-z0-9_]+)>([A-Za-z0-9_]+.*)", RegexOptions.Compiled | RegexOptions.Multiline);
        decompiled_source = r.Replace(decompiled_source, "$1$2_$3");

        r = new(@"^\[.*?: .*?\]\n", RegexOptions.Compiled | RegexOptions.Multiline);
        decompiled_source = r.Replace(decompiled_source, string.Empty);

        decompiled_source = decompiled_source.Replace("nint", "IntPtr");
        decompiled_source = decompiled_source.Replace("object?", "object");

        File.WriteAllText($"{buildDir}/DecompiledFramework.cs", decompiled_source);
        File.WriteAllText($"{buildDir}/MathF.cs", MathF);
        File.WriteAllText($"{buildDir}/System.Numerics.Vectors.cs", SystemNumericsVectors);

        var buildProjectFile = $"{buildDir}/MonoGame.Framework.ConsoleCheck.csproj";
        File.WriteAllText(buildProjectFile, ConsoleCheckProject);

        context.DotNetBuild(buildProjectFile, context.DotNetBuildSettings);
    }
}
