using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace NSubstitute.QuickMock.Test;

public static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
    where TCodeRefactoring : CodeRefactoringProvider, new()
{
    public static Task VerifyRefactoringAsync(string source, string fixedSource, DiagnosticResult[] expected = null, string actionTitle = null, string fileName = null)
    {
        var test = new Test { TestCode = source, FixedCode = fixedSource, CompilerDiagnostics = CompilerDiagnostics.None };
        if (expected != null) test.ExpectedDiagnostics.AddRange(expected);
        if (actionTitle != null) test.CodeActionEquivalenceKey = actionTitle;
        if (fileName != null)
            test.TestState.Sources[0] = (fileName, test.TestState.Sources[0].content);
        else
            ChangeFileName(test.TestState);
        ChangeFileName(test.FixedState);
        return test.RunAsync();
    }

    private static void ChangeFileName(SolutionState state)
    {
        for (var i = 0; i < state.Sources.Count; i++)
        {
            var source = state.Sources[i];
            source.filename = source.filename.Replace("Tests0", "Tests");
            state.Sources[i] = source;
        }
    }
}
