using Borealis.WhiteoutSurvivalHttpClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Borealis.WhiteoutSurvivalProxy;

public class WhiteoutsurvivalProxyTrigger {
    private readonly IWhiteoutSurvivalHttpClient _whiteoutSurvivalHttpClient;
    private readonly ILogger<WhiteoutsurvivalProxyTrigger> _logger;

    public WhiteoutsurvivalProxyTrigger(IWhiteoutSurvivalHttpClient whiteoutSurvivalHttpClient, ILogger<WhiteoutsurvivalProxyTrigger> logger) {
        _whiteoutSurvivalHttpClient = whiteoutSurvivalHttpClient;
        _logger = logger;
    }

    [Function("GetPlayerInfo")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request) {
        var playerIdValues = request.Query["playerId"];

        if(!int.TryParse(playerIdValues.FirstOrDefault(), out var playerId)) {
            return new BadRequestObjectResult("Invalid player ID: " + playerIdValues.FirstOrDefault());
        }

        var playerResponse = await _whiteoutSurvivalHttpClient.GetPlayerInfoAsync(playerId, request.HttpContext.RequestAborted);

        return new OkObjectResult(playerResponse);
    }
}
