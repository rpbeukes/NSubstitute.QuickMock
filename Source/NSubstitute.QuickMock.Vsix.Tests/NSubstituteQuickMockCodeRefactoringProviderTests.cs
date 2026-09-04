using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute.QuickMock;
using VerifyCS = NSubstitute.QuickMock.Test.CSharpCodeRefactoringVerifier<
    NSubstitute.QuickMock.NSubstituteQuickMockCodeRefactoringProvider>;
using VerifySubstituteForToVariable = NSubstitute.QuickMock.Test.CSharpCodeRefactoringVerifier<
    NSubstitute.QuickMock.SubstituteForToVariableCodeRefactoringProvider>;

namespace NSubstitute.QuickMock.Tests;

[TestClass]
public class NSubstituteQuickMockCodeRefactoringProviderTests
{
    private const string Template = @"
using System;
using NSubstitute;
namespace DemoProject.Tests
{
    public interface ILogger<T> { }
    public interface ICurrentUser { }
    public interface IValidator<T> { }
    public class SomeCommand { }
    public class InvoiceDetailsInput { }
    public class DemoClassOnly
    {
        public DemoClassOnly(ILogger<DemoClassOnly> logger, string text, int count, int? optional,
            ICurrentUser user, Func<SomeCommand> commandFactory,
            Func<IValidator<InvoiceDetailsInput>> validatorFactory) { }
    }
    public class DemoClassOnlyTests
    {
        public void Test()
        {
            var systemUnderTest = [|{0}|];
        }
    }
}";

    [TestMethod]
    public async Task QuickMockCtor_UsesSubstitutesAndArgAnyForMixedParameters()
    {
        var start = Template.Replace("[|{0}|]", "new DemoClassOnly[|()|]");
        var fixedCode = Template.Replace("[|{0}|]",
            "new DemoClassOnly(Substitute.For<ILogger<DemoClassOnly>>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int?>(), Substitute.For<ICurrentUser>(), Substitute.For<Func<SomeCommand>>(), Substitute.For<Func<IValidator<InvoiceDetailsInput>>>())");
        await VerifyCS.VerifyRefactoringAsync(start, fixedCode,
            null,
            NSubstituteQuickMockCodeRefactoringProvider.QuickMockCtorTitle);
    }

    [TestMethod]
    public async Task MockCtor_DeclaresSubstitutesAndUsesVariables()
    {
        var start = Template.Replace("[|{0}|]", "new DemoClassOnly[|()|]");
        var fixedCode = Template.Replace("            var systemUnderTest = [|{0}|];",
            "            var loggerMock = Substitute.For<ILogger<DemoClassOnly>>();\r\n" +
            "            var userMock = Substitute.For<ICurrentUser>();\r\n" +
            "            var commandFactoryMock = Substitute.For<Func<SomeCommand>>();\r\n" +
            "            var validatorFactoryMock = Substitute.For<Func<IValidator<InvoiceDetailsInput>>>();\r\n" +
            "            var systemUnderTest = new DemoClassOnly(loggerMock, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int?>(), userMock, commandFactoryMock, validatorFactoryMock);");
        await VerifyCS.VerifyRefactoringAsync(start, fixedCode,
            null,
            NSubstituteQuickMockCodeRefactoringProvider.MockCtorTitle);
    }

    [TestMethod]
    public async Task QuickMockCtor_IsAvailableWithCaretInsideEmptyArgumentList()
    {
        var start = Template.Replace("[|{0}|]", "new DemoClassOnly([||])");
        var fixedCode = Template.Replace("[|{0}|]",
            "new DemoClassOnly(Substitute.For<ILogger<DemoClassOnly>>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int?>(), Substitute.For<ICurrentUser>(), Substitute.For<Func<SomeCommand>>(), Substitute.For<Func<IValidator<InvoiceDetailsInput>>>())");
        await VerifyCS.VerifyRefactoringAsync(start, fixedCode,
            null,
            NSubstituteQuickMockCodeRefactoringProvider.QuickMockCtorTitle);
    }

    [TestMethod]
    public async Task ConstructorRefactorings_AreNotAvailableOnSubstituteForConstructorArgument()
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

        var fixedSource = source.Replace("[|", string.Empty).Replace("|]", string.Empty);
        await VerifyCS.VerifyRefactoringAsync(source, fixedSource);
    }

    [TestMethod]
    public async Task SubstituteForArgument_OffersOnlyVariableRefactoring()
    {
        var argumentSource = @"
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
            var systemUnderTest = new DemoClassOnly([|Substitute.For<ICurrentUser>()|]);
        }
    }
}";

        var argumentFixedSource = @"
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

        await VerifySubstituteForToVariable.VerifyRefactoringAsync(
            argumentSource,
            argumentFixedSource,
            actionTitle: SubstituteForToVariableCodeRefactoringProvider.Title);
        await VerifyCS.VerifyRefactoringAsync(argumentSource, argumentSource.Replace("[|", string.Empty).Replace("|]", string.Empty));
    }

    [TestMethod]
    public async Task EmptyConstructorArgumentList_OffersBothConstructorRefactorings()
    {
        var constructorSource = Template.Replace("[|{0}|]", "new DemoClassOnly([||])");
        var quickMockFixedSource = Template.Replace("[|{0}|]",
            "new DemoClassOnly(Substitute.For<ILogger<DemoClassOnly>>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int?>(), Substitute.For<ICurrentUser>(), Substitute.For<Func<SomeCommand>>(), Substitute.For<Func<IValidator<InvoiceDetailsInput>>>())");
        var mockFixedSource = Template.Replace("            var systemUnderTest = [|{0}|];",
            "            var loggerMock = Substitute.For<ILogger<DemoClassOnly>>();\r\n" +
            "            var userMock = Substitute.For<ICurrentUser>();\r\n" +
            "            var commandFactoryMock = Substitute.For<Func<SomeCommand>>();\r\n" +
            "            var validatorFactoryMock = Substitute.For<Func<IValidator<InvoiceDetailsInput>>>();\r\n" +
            "            var systemUnderTest = new DemoClassOnly(loggerMock, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int?>(), userMock, commandFactoryMock, validatorFactoryMock);");

        await VerifyCS.VerifyRefactoringAsync(
            constructorSource,
            quickMockFixedSource,
            actionTitle: NSubstituteQuickMockCodeRefactoringProvider.QuickMockCtorTitle);
        await VerifyCS.VerifyRefactoringAsync(
            constructorSource,
            mockFixedSource,
            actionTitle: NSubstituteQuickMockCodeRefactoringProvider.MockCtorTitle);
    }

    [TestMethod]
    public async Task ActionsHaveExactTitles()
    {
        Assert.IsTrue(string.Equals("Quick mock ctor (NSubstitute)", NSubstituteQuickMockCodeRefactoringProvider.QuickMockCtorTitle));
        Assert.IsTrue(string.Equals("Mock ctor (NSubstitute)", NSubstituteQuickMockCodeRefactoringProvider.MockCtorTitle));
        Assert.IsTrue(string.Equals("Substitute.For<T> to variable", SubstituteForToVariableCodeRefactoringProvider.Title));
        Assert.IsTrue(NSubstitute.QuickMock.Helpers.SourceFileHelpers.IsTestFile(@"C:\src\FeatureTests.cs"));
        Assert.IsFalse(NSubstitute.QuickMock.Helpers.SourceFileHelpers.IsTestFile(@"C:\src\Feature.cs"));
    }
}
