using System.Windows.Threading;
using AppDock.SDK;
using Microsoft.Win32;

namespace Applets.Watch;

internal sealed class ClockApplet(Dispatcher dispatcher) : IAppDockExtension
{
    private IExtensionContext? context;
    private ClockWindow? clock;
    private ClockWindow? date;
    private IDisposable? settingsSubscription;
    public async Task ActivateAsync(IExtensionContext services, CancellationToken cancellationToken)
    {
        context = services;
        services.Commands.Register(ClockFonts.ChooseCommand, "フォントを選ぶ", async token => {
            var family = await dispatcher.InvokeAsync(() => clock?.ChooseFont());
            token.ThrowIfCancellationRequested();
            if (family is not null) {
                await services.Settings.SetAsync("fontFamily", family, token);
                await ApplyAsync(token);
            }
        });
        await dispatcher.InvokeAsync(() => {
            var options = ClockOptions.Read(services.Settings);
            clock = new ClockWindow(); date = new ClockWindow(dateOnly: true);
            clock.Apply(options); date.Apply(options);
        });
        await PublishMonitorsAsync(cancellationToken);
        settingsSubscription = services.Settings.OnChanged(ApplyAsync);
        SystemEvents.DisplaySettingsChanged += DisplayChanged;
        await PublishStateAsync(cancellationToken);
    }
    private async Task ApplyAsync(CancellationToken token)
    {
        if (context is null) return;
        var options = ClockOptions.Read(context.Settings);
        await dispatcher.InvokeAsync(() => { clock?.Apply(options); date?.Apply(options); });
        await PublishStateAsync(token);
    }
    private async Task PublishMonitorsAsync(CancellationToken token)
    {
        if (context is null) return;
        var options = await dispatcher.InvokeAsync(ClockWindow.MonitorOptions);
        await context.Settings.SetOptionsAsync("monitor", options, token);
        await context.Settings.SetOptionsAsync("dateMonitor", options, token);
    }
    private async Task PublishStateAsync(CancellationToken token)
    {
        if (context is null || clock is null || date is null) return;
        var panel = await dispatcher.InvokeAsync(() => clock.StatePanel(context.ExtensionId, date));
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
