using Applets.Watch;

var passed = 0;
void Check(string name, Action assertion) { assertion(); passed++; Console.WriteLine("PASS " + name); }
void Equal<T>(T expected, T actual) { if (!Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}"); }
var primary = new MonitorInfo("main", "Main", 0, 0, 2560, 1440, true);
var left = new MonitorInfo("left", "Left", -1920, -360, 1920, 1080, false);
var monitors = new[] { left, primary };
Check("primary fallback does not depend on enumeration order", () => Equal(primary, ClockLayout.SelectMonitor(monitors, "primary")));
Check("select a monitor at negative desktop coordinates", () => Equal(left, ClockLayout.SelectMonitor(monitors, "left")));
Check("unplugged monitor falls back without replacing the configured ID", () => { var options = new ClockOptions(Monitor: "unplugged"); Equal(primary, ClockLayout.SelectMonitor(monitors, options.Monitor)); Equal("unplugged", options.Monitor); });
Check("top margin converts DIP to physical pixels", () => Equal(new PixelBounds(-1920, -315, 1920, 300), ClockLayout.Place(left, 200, 1.5, new(VerticalMargin: 30))));
Check("bottom placement respects monitor origin and DPI", () => Equal(new PixelBounds(-1920, 375, 1920, 300), ClockLayout.Place(left, 200, 1.5, new(Alignment: "bottom", VerticalMargin: 30))));
Check("oversized clock is clipped to the selected monitor", () => Equal(new PixelBounds(-1920, -360, 1920, 1080), ClockLayout.Place(left, 3000, 2, new(Alignment: "bottom", VerticalMargin: 1000))));
Check("excessive margins keep the window in the screen", () => Equal(1140, ClockLayout.Place(primary, 300, 1, new(VerticalMargin: 10000)).Top));
Check("manual invalid settings remain safe", () => { var options = new ClockOptions(Alignment: "invalid", FontSize: double.NaN, Opacity: 20, HorizontalMargin: -100, VerticalMargin: double.PositiveInfinity).Normalize(); Equal("top", options.Alignment); Equal(420d, options.FontSize); Equal(1d, options.Opacity); Equal(0d, options.HorizontalMargin); Equal(0d, options.VerticalMargin); });
Check("installed font is selected by family name without loading a file", () => {
    var font = ClockFonts.Select("haettenschweiler", new[] { "Arial", "Haettenschweiler" });
    Equal(true, font.Installed); Equal("Haettenschweiler", font.DisplayFamily);
});
Check("missing font falls back while preserving the requested family", () => {
    var font = ClockFonts.Select("Missing Font", new[] { "Segoe UI" });
    Equal(false, font.Installed); Equal("Segoe UI", font.DisplayFamily); Equal("Missing Font", font.RequestedFamily);
});
Check("invalid font settings normalize without replacing a valid missing selection", () => {
    Equal(ClockFonts.DefaultFamily, new ClockOptions(FontFamily: " ").Normalize().FontFamily);
    Equal(ClockFonts.DefaultFamily, new ClockOptions(FontFamily: new string('x', 257)).Normalize().FontFamily);
    Equal("Missing Font", new ClockOptions(FontFamily: " Missing Font ").Normalize().FontFamily);
});
Check("empty default uses Segoe UI without an unavailable-font status", () => {
    Equal("", new ClockOptions().FontFamily);
    var font = ClockFonts.Select("", Array.Empty<string>());
    Equal("", font.RequestedFamily); Equal("Segoe UI", font.DisplayFamily); Equal(false, font.IsUnavailable);
});
Console.WriteLine($"{passed}/{passed} passed");
