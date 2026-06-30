using System.Text.Json;
using System.Text.Json.Serialization;

namespace MapInator.Shared.Models;

public record GeoJsonFeatureCollection(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("features")] List<GeoJsonFeature> Features
);

public record GeoJsonFeature(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("geometry")] GeoJsonGeometry Geometry,
    [property: JsonPropertyName("properties")] GeoJsonProperties Properties
);

public record GeoJsonGeometry(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("coordinates")] double[] Coordinates
);

public record GeoJsonProperties(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("navn")] string Navn,
    [property: JsonPropertyName("hovedtype")] string Hovedtype,
    [property: JsonPropertyName("undertype")] string Undertype,
    [property: JsonPropertyName("navnestatus")] string Navnestatus,
    [property: JsonPropertyName("kommuner")] List<string> Kommuner
);
