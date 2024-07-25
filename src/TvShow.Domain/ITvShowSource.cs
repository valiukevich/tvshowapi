using System.Runtime.CompilerServices;

namespace TvShow.Domain;

public interface ITvShowSource
{
    string Name { get; }

    IAsyncEnumerable<IReadOnlyCollection<Domain.Models.TvShow>> FetchTvShows([EnumeratorCancellation] CancellationToken cancellationToken);
}
