namespace TvShow.Infrastructure.ElasticSearch
{
    public class ElasticSearchSettings
    {
        public Uri Url { get; set; } = new Uri("http://localhost:9200");

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
