# Integration Testing Guidelines

This document outlines the testing standards and best practices for the Balsis Note Sheet Library project.

## Test Structure

### Test Class Organization
- Test classes should be placed in the `BasisNoteSheetLibrary.Tests` project
- Test classes should mirror the project structure of the code being tested
- Name test classes with the same name as the class under test, suffixed with `Tests`
- Inherit from `IntegrationTestBase` for integration tests
- Use `Xunit` as the testing framework
- Use `UnitOfWork` for database operations in integration tests (instead of `DbContext`)

### Test Naming Conventions
- Use descriptive method names that explain what is being tested and the expected outcome
- Follow the pattern: `MethodName_StateUnderTest_ExpectedBehavior`
- Use `Async` suffix for async test methods; omit for sync methods
- Use constants (e.g., `NonexistentId`) for repeated test values when appropriate

### Test Method Structure
Follow the Arrange-Act-Assert (AAA) pattern:

```csharp
[Fact]
public async Task MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange - Set up test data and dependencies
    var testData = new TestData();
    
    // Act - Perform the action being tested
    var result = await _service.MethodUnderTest(testData);
    
    // Assert - Verify the outcome
    Assert.NotNull(result);
    // Additional assertions
}

[Fact]
public void MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange
    // ...
    // Act
    // ...
    // Assert
    // ...
}
```

## Test Categories

### Unit Tests
- Test individual components in isolation
- Use mocks for all external dependencies
- Focus on business logic and edge cases

### Integration Tests
- Test interactions between components
- Use real dependencies when possible
- Focus on the happy path and critical error scenarios
- Inherit from `IntegrationTestBase`
- Use an in-memory SQLite database for database tests
- Use `UnitOfWork` for data access

## Test Data

### Test Data Setup
- Each test should be independent and set up its own test data
- Clean up test data in the test method or in the test class constructor
- Use meaningful test data that represents real-world scenarios
- Use object initializers and array initializers for test data
- Use constants for repeated test values (e.g., IDs)

### Test Data Factories
- Consider creating test data factory methods for complex objects
- Use object initializers for simple objects

## Assertions

### General Guidelines
- Use `Assert.` methods for assertions
- Prefer specific assertions over general ones
- Include descriptive messages in assertions when the failure reason isn't obvious
- Use `Assert.ThrowsAsync` for exception testing in async methods
- Use `Assert.IsType<T>` and `Assert.Equal` for type and value checks
- Use `ElementAt` for collection assertions when order matters

## Mocking

### When to Mock
- External services
- File system operations

### Mocking Guidelines
- Use `Moq` for creating test doubles
- Set up mocks in the test class constructor or in the test method
- Use `Setup` and `Verify` to configure and check mock interactions
- Use `MemoryStream` for file-related tests

## Best Practices

1. **Test Independence**
   - Each test should be able to run independently
   - Don't rely on test execution order
   - Clean up test data after each test (e.g., using `RemoveRange` and `SaveChangesAsync`)

2. **Test Readability**
   - Keep tests focused and concise
   - Use descriptive variable names

3. **Test Coverage**
   - Aim for high test coverage
   - Focus on testing behavior, not implementation details
   - Don't test framework or library code

## Example Test

```csharp
public class NoteSheetServiceTests : IntegrationTestBase
{
    private const uint NonexistentId = 999;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly NoteSheetService _service;

    public NoteSheetServiceTests()
    {
        _service = new NoteSheetService(UnitOfWork, _fileStorageServiceMock.Object);
        // Clean up database before each test
        UnitOfWork.NoteSheets.RemoveRange(UnitOfWork.NoteSheets);
        UnitOfWork.SaveChanges();
    }

    [Fact]
    public async Task GetNoteSheetAsync_WithExistingId_ReturnsNoteSheet()
    {
        // Arrange
        var noteSheet = new NoteSheet { Id = 1, Title = "Test" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync();

        // Act
        var result = await _service.GetNoteSheetAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(noteSheet.Title, result.Title);
    }
}
```

## Test Data Cleanup

- Clean up test data in the test class constructor or in a `Dispose` method
- Use `RemoveRange` for bulk deletion
- Use `SaveChangesAsync` for async cleanup
- Ensure tests don't leave behind any state that could affect other tests

## Code Review

- Review test code with the same rigor as production code
- Ensure tests are maintainable and follow the guidelines
- Look for opportunities to refactor and improve test code