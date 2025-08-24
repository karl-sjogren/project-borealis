using System.Net;

namespace Borealis.WhiteoutSurvivalHttpClient.Exceptions;

[ExcludeFromCodeCoverage]
public class HttpForbiddenException : HttpExceptionBase {
    public HttpForbiddenException() : base(HttpStatusCode.Forbidden, null) {
    }

    public HttpForbiddenException(string? responseMessage) : base(HttpStatusCode.Forbidden, responseMessage) {
    }

    public HttpForbiddenException(string? responseMessage, string message) : base(HttpStatusCode.Forbidden, responseMessage, message) {
    }

    public HttpForbiddenException(string? responseMessage, string message, Exception innerException) : base(HttpStatusCode.Forbidden, responseMessage, message, innerException) {
    }
}
