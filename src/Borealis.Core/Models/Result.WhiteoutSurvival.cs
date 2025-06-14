using System.Diagnostics;

namespace Borealis.Core.Models;

public record WhiteoutSurvivalMessage : ResultMessage {
    [DebuggerStepThrough]
    public WhiteoutSurvivalMessage(string key, string format, params object[]? parameters) : base(key, format, parameters) { }
}

public record GiftCodeNotFoundMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public GiftCodeNotFoundMessage(string code) : base("GiftCodeNotFound", "The gift code \"{0}\" could not be found.", code) { }
}

public record GiftCodeExpiredMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public GiftCodeExpiredMessage(string code) : base("GiftCodeExpired", "The gift code \"{0}\" has expired.", code) { }
}

public record GiftCodeAlreadyRedeemedMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public GiftCodeAlreadyRedeemedMessage(string code, int playerId, string playerName) : base("GiftCodeAlreadyRedeemed", "The gift code \"{0\" has already been redeemed for player {1} {2}.", code, playerId, playerName) { }
}

public record GiftCodeCaptchaFailedMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public GiftCodeCaptchaFailedMessage(string code) : base("GiftCodeCaptchaFailed", "The gift code \"{0}\" could not be redeemed due to a captcha failure.", code) { }
}

public record GiftCodeRedeemedMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public GiftCodeRedeemedMessage(string code) : base("GiftCodeRedeemed", "The gift code \"{0}\" has been successfully redeemed.", code) { }
}

public record GiftCodeRedeemLimitReachedMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public GiftCodeRedeemLimitReachedMessage(string code) : base("GiftCodeRedeemLimitReached", "The gift code \"{0}\" has reached its redeem limit.", code) { }
}

public record GiftCodePlayerNotFoundMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public GiftCodePlayerNotFoundMessage(int playerId) : base("GiftCodePlayerNotFound", "The player with ID \"{0}\" could not be found.", playerId) { }
}

public record GiftCodePlayerErrorMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public GiftCodePlayerErrorMessage(int playerId, int errorCode, string error) : base("GiftCodePlayerError", "An error occurred for player with ID {0}: {1} - {2}", playerId, errorCode, error) { }
}

public record GiftCodePlayerNotLoggedInMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public GiftCodePlayerNotLoggedInMessage(int playerId) : base("GiftCodePlayerNotLoggedIn", "The player with ID {0} is not logged in.", playerId) { }
}

public record GiftCodeSignErrorMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public GiftCodeSignErrorMessage() : base("GiftCodeSignError", "There was a sign error while redeeming the gift code. This usually means the gift code is invalid.") { }
}

public record FailedToGetCaptchaMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public FailedToGetCaptchaMessage() : base("FailedToGetCaptcha", "Failed to retrieve the captcha image for gift code redemption.") { }
}

public record FailedToSolveCaptchaMessage : WhiteoutSurvivalMessage {
    [DebuggerStepThrough]
    public FailedToSolveCaptchaMessage() : base("FailedToSolveCaptcha", "Failed to solve the captcha for gift code redemption.") { }
}
