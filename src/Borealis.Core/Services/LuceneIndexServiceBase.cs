using System.IO.Abstractions;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Hosting;

namespace Borealis.Core.Services;

public abstract class LuceneIndexServiceBase<T> : IDisposable where T : class {
    private const LuceneVersion _luceneVersion = LuceneVersion.LUCENE_48;
    private readonly FSDirectory _indexDirectory;
    private readonly IndexWriter _indexWriter;
    private readonly CancellationTokenRegistration _shutdownRegistration;
    private readonly Task _commitTask;
    private readonly CancellationTokenSource _commitCancellationTokenSource;

    private readonly IFileSystem _fileSystem;
    private readonly TimeProvider _timeProvider;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger _logger;

    public abstract Analyzer DefaultAnalyzer { get; }

    protected LuceneIndexServiceBase(IFileSystem fileSystem, TimeProvider timeProvider, IHostApplicationLifetime hostApplicationLifetime, ILogger logger) {
        _fileSystem = fileSystem;
        _timeProvider = timeProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;

        var indexName = typeof(T).Name;
        var indexPath = fileSystem.Path.Combine(Environment.CurrentDirectory, indexName);

        _indexDirectory = FSDirectory.Open(indexPath);
        _indexWriter = new IndexWriter(_indexDirectory, new IndexWriterConfig(_luceneVersion, DefaultAnalyzer));

        _shutdownRegistration = _hostApplicationLifetime.ApplicationStopping.Register(OnApplicationStopping);

        _commitCancellationTokenSource = new CancellationTokenSource();
        _commitTask = Task.Run(async () => await CommitLoopAsync(_commitCancellationTokenSource.Token), _commitCancellationTokenSource.Token);
    }

    private async Task CommitLoopAsync(CancellationToken cancellationToken) {
        while(!cancellationToken.IsCancellationRequested) {
            if(_commitAt == 0) {
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            var now = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
            if(now >= _commitAt || now >= _commitAtAbsolute) {
                _indexWriter.Commit();

                _commitAt = 0;
                _commitAtAbsolute = 0;
            }

            // Task.Delay until we reach _commitAt or _commitAtAbsolute
            var delay = Math.Min(_commitAt - now, _commitAtAbsolute - now);
            if(delay > 0) {
                await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
            }
        }
    }

    private void OnApplicationStopping() {
        ShutdownIndexWriter();
    }

    public void Dispose() {
        GC.SuppressFinalize(this);

        ShutdownIndexWriter();

        _shutdownRegistration.Dispose();
        _commitCancellationTokenSource.Cancel();
        _commitTask.Wait();
    }

    private void ShutdownIndexWriter() {
        _logger.LogInformation("Closing Lucene index writer.");
        try {
            _indexWriter.Dispose();
        } finally {
            if(IndexWriter.IsLocked(_indexDirectory)) {
                IndexWriter.Unlock(_indexDirectory);
            }
        }

        _indexDirectory.Dispose();
    }

    protected abstract Document GetIndexDocument(T item);

    private readonly System.Threading.Lock _lockObj = new();
    private readonly TimeSpan _commitDelay = TimeSpan.FromSeconds(2);
    private readonly TimeSpan _commitAbsoluteDelay = TimeSpan.FromSeconds(10);
    private long _commitAt;
    private long _commitAtAbsolute;

    private void CommitDelayed() {
        var startTime = _timeProvider.GetUtcNow();
        var commitBy = startTime.Add(_commitDelay).ToUnixTimeMilliseconds();

        using(_lockObj.EnterScope()) {
            if(_commitAt >= commitBy) {
                return;
            }

            _commitAt = commitBy;
            if(_commitAtAbsolute == 0) {
                _commitAtAbsolute = startTime.Add(_commitAbsoluteDelay).ToUnixTimeMilliseconds();
            }
        }
    }

    public void Index(T item) {
        var doc = GetIndexDocument(item);
        _indexWriter.AddDocument(doc);

        CommitDelayed();
    }

    public T? Read(T item) {
        using var reader = _indexWriter.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        var identifierTerm = GetIdentifierTerm(item);
        var query = new TermQuery(identifierTerm);

        var topDocs = searcher.Search(query, 1);
        if(topDocs.TotalHits == 0) {
            return null;
        }

        var document = searcher.Doc(topDocs.ScoreDocs[0].Doc);
        return null;
    }

    protected abstract Term GetIdentifierTerm(T item);

    public void Delete(T item) {
        var identifierTerm = GetIdentifierTerm(item);
        _indexWriter.DeleteDocuments(identifierTerm);

        CommitDelayed();
    }
}
