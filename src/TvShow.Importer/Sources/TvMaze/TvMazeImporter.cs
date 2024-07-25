using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using TvShow.Domain;
using TvShow.Domain.Models;
using TvShow.Importer.Sources.TvMaze.Models;

namespace TvShow.Importer.Sources.TvMaze;

public class TvMazeImporter : ITvShowSource
{
    private readonly ILogger<TvMazeImporter> _logger;
    private readonly TvMazeClient _tvMazeClient;


    public TvMazeImporter(TvMazeClient tvMazeClient, ILogger<TvMazeImporter> logger)
    {
        _tvMazeClient = tvMazeClient;
        _logger = logger;
    }

    public string Name => "TvMaze";

    public async IAsyncEnumerable<IReadOnlyCollection<Domain.Models.TvShow>> FetchTvShows(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var pageNumber = 0;
        while (true)
        {
            _logger.LogInformation("Loading page {PageNumber} from tv maze shows", pageNumber);
            var shows = await LoadShowsByPageNumberWithCast(pageNumber, cancellationToken);
            if (!shows.Any())
            {
                yield break;
            }

            yield return shows;
            pageNumber++;
        }
    }

    private async Task<IReadOnlyCollection<Domain.Models.TvShow>> LoadShowsByPageNumberWithCast(int pageNumber, CancellationToken cancellationToken)
    {
        var shows = await _tvMazeClient.GetShowsByPage(pageNumber, cancellationToken);
        var showCounter = 0;
        var taskBatches = shows
            .Select(async show =>
            {
                var cast = await LoadShowCast(show, cancellationToken);
                return ConvertToTvShowModel(show, cast);
            })
            .InBatches(50)
            .Select(Task.WhenAll);
        var results = new List<Domain.Models.TvShow>();
        try
        {
            foreach (var taskBatch in taskBatches)
            {
                var batch = await taskBatch;
                results.AddRange(batch);
                _logger.LogInformation("\tLoaded cast for {ShowCounter}/{ShowCount} shows", showCounter += batch.Length, shows.Count);
            }

            return results.AsReadOnly();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Operation cancelled");
            return new List<Domain.Models.TvShow>();
        }
    }

    private async Task<IReadOnlyCollection<Cast>> LoadShowCast(Show show, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading cast for {Show} from tv maze shows", show.Name);
        var policyResult = await RetryPolicy.HttpError()
            .ExecuteAndCaptureAsync(() => _tvMazeClient.GetCastForShow(show.Id, cancellationToken));
        if (policyResult.FinalException != null)
        {
            throw policyResult.FinalException;
        }
        var cast = policyResult.Result;
        _logger.LogDebug("Loaded {CastCount} cast for {Show} from tv maze shows", cast.Count, show.Name);
        return cast;
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
}
