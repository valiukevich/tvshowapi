namespace TvShow.Domain.Models;

public class TvShow
{
    public long Id { get; set; }

    public string Name { get; set; }

    public List<TvShowCast> Cast { get; set; }

}
