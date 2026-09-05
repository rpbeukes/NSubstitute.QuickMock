using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = NSubstitute.QuickMock.Test.CSharpCodeRefactoringVerifier<NSubstitute.QuickMock.SubstituteForToVariableCodeRefactoringProvider>;

namespace NSubstitute.QuickMock.Tests;

[TestClass]
public class SubstituteForToVariableCodeRefactoringProviderTests
{
    [TestMethod]
    public async Task ConvertsSelectedSubstituteForToVariable()
    {
        var source = @"
using NSubstitute;
namespace DemoProject.Tests
{
    public interface ICurrentUser { }

    public class DemoClassOnly
    {
        public DemoClassOnly(ICurrentUser user) { }
    }

    public class DemoClassOnlyTests
    {
        public void Test()
        {
            var systemUnderTest = new DemoClassOnly([|Substitute|].For<ICurrentUser>());
        }
    }
}";

        var fixedSource = @"
using NSubstitute;
namespace DemoProject.Tests
{
    public interface ICurrentUser { }

    public class DemoClassOnly
    {
        public DemoClassOnly(ICurrentUser user) { }
    }

    public class DemoClassOnlyTests
    {
        public void Test()
        {
            var userMock = Substitute.For<ICurrentUser>();
            var systemUnderTest = new DemoClassOnly(userMock);
        }
    }
}";

        await VerifyCS.VerifyRefactoringAsync(
            source,
            fixedSource,
            actionTitle: SubstituteForToVariableCodeRefactoringProvider.Title);
    }

    [TestMethod]
    public async Task UsesBaseMockNameWhenSameNameExistsInAnotherMethod()
    {
        var source = @"
using NSubstitute;
namespace DemoProject.Tests
{
    public interface ILogger<T> { }

    public class DemoClassOnly
    {
        public DemoClassOnly(ILogger<DemoClassOnly> logger) { }
    }

    public class DemoClassOnlyTests
    {
        public void QuickMockCtor_NSubstitute()
        {
            var loggerMock = Substitute.For<ILogger<DemoClassOnly>>();
            var systemUnderTest = new DemoClassOnly(loggerMock);
        }

        public void SubstituteForToVariable_NSubstitute()
        {
            var systemUnderTest = new DemoClassOnly([|Substitute|].For<ILogger<DemoClassOnly>>());
        }
    }
}";

        var fixedSource = @"
using NSubstitute;
namespace DemoProject.Tests
{
    public interface ILogger<T> { }

    public class DemoClassOnly
    {
        public DemoClassOnly(ILogger<DemoClassOnly> logger) { }
    }

    public class DemoClassOnlyTests
    {
        public void QuickMockCtor_NSubstitute()
        {
            var loggerMock = Substitute.For<ILogger<DemoClassOnly>>();
            var systemUnderTest = new DemoClassOnly(loggerMock);
        }

        public void SubstituteForToVariable_NSubstitute()
        {
            var loggerMock = Substitute.For<ILogger<DemoClassOnly>>();
            var systemUnderTest = new DemoClassOnly(loggerMock);
        }
    }
}";

        await VerifyCS.VerifyRefactoringAsync(
            source,
            fixedSource,
            actionTitle: SubstituteForToVariableCodeRefactoringProvider.Title);
    }

    [TestMethod]
    public async Task ReusesVariableNameWhenMatchingLocalIsOutsideCurrentMethodScope()
    {
        var source = @"
using NSubstitute;
namespace DemoProject.Tests
{
    public interface ILogger { }

    public class DemoClassOnly
    {
        public DemoClassOnly(ILogger logger) { }
    }

    public class DemoClassOnlyTests
    {
        public void FirstTest()
        {
            var loggerMock = Substitute.For<ILogger>();
        }

        public void SecondTest()
        {
            var systemUnderTest = new DemoClassOnly([|Substitute|].For<ILogger>());
        }
    }
}";

        var fixedSource = @"
using NSubstitute;
namespace DemoProject.Tests
{
    public interface ILogger { }

    public class DemoClassOnly
    {
        public DemoClassOnly(ILogger logger) { }
    }

    public class DemoClassOnlyTests
    {
        public void FirstTest()
        {
            var loggerMock = Substitute.For<ILogger>();
        }

        public void SecondTest()
        {
            var loggerMock = Substitute.For<ILogger>();
            var systemUnderTest = new DemoClassOnly(loggerMock);
        }
    }
}";

        await VerifyCS.VerifyRefactoringAsync(
            source,
            fixedSource,
            actionTitle: SubstituteForToVariableCodeRefactoringProvider.Title);
    }

    [TestMethod]
    public async Task AvoidsVariableNameUsedLaterInSameMethodScope()
    {
        var source = @"
using NSubstitute;
namespace DemoProject.Tests
{
    public interface ILogger { }

    public class DemoClassOnly
    {
        public DemoClassOnly(ILogger logger) { }
    }

    public class DemoClassOnlyTests
    {
        public void Test()
        {
            var systemUnderTest = new DemoClassOnly([|Substitute|].For<ILogger>());
            var loggerMock = Substitute.For<ILogger>();
        }
    }
}";

        var fixedSource = @"
using NSubstitute;
namespace DemoProject.Tests
{
    public interface ILogger { }

    public class DemoClassOnly
    {
        public DemoClassOnly(ILogger logger) { }
    }

    public class DemoClassOnlyTests
    {
        public void Test()
        {
            var loggerMock2 = Substitute.For<ILogger>();
            var systemUnderTest = new DemoClassOnly(loggerMock2);
            var loggerMock = Substitute.For<ILogger>();
        }
    }
}";

        await VerifyCS.VerifyRefactoringAsync(
            source,
            fixedSource,
            actionTitle: SubstituteForToVariableCodeRefactoringProvider.Title);
    }

    [TestMethod]
    public async Task IsUnavailableWithoutNSubstitutePackageReference()
    {
        var source = @"
namespace DemoProject.Tests
{
    public interface ICurrentUser { }

    public class DemoClassOnly
    {
        public DemoClassOnly(ICurrentUser user) { }
    }

    public class DemoClassOnlyTests
    {
        public void Test()
        {
            var systemUnderTest = new DemoClassOnly([|Substitute.For<ICurrentUser>()|]);
        }
    }
}";

        await VerifyCS.VerifyRefactoringAsync(
            source,
            source.Replace("[|", string.Empty).Replace("|]", string.Empty),
            includeNSubstituteReference: false);
    }
}
