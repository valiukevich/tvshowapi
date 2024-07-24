
using TvShow.Domain;
using TvShow.Infrastructure.ElasticSearch;
using TvShow.Infrastructure.ElasticSearch.Extensions;

namespace TvShow.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddElasticSearch(new ElasticSearchSettings());
            builder.Services.AddScoped<ITvShowRepository, TvShowRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
