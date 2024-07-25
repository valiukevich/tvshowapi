using System.Net;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace TvShow.Importer.Sources.TvMaze;

internal class RetryPolicy
{
    public static AsyncRetryPolicy<HttpResponseMessage> TooManyRequests()
    {
        return Policy<HttpResponseMessage>
            .HandleResult(httpResponseMessage => httpResponseMessage.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                10,
                sleepDurationProvider: (retryCount, response, context) => SleepDurationProvider(retryCount, response),
                onRetryAsync: (response, timespan, retryCount, context) =>
                {
                    //Console.WriteLine($"Retry {retryCount} on {response.Result.StatusCode} status");
                    return Task.CompletedTask;
                }
            );
    }

    public static AsyncRetryPolicy HttpError()
    {
        return Policy.Handle<HttpRequestException>()
            .WaitAndRetryAsync(
                Backoff.ExponentialBackoff(TimeSpan.FromSeconds(1), 10),
                (exception, timespan, retryCount, context) =>
                {
                    //Console.WriteLine($"Retry {retryCount} on {exception.GetType().Name}: {exception.Message}");
                }
            );
    }

    private static TimeSpan SleepDurationProvider(int retryCount, DelegateResult<HttpResponseMessage> result)
    {
        return result.Result.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Pow(2, retryCount));
    }
}
