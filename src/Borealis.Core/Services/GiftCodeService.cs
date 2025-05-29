using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Core.Services;

public class GiftCodeService : QueryServiceBase<GiftCode>, IGiftCodeService {
    private readonly BorealisContext _context;
    private readonly IWhiteoutSurvivalService _whiteoutSurvivalService;
    private readonly IGiftCodeRedemptionQueue _giftCodeRedemptionQueue;
    private readonly IDiscordBotService _discordBotService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GiftCodeService> _logger;

    protected override string DefaultSortProperty => nameof(GiftCode.IsExpired);
    protected override bool DefaultSortAscending => true;

    public GiftCodeService(
            BorealisContext context,
            IWhiteoutSurvivalService whiteoutSurvivalService,
            IGiftCodeRedemptionQueue giftCodeRedemptionQueue,
            IDiscordBotService discordBotService,
            TimeProvider timeProvider,
            ILogger<GiftCodeService> logger) {
        _context = context;
        _whiteoutSurvivalService = whiteoutSurvivalService;
        _giftCodeRedemptionQueue = giftCodeRedemptionQueue;
        _discordBotService = discordBotService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<GiftCode>> GetByIdAsync(Guid giftCodeId, CancellationToken cancellationToken) {
        var entity = await _context
            .GiftCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == giftCodeId, cancellationToken);

        if(entity is null) {
            return Results.Failure<GiftCode>(new NotFoundMessage(giftCodeId.ToString(), "Gift code not found."));
        }

        return Results.Success(entity);
    }

    public async Task<Result> GiftCodeExistsAsync(string giftCode, CancellationToken cancellationToken) {
        var entity = await _context
            .GiftCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == giftCode, cancellationToken);

        if(entity is null) {
            return Results.Failure(new NotFoundMessage(giftCode));
        }

