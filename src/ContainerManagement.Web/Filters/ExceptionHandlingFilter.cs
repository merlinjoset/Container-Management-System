using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ContainerManagement.Web.Filters
{
    public class ExceptionHandlingFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionHandlingFilter> _logger;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public ExceptionHandlingFilter(
            ILogger<ExceptionHandlingFilter> logger,
            IModelMetadataProvider modelMetadataProvider)
        {
            _logger = logger;
            _modelMetadataProvider = modelMetadataProvider;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            _logger.LogError(exception, "Unhandled exception occurred.");

            var result = new ViewResult
            {
                ViewName = "Error",
                ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState)
            };

            result.ViewData["ErrorMessage"] = exception.Message;

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
