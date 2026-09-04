# NSubstitute.QuickMock

NSubstitute.QuickMock is a C# Visual Studio extension that provides quick refactorings for constructor mocking with NSubstitute.

## Compatibility

- Support Visual Studio 2022 and Visual Studio 2026.
- Prefer APIs and project settings that work across both supported Visual Studio versions.
- Keep version-specific behavior isolated and documented when it cannot be shared.

## Requirements

- Read `Requirements.md` before implementing behavior.
- Only transform source files whose names match `*Tests.cs`.
- Preserve valid formatting and produce compilable C#.
- Cover both the documented NSubstitute and quick-mock workflows.

## Engineering practices

- Make focused changes that follow the repository's existing conventions.
- Add or update targeted tests for behavior changes.
- Do not silently swallow parse, analysis, or transformation errors.
- Validate builds and tests before considering work complete.