        // Gift codes are unfortunately case-sensitive
        if(entity.Code.Equals(giftCode, StringComparison.Ordinal)) {
            return Results.Success();
        } else {
            return Results.Failure(new NotFoundMessage(giftCode, "The gift code exists but the case does not match."));
        }
    }

    public async Task<Result> AddGiftCodeAsync(string giftCode, string source, CancellationToken cancellationToken) {
        var existsResult = await GiftCodeExistsAsync(giftCode, cancellationToken);

        if(existsResult.Success) {
            return Results.Failure(new ConflictMessage($"Gift code '{giftCode}' already exists."));
        }

        var player = await _context
            .Players
            .Where(x => x.IsInAlliance || x.ForceRedeemGiftCodes)
            .OrderBy(_ => EF.Functions.Random())
            .FirstOrDefaultAsync(cancellationToken);

        var isExpired = false;
        if(player is not null) {
            var redeemResult = await _whiteoutSurvivalService.RedeemGiftCodeAsync(player.ExternalId, giftCode, cancellationToken);

            isExpired = redeemResult.Message.Key is "GiftCodeExpired";
            if(!redeemResult.Success && !isExpired) {
                return redeemResult;
            }
        }

        var now = _timeProvider.GetUtcNow();
        var newGiftCode = new GiftCode {
            Code = giftCode,
            CreatedAt = now,
            UpdatedAt = now,
            IsExpired = isExpired,
            Source = source
        };

        await _context.GiftCodes.AddAsync(newGiftCode, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        if(newGiftCode.IsExpired) {
            return Results.Failure(new GiftCodeExpiredMessage(giftCode));
        }

        await _discordBotService.SendGiftCodeAddedMessageAsync(newGiftCode, cancellationToken);

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
            dbQuery = dbQuery.Where(x => EF.Functions.Like(x.Code, $"%{query.Query}%"));
        }

        if(query.IsExpired.HasValue) {
            dbQuery = dbQuery.Where(x => x.IsExpired == query.IsExpired.Value);
        }

        // If we requested a specific sort, use that
        if(!string.IsNullOrWhiteSpace(query.SortField)) {
            return base.AddSorting(dbQuery, query);
        }

        return dbQuery.OrderBy(x => x.IsExpired).ThenByDescending(x => x.CreatedAt);
    }

    public async Task<Result> EnqueueGiftCodeAsync(Guid giftCodeId, CancellationToken cancellationToken) {
        var giftCode = await _context.GiftCodes.FirstOrDefaultAsync(x => x.Id == giftCodeId, cancellationToken);

        if(giftCode is null) {
            return Results.Failure(new NotFoundMessage(giftCodeId.ToString(), "Gift code not found."));
        }

        if(giftCode.IsExpired) {
            return Results.Failure(new GiftCodeExpiredMessage(giftCode.Code));
        }

        var players = await _context
            .Players
            .Where(x => x.IsInAlliance || x.ForceRedeemGiftCodes)
            .OrderByDescending(x => x.FurnaceLevel)
            .ToListAsync(cancellationToken);

        foreach(var player in players) {
            await _giftCodeRedemptionQueue.AddToQueueAsync(player, giftCode, cancellationToken);
        }

        return Results.Success();
    }

    public async Task<Result> RedeemGiftCodeAsync(int whiteoutSurvivalPlayerId, string giftCode, CancellationToken cancellationToken) {
        var player = await _context.Players.FirstOrDefaultAsync(x => x.ExternalId == whiteoutSurvivalPlayerId, cancellationToken);

        if(player is null) {
            return Results.Failure(new NotFoundMessage(whiteoutSurvivalPlayerId.ToString(), "Player not found."));
        }

        var giftCodeEntity = await _context
            .GiftCodes
            .FirstOrDefaultAsync(x => x.Code == giftCode, cancellationToken);

        if(giftCodeEntity is null) {
            return Results.Failure(new GiftCodeNotFoundMessage(giftCode));
        }

        if(giftCodeEntity.IsExpired) {
            return Results.Failure(new GiftCodeExpiredMessage(giftCode));
        }

        var existingRedemption = await _context
            .GiftCodeRedemptions
            .FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.GiftCodeId == giftCodeEntity.Id, cancellationToken);

        if(existingRedemption is not null) {
            return Results.Failure(new GiftCodeAlreadyRedeemedMessage(giftCode, player.ExternalId, player.Name));
        }

        var redeemResult = await _whiteoutSurvivalService.RedeemGiftCodeAsync(player.ExternalId, giftCode, cancellationToken);

        if(!redeemResult.Success) {
            return redeemResult;
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

    public async Task<IReadOnlyCollection<GiftCodeRedemption>> GetRedemptionsForGiftCodeAsync(Guid giftCodeId, CancellationToken cancellationToken) {
        return await _context
            .GiftCodeRedemptions
            .Include(x => x.Player)
            .Include(x => x.GiftCode)
            .AsNoTracking()
            .Where(x => x.GiftCodeId == giftCodeId)
            .OrderByDescending(x => x.RedeemedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<GiftCodeRedemption>> GetRedemptionsForPlayerAsync(Guid playerId, CancellationToken cancellationToken) {
        return await _context
            .GiftCodeRedemptions
            .Include(x => x.Player)
            .Include(x => x.GiftCode)
            .AsNoTracking()
            .Where(x => x.PlayerId == playerId)
            .OrderByDescending(x => x.RedeemedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<GiftCode>> UpdateAsync(GiftCode giftCode, CancellationToken cancellationToken) {
        var existingGiftCode = await _context.GiftCodes.FirstOrDefaultAsync(x => x.Id == giftCode.Id, cancellationToken);

        if(existingGiftCode is null) {
            return Results.Failure<GiftCode>(new NotFoundMessage(giftCode.Id, "Gift code not found."));
        }

        existingGiftCode.IsExpired = giftCode.IsExpired;

        if(_context.Entry(existingGiftCode).State == EntityState.Modified) {
            existingGiftCode.UpdatedAt = _timeProvider.GetUtcNow();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success(existingGiftCode);
    }

    public async Task<Result> DeleteAsync(Guid giftCodeId, CancellationToken cancellationToken) {
        var giftCode = await _context.GiftCodes.FirstOrDefaultAsync(x => x.Id == giftCodeId, cancellationToken);

        if(giftCode is null) {
            return Results.Failure<GiftCode>(new NotFoundMessage(giftCodeId, "Gift code not found."));
        }

        _context.GiftCodes.Remove(giftCode);
        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success();
    }
}
