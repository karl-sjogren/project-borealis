
using Borealis.Core.Contracts;
using Borealis.Core.Requests;
using Borealis.WhiteoutSurvivalHttpClient;

namespace Borealis.Web.HostedServices;

public class GiftCodeCheckDailyHostedService : DailyHostedService {
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<GiftCodeCheckDailyHostedService> _logger;

    public GiftCodeCheckDailyHostedService(
            IServiceScopeFactory serviceScopeFactory,
            TimeProvider timeProvider,
            ILogger<GiftCodeCheckDailyHostedService> logger) : base(TimeSpan.Parse("04:00:00"), timeProvider, logger) {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        using var scope = _serviceScopeFactory.CreateScope();

        var giftCodeService = scope.ServiceProvider.GetRequiredService<IGiftCodeService>();
        var giftCodes = await giftCodeService.GetPagedAsync(new GiftCodeQuery { IsExpired = false, PageIndex = 0, PageSize = 10_000 }, cancellationToken);

        var playerService = scope.ServiceProvider.GetRequiredService<IPlayerService>();
        var players = await playerService.GetPagedAsync(new PlayerQuery { PageIndex = 0, PageSize = 1 }, cancellationToken);

        if(players.TotalCount == 0) {
            _logger.LogWarning("No players found, can't validate gift codes.");
            return;
        }

        var player = players.Items.First();

        var whiteoutSurvivalHttpClient = scope.ServiceProvider.GetRequiredService<IWhiteoutSurvivalHttpClient>();

        var playerResult = await whiteoutSurvivalHttpClient.GetPlayerInfoAsync(player.ExternalId, cancellationToken);
        if(playerResult.Code != 0) {
            _logger.LogWarning("Failed to get player info for player {ExternalId}.", player.ExternalId);
            return;
        }

        foreach(var giftCode in giftCodes.Items) {
            var redeemResult = await whiteoutSurvivalHttpClient.RedeemGiftCodeAsync(player.ExternalId, giftCode.Code, cancellationToken);

            var isExpired = redeemResult.ErrorCode == 40007;

            if(!isExpired) {
                continue;
            }

            giftCode.IsExpired = true;

            await giftCodeService.UpdateAsync(giftCode, cancellationToken);
        }
    }
}
