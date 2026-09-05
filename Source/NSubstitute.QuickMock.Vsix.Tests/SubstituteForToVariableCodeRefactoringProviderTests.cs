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
}
