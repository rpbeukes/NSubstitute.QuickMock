using Microsoft.CodeAnalysis;
using NSubstitute.QuickMock.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace NSubstitute.QuickMock.Extensions
{
    public static class ParameterSymbolExtensions
    {
        public static bool IsUsable(this IParameterSymbol parameter)
        {
            return parameter.Type.SpecialType == SpecialType.System_String
                || parameter.Type.IsReferenceType
                || parameter.Type.IsValueType;
        }

        public static bool IsUsable(this ImmutableArray<IParameterSymbol> parameters)
        {
            return parameters.Length > 0 && parameters.All(IsUsable);
        }

        public static void FindReferenceAndValueTypes(this ImmutableArray<IParameterSymbol> ctorWithMostParameters,
                                                      Action<IParameterSymbol> onFoundReferenceType,
                                                      Action<IParameterSymbol, string> onFoundValueType,
                                                      Action<IParameterSymbol, string> onTypeNotIdentified = null)
        {
            foreach (var paramSymbol in ctorWithMostParameters)
            {
                var paramType = paramSymbol.Type;
                var isString = paramType.Name.Equals("string", StringComparison.OrdinalIgnoreCase);

                if (!isString && paramType.IsReferenceType)
                    onFoundReferenceType(paramSymbol);
                else if (isString || paramType.IsValueType)
                {
                    var suggestedArgumentText = NSubstituteSyntaxHelpers.CreateIsAnyMockString(paramType.ToString());
                    onFoundValueType(paramSymbol, suggestedArgumentText);
                }
                else
                    // dont know what to do here, user should look closer and fix.
                    onTypeNotIdentified?.Invoke(paramSymbol, " ");
            }
        }
    }
}
