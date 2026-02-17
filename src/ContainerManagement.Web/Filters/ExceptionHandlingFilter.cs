using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ContainerManagement.Web.Filters
{
    public class ExceptionHandlingFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionHandlingFilter> _logger;

        public ExceptionHandlingFilter(ILogger<ExceptionHandlingFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            _logger.LogError(exception, "Unhandled exception occurred.");

            var result = new ViewResult
            {
                ViewName = "Error"
            };

            result.ViewData["ErrorMessage"] = exception.Message;

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
