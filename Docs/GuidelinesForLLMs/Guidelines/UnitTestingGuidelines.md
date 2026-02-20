# Unit Testing Guidelines

This document outlines the standards and best practices for writing unit tests in the Balsis Sheet Music Library project.

## Test Structure

### Test Class Organization

- Place unit test classes in the `BasisSheetMusicLibrary.Tests` project, under the `Unit` folder.
- Mirror the project structure of the code being tested.
- Name test classes after the class under test, suffixed with `Tests` (e.g., `SheetMusicTests`).
- **Do not** inherit from `IntegrationTestBase` for unit tests.
- Use `Xunit` as the testing framework.

### Test Naming Conventions

- Use descriptive method names that explain what is being tested and the expected outcome.
- Follow the pattern: `MethodName_StateUnderTest_ExpectedBehavior`.
- Use the `Async` suffix for async test methods; omit for sync methods.
- Use constants for repeated test values when appropriate.

### Test Method Structure

Follow the Arrange-Act-Assert (AAA) pattern:

```csharp
[Fact]
public void MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange - Set up test data and dependencies
    // Act - Perform the action being tested
    // Assert - Verify the outcome
}
```

## Test Categories

### Unit Tests

- Test individual components in isolation.
- Use mocks or stubs for all external dependencies (e.g., services, file system, database).
- Focus on business logic, edge cases, and error handling.
- Do not test integration between multiple components.

## Test Data

### Test Data Setup

- Each test should be independent and set up its own test data.
- Use meaningful test data that represents real-world scenarios.
- Use object initializers and array initializers for test data.
- Use constants for repeated test values.

### Test Data Factories

- Consider creating test data factory methods for complex objects.
- Use object initializers for simple objects.

## Assertions

### General Guidelines

- Use `Assert.` methods for assertions.
- Prefer specific assertions over general ones (e.g., `Assert.Equal`, `Assert.Throws`).
- Include descriptive messages in assertions when the failure reason isn't obvious.
- Use `Assert.Throws` for exception testing in sync methods.
- Use `Assert.IsType<T>` and `Assert.Equal` for type and value checks.
- Use `ElementAt` for collection assertions when order matters.

## Mocking

### When to Mock

- Mock external services, file system operations, and any dependencies outside the class under test.

### Mocking Guidelines

- Use `Moq` for creating test doubles.
- Set up mocks in the test class constructor or in the test method.
- Use `Setup` and `Verify` to configure and check mock interactions.

## Best Practices

1. **Test Independence**
    - Each test should be able to run independently.
    - Do not rely on test execution order.
    - Do not leave behind any state that could affect other tests.

2. **Test Readability**
    - Keep tests focused and concise.
    - Use descriptive variable names.

3. **Test Coverage**
    - Aim for high test coverage.
    - Focus on testing behavior, not implementation details.
    - Do not test framework or library code.

## Code Review

- Review test code with the same rigor as production code.
- Ensure tests are maintainable and follow the guidelines.
- Look for opportunities to refactor and improve test code.

## Example Test

```csharp
public class SheetMusicTests
{
    [Fact]
    public void GetFileName_WithValidFields_ReturnsExpectedFileName()
    {
        // Arrange
        var noteSheet = new SheetMusic { Title = "Test", Author = "Composer" };
        // Act
        var fileName = sheetMusic.GetFileName();
        // Assert
        Assert.StartsWith("Test, Composer", fileName);
        Assert.EndsWith(".pdf", fileName);
    }
}
```
