using TvShow.Domain;

namespace TvShow.Infrastructure.ElasticSearch
{
    public class TvShowRepository : ITvShowRepository
    {
        public async Task<IReadOnlyList<Domain.Models.TvShow>> GetShowsPaged(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Domain.Models.TvShow> SaveShows(IEnumerable<Domain.Models.TvShow> shows, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
