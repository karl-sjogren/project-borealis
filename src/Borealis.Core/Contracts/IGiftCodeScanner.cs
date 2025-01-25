namespace Borealis.Core.Contracts;

public interface IGiftCodeScanner {
    Task<ICollection<string>> ScanGiftCodesAsync(CancellationToken cancellationToken);
}
