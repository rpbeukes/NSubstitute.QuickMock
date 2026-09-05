using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using NSubstitute.QuickMock.Helpers;
using System;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace NSubstitute.QuickMock
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SubstituteForToVariableCodeRefactoringProvider)), Shared]
    public sealed class SubstituteForToVariableCodeRefactoringProvider : CodeRefactoringProvider
    {
        public const string Title = "Substitute.For<T> to variable (NSubstitute)";

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var invocation = FindSubstituteForInvocation(root, context.Span);

            if (invocation == null || !SourceFileHelpers.IsTestFile(root.SyntaxTree.FilePath))
                return;

            var member = invocation?.Expression as MemberAccessExpressionSyntax;

            var receiver = member?.Expression as IdentifierNameSyntax;

            var currentArgument = invocation?.Parent as ArgumentSyntax
                ?? invocation?.Ancestors().OfType<ArgumentSyntax>().FirstOrDefault();

            var argumentList = currentArgument?.Parent as ArgumentListSyntax
                ?? currentArgument?.Ancestors().OfType<ArgumentListSyntax>().FirstOrDefault();

            var creation = argumentList?.Parent as ObjectCreationExpressionSyntax
                ?? argumentList?.Ancestors().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();

            if (member == null || member.Name.Identifier.ValueText != "For"
                || receiver == null || receiver.Identifier.ValueText != "Substitute"
                || currentArgument == null || argumentList == null || creation == null)
                return;

            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var method = model.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;

            if (method != null && (method.ContainingType?.Name != "Substitute"
                || method.ContainingNamespace?.ToDisplayString() != "NSubstitute"
                || !method.IsGenericMethod))
                return;

            var constructor = model.GetSymbolInfo(creation, context.CancellationToken).Symbol as IMethodSymbol;

            if (constructor == null || !constructor.MethodKind.Equals(MethodKind.Constructor))
            {
                var targetType = model.GetTypeInfo(creation, context.CancellationToken).ConvertedType as INamedTypeSymbol;
                constructor = targetType?.Constructors
                    .FirstOrDefault(c => c.Parameters.Length == argumentList.Arguments.Count);
            }

            if (constructor == null)
                return;

            var position = argumentList.Arguments.IndexOf(currentArgument);

            if (position < 0 || position >= constructor.Parameters.Length)
                return;

            var parameter = currentArgument.NameColon == null
                ? constructor.Parameters[position]
                : constructor.Parameters.FirstOrDefault(p =>
                    string.Equals(p.Name, currentArgument.NameColon.Name.Identifier.ValueText, StringComparison.Ordinal));

            if (parameter == null)
                return;

            context.RegisterRefactoring(CodeAction.Create(Title,
                                                          cancellationToken => ApplyAsync(context.Document, invocation, argumentList, parameter, model, cancellationToken),
                                                          equivalenceKey: Title));
        }

        private static InvocationExpressionSyntax FindSubstituteForInvocation(SyntaxNode root, Microsoft.CodeAnalysis.Text.TextSpan span)
        {
            var node = root.FindNode(span, getInnermostNodeForTie: true);
            var invocation = node.AncestorsAndSelf()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(IsSubstituteForInvocation);
            if (invocation != null)
                return invocation;

            if (span.IsEmpty)
            {
                var token = root.FindToken(span.Start, findInsideTrivia: true);
                invocation = token.Parent?.AncestorsAndSelf()
                    .OfType<InvocationExpressionSyntax>()
                    .FirstOrDefault(IsSubstituteForInvocation);
                if (invocation != null)
                    return invocation;
            }

            return root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(IsSubstituteForInvocation)
                .Where(candidate => candidate.Span.Contains(span)
                    || (!span.IsEmpty && candidate.Span.IntersectsWith(span)))
                .OrderBy(candidate => candidate.Span.Length)
                .FirstOrDefault();
        }

        private static bool IsSubstituteForInvocation(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression is MemberAccessExpressionSyntax member
                && member.Name.Identifier.ValueText == "For"
                && member.Expression is IdentifierNameSyntax receiver
                && receiver.Identifier.ValueText == "Substitute";
        }

        private static async Task<Document> ApplyAsync(Document document,
                                                       InvocationExpressionSyntax invocation,
                                                       ArgumentListSyntax argumentList,
                                                       IParameterSymbol parameter,
                                                       SemanticModel model,
                                                       System.Threading.CancellationToken cancellationToken)
        {
            var statement = invocation.Ancestors().OfType<LocalDeclarationStatementSyntax>().FirstOrDefault();
            if (statement == null)
                return document;

            var baseName = parameter.Name + "Mock";
            var name = baseName;

            for (var i = 2; IsNameUsedInScope(model, statement, name); i++)
                name = baseName + i;

            var typeSyntax = invocation.Expression is MemberAccessExpressionSyntax access && access.Name is GenericNameSyntax generic
                           ? generic.TypeArgumentList.ToString()
                           : null;

            if (typeSyntax == null)
                return document;

            var declaration = SyntaxFactory.ParseStatement($"var {name} = Substitute.For{typeSyntax}();")
                                           .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed)
                                           .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

            var replacement = SyntaxFactory.IdentifierName(name).WithTriviaFrom(invocation);

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            editor.InsertBefore(statement, declaration);
            editor.ReplaceNode(invocation, replacement);

            return editor.GetChangedDocument();
        }

        private static bool IsNameUsedInScope(SemanticModel model, LocalDeclarationStatementSyntax statement, string name)
        {
            return model.LookupSymbols(statement.SpanStart, name: name).Any();
        }
    }
}
