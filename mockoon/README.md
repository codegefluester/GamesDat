# War Thunder Mockoon Environment

This directory contains Mockoon environment configurations for testing the War Thunder integration without having the game installed.

## What is Mockoon?

[Mockoon](https://mockoon.com/) is a free desktop application that lets you create mock REST APIs in seconds. It's perfect for testing integrations when the real service isn't available.

## Installation

### Desktop App (Recommended)
1. Download Mockoon from https://mockoon.com/download/
2. Install and launch the application

### CLI (Alternative)
```bash
npm install -g @mockoon/cli
```

## Setup

### Using Desktop App

1. **Launch Mockoon**
2. **Import the environment:**
   - Click "Open environment" → "Import from file"
   - Select `war-thunder-environment.json`
3. **Start the mock server:**
   - Click the green "Start server" button
   - The API will run on `http://localhost:8111`

### Using CLI

```bash
mockoon-cli start --data war-thunder-environment.json
```

## Testing the Mock API

Once the server is running, you can test it:

### Browser
Navigate to:
- http://localhost:8111/state
- http://localhost:8111/indicators

### cURL
```bash
curl http://localhost:8111/state
curl http://localhost:8111/indicators
```

### With GamesDat
```csharp
using GamesDat.Core;
using GamesDat.Core.Telemetry.Sources.WarThunder;

// Use the mock API just like the real one
await using var session = new GameSession()
    .AddSource(WarThunderSources.CreateStateSource())
    .AddSource(WarThunderSources.CreateIndicatorsSource())
    .OnData<StateData>(data =>
        Console.WriteLine($"[State] IAS: {data.IndicatedAirspeed} km/h, Alt: {data.Altitude}m"))
    .OnData<IndicatorsData>(data =>
        Console.WriteLine($"[Indicators] Oil: {data.OilTemp}°C, Water: {data.WaterTemp}°C"))
    .AutoOutput();

await session.StartAsync();
```

## Available Endpoints

### `/state` - Primary Flight/Vehicle Telemetry
**Recommended polling rate:** 60Hz

Contains:
- Position (X, Y, Z) in meters
- Velocity (Vx, Vy, Vz) in m/s
- Angular velocity (Wx, Wy, Wz) in rad/s
- Flight parameters (AoA, AoS, IAS, TAS, Mach, Altitude)
- G-force (Ny)
- Engine data (Throttle, RPM, Manifold Pressure, Power)
- Control surfaces (Flaps, Gear, Airbrake)
- Navigation (Compass)
- Fuel in kg
- Timestamp

### `/indicators` - Cockpit Instrumentation
**Recommended polling rate:** 10Hz

Contains:
- Speed indicators
- Engine instruments (RPM, Manifold Pressure, Oil/Water Temp)
- Attitude indicator (Roll, Pitch)
- Altimeter (Hour, Min, 10k hands)
- Vertical speed
- Compass
- Clock (Hour, Min, Sec)

## Response Scenarios

Each endpoint has multiple response scenarios you can switch between in Mockoon:

### `/state` Responses

1. **Flying - Dynamic Values (Default)**
   - Realistic flying values with randomized data
   - Valid = 1
   - Speed: 250-600 km/h
   - Altitude: 500-4000m
   - All systems active

2. **On Ground - Idle**
   - Aircraft on the ground with engine idling
   - Valid = 1
   - All velocities = 0
   - RPM = 800 (idle)
   - Gear down

3. **Not in Match**
   - Simulates being in hangar/menu
   - Valid = 0
   - All values = 0

### `/indicators` Responses

1. **Flying - Dynamic Values (Default)**
   - Realistic instrument readings with randomized data
   - Valid = 1
   - Speed: 250-600 km/h
   - Temperatures in normal operating range

2. **On Ground - Idle**
   - Instruments showing idle state
   - Valid = 1
   - Speed = 0
   - Cold engine temperatures

3. **Not in Match**
   - Simulates being in hangar/menu
   - Valid = 0
   - All values = 0

## Switching Response Scenarios

### Desktop App
1. Click on the endpoint (`/state` or `/indicators`)
2. In the right panel, you'll see "Responses"
3. Select the response you want to use
4. Click the "Set as default" button (star icon)

### CLI
The CLI uses the default response automatically. To use different responses, you'll need to modify the JSON file and set `"default": true` on the desired response.

## Dynamic Data with Faker.js

The default "Flying" responses use Mockoon's templating system with [Faker.js](https://fakerjs.dev/) to generate realistic random values:

- Values change on each request
- Ranges match realistic flight parameters
- Timestamps are current system time

Example template syntax:
```json
{
  "IAS": {{faker 'number.float' min=250 max=600 precision=0.1}}
}
```

This generates a random float between 250-600 with 0.1 precision on each request.

## Simulating Different Flight Scenarios

You can create custom responses for specific testing scenarios:

### High-Speed Flight
Modify the `/state` endpoint to return higher speeds:
- IAS: 800-1200 km/h
- Mach: 1.2-2.0
- Altitude: 8000-12000m

### Emergency Scenarios
- Low fuel: Set `fuel` to 50-100
- High G-force: Set `Ny` to 5.0-9.0
- Engine failure: Set `RPM` and `power` to 0

### Aerobatics
- High angular velocities: `Wx`, `Wy`, `Wz` = -2.0 to 2.0
- Extreme AoA: -20° to 30°
- Varying G-force: -2.0 to 8.0

## Testing Retry Logic

To test GamesDat's retry and error handling:

1. **Simulate game not running:**
   - Stop the Mockoon server
   - Your code should retry with exponential backoff

2. **Simulate connection drops:**
   - Start the server
   - Wait for connection to establish
   - Stop the server
   - Should automatically retry and reconnect when restarted

3. **Simulate invalid data:**
   - Switch to "Not in Match" response
   - `valid` field will be 0

## Performance Testing

The mock API can handle high-frequency polling:

- **60Hz `/state` polling:** No problem
- **10Hz `/indicators` polling:** No problem
- **Both simultaneously:** Works great

Monitor your application's performance while polling at these rates.

## Advanced: Custom Responses

You can create your own response scenarios:

1. In Mockoon, click "Add response" for an endpoint
2. Configure the response body with your custom JSON
3. Optionally add rules for conditional responses
4. Set status codes, headers, and latency

### Example: Damaged Aircraft
```json
{
  "valid": 1,
  "IAS": 180,
  "RPM": 1500,
  "oil_temp": 150.0,
  "water_temp": 130.0,
  "fuel": 50.0,
  "power": 0.4
}
```

### Example: Takeoff Roll
```json
{
  "valid": 1,
  "IAS": 120,
  "TAS": 130,
  "H": 2.0,
  "throttle": 1.0,
  "RPM": 2800,
  "gear": 1.0,
  "flaps": 1.0
}
```

## Troubleshooting

### Port Already in Use
If port 8111 is already taken:
1. In Mockoon, click on the environment settings (gear icon)
2. Change "Port" to another value (e.g., 8112)
3. Update your GamesDat code to use the new port:
   ```csharp
   WarThunderSources.CreateStateSource(baseUrl: "http://localhost:8112")
   ```

### CORS Issues
CORS is enabled by default in this environment. If you have issues:
1. Check the environment settings
2. Ensure "Enable CORS" is checked
3. Restart the server

### Templating Not Working
If Faker.js templates aren't generating random values:
1. Ensure "Disable templating" is unchecked on the response
2. Check that the syntax matches Mockoon's templating format
3. Restart the mock server

## Files in this Directory

- `war-thunder-environment.json` - Main Mockoon environment configuration
- `README.md` - This file

## See Also

- [War Thunder Integration Documentation](../docs/WarThunder.md)
- [Mockoon Documentation](https://mockoon.com/docs/latest/about/)
- [Mockoon CLI](https://mockoon.com/cli/)
- [Faker.js Documentation](https://fakerjs.dev/)

## Tips for Realistic Testing

1. **Use the "Flying" responses as default** - They provide dynamic, realistic data
2. **Test with both endpoints simultaneously** - This is how you'll use it in production
3. **Monitor CPU usage** - Should be <1% even at 60Hz polling
4. **Test the retry logic** - Stop/start the server to verify automatic reconnection
5. **Check the `valid` field** - Always verify it's 1 before processing data
6. **Watch for edge cases** - Test with extreme values (high G, low fuel, etc.)

Happy testing! 🎮✈️
