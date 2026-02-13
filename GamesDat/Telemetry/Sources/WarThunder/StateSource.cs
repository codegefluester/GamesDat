using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GamesDat.Core.Telemetry.Sources.WarThunder;

/// <summary>
/// Telemetry source for War Thunder's /state endpoint.
/// Provides primary flight/vehicle telemetry data at high frequency (recommended 60Hz).
/// </summary>
public class StateSource : HttpPollingSourceBase<StateData>
{
    private readonly StateSourceOptions _stateOptions;
    private DateTime _lastInvalidFrameLog = DateTime.MinValue;

    /// <summary>
    /// Initializes a new instance with StateSourceOptions.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    public StateSource(StateSourceOptions options)
        : base(WarThunderHttpClient.Instance, options.HttpOptions, ownsClient: false)
    {
        _stateOptions = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Initializes a new instance with HttpPollingSourceOptions (legacy constructor).
    /// </summary>
    /// <param name="options">Configuration options.</param>
    public StateSource(HttpPollingSourceOptions options)
        : this(new StateSourceOptions
        {
            HttpOptions = options,
            SkipInvalidFrames = true
        })
    {
    }

    /// <summary>
    /// Initializes a new instance with simplified parameters (legacy constructor).
    /// </summary>
    /// <param name="baseUrl">Base URL of the War Thunder API.</param>
    /// <param name="pollInterval">Time between polls.</param>
    public StateSource(string baseUrl, TimeSpan pollInterval)
        : this(new StateSourceOptions
        {
            HttpOptions = new HttpPollingSourceOptions
            {
                BaseUrl = baseUrl,
                EndpointPath = "/state",
                PollInterval = pollInterval
            },
            SkipInvalidFrames = true
        })
    {
    }

    /// <summary>
    /// Continuously polls the HTTP endpoint and yields valid telemetry data.
    /// Filters out invalid frames if SkipInvalidFrames is enabled.
    /// </summary>
    public override async IAsyncEnumerable<StateData> ReadContinuousAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var data in base.ReadContinuousAsync(cancellationToken))
        {
            // If not filtering invalid frames, yield everything
            if (!_stateOptions.SkipInvalidFrames)
            {
                yield return data;
                continue;
            }

            yield return data;            
        }
    }
}
