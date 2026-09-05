using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;

namespace NSubstitute.QuickMock.Test;

public static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
    where TCodeRefactoring : CodeRefactoringProvider, new()
{
    public static Task VerifyRefactoringAsync(string source,
                                              string fixedSource,
                                              DiagnosticResult[] expected = null,
                                              string actionTitle = null,
                                              string fileName = null,
                                              bool includeNSubstituteReference = true)
    {
        var test = new Test(includeNSubstituteReference) { TestCode = source, FixedCode = fixedSource, CompilerDiagnostics = CompilerDiagnostics.None };
        if (includeNSubstituteReference)
        {
            const string nSubstituteApi = @"
namespace NSubstitute
{
    public static class Substitute
    {
        public static T For<T>() => default(T);
    }

    public static class Arg
    {
        public static T Any<T>() => default(T);
    }
}";
            test.TestState.Sources.Add(("NSubstituteReference.cs", nSubstituteApi));
            test.FixedState.Sources.Add(("NSubstituteReference.cs", nSubstituteApi));
        }
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
