using Borealis.Core.Contracts;
using Borealis.Core.HttpClients.Models;
using Borealis.Core.Models;
using Borealis.Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Core.Services;

public class PlayerService : QueryServiceBase<WhiteoutSurvivalPlayer>, IPlayerService {
    private readonly BorealisContext _context;
    private readonly IWhiteoutSurvivalHttpClient _whiteoutSurvivalHttpClient;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<PlayerService> _logger;

    protected override string DefaultSortProperty => nameof(WhiteoutSurvivalPlayer.FurnaceLevel);
    protected override bool DefaultSortAscending => false;

    public PlayerService(
            BorealisContext context,
            IWhiteoutSurvivalHttpClient whiteoutSurvivalHttpClient,
            TimeProvider timeProvider,
            ILogger<PlayerService> logger) {
        _context = context;
        _whiteoutSurvivalHttpClient = whiteoutSurvivalHttpClient;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<WhiteoutSurvivalPlayer>> GetByIdAsync(Guid playerId, CancellationToken cancellationToken) {
        var player = await _context.Players.FirstOrDefaultAsync(x => x.Id == playerId, cancellationToken);

        if(player is null) {
            return Results.NotFound<WhiteoutSurvivalPlayer>();
        }

        return Results.Success(player);
    }

    public async Task<Result<WhiteoutSurvivalPlayer>> GetByExternalIdAsync(int whiteoutSurvivalPlayerId, CancellationToken cancellationToken) {
        var player = await _context.Players.FirstOrDefaultAsync(x => x.ExternalId == whiteoutSurvivalPlayerId, cancellationToken);

        if(player is null) {
            return Results.NotFound<WhiteoutSurvivalPlayer>();
        }

        return Results.Success(player);
    }

    public async Task<PagedResult<WhiteoutSurvivalPlayer>> GetPagedAsync(PlayerQuery query, CancellationToken cancellationToken) {
        var players = await BuildQuery(_context.Players, query)
            .ToListAsync(cancellationToken);

        var totalPlayers = await BuildQuery(_context.Players, query).CountAsync(cancellationToken);

        return Results.PagedSuccess(players, query, totalPlayers);
    }

    private IQueryable<WhiteoutSurvivalPlayer> BuildQuery(IQueryable<WhiteoutSurvivalPlayer> dbQuery, PlayerQuery query) {
        if(!string.IsNullOrWhiteSpace(query.Query)) {
            dbQuery = dbQuery.Where(x => x.Name.Contains(query.Query) || (x.Notes != null && x.Notes.Contains(query.Query)));
        }

        if(!query.ShowAll) {
            dbQuery = dbQuery.Where(x => x.IsInAlliance);
        }

        return base.AddBaseQuery(dbQuery, query);
    }

    public async Task<Result<WhiteoutSurvivalPlayer>> SynchronizePlayerAsync(int whiteoutSurvivalPlayerId, CancellationToken cancellationToken) {
        WhiteoutSurvivalResponseWrapper<WhiteoutSurvivalPlayerResponse> response;

        try {
            response = await _whiteoutSurvivalHttpClient.GetPlayerInfoAsync(whiteoutSurvivalPlayerId, cancellationToken);
        } catch(Exception ex) {
            _logger.LogError(ex, "Failed to synchronize player {PlayerId}", whiteoutSurvivalPlayerId);
            return Results.Failure<WhiteoutSurvivalPlayer>("Failed to synchronize player.");
        }

        if(response.Code != 0 || response.Data is null) {
            return Results.NotFound<WhiteoutSurvivalPlayer>();
        }

        var externalPlayer = response.Data;

        var existingPlayer = await _context.Players.FirstOrDefaultAsync(x => x.ExternalId == whiteoutSurvivalPlayerId, cancellationToken);
        if(existingPlayer is null) {
            var now = _timeProvider.GetUtcNow();
            existingPlayer = new WhiteoutSurvivalPlayer {
                ExternalId = externalPlayer.FurnaceId,
                Name = externalPlayer.Name,
                FurnaceLevel = externalPlayer.FurnaceLevel,
                IsInAlliance = false,
                State = externalPlayer.State,
                UpdatedAt = now,
                CreatedAt = now
            };
            _context.Add(existingPlayer);
        } else {
            var now = _timeProvider.GetUtcNow();
            if(existingPlayer.Name != externalPlayer.Name) {
                existingPlayer.PreviousNames.Add(new WhiteoutSurvivalPlayerNameHistoryEntry {
                    Name = existingPlayer.Name,
                    Timestamp = now
                });
            }

            existingPlayer.Name = externalPlayer.Name;
            existingPlayer.FurnaceLevel = externalPlayer.FurnaceLevel;
            existingPlayer.State = externalPlayer.State;

            if(_context.Entry(existingPlayer).State == EntityState.Modified) {
                existingPlayer.UpdatedAt = now;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success(existingPlayer);
    }

    public async Task<Result<WhiteoutSurvivalPlayer>> SetPlayerNotesAsync(Guid playerId, string notes, CancellationToken cancellationToken) {
        var player = await _context.Players.FirstOrDefaultAsync(x => x.Id == playerId, cancellationToken);

        if(player is null) {
            return Results.NotFound<WhiteoutSurvivalPlayer>();
        }

        player.Notes = notes;

        if(_context.Entry(player).State == EntityState.Modified) {
            player.UpdatedAt = _timeProvider.GetUtcNow();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Results.Success(player);
    }
}
