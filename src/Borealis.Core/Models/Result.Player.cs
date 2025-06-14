using System.Diagnostics;

namespace Borealis.Core.Models;

public record FailedToSynchronizePlayerMessage : ResultMessage {
    [DebuggerStepThrough]
    public FailedToSynchronizePlayerMessage(int playerId)
        : base("FailedToSynchronizePlayer", "Failed to synchronize player with ID {0}", playerId) {
    }
}
