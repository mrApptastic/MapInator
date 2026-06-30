using System.Text.Json;

namespace MapInator.Shared.Translations;

public interface ITranslationService
{
    string Get(string key);
    string GetType(string hoofdtype);
    string GetSubtype(string undertype);
    string CurrentLanguage { get; }
    Task SetLanguageAsync(string lang);
    event Action? OnLanguageChanged;
}

public class TranslationService : ITranslationService
{
    private Dictionary<string, string> _translations = new();
    public string CurrentLanguage { get; private set; } = "da";
    public event Action? OnLanguageChanged;

    private readonly Func<string, Task<Dictionary<string, string>>> _loader;

    public TranslationService(Func<string, Task<Dictionary<string, string>>> loader)
    {
        _loader = loader;
    }

    public async Task SetLanguageAsync(string lang)
    {
        CurrentLanguage = lang;
        _translations = await _loader(lang);
        OnLanguageChanged?.Invoke();
    }

    public string Get(string key) =>
        _translations.TryGetValue(key, out var val) ? val : key;

    public string GetType(string hoofdtype) =>
        Get($"type.{hoofdtype}");

    public string GetSubtype(string undertype) =>
        Get($"subtype.{undertype}");
}
