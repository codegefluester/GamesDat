# F1 UDP Telemetry Implementation Workflow

This document describes the workflow for implementing F1 game UDP telemetry packet structures from C++ specifications into C# structs for the GamesDat project.

## Overview

The F1 games send UDP telemetry data using a specific binary format defined in C++ header files. This workflow guides the process of converting those C++ specifications into C# structs that can deserialize the UDP packets.

## Prerequisites

- C++ specification file (typically provided by EA/Codemasters)
- Understanding of C struct layout and C# interop
- Existing F1 implementation to use as reference (e.g., F12025)

## Workflow Steps

### 1. Analyze the C++ Specification

Review the C++ header file to understand:
- **Packet format version** (e.g., 2024, 2025)
- **Packet sizes** (documented in comments)
- **Constants** (e.g., `cs_maxNumCarsInUDPData = 22`)
- **Struct hierarchy** (which structs contain others)
- **Array sizes** (fixed-size arrays in structs)

### 2. Create the Namespace Folder

Create a new folder under `Telemetry/Sources/Formula1/` with the naming pattern `F1YYYY` (e.g., `F12024`, `F12025`).

### 3. Implement the PacketHeader

Start with the `PacketHeader` struct as it's used by all packet types:

```csharp
using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F1YYYY
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketHeader
    {
        public ushort m_packetFormat;             // YYYY
        public byte m_gameYear;                   // Last two digits e.g. 24
        // ... other fields from spec
    }
}
```

**Key points:**
- Always use `[StructLayout(LayoutKind.Sequential, Pack = 1)]`
- Keep field names exactly as in C++ spec (helps with debugging)
- Add inline comments from the C++ spec

### 4. Implement Support Structures

Create the smaller, reusable structs before the main packet types:

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
    public string EventCode => Encoding.ASCII.GetString(m_eventStringCode);

    // Helper to safely extract typed event details
    public T GetEventDetails<T>() where T : struct { ... }
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

Add mappings for all packet types to `F1PacketTypeMapper.cs`:

```csharp
private static readonly Dictionary<(ushort format, byte id), Type> _packetTypeMap = new()
{
    // F1 YYYY
    [(YYYY, (byte)PacketId.Motion)] = typeof(F1YYYY.PacketMotionData),
    [(YYYY, (byte)PacketId.Session)] = typeof(F1YYYY.PacketSessionData),
    // ... all packet types

    // F1 previous years
    // ...
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

### 10. Validation

After implementation:

1. **Build the project** - ensure no compilation errors
2. **Check struct sizes** - if possible, verify binary size matches spec comments
3. **Test with real data** - use actual game UDP packets to validate deserialization
4. **Compare with previous year** - look for differences and ensure they're intentional

## Common Patterns

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

## Best Practices

1. **Maintain exact field order** - C# struct layout must match C++ exactly
2. **Use Pack = 1** - prevents automatic padding/alignment
3. **Keep original naming** - makes comparison with spec easier
4. **Add XML comments** - for complex structs, add summary docs
5. **Preserve spec comments** - inline comments help understand field meaning
6. **Test incrementally** - validate each packet type as you implement it
7. **Reference existing implementations** - use previous years as templates
8. **Watch for year-specific changes** - array sizes, new fields, removed fields

## File Organization

Each year's implementation should be self-contained:

```
Formula1/
├── F12024/
│   ├── PacketHeader.cs
│   ├── PacketMotionData.cs
│   ├── CarMotionData.cs
│   ├── PacketSessionData.cs
│   ├── MarshalZone.cs
│   ├── WeatherForecastSample.cs
│   └── ... (all other packet types)
├── F12025/
│   └── ... (same structure)
├── F1PacketTypeMapper.cs
├── PacketId.cs (shared enum)
└── EventCodes.cs (shared constants)
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

## Example: Complete Implementation

See `F12024/` folder for a complete reference implementation following this workflow.

## Version History

- **2024-02-09**: Created workflow documentation based on F12024 implementation
- **Future**: Update as new patterns or edge cases are discovered

---

*This workflow is designed for GamesDat project contributors and AI agents implementing F1 telemetry support.*
