using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MarquitoUtils.ApiPortal.Middleware
{
    /// <summary>
    /// Middleware that logs incoming HTTP requests and unhandled exceptions during request processing.
    /// </summary>
    /// <remarks>This middleware should be registered early in the ASP.NET Core request pipeline to ensure
    /// that all requests and exceptions are logged. Logging includes the request path, query string, and any unhandled
    /// exceptions that occur during downstream processing. This middleware is typically used for monitoring and
    /// troubleshooting purposes.</remarks>
    public sealed class LoggingMiddleware
    {
        /// <summary>
        /// Provides logging capabilities for the LoggingMiddleware component.
        /// </summary>
        private readonly ILogger<LoggingMiddleware> _logger;
        /// <summary>
        /// The next middleware component in the HTTP request pipeline.
        /// </summary>
        private readonly RequestDelegate Next;

        /// <summary>
        /// Initializes a new instance of the LoggingMiddleware class with the specified request delegate and logger
        /// factory.
        /// </summary>
        /// <remarks>This constructor is typically called by the ASP.NET Core framework when configuring
        /// the middleware pipeline. Ensure that both parameters are provided to enable proper request handling and
        /// logging.</remarks>
        /// <param name="next">The next middleware component in the HTTP request pipeline. Cannot be null.</param>
        /// <param name="loggerFactory">The factory used to create an ILogger instance for logging middleware operations. Cannot be null.</param>
        public LoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.Next = next;
            _logger = loggerFactory.CreateLogger<LoggingMiddleware>();
        }

        /// <summary>
        /// Processes an HTTP request asynchronously and logs request details and unhandled exceptions.
        /// </summary>
        /// <remarks>This method logs the route and query string of each incoming API request at the
        /// information level. If an unhandled exception occurs during request processing, the exception is logged at
        /// the critical level. The method does not rethrow exceptions, so downstream middleware will not receive
        /// them.</remarks>
        /// <param name="context">The HTTP context for the current request. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            this._logger.LogInformation($"API request call route : {context.Request.Path}{context.Request.QueryString}");
            try
            {
                await this.Next(context);
            }
            catch (Exception ex)
            {
                this._logger.LogCritical($"{ex.GetType().Name} : {ex.Message} - {ex.StackTrace}");
                throw;
            }
        }
    }
}
