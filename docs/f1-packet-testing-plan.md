# F1 Packet Parsing Test Implementation Plan

## Overview

Create comprehensive automated tests for F1 packet parsing that:
- Automatically test all packet types across all game years (2023-2025)
- Gracefully skip missing fixture files
- Test event packet subdivisions by event code
- Leave room for custom/manual test scenarios
- Use xUnit's Theory + MemberData pattern (C#'s "data provider" concept)

## Current State

**Fixtures Available:**
- Location: `GamesDat.Tests/Fixtures/F1/<year>/packet-<id>.bin`
- Years: 2023 (13 files), 2024 (13 files), 2025 (29 files)
- Event subdivisions: Only F1 2025 has event-specific packets (packet-3-SSTA.bin, packet-3-COLL.bin, etc.)

**F1 Implementation:**
- 16 packet types (PacketId enum 0-15): Motion, Session, LapData, Event, Participants, etc.
- Type mapping via `F1PacketTypeMapper.cs`
- Deserialization API in `F1TelemetryFrameExtensions.cs`:
  - `frame.GetPacket<T>()` - Type-safe generic deserialization
  - `frame.DeserializePacket()` - Dynamic deserialization using type mapper

**Testing Infrastructure:**
- xUnit framework with Theory/MemberData support
- Pattern reference: `FileWatcherSourceDiscovery.cs`

## Implementation Steps

### Step 1: Create Fixture Discovery Helper

**File:** `GamesDat.Tests/Helpers/F1TestFixtureDiscovery.cs`

Create a helper class that:
1. Scans `Fixtures/F1/<year>/*.bin` to discover all fixture files
2. Parses filenames using regex: `packet-(\d+)(?:-([A-Z]{4}))?\.bin`
3. Extracts metadata: year (from directory), packetId, optional eventCode
4. Maps to expected types via `F1PacketTypeMapper.GetPacketType(year, packetId)`
5. Filters out missing files gracefully
6. Caches results for performance (thread-safe with lock)

**Key Classes:**

```csharp
public class F1FixtureMetadata
{
    public string FilePath { get; set; }
    public int Year { get; set; }
    public byte PacketId { get; set; }
    public string? EventCode { get; set; }
    public Type? ExpectedType { get; set; }
    public bool IsEventPacket => PacketId == 3;
}

public static class F1TestFixtureDiscovery
{
    private static List<F1FixtureMetadata>? _cachedFixtures;
    private static readonly object _lock = new();

    public static List<F1FixtureMetadata> DiscoverAllFixtures()
    {
        // Scan Fixtures/F1/<year>/*.bin
        // Parse filenames and extract metadata
        // Map to types via F1PacketTypeMapper
        // Filter File.Exists() checks
        // Cache results
    }

    // MemberData provider methods
    public static IEnumerable<object[]> AllFixtures()
    public static IEnumerable<object[]> StandardPacketFixtures()
    public static IEnumerable<object[]> EventPacketFixtures()
}
```

**Implementation Details:**
- Base path: `Path.Combine(AppContext.BaseDirectory, "Fixtures", "F1")`
- Scan with `Directory.GetFiles(yearPath, "*.bin", SearchOption.TopDirectoryOnly)`
- Use `File.Exists()` to gracefully skip missing files
- For type mapping, call `F1PacketTypeMapper.GetPacketType(year, packetId)` via reflection or create test frames
- Cache with double-check locking pattern (similar to FileWatcherSourceDiscovery)

### Step 2: Create Test Class with Automated Tests

**File:** `GamesDat.Tests/F1/F1PacketParsingTest.cs`

Create comprehensive Theory tests using MemberData:

**Test 1: Basic Deserialization**
```csharp
[Theory]
[MemberData(nameof(F1TestFixtureDiscovery.AllFixtures), MemberType = typeof(F1TestFixtureDiscovery))]
public void DeserializePacket_AllFixtures_SuccessfullyDeserializes(
    string filePath, int year, byte packetId, string eventCode, Type expectedType)
{
    // Load binary fixture file
    // Create F1TelemetryFrame from raw data
    // Call frame.DeserializePacket()
    // Assert: result is not null
    // Assert: result.GetType() == expectedType
    // Assert: no exceptions thrown
}
```

