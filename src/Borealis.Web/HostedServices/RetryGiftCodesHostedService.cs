
using Borealis.Core;
using Borealis.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Web.HostedServices;

public class RetryGiftCodesHostedService : TimedHostedService {
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RetryGiftCodesHostedService> _logger;

    public RetryGiftCodesHostedService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RetryGiftCodesHostedService> logger)
            : base(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5), logger) {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        using var scope = _serviceScopeFactory.CreateScope();
        var borealisContext = scope.ServiceProvider.GetRequiredService<BorealisContext>();
        var giftCodeService = scope.ServiceProvider.GetRequiredService<IGiftCodeService>();

        var nonExpiredGiftCodes = await borealisContext
            .GiftCodes
            .Where(x => !x.IsExpired)
            .ToListAsync(cancellationToken);

        foreach(var giftCode in nonExpiredGiftCodes) {
            _ = await giftCodeService.EnqueueGiftCodeAsync(giftCode.Id, cancellationToken);
        }
    }
}
