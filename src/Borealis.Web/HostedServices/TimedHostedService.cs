using System.Diagnostics;

namespace Borealis.Web.HostedServices;

public abstract class TimedHostedService : IHostedService, IDisposable {
    private readonly TimeSpan _interval;
    private readonly TimeSpan _waitBeforeStart;
    private readonly ILogger<TimedHostedService> _logger;
    private Timer? _timer;

    protected TimedHostedService(TimeSpan interval, TimeSpan waitBeforeStart, ILogger<TimedHostedService> logger) {
        _interval = interval;
        _waitBeforeStart = waitBeforeStart;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Starting timed service ({type}) which will run in {waitBeforeStart} and then every {interval}.", GetType().Name, _waitBeforeStart, _interval);

        _timer = new Timer(TimerCallback, stoppingToken, _waitBeforeStart, _interval);
        return Task.CompletedTask;
    }

#pragma warning disable VSTHRD100 // Avoid "async void" methods, because any exceptions not handled by the method will crash the process.
    private async void TimerCallback(object? state) {
        try {
            var cancellationToken = CancellationToken.None;
            if(state is CancellationToken token) {
                cancellationToken = token;
            }

            _logger.LogInformation("Starting execution of timed service ({type}).", GetType().Name);

            var stopwatch = Stopwatch.StartNew();
            await ExecuteAsync(cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation("Done executing the callback for {type} which took {elapsed}. It will run again in {interval}.", GetType().Name, stopwatch.Elapsed, _interval);
        } catch(Exception e) {
            _logger.LogError(e, "An exception occured while executing the callback for {type}. It will run again in {interval}.", GetType().Name, _interval);
        }
    }
#pragma warning restore CS1998

    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

    public Task StopAsync(CancellationToken stoppingToken) {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
