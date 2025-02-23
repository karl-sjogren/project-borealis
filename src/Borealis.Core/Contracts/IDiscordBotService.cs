namespace Borealis.Core.Contracts;

public interface IDiscordBotService {
    Task SendMessageAsync(string message, CancellationToken cancellationToken);
}
