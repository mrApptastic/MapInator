using System.Text.Json;
using MapInator.Mapper.Services;
using MapInator.Shared.Models;
using MapInator.Shared.Helpers;
using MapInator.Shared.Translations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHttpClient<DawaApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.dataforsyningen.dk/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("MapInator/1.0 (+https://github.com/mrApptastic/MapInator)");
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddSingleton<GeoJsonExporter>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var dawaClient = app.Services.GetRequiredService<DawaApiClient>();
var exporter = app.Services.GetRequiredService<GeoJsonExporter>();

// Resolve output directory: default to ../MapInator.App/wwwroot relative to repo root
string outputRoot = args.Length > 0 && args[0].StartsWith("--output=")
    ? args[0]["--output=".Length..]
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "MapInator.App", "wwwroot"));

if (!Directory.Exists(outputRoot))
{
    logger.LogError("Output directory not found: {Dir}", outputRoot);
    return 1;
}

Directory.CreateDirectory(Path.Combine(outputRoot, "data"));
logger.LogInformation("Output root: {Dir}", outputRoot);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

// Step 1: Fetch type hierarchy
logger.LogInformation("Fetching place name types from DAWA…");
List<StednavnHovedtype> types;
try
{
    types = await dawaClient.GetTypesAsync(cts.Token);
    logger.LogInformation("Found {Count} main types", types.Count);
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to fetch types from DAWA API");
    return 1;
}

// Load translation helpers (Danish display names, English display names)
var daTranslations = await LoadTranslationsAsync("da");
var enTranslations = await LoadTranslationsAsync("en");

// Step 2: Fetch and export all records per type/subtype
var typeIndex = new List<TypeIndexEntry>();
int totalTypes = types.Sum(t => t.Undertyper.Count);
int processed = 0;

foreach (var hovedtype in types)
{
    var undertypeEntries = new List<UndertypeIndexEntry>();

    foreach (var undertype in hovedtype.Undertyper)
    {
        if (cts.IsCancellationRequested) break;
        processed++;
        logger.LogInformation("[{Done}/{Total}] Fetching {HT}/{UT}…",
            processed, totalTypes, hovedtype.Hovedtype, undertype);

        var stream = dawaClient.GetAllForTypeAsync(hovedtype.Hovedtype, undertype, cts.Token);
        int count = await exporter.ExportAsync(stream, outputRoot, hovedtype.Hovedtype, undertype, cts.Token);

        var slug = SlugHelper.Slugify(hovedtype.Hovedtype);
        var subSlug = SlugHelper.Slugify(undertype);

        undertypeEntries.Add(new UndertypeIndexEntry(
            undertype,
            subSlug,
            daTranslations.GetValueOrDefault($"subtype.{undertype}", undertype),
            enTranslations.GetValueOrDefault($"subtype.{undertype}", undertype),
            $"data/{slug}/{subSlug}.geojson",
            count
        ));
    }

    if (undertypeEntries.Count > 0)
    {
        var slug = SlugHelper.Slugify(hovedtype.Hovedtype);
        typeIndex.Add(new TypeIndexEntry(
            hovedtype.Hovedtype,
            slug,
            daTranslations.GetValueOrDefault($"type.{hovedtype.Hovedtype}", hovedtype.Hovedtype),
            enTranslations.GetValueOrDefault($"type.{hovedtype.Hovedtype}", hovedtype.Hovedtype),
            undertypeEntries
        ));
    }
}

// Step 3: Write types.json index
var typesFile = Path.Combine(outputRoot, "data", "types.json");
await using var fs = File.Create(typesFile);
await JsonSerializer.SerializeAsync(fs, typeIndex, new JsonSerializerOptions { WriteIndented = true });
logger.LogInformation("Wrote types index → {File}", typesFile);
logger.LogInformation("Done. {Count} main types exported.", typeIndex.Count);
return 0;

async Task<Dictionary<string, string>> LoadTranslationsAsync(string lang)
{
    var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "MapInator.Shared", "Translations", $"{lang}.json");
    if (!File.Exists(path))
    {
        // Fallback: look next to the executable
        path = Path.Combine(AppContext.BaseDirectory, "Translations", $"{lang}.json");
    }
    if (!File.Exists(path)) return new Dictionary<string, string>();

    await using var stream = File.OpenRead(path);
    return await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream) ?? [];
}
