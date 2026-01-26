# Contributing to GameTelemetry

Thanks for your interest in contributing! This guide will help you get started.

## Development Setup

### Prerequisites

- .NET 8 SDK or later
- Windows (for memory-mapped file testing)
- A supported game (optional, for testing)

### Getting Started

```bash
git clone https://github.com/yourusername/GameTelemetry.git
cd GameTelemetry
dotnet restore
dotnet build
dotnet test  # When tests exist
```

## Code Style

### General Principles

- **Minimal complexity** - Prefer simple, readable code over clever abstractions
- **Performance matters** - This code runs in tight loops; avoid allocations
- **Type safety** - Use strong typing and avoid `dynamic` or excessive reflection

### Naming Conventions

- **Sources:** `[GameName]Sources.cs` with static factory methods
- **Data structures:** `[GameName][DataType].cs` (e.g., `ACCPhysics.cs`)
- **Methods:** PascalCase, descriptive names
- **Private fields:** `_camelCase` with underscore prefix

### Example

```csharp
// ✅ Good
public static class iRacingSources
{
    public static MemoryMappedFileSource<iRacingTelemetry> CreateTelemetrySource() =>
        new("Local\\IRSDKMemMapFileName", TimeSpan.FromMilliseconds(16));
}

// ❌ Bad - unclear naming, no factory pattern
public class IRacingStuff
{
    public object GetData() { ... }
}
```

## Adding New Features

### Before Starting

1. Open an issue to discuss the feature
2. Check if it aligns with project goals (performance, simplicity, extensibility)
3. Consider backward compatibility

### Pull Request Process

1. **Fork** the repository
2. **Create a feature branch:** `git checkout -b feature/your-feature-name`
3. **Write code** following style guidelines
4. **Test thoroughly** (manual testing required until test suite exists)
5. **Update documentation** (README, code comments, etc.)
6. **Submit PR** with clear description

### PR Title Format

```
[Type] Brief description

Types: Feature, Fix, Docs, Perf, Refactor
Examples:
- [Feature] Add iRacing telemetry source
- [Fix] Handle corrupted session files gracefully
- [Perf] Reduce allocations in hot path
```

## Adding Game Integrations

See [CREATING_SOURCES.md](docs/CREATING_SOURCES.md) for detailed guide.

**Quick checklist:**

- [ ] Create `GameTelemetry.Games.[GameName]` project
- [ ] Define data structures (use `unsafe struct` for memory-mapped data)
- [ ] Create `[GameName]Sources.cs` with factory methods
- [ ] Add usage example to main README
- [ ] Test with actual game

## Performance Guidelines

### Critical Rules

1. **Hot path = zero allocations** - `WriteFrame` and enumeration should not allocate
2. **Use `Span<T>` and `stackalloc`** for temporary buffers
3. **Flush strategically** - Balance data safety vs. performance
4. **Profile before optimizing** - Measure, don't guess

### Measuring Performance

```csharp
// Use BenchmarkDotNet for micro-benchmarks
[Benchmark]
public void WriteFrame()
{
    _writer.WriteFrame(_testData, DateTime.UtcNow.Ticks);
}
```

## Testing

Currently manual testing. Automated test suite coming soon.

**Manual test checklist:**

- [ ] Capture runs without errors
- [ ] File grows during capture
- [ ] Ctrl+C stops cleanly
- [ ] Session can be read back
- [ ] CSV export works
- [ ] HTML visualization loads

## Documentation

### When to Update Docs

- New feature → Update README + create example
- API change → Update API_REFERENCE.md
- New game → Update supported games list
- Performance improvement → Update performance metrics

### Writing Style

- **Be concise** - Developers value their time
- **Show, don't tell** - Code examples > long explanations
- **Be honest** - Document limitations and tradeoffs

## Questions?

- **General questions:** Open a discussion
- **Bug reports:** Open an issue with repro steps
- **Feature ideas:** Open an issue for discussion first

## Code of Conduct

Be respectful, constructive, and professional. We're here to build cool stuff together.
