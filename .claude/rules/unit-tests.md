---
description: xUnit unit test conventions for domain entities and application services
globs: ["tests/UnitTests/**/*.cs"]
alwaysApply: false
---

# Unit Test Rules

## Naming Convention — No Exceptions

```
{MethodName}_When{Condition}_Should{ExpectedResult}
```

```csharp
// ✅ Correct
GetById_WhenPlayerDoesNotExist_ShouldReturnNull()
Constructor_WhenPriceIsZero_ShouldThrowArgumentException()
Activate_WhenGameWeekIsCompleted_ShouldThrowInvalidOperationException()

// ✗ Wrong — missing When/Should structure
GetAll_ReturnsOk()
GetAllPlayers_ReturnsMappedDtos()
```

## Which Attribute to Use

| Test type | Attribute |
|-----------|-----------|
| Domain entity — single scenario | `[Fact]` |
| Domain entity — data-driven (multiple inputs) | `[Theory]` + `[InlineData(...)]` |
| Service or controller test (needs mocks) | `[Theory, AutoMockData]` |

> Never use bare `[Fact]` for service/controller tests — always `[Theory, AutoMockData]` so AutoFixture injects mocks.

## Service & Controller Tests (`[Theory, AutoMockData]`)

Mocks are injected as `Mock<T>` **parameters** — no constructor fields, no `_fixture.Freeze<>()`.

```csharp
[Theory, AutoMockData]
public async Task GetById_WhenPlayerExists_ShouldReturnDto(Mock<IPlayerRepository> mockRepo)
{
    // Arrange
    var player = new Player("Salah", "Forward", "Liverpool", 13m);
    mockRepo.Setup(r => r.GetByIdAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player);
    var sut = new PlayerService(mockRepo.Object);

    // Act
    var result = await sut.GetPlayerByIdAsync(player.Id);

    // Assert
    result.Should().NotBeNull();
    result!.Name.Should().Be(player.Name);
}
```

- Always include `// Arrange`, `// Act`, `// Assert` comments
- Use `It.IsAny<CancellationToken>()` for all `CancellationToken` arguments in Setup
- Use `.Verify(..., Times.Once)` only when testing a side-effect (add, update, delete)
- Use `.Invoking(s => s.Method()).Should().ThrowAsync<TException>()` for exception assertions

## Domain Entity Tests (`[Fact]` / `[Theory, InlineData]`)

```csharp
// Multiple equivalent inputs → [Theory, InlineData]
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-100)]
public void Constructor_WhenWeekNumberIsZeroOrNegative_ShouldThrowArgumentException(int weekNumber)
{
    // Arrange
    var start = DateTime.UtcNow;

    // Act
    Action act = () => new GameWeek(weekNumber, start, start.AddDays(7));

    // Assert
    act.Should().Throw<ArgumentException>()
       .WithMessage("*Week number must be greater than zero*");
}

// Single scenario → [Fact]
[Fact]
public void Activate_WhenGameWeekIsCompleted_ShouldThrowInvalidOperationException()
{
    // Arrange
    var gw = new GameWeek(1, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
    gw.Activate();
    gw.Complete();

    // Act
    Action act = () => gw.Activate();

    // Assert
    act.Should().Throw<InvalidOperationException>()
       .WithMessage("*Cannot activate a completed game week*");
}
```

## Namespaces

- Service and controller tests: `namespace UnitTests;`
- Domain entity tests: `namespace UnitTests.Domain.Entities;`

## `AutoMockDataAttribute` Location

Defined once in `tests/UnitTests/AutoMockDataAttribute.cs`. Do not redefine or duplicate it.
