using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace SmartGearOnline.Filters
{
    public class SimpleAuthorizationFilter : IActionFilter
    {
        private readonly ILogger<SimpleAuthorizationFilter> _logger;

        public SimpleAuthorizationFilter(ILogger<SimpleAuthorizationFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // allow actions/controllers marked with [AllowAnonymous]
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
            {
                return;
            }

            // existing logic: block unauthorized access when user is not authenticated / allowed
            if (!context.HttpContext.User?.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogInformation("SimpleAuthorizationFilter: blocking anonymous request to {Path}", context.HttpContext.Request.Path);
                context.Result = new ContentResult
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "Unauthorized Access"
                };
                return;
            }

            var request = context.HttpContext.Request;

            // Try to get 'user' from query string first
            string? user = request.Query["user"];

            // If not in query, try to get it from form data (POST)
            if (string.IsNullOrEmpty(user) && request.HasFormContentType)
            {
                if (request.Form.TryGetValue("user", out var formUser))
                {
                    user = formUser.ToString();
                }
            }

            // Deny access if user is missing or not 'admin'
            if (string.IsNullOrEmpty(user) || user != "admin")
            {
                context.Result = new ContentResult
                {
                    Content = "Unauthorized Access",
                    StatusCode = 403
                };
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Nothing to do after action
        }
    }
}
