
using Borealis.Core.Contracts;
using Borealis.Core.Requests;

namespace Borealis.Web.HostedServices;

public class UpdatePlayersHostedService : TimedHostedService {
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<UpdatePlayersHostedService> _logger;

    public UpdatePlayersHostedService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<UpdatePlayersHostedService> logger)
            : base(TimeSpan.FromHours(1), TimeSpan.FromMinutes(5), logger) {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using var scope = _serviceScopeFactory.CreateScope();
        var playerService = scope.ServiceProvider.GetRequiredService<IPlayerService>();

        try {
            var players = await playerService.GetPagedAsync(new PlayerQuery { PageIndex = 0, PageSize = 10_000 }, stoppingToken);
            foreach(var player in players.Items.OrderByDescending(x => x.UpdatedAt)) {
                await playerService.SynchronizePlayerAsync(player.ExternalId, false, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        } catch(Exception ex) {
            _logger.LogError(ex, "An error occurred while processing the gift code redemption queue.");
        }
    }
}
