using GamesDat.Core;
using GamesDat.Core.Telemetry.Sources.WarThunder;

namespace GamesDat.Examples;

/// <summary>
/// Example demonstrating War Thunder HTTP telemetry integration.
/// </summary>
public static class WarThunderExample
{
    /// <summary>
    /// Basic example: record /state endpoint at 60Hz.
    /// </summary>
    public static async Task BasicStateRecording()
    {
        await using var session = new GameSession()
            .AddSource(WarThunderSources.CreateStateSource())
            .OnData<StateData>(data =>
                Console.WriteLine($"Speed: {data.IndicatedAirspeed:F1} km/h, " +
                                $"Alt: {data.Altitude:F0}m, " +
                                $"Throttle: {data.Throttle * 100:F0}%"))
            .AutoOutput();

        Console.WriteLine("Recording War Thunder /state endpoint at 60Hz...");
        Console.WriteLine("Press Ctrl+C to stop.");
        await session.StartAsync();
    }

    /// <summary>
    /// Multi-endpoint example: record both /state and /indicators with different polling rates.
    /// </summary>
    public static async Task MultiEndpointRecording()
    {
        await using var session = new GameSession()
            .AddSource(WarThunderSources.CreateStateSource(hz: 60))      // 60Hz
            .AddSource(WarThunderSources.CreateIndicatorsSource(hz: 10)) // 10Hz
            .OnData<StateData>(data =>
                Console.WriteLine($"[State] Speed: {data.IndicatedAirspeed:F1} km/h, Alt: {data.Altitude:F0}m"))
            .OnData<IndicatorsData>(data =>
                Console.WriteLine($"[Indicators] Oil: {data.OilTemp:F1}°C, Water: {data.WaterTemp:F1}°C"));

        Console.WriteLine("Recording War Thunder (multiple endpoints)...");
        Console.WriteLine("Press Ctrl+C to stop.");
        await session.StartAsync();
    }

    /// <summary>
    /// Realtime-only example: no file output, only callbacks.
    /// </summary>
    public static async Task RealtimeOnlyMonitoring()
    {
        await using var session = new GameSession()
            .AddSource(WarThunderSources.CreateStateSource().RealtimeOnly())
            .OnData<StateData>(data =>
            {
                Console.Clear();
                Console.WriteLine("=== War Thunder Telemetry ===");
                Console.WriteLine($"Speed (IAS): {data.IndicatedAirspeed:F1} km/h");
                Console.WriteLine($"Speed (TAS): {data.TrueAirspeed:F1} km/h");
                Console.WriteLine($"Altitude:    {data.Altitude:F0} m");
                Console.WriteLine($"Mach:        {data.Mach:F2}");
                Console.WriteLine($"AoA:         {data.AngleOfAttack:F1}°");
                Console.WriteLine($"Throttle:    {data.Throttle * 100:F0}%");
                Console.WriteLine($"RPM:         {data.RPM:F0}");
                Console.WriteLine($"G-Force:     {data.Ny:F2}g");
                Console.WriteLine($"Fuel:        {data.Fuel:F1} kg");
            });

        Console.WriteLine("Starting realtime War Thunder monitoring...");
        await session.StartAsync();
    }

    /// <summary>
    /// Custom configuration example with error handling tuning.
    /// </summary>
    public static async Task CustomConfiguration()
    {
        var options = new HttpPollingSourceOptions
        {
            BaseUrl = "http://localhost:8111",
            EndpointPath = "/state",
            PollInterval = TimeSpan.FromMilliseconds(16.67), // ~60Hz
            MaxConsecutiveErrors = 20,  // More tolerant
            InitialRetryDelay = TimeSpan.FromSeconds(2),
            MaxRetryDelay = TimeSpan.FromSeconds(60)
        };

        await using var session = new GameSession()
            .AddSource(new StateSource(options))
            .OnData<StateData>(data =>
                Console.WriteLine($"Custom config: Speed={data.IndicatedAirspeed:F1} km/h"));

        Console.WriteLine("Recording with custom configuration...");
        await session.StartAsync();
    }

    /// <summary>
    /// Run the specified example.
    /// </summary>
    public static async Task Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] == "basic")
            {
                await BasicStateRecording();
            }
            else if (args[0] == "multi")
            {
                await MultiEndpointRecording();
            }
            else if (args[0] == "realtime")
            {
                await RealtimeOnlyMonitoring();
            }
            else if (args[0] == "custom")
            {
                await CustomConfiguration();
            }
            else
            {
                Console.WriteLine("Usage: WarThunderExample [basic|multi|realtime|custom]");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
            }
        }
    }
}
