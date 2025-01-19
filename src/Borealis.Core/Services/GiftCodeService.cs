using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Core.Services;

public class GiftCodeService : QueryServiceBase<GiftCode>, IGiftCodeService {
    private readonly BorealisContext _context;
    private readonly IWhiteoutSurvivalHttpClient _whiteoutSurvivalHttpClient;
    private readonly IGiftCodeRedemptionQueue _giftCodeRedemptionQueue;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GiftCodeService> _logger;

    protected override string DefaultSortProperty => nameof(Player.CreatedAt);
    protected override bool DefaultSortAscending => false;

    public GiftCodeService(
            BorealisContext context,
            IWhiteoutSurvivalHttpClient whiteoutSurvivalHttpClient,
            IGiftCodeRedemptionQueue giftCodeRedemptionQueue,
            TimeProvider timeProvider,
            ILogger<GiftCodeService> logger) {
        _context = context;
        _whiteoutSurvivalHttpClient = whiteoutSurvivalHttpClient;
        _giftCodeRedemptionQueue = giftCodeRedemptionQueue;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<GiftCode>> GetByIdAsync(Guid playerId, CancellationToken cancellationToken) {
        var entity = await _context
            .GiftCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == playerId, cancellationToken);

        if(entity is null) {
            return Results.NotFound<GiftCode>();
        }

        return Results.Success(entity);
    }

    public async Task<Result> AddGiftCodeAsync(string giftCode, CancellationToken cancellationToken) {
        var existingGiftCode = await _context.GiftCodes.FirstOrDefaultAsync(x => x.Code == giftCode, cancellationToken);

        // Gift codes are unfortunately case-sensitive
        if(existingGiftCode?.Code.Equals(giftCode, StringComparison.Ordinal) == true) {
            return Results.Conflict("Gift code already exists.");
        }

        var player = await _context
            .Players
            .OrderByDescending(x => x.ExternalId)
            .FirstOrDefaultAsync(cancellationToken);

        if(player is not null) {
            var playerResult = await _whiteoutSurvivalHttpClient.GetPlayerInfoAsync(player.ExternalId, cancellationToken); // We need to "sign in" the player
            var redeemResult = await _whiteoutSurvivalHttpClient.RedeemGiftCodeAsync(player.ExternalId, giftCode, cancellationToken);

            switch(redeemResult.ErrorCode) {
                case 20000: // Code success
                case 40008: // Code already used
                case 40011: // Code already used
                    break;
                case 40014:
                    return Results.Failure("Gift code not found.");
                case 40004:
                    return Results.Failure("Player not found.");
                case 40007:
                    return Results.Failure("Gift code expired.");
                case 40009:
                    return Results.Failure("Player not logged in.");
                default:
                    return Results.Failure($"Unknown error code: {redeemResult.ErrorCode}, message: {redeemResult.Message}");
            }
        }

        var newGiftCode = new GiftCode {
            Code = giftCode,
            CreatedAt = _timeProvider.GetUtcNow()
        };

        await _context.GiftCodes.AddAsync(newGiftCode, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await EnqueueGiftCodeAsync(newGiftCode.Id, cancellationToken);

        return Results.Success();
    }

    public async Task<PagedResult<GiftCode>> GetPagedAsync(GiftCodeQuery query, CancellationToken cancellationToken) {
        var entities = await BuildQuery(_context.GiftCodes.AsNoTracking(), query)
            .Skip(query.PageIndex * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await BuildQuery(_context.GiftCodes, query).CountAsync(cancellationToken);

        return Results.PagedSuccess(entities, query, totalCount);
    }

    private IQueryable<GiftCode> BuildQuery(IQueryable<GiftCode> dbQuery, GiftCodeQuery query) {
        if(!string.IsNullOrWhiteSpace(query.Query)) {
            dbQuery = dbQuery.Where(x => x.Code.Contains(query.Query));
        }

        return base.AddSorting(dbQuery, query);
    }

    public async Task<Result> EnqueueGiftCodeAsync(Guid giftCodeId, CancellationToken cancellationToken) {
        var giftCode = await _context.GiftCodes.FirstOrDefaultAsync(x => x.Id == giftCodeId, cancellationToken);

        if(giftCode is null) {
            return Results.NotFound("Gift code not found.");
        }

        if(giftCode.IsExpired) {
            return Results.Failure("Gift code is expired.");
        }

        var players = await _context
            .Players
            .Where(x => x.IsInAlliance || x.ForceRedeemGiftCodes)
            .OrderByDescending(x => x.ExternalId)
            .ToListAsync(cancellationToken);

        foreach(var player in players) {
            await _giftCodeRedemptionQueue.AddToQueueAsync(player, giftCode, cancellationToken);
        }

        return Results.Success();
    }

    public async Task<Result> RedeemGiftCodeAsync(int whiteoutSurvivalPlayerId, string giftCode, CancellationToken cancellationToken) {
        var player = await _context.Players.FirstOrDefaultAsync(x => x.ExternalId == whiteoutSurvivalPlayerId, cancellationToken);

        if(player is null) {
            return Results.NotFound("Player not found.");
        }

        var giftCodeEntity = await _context.GiftCodes.FirstOrDefaultAsync(x => x.Code == giftCode, cancellationToken);

        if(giftCodeEntity is null) {
            return Results.NotFound("Gift code not found.");
        }

        var existingRedemption = await _context.GiftCodeRedemptions.FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.GiftCodeId == giftCodeEntity.Id, cancellationToken);

        if(existingRedemption is not null) {
            return Results.Conflict("Gift code already redeemed.");
        }

        var playerResult = await _whiteoutSurvivalHttpClient.GetPlayerInfoAsync(player.ExternalId, cancellationToken); // We need to "sign in" the player
        var redeemResult = await _whiteoutSurvivalHttpClient.RedeemGiftCodeAsync(player.ExternalId, giftCode, cancellationToken);

        switch(redeemResult.ErrorCode) {
            case 20000: // Code success
            case 40008: // Code already used
                break;
            case 40014:
                return Results.Failure("Gift code not found.");
            case 40004:
                return Results.Failure("Player not found.");
            case 40007:
                return Results.Failure("Gift code expired.");
            case 40009:
                return Results.Failure("Player not logged in.");
            default:
                return Results.Failure($"Unknown error code: {redeemResult.ErrorCode}, message: {redeemResult.Message}");
        }

        var newRedemption = new GiftCodeRedemption {
            Player = player,
            PlayerId = player.Id,
            GiftCode = giftCodeEntity,
            GiftCodeId = giftCodeEntity.Id,
            RedeemedAt = _timeProvider.GetUtcNow(),
        };

        await _context.GiftCodeRedemptions.AddAsync(newRedemption, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success();
    }

    public async Task<ICollection<GiftCodeRedemption>> GetRedemptionsForPlayerAsync(Guid playerId, CancellationToken cancellationToken) {
        return await _context
            .GiftCodeRedemptions
            .Include(x => x.Player)
            .Include(x => x.GiftCode)
            .AsNoTracking()
            .Where(x => x.PlayerId == playerId)
            .OrderByDescending(x => x.RedeemedAt)
            .ToListAsync(cancellationToken);
    }
}
