namespace System.Diagnostics.CodeAnalysis;

#if !NET8_0_OR_GREATER

// C# compiler matches by name and constructor signature,
// so we define our internal ExperimentalAttribute for older frameworks,
// until we move away from netstandard2.1.
[AttributeUsage(AttributeTargets.All, Inherited = false)]
internal sealed class ExperimentalAttribute(string diagnosticId) : Attribute
{
    public string DiagnosticId { get; } = diagnosticId;
    public string? UrlFormat { get; set; }
}

#endif

internal static class DiagnosticIds
{
    private const string DiagnosticIdPrefix = "mg_exp_";
    private const string BaseUrl = "https://github.com/MonoGame/MonoGame/issues/";

    internal static class PlatformConfiguration
    {
        private const string IssueId = "9382";
        public const string DiagnosticId = $"{DiagnosticIdPrefix}{IssueId}";
        public const string UrlFormat = $"{BaseUrl}{IssueId}";
    }
}
