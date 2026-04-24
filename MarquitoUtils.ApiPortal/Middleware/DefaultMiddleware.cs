using Microsoft.AspNetCore.Http;

namespace MarquitoUtils.ApiPortal.Middleware
{
    /// <summary>
    /// Default middleware base class for processing HTTP requests.
    /// </summary>
    public abstract class DefaultMiddleware
    {
        /// <summary>
        /// The next middleware component in the HTTP request pipeline.
        /// </summary>
        protected readonly RequestDelegate Next;

        /// <summary>
        /// Initializes a new instance of the DefaultMiddleware class with the specified request delegate
        /// factory.
        /// </summary>
        /// <remarks>This constructor is typically called by the ASP.NET Core framework when configuring
        /// the middleware pipeline. Ensure that both parameters are provided to enable proper request handling</remarks>
        /// <param name="next">The next middleware component in the HTTP request pipeline. Cannot be null.</param>
        public DefaultMiddleware(RequestDelegate next)
        {
            this.Next = next;
        }

        /// <summary>
        /// Processes an HTTP request asynchronously
        /// </summary>
        /// <param name="context">The HTTP context for the current request. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task InvokeAsync(HttpContext context);
    }
}
