using System.Net;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace TvShow.Importer.Sources.TvMaze;

internal class TvMazeApiPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> RateLimit()
    {
        return Policy<HttpResponseMessage>
            .HandleResult(res =>
            {
                if (!res.IsSuccessStatusCode)
                {
                }

                return res.StatusCode == HttpStatusCode.TooManyRequests;
            })
            .WaitAndRetryAsync(
                10,
                (retryCount, response, context) => SleepDurationProvider(retryCount, response),
                async (response, timespan, retryCount, context) => { Console.WriteLine($"Retry on status is {retryCount}"); }); //.WrapAsync(p);
    }

    public static AsyncRetryPolicy HttpError()
    {
        return Policy.Handle<Exception>()
            .WaitAndRetryAsync(
                Backoff.ExponentialBackoff(TimeSpan.FromSeconds(1), 10),
                async (exception, timespan, retryCount, context) => { Console.WriteLine($"Retry on error is {retryCount}: {exception.Message}"); }
            );
    }

    private static TimeSpan SleepDurationProvider(int retryCount, DelegateResult<HttpResponseMessage> result)
    {
        return result.Result.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Pow(2, retryCount));
    }
}
