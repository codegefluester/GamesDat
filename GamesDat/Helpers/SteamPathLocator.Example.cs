// ReSharper disable All
#pragma warning disable

/*
 * USAGE EXAMPLES FOR SteamPathLocator
 * This file demonstrates how to use the SteamPathLocator API.
 * This file is not compiled - it's for documentation purposes only.
 */

using GamesDat.Core.Helpers;

namespace Examples;

public static class SteamPathLocatorExamples
{
    // Example 1: Simple synchronous usage with Switch (for side effects)
    public static void Example1_SyncWithSwitch()
    {
        var result = SteamPathLocator.GetSteamVDFPath();

        result.Switch(
            onSuccess: path => Console.WriteLine($"Found VDF at: {path}"),
            onError: error => Console.WriteLine($"Error: {error.Message}")
        );
    }

    // Example 2: Synchronous usage with IsSuccess check
    public static void Example2_SyncWithIsSuccess()
    {
        var result = SteamPathLocator.GetSteamVDFPath();

        if (result.IsSuccess)
        {
            Console.WriteLine($"VDF file located: {result.Path}");
            // Do something with the path...
        }
        else
        {
            Console.WriteLine($"Failed to locate VDF: {result.Error.Message}");
        }
    }

    // Example 3: Asynchronous usage
    public static async Task Example3_Async()
    {
        var result = await SteamPathLocator.GetSteamVDFPathAsync();

        if (result.TryGetPath(out var path))
        {
            Console.WriteLine($"Found: {path}");
            // Process the VDF file...
        }
        else if (result.TryGetError(out var error))
        {
            Console.WriteLine($"Error: {error.Message}");
        }
    }

    // Example 4: Using Switch for side effects
    public static void Example4_Switch()
    {
        var result = SteamPathLocator.GetSteamVDFPath();

        result.Switch(
            onSuccess: path =>
            {
                // Perform operations with the path
                var content = File.ReadAllText(path);
                Console.WriteLine("VDF content loaded successfully");
            },
            onError: error =>
            {
                // Handle error
                Console.Error.WriteLine($"Cannot proceed: {error.Message}");
            }
        );
    }

    // Example 5: Integration with existing code
    public static string? GetSteamLibraryFoldersContent()
    {
        var result = SteamPathLocator.GetSteamVDFPath();

        return result.Match(
            onSuccess: File.ReadAllText,
            onError: _ => null
        );
    }

    // Example 6: Caching demonstration
    public static void Example6_Caching()
    {
        // First call - performs actual search
        var result1 = SteamPathLocator.GetSteamVDFPath();

        // Subsequent calls - returns cached result instantly
        var result2 = SteamPathLocator.GetSteamVDFPath();
        var result3 = SteamPathLocator.GetSteamVDFPath();

        // All three results are identical
        Console.WriteLine($"All cached: {result1 == result2 && result2 == result3}");
    }
}