**Test 2: Type-Safe Deserialization**
```csharp
[Theory]
[MemberData(nameof(F1TestFixtureDiscovery.AllFixtures), MemberType = typeof(F1TestFixtureDiscovery))]
public void GetPacket_WithCorrectType_ReturnsValidStruct(
    string filePath, int year, byte packetId, string eventCode, Type expectedType)
{
    // Load binary fixture file
    // Create F1TelemetryFrame from raw data
    // Call frame.GetPacket<T>() using expectedType via reflection
    // Assert: struct is not all zeros (basic sanity)
    // Assert: no exceptions thrown
}
```

**Test 3: Header Validation**
```csharp
[Theory]
[MemberData(nameof(F1TestFixtureDiscovery.AllFixtures), MemberType = typeof(F1TestFixtureDiscovery))]
public void PacketHeader_AllFixtures_ContainsValidMetadata(
    string filePath, int year, byte packetId, string eventCode, Type expectedType)
{
    // Load binary fixture file
    // Create F1TelemetryFrame from raw data
    // Assert: frame.PacketFormat == year
    // Assert: frame.PacketId == packetId
    // Assert: frame.DataLength > 0
}
```

**Test 4: Event Code Validation (Event Packets Only)**
```csharp
[Theory]
[MemberData(nameof(F1TestFixtureDiscovery.EventPacketFixtures), MemberType = typeof(F1TestFixtureDiscovery))]
public void EventPacket_WithEventCode_MatchesFixtureName(
    string filePath, int year, string eventCode, Type expectedType)
{
    // Load binary fixture file
    // Create F1TelemetryFrame and deserialize to event packet type
    // Extract EventCode field from packet (e.g., via reflection or dynamic)
    // Assert: extracted EventCode == eventCode from filename
}
```

### Step 3: Add Custom/Manual Test Scenarios

Add specialized tests for deeper validation:

**Test 5: File Size Validation**
```csharp
[Theory]
[MemberData(nameof(F1TestFixtureDiscovery.AllFixtures), MemberType = typeof(F1TestFixtureDiscovery))]
public void FixtureFileSize_AllFixtures_MatchesOrExceedsExpectedSize(
    string filePath, int year, byte packetId, string eventCode, Type expectedType)
{
    // Get file size with FileInfo
    // Get expected size with Marshal.SizeOf(expectedType)
    // Assert: fileSize >= expectedSize (allow for padding/extra data)
}
```

**Test 6: Specific Field Validation (Examples)**
```csharp
[Fact]
public void PacketMotionData_2025_ContainsValidWorldPositions()
{
    // Load specific fixture: Fixtures/F1/2025/packet-0.bin
    // Deserialize to F12025.MotionData
    // Assert: position coordinates are reasonable (not all zeros, within plausible range)
    // Assert: specific fields like m_worldPositionX have non-zero values
}

[Fact]
public void PacketEventData_CollisionEvent_ContainsVehicleIndices()
{
    // Load specific fixture: Fixtures/F1/2025/packet-3-COLL.bin
    // Deserialize to F12025.EventData
    // Assert: EventCode == "COLL"
    // Assert: Collision data contains valid vehicle indices (0-21)
}
```

**Test 7: Cross-Year Consistency (Optional)**
```csharp
[Theory]
[InlineData(2023, 2024)]
[InlineData(2024, 2025)]
public void CommonPackets_AcrossYears_HaveSimilarStructure(int year1, int year2)
{
    // Load same packet ID from different years (e.g., packet-0.bin from 2023 and 2024)
    // Deserialize both
    // Assert: Both deserialize successfully
    // Optional: Compare struct sizes to detect breaking changes
}
```

### Step 4: Helper Methods for Frame Creation

Add helper methods in the test class to create `F1TelemetryFrame` from fixture files:

```csharp
private static F1TelemetryFrame CreateFrameFromFixture(string filePath)
{
    var bytes = File.ReadAllBytes(filePath);

    // Create frame and populate from bytes
    // Extract header: PacketFormat (bytes 0-1), PacketId (byte 6)
    // Copy data to RawData buffer
    // Return populated frame
}
```

