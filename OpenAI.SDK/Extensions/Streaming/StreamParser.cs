using System.Runtime.CompilerServices;
using System.Text.Json;
using Betalgo.Ranul.OpenAI.Contracts.Interfaces;

namespace Betalgo.Ranul.OpenAI.Extensions.Streaming;

/// <summary>
///     Core SSE stream parser. Works with any IStreamEvent implementation.
///     <see href="https://platform.openai.com/docs/api-reference/streaming">OpenAI Streaming documentation</see>.
/// </summary>
public static class StreamParser
{
    /// <summary>
    ///     Parses Server-Sent Events from an HTTP response into strongly-typed events.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to deserialize to.</typeparam>
    /// <param name="response">The HTTP response containing the SSE stream.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of parsed events.</returns>
    public static async IAsyncEnumerable<TEvent> ParseSseStreamAsync<TEvent>(
        this HttpResponseMessage response,
        JsonSerializerOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TEvent : IStreamEvent
    {
#if NET6_0_OR_GREATER
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
#else
        using var stream = await response.Content.ReadAsStreamAsync();
#endif
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

#if NET7_0_OR_GREATER
            var line = await reader.ReadLineAsync(cancellationToken);
#else
            var line = await reader.ReadLineAsync();
#endif

            // Skip empty lines
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            // Skip event type lines (we use the type property in the JSON)
            if (line.StartsWith("event:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Process data lines
            if (line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var data = line.Substring(5).TrimStart();

                // Check for stream termination
                if (data == "[DONE]")
                {
                    yield break;
                }

                TEvent? evt;
                try
                {
                    evt = JsonSerializer.Deserialize<TEvent>(data, options);
                }
                catch (JsonException)
                {
                    // If the data is incomplete, try reading more
#if NET7_0_OR_GREATER
                    data += await reader.ReadToEndAsync(cancellationToken);
#else
                    data += await reader.ReadToEndAsync();
#endif
                    evt = JsonSerializer.Deserialize<TEvent>(data, options);
                }

                if (evt != null)
                {
                    yield return evt;
                }
            }
        }
    }

    /// <summary>
    ///     Parses Server-Sent Events from an HTTP response, passing through HTTP metadata.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to deserialize to.</typeparam>
    /// <param name="response">The HTTP response containing the SSE stream.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of parsed events with HTTP status code.</returns>
    public static async IAsyncEnumerable<(TEvent Event, System.Net.HttpStatusCode StatusCode)> ParseSseStreamWithStatusAsync<TEvent>(
        this HttpResponseMessage response,
        JsonSerializerOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TEvent : IStreamEvent
    {
        var statusCode = response.StatusCode;

        await foreach (var evt in response.ParseSseStreamAsync<TEvent>(options, cancellationToken))
        {
            yield return (evt, statusCode);
        }
    }
}
