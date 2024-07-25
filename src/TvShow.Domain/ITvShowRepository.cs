namespace TvShow.Domain;

public interface ITvShowRepository
{
    Task<IReadOnlyCollection<Models.TvShow>> GetShowsPaged(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task SaveShows(IEnumerable<Models.TvShow> shows, CancellationToken cancellationToken);
}
