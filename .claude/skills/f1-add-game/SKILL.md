---
name: f1-add-game
description: Adds the required telemetry structs for a specific game year of the F1 game series made by EA/Codemasters, based on their C++ specification files.
---

# F1 UDP Telemetry Implementation Skill

You are an expert F1 UDP telemetry implementation specialist for the GamesDat project. Your role is to convert C++ specification files from EA/Codemasters into C# structs that can deserialize UDP packets.

## Invocation

When invoked with `/f1-implementation <year>` (e.g., `/f1-implementation 2026`), follow this workflow to implement F1 telemetry packet structures.

## Step 0: Locate the C++ Specification

**Before starting implementation, you MUST locate the C++ specification file using these strategies:**

### Strategy 1: Check Convention-Based Location
Look for specification files in these locations:
- `Telemetry/Sources/Formula1/Specifications/F1_<YEAR>_spec.h`
- `Telemetry/Sources/Formula1/Specifications/F1<YEAR>_spec.h`
- `Telemetry/Sources/Formula1/Specifications/<YEAR>_udp_spec.h`
- `Telemetry/Sources/Formula1/Docs/` directory

Use Glob tool to search:
```
pattern: "Telemetry/Sources/Formula1/**/*.h"
pattern: "Telemetry/Sources/Formula1/**/*.cpp"
pattern: "Telemetry/Sources/Formula1/**/*spec*"
pattern: "Telemetry/Sources/Formula1/**/*<YEAR>*"
```

### Strategy 2: Search Repository-Wide
If not found in Formula1 directory, search the entire repository:
```
pattern: "**/*F1*<YEAR>*.h"
pattern: "**/*udp*<YEAR>*.h"
```

### Strategy 3: Ask User for Specification
If specification is not found, ask the user:

**Use AskUserQuestion tool with these options:**
- **Question**: "I couldn't find the C++ specification file for F1 <YEAR>. How would you like to provide it?"
- **Header**: "Spec Location"
- **Options**:
  1. **Label**: "File path on disk", **Description**: "I'll provide the path to the .h or .cpp file"
  2. **Label**: "Paste content", **Description**: "I'll paste the C++ specification content directly"
  3. **Label**: "Download from URL", **Description**: "I'll provide a URL to download the spec"
  4. **Label**: "Use previous year as template", **Description**: "Copy from F1 <PREVIOUS_YEAR> and I'll modify manually"

### Strategy 4: Handle User Response
- **If file path provided**: Read the file using Read tool
- **If content pasted**: Proceed with the pasted content
- **If URL provided**: Use WebFetch or Bash (curl) to download
- **If template requested**: Copy previous year's implementation and note differences for manual review

**IMPORTANT**: Do NOT proceed with implementation until you have the specification content in hand.

---

## Implementation Workflow

Once you have the C++ specification, follow these steps:

### 1. Analyze the C++ Specification

Review the spec to understand:
- **Packet format version** (e.g., 2024, 2025)
- **Packet sizes** (documented in comments)
- **Constants** (e.g., `cs_maxNumCarsInUDPData = 22`)
- **Struct hierarchy** (which structs contain others)
- **Array sizes** (fixed-size arrays in structs)

### 2. Create the Namespace Folder

Create: `Telemetry/Sources/Formula1/F1<YEAR>/`

Example: `Telemetry/Sources/Formula1/F12026/`

### 3. Implement the PacketHeader

Start with the `PacketHeader` struct as it's used by all packet types:

```csharp
using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F1<YEAR>
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketHeader
    {
        public ushort m_packetFormat;             // <YEAR>
        public byte m_gameYear;                   // Last two digits e.g. 26
        public byte m_gameMajorVersion;           // Game major version
        public byte m_gameMinorVersion;           // Game minor version
        public byte m_packetVersion;              // Version of this packet type
        public byte m_packetId;                   // Identifier for the packet type
        public ulong m_sessionUID;                // Unique identifier for the session
        public float m_sessionTime;               // Session timestamp
        public uint m_frameIdentifier;            // Identifier for the frame
        public uint m_overallFrameIdentifier;     // Overall identifier
        public byte m_playerCarIndex;             // Index of player's car
        public byte m_secondaryPlayerCarIndex;    // Index of secondary player's car
    }
}
```

**Key points:**
- Always use `[StructLayout(LayoutKind.Sequential, Pack = 1)]`
- Keep field names exactly as in C++ spec (helps with debugging)
- Add inline comments from the C++ spec

### 4. Implement Support Structures

Create the smaller, reusable structs before the main packet types.

Examples: `MarshalZone`, `WeatherForecastSample`, `CarMotionData`, etc.

**Pattern:**
```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StructName
{
    public type m_fieldName;  // comment from spec
    // ... more fields
}
```

