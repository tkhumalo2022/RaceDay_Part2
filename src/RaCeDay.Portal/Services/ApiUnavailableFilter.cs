using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaCeDay.Portal.Services;

// Friendly failure view for API outage/timeouts; never expose stack traces or database details.
public sealed class ApiUnavailableFilter(ILogger<ApiUnavailableFilter> logger) : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not HttpRequestException &&
            context.Exception is not TaskCanceledException &&
            context.Exception is not System.Text.Json.JsonException)
            return Task.CompletedTask;
        logger.LogWarning(context.Exception, "Part 2 API is unavailable or returned invalid data.");
        context.ExceptionHandled = true;
        context.HttpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Result = new ViewResult
        {
            ViewName = "~/Views/Home/Error.cshtml",
            StatusCode = StatusCodes.Status503ServiceUnavailable,
            ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                context.ModelState)
            {
                ["Title"] = "RaceDay API unavailable",
                ["Message"] = "RaceDay is temporarily unable to load your data. Please try again."
            }
        };
        return Task.CompletedTask;
    }
}