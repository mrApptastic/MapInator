using MapInator.Shared.Models;

namespace MapInator.Shared.Validation;

public static class StednavnValidator
{
    public static bool IsValid(Stednavn stednavn) =>
        !string.IsNullOrWhiteSpace(stednavn.Id) &&
        !string.IsNullOrWhiteSpace(stednavn.Navn) &&
        !string.IsNullOrWhiteSpace(stednavn.Hovedtype) &&
        !string.IsNullOrWhiteSpace(stednavn.Undertype) &&
        stednavn.Visueltcenter is { Length: 2 };
}
