using Nest;
using System.Linq.Expressions;
using System.Runtime.ExceptionServices;

namespace TvShow.Infrastructure.ElasticSearch.Extensions;

public static class ElasticClientExtensions
{
    public static void BulkInsert<T>(this IElasticClient elasticClient, IEnumerable<T> items, string indexName, CancellationToken cancellationToken)
        where T : class
    {
        var waitHandle = new CountdownEvent(1);

        var bulkAll = elasticClient.BulkAll(
            items,
            b => b.Index(indexName).BackOffRetries(2).BackOffTime("30s").RefreshOnCompleted(true).MaxDegreeOfParallelism(4).Size(1000),
            cancellationToken);

        ExceptionDispatchInfo captureInfo = null;

        bulkAll.Subscribe(new BulkAllObserver(
            onError: e =>
            {
                captureInfo = ExceptionDispatchInfo.Capture(e);
                waitHandle.Signal();
            },
            onCompleted: () => waitHandle.Signal()));

        waitHandle.Wait(cancellationToken);
        captureInfo?.Throw();
    }

    public static async Task DeleteByQueryAsync<T, TValue>(this IElasticClient elasticClient, string indexName, Expression<Func<T, TValue>> keySelector, IEnumerable<TValue> keyValues, CancellationToken cancellationToken)
        where T : class
    {
        var response = await elasticClient.DeleteByQueryAsync<T>(
                        q => q.Index(indexName)
                              .Query(rq => rq.Terms(c => c
                                .Field(keySelector)
                                .Terms(keyValues)))
                              .Timeout(new Time(TimeSpan.FromMinutes(10)))
                              .Refresh(true), cancellationToken);


        AssertResponse(response);
    }

    public static async Task CreateIndexIfNotExists<T>(this IElasticClient _elasticClient, string indexName, CancellationToken cancellationToken)
        where T : class
    {
        var existsResponse = await _elasticClient.Indices.ExistsAsync(indexName, null, cancellationToken);

        if (!existsResponse.Exists)
        {
            var createIndexResponse = await _elasticClient.Indices.CreateAsync(indexName,
                createIndex => createIndex
                    .Settings(s => s.NumberOfShards(1)
                        .NumberOfReplicas(0)
                        .RefreshInterval(-1))
                    .Map<T>(map => map.AutoMap()),
                cancellationToken);

            AssertResponse(createIndexResponse);
        }
    }

    private static void AssertResponse(ResponseBase response)
    {
        if (!response.IsValid)
        {
            throw new InvalidOperationException(response.DebugInformation);
        }
    }
}
