using System.Diagnostics;
using System.Globalization;
using System.Threading.Channels;
using AppDock.SDK;

namespace Applets.Watch;

// Diagnostic delivery must not make the UI thread wait for a host RPC response.
internal sealed class StartupDiagnostics : IAsyncDisposable
{
    private readonly long startedAt = Stopwatch.GetTimestamp();
    private readonly long processStartedAt;
    private readonly string attempt = Guid.NewGuid().ToString("N")[..8];
    // The fixed startup path emits fewer than 64 records. Bound queued diagnostics
    // if a future caller repeats a stage while the host has stopped responding.
    private readonly Channel<string> queue = Channel.CreateBounded<string>(new BoundedChannelOptions(64) {
        SingleReader = true, FullMode = BoundedChannelFullMode.DropOldest
    });
    private readonly CancellationTokenSource delivery = new();
    private readonly Task pump;
    private readonly object sync = new();
    private long previousAt;
    private string previousStage = "none";

    public StartupDiagnostics(ILogService log, long processStartedAt)
    {
        this.processStartedAt = processStartedAt;
        previousAt = startedAt;
        pump = Task.Run(async () => {
            try {
                await foreach (var message in queue.Reader.ReadAllAsync(delivery.Token))
                    await log.InfoAsync(message, delivery.Token).WaitAsync(delivery.Token);
            } catch (Exception) {
                // Logging failures neither fail activation nor start retries/fallback logging.
            }
        });
    }

    public void Mark(string stage)
    {
        lock (sync) {
            var now = Stopwatch.GetTimestamp();
            var message = string.Create(CultureInfo.InvariantCulture,
                $"[Watch起動診断] attempt={attempt} stage={stage} elapsedMs={Stopwatch.GetElapsedTime(startedAt, now).TotalMilliseconds:F1} previousStage={previousStage} previousMs={Stopwatch.GetElapsedTime(previousAt, now).TotalMilliseconds:F1} managedMainMs={Stopwatch.GetElapsedTime(processStartedAt, now).TotalMilliseconds:F1} utc={DateTimeOffset.UtcNow:O}");
            queue.Writer.TryWrite(message);
            previousAt = now;
            previousStage = stage;
        }
    }

    public async ValueTask DisposeAsync()
    {
        queue.Writer.TryComplete();
        try { await pump.WaitAsync(TimeSpan.FromMilliseconds(250)); }
        catch (TimeoutException) { await delivery.CancelAsync(); await pump; }
        finally { delivery.Dispose(); }
    }
}
