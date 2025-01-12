using System.Collections.Concurrent;
using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Borealis.Core.Services;

public class GiftCodeRedemptionQueue : IGiftCodeRedemptionQueue {
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<GiftCodeRedemptionQueue> _logger;

    private readonly ConcurrentQueue<GiftCodeRedemptionQueueItem> _queue = new();

    public GiftCodeRedemptionQueue(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<GiftCodeRedemptionQueue> logger) {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task AddToQueueAsync(WhiteoutSurvivalPlayer player, GiftCode giftCode, CancellationToken cancellationToken) {
        if(_queue.Any(x => x.Player.Id == player.Id && x.GiftCode.Id == giftCode.Id)) {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BorealisContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        var previousRedemption = await context
            .GiftCodeRedemptions
            .FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.GiftCodeId == giftCode.Id, cancellationToken);

        if(previousRedemption is not null) {
            return;
        }

        _queue.Enqueue(new GiftCodeRedemptionQueueItem {
            Player = player,
            GiftCode = giftCode,
            EnqueuedAt = timeProvider.GetUtcNow()
        });
    }

    public async Task ProcessQueueAsync(CancellationToken cancellationToken) {
        using var scope = _serviceScopeFactory.CreateScope();
        var giftCodeService = scope.ServiceProvider.GetRequiredService<IGiftCodeService>();

        while(_queue.TryDequeue(out var item)) {
            if(cancellationToken.IsCancellationRequested) {
                _queue.Enqueue(item);
                return;
            }

            var result = await giftCodeService.RedeemGiftCodeAsync(item.Player.ExternalId, item.GiftCode.Code, cancellationToken);

            if(!result.Success) {
                _logger.LogError("Failed to redeem gift code for player {PlayerId}.", item.Player.ExternalId);
                _queue.Enqueue(item);
            }
        }
    }

    private class GiftCodeRedemptionQueueItem {
        public required WhiteoutSurvivalPlayer Player { get; set; }
        public required GiftCode GiftCode { get; set; }
        public required DateTimeOffset EnqueuedAt { get; set; }
    }
}
