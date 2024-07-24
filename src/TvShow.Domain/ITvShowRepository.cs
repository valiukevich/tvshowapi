using TvShow.Domain.Models;

namespace TvShow.Domain;

public interface ITvShowRepository
{
    Task<IReadOnlyList<Models.TvShow>> GetShowsPaged(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<Models.TvShow> SaveShows(IEnumerable<Models.TvShow> shows, CancellationToken cancellationToken);
}
