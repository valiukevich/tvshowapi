using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nest;
using TvShow.Domain;
using TvShow.Infrastructure.ElasticSearch.Extensions;
using IndexName = TvShow.Infrastructure.ElasticSearch.IndexName;

namespace TvShow.Importer;

public class ImportService : BackgroundService
{
    private readonly IElasticClient _elasticClient;
    private readonly IEnumerable<ITvShowSource> _sources;
    private readonly ITvShowRepository _tvShowRepository;
    private readonly ILogger<ImportService> _logger;

    public ImportService(
        IElasticClient elasticClient,
        IEnumerable<ITvShowSource> sources,
        ITvShowRepository tvShowRepository,
        ILogger<ImportService> logger)
    {
        _elasticClient = elasticClient;
        _sources = sources;
        _tvShowRepository = tvShowRepository;
        _logger = logger;
    }

    public async Task ImportAllData(CancellationToken cancellationToken)
    {
        try
        {
            await _elasticClient.CreateIndexIfNotExists<Domain.Models.TvShow>(IndexName.TvShow, cancellationToken);

            await ImportShows(_sources, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected import error: {Message}", e.Message);
        }
    }

    private async Task ImportShows(IEnumerable<ITvShowSource> sources, CancellationToken cancellationToken)
    {
        foreach (var tvShowSource in sources)
        {
            _logger.LogInformation("Started fetching tv shows from {Source}", tvShowSource.Name);
            var batchNumber = 0;
            var totalCount = 0;
            await foreach (var batch in tvShowSource.FetchTvShows(cancellationToken))
            {
                _logger.LogInformation("Loaded {BatchNumber} batch of {BatchCount} items from {Source}",
                    ++batchNumber,
                    batch.Count,
                    tvShowSource.Name);

                await _tvShowRepository.SaveShows(batch, cancellationToken);

                _logger.LogInformation(
                    "Batch {BatchNumber} of {Count} items from {Source} was imported successfully. In total processed {TotalCount} documents",
                    batchNumber,
                    batch.Count,
                    tvShowSource.Name,
                    totalCount += batch.Count);
            }
            _logger.LogInformation("Fetching of tv shows from {Source} completed", tvShowSource.Name);
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return ImportAllData(stoppingToken);
    }
}
