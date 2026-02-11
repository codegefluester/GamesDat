# F1 UDP Telemetry Specifications

This directory stores the official C++ specification files from EA/Codemasters for F1 game UDP telemetry.

## Naming Convention

Place specification files here using this naming pattern:
- `F1_<YEAR>_spec.h` (e.g., `F1_2026_spec.h`)
- `F1<YEAR>_spec.h` (e.g., `F12026_spec.h`)
- `<YEAR>_udp_spec.h` (e.g., `2026_udp_spec.h`)

## Usage

The `/f1-implementation` skill automatically looks for specifications in this directory first. If your spec file follows the naming convention above, it will be found automatically.

## Obtaining Specifications

Official F1 game UDP specifications are typically:
- Provided by EA/Codemasters in game documentation
- Available in the game installation directory
- Published on official forums or GitHub repositories
- Included in game modding documentation

## Example

```
Specifications/
├── F1_2024_spec.h    # F1 2024 specification
├── F1_2025_spec.h    # F1 2025 specification
└── F1_2026_spec.h    # F1 2026 specification (future)
```
