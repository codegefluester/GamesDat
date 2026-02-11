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
/// <typeparam name="T">The telemetry data type (must be unmanaged struct).</typeparam>
public abstract class HttpPollingSourceBase<T> : TelemetrySourceBase<T> where T : unmanaged
{
    private readonly HttpClient _httpClient;
    private readonly HttpPollingSourceOptions _options;
    private readonly bool _ownsClient;
    private int _consecutiveErrors;
    private TimeSpan _currentRetryDelay;

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

        while (!cancellationToken.IsCancellationRequested)
        {
            T? data = default;
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

                // Success - reset error tracking
                _consecutiveErrors = 0;
                _currentRetryDelay = _options.InitialRetryDelay;
                firstError = true;
            }
            catch (JsonException ex)
            {
                // JSON parse errors are logged but don't trigger aggressive retry
                Console.WriteLine($"[{GetType().Name}] JSON parse error (skipping frame): {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                // Clean cancellation
                yield break;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Connection/timeout errors - use exponential backoff
                _consecutiveErrors++;

                if (firstError)
                {
                    Console.WriteLine($"[{GetType().Name}] Connection error: {ex.Message}");
                    Console.WriteLine($"[{GetType().Name}] Retrying with exponential backoff...");
                    firstError = false;
                }

                if (_consecutiveErrors >= _options.MaxConsecutiveErrors)
                {
                    errorToThrow = new InvalidOperationException(
                        $"Failed to connect after {_consecutiveErrors} consecutive attempts. " +
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
            if (hasData && data.HasValue)
            {
                yield return data.Value;
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
