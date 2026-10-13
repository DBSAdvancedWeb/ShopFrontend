using System.Net;

namespace ShopWeb.Services;

// Thrown when the Shop API can't be reached or returns an unexpected status.
public class ShopApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public ShopApiException(string message, HttpStatusCode? statusCode = null, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
    }
}