### 5. Implement Main Packet Structures

For each packet type from the spec, create:

1. **Component struct** (e.g., `LapData`) - data for one car/item
2. **Packet struct** (e.g., `PacketLapData`) - complete packet with header and array

**Pattern:**
```csharp
// Component for one car
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ComponentData
{
    public type m_field1;
    public type m_field2;
}

// Full packet
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PacketComponentData
{
    public PacketHeader m_header;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
    public ComponentData[] m_dataArray;

    // ... any additional single fields
}
```

### 6. Handle Special Cases

#### Fixed-Size Arrays
```csharp
[MarshalAs(UnmanagedType.ByValArray, SizeConst = SIZE)]
public Type[] m_arrayField;
```

#### Strings/Character Arrays
```csharp
[MarshalAs(UnmanagedType.ByValArray, SizeConst = LENGTH)]
public byte[] m_name;  // UTF-8 null-terminated string
```

#### Unions (EventDataDetails)
Use `StructLayout(LayoutKind.Explicit)` with `FieldOffset`:

```csharp
[StructLayout(LayoutKind.Explicit)]
public struct EventDataDetails
{
    [FieldOffset(0)] public FastestLapData FastestLap;
    [FieldOffset(0)] public RetirementData Retirement;
    // ... all union members at offset 0
}
```

Then create a wrapper with helper methods:
```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PacketEventData
{
    public PacketHeader m_header;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] m_eventStringCode;

    public EventDataDetails m_eventDetails;

    // Helper to get event code as string
    public string EventCode => System.Text.Encoding.ASCII.GetString(m_eventStringCode);

    // Helper to safely extract typed event details
    public T GetEventDetails<T>() where T : struct
    {
        // Implementation for safe union access
    }
}
```

### 7. Type Mapping Reference

| C++ Type | C# Type | Notes |
|----------|---------|-------|
| `uint8` | `byte` | Unsigned 8-bit |
| `int8` | `sbyte` | Signed 8-bit |
| `uint16` | `ushort` | Unsigned 16-bit |
| `int16` | `short` | Signed 16-bit |
| `uint32` / `uint` | `uint` | Unsigned 32-bit |
| `int32` | `int` | Signed 32-bit |
| `uint64` | `ulong` | Unsigned 64-bit |
| `float` | `float` | 32-bit float |
| `double` | `double` | 64-bit float |
| `char[]` | `byte[]` | UTF-8 strings |

### 8. Update F1PacketTypeMapper

Add mappings for all packet types to `Telemetry/Sources/Formula1/F1PacketTypeMapper.cs`:

```csharp
private static readonly Dictionary<(ushort format, byte id), Type> _packetTypeMap = new()
{
    // F1 <YEAR>
    [(<YEAR>, (byte)PacketId.Motion)] = typeof(F1<YEAR>.PacketMotionData),
    [(<YEAR>, (byte)PacketId.Session)] = typeof(F1<YEAR>.PacketSessionData),
    [(<YEAR>, (byte)PacketId.LapData)] = typeof(F1<YEAR>.PacketLapData),
    [(<YEAR>, (byte)PacketId.Event)] = typeof(F1<YEAR>.PacketEventData),
    [(<YEAR>, (byte)PacketId.Participants)] = typeof(F1<YEAR>.PacketParticipantsData),
    [(<YEAR>, (byte)PacketId.CarSetups)] = typeof(F1<YEAR>.PacketCarSetupData),
    [(<YEAR>, (byte)PacketId.CarTelemetry)] = typeof(F1<YEAR>.PacketCarTelemetryData),
    [(<YEAR>, (byte)PacketId.CarStatus)] = typeof(F1<YEAR>.PacketCarStatusData),
    [(<YEAR>, (byte)PacketId.FinalClassification)] = typeof(F1<YEAR>.PacketFinalClassificationData),
    [(<YEAR>, (byte)PacketId.LobbyInfo)] = typeof(F1<YEAR>.PacketLobbyInfoData),
    [(<YEAR>, (byte)PacketId.CarDamage)] = typeof(F1<YEAR>.PacketCarDamageData),
    [(<YEAR>, (byte)PacketId.SessionHistory)] = typeof(F1<YEAR>.PacketSessionHistoryData),
    [(<YEAR>, (byte)PacketId.TyreSets)] = typeof(F1<YEAR>.PacketTyreSetsData),
    [(<YEAR>, (byte)PacketId.MotionEx)] = typeof(F1<YEAR>.PacketMotionExData),
    [(<YEAR>, (byte)PacketId.TimeTrial)] = typeof(F1<YEAR>.PacketTimeTrialData),
    // ... existing previous years ...
};
```

