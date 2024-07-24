using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using TvShow.Domain;
using TvShow.Importer.Sources.TvMaze;
using TvShow.Infrastructure.ElasticSearch;

namespace TvShow.Importer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ITvShowRepository, TvShowRepository>();
                services.AddHttpClient<TvMazeClient>().AddPolicyHandler(TvMazeApiRateLimitPolicy.Build);
                services.AddTransient<ImportService>();
            }).Build();
            await builder.RunAsync();
        }
    }
}
