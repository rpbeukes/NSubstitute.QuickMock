using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSubstitute.QuickMock.NSubstituteQuickMockCodeRefactoringProviderActions;
using NSubstitute.QuickMock.Helpers;
using NSubstitute.QuickMock.Extensions;
using System;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace NSubstitute.QuickMock
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(NSubstituteQuickMockCodeRefactoringProvider)), Shared]
    public class NSubstituteQuickMockCodeRefactoringProvider : CodeRefactoringProvider
    {
        public static string QuickMockCtorTitle = "Quick mock ctor (NSubstitute)";
        public static string MockCtorTitle = "Mock ctor (NSubstitute)";

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span, getInnermostNodeForTie: true);

            // this refactor is only available in tests files eg: "*Tests.cs"
            if (SourceFileHelpers.IsTestFile(node.SyntaxTree.FilePath))
            {
                // A Substitute.For<T>() used as a constructor argument has its own
                // refactoring; do not also offer constructor refactorings there.
                var substituteInvocation = node.AncestorsAndSelf()
                    .OfType<InvocationExpressionSyntax>()
                    .FirstOrDefault(IsSubstituteForInvocation);
                if (substituteInvocation?.Ancestors().OfType<ArgumentSyntax>()
                    .Any(argument => argument.Parent is ArgumentListSyntax list
                        && list.Parent is ObjectCreationExpressionSyntax) == true)
                    return;

                //Debug.WriteLine($"node: {node}");
                //Debug.WriteLine($"node: {node.GetType()}");
                var argumentList = node.AncestorsAndSelf().OfType<ArgumentListSyntax>().FirstOrDefault();
                if (argumentList != null)
                {
                    var isCreatingNewObject = argumentList.Parent.IsKind(SyntaxKind.ObjectCreationExpression);
                    if (isCreatingNewObject)
                    {
                        var document = context.Document;
                        var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken);
                        var objectCreationExpressionSyntax = argumentList.Parent as ObjectCreationExpressionSyntax;

                        if (objectCreationExpressionSyntax is null) return;

                        var typeInfo = semanticModel.GetTypeInfo(objectCreationExpressionSyntax);
                        var classDefinition = typeInfo.ConvertedType as INamedTypeSymbol;

                        if (classDefinition is null) return;

                        if (classDefinition.TypeKind == TypeKind.Class && classDefinition.Constructors.Any())
                        {
                            var ctorMethodSymbols = classDefinition.Constructors.Where(x => x.Parameters.IsUsable());
                            if (ctorMethodSymbols.Any())
                            {
                                var title = QuickMockCtorTitle;
                                var quickMockCtorAction = CodeAction.Create(title,
                                                                            c => NSubstituteActions.QuickMockCtor(context.Document, ctorMethodSymbols, argumentList, c),
                                                                            equivalenceKey: title);

                                title = MockCtorTitle;
                                var mockCtorAction = CodeAction.Create(title,
                                                                       c => NSubstituteActions.MockCtor(context.Document, ctorMethodSymbols, argumentList, c),
                                                                       equivalenceKey: title);

                                // Register these code actions.
                                context.RegisterRefactoring(quickMockCtorAction);
                                context.RegisterRefactoring(mockCtorAction);
                            }
                        }
                    }
                }
            }
            return;
        }

        private static bool IsSubstituteForInvocation(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression is MemberAccessExpressionSyntax member
                && member.Name.Identifier.ValueText == "For"
                && member.Expression is IdentifierNameSyntax receiver
                && receiver.Identifier.ValueText == "Substitute";
        }
    }
}
