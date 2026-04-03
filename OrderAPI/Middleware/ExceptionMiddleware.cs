namespace OrderAPI.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly ILogger<ExceptionMiddleware> _logger;
		private readonly RequestDelegate _next;

		public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, RequestDelegate next)
		{
			_logger = logger;
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An unhandled exception occurred.");

				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				context.Response.ContentType = "application/json";

				await context.Response.WriteAsJsonAsync(new 
				{ 
					error = "An unexpected error occurred. Please try again later." 
				});
			}
		}
	}
}
