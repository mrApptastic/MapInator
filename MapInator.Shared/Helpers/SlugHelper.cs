namespace MapInator.Shared.Helpers;

public static class SlugHelper
{
    public static string Slugify(string input)
    {
        return input
            .ToLowerInvariant()
            .Replace("æ", "ae")
            .Replace("ø", "oe")
            .Replace("å", "aa")
            .Replace(" ", "-")
            .Replace("/", "-");
    }
}
