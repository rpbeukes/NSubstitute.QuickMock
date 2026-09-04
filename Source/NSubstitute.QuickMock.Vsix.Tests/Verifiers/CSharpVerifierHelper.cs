using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NSubstitute.QuickMock.Test;

internal static class CSharpVerifierHelper
{
    internal static ImmutableDictionary<string, ReportDiagnostic> NullableWarnings { get; } = GetNullableWarnings();

    private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarnings()
    {
        var options = CSharpCommandLineParser.Default.Parse(
            new[] { "/warnaserror:nullable" },
            Environment.CurrentDirectory,
            Environment.CurrentDirectory).CompilationOptions.SpecificDiagnosticOptions;

        return options.SetItem("CS8632", ReportDiagnostic.Error)
                      .SetItem("CS8669", ReportDiagnostic.Error);
    }
}
