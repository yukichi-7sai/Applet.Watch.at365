using AppDock.SDK;

namespace Applets.Watch;

internal sealed record ClockFontSelection(string RequestedFamily, string DisplayFamily, bool Installed)
{
    public bool NeedsNotice => !string.IsNullOrEmpty(RequestedFamily) && !Installed;
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

    public static NotificationRequest Notice(ClockFontSelection font)
    {
        var body = $"「{font.RequestedFamily}」が見つからないため、{font.DisplayFamily}で時計を表示します。Watchの設定で、このPCにインストール済みのフォントを選べます。フォントを新しくインストールした場合はWatchを再起動してください。";
        return new("font-unavailable", "時計のフォントを確認してください", body, Level: "info", DedupeKey: "font-unavailable",
            HistoryText: new("時計のフォント", body));
    }
}

internal sealed class ClockFontNotice(INotificationService notifications, ILogService log)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private string? missingFamily;
    private string? notificationId;

    public async Task UpdateAsync(ClockFontSelection font, CancellationToken token)
    {
        await gate.WaitAsync(token);
        try {
            if (font.NeedsNotice && string.Equals(missingFamily, font.RequestedFamily, StringComparison.OrdinalIgnoreCase)) return;
            if (notificationId is not null) {
                await notifications.EndAsync(notificationId, font.NeedsNotice ? "withdrawn" : "resolved", token);
                notificationId = null;
            }
            missingFamily = font.NeedsNotice ? font.RequestedFamily : null;
            if (font.NeedsNotice) {
                var result = await notifications.PublishAsync(ClockFonts.Notice(font), token);
                notificationId = result.NotificationId;
            }
        } catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception) {
            try { await log.ErrorAsync("時計のフォント案内を更新できませんでした。フォントはWatchの設定から選べます。", token); }
            catch (Exception) { Console.Error.WriteLine("時計のフォント案内を更新できませんでした。"); }
        } finally { gate.Release(); }
    }
}
