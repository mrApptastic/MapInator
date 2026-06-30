using System.Text.Json.Serialization;

namespace MapInator.Shared.Models;

public record StednavnHovedtype(
    [property: JsonPropertyName("href")] string? Href,
    [property: JsonPropertyName("hovedtype")] string Hovedtype,
    [property: JsonPropertyName("undertyper")] List<string> Undertyper
);
