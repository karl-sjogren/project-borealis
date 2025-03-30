namespace Borealis.Core.Contracts;

public interface IGiftCodeScanner {
    string Name { get; }
    Task<ICollection<string>> ScanGiftCodesAsync(CancellationToken cancellationToken);
}
