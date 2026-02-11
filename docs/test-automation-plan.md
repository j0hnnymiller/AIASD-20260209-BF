# Test Automation Plan for PostHubAPI

**Document Version**: 1.0.0
**Created**: February 11, 2026
**Project**: PostHubAPI
**Target Framework**: .NET 8.0

## Table of Contents

- [Executive Summary](#executive-summary)
- [Project Analysis](#project-analysis)
- [Test Strategy](#test-strategy)
- [Automation Framework](#automation-framework)
- [Quality Metrics and Coverage Goals](#quality-metrics-and-coverage-goals)
- [Defect Prevention Strategy](#defect-prevention-strategy)
- [Performance Testing Strategy](#performance-testing-strategy)
- [Implementation Roadmap](#implementation-roadmap)
- [CI/CD Pipeline Configuration](#cicd-pipeline-configuration)
- [Sample Test Examples](#sample-test-examples)
- [Quality Gates](#quality-gates)
- [Tools and Technologies](#tools-and-technologies)
- [Next Steps](#next-steps)

## Executive Summary

This document outlines a comprehensive test automation strategy for the PostHubAPI project. The plan follows industry best practices and the testing pyramid approach, targeting 80%+ overall code coverage with a focus on unit tests (60%), integration tests (30%), and end-to-end tests (10%).

**Key Objectives:**

- Establish robust automated testing framework
- Achieve 80%+ code coverage
- Integrate tests into CI/CD pipeline
- Implement performance and security testing
- Reduce defect escape rate to < 5%

## Project Analysis

### Current Architecture

**Technology Stack:**

- ASP.NET Core 8.0 Web API
- Entity Framework Core (In-Memory Database)
- JWT Authentication
- AutoMapper for DTO mappings
- BCrypt for password hashing

**Project Structure:**

```
PostHubAPI/
├── Controllers/
│   ├── PostController.cs
│   ├── CommentController.cs
│   └── UserController.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IPostService.cs
│   │   ├── ICommentService.cs
│   │   └── IUserService.cs
│   └── Implementations/
│       ├── PostService.cs
│       ├── CommentService.cs
│       └── UserService.cs
├── Models/
│   ├── Post.cs
│   ├── Comment.cs
│   └── User.cs
├── Dtos/
│   ├── Post/
│   ├── Comment/
│   └── User/
├── Data/
│   └── ApplicationDbContext.cs
└── Profiles/
    ├── PostProfile.cs
    └── CommentProfile.cs
```

### Testing Opportunities

**High-Value Test Areas:**

1. **Service Layer**: Business logic and data access (Critical)
2. **Controllers**: API endpoints and HTTP responses (Critical)
3. **Authentication**: JWT token generation and validation (Critical)
4. **AutoMapper Profiles**: DTO mappings (High)
5. **Exception Handling**: NotFoundException and validation (High)

## Test Strategy

### Testing Pyramid Structure

```
                    🔺 E2E Tests (10%)
                   Integration Tests (30%)
              ═══════════════════════════════
         Unit Tests (60%)
```

### Test Categories

#### 1. Unit Tests (60% of test suite)

**Scope:**

- Service layer methods (isolated with mocks)
- Controller actions (isolated with mocked services)
- AutoMapper profile configurations
- DTO validation logic
- Exception handling

**Benefits:**

- Fast execution
- Pinpoint failure location
- Easy to maintain
- Simple to write

**Target Coverage**: 90%+ for services, 85%+ for controllers

#### 2. Integration Tests (30% of test suite)

**Scope:**

- API endpoints with real HTTP requests
- Database interactions with test database
- Authentication and authorization flows
- Complete request/response cycles
- DTO serialization/deserialization

**Benefits:**

- Validates component interactions
- Tests realistic scenarios
- Catches integration issues

**Target Coverage**: 85%+ for API endpoints

#### 3. End-to-End Tests (10% of test suite)

**Scope:**

- Complete user workflows
- Multi-entity operations
- Complex business scenarios
- Authentication + Authorization + CRUD operations

**Benefits:**

- Validates entire application flow
- User perspective testing
- Confidence in deployments

**Target Coverage**: Critical user journeys

## Automation Framework

### Test Project Structure

```
PostHubAPI.Tests/
├── PostHubAPI.Tests.csproj
├── UnitTests/
│   ├── Services/
│   │   ├── PostServiceTests.cs
│   │   ├── CommentServiceTests.cs
│   │   └── UserServiceTests.cs
│   ├── Controllers/
│   │   ├── PostControllerTests.cs
│   │   ├── CommentControllerTests.cs
│   │   └── UserControllerTests.cs
│   └── Profiles/
│       ├── PostProfileTests.cs
│       └── CommentProfileTests.cs
├── IntegrationTests/
│   ├── PostIntegrationTests.cs
│   ├── CommentIntegrationTests.cs
│   └── UserIntegrationTests.cs
├── E2ETests/
│   └── ApiEndToEndTests.cs
├── PerformanceTests/
│   ├── ServiceBenchmarks.cs
│   └── EndpointBenchmarks.cs
└── TestUtilities/
    ├── Fixtures/
    │   ├── DatabaseFixture.cs
    │   ├── WebApplicationFactory.cs
    │   └── JwtTokenFixture.cs
    ├── Builders/
    │   ├── PostBuilder.cs
    │   ├── CommentBuilder.cs
    │   └── UserBuilder.cs
    └── Helpers/
        ├── JwtTokenHelper.cs
        ├── TestDataSeeder.cs
        └── HttpContentHelper.cs
```

### Required NuGet Packages

```xml
<ItemGroup>
  <!-- Testing Framework -->
  <PackageReference Include="xunit" Version="2.6.6" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />

  <!-- Mocking and Assertions -->
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />

  <!-- Integration Testing -->
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.1" />

  <!-- Test Data Generation -->
  <PackageReference Include="AutoFixture" Version="4.18.1" />
  <PackageReference Include="AutoFixture.Xunit2" Version="4.18.1" />
  <PackageReference Include="Bogus" Version="35.4.0" />

  <!-- Code Coverage -->
  <PackageReference Include="coverlet.collector" Version="6.0.0" />
  <PackageReference Include="ReportGenerator" Version="5.2.2" />

  <!-- Performance Testing -->
  <PackageReference Include="BenchmarkDotNet" Version="0.13.12" />
</ItemGroup>
```

## Quality Metrics and Coverage Goals

### Coverage Targets

| Component                 | Target Coverage | Priority    | Rationale               |
| ------------------------- | --------------- | ----------- | ----------------------- |
| **Services**              | 90%+            | 🔴 Critical | Core business logic     |
| **Controllers**           | 85%+            | 🔴 Critical | API contract validation |
| **DTOs/Models**           | 80%+            | 🟡 High     | Data validation         |
| **Profiles (AutoMapper)** | 100%            | 🟡 High     | Critical mappings       |
| **Exception Handlers**    | 100%            | 🟡 High     | Error handling          |
| **Overall Code Coverage** | 80%+            | 🔴 Critical | Industry standard       |

### Key Performance Indicators (KPIs)

**Test Execution Metrics:**

- Total test execution time: < 5 minutes
- Unit test execution time: < 30 seconds
- Integration test execution time: < 3 minutes
- E2E test execution time: < 2 minutes

**Quality Metrics:**

- Build pipeline success rate: > 95%
- Mean Time to Detect (MTTD): < 1 hour
- Test maintenance ratio: < 10% of development time
- Flaky test rate: < 2%
- Defect escape rate: < 5%

**Code Quality Metrics:**

- Code coverage trend: Increasing
- Cyclomatic complexity: < 10 per method
- Technical debt ratio: < 5%
- Code duplication: < 3%

## Defect Prevention Strategy

### Test Coverage Areas

#### 1. Functional Testing

**CRUD Operations:**

- ✅ Create operations with valid data
- ✅ Read operations for existing and non-existing entities
- ✅ Update operations with valid and invalid data
- ✅ Delete operations with cascading effects
- ✅ Bulk operations and batch processing

**Authentication & Authorization:**

- ✅ User registration with valid/invalid data
- ✅ Login with correct/incorrect credentials
- ✅ JWT token generation and validation
- ✅ Token expiration handling
- ✅ Authorization for protected endpoints
- ✅ Role-based access control

**Input Validation:**

- ✅ Required field validation
- ✅ Data type validation
- ✅ Length constraints
- ✅ Format validation (email, phone, etc.)
- ✅ Business rule validation

**Error Handling:**

- ✅ NotFoundException scenarios
- ✅ ValidationException scenarios
- ✅ Concurrent modification handling
- ✅ Database constraint violations
- ✅ Network timeout scenarios

#### 2. Non-Functional Testing

**Performance:**

- ✅ Response time benchmarks for all endpoints
- ✅ Database query optimization validation
- ✅ N+1 query detection
- ✅ Memory leak detection
- ✅ Connection pool management

**Security:**

- ✅ SQL injection prevention
- ✅ XSS attack prevention
- ✅ CSRF protection
- ✅ Password hashing verification
- ✅ JWT token security
- ✅ Input sanitization

**Reliability:**

- ✅ Concurrent user scenarios
- ✅ Database connection failures
- ✅ Service degradation handling
- ✅ Retry logic validation

## Performance Testing Strategy

### Load Testing Scenarios

#### Scenario 1: Normal Load (Baseline)

```yaml
Configuration:
  Virtual Users: 50
  Duration: 10 minutes
  Ramp-up Time: 2 minutes
  Target Throughput: 200 requests/second

Success Criteria:
  - Success Rate: > 99%
  - P50 Response Time: < 100ms
  - P95 Response Time: < 200ms
  - P99 Response Time: < 500ms
  - Error Rate: < 1%
  - CPU Usage: < 70%
  - Memory Usage: < 80%

Endpoints to Test:
  - GET /api/Post (50% of requests)
  - GET /api/Post/{id} (20% of requests)
  - POST /api/Post (15% of requests)
  - PUT /api/Post/{id} (10% of requests)
  - DELETE /api/Post/{id} (5% of requests)
```

#### Scenario 2: Peak Load

```yaml
Configuration:
  Virtual Users: 200
  Duration: 5 minutes
  Ramp-up Time: 1 minute
  Target Throughput: 500 requests/second

Success Criteria:
  - Success Rate: > 95%
  - P50 Response Time: < 150ms
  - P95 Response Time: < 500ms
  - P99 Response Time: < 1000ms
  - Error Rate: < 5%
  - CPU Usage: < 85%
  - Memory Usage: < 90%
```

#### Scenario 3: Stress Test (Breaking Point)

```yaml
Configuration:
  Virtual Users: Start at 100, increase by 50 every minute
  Duration: 15 minutes
  Maximum Users: 1000

Objectives:
  - Identify system breaking point
  - Measure graceful degradation
  - Validate error handling under stress
  - Monitor resource exhaustion

Monitoring:
  - CPU utilization
  - Memory usage and GC pressure
  - Thread pool exhaustion
  - Database connection pool saturation
  - Response time degradation curve
```

### Performance Testing Tools

**Recommended Tools:**

1. **BenchmarkDotNet**: Micro-benchmarking of methods and services
2. **NBomber**: Load testing with realistic scenarios (C# native)
3. **k6**: Alternative load testing tool (JavaScript-based)
4. **Application Insights**: Real-time monitoring and APM

### Performance Benchmarks

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class ServiceBenchmarks
{
    [Benchmark]
    public async Task GetAllPosts_Benchmark()
    {
        // Benchmark implementation
    }

    [Benchmark]
    public async Task CreatePost_Benchmark()
    {
        // Benchmark implementation
    }
}
```

## Implementation Roadmap

### Week 1: Foundation Setup

**Objectives:**

- Set up test project infrastructure
- Configure development environment
- Establish testing standards

**Tasks:**

- [ ] Create PostHubAPI.Tests project
- [ ] Install required NuGet packages
- [ ] Set up test project structure (folders)
- [ ] Configure code coverage tools (Coverlet)
- [ ] Set up ReportGenerator for coverage visualization
- [ ] Create test utilities and base classes
- [ ] Document testing guidelines

**Deliverables:**

- Test project with proper structure
- Configuration files (xunit.runner.json, .runsettings)
- README for test project
- Initial CI/CD pipeline (basic)

### Week 2: Unit Tests Implementation

**Objectives:**

- Implement comprehensive unit tests
- Achieve 85%+ coverage for critical components

**Tasks:**

**Service Layer Tests:**

- [ ] PostService unit tests (CRUD operations)
- [ ] CommentService unit tests (CRUD operations)
- [ ] UserService unit tests (authentication logic)
- [ ] Exception handling tests
- [ ] Edge case and boundary tests

**Controller Tests:**

- [ ] PostController unit tests
- [ ] CommentController unit tests
- [ ] UserController unit tests
- [ ] ModelState validation tests
- [ ] Action result type tests

**Profile Tests:**

- [ ] PostProfile mapping tests
- [ ] CommentProfile mapping tests
- [ ] Bidirectional mapping validation

**Deliverables:**

- 150+ unit tests
- 85%+ code coverage for services
- 80%+ code coverage for controllers
- Test documentation

### Week 3: Integration Tests Implementation

**Objectives:**

- Implement integration tests for API endpoints
- Validate end-to-end request/response flows

**Tasks:**

- [ ] Set up WebApplicationFactory
- [ ] Configure test database (in-memory or test instance)
- [ ] Create integration test base class
- [ ] Implement JWT token helper for authenticated tests
- [ ] Post endpoint integration tests
- [ ] Comment endpoint integration tests
- [ ] User authentication integration tests
- [ ] Error scenario integration tests
- [ ] Content negotiation tests

**Deliverables:**

- 50+ integration tests
- WebApplicationFactory configuration
- Authentication helper utilities
- Integration test documentation

### Week 4: Advanced Testing

**Objectives:**

- Implement E2E tests
- Set up performance testing
- Add security tests

**Tasks:**

**E2E Tests:**

- [ ] User registration and login flow
- [ ] Complete post creation with comments
- [ ] Authorization scenarios
- [ ] Multi-entity operations

**Performance Tests:**

- [ ] Set up BenchmarkDotNet
- [ ] Service method benchmarks
- [ ] Controller action benchmarks
- [ ] Database query performance tests
- [ ] Load testing with NBomber

**Security Tests:**

- [ ] SQL injection prevention tests
- [ ] XSS prevention tests
- [ ] JWT token security tests
- [ ] Password hashing validation

**Deliverables:**

- 20+ E2E tests
- Performance benchmark suite
- Security test suite
- Performance baseline report

### Week 5: CI/CD Integration and Documentation

**Objectives:**

- Integrate tests into CI/CD pipeline
- Set up quality gates
- Complete documentation

**Tasks:**

- [ ] Create GitHub Actions workflow
- [ ] Configure automated test execution
- [ ] Set up code coverage reporting
- [ ] Implement quality gates (coverage thresholds)
- [ ] Configure test result reporting
- [ ] Set up automated performance regression detection
- [ ] Create test execution dashboard
- [ ] Write comprehensive test documentation
- [ ] Create test maintenance guide

**Deliverables:**

- Complete CI/CD pipeline
- Automated quality gates
- Test execution reports
- Comprehensive documentation

## CI/CD Pipeline Configuration

### GitHub Actions Workflow

```yaml
name: Test Automation Pipeline

on:
  push:
    branches: [master, develop]
  pull_request:
    branches: [master]
  schedule:
    - cron: "0 0 * * *" # Daily at midnight

env:
  DOTNET_VERSION: "8.0.x"

jobs:
  unit-tests:
    name: Unit Tests
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v3
        with:
          fetch-depth: 0

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Run Unit Tests
        run: |
          dotnet test \
            --no-build \
            --configuration Release \
            --filter "Category=Unit" \
            --logger "trx;LogFileName=unit-test-results.trx" \
            --collect:"XPlat Code Coverage" \
            --results-directory ./TestResults

      - name: Generate Coverage Report
        run: |
          dotnet tool install -g dotnet-reportgenerator-globaltool
          reportgenerator \
            -reports:./TestResults/**/coverage.cobertura.xml \
            -targetdir:./CoverageReport \
            -reporttypes:"Html;Badges;Cobertura"

      - name: Upload Coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: ./TestResults/**/coverage.cobertura.xml
          flags: unittests
          name: codecov-unit-tests

      - name: Upload Test Results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: unit-test-results
          path: ./TestResults/unit-test-results.trx

      - name: Upload Coverage Report
        uses: actions/upload-artifact@v3
        with:
          name: coverage-report
          path: ./CoverageReport

  integration-tests:
    name: Integration Tests
    runs-on: ubuntu-latest
    needs: unit-tests

    steps:
      - name: Checkout code
        uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Run Integration Tests
        run: |
          dotnet test \
            --no-build \
            --configuration Release \
            --filter "Category=Integration" \
            --logger "trx;LogFileName=integration-test-results.trx" \
            --results-directory ./TestResults

      - name: Upload Test Results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: integration-test-results
          path: ./TestResults/integration-test-results.trx

  e2e-tests:
    name: End-to-End Tests
    runs-on: ubuntu-latest
    needs: integration-tests

    steps:
      - name: Checkout code
        uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Run E2E Tests
        run: |
          dotnet test \
            --no-build \
            --configuration Release \
            --filter "Category=E2E" \
            --logger "trx;LogFileName=e2e-test-results.trx" \
            --results-directory ./TestResults

      - name: Upload Test Results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: e2e-test-results
          path: ./TestResults/e2e-test-results.trx

  performance-tests:
    name: Performance Tests
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/master'
    needs: e2e-tests

    steps:
      - name: Checkout code
        uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Run Performance Benchmarks
        run: |
          dotnet run \
            --project PostHubAPI.Tests \
            --configuration Release \
            --filter "*Benchmarks*" \
            -- --exporters json

      - name: Upload Benchmark Results
        uses: actions/upload-artifact@v3
        with:
          name: benchmark-results
          path: ./BenchmarkDotNet.Artifacts/results/*.json

  quality-gate:
    name: Quality Gate Check
    runs-on: ubuntu-latest
    needs: [unit-tests, integration-tests, e2e-tests]

    steps:
      - name: Download Coverage Report
        uses: actions/download-artifact@v3
        with:
          name: coverage-report

      - name: Check Coverage Threshold
        run: |
          # Extract coverage percentage from report
          COVERAGE=$(grep -oP 'Line coverage: \K[0-9.]+' ./CoverageReport/index.html || echo "0")
          echo "Code Coverage: $COVERAGE%"

          # Fail if coverage is below 80%
          if (( $(echo "$COVERAGE < 80" | bc -l) )); then
            echo "❌ Code coverage ($COVERAGE%) is below the required threshold (80%)"
            exit 1
          else
            echo "✅ Code coverage ($COVERAGE%) meets the required threshold (80%)"
          fi

      - name: Quality Gate Passed
        run: echo "🎉 All quality gates passed!"
```

### Quality Gate Configuration

```yaml
# quality-gates.yml
coverage:
  minimum_threshold: 80
  target_threshold: 85

test_execution:
  maximum_duration_minutes: 5
  minimum_pass_rate: 95

performance:
  maximum_p95_response_time_ms: 200
  maximum_p99_response_time_ms: 500

security:
  fail_on_critical: true
  fail_on_high: true
  fail_on_medium: false
```

## Sample Test Examples

### Unit Test Examples

#### Service Layer Test

```csharp
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PostHubAPI.Data;
using PostHubAPI.Dtos.Post;
using PostHubAPI.Exceptions;
using PostHubAPI.Models;
using PostHubAPI.Services.Implementations;
using Xunit;

namespace PostHubAPI.Tests.UnitTests.Services;

[Trait("Category", "Unit")]
public class PostServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly PostService _sut;

    public PostServiceTests()
    {
        // Set up in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockMapper = new Mock<IMapper>();
        _sut = new PostService(_context, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAllPostsAsync_WhenPostsExist_ReturnsAllPosts()
    {
        // Arrange
        var posts = new List<Post>
        {
            new Post { Id = 1, Title = "Post 1", Body = "Body 1" },
            new Post { Id = 2, Title = "Post 2", Body = "Body 2" }
        };

        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();

        var expectedDtos = new List<ReadPostDto>
        {
            new ReadPostDto { Id = 1, Title = "Post 1", Body = "Body 1" },
            new ReadPostDto { Id = 2, Title = "Post 2", Body = "Body 2" }
        };

        _mockMapper.Setup(m => m.Map<IEnumerable<ReadPostDto>>(It.IsAny<List<Post>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetAllPostsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedDtos);
    }

    [Fact]
    public async Task GetPostByIdAsync_WhenPostExists_ReturnsPost()
    {
        // Arrange
        var post = new Post { Id = 1, Title = "Test Post", Body = "Test Body" };
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var expectedDto = new ReadPostDto { Id = 1, Title = "Test Post", Body = "Test Body" };
        _mockMapper.Setup(m => m.Map<ReadPostDto>(It.IsAny<Post>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetPostByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task GetPostByIdAsync_WhenPostDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        Func<Task> act = async () => await _sut.GetPostByIdAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Post not found!");
    }

    [Fact]
    public async Task CreateNewPostAsync_WithValidDto_ReturnsNewPostId()
    {
        // Arrange
        var createDto = new CreatePostDto { Title = "New Post", Body = "New Body" };
        var post = new Post { Id = 1, Title = "New Post", Body = "New Body" };

        _mockMapper.Setup(m => m.Map<Post>(It.IsAny<CreatePostDto>()))
            .Returns(post);

        // Act
        var result = await _sut.CreateNewPostAsync(createDto);

        // Assert
        result.Should().BeGreaterThan(0);
        _context.Posts.Should().Contain(p => p.Title == "New Post");
    }

    [Fact]
    public async Task EditPostAsync_WhenPostExists_UpdatesAndReturnsPost()
    {
        // Arrange
        var post = new Post { Id = 1, Title = "Original", Body = "Original Body" };
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var editDto = new EditPostDto { Title = "Updated", Body = "Updated Body" };
        var expectedDto = new ReadPostDto { Id = 1, Title = "Updated", Body = "Updated Body" };

        _mockMapper.Setup(m => m.Map(It.IsAny<EditPostDto>(), It.IsAny<Post>()));
        _mockMapper.Setup(m => m.Map<ReadPostDto>(It.IsAny<Post>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.EditPostAsync(1, editDto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task DeletePostAsync_WhenPostExists_RemovesPost()
    {
        // Arrange
        var post = new Post { Id = 1, Title = "To Delete", Body = "Body" };
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeletePostAsync(1);

        // Assert
        _context.Posts.Should().NotContain(p => p.Id == 1);
    }

    [Fact]
    public async Task DeletePostAsync_WhenPostDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        Func<Task> act = async () => await _sut.DeletePostAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Post not found!");
    }
}
```

#### Controller Test Example

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PostHubAPI.Controllers;
using PostHubAPI.Dtos.Post;
using PostHubAPI.Exceptions;
using PostHubAPI.Services.Interfaces;
using Xunit;

namespace PostHubAPI.Tests.UnitTests.Controllers;

[Trait("Category", "Unit")]
public class PostControllerTests
{
    private readonly Mock<IPostService> _mockPostService;
    private readonly PostController _sut;

    public PostControllerTests()
    {
        _mockPostService = new Mock<IPostService>();
        _sut = new PostController(_mockPostService.Object);
    }

    [Fact]
    public async Task GetAllPosts_ReturnsOkResultWithPosts()
    {
        // Arrange
        var posts = new List<ReadPostDto>
        {
            new ReadPostDto { Id = 1, Title = "Post 1" },
            new ReadPostDto { Id = 2, Title = "Post 2" }
        };

        _mockPostService.Setup(s => s.GetAllPostsAsync())
            .ReturnsAsync(posts);

        // Act
        var result = await _sut.GetAllPosts();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(posts);
    }

    [Fact]
    public async Task GetPostById_WhenPostExists_ReturnsOkResultWithPost()
    {
        // Arrange
        var post = new ReadPostDto { Id = 1, Title = "Test Post" };
        _mockPostService.Setup(s => s.GetPostByIdAsync(1))
            .ReturnsAsync(post);

        // Act
        var result = await _sut.GetPostById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(post);
    }

    [Fact]
    public async Task GetPostById_WhenPostDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockPostService.Setup(s => s.GetPostByIdAsync(999))
            .ThrowsAsync(new NotFoundException("Post not found!"));

        // Act
        var result = await _sut.GetPostById(999);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Post not found!");
    }

    [Fact]
    public async Task CreatePost_WhenModelStateIsValid_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreatePostDto { Title = "New Post", Body = "Body" };
        _mockPostService.Setup(s => s.CreateNewPostAsync(createDto))
            .ReturnsAsync(1);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _sut.CreatePost(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Value.Should().Be(1);
        createdResult.Location.Should().Contain("/api/Post/1");
    }

    [Fact]
    public async Task CreatePost_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreatePostDto();
        _sut.ModelState.AddModelError("Title", "Required");

        // Act
        var result = await _sut.CreatePost(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
```

### Integration Test Examples

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PostHubAPI.Dtos.Post;
using Xunit;

namespace PostHubAPI.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class PostIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PostIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllPosts_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/Post");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllPosts_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/Post");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task CreatePost_WithValidData_ReturnsCreatedStatus()
    {
        // Arrange
        var createDto = new CreatePostDto
        {
            Title = "Integration Test Post",
            Body = "This is a test post body"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Post", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPostById_WhenPostExists_ReturnsPost()
    {
        // Arrange - Create a post first
        var createDto = new CreatePostDto
        {
            Title = "Test Post",
            Body = "Test Body"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Post", createDto);
        var postId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Act
        var response = await _client.GetAsync($"/api/Post/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var post = await response.Content.ReadFromJsonAsync<ReadPostDto>();
        post.Should().NotBeNull();
        post!.Title.Should().Be("Test Post");
    }

    [Fact]
    public async Task GetPostById_WhenPostDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/Post/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePost_WithValidData_ReturnsOk()
    {
        // Arrange - Create a post first
        var createDto = new CreatePostDto
        {
            Title = "Original Title",
            Body = "Original Body"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Post", createDto);
        var postId = await createResponse.Content.ReadFromJsonAsync<int>();

        var updateDto = new EditPostDto
        {
            Title = "Updated Title",
            Body = "Updated Body"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Post/{postId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedPost = await response.Content.ReadFromJsonAsync<ReadPostDto>();
        updatedPost!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeletePost_WhenPostExists_ReturnsNoContent()
    {
        // Arrange - Create a post first
        var createDto = new CreatePostDto
        {
            Title = "To Delete",
            Body = "Body"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Post", createDto);
        var postId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Act
        var response = await _client.DeleteAsync($"/api/Post/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/Post/{postId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

### E2E Test Example

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PostHubAPI.Dtos.Post;
using PostHubAPI.Dtos.Comment;
using PostHubAPI.Dtos.User;
using Xunit;

namespace PostHubAPI.Tests.E2ETests;

[Trait("Category", "E2E")]
public class CompleteWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CompleteWorkflowTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteUserJourney_CreatePostAndAddComments_Success()
    {
        // Step 1: Register a new user
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/User/register", registerDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 2: Login
        var loginDto = new LoginUserDto
        {
            Username = "testuser",
            Password = "Test123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/User/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await loginResponse.Content.ReadAsStringAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Step 3: Create a post
        var createPostDto = new CreatePostDto
        {
            Title = "My First Post",
            Body = "This is my first post content"
        };

        var createPostResponse = await _client.PostAsJsonAsync("/api/Post", createPostDto);
        createPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var postId = await createPostResponse.Content.ReadFromJsonAsync<int>();
        postId.Should().BeGreaterThan(0);

        // Step 4: Add comments to the post
        var commentDto = new CreateCommentDto
        {
            PostId = postId,
            Text = "Great post!"
        };

        var createCommentResponse = await _client.PostAsJsonAsync("/api/Comment", commentDto);
        createCommentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 5: Retrieve the post with comments
        var getPostResponse = await _client.GetAsync($"/api/Post/{postId}");
        getPostResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var post = await getPostResponse.Content.ReadFromJsonAsync<ReadPostDto>();
        post.Should().NotBeNull();
        post!.Comments.Should().NotBeEmpty();
        post.Comments.Should().Contain(c => c.Text == "Great post!");

        // Step 6: Update the post
        var updatePostDto = new EditPostDto
        {
            Title = "My Updated Post",
            Body = "Updated content"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/Post/{postId}", updatePostDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 7: Delete the post
        var deleteResponse = await _client.DeleteAsync($"/api/Post/{postId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
```

### Performance Test Example

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PostHubAPI.Services.Implementations;

namespace PostHubAPI.Tests.PerformanceTests;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[Trait("Category", "Performance")]
public class ServiceBenchmarks
{
    private PostService _postService;
    private ApplicationDbContext _context;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize services and seed data
    }

    [Benchmark]
    public async Task GetAllPosts_Benchmark()
    {
        await _postService.GetAllPostsAsync();
    }

    [Benchmark]
    public async Task GetPostById_Benchmark()
    {
        await _postService.GetPostByIdAsync(1);
    }

    [Benchmark]
    public async Task CreatePost_Benchmark()
    {
        var dto = new CreatePostDto { Title = "Benchmark", Body = "Test" };
        await _postService.CreateNewPostAsync(dto);
    }
}
```

## Quality Gates

### Pre-Commit Quality Gates

**Developer Workstation Checks:**

- ✅ All unit tests pass
- ✅ Code compiles without warnings
- ✅ Code formatting standards met (using .editorconfig)
- ✅ No obvious code smells detected

**Pre-Commit Hook (Optional):**

```bash
#!/bin/bash
# .git/hooks/pre-commit

echo "Running pre-commit tests..."
dotnet test --filter "Category=Unit" --no-build

if [ $? -ne 0 ]; then
    echo "❌ Unit tests failed. Commit aborted."
    exit 1
fi

echo "✅ Pre-commit checks passed!"
exit 0
```

### Pre-Merge Quality Gates (Pull Request)

**Required Checks:**

- ✅ All tests pass (unit + integration + E2E)
- ✅ Code coverage ≥ 80% (overall)
- ✅ Code coverage ≥ 85% (for services)
- ✅ No critical or high-severity security vulnerabilities
- ✅ No code duplication > 3%
- ✅ Build succeeds on CI/CD pipeline
- ✅ Code review approval from at least 1 reviewer

**Branch Protection Rules:**

```yaml
branch_protection:
  required_status_checks:
    - "unit-tests"
    - "integration-tests"
    - "e2e-tests"
    - "quality-gate"
  required_pull_request_reviews:
    required_approving_review_count: 1
  enforce_admins: true
```

### Pre-Deployment Quality Gates

**Staging Environment:**

- ✅ All automated tests pass
- ✅ Performance benchmarks within acceptable range
- ✅ Load testing scenarios completed successfully
- ✅ Security scanning completed (no critical issues)
- ✅ Manual exploratory testing completed
- ✅ Smoke tests pass

**Production Environment:**

- ✅ All staging quality gates passed
- ✅ Canary deployment successful (if applicable)
- ✅ Rollback plan documented and tested
- ✅ Monitoring and alerting configured
- ✅ Production readiness checklist completed

## Tools and Technologies

### Testing Frameworks and Libraries

| Tool                                 | Purpose                   | Version | License      |
| ------------------------------------ | ------------------------- | ------- | ------------ |
| **xUnit**                            | Primary testing framework | 2.6.6   | Apache 2.0   |
| **Moq**                              | Mocking framework         | 4.20.70 | BSD-3-Clause |
| **FluentAssertions**                 | Assertion library         | 6.12.0  | Apache 2.0   |
| **Microsoft.AspNetCore.Mvc.Testing** | Integration testing       | 8.0.1   | MIT          |
| **AutoFixture**                      | Test data generation      | 4.18.1  | MIT          |
| **Bogus**                            | Fake data generation      | 35.4.0  | MIT          |
| **coverlet.collector**               | Code coverage             | 6.0.0   | MIT          |
| **ReportGenerator**                  | Coverage visualization    | 5.2.2   | Apache 2.0   |
| **BenchmarkDotNet**                  | Performance benchmarking  | 0.13.12 | MIT          |

### Development Tools

- **Visual Studio 2022** or **VS Code**: Primary IDE
- **ReSharper** or **Rider**: Advanced code analysis (optional)
- **NCrunch**: Continuous test runner (optional)
- **dotCover**: Code coverage analysis (optional)

### CI/CD Tools

- **GitHub Actions**: Primary CI/CD platform
- **Codecov**: Code coverage reporting and tracking
- **SonarCloud**: Code quality and security analysis (optional)

### Monitoring and Reporting

- **Azure Application Insights**: APM and monitoring
- **Coveralls**: Coverage tracking and history
- **Test Results Dashboard**: Custom HTML reports

## Next Steps

### Immediate Actions (This Week)

1. **Review and Approve Plan**: Stakeholder review of this document
2. **Set Up Test Project**: Create PostHubAPI.Tests project structure
3. **Install Dependencies**: Add all required NuGet packages
4. **Create First Test**: Write initial unit test as proof of concept

### Short-Term Actions (Next 2 Weeks)

5. **Implement Unit Tests**: Focus on service layer (highest ROI)
6. **Set Up CI/CD**: Configure GitHub Actions for automated testing
7. **Configure Coverage**: Enable code coverage reporting
8. **Team Training**: Conduct testing workshop for team members

### Medium-Term Actions (Next 4 Weeks)

9. **Complete Integration Tests**: Implement all endpoint tests
10. **Add E2E Tests**: Cover critical user journeys
11. **Performance Testing**: Set up benchmarks and load tests
12. **Documentation**: Complete test documentation and guidelines

### Long-Term Actions (Next 8 Weeks)

13. **Optimize Test Suite**: Improve test execution speed
14. **Advanced Scenarios**: Add security and chaos testing
15. **Continuous Improvement**: Regular review and enhancement
16. **Metrics Tracking**: Establish quality metrics dashboard

---

## Appendix

### Glossary

- **SUT**: System Under Test
- **AAA**: Arrange-Act-Assert (test pattern)
- **TDD**: Test-Driven Development
- **BDD**: Behavior-Driven Development
- **CI/CD**: Continuous Integration/Continuous Deployment
- **APM**: Application Performance Monitoring
- **P50/P95/P99**: Percentile response times

### Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions Documentation](https://fluentassertions.com/introduction)
- [Microsoft Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/)
- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)

### Contact and Support

For questions or issues related to this test automation plan:

- **Project Lead**: [Name]
- **QA Lead**: [Name]
- **DevOps Lead**: [Name]

---

**Document Prepared By**: DevTest Engineering Team
**Last Updated**: February 11, 2026
**Version**: 1.0.0
**Status**: Active
