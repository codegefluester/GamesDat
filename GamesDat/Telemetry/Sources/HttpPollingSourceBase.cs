using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GamesDat.Core.Telemetry.Sources;

/// <summary>
/// Abstract base class for HTTP polling-based telemetry sources.
/// Handles HTTP request lifecycle, polling loop, retry logic, and JSON deserialization.
/// </summary>
/// <typeparam name="T">The telemetry data type.</typeparam>
public abstract class HttpPollingSourceBase<T> : TelemetrySourceBase<T>
{
    private readonly HttpClient _httpClient;
    private readonly HttpPollingSourceOptions _options;
    private readonly bool _ownsClient;
    private int _consecutiveErrors;
    private TimeSpan _currentRetryDelay;
    private DateTime _retryStartTime;

    /// <summary>
    /// Initializes a new instance with a provided HttpClient.
    /// </summary>
    /// <param name="httpClient">The HttpClient to use for requests.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="ownsClient">Whether this instance owns the HttpClient and should dispose it.</param>
    protected HttpPollingSourceBase(HttpClient httpClient, HttpPollingSourceOptions options, bool ownsClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _ownsClient = ownsClient;
        _currentRetryDelay = options.InitialRetryDelay;
    }

    /// <summary>
    /// Initializes a new instance with a new HttpClient.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    protected HttpPollingSourceBase(HttpPollingSourceOptions options)
        : this(new HttpClient { Timeout = options.RequestTimeout }, options, ownsClient: true)
    {
    }

    /// <summary>
    /// Continuously polls the HTTP endpoint and yields telemetry data.
    /// </summary>
    public override async IAsyncEnumerable<T> ReadContinuousAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var fullUrl = _options.GetFullUrl();
        var firstError = true;
        var loopStartTime = DateTime.UtcNow;

