using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TvShow.Domain;
using TvShow.Importer.Sources.TvMaze;
using TvShow.Infrastructure.ElasticSearch;
using TvShow.Infrastructure.ElasticSearch.Extensions;

namespace TvShow.Importer;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ITvShowRepository, TvShowRepository>();
                services
                    .AddHttpClient<TvMazeClient>(client => client.BaseAddress = new Uri("http://api.tvmaze.com"))
                    .AddPolicyHandler(arg => TvMazeApiPolicy.RateLimit());
                services.AddHostedService<ImportService>();
                services.AddElasticSearch(new ElasticSearchSettings());
            }).Build();
        await builder.RunAsync();
    }
}
