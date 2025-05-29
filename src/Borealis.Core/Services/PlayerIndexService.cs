using System.Globalization;
using System.IO.Abstractions;
using Borealis.Core.Lucene;
using Borealis.Core.Models;
using Borealis.Core.Requests;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Borealis.Core.Services;

public class PlayerIndexService : LuceneIndexServiceBase<Player, PlayerQuery> {
    private readonly BorealisContext _context;

    protected override Analyzer DefaultAnalyzer => new BorealisAnalyzer();

    public PlayerIndexService(
        BorealisContext context,
        IFileSystem fileSystem,
        TimeProvider timeProvider,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<PlayerIndexService> logger)
         : base(fileSystem, timeProvider, hostApplicationLifetime, logger) {
        _context = context;
    }

    protected override Document GetIndexDocument(Player item) {
        var doc = new Document {
            new StringField("id", item.Id.ToString(), Field.Store.YES),
            new StringField("externalId", item.ExternalId.ToString(CultureInfo.InvariantCulture), Field.Store.YES),
            new TextField("name", item.Name, Field.Store.YES),
            new StringField("isInAlliance", item.IsInAlliance.ToString(), Field.Store.NO),
            new StringField("createdAt", item.CreatedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture), Field.Store.NO),
            new StringField("updatedAt", item.UpdatedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture), Field.Store.NO),
        };

        foreach(var previousName in item.PreviousNames) {
            doc.Add(new TextField("previousName", previousName.Name, Field.Store.YES));
        }

        return doc;
    }

    protected override Term GetIdentifierTerm(Player item) {
        return new Term("id", item.Id.ToString());
    }

    protected override Query GetQuery(PlayerQuery query) {
        var queryParser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, ["name", "previousName", "externalId"], DefaultAnalyzer);
        var contentQuery = string.IsNullOrWhiteSpace(query.Query) ? new MatchAllDocsQuery() : queryParser.Parse(query.Query);

        var booleanQuery = new BooleanQuery {
            { contentQuery, Occur.MUST },
        };

        if(!query.ShowAll) {
            booleanQuery.Add(new TermQuery(new Term("isInAlliance", "false")), Occur.MUST);
        }

        return booleanQuery;
    }

    public async Task<PagedResult<Player>> SearchAsync(PlayerQuery query, CancellationToken cancellationToken) {
        var identifiersResult = GetIdentifiers(query);

        if(identifiersResult.TotalCount == 0) {
            return new PagedResult<Player> {
                Items = [],
                TotalCount = 0,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };
        }

        var identifiers = identifiersResult.Items.ToList();

        var players = await _context
            .Players
            .Where(x => identifiers.Contains(x.Id))
            .ToListAsync(cancellationToken);

        return new PagedResult<Player> {
            Items = [.. players.OrderByDescending(x => identifiers.IndexOf(x.Id))],
            TotalCount = identifiersResult.TotalCount,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }
}
