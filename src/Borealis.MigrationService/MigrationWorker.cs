using System.Diagnostics;
using Borealis.Core;
using Microsoft.EntityFrameworkCore;

namespace Borealis.MigrationService;

public class MigrationWorker : BackgroundService {
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public MigrationWorker(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime) {
        _serviceProvider = serviceProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        using var activity = _activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BorealisContext>();

            await RunMigrationAsync(context, cancellationToken);
        } catch(Exception ex) {
            activity?.AddException(ex);
            throw;
        }

        _hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(BorealisContext context, CancellationToken cancellationToken) {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => {
            await context.Database.MigrateAsync(cancellationToken);
        });
    }
}
