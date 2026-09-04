using DemoProject;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;

namespace DemoProject.Tests;

[TestClass]
public class DemoClassOnlyTests
{
    [TestMethod]
    public void MockCtor_NSubstitute()
    {
        // Put the caret between the parentheses and choose "Mock ctor (NSubstitute)".
        var systemUnderTest = new DemoClassOnly();
        Assert.IsNotNull(systemUnderTest);
    }

    [TestMethod]
    public void QuickMockCtor_NSubstitute()
    {
        var loggerMock = Substitute.For<ILogger<DemoClassOnly>>();
        var currentUserMock = Substitute.For<ICurrentUser>();
        var cmdFactoryMock = Substitute.For<Func<SomeCommand>>();
        var validatorFactoryMock = Substitute.For<Func<IValidator<InvoiceDetailsInput>>>();
        // Put the caret between the parentheses and choose "Quick mock ctor (NSubstitute)".
        var systemUnderTest = new DemoClassOnly(loggerMock,
                                                Arg.Any<string>(),
                                                Arg.Any<int>(),
                                                Arg.Any<int?>(),
                                                currentUserMock,
                                                cmdFactoryMock,
                                                validatorFactoryMock);
        Assert.IsNotNull(systemUnderTest);
    }

    [TestMethod]
    public void SubstituteForToVariable_NSubstitute()
    {
        var systemUnderTest = new DemoClassOnly(Substitute.For<ILogger<DemoClassOnly>>(),
                                                Arg.Any<string>(),
                                                Arg.Any<int>(),
                                                Arg.Any<int?>(),
                                                Substitute.For<ICurrentUser>(),
                                                Substitute.For<Func<SomeCommand>>(),
                                                Substitute.For<Func<IValidator<InvoiceDetailsInput>>>());

        // Put the caret on Substitute.For<ICurrentUser>() and choose
        // "Substitute.For<T> to variable".
        Assert.IsNotNull(systemUnderTest);
    }
}
