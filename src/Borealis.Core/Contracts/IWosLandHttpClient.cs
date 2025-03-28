namespace Borealis.Core.Contracts;

public interface IWosLandHttpClient {
    Task<ICollection<string>> GetGiftCodesAsync(CancellationToken cancellationToken);
}
