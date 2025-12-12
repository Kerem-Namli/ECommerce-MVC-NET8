using System.Net;
using System.Text.Json;
using ETicaret.Business.Constants;
using ETicaret.Business.Utilities.Results;

namespace ETicaret.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var message = "Internal Server Error";
            // In development, you might want to return the actual exception message
            // message = exception.Message;

            var result = new ErrorResult(message); 
            // Or return custom ErrorDetails object
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                statusCode = context.Response.StatusCode,
                message = exception.Message, // For now returning actual message for debugging
                success = false
            }));
        }
    }
}
