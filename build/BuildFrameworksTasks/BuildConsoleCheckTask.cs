using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;

namespace BuildScripts;

[TaskName("Build ConsoleCheck")]
public sealed class BuildConsoleCheckTask : FrostingTask<BuildContext>
{
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

    private static readonly Regex hiddenMemberReplaceA = new(@"<([A-Za-z0-9_]*)>([A-Za-z0-9_]+)\|([A-Za-z0-9_]+)", RegexOptions.Compiled);
    private static readonly Regex hiddenMemberReplaceB = new(@"<([A-Za-z0-9_]*)>([A-Za-z0-9_]+)", RegexOptions.Compiled);
    private static readonly Regex emptyArray = new(@"Array.Empty<([A-Za-z0-9_]+)>", RegexOptions.Compiled);
    private static readonly Regex argumentNullException = new(@"ArgumentNullException\.ThrowIfNull\s+\((.*)\)", RegexOptions.Compiled);

    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        var recompileBbuildSettings = new DotNetBuildSettings
        {
            MSBuildSettings = context.DotNetBuildSettings.MSBuildSettings,
            Verbosity = context.DotNetBuildSettings.Verbosity,
            Configuration = "Release"
        };

        context.DotNetBuild(context.GetProjectPath(ProjectType.Framework, "ConsoleCheck"), recompileBbuildSettings);

        var buildDir = $"{context.BuildOutput}/MonoGame.Framework/ConsoleCheck/Recompiled";

        Recompile($"{context.BuildOutput}/MonoGame.Framework/ConsoleCheck/Release/net8.0/MonoGame.Framework.dll", buildDir, out string buildProjectFile);

