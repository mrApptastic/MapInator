using System.Text.Json;
using System.Text.Json.Serialization;
using MapInator.Shared.Models;
using MapInator.Shared.Helpers;
using MapInator.Shared.Validation;
using Microsoft.Extensions.Logging;

namespace MapInator.Mapper.Services;

public class GeoJsonExporter(ILogger<GeoJsonExporter> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<int> ExportAsync(
        IAsyncEnumerable<Stednavn> places,
        string outputRoot,
        string hovedtype,
        string undertype,
        CancellationToken ct = default)
    {
        var features = new List<GeoJsonFeature>();

        await foreach (var place in places.WithCancellation(ct))
        {
            if (!StednavnValidator.IsValid(place))
                continue;

            features.Add(new GeoJsonFeature(
                "Feature",
                new GeoJsonGeometry("Point", place.Visueltcenter),
                new GeoJsonProperties(
                    place.Id,
                    place.Navn,
                    place.Hovedtype,
                    place.Undertype,
                    place.Navnestatus,
                    place.Kommuner.Select(k => k.Navn).ToList()
                )
            ));
        }

        if (features.Count == 0)
        {
            logger.LogInformation("No valid records for {Type}/{Sub} — skipping", hovedtype, undertype);
            return 0;
        }

        var collection = new GeoJsonFeatureCollection("FeatureCollection", features);
        var slug = SlugHelper.Slugify(hovedtype);
        var subSlug = SlugHelper.Slugify(undertype);
        var dir = Path.Combine(outputRoot, "data", slug);
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, $"{subSlug}.geojson");
        await using var fs = File.Create(filePath);
        await JsonSerializer.SerializeAsync(fs, collection, JsonOptions, ct);

        logger.LogInformation("Wrote {Count} features → {File}", features.Count, filePath);
        return features.Count;
    }
}
