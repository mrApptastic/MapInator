using System.Text.Json;
using System.Text.Json.Serialization;

namespace MapInator.Shared.Models;

public record Stednavn(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("hovedtype")] string Hovedtype,
    [property: JsonPropertyName("undertype")] string Undertype,
    [property: JsonPropertyName("navn")] string Navn,
    [property: JsonPropertyName("navnestatus")] string Navnestatus,
    [property: JsonPropertyName("ændret")] string Ændret,
    [property: JsonPropertyName("geo_ændret")] string GeoÆndret,
    [property: JsonPropertyName("geo_version")] int GeoVersion,
    [property: JsonPropertyName("href")] string Href,
    [property: JsonPropertyName("egenskaber")] JsonElement Egenskaber,
    [property: JsonPropertyName("visueltcenter")] double[] Visueltcenter,
    [property: JsonPropertyName("kommuner")] List<Kommune> Kommuner
);

public record Kommune(
    [property: JsonPropertyName("href")] string Href,
    [property: JsonPropertyName("kode")] string Kode,
    [property: JsonPropertyName("navn")] string Navn
);
