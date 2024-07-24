using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nest;
using TvShow.Domain;
using TvShow.Domain.Models;
using TvShow.Importer.Sources.TvMaze;
using TvShow.Importer.Sources.TvMaze.Models;
using TvShow.Infrastructure.ElasticSearch.Extensions;
using IndexName = TvShow.Infrastructure.ElasticSearch.IndexName;

namespace TvShow.Importer;

public class ImportService : BackgroundService
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<ImportService> _logger;
    private readonly TvMazeClient _tvMazeClient;
    private readonly ITvShowRepository _tvShowRepository;


    public ImportService(TvMazeClient tvMazeClient, ITvShowRepository tvShowRepository, ILogger<ImportService> logger, IElasticClient elasticClient)
    {
        _tvMazeClient = tvMazeClient;
        _tvShowRepository = tvShowRepository;
        _logger = logger;
        _elasticClient = elasticClient;
    }

    public async Task ImportAllData(CancellationToken cancellationToken)
    {
        try
        {
            await _elasticClient.CreateIndexIfNotExists<Domain.Models.TvShow>(IndexName.TvShow, cancellationToken);

            await ImportShows(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected import error: {Message}", e.Message);
        }
    }

    private async Task ImportShows(CancellationToken cancellationToken)
    {
        var pageNumber = 0;
        while (true)
        {
            _logger.LogInformation("Loading page {PageNumber} from tv maze shows", pageNumber);
            var shows = await LoadShowsPageWithCast(pageNumber, cancellationToken);
            if (!shows.Any())
            {
                break;
            }

            await _tvShowRepository.SaveShows(shows, cancellationToken);

            _logger.LogInformation("Page {PageNumber} of {Count} tv maze shows was imported successfully", pageNumber, shows.Count());
            pageNumber++;
        }

        _logger.LogInformation("Loading tv maze data is completed");
    }

    private async Task<IEnumerable<Domain.Models.TvShow>> LoadShowsPageWithCast(int pageNumber, CancellationToken cancellationToken)
    {
        var shows = await _tvMazeClient.GetShowsByPage(pageNumber, cancellationToken);

        var tasks = shows.Select(async show =>
        {
            var cast = await LoadCast(show, cancellationToken);
            return ConvertToTvShowModel(show, cast);
        });
        return await Task.WhenAll(tasks);
    }

    private async Task<IEnumerable<Cast>> LoadCast(Show show, CancellationToken cancellationToken)
    {
        var policyResult = await TvMazeApiPolicy.HttpError()
            .ExecuteAndCaptureAsync(() => _tvMazeClient.GetCastForShow(show.Id, cancellationToken));
        return policyResult.Result;
    }

    private static Domain.Models.TvShow ConvertToTvShowModel(Show show, IEnumerable<Cast> cast)
    {
        return new Domain.Models.TvShow
        {
            Id = show.Id,
            Name = show.Name,
            Cast = cast.Select(x => x.Person).OrderByDescending(x => x.BirthDate).Select(x => new TvShowCast
            {
                Id = x.Id,
                Name = x.Name,
                BirthDate = x.BirthDate
            }).ToList()
        };
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return ImportAllData(stoppingToken);
    }
}