        if (_options.EnableDebugLogging)
        {
            Console.WriteLine($"[{GetType().Name}] [DEBUG] Polling loop started at {loopStartTime:HH:mm:ss.fff}");
            Console.WriteLine($"[{GetType().Name}] [DEBUG] Target URL: {fullUrl}");
            Console.WriteLine($"[{GetType().Name}] [DEBUG] Poll interval: {_options.PollInterval.TotalMilliseconds}ms");
            Console.WriteLine($"[{GetType().Name}] [DEBUG] Cancellation requested: {cancellationToken.IsCancellationRequested}");
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            T data = default!;
            bool hasData = false;
            Exception? errorToThrow = null;

            // Request and error handling (no yield in try-catch)
            HttpResponseMessage? response = null;
            try
            {
                // Create request with custom headers
                using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                if (_options.Headers != null)
                {
                    foreach (var header in _options.Headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Send request with per-request timeout via linked cancellation token
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                if (_options.RequestTimeout != Timeout.InfiniteTimeSpan)
                {
                    linkedCts.CancelAfter(_options.RequestTimeout);
                }

                response = await _httpClient.SendAsync(request, linkedCts.Token);
                response.EnsureSuccessStatusCode();

                // Read and parse response
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                data = ParseJson(json);
                hasData = true;

                if (_options.EnableDebugLogging)
                {
                    Console.WriteLine($"[{GetType().Name}] [DEBUG] Poll successful at {DateTime.UtcNow:HH:mm:ss.fff}");
                }

                // Success - reset error tracking
                if (_consecutiveErrors > 0 && _options.EnableDebugLogging)
                {
                    var recoveryTime = DateTime.UtcNow - _retryStartTime;
                    Console.WriteLine($"[{GetType().Name}] Connection recovered after {_consecutiveErrors} attempts ({recoveryTime.TotalSeconds:F1}s)");
                }
                _consecutiveErrors = 0;
                _currentRetryDelay = _options.InitialRetryDelay;
                firstError = true;
            }
            catch (JsonException ex)
            {
                // JSON parse errors are logged but don't trigger aggressive retry
                Console.WriteLine($"[{GetType().Name}] JSON parse error (skipping frame): {ex.Message}");
            }
            catch (OperationCanceledException ex)
            {
                // Check if this was expected cancellation
                if (cancellationToken.IsCancellationRequested)
                {
                    if (_options.EnableDebugLogging)
                    {
                        Console.WriteLine($"[{GetType().Name}] [DEBUG] Polling loop cancelled (expected) at {DateTime.UtcNow:HH:mm:ss.fff}");
                    }
                }
                else
                {
                    // Unexpected cancellation - log details
                    Console.WriteLine($"[{GetType().Name}] WARNING: Unexpected OperationCanceledException caught");
                    Console.WriteLine($"[{GetType().Name}]   Exception source: {ex.Source}");
                    Console.WriteLine($"[{GetType().Name}]   CancellationToken.IsCancellationRequested: {cancellationToken.IsCancellationRequested}");
                    if (_options.EnableDebugLogging)
                    {
                        Console.WriteLine($"[{GetType().Name}] [DEBUG] Stack trace: {ex.StackTrace}");
                    }
                }
                yield break;
            }
            catch (HttpRequestException ex)
            {
                // Connection/timeout errors - use exponential backoff
                _consecutiveErrors++;

                if (firstError)
                {
                    _retryStartTime = DateTime.UtcNow;
                    Console.WriteLine($"[{GetType().Name}] Connection error: {ex.GetType().Name}: {ex.Message}");
                    Console.WriteLine($"[{GetType().Name}] Retrying with exponential backoff (max {_options.MaxConsecutiveErrors} attempts)...");
                    firstError = false;
                }

                // Show retry progress every 5 attempts or when debug logging is enabled
                if (_consecutiveErrors % 5 == 0 || _options.EnableDebugLogging)
                {
                    var elapsedTime = DateTime.UtcNow - _retryStartTime;
                    var nextDelay = _currentRetryDelay;
                    Console.WriteLine($"[{GetType().Name}] Retry {_consecutiveErrors}/{_options.MaxConsecutiveErrors} " +
                                    $"(elapsed: {elapsedTime.TotalSeconds:F0}s, next delay: {nextDelay.TotalSeconds:F0}s)");
                }

                if (_consecutiveErrors >= _options.MaxConsecutiveErrors)
                {
                    var totalRetryTime = DateTime.UtcNow - _retryStartTime;
                    errorToThrow = new InvalidOperationException(
                        $"Failed to connect after {_consecutiveErrors} consecutive attempts over {totalRetryTime.TotalSeconds:F0}s. " +
                        $"Ensure the game is running and the API is accessible at {fullUrl}",
                        ex);
                }
            }
            finally
            {
                response?.Dispose();
            }

            // Throw error outside try-catch if needed
            if (errorToThrow != null)
            {
                throw errorToThrow;
            }

            // Yield data outside try-catch
            if (hasData)
            {
                yield return data;
                await Task.Delay(_options.PollInterval, cancellationToken);
            }
            else if (!hasData)
            {
                // Wait before retry (for connection errors or JSON errors)
                var delayTime = _consecutiveErrors > 0 ? _currentRetryDelay : _options.PollInterval;
                await Task.Delay(delayTime, cancellationToken);

                // Update backoff for next retry
                if (_consecutiveErrors > 0)
                {
                    _currentRetryDelay = TimeSpan.FromMilliseconds(
                        Math.Min(_currentRetryDelay.TotalMilliseconds * 2, _options.MaxRetryDelay.TotalMilliseconds));
                }
            }
        }

        // Loop exited - log diagnostics
        if (_options.EnableDebugLogging)
        {
            var loopDuration = DateTime.UtcNow - loopStartTime;
            Console.WriteLine($"[{GetType().Name}] [DEBUG] Polling loop exited at {DateTime.UtcNow:HH:mm:ss.fff}");
            Console.WriteLine($"[{GetType().Name}] [DEBUG] Loop duration: {loopDuration.TotalSeconds:F1}s");
            Console.WriteLine($"[{GetType().Name}] [DEBUG] Cancellation requested: {cancellationToken.IsCancellationRequested}");
        }

        // Check for unexpected exit
        if (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine($"[{GetType().Name}] WARNING: Polling loop exited without cancellation request");
            Console.WriteLine($"[{GetType().Name}]   This may indicate an unexpected termination condition");
        }
    }

    /// <summary>
    /// Parses JSON string into telemetry data structure.
    /// Override this method for custom JSON parsing logic.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>The parsed telemetry data.</returns>
    protected virtual T ParseJson(string json)
    {
        // Use non-generic overload to safely detect null/incompatible results, but do not treat default(T) as an error.
        var obj = JsonSerializer.Deserialize(json, typeof(T), GetJsonSerializerOptions());
        if (obj is T value)
            return value;

        throw new JsonException("Deserialization returned null or an incompatible type.");
    }

    /// <summary>
    /// Gets the JsonSerializerOptions for parsing.
    /// Override this method to customize JSON deserialization settings.
    /// </summary>
    /// <returns>JsonSerializerOptions instance.</returns>
    protected virtual JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
    }

    /// <summary>
    /// Disposes resources used by this source.
    /// </summary>
    public override void Dispose()
    {
        if (_ownsClient)
        {
            _httpClient.Dispose();
        }

        base.Dispose();
    }
}
