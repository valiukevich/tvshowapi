using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace TvShow.Infrastructure.ElasticSearch.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddElasticSearch(this IServiceCollection services, ElasticSearchSettings settings)
    {
        var connectionSettings = new ConnectionSettings(settings.Url)
            .ServerCertificateValidationCallback((sender, cert, chain, errors) => true)
            .BasicAuthentication(settings.Username, settings.Password)
            .DefaultMappingFor<Domain.Models.TvShow>(x => x.IndexName("shows"));

        var elasticClient = new ElasticClient(connectionSettings);
        services.AddSingleton<IElasticClient>(elasticClient);

        return services;
    }
}
