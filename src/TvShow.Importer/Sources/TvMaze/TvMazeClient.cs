using System.Text.Json;
using TvShow.Importer.Sources.TvMaze.Models;

namespace TvShow.Importer.Sources.TvMaze;

public class TvMazeClient
{
    private readonly HttpClient _httpClient;

    public TvMazeClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Show>> GetShowsByPage(int page, CancellationToken cancellationToken)
    {
        var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"shows?page={page}"), cancellationToken);
        return await ReadResponseAsJson<Show>(response, cancellationToken);
    }

    public async Task<IEnumerable<Cast>> GetCastForShow(string showId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"shows/{showId}/cast"), cancellationToken);
        return await ReadResponseAsJson<Cast>(response, cancellationToken);
    }

    private static async Task<IEnumerable<T>> ReadResponseAsJson<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                return Enumerable.Empty<T>();
            }

            return JsonSerializer.Deserialize<List<T>>(content);
        }

        throw new HttpRequestException("Unexpected http error occurred when calling TV Maze API", inner: null, statusCode: response.StatusCode);
    }
}
