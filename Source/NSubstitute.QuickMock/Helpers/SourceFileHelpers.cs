using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace NSubstitute.QuickMock.Helpers
{
    public static class SourceFileHelpers
    {
        public static bool IsTestFile(string filePath)
        {
            return !string.IsNullOrEmpty(filePath)
                && Path.GetFileName(filePath).EndsWith("Tests.cs", StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasNSubstituteReference(Compilation compilation)
        {
            return compilation?.GetTypeByMetadataName("NSubstitute.Substitute") != null;
        }
    }
}
