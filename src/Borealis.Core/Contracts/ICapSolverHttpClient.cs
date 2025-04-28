using Borealis.Core.HttpClients.DataTransferObjects;

namespace Borealis.Core.Contracts;

public interface ICapSolverHttpClient {
    Task<CapSolverImageToTextResponse?> ImageToTextAsync(byte[] buffer, CancellationToken cancellationToken);
}