        context.DotNetBuild(buildProjectFile, context.DotNetBuildSettings);
    }

    private static void Recompile(string oldAssembly, string newAssemblyDir, out string buildProjectFile)
    {
        if (!Directory.Exists(newAssemblyDir))
            Directory.CreateDirectory(newAssemblyDir);

        var decompiledSource = Decompile(oldAssembly);

        File.WriteAllText($"{newAssemblyDir}/DecompiledFramework.cs", decompiledSource.ToString());
        foreach (var file in Directory.EnumerateFiles("build/BuildFrameworksTasks/ConsoleCheckFiles"))
            File.Copy(file, $"{newAssemblyDir}/{System.IO.Path.GetFileNameWithoutExtension(file)}", true);

        using (var tupleFile = File.CreateText($"{newAssemblyDir}/ValueTuple.cs"))
            WriteValueTuples(tupleFile, 16);

        buildProjectFile = $"{newAssemblyDir}/MonoGame.Framework.csproj";
        File.WriteAllText(buildProjectFile, ConsoleCheckProject);
    }

    private static void WriteValueTuples(StreamWriter writer, int typeCount)
    {
        writer.WriteLine("namespace System");
        writer.WriteLine("{");
        for (int count = 2; count <= typeCount; count++)
        {
            writer.WriteLine($"    public struct ValueTuple<{string.Join(", ", Enumerable.Range(1, count).Select(index => $"T{index}"))}>");
            writer.WriteLine("    {");

            for (int index = 1; index <= count; index++)
                writer.WriteLine($"        public T{index} Item{index};");

            writer.WriteLine($"        public ValueTuple({string.Join(", ", Enumerable.Range(1, count).Select(index => $"T{index} t{index}"))})");
            writer.WriteLine("        {");
            for (int index = 1; index <= count; index++)
                writer.WriteLine($"            Item{index} = t{index};");
            writer.WriteLine("        }");
            writer.WriteLine("    }");
        }
        writer.WriteLine("}");
    }

    private static string Decompile(string assemblyPath)
    {
        var decompilerSettings = new DecompilerSettings(LanguageVersion.CSharp5)
        {
            ShowXmlDocumentation = false,
            NativeIntegers = false,
            NullableReferenceTypes = false,
            StringInterpolation = true,
            TupleTypes = false,
            TupleConversions = false
        };

        var decompiler = new CSharpDecompiler(assemblyPath, decompilerSettings);
        var tree = decompiler.DecompileWholeModuleAsSingleFile();

        var sb = new StringBuilder();

        foreach (var node in tree.Children)
        {
            if (node is AttributeSection)
                continue;

            if (node is NamespaceDeclaration)
            {
                foreach (var type in node.Children)

                    if (type is TypeDeclaration td)
                        FixType(td);

                sb.Append(FixEverythingInSource(node.ToString()));
                continue;
            }

            sb.Append(node);
        }

        return sb.ToString();
    }

    private static void FixType(TypeDeclaration type)
    {
        foreach (var member in type.Children)
        {
            // fix primary constructor
            if (member is ConstructorDeclaration cd &&
                cd.Body.LastChild is ExpressionStatement es &&
                es.FirstChild is InvocationExpression ie &&
                ie.FirstChild is MemberReferenceExpression mr &&
                mr.Target is BaseReferenceExpression &&
                mr.MemberNameToken.Name == ".ctor")
                cd.Body.LastChild.Remove();
        }

        FixInNode(type);
    }

    private static void FixInNode(AstNode node)
    {
        if (node is TupleAstType tuple)
            FixTuple(tuple);

        if (node is EntityDeclaration decl && decl.HasModifier(Modifiers.Readonly) &&
            decl is MethodDeclaration or PropertyDeclaration or OperatorDeclaration or IndexerDeclaration or TypeDeclaration or Accessor)
            decl.Modifiers &= ~Modifiers.Readonly;

        if (node is ComposedType nullable && nullable.HasNullableSpecifier && nullable.BaseType is SimpleType or PrimitiveType)
            FixNullable(nullable);

        if (node is InterpolatedStringExpression interExpr)
            FixInterpolatedString(interExpr);

        if (node is PropertyDeclaration pd && !pd.Initializer.IsNull)
            FixProperty(pd);

        foreach (var child in node.Children)
            FixInNode(child);
    }

    private static void FixTuple(TupleAstType tuple)
    {
        tuple.ReplaceWith(new SimpleType("ValueTuple", tuple.Elements.Select(x => x.Type.Clone())));
    }

    private static void FixNullable(ComposedType nullable)
    {
        var annotation = nullable.BaseType.Annotation<TypeResolveResult>();
        if (annotation is not null && annotation.Type.IsReferenceType == true)
            nullable.ReplaceWith(nullable.BaseType.Clone());
    }

    private static void FixInterpolatedString(InterpolatedStringExpression interpolationExpression)
    {
        var interpBuilder = new StringBuilder();
        var interpolations = new List<Expression>();

        int index = 0;
        foreach (var content in interpolationExpression.Content)
        {
            if (content is Interpolation interp)
            {
                interpBuilder.Append($"{{{index}");
                if (interp.Alignment != 0)
                    interpBuilder.Append($",{interp.Alignment}");
                if (!string.IsNullOrEmpty(interp.Suffix))
                    interpBuilder.Append($":{interp.Suffix}");
                interpBuilder.Append('}');

                interpolations.Add(interp.Expression.Clone());
                index++;
            }
            else if (content is InterpolatedStringText text)
                interpBuilder.Append(text.Text);
        }

        interpolations.Insert(0, new PrimitiveExpression(interpBuilder.ToString(), LiteralFormat.StringLiteral));

        interpolationExpression.ReplaceWith(
            new InvocationExpression(new MemberReferenceExpression(new TypeReferenceExpression(new PrimitiveType("string")), "Format"), interpolations));
    }

    private static void FixProperty(PropertyDeclaration pd)
    {
        var field = new FieldDeclaration { Modifiers = Modifiers.Private, ReturnType = pd.ReturnType.Clone() };
        if (pd.HasModifier(Modifiers.Static))
            field.Modifiers |= Modifiers.Static;

        var backingField = new VariableInitializer($"__{pd.Name}__backing_field", pd.Initializer.Clone());
        field.Variables.Add(backingField);

        pd.Initializer = Expression.Null;
        pd.Parent!.InsertChildBefore(pd, field, Roles.TypeMemberRole);

        var backingFieldRef = new IdentifierExpression(backingField.Name);
        pd.Getter.Body = [new ReturnStatement(backingFieldRef)];
        pd.Setter.Body = [new ExpressionStatement(new AssignmentExpression(backingFieldRef.Clone(), new IdentifierExpression("value")))];
    }

    private static string FixEverythingInSource(string source)
    {
        source = hiddenMemberReplaceA.Replace(source, "__$1_$2_$3");
        source = hiddenMemberReplaceB.Replace(source, "__$1_$2");
        source = emptyArray.Replace(source, "ArrayHelper<$1>.Empty");
        source = argumentNullException.Replace(source, "ArgumentNullExceptionHelper.ThrowIfNull($1)");
        source = source.Replace("StringBuilder.AppendInterpolatedStringHandler", "StringBuilderHelper.AppendInterpolatedStringHandler");

        source = source.Replace("nint", "IntPtr");

        return source;
    }
}
