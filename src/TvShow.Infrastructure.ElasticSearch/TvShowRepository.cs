using Nest;
using TvShow.Domain;
using TvShow.Infrastructure.ElasticSearch.Extensions;

namespace TvShow.Infrastructure.ElasticSearch;

public class TvShowRepository : ITvShowRepository
{
    private readonly IElasticClient _elasticClient;

    public TvShowRepository(IElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    public async Task<IReadOnlyCollection<Domain.Models.TvShow>> GetShowsPaged(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.SearchAsync<Domain.Models.TvShow>(x =>
            x
            .Query(rq => rq.MatchAll())
            .From(pageSize * (pageNumber))
            .Size(pageSize)
            .Sort(s => s.Field(SortSelector)),
            cancellationToken);
        return response.Documents;
    }

    private IFieldSort SortSelector(FieldSortDescriptor<Domain.Models.TvShow> arg)
    {
        return arg.Field(y => y.Name.Suffix("keyword")).Ascending();
    }

    public async Task SaveShows(IEnumerable<Domain.Models.TvShow> shows, CancellationToken cancellationToken)
    {
        await _elasticClient.DeleteByQueryAsync(IndexName.TvShow, (Domain.Models.TvShow p) => p.Id, shows.Select(i => i.Id), cancellationToken);

        _elasticClient.BulkInsert(shows, IndexName.TvShow, cancellationToken);
    }
}
