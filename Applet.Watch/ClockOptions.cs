using AppDock.SDK;

namespace Applets.Watch;

internal sealed record ClockOptions(bool Visible = false, string Monitor = "primary", string Alignment = "top", double FontSize = 420, double Opacity = 0.4, double HorizontalMargin = 20, double VerticalMargin = 0, bool ShowSeconds = true, bool ShowDate = false, string FontFamily = ClockFonts.DefaultFamily,
    double SecondsFontSize = 210, double DateFontSize = 210, string DateFormat = "M/d ddd", string DateMonitor = "primary", string DateAlignment = "top-right", double DateHorizontalMargin = 20, double DateVerticalMargin = 0)
{
    public static ClockOptions Read(ISettingsService settings) => new ClockOptions(
        settings.Get("visible", false), settings.Get("monitor", "primary"), settings.Get("alignment", "top"),
        settings.Get("fontSize", 420d), settings.Get("opacity", 0.4), settings.Get("horizontalMargin", 20d),
        settings.Get("verticalMargin", 0d), settings.Get("showSeconds", true), settings.Get("showDate", false),
        settings.Get("fontFamily", ClockFonts.DefaultFamily), settings.Get("secondsFontSize", 210d),
        settings.Get("dateFontSize", 210d), settings.Get("dateFormat", "M/d ddd"), settings.Get("dateMonitor", "primary"),
        settings.Get("dateAlignment", "top-right"), settings.Get("dateHorizontalMargin", 20d), settings.Get("dateVerticalMargin", 0d)).Normalize();
    public ClockOptions Normalize() => this with {
        Monitor = string.IsNullOrWhiteSpace(Monitor) || Monitor.Length > 256 ? "primary" : Monitor,
        Alignment = NormalizeAlignment(Alignment, "top"),
        DateAlignment = NormalizeAlignment(DateAlignment, "top-right"),
        DateMonitor = string.IsNullOrWhiteSpace(DateMonitor) || DateMonitor.Length > 256 ? "primary" : DateMonitor,
        DateFormat = ClockFormatting.Formats.Contains(DateFormat) ? DateFormat : ClockFormatting.DefaultFormat,
        FontFamily = string.IsNullOrWhiteSpace(FontFamily) || FontFamily.Length > 256 ? ClockFonts.DefaultFamily : FontFamily.Trim(),
        FontSize = Limit(FontSize, 420, 48, 720), Opacity = Limit(Opacity, 0.4, 0.05, 1),
        HorizontalMargin = Limit(HorizontalMargin, 20, 0, 1000), VerticalMargin = Limit(VerticalMargin, 0, 0, 1000),
        SecondsFontSize = Limit(SecondsFontSize, 210, 24, 720), DateFontSize = Limit(DateFontSize, 210, 24, 720),
        DateHorizontalMargin = Limit(DateHorizontalMargin, 20, 0, 1000), DateVerticalMargin = Limit(DateVerticalMargin, 0, 0, 1000)
    };
    public ClockOptions DatePlacement => this with { Monitor = DateMonitor, Alignment = DateAlignment, HorizontalMargin = DateHorizontalMargin, VerticalMargin = DateVerticalMargin };
    private static string NormalizeAlignment(string value, string fallback) => value is "top" or "top-center" or "top-right" or "bottom" or "bottom-center" or "bottom-right" ? value : fallback;
    private static double Limit(double value, double fallback, double min, double max) => double.IsFinite(value) ? Math.Clamp(value, min, max) : fallback;
}
