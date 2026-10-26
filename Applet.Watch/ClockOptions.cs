using AppDock.SDK;

namespace Applets.Watch;

internal sealed record ClockOptions(bool Visible = true, string Monitor = "primary", string Alignment = "top", double FontSize = 420, double Opacity = 0.4, double HorizontalMargin = 20, double VerticalMargin = 0, bool ShowSeconds = true, bool ShowDate = true, string FontFamily = ClockFonts.DefaultFamily)
{
    public static ClockOptions Read(ISettingsService settings) => new ClockOptions(
        settings.Get("visible", true), settings.Get("monitor", "primary"), settings.Get("alignment", "top"),
        settings.Get("fontSize", 420d), settings.Get("opacity", 0.4), settings.Get("horizontalMargin", 20d),
        settings.Get("verticalMargin", 0d), settings.Get("showSeconds", true), settings.Get("showDate", true),
        settings.Get("fontFamily", ClockFonts.DefaultFamily)).Normalize();
    public ClockOptions Normalize() => this with {
        Monitor = string.IsNullOrWhiteSpace(Monitor) || Monitor.Length > 256 ? "primary" : Monitor,
        Alignment = Alignment == "bottom" ? "bottom" : "top",
        FontFamily = string.IsNullOrWhiteSpace(FontFamily) || FontFamily.Length > 256 ? ClockFonts.DefaultFamily : FontFamily.Trim(),
        FontSize = Limit(FontSize, 420, 48, 720), Opacity = Limit(Opacity, 0.4, 0.05, 1),
        HorizontalMargin = Limit(HorizontalMargin, 20, 0, 1000), VerticalMargin = Limit(VerticalMargin, 0, 0, 1000)
    };
    private static double Limit(double value, double fallback, double min, double max) => double.IsFinite(value) ? Math.Clamp(value, min, max) : fallback;
}
