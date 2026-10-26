namespace Applets.Watch;

internal sealed record MonitorInfo(string Id, string Label, int Left, int Top, int Width, int Height, bool Primary);
internal sealed record PixelBounds(int Left, int Top, int Width, int Height);
internal static class ClockLayout
{
    public static MonitorInfo SelectMonitor(IReadOnlyList<MonitorInfo> monitors, string id) =>
        monitors.FirstOrDefault(m => m.Id == id) ?? monitors.FirstOrDefault(m => m.Primary) ?? monitors.FirstOrDefault() ?? throw new InvalidOperationException("モニターを取得できません。");
    public static PixelBounds Place(MonitorInfo monitor, double desiredHeight, double dpiScale, ClockOptions settings)
    {
        var height = Math.Clamp((int)Math.Ceiling(desiredHeight * dpiScale), 1, monitor.Height);
        var margin = Math.Clamp((int)Math.Round(settings.VerticalMargin * dpiScale), 0, monitor.Height - height);
        var top = settings.Alignment == "bottom" ? monitor.Top + monitor.Height - height - margin : monitor.Top + margin;
        return new(monitor.Left, top, monitor.Width, height);
    }
}
