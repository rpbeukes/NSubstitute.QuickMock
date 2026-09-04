using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSubstitute.QuickMock.Helpers
{
    public static class NSubstituteSyntaxHelpers
    {
        public static string CreateIsAnyMockString(string typeValue)
        {
            return $"Arg.Any<{typeValue}>()";
        }
    }
}
