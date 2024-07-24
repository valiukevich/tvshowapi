using System.Net;
using Polly;

namespace TvShow.Importer.Sources.TvMaze;

internal class TvMazeApiRateLimitPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> Build(HttpRequestMessage arg)
    {
        return Policy<HttpResponseMessage>
            .HandleResult(res => res.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 0,
                sleepDurationProvider: (retryCount, response, context) => SleepDurationProvider(response),
                onRetryAsync: (response, timespan, retryCount, context) => Task.CompletedTask);
    }

    private static TimeSpan SleepDurationProvider(DelegateResult<HttpResponseMessage> result)
    {
        return result.Result.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
    }
}