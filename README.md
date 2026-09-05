[![GitHub Workflow Status for main branch](https://img.shields.io/github/actions/workflow/status/rpbeukes/NSubstitute.QuickMock/CI_main.yml?branch=main)](https://github.com/rpbeukes/NSubstitute.QuickMock/actions/workflows/CI_main.yml?query=branch%3Amain+) [![GitHub](https://img.shields.io/github/license/rpbeukes/NSubstitute.QuickMock)](https://github.com/rpbeukes/NSubstitute.QuickMock/blob/main/LICENSE)

<img src="./Doco/Assets/coffee.png" alt="drawing" width="25"/><a href="https://buy.stripe.com/eVa7w16MSehn9QA3cc" target="_blank"> Pay it forward </a> <img src="./Doco/Assets/coffee.png" alt="drawing" width="25"/>

# NSubstitute.QuickMock

Small Visual Studio extension for C# developers writing [NSubstitute](https://github.com/nsubstitute/NSubstitute) tests.
 It provides refactorings for filling constructor arguments with NSubstitute substitutes and extracting `Substitute.For<T>()` expressions into local variables.

The extension supports **Visual Studio v2022/2026**.

---

## Installation

Download the `NSubstitute.QuickMock.*.vsix` asset from the latest successful [main-branch build](https://github.com/rpbeukes/NSubstitute.QuickMock/actions/workflows/CI_main.yml?query=branch%3Amain+is%3Asuccess), then open the VSIX file to install it in Visual Studio.

Your test project must reference the [NSubstitute NuGet package](https://www.nuget.org/packages/NSubstitute/).
The refactorings are offered only when that reference is available.

---

## Available refactorings

- [NSubstitute.QuickMock](#nsubstitutequickmock)
  - [Installation](#installation)
  - [Available refactorings](#available-refactorings)
  - [Scenario](#scenario)
  - [Refactorings](#refactorings)
    - [Mock ctor (NSubstitute)](#mock-ctor-nsubstitute)
    - [Quick mock ctor (NSubstitute)](#quick-mock-ctor-nsubstitute)
    - [Substitute.For to variable (NSubstitute)](#substitutefor-to-variable-nsubstitute)
  - [Test file naming convention](#test-file-naming-convention)
  - [Current limitations](#current-limitations)
  - [Development and validation](#development-and-validation)
  - [TODO](#todo)

All examples below are based on the demo in [`DemoProjectUnitTests/DemoClassOnlyTests.cs`](https://github.com/rpbeukes/NSubstitute.QuickMock/blob/main/DemoProject/DemoProjectUnitTests/DemoClassOnlyTests.cs).

## Scenario

The demo class has a constructor with reference-type and value-type parameters:

```csharp
public DemoClassOnly(ILogger<DemoClassOnly> logger,
                     string stringValue,
                     int intValue,
                     int? nullIntValue,
                     ICurrentUser currentUser,
                     Func<SomeCommand> cmdFactory,
                     Func<IValidator<InvoiceDetailsInput>> validatorFactory)
{ }
```

The examples assume:

```csharp
using NSubstitute;
```

## Refactorings

### Mock ctor (NSubstitute)

Place the cursor between the empty parentheses of a constructor call and press `Ctrl + .`.

```csharp
var systemUnderTest = new DemoClassOnly(<cursor>);
```

Choose **Mock ctor (NSubstitute)**. 
Reference-type parameters are declared as local substitutes, while value-type parameters use the provider's suggested arguments.

Output:

```csharp
var loggerMock = Substitute.For<ILogger<DemoClassOnly>>();
var currentUserMock = Substitute.For<ICurrentUser>();
var cmdFactoryMock = Substitute.For<Func<SomeCommand>>();
var validatorFactoryMock = Substitute.For<Func<IValidator<InvoiceDetailsInput>>>();

var systemUnderTest = new DemoClassOnly(loggerMock,
                                        Arg.Any<string>(),
                                        Arg.Any<int>(),
                                        Arg.Any<int?>(),
                                        currentUserMock,
                                        cmdFactoryMock,
                                        validatorFactoryMock);
```

### Quick mock ctor (NSubstitute)

Place the cursor between the empty parentheses of a constructor call and press `Ctrl + .`.

```csharp
var systemUnderTest = new DemoClassOnly(<cursor>);
```

Choose **Quick mock ctor (NSubstitute)**. 
This inserts substitutes inline for reference-type parameters and suggested arguments for value-type parameters.

Output:


```csharp
var systemUnderTest = new DemoClassOnly(Substitute.For<ILogger<DemoClassOnly>>(),
                                        Arg.Any<string>(),
                                        Arg.Any<int>(),
                                        Arg.Any<int?>(),
                                        Substitute.For<ICurrentUser>(),
                                        Substitute.For<Func<SomeCommand>>(),
                                        Substitute.For<Func<IValidator<InvoiceDetailsInput>>>());
```

### Substitute.For<T> to variable (NSubstitute)

Place the cursor on a `Substitute.For<T>()` expression used as a constructor argument and press `Ctrl + .`.

```csharp
var systemUnderTest = new DemoClassOnly(Substitute.For<ILogger<DemoClassOnly>>(),
                                        Arg.Any<string>(),
                                        Arg.Any<int>(),
                                        Arg.Any<int?>(),
                                        <cursor>Substitute.For<ICurrentUser>(),
                                        Substitute.For<Func<SomeCommand>>(),
                                        Substitute.For<IValidator<InvoiceDetailsInput>>());
```

Choose **Substitute.For<T> to variable (NSubstitute)**. 
The expression is replaced with a local variable named from the constructor parameter (for example, `currentUserMock`).

Output:

```csharp
var currentUserMock = Substitute.For<ICurrentUser>();
var systemUnderTest = new DemoClassOnly(Substitute.For<ILogger<DemoClassOnly>>(),
                                        Arg.Any<string>(),
                                        Arg.Any<int>(),
                                        Arg.Any<int?>(),
                                        currentUserMock,
                                        Substitute.For<Func<SomeCommand>>(),
                                        Substitute.For<IValidator<InvoiceDetailsInput>>());
```

If the suggested name is already used in the method, the refactoring adds a numeric suffix such as `loggerMock2`.

---

## Test file naming convention

**Important:** The extension only changes C# source files whose names end with `Tests.cs` (case-insensitive), for example `TheseAreMyHeroTests.cs`.

---

## Current limitations

- C# is currently supported; Visual Basic is not.
- Refactorings require a project reference to NSubstitute.

---

## Development and validation

The repository includes the demo project and Roslyn refactoring tests. 
The CI workflow builds the refactoring project and VSIX, runs the automated tests.
 
## TODO
- Publish the generated VSIX as a workflow artifact
- Publish to Visual Studio Marketplace

<div style="text-align:right">
<small><a href="https://www.flaticon.com/free-icons/coffee" title="coffee icons">Coffee icons created by Freepik - Flaticon</a></small>
</div>
