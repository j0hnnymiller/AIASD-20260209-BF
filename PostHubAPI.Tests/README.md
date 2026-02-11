# PostHubAPI.Tests

Comprehensive test suite for the PostHubAPI project.

## Structure

```
PostHubAPI.Tests/
├── UnitTests/              # Isolated component tests (60% of suite)
│   ├── Services/          # Service layer tests
│   ├── Controllers/       # Controller tests
│   └── Profiles/          # AutoMapper profile tests
├── IntegrationTests/       # Component integration tests (30% of suite)
├── E2ETests/              # End-to-end workflow tests (10% of suite)
└── TestUtilities/         # Shared test infrastructure
    ├── Fixtures/          # Test fixtures and factories
    ├── Builders/          # Test data builders
    └── Helpers/           # Helper utilities
```

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Categories

```bash
# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"

# E2E tests only
dotnet test --filter "Category=E2E"
```

### Run Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Generate Coverage Report

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
```

## Test Conventions

### Naming

- Test classes: `{ClassUnderTest}Tests` (e.g., `PostServiceTests`)
- Test methods: `{MethodName}_{Scenario}_{ExpectedBehavior}` (e.g., `GetPostById_WhenPostExists_ReturnsPost`)

### AAA Pattern

All tests follow the Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up test data and dependencies

    // Act - Execute the method under test

    // Assert - Verify the expected outcome
}
```

### Test Categories

Use the `[Trait("Category", "...")]` attribute:

- `Unit` - Isolated unit tests
- `Integration` - Integration tests
- `E2E` - End-to-end tests
- `Performance` - Performance benchmarks

## Coverage Goals

| Component   | Target Coverage |
| ----------- | --------------- |
| Services    | 90%+            |
| Controllers | 85%+            |
| DTOs/Models | 80%+            |
| Profiles    | 100%            |
| Overall     | 80%+            |

## Best Practices

1. **Test Isolation**: Each test should be independent and not rely on other tests
2. **Clear Intent**: Test names should clearly describe what is being tested
3. **Fast Execution**: Keep unit tests fast (< 1ms each)
4. **Minimal Mocking**: Only mock external dependencies, not the system under test
5. **Meaningful Assertions**: Use FluentAssertions for readable, expressive assertions
6. **Test Data Builders**: Use builders for complex object creation

## Continuous Integration

Tests run automatically on:

- Every push to `master` and `develop` branches
- Every pull request
- Scheduled daily builds

Quality gates require:

- All tests passing
- Code coverage ≥ 80%
- No critical security vulnerabilities
