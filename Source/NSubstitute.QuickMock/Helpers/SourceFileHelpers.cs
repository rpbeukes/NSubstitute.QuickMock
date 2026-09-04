using System;
using System.IO;

namespace NSubstitute.QuickMock.Helpers
{
    public static class SourceFileHelpers
    {
        public static bool IsTestFile(string filePath)
        {
            return !string.IsNullOrEmpty(filePath)
                && Path.GetFileName(filePath).EndsWith("Tests.cs", StringComparison.OrdinalIgnoreCase);
        }
    }
}
