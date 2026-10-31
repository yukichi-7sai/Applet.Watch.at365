using System.Windows.Threading;
using AppDock.SDK;
using Microsoft.Win32;

namespace Applets.Watch;

internal sealed class ClockApplet(Dispatcher dispatcher, long processStartedAt) : IAppDockExtension
{
    private IExtensionContext? context;
    private ClockWindow? clock;
    private ClockWindow? date;
    private IDisposable? settingsSubscription;
    public async Task ActivateAsync(IExtensionContext services, CancellationToken cancellationToken)
    {
        await using var diagnostics = new StartupDiagnostics(services.Log, processStartedAt);
        diagnostics.Mark("activate.received");
        try {
            context = services;
            services.Commands.Register(ClockFonts.ChooseCommand, "フォントを選ぶ", async token => {
                var family = await dispatcher.InvokeAsync(() => clock?.ChooseFont());
                token.ThrowIfCancellationRequested();
                if (family is not null) {
                    await services.Settings.SetAsync("fontFamily", family, token);
                    await ApplyAsync(token);
                }
            });
            diagnostics.Mark("ui.dispatch");
            await dispatcher.InvokeAsync(() => {
                diagnostics.Mark("settings.read");
                var options = ClockOptions.Read(services.Settings);
                diagnostics.Mark("clock.create");
                clock = new ClockWindow();
                diagnostics.Mark("date.create");
                date = new ClockWindow(dateOnly: true);
                clock.Apply(options, stage => diagnostics.Mark("clock." + stage));
                date.Apply(options, stage => diagnostics.Mark("date." + stage));
            });
            await PublishMonitorsAsync(cancellationToken, diagnostics);
            diagnostics.Mark("events.subscribe");
            settingsSubscription = services.Settings.OnChanged(ApplyAsync);
            SystemEvents.DisplaySettingsChanged += DisplayChanged;
            await PublishStateAsync(cancellationToken, diagnostics);
            diagnostics.Mark("activate.ready");
        } catch {
            diagnostics.Mark("activate.failed");
            throw;
        }
    }
    private async Task ApplyAsync(CancellationToken token)
    {
        if (context is null) return;
        var options = ClockOptions.Read(context.Settings);
        await dispatcher.InvokeAsync(() => { clock?.Apply(options); date?.Apply(options); });
        await PublishStateAsync(token);
    }
    private async Task PublishMonitorsAsync(CancellationToken token, StartupDiagnostics? diagnostics = null)
    {
        if (context is null) return;
        diagnostics?.Mark("monitors.dispatch");
        var options = await dispatcher.InvokeAsync(() => {
            diagnostics?.Mark("monitors.enumerate");
            return ClockWindow.MonitorOptions();
        });
        diagnostics?.Mark("monitor.options");
        await context.Settings.SetOptionsAsync("monitor", options, token);
        diagnostics?.Mark("dateMonitor.options");
        await context.Settings.SetOptionsAsync("dateMonitor", options, token);
    }
    private async Task PublishStateAsync(CancellationToken token, StartupDiagnostics? diagnostics = null)
    {
        if (context is null || clock is null || date is null) return;
        diagnostics?.Mark("state.dispatch");
        var panel = await dispatcher.InvokeAsync(() => {
            diagnostics?.Mark("state.build");
            return clock.StatePanel(context.ExtensionId, date);
        });
        diagnostics?.Mark("state.publish");
        await context.Ui.ShowPanelAsync(panel, token);
    }
    private async void DisplayChanged(object? sender, EventArgs e)
    {
        try { await PublishMonitorsAsync(CancellationToken.None); await ApplyAsync(CancellationToken.None); }
        catch (Exception error) { Console.Error.WriteLine(error); }
    }
    public async Task DeactivateAsync(CancellationToken cancellationToken)
    {
        SystemEvents.DisplaySettingsChanged -= DisplayChanged;
        settingsSubscription?.Dispose();
        context = null;
        await dispatcher.InvokeAsync(() => { clock?.Close(); date?.Close(); clock = null; date = null; });
    }
}
