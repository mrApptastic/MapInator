using System.Text.Json.Serialization;

namespace MapInator.Shared.Models;

public record TypeIndexEntry(
    [property: JsonPropertyName("hovedtype")] string Hovedtype,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("displayNameDa")] string DisplayNameDa,
    [property: JsonPropertyName("displayNameEn")] string DisplayNameEn,
    [property: JsonPropertyName("undertyper")] List<UndertypeIndexEntry> Undertyper
);

public record UndertypeIndexEntry(
    [property: JsonPropertyName("undertype")] string Undertype,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("displayNameDa")] string DisplayNameDa,
    [property: JsonPropertyName("displayNameEn")] string DisplayNameEn,
    [property: JsonPropertyName("filePath")] string FilePath,
    [property: JsonPropertyName("count")] int Count
);
