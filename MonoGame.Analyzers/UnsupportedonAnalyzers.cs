using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace MonoGame.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnsupportedOnAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MG0001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "API is not supported on this MonoGame platform",
        "{0}",
        "MonoGame.Compatibility",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startContext =>
        {
            INamedTypeSymbol unsupportedOnAttr = startContext.Compilation.GetTypeByMetadataName("MonoGame.Framework.UnsupportedOnAttribute");

            if (unsupportedOnAttr is null)
            {
                return;
            }

            startContext.RegisterOperationAction(
                ctx => AnalyzeObjectCreation(ctx, unsupportedOnAttr),
                OperationKind.ObjectCreation);
        });
    }

    private static void AnalyzeObjectCreation(OperationAnalysisContext context, INamedTypeSymbol unsupportedOnAttrSymbol)
    {
        if (context.Operation is not IObjectCreationOperation creation)
        {
            return;
        }

        IMethodSymbol ctor = creation.Constructor;
        if (ctor is null)
        {
            return;
        }

        // Read MonoGamePlatform from consuming project
        AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (!options.TryGetValue("build_property.MonoGamePlatform", out string monogamePlatform))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(monogamePlatform))
        {
            return;
        }

        AttributeData[] attrs = ctor.GetAttributes()
            .Where(a => a.AttributeClass is not null &&
                        SymbolEqualityComparer.Default.Equals(a.AttributeClass, unsupportedOnAttrSymbol))
            .ToArray();

        if (attrs.Length == 0)
        {
            return;
        }

        foreach (AttributeData attr in attrs)
        {
            if (attr.ConstructorArguments.Length < 2)
                continue;

            string platforms = attr.ConstructorArguments[0].Value as string;
            string message = attr.ConstructorArguments[1].Value as string;

            if (string.IsNullOrWhiteSpace(platforms) || string.IsNullOrWhiteSpace(message))
                continue;

            if (IsPlatformMatch(platforms, monogamePlatform))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, creation.Syntax.GetLocation(), message));
                return;
            }
        }
    }

    private static bool IsPlatformMatch(string semicolonList, string platform)
    {
        ImmutableHashSet<string> set = semicolonList
                                       .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(p => p.Trim())
                                       .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        return set.Contains(platform.Trim());
    }
}
