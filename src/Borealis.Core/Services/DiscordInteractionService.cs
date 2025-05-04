using Discord.Interactions;

namespace Borealis.Core.Services;

public class DiscordInteractionService : InteractionModuleBase {
    [SlashCommand("gift-code", "Echo an input")]
    public async Task EchoAsync(string input) {
        await RespondAsync(input);
    }

    [Group("gift-code", "Gift code commands")]
    public class DiscordGiftCodeInteractionService : InteractionModuleBase {
        [SlashCommand("add", "Try to add a gift code")]
        public async Task EchoAsync(string input) {
            await RespondAsync(input);
        }
    }
}