**Important:** Use fully qualified type names (e.g., `F12024.PacketMotionData`) to avoid conflicts between years.

### 9. Packet Types Checklist

Ensure you implement all standard packet types:

- [ ] Motion (`PacketMotionData`)
- [ ] Session (`PacketSessionData`)
- [ ] Lap Data (`PacketLapData`)
- [ ] Event (`PacketEventData`)
- [ ] Participants (`PacketParticipantsData`)
- [ ] Car Setups (`PacketCarSetupData`)
- [ ] Car Telemetry (`PacketCarTelemetryData`)
- [ ] Car Status (`PacketCarStatusData`)
- [ ] Final Classification (`PacketFinalClassificationData`)
- [ ] Lobby Info (`PacketLobbyInfoData`)
- [ ] Car Damage (`PacketCarDamageData`)
- [ ] Session History (`PacketSessionHistoryData`)
- [ ] Tyre Sets (`PacketTyreSetsData`)
- [ ] Motion Ex (`PacketMotionExData`)
- [ ] Time Trial (`PacketTimeTrialData`)

**Note:** Some years may have additional packet types or missing ones. Follow the specification exactly.

### 10. Validation

After implementation:

1. **Build the project** - `dotnet build` to ensure no compilation errors
2. **Check struct sizes** - if possible, verify binary size matches spec comments
3. **Compare with previous year** - look for differences and ensure they're intentional
4. **Create summary** - list all implemented packet types and any notable changes from previous year

---

## Best Practices

1. **Maintain exact field order** - C# struct layout must match C++ exactly
2. **Use Pack = 1** - prevents automatic padding/alignment
3. **Keep original naming** - makes comparison with spec easier
4. **Add XML comments** - for complex structs, add summary docs
5. **Preserve spec comments** - inline comments help understand field meaning
6. **Reference existing implementations** - use previous years as templates (F12024, F12025)
7. **Watch for year-specific changes** - array sizes, new fields, removed fields
8. **One file per packet type** - keeps code organized and maintainable

## File Organization Pattern

```
Formula1/
├── F1<YEAR>/
│   ├── PacketHeader.cs
│   ├── CarMotionData.cs
│   ├── PacketMotionData.cs
│   ├── MarshalZone.cs
│   ├── WeatherForecastSample.cs
│   ├── PacketSessionData.cs
│   ├── LapData.cs
│   ├── PacketLapData.cs
│   └── ... (all other packet types)
├── F1PacketTypeMapper.cs (update this)
├── PacketId.cs (shared enum)
└── EventCodes.cs (shared constants)
```

## Common Patterns Reference

### Pattern 1: Simple Data Struct
```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SimpleData
{
    public byte m_field1;
    public float m_field2;
}
```

### Pattern 2: Packet with Array
```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PacketWithArray
{
    public PacketHeader m_header;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
    public ItemData[] m_items;
}
```

### Pattern 3: Nested Arrays
```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DataWithArrays
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] m_tyresPressure;  // 4 tyres
}
```

### Pattern 4: Data + Extra Fields
```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PacketWithExtras
{
    public PacketHeader m_header;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
    public ItemData[] m_items;

    public byte m_extraField1;
    public byte m_extraField2;
}
```

## Troubleshooting

### Issue: Struct size doesn't match spec
- Check for missing `Pack = 1`
- Verify all arrays have correct `SizeConst`
- Ensure no fields were skipped
- Check type mappings (e.g., `int8` vs `uint8`)

### Issue: Deserialization fails
- Verify packet header format number matches
- Check PacketTypeMapper has correct entries
- Ensure union types use `LayoutKind.Explicit`
- Validate field order exactly matches spec

### Issue: Data seems corrupted
- Check endianness (should be little-endian)
- Verify `Pack = 1` is set
- Ensure no extra padding between fields
- Check array sizes match spec constants

---

## Execution Approach

When this skill is invoked:

1. **Extract the year** from the invocation (e.g., "2026" from `/f1-implementation 2026`)
2. **Locate the specification** using the strategies in Step 0
3. **Read existing implementations** (F12024, F12025) to understand patterns
4. **Implement all packet types** following the workflow above
5. **Update F1PacketTypeMapper** with new mappings
6. **Build and validate** the implementation
7. **Provide summary** of what was implemented

## Reference Implementations

Use these existing implementations as templates:
- `Telemetry/Sources/Formula1/F12024/` - Complete F1 2024 implementation
- `Telemetry/Sources/Formula1/F12025/` - Complete F1 2025 implementation
- `Telemetry/Sources/Formula1/F1_IMPLEMENTATION_WORKFLOW.md` - Original workflow documentation

---

*This skill automates F1 UDP telemetry implementation for GamesDat.*
