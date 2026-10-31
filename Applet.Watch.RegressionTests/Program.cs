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
Check("new installations keep both displays off and retain appearance defaults", () => {
    var options = ClockOptions.Read(new TestSettings(new()));
    Equal(false, options.Visible); Equal(false, options.ShowDate); Equal(true, options.ShowSeconds);
    Equal(420d, options.FontSize); Equal(210d, options.SecondsFontSize); Equal(210d, options.DateFontSize);
    Equal(0.4, options.Opacity); Equal("top", options.Alignment); Equal("top-right", options.DateAlignment);
});
Check("saved visibility and clock settings override the new defaults", () => {
    var options = ClockOptions.Read(new TestSettings(new() { ["visible"] = true, ["showDate"] = true, ["fontSize"] = 160d, ["monitor"] = "left", ["alignment"] = "bottom" }));
    Equal(true, options.Visible); Equal(true, options.ShowDate); Equal(160d, options.FontSize); Equal("left", options.Monitor); Equal("bottom", options.Alignment);
});
Check("date remains enabled when the clock is disabled", () => {
    var options = new ClockOptions(Visible: false, ShowDate: true).Normalize();
    Equal(false, options.Visible); Equal(true, options.ShowDate);
});
Check("clock, seconds and date sizes are independent", () => {
    var options = new ClockOptions(FontSize: 160, SecondsFontSize: 320, DateFontSize: 80).Normalize();
    Equal(160d, options.FontSize); Equal(320d, options.SecondsFontSize); Equal(80d, options.DateFontSize);
});
Check("date monitor and margins are independent of the clock", () => {
    var options = new ClockOptions(Monitor: "main", VerticalMargin: 10, DateMonitor: "left", DateAlignment: "bottom-right", DateHorizontalMargin: 40, DateVerticalMargin: 30).Normalize();
    Equal("main", options.Monitor); Equal(10d, options.VerticalMargin);
    Equal("left", options.DatePlacement.Monitor); Equal(40d, options.DatePlacement.HorizontalMargin);
    Equal(new PixelBounds(-1920, 375, 1920, 300), ClockLayout.Place(left, 200, 1.5, options.DatePlacement));
});
Check("all six anchors retain their vertical edge", () => {
    foreach (var anchor in new[] { "top", "top-center", "top-right", "bottom", "bottom-center", "bottom-right" }) {
        var options = new ClockOptions(Alignment: anchor, DateAlignment: anchor).Normalize();
        Equal(anchor, options.Alignment); Equal(anchor, options.DateAlignment);
        Equal(anchor.StartsWith("bottom") ? 1230 : 10, ClockLayout.Place(primary, 200, 1, options with { VerticalMargin = 10 }).Top);
    }
});
Check("invalid independent settings fall back without losing valid fields", () => {
    var options = new ClockOptions(SecondsFontSize: double.NaN, DateFontSize: double.PositiveInfinity, DateMonitor: " ", DateAlignment: "invalid", DateHorizontalMargin: -5, DateVerticalMargin: double.NaN, DateFormat: "invalid").Normalize();
    Equal(210d, options.SecondsFontSize); Equal(210d, options.DateFontSize); Equal("primary", options.DateMonitor);
    Equal("top-right", options.DateAlignment); Equal(0d, options.DateHorizontalMargin); Equal(0d, options.DateVerticalMargin); Equal("M/d ddd", options.DateFormat);
});
Check("date format retains the default English weekday and supports Japanese formats", () => {
    var date = new DateTime(2026, 10, 30);
    Equal("10/30 Fri", ClockFormatting.DateText(date, "M/d ddd"));
    Equal("2026/10/30", ClockFormatting.DateText(date, "yyyy/MM/dd"));
    Equal("2026-10-30", ClockFormatting.DateText(date, "yyyy-MM-dd"));
    Equal("10月30日", ClockFormatting.DateText(date, "M月d日"));
    Equal("10月30日（金）", ClockFormatting.DateText(date, "M月d日（ddd）"));
    Equal("2026年10月30日（金）", ClockFormatting.DateText(date, "yyyy年M月d日（ddd）"));
    Equal("10/30 Fri", ClockFormatting.DateText(date, "invalid"));
});
Check("date formatting crosses day, year and leap-day boundaries", () => {
    Equal("2027/01/01", ClockFormatting.DateText(new DateTime(2026, 12, 31, 23, 59, 59).AddSeconds(1), "yyyy/MM/dd"));
    Equal("2028/02/29", ClockFormatting.DateText(new DateTime(2028, 2, 28).AddDays(1), "yyyy/MM/dd"));
});
var diagnosticMessages = new List<string>();
await using (var diagnostics = new StartupDiagnostics(new TestLog((message, _) => {
    diagnosticMessages.Add(message); return Task.CompletedTask;
}), System.Diagnostics.Stopwatch.GetTimestamp())) {
    diagnostics.Mark("activate.received");
    diagnostics.Mark("clock.fonts");
    diagnostics.Mark("activate.ready");
}
Check("startup diagnostics preserve stage order and invariant elapsed times", () => {
    Equal(3, diagnosticMessages.Count);
    if (!diagnosticMessages[1].Contains("stage=clock.fonts") || !diagnosticMessages[1].Contains("previousStage=activate.received")) throw new Exception("Missing stage transition");
    foreach (var message in diagnosticMessages)
        if (!System.Text.RegularExpressions.Regex.IsMatch(message, @"elapsedMs=\d+\.\d previousStage=\S+ previousMs=\d+\.\d managedMainMs=\d+\.\d utc=")) throw new Exception(message);
});
var failedLogCalls = 0;
await using (var diagnostics = new StartupDiagnostics(new TestLog((_, _) => {
    failedLogCalls++; throw new IOException("Fixture logging failure");
}), System.Diagnostics.Stopwatch.GetTimestamp())) {
    diagnostics.Mark("activate.received"); diagnostics.Mark("activate.ready");
}
Check("failed logging does not fail startup or retry", () => Equal(1, failedLogCalls));
var logEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var never = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var stalledDiagnostics = new StartupDiagnostics(new TestLog((_, _) => {
    logEntered.TrySetResult(); return never.Task;
}), System.Diagnostics.Stopwatch.GetTimestamp());
stalledDiagnostics.Mark("activate.received");
await logEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
stalledDiagnostics.Mark("activate.ready");
await stalledDiagnostics.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(2));
Check("unresponsive logging has a bounded drain even if it ignores cancellation", () => Equal(false, never.Task.IsCompleted));
Console.WriteLine($"{passed}/{passed} passed");

sealed class TestLog(Func<string, CancellationToken, Task> write) : AppDock.SDK.ILogService
{
    public Task InfoAsync(string message, CancellationToken cancellationToken = default) => write(message, cancellationToken);
    public Task ErrorAsync(string message, CancellationToken cancellationToken = default) => throw new NotSupportedException();
}

sealed class TestSettings(Dictionary<string, object> values) : AppDock.SDK.ISettingsService
{
    public T Get<T>(string key, T fallback) => values.TryGetValue(key, out var value) && value is T result ? result : fallback;
    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public IDisposable OnChanged(Func<CancellationToken, Task> handler) => throw new NotSupportedException();
    public Task SetOptionsAsync(string key, IReadOnlyList<AppDock.SDK.SettingOption> options, CancellationToken cancellationToken = default) => throw new NotSupportedException();
}
