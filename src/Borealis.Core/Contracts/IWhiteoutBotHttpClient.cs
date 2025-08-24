namespace Borealis.Core.Contracts;

public interface IWhiteoutBotHttpClient {
    Task<ICollection<string>> GetGiftCodesAsync(CancellationToken cancellationToken);
}
