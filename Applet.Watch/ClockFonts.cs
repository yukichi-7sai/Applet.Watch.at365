namespace Applets.Watch;

internal sealed record ClockFontSelection(string RequestedFamily, string DisplayFamily, bool Installed)
{
    public bool IsUnavailable => !string.IsNullOrEmpty(RequestedFamily) && !Installed;
}

internal static class ClockFonts
{
    public const string DefaultFamily = "";
    public const string FallbackFamily = "Segoe UI";
    public const string ChooseCommand = "at365.watch.choose-font";

    public static ClockFontSelection Select(string requested, IEnumerable<string> installed)
    {
        if (string.IsNullOrWhiteSpace(requested))
            return new("", FallbackFamily, installed.Contains(FallbackFamily, StringComparer.OrdinalIgnoreCase));
        var match = installed.FirstOrDefault(name => string.Equals(name, requested, StringComparison.OrdinalIgnoreCase));
        return new(requested, match ?? FallbackFamily, match is not null);
    }
}
