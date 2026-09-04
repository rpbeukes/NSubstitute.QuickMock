using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSubstitute.QuickMock.Helpers
{
    public class UpdateEditorHelpers
    {
        public static async Task<DocumentEditor> ReplaceArguments(Document document, ArgumentListSyntax location, List<string> newArgsList)
        {
            var editor = await DocumentEditor.CreateAsync(document);

            var modifiedArguments = new SeparatedSyntaxList<ArgumentSyntax>();

            foreach (var arg in newArgsList)
            {
                var expression = SyntaxFactory.ParseExpression(arg)
                    .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
                modifiedArguments = modifiedArguments.Add(SyntaxFactory.Argument(expression));
            }

            var modifiedArgumentList = SyntaxFactory.ArgumentList(modifiedArguments);
            
            editor.ReplaceNode(location, modifiedArgumentList);
            return editor;
        }
    }
}
