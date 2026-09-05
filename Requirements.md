### Mock ctor (NSubstitute)

Put the `cursor (caret)` between the `()`, and hit `CTRL + .`.

```csharp
var systemUnderTest = new DemoClassOnly(<cursor>);
```

Find `Mock ctor (NSubstitute)` Refactor Menu Options.

Refactor output:

```csharp
var loggerMock = Substitute.For<ILogger>();
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

---

### Quick mock ctor (NSubstitute)

Put the `cursor (caret)` between the `()`, and hit `CTRL + .`.

```csharp
var systemUnderTest = new DemoClassOnly(<cursor>);
```

Find `Quick mock ctor (NSubstitute)` Refactor Menu Options.

Refactor output:

```csharp
var systemUnderTest = new DemoClassOnly(Substitute.For<ILogger<DemoClassOnly>>(),
It.IsAny<string>(),
It.IsAny<int>(),
It.IsAny<int?>(),
Substitute.For<ICurrentUser>(),
Substitute.For<Func<SomeCommand>>(),
Substitute.For<Func<IValidator<InvoiceDetailsInput>>>());
```

---
### Substitute.For&lt;T&gt; to variable
Put the `cursor (caret)` on an argument where `Substitute.For<T>` is used.

Find `Substitute.For<T> to variable` Refactor Menu Options.

Make sure you put the `cursor` on the word `Mock` or just in front of it.

```csharp
var systemUnderTest = new DemoClassOnly(Substitute.For<ILogger<DemoClassOnly>>(),
It.IsAny<string>(),
It.IsAny<int>(),
It.IsAny<int?>(),
<cursor>Substitute.For<ICurrentUser>(),
Substitute.For<Func<SomeCommand>>(),
Substitute.For<Func<IValidator<InvoiceDetailsInput>>>());
```

Refactor output:

```csharp
var currentUserMock = new Mock<ICurrentUser>();
var systemUnderTest = new DemoClassOnly(Mock.Of<ILogger<DemoClassOnly>>(),
It.IsAny<string>(),
It.IsAny<int>(),
It.IsAny<int?>(),
currentUserMock,
Mock.Of<Func<SomeCommand>>(),
Mock.Of<Func<IValidator<InvoiceDetailsInput>>>());
```

---

## Test File Naming Convention
**NOTE:** This extension will only change code following the file naming convention `*tests.cs` eg: `TheseAreMyHeroTests.cs`.

--