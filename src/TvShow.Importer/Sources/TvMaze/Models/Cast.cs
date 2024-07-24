using System.Globalization;
using System.Text.Json.Serialization;

namespace TvShow.Importer.Sources.TvMaze.Models;

public class Cast
{
    public Person Person { get; set; }
}

public class Person
{
    public long Id { get; set; }

    public string Name { get; set; }

    [JsonPropertyName("birthday")]
    public string BirthDayString { get; set; }

    public DateTime? BirthDate =>
        DateTime.TryParseExact(BirthDayString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;
}
