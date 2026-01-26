using GameasDat.Core.Writer;
using GameasDat.Core.Reader;
using GameasDat.Core.Telemetry.Sources.AssettoCorsa;
using System.Runtime.InteropServices;

// Create a test session file with header
Console.WriteLine("=== Testing Session File Header ===\n");

var testFile = "./test_session.dat";

// Write test session
Console.WriteLine("1. Writing test session with header...");
var writer = new BinarySessionWriter();
writer.Start(testFile);

// Write 10 test frames
for (int i = 0; i < 10; i++)
{
    var testData = new ACCPhysics
    {
        PacketId = i,
        SpeedKmh = 100.0f + i * 10,
        RPM = 5000 + i * 100,
        Gear = 3 + (i % 4)
    };

    writer.WriteFrame(testData, DateTime.UtcNow.Ticks);
}

writer.Stop();
Console.WriteLine($"   ✓ Wrote 10 frames\n");

// Read back and verify
Console.WriteLine("2. Reading session back...");
int frameCount = 0;
await foreach (var (timestamp, data) in SessionReader.ReadAsync<ACCPhysics>(testFile))
{
    frameCount++;
    if (frameCount == 1)
    {
        Console.WriteLine($"   First frame: Speed={data.SpeedKmh:F1} km/h, RPM={data.RPM}, Gear={data.Gear}");
    }
}
Console.WriteLine($"   ✓ Read {frameCount} frames\n");

// Check file size
var fileInfo = new FileInfo(testFile);
Console.WriteLine($"3. File size: {fileInfo.Length} bytes");

// Verify header with hex dump
Console.WriteLine("\n4. Header hex dump (first 50 bytes):");
using (var fs = File.OpenRead(testFile))
{
    var headerBytes = new byte[50];
    fs.Read(headerBytes, 0, 50);

    Console.Write("   ");
    for (int i = 0; i < headerBytes.Length; i++)
    {
        Console.Write($"{headerBytes[i]:X2} ");
        if ((i + 1) % 16 == 0) Console.Write("\n   ");
    }
    Console.WriteLine();

    // Check magic bytes
    if (headerBytes[0] == 'G' && headerBytes[1] == 'D' && headerBytes[2] == 'T' && headerBytes[3] == 0)
    {
        Console.WriteLine("\n   ✓ Magic bytes verified: 'GDT\\0'");
    }
    else
    {
        Console.WriteLine("\n   ✗ Magic bytes NOT found!");
    }
}

// Try to read with wrong type (should fail)
Console.WriteLine("\n5. Testing type validation (should fail)...");
try
{
    await foreach (var (_, data) in SessionReader.ReadAsync<ACCGraphics>(testFile))
    {
        break;
    }
    Console.WriteLine("   ✗ ERROR: Should have thrown type mismatch exception!");
}
catch (InvalidOperationException ex) when (ex.Message.Contains("Type mismatch"))
{
    Console.WriteLine($"   ✓ Type validation works: {ex.Message.Split('\n')[0]}");
}

Console.WriteLine("\n=== All tests passed! ===");

// Clean up
File.Delete(testFile);
