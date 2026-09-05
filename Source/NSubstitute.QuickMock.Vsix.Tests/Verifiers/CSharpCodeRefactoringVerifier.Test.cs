using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace NSubstitute.QuickMock.Test;

public static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
    where TCodeRefactoring : CodeRefactoringProvider, new()
{
    public class Test : CSharpCodeRefactoringTest<TCodeRefactoring, DefaultVerifier>
    {
        public Test(bool includeNSubstituteReference = true)
        {
            SolutionTransforms.Add((solution, projectId) =>
            {
                var options = solution.GetProject(projectId).CompilationOptions;
                solution = solution.WithProjectCompilationOptions(projectId,
                                                                  options.WithSpecificDiagnosticOptions(options.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings)));
                return solution;
            });
        }

        protected override string DefaultFilePathPrefix => "/0/TheTests";
        protected override string DefaultFilePath => DefaultFilePathPrefix + "." + DefaultFileExt;
    }
}
