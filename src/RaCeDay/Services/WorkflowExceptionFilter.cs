using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RaCeDay.Services;

public class WorkflowExceptionFilter(IModelMetadataProvider metadata) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not WorkflowException error) return;
        context.Result = context.HttpContext.Request.Path.StartsWithSegments("/api")
            ? new ObjectResult(new ProblemDetails { Status = error.Status, Title = error.Message }) { StatusCode = error.Status }
            : new ViewResult
            {
                ViewName = "WorkflowError", StatusCode = error.Status,
                ViewData = new ViewDataDictionary(metadata, context.ModelState) { Model = error.Message }
            };
        context.ExceptionHandled = true;
    }
}
