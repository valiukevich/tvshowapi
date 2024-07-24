using Microsoft.Extensions.Logging;
using TvShow.Domain;
using TvShow.Domain.Models;
using TvShow.Importer.Sources.TvMaze;
using TvShow.Importer.Sources.TvMaze.Models;

namespace TvShow.Importer;

public class ImportService
{
    private readonly TvMazeClient _tvMazeClient;
    private readonly ITvShowRepository _tvShowRepository;
    private readonly ILogger<ImportService> _logger;


    public ImportService(TvMazeClient tvMazeClient, ITvShowRepository tvShowRepository, ILogger<ImportService> logger)
    {
        _tvMazeClient = tvMazeClient;
        _tvShowRepository = tvShowRepository;
        _logger = logger;
    }

    public async Task ImportAllData(CancellationToken cancellationToken)
    {
        try
        {
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

            _logger.LogInformation("Page {PageNumber} of tv maze shows was imported successfully", pageNumber);
            pageNumber++;
        }

        _logger.LogInformation("Loading tv maze data is completed");
    }

    private async Task<IEnumerable<Domain.Models.TvShow>> LoadShowsPageWithCast(int pageNumber, CancellationToken cancellationToken)
    {
        var shows = await _tvMazeClient.GetShowsByPage(pageNumber, cancellationToken);

        var tasks = shows.Select(async show =>
        {
            var cast = await _tvMazeClient.GetCastForShow(show.Id, cancellationToken);
            return ConvertToTvShowModel(show, cast);
        });
        return await Task.WhenAll(tasks);
    }

    private static Domain.Models.TvShow ConvertToTvShowModel(Show show, IEnumerable<Cast> cast)
    {
        return new Domain.Models.TvShow
        {
            Id = show.Id,
            Name = show.Name,
            Cast = cast.OrderByDescending(x => x.BirthDate).Select(x => new TvShowCast
            {
                Id = x.Id,
                Name = x.Name,
                BirthDate = x.BirthDate
            }).ToList()
        };
    }
}
