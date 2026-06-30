using System.Net.Http.Json;
using MapInator.Shared.Translations;

namespace MapInator.App.Services;

public class AppTranslationService : ITranslationService
{
    private Dictionary<string, string> _translations = new();
    public string CurrentLanguage { get; private set; } = "da";
    public event Action? OnLanguageChanged;

    private readonly HttpClient _http;

    public AppTranslationService(HttpClient http) => _http = http;

    public async Task SetLanguageAsync(string lang)
    {
        CurrentLanguage = lang;
        var dict = await _http.GetFromJsonAsync<Dictionary<string, string>>($"i18n/{lang}.json");
        _translations = dict ?? new();
        OnLanguageChanged?.Invoke();
    }

    public string Get(string key) =>
        _translations.TryGetValue(key, out var val) ? val : key;

    public string GetType(string hoofdtype) => Get($"type.{hoofdtype}");
    public string GetSubtype(string undertype) => Get($"subtype.{undertype}");
}
