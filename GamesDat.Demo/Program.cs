using GameasDat.Core.Reader;
using GameasDat.Core.Telemetry.Sources.AssettoCorsa;
using GameasDat.Core.Telemetry.Sources.Rocket_League;
using GameasDat.Core.Writer;

namespace GamesDat.Demo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //if (args.Length > 0 && args[0] == "read")
            //{
            //    await ReadSessionAsync(args.Length > 1 ? args[1] : null);
            //    return;
            //}

            //await CaptureSessionAsync();
        
            await DemoFileWatcherAsync();
        }

        #region Sim Racing
        static async Task CaptureSessionAsync()
        {
            Console.WriteLine("Starting ACC telemetry capture...");
            Console.WriteLine("Make sure ACC is running!");
            Console.WriteLine("Press Ctrl+C to stop\n");

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            var sessionPath = $"./sessions/acc_{DateTime.UtcNow:yyyyMMdd_HHmmss}.msgpack";

            using var source = ACCSources.CreatePhysicsSource();
            using var writer = new BinarySessionWriter();
            writer.Start(sessionPath);

            try
            {
                int frameCount = 0;
                await foreach (var data in source.ReadContinuousAsync(cts.Token))
                {
                    writer.WriteFrame(data, DateTime.UtcNow.Ticks);

                    frameCount++;
                    if (frameCount % 100 == 0)
                    {
                        Console.WriteLine($"Frames: {frameCount} | Speed: {data.SpeedKmh:F1} km/h | RPM: {data.RPM} | Gear: {data.Gear}");
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("\nError: ACC is not running or shared memory is not available.");
                Console.WriteLine("Make sure ACC is running before starting capture.");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nStopping capture...");
            }
            finally
            {
                writer.Stop();
                Console.WriteLine($"\nSession saved to: {sessionPath}");
            }
        }

        static async Task ReadSessionAsync(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                // Find most recent session
                var sessions = Directory.GetFiles("./sessions", "*.msgpack")
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .ToList();

                if (!sessions.Any())
                {
                    Console.WriteLine("No session files found in ./sessions");
                    return;
                }

                filePath = sessions.First();
            }

            Console.WriteLine($"Reading session: {filePath}");

            // Check file size first
            var fileInfo = new FileInfo(filePath);
            Console.WriteLine($"File size: {fileInfo.Length:N0} bytes");

            if (fileInfo.Length == 0)
            {
                Console.WriteLine("ERROR: Session file is empty!");
                return;
            }

            Console.WriteLine("Starting to read frames...\n");

            // Stats
            int frameCount = 0;
            float maxSpeed = 0;
            long startTime = 0;
            long endTime = 0;

            try
            {
                await foreach (var (timestamp, data) in SessionReader.ReadAsync<ACCPhysics>(filePath))
                {
                    if (frameCount == 0)
                    {
                        startTime = timestamp;
                        Console.WriteLine($"First frame read successfully!");
                        Console.WriteLine($"  Speed: {data.SpeedKmh:F1} km/h");
                        Console.WriteLine($"  RPM: {data.RPM}");
                        Console.WriteLine($"  Gear: {data.Gear}\n");
                    }

                    endTime = timestamp;
                    frameCount++;

                    maxSpeed = Math.Max(maxSpeed, data.SpeedKmh);

                    if (frameCount % 1000 == 0)
                    {
                        var elapsed = TimeSpan.FromTicks(timestamp - startTime);
                        Console.WriteLine($"Frame {frameCount}: {elapsed:mm\\:ss\\.ff} | Speed: {data.SpeedKmh:F1} km/h | Gear: {data.Gear}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERROR while reading frames: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            if (frameCount == 0)
            {
                Console.WriteLine("\nERROR: No frames were read from the session file!");
                Console.WriteLine("The file might be corrupted or empty.");
                return;
            }

            var duration = TimeSpan.FromTicks(endTime - startTime);

            Console.WriteLine("\n=== Session Summary ===");
            Console.WriteLine($"Total frames: {frameCount}");
            Console.WriteLine($"Duration: {duration:mm\\:ss}");
            Console.WriteLine($"Max speed: {maxSpeed:F1} km/h");

            // Export to CSV
            var csvPath = filePath.Replace(".msgpack", ".csv");
            Console.WriteLine($"\nExporting to CSV: {csvPath}");
            await ExportToCsvAsync(filePath, csvPath);
            Console.WriteLine("CSV export complete!");

            // Export to HTML
            var htmlPath = filePath.Replace(".msgpack", ".html");
            Console.WriteLine($"Creating chart: {htmlPath}");
            await ExportToHtmlChartAsync(filePath, htmlPath);
            Console.WriteLine($"Chart created! Open {htmlPath} in your browser.");
        }

        static async Task ExportToCsvAsync(string sessionPath, string csvPath)
        {
            using var writer = new StreamWriter(csvPath);

            // Header
            await writer.WriteLineAsync("Timestamp,TimeSeconds,SpeedKmh,RPM,Gear,Gas,Brake,SteerAngle,FuelLevel");

            long startTime = 0;
            int rowCount = 0;

            try
            {
                await foreach (var (timestamp, data) in SessionReader.ReadAsync<ACCPhysics>(sessionPath))
                {
                    if (startTime == 0)
                        startTime = timestamp;

                    var elapsed = TimeSpan.FromTicks(timestamp - startTime).TotalSeconds;

                    await writer.WriteLineAsync(
                        $"{timestamp}," +
                        $"{elapsed:F3}," +
                        $"{data.SpeedKmh:F2}," +
                        $"{data.RPM}," +
                        $"{data.Gear}," +
                        $"{data.Gas:F3}," +
                        $"{data.Brake:F3}," +
                        $"{data.SteerAngle:F2}," +
                        $"{data.Fuel:F2}"
                    );

                    rowCount++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during CSV export: {ex.Message}");
                throw;
            }

            Console.WriteLine($"  Wrote {rowCount} data rows to CSV");
        }

        static async Task ExportToHtmlChartAsync(string sessionPath, string htmlPath)
        {
            try
            {
                Console.WriteLine("  Starting HTML export...");

                var frames = new List<(double time, float speed, int rpm, int gear)>();

                long startTime = 0;
                long lastIncludedTime = 0;
                long minTimeBetweenFrames = TimeSpan.FromMilliseconds(16.67).Ticks; // ~60Hz

                Console.WriteLine("  Reading frames...");
                await foreach (var (timestamp, data) in SessionReader.ReadAsync<ACCPhysics>(sessionPath))
                {
                    if (startTime == 0)
                    {
                        startTime = timestamp;
                        lastIncludedTime = timestamp;
                    }

                    if (timestamp - lastIncludedTime >= minTimeBetweenFrames)
                    {
                        var elapsed = TimeSpan.FromTicks(timestamp - startTime).TotalSeconds;
                        frames.Add((elapsed, data.SpeedKmh, data.RPM, data.Gear));
                        lastIncludedTime = timestamp;
                    }
                }

                if (frames.Count == 0)
                {
                    Console.WriteLine("  WARNING: No frames to visualize");
                    await File.WriteAllTextAsync(htmlPath, "<html><body><h1>No data to display</h1></body></html>");
                    return;
                }

                Console.WriteLine($"  Collected {frames.Count} frames");
                Console.WriteLine("  Building data arrays...");

                // Build data arrays with invariant culture
                var timeLabels = string.Join(",", frames.Select(f => f.time.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)));
                Console.WriteLine($"  Time labels length: {timeLabels.Length}");

                var speedData = string.Join(",", frames.Select(f => f.speed.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)));
                Console.WriteLine($"  Speed data length: {speedData.Length}");

                var rpmData = string.Join(",", frames.Select(f => f.rpm.ToString()));
                Console.WriteLine($"  RPM data length: {rpmData.Length}");

                Console.WriteLine("  Computing stats...");
                var duration = TimeSpan.FromSeconds(frames.Last().time);
                var maxSpeed = frames.Max(f => f.speed);
                var maxRpm = frames.Max(f => f.rpm);

                Console.WriteLine($"  Stats: Duration={duration}, MaxSpeed={maxSpeed}, MaxRPM={maxRpm}");
                Console.WriteLine("  Building HTML...");

                var html = $@"<!DOCTYPE html>
<html>
<head>
    <title>ACC Telemetry</title>
    <script src=""https://cdn.jsdelivr.net/npm/chart.js""></script>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; background: #1e1e1e; color: #fff; }}
        h1 {{ color: #fff; }}
        .stats {{ background: #2d2d2d; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        .stats span {{ margin-right: 20px; }}
        canvas {{ background: #2d2d2d; border-radius: 5px; }}
    </style>
</head>
<body>
    <h1>ACC Session Telemetry</h1>
    <div class=""stats"">
        <span>Total Frames: {frames.Count}</span>
        <span>Duration: {duration.Minutes}m {duration.Seconds}s</span>
        <span>Max Speed: {maxSpeed.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)} km/h</span>
        <span>Max RPM: {maxRpm}</span>
    </div>
    <canvas id=""chart"" width=""1400"" height=""700""></canvas>
    <script>
    const data = {{
        labels: [{timeLabels}],
        datasets: [
            {{
                label: 'Speed (km/h)',
                data: [{speedData}],
                borderColor: 'rgb(255, 99, 132)',
                backgroundColor: 'rgba(255, 99, 132, 0.1)',
                borderWidth: 2,
                pointRadius: 0,
                yAxisID: 'y',
            }},
            {{
                label: 'RPM',
                data: [{rpmData}],
                borderColor: 'rgb(54, 162, 235)',
                backgroundColor: 'rgba(54, 162, 235, 0.1)',
                borderWidth: 2,
                pointRadius: 0,
                yAxisID: 'y1',
            }}
        ]
    }};
    
    const config = {{
        type: 'line',
        data: data,
        options: {{
            responsive: true,
            interaction: {{ mode: 'index', intersect: false }},
            scales: {{
                x: {{
                    title: {{ display: true, text: 'Time (seconds)', color: '#fff' }},
                    ticks: {{ color: '#aaa' }},
                    grid: {{ color: '#444' }}
                }},
                y: {{
                    type: 'linear',
                    position: 'left',
                    title: {{ display: true, text: 'Speed (km/h)', color: '#fff' }},
                    ticks: {{ color: '#aaa' }},
                    grid: {{ color: '#444' }}
                }},
                y1: {{
                    type: 'linear',
                    position: 'right',
                    title: {{ display: true, text: 'RPM', color: '#fff' }},
                    ticks: {{ color: '#aaa' }},
                    grid: {{ drawOnChartArea: false, color: '#444' }}
                }}
            }},
            plugins: {{
                legend: {{ labels: {{ color: '#fff' }} }},
                tooltip: {{
                    callbacks: {{
                        title: function(context) {{ return 'Time: ' + context[0].label + 's'; }}
                    }}
                }}
            }}
        }}
    }};
    
    new Chart(document.getElementById('chart'), config);
    </script>
</body>
</html>";

                Console.WriteLine("  Writing HTML file...");
                await File.WriteAllTextAsync(htmlPath, html);
                Console.WriteLine("  HTML export complete!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR during HTML export: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"  Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        #endregion

        #region Replay Files (RB6 Siege, Rocket League etc.)
        static async Task DemoFileWatcherAsync()
        {
            Console.WriteLine("File Watcher Demo - Monitoring for Rocket League replays");
            Console.WriteLine("Play a match in Rocket League to generate a replay file");
            Console.WriteLine("Press Ctrl+C to stop\n");

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                using var source = RocketLeagueReplayFileSource.CreateReplaySource();

                Console.WriteLine($"Monitoring: {RocketLeagueReplayFileSource.GetDefaultReplayPath()}");
                Console.WriteLine("Waiting for new replay files...\n");

                await foreach (var replayPath in source.ReadContinuousAsync(cts.Token))
                {
                    var fileInfo = new FileInfo(replayPath);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] New replay detected!");
                    Console.WriteLine($"  File: {fileInfo.Name}");
                    Console.WriteLine($"  Size: {fileInfo.Length:N0} bytes");
                    Console.WriteLine($"  Path: {replayPath}");
                    Console.WriteLine();

                    // Here you could:
                    // - Copy the file somewhere
                    // - Parse it
                    // - Upload it
                    // - Trigger processing
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nStopped monitoring.");
            }
        }
        #endregion
    }
}
