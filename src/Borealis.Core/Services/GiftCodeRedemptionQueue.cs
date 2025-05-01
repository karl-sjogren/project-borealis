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

    private GiftCodeRedemptionQueueItem? _currentItem = null;

    public GiftCodeRedemptionQueue(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<GiftCodeRedemptionQueue> logger) {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task AddToQueueAsync(Player player, GiftCode giftCode, CancellationToken cancellationToken) {
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

    public Task<IReadOnlyCollection<GiftCodeRedemptionQueueItem>> GetQueueAsync(CancellationToken cancellationToken) {
        var queue = _queue.ToList();
        if(_currentItem is not null) {
            queue.Add(_currentItem);
        }

        return Task.FromResult<IReadOnlyCollection<GiftCodeRedemptionQueueItem>>(queue);
    }

    public async Task<int> GetQueueLengthAsync(CancellationToken cancellationToken) {
        var queueItems = await GetQueueAsync(cancellationToken);

        return queueItems.Count;
    }

    public async Task ProcessQueueAsync(CancellationToken cancellationToken) {
        using var scope = _serviceScopeFactory.CreateScope();
        var giftCodeService = scope.ServiceProvider.GetRequiredService<IGiftCodeService>();

        while(_queue.TryDequeue(out var item)) {
            _currentItem = item;
            if(cancellationToken.IsCancellationRequested) {
                _queue.Enqueue(item);
                _currentItem = null;
                return;
            }

            var result = await giftCodeService.RedeemGiftCodeAsync(item.Player.ExternalId, item.GiftCode.Code, cancellationToken);

            if(!result.Success) {
                _logger.LogError("Failed to redeem gift code for player {PlayerName} ({PlayerId}). Reason: {Message}", item.Player.Name, item.Player.ExternalId, result.Message);

                if(result.Message == "Gift code expired." || result.Message == "Claim limit reached.") {
                    item.GiftCode.IsExpired = true;
                    await giftCodeService.UpdateAsync(item.GiftCode, cancellationToken);
                }
            }

            _currentItem = null;
        }
    }
}
