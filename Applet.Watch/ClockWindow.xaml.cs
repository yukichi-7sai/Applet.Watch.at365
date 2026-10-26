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
    public ClockWindow()
    {
        InitializeComponent();
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
    public void Apply(ClockOptions next)
    {
        options = next.Normalize();
        FontSelection = ClockFonts.Select(options.FontFamily, InstalledFonts());
        foreach (var block in new[] { Hours, Seconds, Date }) block.FontFamily = new FontFamily(FontSelection.DisplayFamily);
        new WindowInteropHelper(this).EnsureHandle();
        Opacity = options.Opacity;
        UpdateTime(DateTime.Now);
        RefreshBounds();
        if (options.Visible) { Show(); RefreshBounds(); timer.Start(); }
        else { timer.Stop(); Hide(); }
        WriteTestSnapshot();
    }
    private void UpdateTime(DateTime now)
    {
        Hours.Text = now.ToString("HH:mm", CultureInfo.InvariantCulture);
        Seconds.Text = now.ToString(":ss", CultureInfo.InvariantCulture);
        Date.Text = now.ToString("M/d ddd", CultureInfo.InvariantCulture);
        previousTime = now;
    }
    public void RefreshBounds()
    {
        if (closed || refreshing || handle == 0) return;
        refreshing = true;
        try {
            var monitor = ClockLayout.SelectMonitor(Monitors(), options.Monitor);
            if (monitorId != monitor.Id) {
                monitorId = monitor.Id;
                NativeWindow.Move(handle, new(monitor.Left, monitor.Top, monitor.Width, Math.Max(1, (int)ActualHeight)));
            }
            var scale = Math.Max(96, NativeWindow.GetDpiForWindow(handle)) / 96d;
            var width = monitor.Width / scale;
            var margin = Math.Min(options.HorizontalMargin, width / 4);
            Seconds.Visibility = options.ShowSeconds ? Visibility.Visible : Visibility.Collapsed;
            Date.Visibility = options.ShowDate ? Visibility.Visible : Visibility.Collapsed;
            SetFont(options.FontSize);
            foreach (var block in new[] { Hours, Seconds, Date }) block.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var needed = Hours.DesiredSize.Width + (options.ShowSeconds ? Seconds.DesiredSize.Width : 0) + (options.ShowDate ? Date.DesiredSize.Width : 0);
            SetFont(options.FontSize * Math.Min(1, Math.Max(1, width - 2 * margin) / Math.Max(1, needed)));
            ClockContent.Margin = new Thickness(margin, 0, margin, 0);
            ClockContent.Measure(new Size(Math.Max(1, width - 2 * margin), double.PositiveInfinity));
            var desiredHeight = ClockContent.DesiredSize.Height;
            var bounds = ClockLayout.Place(monitor, desiredHeight, scale, options);
            Width = width;
            Height = bounds.Height / scale;
            ClockContent.UpdateLayout();
            NativeWindow.Move(handle, bounds);
        } finally { refreshing = false; }
    }
    private void SetFont(double size)
    {
        Hours.FontSize = size; Hours.Margin = new Thickness(0, -size / 6, 0, -size / 84);
        Seconds.FontSize = size / 2; Seconds.Margin = new Thickness(0, -size / 14, 0, size / 16.8);
        Date.FontSize = size / 2; Date.Margin = new Thickness(0, -size / 14, 0, size / 16.8);
        var alignment = options.Alignment == "bottom" ? VerticalAlignment.Bottom : VerticalAlignment.Top;
        Hours.VerticalAlignment = Seconds.VerticalAlignment = Date.VerticalAlignment = alignment;
    }
    public Panel StatePanel(string commandPrefix)
    {
        NativeWindow.GetWindowRect(handle, out var rect);
        var selected = ClockLayout.SelectMonitor(Monitors(), options.Monitor);
        return new Panel("Your desktop clock.", "元のWatchの時計を、AppDockから管理します。時計はクリックを透過し、フォーカスを奪いません。", [
            new("表示", NativeWindow.IsWindowVisible(handle) ? "表示中" : "非表示"),
            new("フォント", FontSelection.DisplayFamily + (FontSelection.NeedsNotice ? "（選択したフォントが未インストール）" : "")),
            new("モニター", selected.Label + (options.Monitor != "primary" && selected.Id != options.Monitor ? "（未接続のためメインへ退避）" : "")),
            new("位置", options.Alignment == "bottom" ? "下端" : "上端"),
            new("座標", $"{rect.Left}, {rect.Top} · {rect.Right - rect.Left}×{rect.Bottom - rect.Top}")
        ], [new("表示", commandPrefix + ".settings.visible.on"), new("非表示", commandPrefix + ".settings.visible.off"), new("表示を切り替え", commandPrefix + ".settings.visible.toggle")]);
    }
    private void WriteTestSnapshot()
    {
        var directory = Environment.GetEnvironmentVariable("APPDOCK_WATCH_TEST_OUTPUT");
        if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory);
        NativeWindow.GetWindowRect(handle, out var rect);
        var monitor = ClockLayout.SelectMonitor(Monitors(), options.Monitor);
        var stateFile = Path.Combine(directory, "clock-state.json");
        File.WriteAllText(stateFile + ".tmp", System.Text.Json.JsonSerializer.Serialize(new {
            visible = NativeWindow.IsWindowVisible(handle), text = Hours.Text + Seconds.Text, date = Date.Text,
            left = rect.Left, top = rect.Top, width = rect.Right - rect.Left, height = rect.Bottom - rect.Top,
            monitor, dpi = NativeWindow.GetDpiForWindow(handle), styles = NativeWindow.GetWindowLong(handle, NativeWindow.ExStyleIndex),
            showInTaskbar = ShowInTaskbar, showActivated = ShowActivated, opacity = Opacity, processId = Environment.ProcessId,
            showSeconds = options.ShowSeconds, showDate = options.ShowDate, font = Hours.FontFamily.Source,
            requestedFont = FontSelection.RequestedFamily, fontInstalled = FontSelection.Installed
        }));
        File.Move(stateFile + ".tmp", stateFile, overwrite: true);
        if (options.Visible && ClockContent.ActualWidth > 0 && ClockContent.ActualHeight > 0) {
            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(ClockContent.ActualWidth), (int)Math.Ceiling(ClockContent.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            var drawing = new DrawingVisual();
            using (var graphics = drawing.RenderOpen()) {
                graphics.PushOpacity(Opacity);
                graphics.DrawRectangle(new VisualBrush(ClockContent), null, new Rect(0, 0, ClockContent.ActualWidth, ClockContent.ActualHeight));
                graphics.Pop();
            }
            bitmap.Render(drawing);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var file = File.Create(Path.Combine(directory, "clock.png")); encoder.Save(file);
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
