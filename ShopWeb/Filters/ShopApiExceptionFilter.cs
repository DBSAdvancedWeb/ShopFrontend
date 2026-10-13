using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ShopWeb.Services;

namespace ShopWeb.Filters;

// Turns a ShopApiException thrown by any controller action into a friendly error page
// instead of an unhandled 500. ShopApiClient wraps HttpRequestException and the Polly
// timeout/circuit-breaker exceptions in ShopApiException, so API outages land here too.
public class ShopApiExceptionFilter(
    ILogger<ShopApiExceptionFilter> logger,
    IModelMetadataProvider metadataProvider) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ShopApiException ex)
        {
            return; // not an API problem: let normal error handling run
        }

        logger.LogError(ex, "Shop API call failed for {Path}", context.HttpContext.Request.Path);

        var statusCode = ex.StatusCode switch
        {
            HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
            HttpStatusCode.BadRequest => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status503ServiceUnavailable
        };

        context.Result = new ViewResult
        {
            ViewName = "ApiUnavailable",
            StatusCode = statusCode,
            ViewData = new ViewDataDictionary(metadataProvider, context.ModelState)
            {
                ["Message"] = ex.Message
            }
        };
        context.ExceptionHandled = true;
    }
}
