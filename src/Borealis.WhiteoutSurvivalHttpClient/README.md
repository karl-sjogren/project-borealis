# Whiteout Survival Gift Code API HttpClient

This is a simple HttpClient for the Whiteout Survival Gift Code API. It is
used to redeem gift codes for a player in the game Whiteout Survival. It also
allowes for retrieving some basic info about a player such as name, furnace
level and the url for their avatar.

## Usage

Easiest is to use the client via dependency inejction. The client will be setup
with the needed options and with a retry when the API returns a 429 status code.

```csharp
builder.Services.AddWhiteoutSurvivalHttpClient();
```

Then you can inject the client into your service and use it to redeem gift codes
for a player by their player ID.

```csharp
public class GiftCodeService
{
    private readonly IWhiteoutSurvivalHttpClient _client;

    public GiftCodeService(IWhiteoutSurvivalHttpClient client)
    {
        _client = client;
    }

    public async Task RedeemGiftCodeAsync(string playerId, string giftCode, CancellationToken cancellationToken)
    {
         // We need to "sign in" the player before we can redeem a gift code
        var playerResult = await _client.GetPlayerInfoAsync(playerId, cancellationToken);
        var redeemResult = await _client.RedeemGiftCodeAsync(playerId, giftCode, cancellationToken);
    }
}
```
