using System.Globalization;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AppDock.SDK;
using Forms = System.Windows.Forms;

namespace Applets.Watch;

internal partial class ClockWindow : Window
{
    private readonly DispatcherTimer timer;
    private ClockOptions options = new();
    private HwndSource? source;
    private nint handle;
    private bool closed;
    private bool refreshing;
    private bool queued;
    private string monitorId = "";
    private DateTime previousTime;
    private readonly bool dateOnly;
    private ClockOptions Placement => dateOnly ? options.DatePlacement : options;
    private bool DisplayVisible => dateOnly ? options.ShowDate : options.Visible;
    public ClockFontSelection FontSelection { get; private set; } = new(ClockFonts.DefaultFamily, ClockFonts.FallbackFamily, false);
    private static IEnumerable<string> InstalledFonts() => Fonts.SystemFontFamilies.Select(family => family.Source);
    public string? ChooseFont()
    {
        using var initial = new System.Drawing.Font(FontSelection.DisplayFamily, 12);
        using var dialog = new Forms.FontDialog {
            Font = initial, FontMustExist = true, ShowEffects = false, ShowColor = false,
            AllowVerticalFonts = false, MinSize = 12, MaxSize = 12
        };
        return dialog.ShowDialog(new DialogOwner(handle)) == Forms.DialogResult.OK ? dialog.Font.Name : null;
    }
    private sealed record DialogOwner(nint Handle) : Forms.IWin32Window;
    public ClockWindow(bool dateOnly = false)
    {
        this.dateOnly = dateOnly;
        InitializeComponent();
        Title = dateOnly ? "Watch - 日付" : "Watch - 時計";
        TimeContent.Visibility = dateOnly ? Visibility.Collapsed : Visibility.Visible;
        Date.Visibility = dateOnly ? Visibility.Visible : Visibility.Collapsed;
        timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher) { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => {
            try {
                var now = DateTime.Now;
                timer.Interval = TimeSpan.FromMilliseconds(1000 - now.Millisecond);
                var minuteChanged = now.Ticks / TimeSpan.TicksPerMinute != previousTime.Ticks / TimeSpan.TicksPerMinute;
                UpdateTime(now);
                if (minuteChanged) RefreshBounds();
                WriteTestSnapshot();
            } catch (Exception e) { Console.Error.WriteLine(e); }
        };
    }
    public static MonitorInfo[] Monitors() => Forms.Screen.AllScreens.OrderBy(s => s.Primary ? 0 : 1).ThenBy(s => s.Bounds.Left).ThenBy(s => s.Bounds.Top)
        .Select(s => new MonitorInfo(s.DeviceName, $"{s.DeviceName.Replace(@"\\.\", "")} · {(s.Primary ? "メイン · " : "")}{s.Bounds.Width}×{s.Bounds.Height}", s.Bounds.Left, s.Bounds.Top, s.Bounds.Width, s.Bounds.Height, s.Primary)).ToArray();
    public static SettingOption[] MonitorOptions() => new[] { new SettingOption("メインモニター（自動）", "primary") }.Concat(Monitors().Select(m => new SettingOption(m.Label, m.Id))).ToArray();
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        handle = new WindowInteropHelper(this).Handle;
        NativeWindow.Overlay(handle);
        source = HwndSource.FromHwnd(handle);
        source?.AddHook(Message);
    }
    private nint Message(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == 0x0084) { handled = true; return -1; } // HTTRANSPARENT
        if (message == 0x0021) { handled = true; return 3; } // MA_NOACTIVATE
        if (message == 0x02E0 || message == 0x007E) QueueRefresh();
        return 0;
    }
    private void QueueRefresh()
    {
        if (closed || queued) return;
        queued = true;
        Dispatcher.InvokeAsync(() => {
            queued = false;
            if (closed) return;
            try { RefreshBounds(); WriteTestSnapshot(); } catch (Exception e) { Console.Error.WriteLine(e); }
        }, DispatcherPriority.Loaded);
    }
    public void Apply(ClockOptions next, Action<string>? startupStage = null)
    {
        options = next.Normalize();
        startupStage?.Invoke("fonts");
        FontSelection = ClockFonts.Select(options.FontFamily, InstalledFonts());
        foreach (var block in new[] { Hours, Seconds, Date }) block.FontFamily = new FontFamily(FontSelection.DisplayFamily);
        startupStage?.Invoke("handle");
        new WindowInteropHelper(this).EnsureHandle();
        startupStage?.Invoke("text");
        Opacity = options.Opacity;
        UpdateTime(DateTime.Now);
        startupStage?.Invoke("layout");
        RefreshBounds();
        if (DisplayVisible) {
            startupStage?.Invoke("show");
            Show();
            startupStage?.Invoke("visible-layout");
            RefreshBounds(); timer.Start();
        } else {
            startupStage?.Invoke("hide");
            timer.Stop(); Hide();
        }
        WriteTestSnapshot();
        startupStage?.Invoke("applied");
    }
    private void UpdateTime(DateTime now)
    {
        Hours.Text = now.ToString("HH:mm", CultureInfo.InvariantCulture);
        Seconds.Text = now.ToString(":ss", CultureInfo.InvariantCulture);
        Date.Text = ClockFormatting.DateText(now, options.DateFormat);
        previousTime = now;
    }
    public void RefreshBounds()
    {
        if (closed || refreshing || handle == 0) return;
        refreshing = true;
        try {
            var placement = Placement;
            var monitor = ClockLayout.SelectMonitor(Monitors(), placement.Monitor);
            if (monitorId != monitor.Id) {
                monitorId = monitor.Id;
                NativeWindow.Move(handle, new(monitor.Left, monitor.Top, monitor.Width, Math.Max(1, (int)ActualHeight)));
            }
            var scale = Math.Max(96, NativeWindow.GetDpiForWindow(handle)) / 96d;
            var width = monitor.Width / scale;
            var margin = Math.Min(placement.HorizontalMargin, Math.Max(0, (width - 1) / 2));
            Seconds.Visibility = options.ShowSeconds ? Visibility.Visible : Visibility.Collapsed;
            var horizontal = placement.Alignment.EndsWith("-right", StringComparison.Ordinal) ? HorizontalAlignment.Right
                : placement.Alignment.EndsWith("-center", StringComparison.Ordinal) ? HorizontalAlignment.Center : HorizontalAlignment.Left;
            TimeContent.HorizontalAlignment = Date.HorizontalAlignment = horizontal;
            SetFont(1);
            foreach (var block in new[] { Hours, Seconds, Date }) block.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var needed = dateOnly ? Date.DesiredSize.Width : Hours.DesiredSize.Width + (options.ShowSeconds ? Seconds.DesiredSize.Width : 0);
            SetFont(Math.Min(1, Math.Max(1, width - 2 * margin) / Math.Max(1, needed)));
            ClockContent.Margin = new Thickness(margin, 0, margin, 0);
            ClockContent.Measure(new Size(Math.Max(1, width - 2 * margin), double.PositiveInfinity));
            var desiredHeight = ClockContent.DesiredSize.Height;
            var bounds = ClockLayout.Place(monitor, desiredHeight, scale, placement);
            Width = width;
            Height = bounds.Height / scale;
            ClockContent.UpdateLayout();
            NativeWindow.Move(handle, bounds);
        } finally { refreshing = false; }
    }
    private void SetFont(double scale)
    {
        var size = options.FontSize * scale;
        Hours.FontSize = size; Hours.Margin = new Thickness(0, -size / 6, 0, -size / 84);
        var secondsSize = options.SecondsFontSize * scale;
        var dateSize = options.DateFontSize * scale;
        Seconds.FontSize = secondsSize; Seconds.Margin = new Thickness(0, -secondsSize / 7, 0, secondsSize / 8.4);
        Date.FontSize = dateSize; Date.Margin = new Thickness(0, -dateSize / 7, 0, dateSize / 8.4);
        var alignment = Placement.Alignment.StartsWith("bottom", StringComparison.Ordinal) ? VerticalAlignment.Bottom : VerticalAlignment.Top;
        Hours.VerticalAlignment = Seconds.VerticalAlignment = Date.VerticalAlignment = alignment;
    }
    public Panel StatePanel(string commandPrefix, ClockWindow dateWindow)
    {
        NativeWindow.GetWindowRect(handle, out var rect);
        var selected = ClockLayout.SelectMonitor(Monitors(), options.Monitor);
        var dateMonitor = ClockLayout.SelectMonitor(Monitors(), options.DateMonitor);
        return new Panel("Watch", "時計と日付を個別に配置できます。表示はクリックを透過し、フォーカスを奪いません。", [
            new("時計", NativeWindow.IsWindowVisible(handle) ? "表示中" : "非表示"),
            new("日付", NativeWindow.IsWindowVisible(dateWindow.handle) ? "表示中" : "非表示"),
            new("フォント", FontSelection.DisplayFamily + (FontSelection.IsUnavailable ? "（選択したフォントが未インストール）" : "")),
            new("時計のモニター", selected.Label + (options.Monitor != "primary" && selected.Id != options.Monitor ? "（未接続のためメインへ退避）" : "")),
            new("日付のモニター", dateMonitor.Label + (options.DateMonitor != "primary" && dateMonitor.Id != options.DateMonitor ? "（未接続のためメインへ退避）" : "")),
            new("日付の表示例", ClockFormatting.DateText(DateTime.Now, options.DateFormat)),
            new("時計の座標", $"{rect.Left}, {rect.Top} · {rect.Right - rect.Left}×{rect.Bottom - rect.Top}")
        ], [new("時計を表示", commandPrefix + ".settings.visible.on"), new("時計を非表示", commandPrefix + ".settings.visible.off"), new("時計を切り替え", commandPrefix + ".settings.visible.toggle"),
            new("日付を表示", commandPrefix + ".settings.showDate.on"), new("日付を非表示", commandPrefix + ".settings.showDate.off"), new("日付を切り替え", commandPrefix + ".settings.showDate.toggle")]);
    }
    private void WriteTestSnapshot()
    {
        var directory = Environment.GetEnvironmentVariable("APPDOCK_WATCH_TEST_OUTPUT");
        if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory);
        NativeWindow.GetWindowRect(handle, out var rect);
        var monitor = ClockLayout.SelectMonitor(Monitors(), Placement.Monitor);
        FrameworkElement content = dateOnly ? Date : TimeContent;
        var contentOrigin = content.TranslatePoint(new Point(0, 0), this);
        var stateFile = Path.Combine(directory, dateOnly ? "date-state.json" : "clock-state.json");
        File.WriteAllText(stateFile + ".tmp", System.Text.Json.JsonSerializer.Serialize(new {
            visible = NativeWindow.IsWindowVisible(handle), text = dateOnly ? Date.Text : Hours.Text + (options.ShowSeconds ? Seconds.Text : ""), date = Date.Text,
            left = rect.Left, top = rect.Top, width = rect.Right - rect.Left, height = rect.Bottom - rect.Top,
            monitor, dpi = NativeWindow.GetDpiForWindow(handle), styles = NativeWindow.GetWindowLong(handle, NativeWindow.ExStyleIndex),
            showInTaskbar = ShowInTaskbar, showActivated = ShowActivated, opacity = Opacity, processId = Environment.ProcessId,
            showSeconds = options.ShowSeconds, showDate = options.ShowDate, font = Hours.FontFamily.Source,
            requestedFont = FontSelection.RequestedFamily, fontInstalled = FontSelection.Installed,
            alignment = Placement.Alignment, requestedMonitor = Placement.Monitor,
            fontSize = dateOnly ? Date.FontSize : Hours.FontSize, secondsFontSize = Seconds.FontSize,
            configuredFontSize = dateOnly ? options.DateFontSize : options.FontSize,
            configuredSecondsFontSize = options.SecondsFontSize,
            horizontalMargin = Placement.HorizontalMargin, verticalMargin = Placement.VerticalMargin,
            timerRunning = timer.IsEnabled, windowHandle = handle.ToInt64(),
            contentLeft = contentOrigin.X, contentWidth = content.ActualWidth, windowWidth = ActualWidth
        }));
        File.Move(stateFile + ".tmp", stateFile, overwrite: true);
        if (DisplayVisible && ClockContent.ActualWidth > 0 && ClockContent.ActualHeight > 0) {
            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(ActualWidth), (int)Math.Ceiling(ActualHeight), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(this);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            var imageFile = Path.Combine(directory, dateOnly ? "date.png" : "clock.png");
            using (var file = File.Create(imageFile + ".tmp")) encoder.Save(file);
            File.Move(imageFile + ".tmp", imageFile, overwrite: true);
        }
    }
    protected override void OnClosed(EventArgs e)
    {
        closed = true;
        timer.Stop();
        source?.RemoveHook(Message);
        base.OnClosed(e);
    }
}
