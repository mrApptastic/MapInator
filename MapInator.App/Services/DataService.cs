using System.Net.Http.Json;
using MapInator.Shared.Models;

namespace MapInator.App.Services;

public class DataService(HttpClient http)
{
    private List<TypeIndexEntry>? _types;

    public async Task<List<TypeIndexEntry>> GetTypesAsync()
    {
        _types ??= await http.GetFromJsonAsync<List<TypeIndexEntry>>("data/types.json") ?? [];
        return _types;
    }
}
