using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.Core.Requests;
using Borealis.WhiteoutSurvivalHttpClient;
using Borealis.WhiteoutSurvivalHttpClient.Models;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Core.Services;

public class PlayerService : QueryServiceBase<Player>, IPlayerService {
    private readonly BorealisContext _context;
    private readonly IWhiteoutSurvivalHttpClient _whiteoutSurvivalHttpClient;
    private readonly IDiscordBotService _discordBotService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<PlayerService> _logger;

    protected override string DefaultSortProperty => nameof(Player.FurnaceLevel);
    protected override bool DefaultSortAscending => false;

    public PlayerService(
            BorealisContext context,
            IWhiteoutSurvivalHttpClient whiteoutSurvivalHttpClient,
            IDiscordBotService discordBotService,
            TimeProvider timeProvider,
            ILogger<PlayerService> logger) {
        _context = context;
        _whiteoutSurvivalHttpClient = whiteoutSurvivalHttpClient;
        _discordBotService = discordBotService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<Player>> GetByIdAsync(Guid playerId, CancellationToken cancellationToken) {
        var entity = await _context
            .Players
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == playerId, cancellationToken);

        if(entity is null) {
            return Results.NotFound<Player>();
        }

        return Results.Success(entity);
    }

    public async Task<Result<Player>> GetByExternalIdAsync(int whiteoutSurvivalPlayerId, CancellationToken cancellationToken) {
        var entity = await _context.Players.FirstOrDefaultAsync(x => x.ExternalId == whiteoutSurvivalPlayerId, cancellationToken);

        if(entity is null) {
            return Results.NotFound<Player>();
        }

        return Results.Success(entity);
    }

    public async Task<PagedResult<Player>> GetPagedAsync(PlayerQuery query, CancellationToken cancellationToken) {
        var entities = await BuildQuery(_context.Players.AsNoTracking(), query)
            .Skip(query.PageIndex * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await BuildQuery(_context.Players, query).CountAsync(cancellationToken);

        return Results.PagedSuccess(entities, query, totalCount);
    }

    private IQueryable<Player> BuildQuery(IQueryable<Player> dbQuery, PlayerQuery query) {
        if(!string.IsNullOrWhiteSpace(query.Query)) {
#pragma warning disable CA1307 // Specify StringComparison for clarity
            dbQuery = dbQuery.Where(x => EF.Functions.Like(x.Name, $"%{query.Query}%") || x.PreviousNames.Any(x => EF.Functions.Like(x.Name, $"%{query.Query}%")) || (x.Notes != null && EF.Functions.Like(x.Notes, $"%{query.Query}%")));
#pragma warning restore CA1307 // Specify StringComparison for clarity
        }

        if(!query.ShowAll) {
            dbQuery = dbQuery.Where(x => x.IsInAlliance);
        }

        return base.AddSorting(dbQuery, query);
    }

    public async Task<Result<Player>> SynchronizePlayerAsync(int whiteoutSurvivalPlayerId, bool addAsInAlliance, CancellationToken cancellationToken) {
        WhiteoutSurvivalResponseWrapper<WhiteoutSurvivalPlayerResponse> response;

        try {
            response = await _whiteoutSurvivalHttpClient.GetPlayerInfoAsync(whiteoutSurvivalPlayerId, cancellationToken);
        } catch(Exception ex) {
            _logger.LogError(ex, "Failed to synchronize player {PlayerId}", whiteoutSurvivalPlayerId);
            return Results.Failure<Player>(new FailedToSynchronizePlayerMessage(whiteoutSurvivalPlayerId));
        }

        if(response.Code != 0 || response.Data is null) {
            return Results.NotFound<Player>();
        }

        var externalPlayer = response.Data;

        var existingPlayer = await _context.Players.FirstOrDefaultAsync(x => x.ExternalId == whiteoutSurvivalPlayerId, cancellationToken);
        if(existingPlayer is null) {
            var now = _timeProvider.GetUtcNow();
            existingPlayer = new Player {
                ExternalId = externalPlayer.FurnaceId,
                Name = externalPlayer.Name,
                FurnaceLevel = externalPlayer.FurnaceLevel,
                IsInAlliance = addAsInAlliance,
                IsMuted = !addAsInAlliance,
                State = externalPlayer.State,
                UpdatedAt = now,
                CreatedAt = now
            };
            _context.Add(existingPlayer);
        } else {
            var now = _timeProvider.GetUtcNow();
            if(existingPlayer.Name != externalPlayer.Name) {
                if(!existingPlayer.IsMuted) {
                    await _discordBotService.SendPlayerChangedNameMessageAsync(existingPlayer, externalPlayer.Name, existingPlayer.Name, cancellationToken);
                }

                existingPlayer.PreviousNames.Add(new PlayerNameHistoryEntry {
                    Name = existingPlayer.Name,
                    Timestamp = now
                });
            }

            if(existingPlayer.State != externalPlayer.State) {
                await _discordBotService.SendPlayerChangedStateMessageAsync(existingPlayer, externalPlayer.State, existingPlayer.State, cancellationToken);

                existingPlayer.PreviousStates.Add(new PlayerStateHistoryEntry {
                    State = existingPlayer.State,
                    Timestamp = now
                });
            }

            var hasUpdatedFurnaceLevel = false;
            if(existingPlayer.FurnaceLevel != externalPlayer.FurnaceLevel) {
                hasUpdatedFurnaceLevel = true;
            }

            existingPlayer.Name = externalPlayer.Name;
            existingPlayer.FurnaceLevel = externalPlayer.FurnaceLevel;
            existingPlayer.State = externalPlayer.State;

            if(hasUpdatedFurnaceLevel) {
                await _discordBotService.SendPlayerChangedFurnaceLevelMessageAsync(existingPlayer, existingPlayer.ExactFurnaceLevelString, cancellationToken);
            }

            if(_context.Entry(existingPlayer).State == EntityState.Modified) {
                existingPlayer.UpdatedAt = now;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success(existingPlayer);
    }

    public async Task<Result<Player>> UpdateAsync(Player player, CancellationToken cancellationToken) {
        var existingPlayer = await _context.Players.FirstOrDefaultAsync(x => x.Id == player.Id, cancellationToken);

        if(existingPlayer is null) {
            return Results.NotFound<Player>();
        }

        existingPlayer.Notes = player.Notes?.Trim();
        existingPlayer.IsInAlliance = player.IsInAlliance;
        existingPlayer.ForceRedeemGiftCodes = player.ForceRedeemGiftCodes;
        existingPlayer.IsMuted = player.IsMuted;
        existingPlayer.AwayUntil = player.AwayUntil;

        if(_context.Entry(existingPlayer).State == EntityState.Modified) {
            existingPlayer.UpdatedAt = _timeProvider.GetUtcNow();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success(existingPlayer);
    }

    public async Task<Result> DeleteAsync(Guid playerId, CancellationToken cancellationToken) {
        var player = await _context.Players.FirstOrDefaultAsync(x => x.Id == playerId, cancellationToken);

        if(player is null) {
            return Results.NotFound();
        }

        _context.Players.Remove(player);
        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success();
    }
}