**Important:** Handle unsafe code properly since `F1TelemetryFrame` uses fixed buffers.

## Critical Files to Create/Modify

1. **Create:** `GamesDat.Tests/Helpers/F1TestFixtureDiscovery.cs`
   - Fixture discovery and metadata extraction
   - MemberData provider methods

2. **Create:** `GamesDat.Tests/F1/F1PacketParsingTest.cs`
   - Theory tests with MemberData
   - Custom Fact tests for specialized scenarios
   - Helper methods for frame creation

3. **Reference (Read-only):**
   - `F1PacketTypeMapper.cs` - Type mapping logic
   - `F1TelemetryFrameExtensions.cs` - Deserialization API
   - `FileWatcherSourceDiscovery.cs` - Pattern reference

## Key Design Decisions

**1. Graceful Skipping**
- Discovery filters out non-existent files before yielding test data
- Tests never receive invalid file paths
- No test failures due to missing fixtures

**2. Event Packet Handling**
- Separate MemberData provider: `EventPacketFixtures()`
- Only includes fixtures with event code subdivision (packet-3-XXXX.bin)
- Standard tests skip event code validation for non-event packets

**3. Type Mapping Approach**
- Use `F1PacketTypeMapper.GetPacketType(year, packetId)` during discovery
- Store expected type in metadata
- Tests validate returned type matches expected type

**4. MemberData Pattern**
- Static methods in discovery class return `IEnumerable<object[]>`
- Each object array contains test parameters: filePath, year, packetId, eventCode, expectedType
- Reference via `[MemberData(nameof(Method), MemberType = typeof(Class))]`

**5. Frame Creation**
- Tests read binary fixture files directly
- Create `F1TelemetryFrame` instances by copying data
- Extract header info from first 29 bytes (F1 2025 format)

## Verification & Testing

After implementation, verify:

1. **Run all tests:** `dotnet test --filter "F1PacketParsingTest"`
2. **Check coverage:** All 55 fixture files should generate test cases
3. **Verify graceful skipping:** Add/remove fixture files and rerun tests
4. **Inspect test output:** Should show which fixtures were tested
5. **Performance:** Tests should complete quickly (cached discovery, minimal file I/O)

**Expected Results:**
- ~55 test cases from automated Theory tests (varies by fixture count)
- All tests pass for existing fixtures
- No failures from missing fixtures
- Custom tests provide deeper validation of specific scenarios

## Edge Cases & Considerations

1. **Missing Type Mappings:** Some (year, packetId) combinations may not exist in mapper
   - **Handled by:** Discovery filters where `GetPacketType()` returns null

2. **File Size Mismatches:** Binary files might not exactly match struct sizes
   - **Handled by:** Use `>=` comparison, validate minimum size only

3. **Event Code Extraction:** Need to read EventCode from deserialized packet
   - **Approach:** Cast to specific year's EventData type, read EventCode field

4. **F1 2022 Fixtures:** No fixture directory exists for 2022
   - **Handled by:** Discovery only scans existing directories

5. **Unsafe Code:** F1TelemetryFrame uses fixed buffers
   - **Handled by:** Ensure test methods are marked unsafe if needed, use proper marshaling

## Benefits of This Approach

1. **Automated Scale:** Tests all 55 fixtures without manual enumeration
2. **Maintainable:** Adding new fixtures requires no code changes
3. **Consistent:** Follows xUnit conventions and existing test patterns
4. **Extensible:** Easy to add custom tests alongside automated ones
5. **Debuggable:** Clear test names indicate which fixture failed
6. **Resilient:** Gracefully handles missing fixtures without test failures

## Future Enhancements

1. **Fixture Generation:** Tools to capture real game packets and generate fixture files
2. **Golden Master Testing:** Compare deserialized output against known-good JSON
3. **Fuzz Testing:** Generate random binary data to test robustness
4. **Performance Benchmarks:** Measure deserialization speed at scale
5. **Visual Regression:** Compare packet data visualizations across versions
