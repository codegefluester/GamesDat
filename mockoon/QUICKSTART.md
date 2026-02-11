# War Thunder Mock API - Quick Start Guide

Get up and running with the War Thunder mock API in 5 minutes.

## Prerequisites

- [Mockoon Desktop App](https://mockoon.com/download/) (free)
- OR [Mockoon CLI](https://mockoon.com/cli/) via npm

## Step 1: Start the Mock Server

### Option A: Desktop App (Recommended)

1. Download and install [Mockoon](https://mockoon.com/download/)
2. Launch Mockoon
3. Click **"Open environment"** → **"Import from file"**
4. Select `war-thunder-environment.json` from this directory
5. Click the green **"Start server"** button
6. The mock API is now running on `http://localhost:8111` ✅

### Option B: CLI

```bash
# Install Mockoon CLI globally
npm install -g @mockoon/cli

# Start the mock server
cd mockoon
mockoon-cli start --data war-thunder-environment.json
```

## Step 2: Verify It's Working

Open a browser or use curl:

```bash
curl http://localhost:8111/state
curl http://localhost:8111/indicators
```

You should see JSON responses with telemetry data.

## Step 3: Use with GamesDat

Create a simple C# console app:

```csharp
using GamesDat.Core;
using GamesDat.Core.Telemetry.Sources.WarThunder;

// Create a session with both War Thunder sources
await using var session = new GameSession()
    .AddSource(WarThunderSources.CreateStateSource(hz: 60))      // 60Hz polling
    .AddSource(WarThunderSources.CreateIndicatorsSource(hz: 10)) // 10Hz polling
    .OnData<StateData>(data =>
    {
        if (data.Valid == 1)
        {
            Console.WriteLine($"[State] Speed: {data.IndicatedAirspeed:F1} km/h | " +
                            $"Alt: {data.Altitude:F1}m | " +
                            $"Fuel: {data.Fuel:F0}kg");
        }
    })
    .OnData<IndicatorsData>(data =>
    {
        if (data.Valid == 1)
        {
            Console.WriteLine($"[Indicators] Oil: {data.OilTemp:F1}°C | " +
                            $"Water: {data.WaterTemp:F1}°C | " +
                            $"RPM: {data.RpmMin:F0}");
        }
    })
    .AutoOutput();

Console.WriteLine("Recording War Thunder telemetry (Mock API)");
Console.WriteLine("Press Ctrl+C to stop...");

await session.StartAsync();
```

Run it and you'll see:
```
Recording War Thunder telemetry (Mock API)
Press Ctrl+C to stop...
[State] Speed: 425.8 km/h | Alt: 2845.6m | Fuel: 542kg
[Indicators] Oil: 95.3°C | Water: 88.7°C | RPM: 2650
[State] Speed: 431.2 km/h | Alt: 2867.1m | Fuel: 541kg
[Indicators] Oil: 96.1°C | Water: 89.2°C | RPM: 2655
...
```

## What You Get

### `/state` endpoint (60Hz)
- **Dynamic values** that change on each request
- Realistic flight parameters (speed, altitude, G-force)
- Position and velocity vectors
- Engine data (throttle, RPM, power)
- Control surfaces (flaps, gear, airbrake)
- Fuel and navigation

### `/indicators` endpoint (10Hz)
- **Dynamic instrument readings**
- Engine temperatures (oil, water)
- Attitude indicator (roll, pitch)
- Altimeter readings
- Vertical speed
- Clock time

## Testing Different Scenarios

### Switch Response Types in Mockoon Desktop

1. Click on an endpoint (`/state` or `/indicators`)
2. In the right panel, select a different response:
   - **Flying - Dynamic Values** (default) - Active flight with random data
   - **On Ground - Idle** - Aircraft on the ground
   - **Not in Match** - In hangar/menu (valid=0)
3. Click the star icon to "Set as default"

### Scenario Ideas

**Test Takeoff:**
- Switch `/state` to "On Ground - Idle"
- Watch your app handle `valid=1` but zero velocity
- Switch to "Flying" to simulate takeoff
- See the transition in your telemetry

**Test Connection Loss:**
- Start recording with "Flying" response
- Stop the Mockoon server (simulates game crash)
- Watch your app's retry logic kick in
- Restart the server (simulates recovery)
- See automatic reconnection

**Test Invalid Data:**
- Switch to "Not in Match" response
- Your app should check `valid==0` and skip processing
- Useful for testing data validation logic

## Example Files

Check the `examples/` directory for static JSON samples:
- `state-flying.json` - Aircraft in flight
- `state-ground.json` - Aircraft on ground
- `indicators-flying.json` - Active instruments
- `indicators-ground.json` - Idle instruments

These are useful for:
- Understanding the data format
- Creating test fixtures
- Designing custom responses in Mockoon

## Performance

The mock API can easily handle:
- ✅ 60Hz `/state` polling
- ✅ 10Hz `/indicators` polling
- ✅ Both simultaneously
- ✅ Multiple concurrent sessions

Typical overhead: <1% CPU, ~10MB memory

## Next Steps

- **Customize responses** - Edit the Mockoon environment to add your own scenarios
- **Add custom fields** - Modify JSON to test edge cases
- **Test error handling** - Use Mockoon's rules to simulate HTTP errors
- **Build dashboards** - Use the realtime callbacks to create live visualizations
- **Analyze data** - Let sessions write to disk and analyze the binary files

## Troubleshooting

### "Connection refused" error
- ✅ Ensure Mockoon server is running (green "Started" indicator)
- ✅ Check it's on port 8111
- ✅ Try accessing http://localhost:8111/state in a browser

### Port already in use
If another app is using 8111:
1. In Mockoon, click the environment settings (gear icon)
2. Change "Port" to another value (e.g., 8112)
3. Update your code:
   ```csharp
   WarThunderSources.CreateStateSource(baseUrl: "http://localhost:8112")
   ```

### No random values
- ✅ Ensure "Disable templating" is **unchecked** on the response
- ✅ Try restarting the Mockoon server

### Valid field is always 0
- ✅ You're using the "Not in Match" response
- ✅ Switch to "Flying" or "On Ground" response

## Learn More

- [Full README](README.md) - Detailed documentation
- [War Thunder Integration Docs](../docs/WarThunder.md) - Integration guide
- [Mockoon Documentation](https://mockoon.com/docs/latest/about/) - Mock server features

---

**Happy Testing!** 🎮✈️

Now you can develop and test your War Thunder integration without needing the game running. The mock API provides realistic, dynamic data that matches the real War Thunder HTTP API format.
